using AutoFixture;
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
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System.Security.Claims;

namespace CheckChildcareEligibility.Admin.Tests.Controllers;

[TestFixture]
public class FosterFamiliesControllerTests : TestBase
{
    [SetUp]
    public void SetUp()
    {
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


    // System under test
    private FosterFamiliesController _sut;




    [Test]
    public void Given_ManageFosterFamilies_Get_ReturnsView()
    {
        // Arrange

        // Act
        var result = _sut.SearchFosterFamiliesRecords(1);

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult.ViewName.Should().Be("FosterFamilies");
    }
}