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
    private Mock<IDfeSignInApiService> _mockDfeSignInApiService;
    private HomeController _sut;

    [SetUp]
    public void SetUp()
    {
        _mockDfeSignInApiService = new Mock<IDfeSignInApiService>();
        _sut = new HomeController(_mockDfeSignInApiService.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _sut.Dispose();
    }

    [Test]
    public void Given_Accessibility_LoadsWithEmptyModel()
    {
        // Arrange

        // Act
        var result = _sut.Accessibility();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be("Accessibility");
        viewResult.Model.Should().BeNull();
    }

    [Test]
    public void Given_Cookies_LoadsWithEmptyModel()
    {
        // Arrange

        // Act
        var result = _sut.Cookies();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be("Cookies");
        viewResult.Model.Should().BeNull();
    }
    
    [Test]
    public async Task Given_Index_WithValidLocalAuthorityAndRole_ReturnsClaims()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var userId = "user123";
        var claims = new List<Claim>
        {
            new Claim($"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/{ClaimConstants.NameIdentifier}", userId),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "test@example.com"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "Test"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "User"),
            new Claim(ClaimConstants.Organisation, $"{{\"id\":\"{organisationId}\",\"name\":\"Test Organisation\",\"category\":{{\"id\": 2,\"name\":\"Local Authority\"}}}}"),
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

        // Mock the API service to return the required role
        var roles = new List<Role>
        {
            new Role { Code = "mefcsLocalAuthority", Name = "MEFCS - Local Authority Role" }
        };
        _mockDfeSignInApiService
            .Setup(x => x.GetUserRolesAsync(userId, organisationId))
            .ReturnsAsync(roles);

        // Act
        var result = await _sut.Index();

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
        dfeClaims.Roles.Should().HaveCount(1);
        dfeClaims.Roles.First().Code.Should().Be("mefcsLocalAuthority");
    }

    [Test]
    public async Task Given_Index_WithNonLocalAuthority_ReturnsUnauthorizedOrganizationView()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var userId = "user123";
        var claims = new List<Claim>
        {
            new Claim($"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/{ClaimConstants.NameIdentifier}", userId),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "test@example.com"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "Test"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "User"),
            new Claim(ClaimConstants.Organisation, $"{{\"id\":\"{organisationId}\",\"name\":\"Test School\",\"category\":{{\"id\": 1,\"name\":\"School\"}}}}"),
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
        var result = await _sut.Index();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult.ViewName.Should().Be("UnauthorizedOrganization");
    }

    [Test]
    public async Task Given_Index_WithLocalAuthorityButNoRole_ReturnsUnauthorizedRoleView()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var userId = "user123";
        var claims = new List<Claim>
        {
            new Claim($"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/{ClaimConstants.NameIdentifier}", userId),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "test@example.com"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "Test"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "User"),
            new Claim(ClaimConstants.Organisation, $"{{\"id\":\"{organisationId}\",\"name\":\"Test Organisation\",\"category\":{{\"id\": 2,\"name\":\"Local Authority\"}}}}"),
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

        // Mock the API service to return no roles
        _mockDfeSignInApiService
            .Setup(x => x.GetUserRolesAsync(userId, organisationId))
            .ReturnsAsync(new List<Role>());

        // Act
        var result = await _sut.Index();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult.ViewName.Should().Be("UnauthorizedRole");
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
    public void Given_ManageFosterFamilies_Get_ReturnsView()
    {
        // Arrange

        // Act
        var result = _sut.ManageFosterFamilies();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult.ViewName.Should().Be("ManageFosterFamilies");
    }

    [Test]
    public void Given_Guidance_ReturnsView()
    {
        // Act
        var result = _sut.GuidanceForReviewingEvidence();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be("GuidanceForReviewingEvidence");
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