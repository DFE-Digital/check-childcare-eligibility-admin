using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Tests.Properties;
using CheckChildcareEligibility.Admin.Usecases;
using CheckChildcareEligibility.Admin.Usecases.Constants;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Moq;

namespace CheckChildcareEligibility.Admin.Tests.Usecases
{
    [TestFixture]
    public class ParseBulkCheckFileUseCaseTests
    {
        [SetUp]
        public void SetUp()
        {
            _checkEligibilityRequestDataValidator = new Mock<IValidator<IEligibilityServiceType>>();
            _config = new Mock<IConfiguration>();
            _config.Setup(c => c["BulkEligibilityCheckLimit"]).Returns("250");
            _sut = new ParseBulkCheckFileUseCase(_checkEligibilityRequestDataValidator.Object, _config.Object);
        }

        [TearDown]
        public void TearDown()
        {
        }

        private Mock<IValidator<IEligibilityServiceType>> _checkEligibilityRequestDataValidator;
        private Mock<IConfiguration> _config;
        // system under test
        private IParseBulkCheckFileUseCase _sut;


        [Test]
        public async Task Given_Bulk_Check_When_FileHasInvalidHeaders_2YO()
        {
            // Arrange
            var content = Resources.bulkchecktemplate_invalid_headers;
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            // Act
            var result = await _sut.Execute(stream, CheckEligibilityType.TwoYearOffer);


            // Assert
            result.ErrorMessage.Should().BeEquivalentTo(BulkCheckUseCaseConstants.InvalidHeadersErrorMessage);

        }

        [Test]
        public async Task Given_Bulk_Check_When_FileHasInvalidHeaders_EYPP()
        {
            // Arrange
            var content = Resources.bulkchecktemplate_invalid_headers;
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            // Act
            var result = await _sut.Execute(stream, CheckEligibilityType.EarlyYearPupilPremium);


            // Assert
            result.ErrorMessage.Should().BeEquivalentTo(BulkCheckUseCaseConstants.InvalidHeadersErrorMessage);
        
        }
        [Test]
        public async Task Given_Bulk_Check_When_FileHasInvalidHeaders_WF()
        {
            // Arrange
            var content = Resources.bulkchecktemplate_invalid_headers;
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            // Act
            var result = await _sut.Execute(stream, CheckEligibilityType.WorkingFamilies);

            // Assert
            result.ErrorMessage.Should().BeEquivalentTo(BulkCheckUseCaseConstants.InvalidHeadersErrorMessage);

        }

        [Test]
        public async Task Given_Bulk_Check_When_FileHasTooManyRecords_Should_ReturnBulkCheckPage()
        {
            // Arrange
            var content = Resources.bulkchecktemplate_too_many_records;
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            // Act
            var result = await _sut.Execute(stream, CheckEligibilityType.TwoYearOffer);

            // Assert
            result.ErrorMessage.Should().BeEquivalentTo(BulkCheckUseCaseConstants.TooManyRowsErrorMessage(250));

        }
        [Test]
        public async Task Given_Bulk_Check_When_FileData_Contains_Invalid_Row_Should_Return_Listed_Error()
        {
            // Arrange
            var errors = new List<CsvRowError>
            {
               new CsvRowError{ LineNumber = 2, Message = "Something is wrong!" },
               new CsvRowError{ LineNumber = 3, Message = "Something is wrong!" },
               new CsvRowError{ LineNumber = 4, Message = "Something is wrong!" },
               new CsvRowError{ LineNumber = 5, Message = "Something is wrong!" },
            };

            var content = Resources.bulkchecktemplate_some_invalid_items;
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            var validationResults = new FluentValidation.Results.ValidationResult()
            {
                Errors = new List<FluentValidation.Results.ValidationFailure>()
                {
                    new FluentValidation.Results.ValidationFailure()
                    {
                        ErrorMessage = "Something is wrong!"
                    }
                }
            };

            _checkEligibilityRequestDataValidator.Setup(x => x.Validate(It.IsAny<CheckEligibilityRequestData>()))
                .Returns(validationResults);

            // Act
            var result = await _sut.Execute(stream, CheckEligibilityType.TwoYearOffer);

            // Assert
            result.Errors.Should().BeEquivalentTo(errors);

        }

        [Test]
        public async Task Given_Bulk_Check_When_FileData_Contains_Valid_Data_Should_Return_Valid_Results()
        {
            // Arrange
            var errors = new List<CsvRowError>();
            var validRows = new List<CheckEligibilityRequestData>();

            var content = Resources.bulkchecktemplate_small_Valid;
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            var validationResults = new FluentValidation.Results.ValidationResult()
            {
                Errors = new List<FluentValidation.Results.ValidationFailure>()
            };


            _checkEligibilityRequestDataValidator.Setup(x => x.Validate(It.IsAny<CheckEligibilityRequestData>()))
                .Returns(validationResults);

            // Act
            var result = await _sut.Execute(stream, CheckEligibilityType.TwoYearOffer);

            // Assert
            result.Errors.Should().BeEquivalentTo(errors);
            result.Errors.Count().Should().Be(0);
            result.ValidRequests.Count().Should().Be(2);
        }

        [Test]
        public async Task Given_Bulk_Check_When_FileData_Valid_2YO_Should_Return_Valid_2YO_Results()
        {
            // Arrange
            var errors = new List<CsvRowError>();
            var validRows = new List<CheckEligibilityRequestData>();

            var content = Resources.bulkchecktemplate_small_Valid;
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            var validationResults = new FluentValidation.Results.ValidationResult()
            {
                Errors = new List<FluentValidation.Results.ValidationFailure>()
            };


            _checkEligibilityRequestDataValidator.Setup(x => x.Validate(It.IsAny<CheckEligibilityRequestData>()))
                .Returns(validationResults);

            // Act
            var result = await _sut.Execute(stream, CheckEligibilityType.TwoYearOffer);

            // Assert
            result.Errors.Should().BeEquivalentTo(errors);
            result.Errors.Count().Should().Be(0);
            result.ValidRequests[0].Type.Should().Be(CheckEligibilityType.TwoYearOffer);
        }
        [Test]
        public async Task Given_Bulk_Check_When_FileData_Valid_WF_Should_Return_Valid_WF_Results()
        {
            // Arrange
            var errors = new List<CsvRowError>();
            var validRows = new List<CheckEligibilityRequestWorkingFamiliesData>();

            var content = Resources.working_families_small_valid;
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            var validationResults = new FluentValidation.Results.ValidationResult()
            {
                Errors = new List<FluentValidation.Results.ValidationFailure>()
            };

            _checkEligibilityRequestDataValidator.Setup(x => x.Validate(It.IsAny<CheckEligibilityRequestWorkingFamiliesData>()))
                .Returns(validationResults);

            // Act
            var result = await _sut.Execute(stream, CheckEligibilityType.WorkingFamilies);

            // Assert
            result.Errors.Should().BeEquivalentTo(errors);
            result.Errors.Count().Should().Be(0);
            result.ValidRequests[0].Type.Should().Be(CheckEligibilityType.WorkingFamilies);
        }


        [Test]
        public async Task Given_Bulk_Check_When_FileData_Valid_EYPP_Should_Return_Valid_EYPP_Results()
        {
            // Arrange
            var errors = new List<CsvRowError>();
            var validRows = new List<CheckEligibilityRequestData>();

            var content = Resources.bulkchecktemplate_small_Valid;
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            var validationResults = new FluentValidation.Results.ValidationResult()
            {
                Errors = new List<FluentValidation.Results.ValidationFailure>()
            };

            _checkEligibilityRequestDataValidator.Setup(x => x.Validate(It.IsAny<CheckEligibilityRequestData>()))
                .Returns(validationResults);

            // Act
            var result = await _sut.Execute(stream, CheckEligibilityType.EarlyYearPupilPremium);

            // Assert
            result.Errors.Should().BeEquivalentTo(errors);
            result.Errors.Count().Should().Be(0);
            result.ValidRequests[0].Type.Should().Be(CheckEligibilityType.EarlyYearPupilPremium);
        }

        [Test]
        public async Task Given_Bulk_Check_When_FileData_Contains_Valid_Data_Rows_Should_Return_Valid_Results_with_Sequence()
        {
            // Arrange
            var errors = new List<CsvRowError>();
            var validRows = new List<CheckEligibilityRequestData>();

            var content = Resources.bulkchecktemplate_small_Valid;
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            var validationResults = new FluentValidation.Results.ValidationResult()
            {
                Errors = new List<FluentValidation.Results.ValidationFailure>()
            };


            _checkEligibilityRequestDataValidator.Setup(x => x.Validate(It.IsAny<CheckEligibilityRequestData>()))
                .Returns(validationResults);

            // Act
            var result = await _sut.Execute(stream, CheckEligibilityType.TwoYearOffer);

            // Assert
            result.Errors.Should().BeEquivalentTo(errors);
            result.Errors.Count().Should().Be(0);
            var sequence = 1;
            foreach (var request in result.ValidRequests)
            {
                request.Sequence.Should().Be(sequence);
                sequence++;
            }
        }

    }
}