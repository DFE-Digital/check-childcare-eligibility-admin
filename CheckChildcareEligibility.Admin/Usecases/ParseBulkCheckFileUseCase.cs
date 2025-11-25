using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Models;
using CheckChildcareEligibility.Admin.Usecases.Constants;
using CsvHelper;
using CsvHelper.Configuration;
using FluentValidation;
using FluentValidation.Results;
using System.Globalization;

namespace CheckChildcareEligibility.Admin.Usecases
{
    public class BulkCheckCsvResult
    {
        public List<CheckEligibilityRequestDataBase> ValidRequests { get; set; } = new();
        public List<CsvRowError> Errors { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;

    }

    public class CsvRowError
    {
        public int LineNumber { get; set; }
        public string Message { get; set; }
    }

    public interface IParseBulkCheckFileUseCase
    {
        Task<BulkCheckCsvResult> Execute(
            Stream stream,
            CheckEligibilityType eligibilityType
        );
    }
    public class ParseBulkCheckFileUseCase : IParseBulkCheckFileUseCase
    {
        private readonly IValidator<IEligibilityServiceType> _validator;
        private readonly IConfiguration _config;
        private readonly int _rowCountLimit;

        public ParseBulkCheckFileUseCase(IValidator<IEligibilityServiceType> validator, IConfiguration configuration)
        {
            _validator = validator;
            _config = configuration;
            _rowCountLimit = int.Parse(_config["BulkEligibilityCheckLimit"]);

        }
        public async Task<BulkCheckCsvResult> Execute(Stream csvStream, CheckEligibilityType eligibilityType)
        {
            string[] expectedHeaders = [];

            var result = new BulkCheckCsvResult();

            using var reader = new StreamReader(csvStream);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                BadDataFound = null,
                MissingFieldFound = null
            };

            using var csv = new CsvReader(reader, config);

            var lineNumber = 2; // headers on line 1
            var sequence = 1;
            IEnumerable<CheckRowBase> records = Enumerable.Empty<CheckRowBase>();

            try
            {
                csv.Read();
                csv.ReadHeader();

                // Validate headers 
                var actualHeaders = csv.HeaderRecord;
                switch (eligibilityType)
                {

                    case CheckEligibilityType.WorkingFamilies:
                        expectedHeaders = ["Eligibility code", "National Insurance number", "Child date of birth"];
                        break;
                    default:
                        expectedHeaders = ["Parent Last Name", "Parent Date of Birth", "Parent National Insurance Number"];
                        break;
                }

                if (!expectedHeaders.SequenceEqual(actualHeaders))
                {
                    result.ErrorMessage = BulkCheckUseCaseValidationMessages.InvalidHeaders;

                    return result;
                }
                // Map records according to the type
                switch (eligibilityType)
                {

                    case CheckEligibilityType.WorkingFamilies:
                        csv.Context.RegisterClassMap<CheckRowRowMapWorkingFamilies>();
                        records = csv.GetRecords<CheckRowWorkingFamilies>().ToList();
                        break;
                    default:
                        csv.Context.RegisterClassMap<CheckRowRowMap>();
                        records = csv.GetRecords<CheckRow>().ToList();
                        break;
                }

                csv.Dispose();

                // Validate row count
                if (records.Count() > _rowCountLimit)
                {

                    result.ErrorMessage = BulkCheckUseCaseValidationMessages.TooManyRows(_rowCountLimit);
                    return result;
                }
                // check for malformed rows
                foreach (var record in records)
                {
                    if (record == null)
                    {
                        result.Errors.Add(new CsvRowError
                        {
                            LineNumber = lineNumber,
                            Message = "Empty or malformed row."
                        });
                        lineNumber++;
                        continue;
                    }

                    try
                    {
                        var validationResults = new ValidationResult();
                       
                        switch (eligibilityType)
                        {
                            case CheckEligibilityType.WorkingFamilies:
                                var workingFamilyRow = record as CheckRowWorkingFamilies;
                                var requestDataWF = new CheckEligibilityRequestWorkingFamiliesData();
                                requestDataWF.EligibilityCode = workingFamilyRow.EligibilityCode;
                                requestDataWF.DateOfBirth = workingFamilyRow.DOB;//must remain in original pre-parsed form to go through validator
                                requestDataWF.NationalInsuranceNumber = workingFamilyRow.Ni.ToUpper();
                                requestDataWF.Type = eligibilityType;
                                requestDataWF.Sequence = sequence;
                                validationResults = _validator.Validate(requestDataWF);
                                if (validationResults.IsValid) {
                                    //We know this passed parse earlier but it must be translated to correct format (yyyy-MM-dd) for Database to access
                                    requestDataWF.DateOfBirth = DateTime.Parse(record.DOB).ToString("yyyy-MM-dd");
                                    result.ValidRequests.Add(requestDataWF);
                                }
                                break;
                            default:
                                var row = record as CheckRow;
                                var requestData = new CheckEligibilityRequestData();
                                requestData.LastName = row.LastName;
                                requestData.DateOfBirth = row.DOB;//must remain in original pre-parsed form to go through validator
                                requestData.NationalInsuranceNumber = row.Ni.ToUpper();
                                requestData.Type = eligibilityType;
                                requestData.Sequence = sequence;
                                validationResults = _validator.Validate(requestData);
                                if (validationResults.IsValid)
                                {
                                    //We know this passed parse earlier but it must be translated to correct format (yyyy-MM-dd) for Database to access
                                    requestData.DateOfBirth = DateTime.Parse(record.DOB).ToString("yyyy-MM-dd");
                                    result.ValidRequests.Add(requestData);
                                }
                                break;

                        }
                        if (!validationResults.IsValid)
                        {
                            foreach (var error in validationResults.Errors)
                            {
                                var errorMessage = error.ToString();

                                if (ContainsError(result.Errors, lineNumber, errorMessage))
                                {
                                    lineNumber++;
                                    continue;
                                }

                                result.Errors.Add(new CsvRowError
                                {
                                    LineNumber = lineNumber,
                                    Message = errorMessage
                                });
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add(new CsvRowError
                        {
                            LineNumber = lineNumber,
                            Message = $"Unexpected error in processing: {ex.Message}"
                        });
                    }

                    sequence++;
                    lineNumber++;
                }
            }
            catch (HeaderValidationException ex)
            {
                result.Errors.Add(new CsvRowError
                {
                    LineNumber = 1,
                    Message = $"CSV header error: {ex.Message}"
                });
            }
            catch (ReaderException ex) //Updated to userFriendlyMessage to stop leaking variable names to users
            {
                string userFriendlyMessage;

                if (ex.Message.Contains("No header record was found"))
                {
                    userFriendlyMessage = "The selected file is empty or missing headers.";
                }
                else if (ex.Message.Contains("data type"))
                {
                    userFriendlyMessage = $"CSV read error: The file contains invalid data at line {lineNumber}.";
                }
                else
                {
                    userFriendlyMessage = "CSV read error: An error occurred while processing the file.";
                }

                result.Errors.Add(new CsvRowError
                {
                    Message = userFriendlyMessage
                });
            }
            catch (Exception ex)
            {
                result.Errors.Add(new CsvRowError
                {
                    LineNumber = 0,
                    Message = $"Unexpected CSV error: {ex.Message}"
                });
            }
            return result;

        }
        private bool ContainsError(IEnumerable<CsvRowError> errors, int lineNumber, string errorMessage)
        {
            return errors.Any(x => x.LineNumber == lineNumber && string.Equals(x.Message, errorMessage));
        }
    }
}
