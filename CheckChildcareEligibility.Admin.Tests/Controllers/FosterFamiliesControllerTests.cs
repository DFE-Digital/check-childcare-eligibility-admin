using System.Security.Claims;
using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Controllers;
using CheckChildcareEligibility.Admin.Domain.DfeSignIn;
using CheckChildcareEligibility.Admin.Infrastructure;
using CheckChildcareEligibility.Admin.Services;
using CheckChildcareEligibility.Admin.Usecases;
using CheckChildcareEligibility.Admin.UseCases;
using CheckChildcareEligibility.Admin.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace CheckChildcareEligibility.Admin.Tests.Controllers;

[TestFixture]
public class FosterFamiliesControllerTests : TestBase
{
    private Mock<ISessionContextService> _sessionContextService = null!;
    private Mock<ISearchFosterFamiliesRecordsUseCase> _searchRecords = null!;
    private Mock<ILoadFosterCarerDetailsUseCase> _loadCarer = null!;
    private Mock<ILoadFosterPartnerDetailsUseCase> _loadPartner = null!;
    private Mock<ILoadFosterChildDetailsUseCase> _loadChild = null!;
    private Mock<IValidateFosterCarerDetailsUseCase> _validateCarer = null!;
    private Mock<IValidateFosterPartnerDetailsUseCase> _validatePartner = null!;
    private Mock<IValidateFosterChildDetailsUseCase> _validateChild = null!;
    private Mock<IValidateFosterApplicationSubmittedDateUseCase> _validateSubmittedDate = null!;
    private Mock<IValidateFosterCodeReconfirmDateUseCase> _validateReconfirmDate = null!;
    private Mock<ICreateFosterFamilyUseCase> _createFamily = null!;
    private Mock<IGetFosterFamilyUseCase> _getFamily = null!;
    private Mock<IGetFosterChildUseCase> _getChild = null!;
    private Mock<IUpdateFosterCarerUseCase> _updateCarer = null!;
    private Mock<IPreviewFosterFamilyCodeUseCase> _previewCode = null!;
    private Mock<IPreviewFosterCodeReconfirmUseCase> _previewReconfirm = null!;
    private Mock<IReconfirmFosterCodeUseCase> _reconfirmCode = null!;
    private Mock<IUpdateFosterChildUseCase> _updateChild = null!;
    private Mock<IDfeSignInApiService> _dfeSignIn = null!;
    private FosterFamiliesController _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sessionContextService = new Mock<ISessionContextService>();
        _searchRecords = new Mock<ISearchFosterFamiliesRecordsUseCase>();
        _loadCarer = new Mock<ILoadFosterCarerDetailsUseCase>();
        _loadPartner = new Mock<ILoadFosterPartnerDetailsUseCase>();
        _loadChild = new Mock<ILoadFosterChildDetailsUseCase>();
        _validateCarer = new Mock<IValidateFosterCarerDetailsUseCase>();
        _validatePartner = new Mock<IValidateFosterPartnerDetailsUseCase>();
        _validateChild = new Mock<IValidateFosterChildDetailsUseCase>();
        _validateSubmittedDate = new Mock<IValidateFosterApplicationSubmittedDateUseCase>();
        _validateReconfirmDate = new Mock<IValidateFosterCodeReconfirmDateUseCase>();
        _createFamily = new Mock<ICreateFosterFamilyUseCase>();
        _getFamily = new Mock<IGetFosterFamilyUseCase>();
        _getChild = new Mock<IGetFosterChildUseCase>();
        _updateCarer = new Mock<IUpdateFosterCarerUseCase>();
        _previewCode = new Mock<IPreviewFosterFamilyCodeUseCase>();
        _previewReconfirm = new Mock<IPreviewFosterCodeReconfirmUseCase>();
        _reconfirmCode = new Mock<IReconfirmFosterCodeUseCase>();
        _updateChild = new Mock<IUpdateFosterChildUseCase>();
        _dfeSignIn = new Mock<IDfeSignInApiService>();
        _sut = new FosterFamiliesController(
            _sessionContextService.Object,
            _searchRecords.Object,
            _loadCarer.Object,
            _loadPartner.Object,
            _loadChild.Object,
            _validateCarer.Object,
            _validatePartner.Object,
            _validateChild.Object,
            _validateSubmittedDate.Object,
            _validateReconfirmDate.Object,
            _createFamily.Object,
            _getFamily.Object,
            _getChild.Object,
            _updateCarer.Object,
            _previewCode.Object,
            _previewReconfirm.Object,
            _reconfirmCode.Object,
            _updateChild.Object,
            _dfeSignIn.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            },
            TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        };
    }

    [TearDown]
    public void TearDown()
    {
        _sut.Dispose();
    }

    [Test]
    public async Task Search_MapsSearchResponseToViewModel()
    {
        var records = new[]
        {
            new FosterFamiliesSearchItemResponse { FosterCarerId = Guid.NewGuid(), CarerName = "Alex Smith", ChildName = "Sam Smith" }
        };
        _searchRecords
            .Setup(x => x.Execute(It.IsAny<FosterFamiliesSearchRequest>()))
            .ReturnsAsync(new FosterFamiliesSearchResponse
            {
                PageNumber = 3,
                PageSize = 10,
                TotalNumberOfRecords = 21,
                Data = records
            });

        var result = await _sut.Search_Records_FF(3, "AB123456C");

        var view = result.Should().BeOfType<ViewResult>().Subject;
        var model = view.Model.Should().BeOfType<SearchFosterFamiliesRecordsViewModel>().Subject;
        model.PageNumber.Should().Be(3);
        model.PageSize.Should().Be(10);
        model.TotalNumberOfRecords.Should().Be(21);
        model.Data.Should().BeSameAs(records);
        _searchRecords.Verify(x => x.Execute(It.Is<FosterFamiliesSearchRequest>(r =>
            r.PageNumber == 3 && r.PageSize == 10 && r.NINOFilter == "AB123456C")), Times.Once);
    }

    [Test]
    public async Task EnterCarerGet_WithoutContextId_CreatesNewContext()
    {
        var result = await _sut.Enter_Carer_Details_FF((string?)null);

        var model = result.Should().BeOfType<ViewResult>().Subject.Model
            .Should().BeOfType<FosterCarerDetailsViewModel>().Subject;
        Guid.TryParse(model.ContextId, out _).Should().BeTrue();
        _sessionContextService.Verify(
            x => x.GetSessionData<FosterCarerDetailsViewModel>(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public async Task EnterChildGet_WithoutCarerSession_RedirectsToCarerEntry()
    {
        var result = await _sut.Enter_Child_Details_FF("journey");

        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("Enter_Carer_Details_FF");
        _validateChild.Verify(
            x => x.Execute(It.IsAny<FosterChildDetailsViewModel>(), It.IsAny<Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary>()),
            Times.Never);
    }

    [Test]
    public async Task EnterChildPost_WhenSubmittedDateExists_RedirectsToCheckDetails()
    {
        var request = new FosterChildDetailsViewModel { ContextId = "journey" };
        var submittedDate = new FosterApplicationSubmittedDateViewModel { ContextId = "journey" };
        _validateChild
            .Setup(x => x.Execute(request, It.IsAny<Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary>()))
            .ReturnsAsync(new FosterChildDetailsValidationResult { IsValid = true });
        _sessionContextService
            .Setup(x => x.GetSessionData<FosterApplicationSubmittedDateViewModel>("journey", "FosterApplicationSubmittedDate"))
            .Returns(submittedDate);

        var result = await _sut.Enter_Child_Details_FF(request);

        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("Check_Details_FF");
        _sessionContextService.Verify(
            x => x.GetSessionData<FosterApplicationSubmittedDateViewModel>("journey", "FosterApplicationSubmittedDate"),
            Times.Once);
        _sessionContextService.Verify(
            x => x.SetSessionData("journey", "FosterChildDetails", request),
            Times.Exactly(2));
    }

    [Test]
    public async Task EnterChildPost_WhenValidationFails_RedirectsBackToChildForm()
    {
        var request = new FosterChildDetailsViewModel { ContextId = "journey" };
        _validateChild
            .Setup(x => x.Execute(request, It.IsAny<Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary>()))
            .ReturnsAsync(new FosterChildDetailsValidationResult { IsValid = false });

        var result = await _sut.Enter_Child_Details_FF(request);

        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("Enter_Child_Details_FF");
        _sessionContextService.Verify(
            x => x.SetSessionData("journey", "FosterChildDetails", request),
            Times.Once);
    }

    [Test]
    public async Task EnterPartnerPost_WhenValidationResultIsNull_RedirectsBackToPartnerForm()
    {
        var request = new FosterPartnerDetailsViewModel { ContextId = "journey" };
        _validatePartner
            .Setup(x => x.Execute(request, It.IsAny<Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary>()))
            .ReturnsAsync((FosterPartnerDetailsValidationResult)null!);

        var result = await _sut.Enter_Partner_Details_FF(request);

        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("Enter_Partner_Details_FF");
    }

    [Test]
    public async Task EnterCarerPost_WhenValidationResultIsNull_RedirectsBackToCarerForm()
    {
        var request = new FosterCarerDetailsViewModel { ContextId = "journey" };
        _validateCarer
            .Setup(x => x.Execute(request, It.IsAny<Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary>()))
            .ReturnsAsync((FosterCarerDetailsValidationResult)null!);

        var result = await _sut.Enter_Carer_Details_FF(request);

        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("Enter_Carer_Details_FF");
    }

    [Test]
    public async Task CheckDetailsGet_WhenSubmittedDateIsMissing_RedirectsToDateEntry()
    {
        var contextId = "journey";
        _sessionContextService
            .Setup(x => x.GetSessionData<FosterCarerDetailsViewModel>(contextId, "FosterCarerDetails"))
            .Returns(new FosterCarerDetailsViewModel { HasPartner = false });
        _sessionContextService
            .Setup(x => x.GetSessionData<FosterChildDetailsViewModel>(contextId, "FosterChildDetails"))
            .Returns(new FosterChildDetailsViewModel());

        var result = await _sut.Check_Details_FF(contextId);

        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("Enter_Submitted_Date_Details_FF");
        _previewCode.Verify(x => x.Execute(It.IsAny<FosterFamilyRequest>()), Times.Never);
    }

    [Test]
    public async Task CheckDetailsPost_WhenContextIdIsMissing_RedirectsToCarerEntry()
    {
        var result = await _sut.Check_Details_FF(new FosterApplicationCheckDetailsViewModel());

        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("Enter_Carer_Details_FF");
        _createFamily.Verify(x => x.Execute(It.IsAny<FosterFamilyRequest>()), Times.Never);
    }

    [Test]
    public async Task CheckDetailsPost_WhenCreationSucceeds_RedirectsToCreatedCode()
    {
        await SetLocalAuthorityClaims();
        var contextId = "journey";
        SetupCompleteJourney(contextId);
        var childId = Guid.NewGuid();
        _createFamily
            .Setup(x => x.Execute(It.IsAny<FosterFamilyRequest>()))
            .ReturnsAsync(new FosterFamilyCreatedResponse { FosterChildId = childId });

        var result = await _sut.Check_Details_FF(new FosterApplicationCheckDetailsViewModel { ContextId = contextId });

        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("Code_Created_FF");
        ((RedirectToActionResult)result).RouteValues!["fosterChildId"].Should().Be(childId);
        _createFamily.Verify(x => x.Execute(It.Is<FosterFamilyRequest>(request =>
            request.FosterCarer.LocalAuthorityID == 12345 &&
            request.SubmissionDate == new DateTime(2026, 1, 2) &&
            request.HasPartner == false)), Times.Once);
    }

    [Test]
    public async Task CheckDetailsPost_WhenCreateReturnsBadRequest_SavesErrorAndRedirectsBack()
    {
        await SetLocalAuthorityClaims();
        var contextId = "journey";
        SetupCompleteJourney(contextId);
        _createFamily
            .Setup(x => x.Execute(It.IsAny<FosterFamilyRequest>()))
            .ThrowsAsync(new BadHttpRequestException("Unable to create family"));

        var result = await _sut.Check_Details_FF(new FosterApplicationCheckDetailsViewModel { ContextId = contextId });

        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("Check_Details_FF");
        _sut.TempData["ErrorMessage"].Should().Be("Unable to create family");
    }

    [Test]
    public async Task UpdateCarerPost_WhenValidationResultIsNull_RedirectsWithoutUpdatingFamily()
    {
        var carerId = Guid.NewGuid();
        var request = new FosterCarerDetailsViewModel { FosterCarerId = carerId };
        _getFamily.Setup(x => x.Execute(carerId, true))
            .ReturnsAsync(new FosterFamilyResponse { FosterCarerId = carerId });
        _validateCarer
            .Setup(x => x.Execute(request, It.IsAny<Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary>()))
            .ReturnsAsync((FosterCarerDetailsValidationResult)null!);

        var result = await _sut.Update_Carer_Details_FF(request);

        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("Update_Carer_Details_FF");
        _updateCarer.Verify(x => x.Execute(It.IsAny<Guid>(), It.IsAny<UpdateFosterCarerRequest>()), Times.Never);
    }

    [Test]
    public async Task UpdatePartnerPost_WhenValidationResultIsNull_RedirectsWithoutUpdatingFamily()
    {
        var carerId = Guid.NewGuid();
        var request = new FosterPartnerDetailsViewModel { FosterCarerId = carerId };
        _getFamily.Setup(x => x.Execute(carerId, true))
            .ReturnsAsync(new FosterFamilyResponse { FosterCarerId = carerId });
        _validatePartner
            .Setup(x => x.Execute(request, It.IsAny<Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary>()))
            .ReturnsAsync((FosterPartnerDetailsValidationResult)null!);

        var result = await _sut.Update_Partner_Details_FF(request);

        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("Update_Partner_Details_FF");
        _updateCarer.Verify(x => x.Execute(It.IsAny<Guid>(), It.IsAny<UpdateFosterCarerRequest>()), Times.Never);
    }

    [Test]
    public async Task UpdateChildPost_WhenValidationSucceeds_UpdatesChildAndRedirectsToCodeRecord()
    {
        var childId = Guid.NewGuid();
        var request = new FosterChildDetailsViewModel
        {
            FosterChildId = childId,
            ChildFirstName = "Sam",
            ChildLastName = "Smith",
            ChildDateOfBirth = new DateTime(2020, 1, 1),
            ChildPostCode = "SW1A 1AA"
        };
        _getChild.Setup(x => x.Execute(childId, true)).ReturnsAsync(new FosterChildResponse { FosterChildId = childId });
        _validateChild
            .Setup(x => x.Execute(request, It.IsAny<Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary>()))
            .ReturnsAsync(new FosterChildDetailsValidationResult { IsValid = true });

        var result = await _sut.Update_Child_Details_FF(request);

        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("Code_Record_FF");
        _updateChild.Verify(x => x.Execute(childId, It.Is<UpdateFosterChildRequest>(update =>
            update.FosterChildRequest.ChildFirstName == "Sam" &&
            update.FosterChildRequest.ChildPostCode == "SW1A 1AA")), Times.Once);
        _sessionContextService.Verify(
            x => x.ClearSessionData(childId.ToString(), "FosterChildDetails"),
            Times.Once);
    }

    [Test]
    public async Task RemovePartnerPost_UpdatesFamilyAndClearsCachedPartnerDetails()
    {
        var carerId = Guid.NewGuid();
        _getFamily.Setup(x => x.Execute(carerId, true))
            .ReturnsAsync(new FosterFamilyResponse
            {
                FosterCarerId = carerId,
                CarerFirstName = "Alex",
                CarerLastName = "Smith",
                CarerDateOfBirth = new DateTime(1980, 1, 1),
                CarerNationalInsuranceNumber = "AB123456C"
            });

        var result = await _sut.Remove_Partner_Details_FF(new FosterPartnerDetailsViewModel { FosterCarerId = carerId });

        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("Family_Record_FF");
        _updateCarer.Verify(x => x.Execute(carerId, It.Is<UpdateFosterCarerRequest>(update =>
            update.FosterCarerRequest.HasPartner == false &&
            update.FosterCarerRequest.CarerNationalInsuranceNumber == "AB123456C")), Times.Once);
        _sessionContextService.Verify(
            x => x.ClearSessionData(carerId.ToString(), "FosterPartnerDetails"),
            Times.Once);
    }

    [Test]
    public async Task UpdateCarerGet_WithMissingReferrer_DoesNotThrowAndLoadsDetails()
    {
        var carerId = Guid.NewGuid();
        _getFamily.Setup(x => x.Execute(carerId, false)).ReturnsAsync(new FosterFamilyResponse { FosterCarerId = carerId });
        _loadCarer.Setup(x => x.Execute(It.IsAny<FosterFamilyResponse>()))
            .ReturnsAsync(new FosterCarerDetailsViewModel { FosterCarerId = carerId });

        var result = await _sut.Update_Carer_Details_FF(carerId);

        result.Should().BeOfType<ViewResult>()
            .Which.ViewName.Should().Be("Enter_Carer_Details_FF");
        _sessionContextService.Verify(x => x.ClearSessionData(carerId.ToString(), "FosterCarerDetails"), Times.Once);
    }

    [Test]
    public async Task EnterReconfirmDatePost_WhenValidationFails_ReturnsFormWithoutSavingSession()
    {
        var request = new FosterCodeReconfirmDateViewModel { FosterChildId = Guid.NewGuid() };
        _validateReconfirmDate
            .Setup(x => x.Execute(request, It.IsAny<Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary>()))
            .Returns(new FosterCodeReconfirmedDateValidationResult { IsValid = false });

        var result = await _sut.Enter_Reconfirm_Date_FF(request);

        result.Should().BeOfType<ViewResult>()
            .Which.ViewName.Should().Be("Enter_Reconfirm_Date_Details_FF");
        _sessionContextService.Verify(
            x => x.SetSessionData(request.FosterChildId.ToString(), "FosterCodeReconfirmDate", request),
            Times.Never);
    }

    [Test]
    public async Task CheckReconfirmDetailsPost_WhenDateIsMissing_RedirectsToDateEntry()
    {
        var childId = Guid.NewGuid();

        var result = await _sut.Check_Reconfirm_Details_FF(new FosterCodeReconfirmCheckDetailsViewModel
        {
            FosterChildId = childId
        });

        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("Enter_Reconfirm_Date_FF");
        _reconfirmCode.Verify(x => x.Execute(It.IsAny<Guid>(), It.IsAny<FosterChildReconfirmRequest>()), Times.Never);
    }

    [Test]
    public async Task CheckReconfirmDetailsPost_WhenDateExists_ReconfirmsCodeUsingCachedDateAndEligibilityCode()
    {
        var childId = Guid.NewGuid();
        var submissionDate = new DateTime(2026, 2, 3);
        _sessionContextService
            .Setup(x => x.GetSessionData<FosterCodeReconfirmDateViewModel>(childId.ToString(), "FosterCodeReconfirmDate"))
            .Returns(new FosterCodeReconfirmDateViewModel { FosterChildId = childId, SubmissionDate = submissionDate });
        _getChild.Setup(x => x.Execute(childId, true))
            .ReturnsAsync(new FosterChildResponse { FosterChildId = childId, EligibilityCode = "FOSTER-CODE" });
        _reconfirmCode.Setup(x => x.Execute(childId, It.IsAny<FosterChildReconfirmRequest>()))
            .ReturnsAsync(new FosterChildResponse { FosterChildId = childId, EligibilityCode = "UPDATED-CODE" });

        var result = await _sut.Check_Reconfirm_Details_FF(new FosterCodeReconfirmCheckDetailsViewModel
        {
            FosterChildId = childId
        });

        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.ViewName.Should().Be("Code_Reconfirmed_FF");
        _reconfirmCode.Verify(x => x.Execute(childId, It.Is<FosterChildReconfirmRequest>(request =>
            request.SubmissionDate == submissionDate &&
            request.EligibilityCode == "FOSTER-CODE")), Times.Once);
    }

    private void SetupCompleteJourney(string contextId)
    {
        _sessionContextService
            .Setup(x => x.GetSessionData<FosterCarerDetailsViewModel>(contextId, "FosterCarerDetails"))
            .Returns(new FosterCarerDetailsViewModel
            {
                CarerFirstName = "Alex",
                CarerLastName = "Smith",
                CarerDateOfBirth = new DateTime(1980, 1, 1),
                CarerNationalInsuranceNumber = "AB123456C",
                HasPartner = false
            });
        _sessionContextService
            .Setup(x => x.GetSessionData<FosterChildDetailsViewModel>(contextId, "FosterChildDetails"))
            .Returns(new FosterChildDetailsViewModel
            {
                ChildFirstName = "Sam",
                ChildLastName = "Smith",
                ChildDateOfBirth = new DateTime(2020, 1, 1),
                ChildPostCode = "SW1A 1AA"
            });
        _sessionContextService
            .Setup(x => x.GetSessionData<FosterApplicationSubmittedDateViewModel>(contextId, "FosterApplicationSubmittedDate"))
            .Returns(new FosterApplicationSubmittedDateViewModel { SubmissionDate = new DateTime(2026, 1, 2) });
    }

    private async Task SetLocalAuthorityClaims()
    {
        var organisationId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(ClaimConstants.Organisation, $"{{\"id\":\"{organisationId}\",\"name\":\"Test LA\",\"category\":{{\"id\":2,\"name\":\"Local Authority\"}},\"establishmentNumber\":\"12345\"}}"),
            new($"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/{ClaimConstants.NameIdentifier}", "user-123"),
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "test@example.com"),
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "Test"),
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "User")
        };
        _sut.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
        _dfeSignIn.Setup(x => x.GetUserRolesAsync("user-123", organisationId)).ReturnsAsync([]);
        await _sut.GetDfeClaimsAsync();
    }
}
