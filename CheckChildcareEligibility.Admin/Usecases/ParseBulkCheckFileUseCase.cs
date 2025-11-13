using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Models;
using CsvHelper;
using CsvHelper.Configuration;
using FluentValidation;
using System.Globalization;

namespace CheckChildcareEligibility.Admin.Usecases
{
    public class BulkCheckCsvResult
    {
        public List<CheckEligibilityRequestData> ValidRequests { get; set; } = new();
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
        private readonly IValidator<CheckEligibilityRequestData> _validator;
        private readonly IConfiguration _config;
        private readonly int  _rowCountLimit;

        public ParseBulkCheckFileUseCase(IValidator<CheckEligibilityRequestData> validator, IConfiguration configuration)
        {
            _validator = validator;
            _config = configuration;
            _rowCountLimit = int.Parse(_config["BulkEligibilityCheckLimit"]); 
            
        }
        public async Task<BulkCheckCsvResult> Execute(Stream csvStream, CheckEligibilityType eligibilityType)
        {
            int checkrowLimit = int.Parse(_config["BulkEligibilityCheckLimit"]);
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

            csv.Context.RegisterClassMap<CheckRowRowMap>();

            var lineNumber = 2; // headers on line 1
            var sequence = 1;
            IEnumerable<CheckRowBase> records = Enumerable.Empty<CheckRowBase>();

            try
            {
                csv.Read();
                csv.ReadHeader();     
                var actualHeaders = csv.HeaderRecord;

                switch (eligibilityType) {

                    case CheckEligibilityType.WorkingFamilies:
                        expectedHeaders = ["Eligibility code", "National Insurance number", "Child date of birth"];
                        records = csv.GetRecords<CheckRowWorkingFamilies>().ToList();
                        break;
                    default:
                       expectedHeaders = ["Parent Last Name", "Parent Date of Birth", "Parent National Insurance Number"];
                        records = csv.GetRecords<CheckRow>().ToList();
                        break;
                }
                csv.Dispose();
                // Validate headers 
                if (!expectedHeaders.SequenceEqual(actualHeaders))
                {
                    result.ErrorMessage = "The column headings in the selected file must exactly match the template";

                    return result;
                }
             
                // Validate row count
                if (records.Count() > _rowCountLimit) {

                    result.ErrorMessage = $"The selected file must contain fewer than {_rowCountLimit} rows";
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
                        var requestItem = new CheckEligibilityRequestData();
                        requestItem.Type = eligibilityType;
                        requestItem.Sequence = sequence;

                        switch (eligibilityType) {
                            case CheckEligibilityType.WorkingFamilies:
                                var workingFamilyRow =  record as CheckRowWorkingFamilies;                          
                                requestItem.LastName = workingFamilyRow.EligibilityCode;
                                requestItem.DateOfBirth = workingFamilyRow.DOB;//must remain in original pre-parsed form to go through validator
                                requestItem.NationalInsuranceNumber = workingFamilyRow.Ni.ToUpper();                        
                                break;
                            default:
                                var row = record as CheckRow;
                                requestItem.LastName = row.LastName;
                                requestItem.DateOfBirth = row.DOB;//must remain in original pre-parsed form to go through validator
                                requestItem.NationalInsuranceNumber = row.Ni.ToUpper();                              
                                break;
                        
                        }                   
                        var validationResults = _validator.Validate(requestItem);
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
                        else
                        {
                            //We know this passed parse earlier but it must be translated to correct format (yyyy-MM-dd) for Database to access
                            requestItem.DateOfBirth = DateTime.Parse(record.DOB).ToString("yyyy-MM-dd");
                            
                            result.ValidRequests.Add(requestItem);
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
