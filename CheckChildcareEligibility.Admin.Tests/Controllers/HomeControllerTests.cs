using CheckChildcareEligibility.Admin.Controllers;
using CheckChildcareEligibility.Admin.Domain.DfeSignIn;
using CheckChildcareEligibility.Admin.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using System.Collections.Generic;

namespace CheckChildcareEligibility.Admin.Tests.Controllers;

[TestFixture]
internal class HomeControllerTests
{
    [SetUp]
    public void SetUp()
    {
        _sut = new HomeController();
    }

    [TearDown]
    public void TearDown()
    {
        _sut.Dispose();
    }

    private HomeController _sut;

    [Test]
    public void Given_Accessibility_LoadsWithEmptyModel()
    {
        // Arrange
        var controller = new HomeController();

        // Act
        var result = controller.Accessibility();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be("Accessibility");
        viewResult.Model.Should().BeNull();
    }

    [Test]
    public void Given_Privacy_LoadsWithEmptyModel()
    {
        // Arrange
        var controller = new HomeController();

        // Act
        var result = controller.Privacy();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be("Privacy");
        viewResult.Model.Should().BeNull();
    }

    [Test]
    public void Given_Cookies_LoadsWithEmptyModel()
    {
        // Arrange
        var controller = new HomeController();

        // Act
        var result = controller.Cookies();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be("Cookies");
        viewResult.Model.Should().BeNull();
    }
    
    [Test]
    public void Given_Index_ReturnsClaims()
    {
        // Arrange
        var claims = new List<Claim>
        {
            // Add the required claims for DfeSignInExtensions.GetDfeClaims
            new Claim($"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/{ClaimConstants.NameIdentifier}", "user123"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "test@example.com"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "Test"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "User"),
            new Claim(ClaimConstants.Organisation, "{\"id\":\"12345678-1234-1234-1234-123456789012\",\"name\":\"Test Organisation\",\"category\":{\"id\": 2,\"name\":\"Local Authority\"}}"),
        };
        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = _sut.Index();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult.Model.Should().NotBeNull();
        viewResult.Model.Should().BeOfType<DfeClaims>();
        var dfeClaims = viewResult.Model as DfeClaims;
        dfeClaims.User.Should().NotBeNull();
        dfeClaims.User.Email.Should().Be("test@example.com");
        dfeClaims.User.FirstName.Should().Be("Test");
        dfeClaims.User.Surname.Should().Be("User");
    }

    [Test]
    public void Given_MenuSingleCheck_Get_ReturnsView()
    {
        // Arrange
        
        // Act
        var result = _sut.MenuSingleCheck();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be("MenuSingleCheck");
    }

    [Test]
    public void Given_MenuSingleCheck_Post_WithValidEligibilityType_RedirectsToEnterDetails()
    {
        // Arrange
        var eligibilityType = "FSM";
        
        // Create a mock TempData dictionary
        var tempData = new Mock<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionary>();
        _sut.TempData = tempData.Object;

        // Act
        var result = _sut.MenuSingleCheck(eligibilityType);

        // Assert
        var redirectResult = result as RedirectToActionResult;
        redirectResult.Should().NotBeNull();
        redirectResult.ActionName.Should().Be("Enter_Details");
        redirectResult.ControllerName.Should().Be("Check");
        
        // Verify that TempData was set with the eligibility type
        tempData.VerifySet(t => t["eligibilityType"] = eligibilityType);
    }

    [Test]
    public void Given_MenuSingleCheck_Post_WithNullEligibilityType_ReturnsBadRequest()
    {
        // Arrange
        string eligibilityType = null;

        // Act
        var result = _sut.MenuSingleCheck(eligibilityType);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult.Value.Should().Be("Eligibility type is required.");
    }

    [Test]
    public void Given_Guidance_ReturnsView()
    {
        // Act
        var result = _sut.Guidance();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be("Guidance");
    }

    [Test]
    public void Given_FSMFormDownload_ReturnsView()
    {
        // Act
        var result = _sut.FSMFormDownload();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be("FSMFormDownload");
    }
}