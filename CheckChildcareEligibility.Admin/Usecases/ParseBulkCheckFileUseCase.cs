using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Net.Security;
using System.Reflection.PortableExecutable;
using System.Text;
using AspNetCoreGeneratedDocument;
using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Domain.Validation;
using CheckChildcareEligibility.Admin.Models;
using CsvHelper;
using CsvHelper.Configuration;
using FluentValidation;
using FluentValidation.Results;

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
            var expectedHeaders = new[] { "Parent Last Name", "Parent Date of Birth", "Parent National Insurance Number" };

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
                            DateOfBirth = DateTime.TryParse(record.DOB, out var dtval) ? dtval.ToString("yyyy-MM-dd") : string.Empty,
                            NationalInsuranceNumber = record.Ni.ToUpper(),
                            Sequence = sequence
                        };

                        var validationResults = _validator.Validate(requestItem);

                        if (!validationResults.IsValid)
                        {
                            foreach (var error in validationResults.Errors)
                            {
                                var errorMessage = ExtractValidationErrorMessage(error);

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
            catch (ReaderException ex)
            {
                result.Errors.Add(new CsvRowError
                {
                    LineNumber = lineNumber,
                    Message = $"CSV read error: {ex.Message}"
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

        private string ExtractValidationErrorMessage(ValidationFailure error)
        {
            switch (error.ErrorMessage)
            {
                case ValidationMessages.LastName:
                case "'LastName' must not be empty.":
                    return "Issue with Surname";
                case ValidationMessages.DOB:
                case "'Date Of Birth' must not be empty.":
                    return "Issue with date of birth";
                case ValidationMessages.NI:
                    return "Issue with National Insurance number";
                case ValidationMessages.NI_and_NASS:
                    return $"Issue {ValidationMessages.NI_and_NASS}";
                case ValidationMessages.NI_or_NASS:
                    return $"Issue {ValidationMessages.NI_or_NASS}";
                default:
                    return $"Issue {error.ErrorMessage}";
            }
        }

        private bool ContainsError(IEnumerable<CsvRowError> errors, int lineNumber, string errorMessage)
        {
            return errors.Any(x => x.LineNumber == lineNumber && string.Equals(x.Message, errorMessage));
        }
    }
}
