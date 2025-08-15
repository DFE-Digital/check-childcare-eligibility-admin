using System.Security.Claims;
using AutoFixture;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Controllers;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Gateways;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.Models;
using CheckChildcareEligibility.Admin.UseCases;
using CheckChildcareEligibility.Admin.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace CheckChildcareEligibility.Admin.Tests.Controllers;

[TestFixture]
public class CheckControllerTests : TestBase
{
    [SetUp]
    public void SetUp()
    {
        // Initialize legacy service mocks
        _checkGatewayMock = new Mock<ICheckGateway>();
        _loggerMock = Mock.Of<ILogger<CheckController>>();

        // Initialize use case mocks
        _loadParentDetailsUseCaseMock = new Mock<ILoadParentDetailsUseCase>();
        _loadParentAndChildDetailsUseCaseMock = new Mock<ILoadParentAndChildDetailsUseCase>();
        _perform2YoEligibilityCheckUseCaseMock = new Mock<IPerform2YoEligibilityCheckUseCase>();
        _performEyppEligibilityCheckUseCaseMock = new Mock<IPerformEyppEligibilityCheckUseCase>();
        _performWFEligibilityCheckUseCaseMock = new Mock<IPerformWFEligibilityCheckUseCase>();
        _getCheckStatusUseCaseMock = new Mock<IGetCheckStatusUseCase>();
        _validateParentDetailsUseCaseMock = new Mock<IValidateParentDetailsUseCase>();
        _validateParentAndChildDetailsUseCaseMock = new Mock<IValidateParentAndChildDetailsUseCase>();

        // Initialize controller with all dependencies
        _sut = new CheckController(
            _loggerMock,
            _checkGatewayMock.Object,
            _configMock.Object,
            _loadParentDetailsUseCaseMock.Object,
            _loadParentAndChildDetailsUseCaseMock.Object,
            _performWFEligibilityCheckUseCaseMock.Object,
            _perform2YoEligibilityCheckUseCaseMock.Object,
            _performEyppEligibilityCheckUseCaseMock.Object,
            _getCheckStatusUseCaseMock.Object,
            _validateParentDetailsUseCaseMock.Object,
            _validateParentAndChildDetailsUseCaseMock.Object
        );

        SetUpSessionData();

        base.SetUp();

        _sut.TempData = _tempData;
        _sut.ControllerContext.HttpContext = _httpContext.Object;
    }

    [TearDown]
    public void TearDown()
    {
        _sut.Dispose();
    }

    // Mocks for use cases
    private ILogger<CheckController> _loggerMock;
    private Mock<ILoadParentDetailsUseCase> _loadParentDetailsUseCaseMock;
    private Mock<ILoadParentAndChildDetailsUseCase> _loadParentAndChildDetailsUseCaseMock;
    private Mock<IPerform2YoEligibilityCheckUseCase> _perform2YoEligibilityCheckUseCaseMock;
    private Mock<IPerformEyppEligibilityCheckUseCase> _performEyppEligibilityCheckUseCaseMock;
    private Mock<IPerformWFEligibilityCheckUseCase> _performWFEligibilityCheckUseCaseMock;
    private Mock<IGetCheckStatusUseCase> _getCheckStatusUseCaseMock;
    private Mock<IValidateParentDetailsUseCase> _validateParentDetailsUseCaseMock;
    private Mock<IValidateParentAndChildDetailsUseCase> _validateParentAndChildDetailsUseCaseMock;

    // Legacy service mocks
    private Mock<ICheckGateway> _checkGatewayMock;

    // System under test
    private CheckController _sut;

    [Test]
    public async Task Enter_Details_Get_When_NoResponseInTempData_Should_ReturnView()
    {
        // Arrange
        var expectedParent = _fixture.Create<ParentGuardian>();
        var expectedErrors = new Dictionary<string, List<string>>();

        // Setup TempData with eligibilityType
        _tempData["eligibilityType"] = "2YO";

        _loadParentDetailsUseCaseMock
            .Setup(x => x.Execute(
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync((expectedParent, expectedErrors));

        // Act
        var result = await _sut.Enter_Details();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.Model.Should().Be(expectedParent);
    }

    [Test]
    public async Task Enter_Details_Get_When_ErrorsInTempData_Should_AddToModelState()
    {
        // Arrange
        var expectedParent = _fixture.Create<ParentGuardian>();
        var expectedErrors = new Dictionary<string, List<string>>
        {
            { "TestError", new List<string> { "Error message 1", "Error message 2" } }
        };

        // Setup TempData with eligibilityType
        _tempData["eligibilityType"] = "2YO";
        _tempData["ParentDetails"] = JsonConvert.SerializeObject(expectedParent);
        _tempData["Errors"] = JsonConvert.SerializeObject(expectedErrors);

        _loadParentDetailsUseCaseMock
            .Setup(x => x.Execute(
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync((expectedParent, expectedErrors));

        // Act
        var result = await _sut.Enter_Details();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.Model.Should().Be(expectedParent);
        
        // Verify ModelState contains the expected errors
        _sut.ModelState.ErrorCount.Should().Be(2);
        _sut.ModelState["TestError"].Errors.Count.Should().Be(2);
        _sut.ModelState["TestError"].Errors[0].ErrorMessage.Should().Be("Error message 1");
        _sut.ModelState["TestError"].Errors[1].ErrorMessage.Should().Be("Error message 2");
    }

    [Test]
    [TestCase("AB123456C")]
    public async Task Enter_Details_Post_When_ValidationFails_Should_RedirectBack(
        string? nino)
    {
        // Arrange
        var request = _fixture.Create<ParentGuardian>();
        request.NationalInsuranceNumber = nino;
        request.Day = "1";
        request.Month = "1";
        request.Year = "1990";

        var validationResult = new ValidationResult
        {
            IsValid = false,
            Errors = new Dictionary<string, List<string>>
            {
                { "Error Key", new List<string> { "Error Message" } }
            }
        };

        _validateParentDetailsUseCaseMock
            .Setup(x => x.Execute(request, It.IsAny<ModelStateDictionary>()))
            .Returns(validationResult);

        // Act
        var result = await _sut.Enter_Details(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult.ActionName.Should().Be("Enter_Details");

        // Verify TempData contains expected values
        _sut.TempData.Should().ContainKey("ParentDetails");
        _sut.TempData.Should().ContainKey("Errors");

        // Verify the mock was called with correct parameters
        _validateParentDetailsUseCaseMock.Verify(
            x => x.Execute(request, It.IsAny<ModelStateDictionary>()),
            Times.Once);
    }

    [Test]
    [TestCase("AB123456C")]
    public async Task Enter_Details_Post_When_Valid_Should_ProcessAndRedirectToLoader(
        string? nino)
    {
        // Arrange
        var request = _fixture.Create<ParentGuardian>();
        request.NationalInsuranceNumber = nino;
        request.Day = "01";
        request.Month = "01";
        request.Year = "1990";

        var tempData = new TempDataDictionary(
           new DefaultHttpContext(),
           Mock.Of<ITempDataProvider>()
        );
        tempData["eligibilityType"] = "2YO";

        _sut.TempData = tempData;

        var validationResult = new ValidationResult { IsValid = true };
        var checkEligibilityResponse = _fixture.Create<CheckEligibilityResponse>();

        _validateParentDetailsUseCaseMock
            .Setup(x => x.Execute(request, It.IsAny<ModelStateDictionary>()))
            .Returns(validationResult);

        _perform2YoEligibilityCheckUseCaseMock
            .Setup(x => x.Execute(request, _sut.HttpContext.Session))
            .ReturnsAsync(checkEligibilityResponse);

        // Act
        var result = await _sut.Enter_Details(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult.ActionName.Should().Be("Loader");
        _sut.TempData["Response"].Should().NotBeNull();

        _validateParentDetailsUseCaseMock.Verify(
            x => x.Execute(request, It.IsAny<ModelStateDictionary>()),
            Times.Once);

        _perform2YoEligibilityCheckUseCaseMock.Verify(
            x => x.Execute(request, _sut.HttpContext.Session),
            Times.Once);
    }

    [Test]
    public async Task Enter_Details_Post_When_ValidationPasses_With_AsrnSelected_Should_ProcessAndRedirectToLoader()
    {
        // Arrange
        var request = _fixture.Create<ParentGuardian>();
        request.NationalInsuranceNumber = null;
        request.Day = "01";
        request.Month = "01";
        request.Year = "1990";

        var tempData = new TempDataDictionary(
           new DefaultHttpContext(),
           Mock.Of<ITempDataProvider>()
        );
        tempData["eligibilityType"] = "2YO";

        _sut.TempData = tempData;

        var validationResult = new ValidationResult { IsValid = true };
        var checkEligibilityResponse = _fixture.Create<CheckEligibilityResponse>();

        _validateParentDetailsUseCaseMock
            .Setup(x => x.Execute(request, It.IsAny<ModelStateDictionary>()))
            .Returns(validationResult);

        _perform2YoEligibilityCheckUseCaseMock
            .Setup(x => x.Execute(request, _sut.HttpContext.Session))
            .ReturnsAsync(checkEligibilityResponse);

        // Act
        var result = await _sut.Enter_Details(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult.ActionName.Should().Be("Loader");
        
        // Verify that TempData entries are removed
        _sut.TempData.Keys.Should().NotContain("FsmApplication");
        _sut.TempData.Keys.Should().NotContain("FsmEvidence");
        _sut.TempData["Response"].Should().NotBeNull();

        _validateParentDetailsUseCaseMock.Verify(
            x => x.Execute(request, It.IsAny<ModelStateDictionary>()),
            Times.Once);

        _perform2YoEligibilityCheckUseCaseMock.Verify(
            x => x.Execute(request, _sut.HttpContext.Session),
            Times.Once);
    }
    
    [Test]
    public async Task Loader_Should_Handle_Exceptions_And_Return_Technical_Error_View()
    {
        // Arrange
        var responseJson = JsonConvert.SerializeObject(_fixture.Create<CheckEligibilityResponse>());
        _tempData["Response"] = responseJson;
        _tempData["EligibilityType"] = "2YO";
        
        // Setup mock to throw an exception when called
        _getCheckStatusUseCaseMock
            .Setup(x => x.Execute(It.IsAny<string>(), _sessionMock.Object))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _sut.Loader();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be("Outcome/Technical_Error");
        
        // Verify model has some basic properties set
        var model = viewResult.Model as EligibilityOutcomeViewModel;
        model.Should().NotBeNull();
        model.EligibilityType.Should().Be("2YO");
    }

    [Test]
    public async Task Loader_When_School_User_Should_Return_Non_LA_View()
    {
        // Arrange
        var statusValue = _fixture.Build<StatusValue>()
            .With(x => x.Status, "eligible")
            .Create();

        var checkEligibilityResponse = _fixture.Build<CheckEligibilityResponse>()
            .With(x => x.Data, statusValue)
            .Create();

        _httpContext.Setup(ctx => ctx.Session).Returns(_sessionMock.Object);
        // Set up a school user (not LA)
        _sut.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "12345"),
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "test@example.com"),
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "John"),
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "Doe"),
            new("OrganisationCategoryName", Constants.CategoryTypeSchool)
        }));

        var responseJson = JsonConvert.SerializeObject(checkEligibilityResponse);
        _tempData["Response"] = responseJson;
        _tempData["EligibilityType"] = "2YO";
        
        // Setup parent details
        var parent = _fixture.Create<ParentGuardian>();
        _tempData["ParentDetails"] = JsonConvert.SerializeObject(parent);
        
        _getCheckStatusUseCaseMock
            .Setup(x => x.Execute(responseJson, _sessionMock.Object))
            .ReturnsAsync("eligible");
            
        _loadParentDetailsUseCaseMock
            .Setup(x => x.Execute(
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync((parent, null));

        // Act
        var result = await _sut.Loader();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        // Should return the non-LA view for eligible result
        viewResult.ViewName.Should().Be("Outcome/Eligible");
    }

    [Test]
    public async Task Loader_When_DateOfBirth_Is_Invalid_Should_Handle_Gracefully()
    {
        // Arrange
        var statusValue = _fixture.Build<StatusValue>()
            .With(x => x.Status, "notEligible")
            .Create();

        var checkEligibilityResponse = _fixture.Build<CheckEligibilityResponse>()
            .With(x => x.Data, statusValue)
            .Create();

        _httpContext.Setup(ctx => ctx.Session).Returns(_sessionMock.Object);
        _sut.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "12345"),
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "test@example.com"),
            new("OrganisationCategoryName", Constants.CategoryTypeSchool)
        }));

        // Create parent with invalid date of birth
        var parent = _fixture.Create<ParentGuardian>();
        parent.Day = "99"; // Invalid day
        parent.Month = "99"; // Invalid month
        parent.Year = "9999"; // Future year
        
        var responseJson = JsonConvert.SerializeObject(checkEligibilityResponse);
        _tempData["Response"] = responseJson;
        _tempData["EligibilityType"] = "2YO";
        _tempData["ParentDetails"] = JsonConvert.SerializeObject(parent);
        
        _getCheckStatusUseCaseMock
            .Setup(x => x.Execute(responseJson, _sessionMock.Object))
            .ReturnsAsync("notEligible");
            
        _loadParentDetailsUseCaseMock
            .Setup(x => x.Execute(
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync((parent, null));

        // Act
        var result = await _sut.Loader();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be("Outcome/Not_Eligible");
        
        // The view model should have the date conversion handled properly
        var model = viewResult.Model as EligibilityOutcomeViewModel;
        model.Should().NotBeNull();
        // It should default to DateTime.MinValue if invalid
        model.ParentDateOfBirth.Should().Be(DateTime.MinValue.ToString());
    }
    
    [Test]
    public async Task Loader_When_ParentDetails_Is_Null_Should_Handle_Gracefully_In_Error_Case()
    {
        // Arrange
        _tempData["Response"] = JsonConvert.SerializeObject(_fixture.Create<CheckEligibilityResponse>());
        _tempData["EligibilityType"] = "EYPP";
        _tempData["ParentDetails"] = null;
        
        // Setup mock to throw an exception when called
        _getCheckStatusUseCaseMock
            .Setup(x => x.Execute(It.IsAny<string>(), _sessionMock.Object))
            .ThrowsAsync(new Exception("Test exception"));
            
        _loadParentDetailsUseCaseMock
            .Setup(x => x.Execute(
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync((null, null));

        // Act
        var result = await _sut.Loader();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be("Outcome/Technical_Error");
        
        // The view model should still be created with default values
        var model = viewResult.Model as EligibilityOutcomeViewModel;
        model.Should().NotBeNull();
        model.EligibilityType.Should().Be("EYPP");
        model.ParentLastName.Should().Be(string.Empty);
        // In the actual implementation, with null parent details, 
        // GetDateOfBirth returns DateTime.MinValue.ToString() which ends up empty
        // in some cultures and with a value in others
        model.ParentNino.Should().Be(string.Empty);
    }

    [TestCase("eligible", "Outcome/Eligible")]
    [TestCase("notEligible", "Outcome/Not_Eligible")]
    [TestCase("parentNotFound", "Outcome/Not_Found")]
    [TestCase("queuedForProcessing", "Loader")]
    [TestCase("error", "Outcome/Technical_Error")]
    public async Task Given_Poll_Status_With_Valid_Status_Returns_Correct_View(string status, string expectedView)
    {
        // Arrange
        var statusValue = _fixture.Build<StatusValue>()
            .With(x => x.Status, status)
            .Create();

        var checkEligibilityResponse = _fixture.Build<CheckEligibilityResponse>()
            .With(x => x.Data, statusValue)
            .Create();

        _httpContext.Setup(ctx => ctx.Session).Returns(_sessionMock.Object);
        _sut.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "12345"),
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "test@example.com"),
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "John"),
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "Doe"),
            new("OrganisationCategoryName", Constants.CategoryTypeLA)
        }));

        var responseJson = JsonConvert.SerializeObject(checkEligibilityResponse);
        _tempData["Response"] = responseJson;
        _getCheckStatusUseCaseMock
            .Setup(x => x.Execute(responseJson, _sessionMock.Object))
            .ReturnsAsync(status);

        // Act
        var result = await _sut.Loader();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be(expectedView);
        _getCheckStatusUseCaseMock.Verify(x => x.Execute(responseJson, _sessionMock.Object), Times.Once);
    }

    [Test]
    public async Task Given_Poll_Status_When_Response_Is_Null_Returns_Error_Status()
    {
        // Arrange
        _tempData["Response"] = null;

        // Act
        var result = await _sut.Loader();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be("Outcome/Technical_Error");
    }

    [Test]
    public async Task Given_Poll_Status_When_Status_Is_Processing_Returns_Processing()
    {
        // Arrange
        var response = new CheckEligibilityResponse
        {
            Data = new StatusValue { Status = "queuedForProcessing" }
        };
        _tempData["Response"] = JsonConvert.SerializeObject(response);

        _getCheckStatusUseCaseMock.Setup(x => x.Execute(It.IsAny<string>(), _sessionMock.Object))
            .ReturnsAsync("queuedForProcessing");

        // Act
        var result = await _sut.Loader();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be("Loader");
    }

    [Test]
    public async Task Consent_Declaration_Should_Return_View()
    {
        // Act
        var result = await _sut.Consent_Declaration();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().BeNull(); // Uses default view name
    }

    [Test]
    [TestCase("checked", "Enter_Details")]
    [TestCase("notchecked", "Consent_Declaration")]
    public async Task Consent_Declaration_Approval_Should_Redirect_Based_On_Consent(string consent, string expectedAction)
    {
        // Act
        var result = await _sut.Consent_Declaration_Approval(consent);

        // Assert
        if (consent == "checked")
        {
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be(expectedAction);
        }
        else
        {
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be(expectedAction);
            viewResult.Model.Should().Be(true);
        }
    }
}