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

        public ParseBulkCheckFileUseCase(IValidator<CheckEligibilityRequestData> validator)
        {
            _validator = validator;
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

            csv.Context.RegisterClassMap<CheckRowRowMap>();

            var lineNumber = 2; // headers on line 1
            var sequence = 1;
            try
            {
                csv.Read();
                csv.ReadHeader();

                var records = csv.GetRecords<CheckRow>();
                var actualHeaders = csv.HeaderRecord;

                switch (eligibilityType) {

                    case CheckEligibilityType.WorkingFamilies:
                        expectedHeaders = ["Eligibility code", "National Insurance number", "Child date of birth"];
                        break;
                    default:
                       expectedHeaders = ["Parent Last Name", "Parent Date of Birth", "Parent National Insurance Number"];
                        break;
                }
                if (!expectedHeaders.SequenceEqual(actualHeaders))
                {
                    result.ErrorMessage = "The column headings in the selected file must exactly match the template";

                    return result;
                }

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
                        var requestItem = new CheckEligibilityRequestData(eligibilityType)
                        {
                            LastName = record.LastName,
                            DateOfBirth = record.DOB, //must remain in original pre-parsed form to go through validator
                            NationalInsuranceNumber = record.Ni.ToUpper(),
                            Sequence = sequence
                        };

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
