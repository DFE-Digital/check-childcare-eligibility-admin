using System.Text;
using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.UseCases;
using CheckChildcareEligibility.Admin.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace CheckChildcareEligibility.Admin.Tests.UseCases;

[TestFixture]
public class PerformWFEligibilityCheckUseCaseTests
{
    [TestCase("AB123456C")]
    [TestCase("ab 12 34 56 c")]
    [TestCase("ab-12.34/56c")]
    [TestCase("ab\t12\r\n3456c")]
    public async Task Execute_Should_Canonicalise_Nino_Without_Changing_Submitted_Model(
        string input)
    {
        var gateway = new Mock<ICheckGateway>(MockBehavior.Strict);
        var session = new Mock<ISession>();
        var sessionStorage = new Dictionary<string, byte[]>();

        session
            .Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Callback<string, byte[]>((key, value) =>
                sessionStorage[key] = value);

        var request = new ParentAndChildViewModel
        {
            NationalInsuranceNumber = input
        };
        request.Child.EligibilityCode = "50173110190";
        request.Child.Day = "01";
        request.Child.Month = "06";
        request.Child.Year = "2022";

        var expectedResponse = new CheckEligibilityResponse();
        CheckEligibilityRequest? capturedRequest = null;

        gateway
            .Setup(g => g.PostCheck(It.IsAny<CheckEligibilityRequest>()))
            .Callback<CheckEligibilityRequest>(value =>
                capturedRequest = value)
            .ReturnsAsync(expectedResponse);

        var sut = new PerformWFEligibilityCheckUseCase(gateway.Object);

        var result = await sut.Execute(request, session.Object);

        result.Should().BeSameAs(expectedResponse);
        capturedRequest.Should().NotBeNull();

        var data = capturedRequest!.Data.Should()
            .BeOfType<CheckEligibilityRequestWorkingFamiliesData>().Subject;

        data.Type.Should().Be(CheckEligibilityType.WorkingFamilies);
        data.EligibilityCode.Should().Be("50173110190");
        data.DateOfBirth.Should().Be("2022-06-01");
        data.NationalInsuranceNumber.Should().Be("AB123456C");

        Encoding.UTF8.GetString(sessionStorage["EligibilityCode"])
            .Should().Be("50173110190");
        Encoding.UTF8.GetString(sessionStorage["ChildDOB"])
            .Should().Be("2022-06-01");
        Encoding.UTF8.GetString(sessionStorage["ParentNINO"])
            .Should().Be("AB123456C");

        request.NationalInsuranceNumber.Should().Be(input);

        gateway.Verify(
            g => g.PostCheck(It.IsAny<CheckEligibilityRequest>()),
            Times.Once);
    }
}