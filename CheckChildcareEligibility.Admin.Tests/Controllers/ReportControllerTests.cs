using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Controllers;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.Infrastructure;
using CheckChildcareEligibility.Admin.Models;
using CheckChildcareEligibility.Admin.UseCases;
using CheckChildcareEligibility.Admin.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CheckChildcareEligibility.Admin.Tests.Controllers;
[TestFixture]
public class ReportControllerTests : TestBase
{
    private Mock<IMenuProvider> _menuProviderMock;
    private Mock<IPerformEligibilityCodeHistoryReportUseCase> _performEligibilityCodeHistoryReportUseCaseMock;
    private Mock<IValidateEligibilityCodeUseCase> _validateEligibilityCodeUseCaseMock;
    private Mock<IDfeSignInApiService> _mockDfeSignInApiService;

    private ReportController _sut;

    [SetUp]
    public void SetUp()
    {
        _menuProviderMock = new Mock<IMenuProvider>();
        _performEligibilityCodeHistoryReportUseCaseMock =
        new Mock<IPerformEligibilityCodeHistoryReportUseCase>();
        _validateEligibilityCodeUseCaseMock =
        new Mock<IValidateEligibilityCodeUseCase>();
        _mockDfeSignInApiService =
        new Mock<IDfeSignInApiService>();

        _sut = new ReportController(
        _menuProviderMock.Object,
        _performEligibilityCodeHistoryReportUseCaseMock.Object,
        _validateEligibilityCodeUseCaseMock.Object,
        _mockDfeSignInApiService.Object);

        SetUpSessionData();
        base.SetUp();

        _sut.ControllerContext.HttpContext = _httpContext.Object;
        _sut.GetDfeClaimsAsync().Wait();
        _sut.TempData = _tempData;
    }

    [TearDown]
    public void TearDown()
    {
        _sut.Dispose();
    }

    [Test]
    public void Reports_Should_Return_View()
    {
        // Act
        var result = _sut.Reports();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [Test]
    public void CodeSearch_Get_Should_Return_View_With_Empty_Model()
    {
        // Act
        var result = _sut.Code_Search();

        // Assert
        var viewResult = result as ViewResult;

        viewResult.Should().NotBeNull();
        viewResult!.Model.Should().BeOfType<EligibilityCodeSearchViewModel>();
    }

    [Test]
    public void CodeSearch_Get_Should_Load_Errors_From_TempData()
    {
        // Arrange
        var errors = new Dictionary<string, List<string>>
        {
            {
                "EligibilityCode", new List<string> { "Eligibility code must be 11 digits long" }
            }
        };
        _sut.TempData["Errors"] = JsonConvert.SerializeObject(errors);
        _sut.TempData["EligibilityCode"] = "123";

        // Act
        var result = _sut.Code_Search();

        // Assert
        var viewResult = result as ViewResult;
        var model = viewResult!.Model as EligibilityCodeSearchViewModel;

        model!.EligibilityCode.Should().Be("123");
        _sut.ModelState.ContainsKey("EligibilityCode").Should().BeTrue();
        _sut.ModelState["EligibilityCode"]!.Errors.Should().HaveCount(1);
    }

    [Test]
    public async Task CodeSearch_Post_When_Validation_Fails_Should_Redirect()
    {
        // Arrange
        const string eligibilityCode = "123";

        _validateEligibilityCodeUseCaseMock.Setup(x => x.Execute(eligibilityCode)).Returns(new ValidationResult
        {
            IsValid = false,
            Errors = new Dictionary<string, List<string>>
                {
                    {
                        "EligibilityCode",
                        new List<string>
                        {
                            "Eligibility code must be 11 digits long"
                        }
                    }
                }
        });

        // Act
        var result = await _sut.Code_Search(eligibilityCode);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be("Code_Search");

        _sut.TempData["EligibilityCode"].Should().Be(eligibilityCode);
        _sut.TempData["Errors"].Should().NotBeNull();
    }

    [Test]
    public async Task CodeSearch_Post_When_Validation_Passes_Should_Return_EventHistory_View()
    {
        // Arrange
        const string eligibilityCode = "12345678901";

        _validateEligibilityCodeUseCaseMock.Setup(x => x.Execute(eligibilityCode))
            .Returns(new ValidationResult
            {
                IsValid = true
            });

        var reportResponse = new WorkingFamilyEventByEligibilityCodeResponse
        {
            Data = new List<WorkingFamilyEventByEligibilityCodeResponseItem>
            {
                new()
                {
                    Event = WorkingFamilyEventType.Application,
                    Record = new WorkingFamiliesEventEligibilityCodeResponseRecord
                    {
                        EventId = "123",
                        ParentFirstName = "John",
                        ParentLastName = "Smith"
                    }
                }
            }
        };

        _performEligibilityCodeHistoryReportUseCaseMock
            .Setup(x => x.Execute(eligibilityCode))
            .ReturnsAsync(reportResponse);

        // Act
        var result = await _sut.Code_Search(eligibilityCode);

        // Assert
        var viewResult = result as ViewResult;

        viewResult.Should().NotBeNull();
        viewResult!.ViewName.Should().Be("Event_History");

        var model = viewResult.Model as EligibilityCodeHistoryReportViewModel;

        model.Should().NotBeNull();
        model!.EligibilityCode.Should().Be(eligibilityCode);
        model.Response.Should().Be(reportResponse);
    }
    [Test]
    public async Task CodeSearch_Post_When_Validation_Passes_And_Result_Empty_Should_Return_No_Match_View()
    {
        // Arrange
        const string eligibilityCode = "12345678901";

        _validateEligibilityCodeUseCaseMock.Setup(x => x.Execute(eligibilityCode))
            .Returns(new ValidationResult
            {
                IsValid = true
            });

        var reportResponse = new WorkingFamilyEventByEligibilityCodeResponse
        {
            Data = new List<WorkingFamilyEventByEligibilityCodeResponseItem>()
        };

        _performEligibilityCodeHistoryReportUseCaseMock
            .Setup(x => x.Execute(eligibilityCode))
            .ReturnsAsync(reportResponse);

        // Act
        var result = await _sut.Code_Search(eligibilityCode);

        // Assert
        var viewResult = result as ViewResult;

        viewResult.Should().NotBeNull();
        viewResult!.ViewName.Should().Be("No_Match");

        var model = viewResult.Model as EligibilityCodeHistoryReportViewModel;
    }
}