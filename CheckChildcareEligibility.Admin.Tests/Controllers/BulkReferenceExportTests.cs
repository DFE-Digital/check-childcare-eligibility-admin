using System.Globalization;
using System.Net;
using CheckChildcareEligibility.Admin.Controllers;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Gateways;
using CheckChildcareEligibility.Admin.Infrastructure;
using CheckChildcareEligibility.Admin.Usecases;
using CsvHelper;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;

namespace CheckChildcareEligibility.Admin.Tests.Controllers;

[TestFixture]
public class BulkReferenceExportTests : TestBase
{
    [Test]
    public async Task Download_Preserves_Reference_Column_Row_Association_And_Existing_Fields(
        [Values(CheckEligibilityType.TwoYearOffer, CheckEligibilityType.EarlyYearPupilPremium,
            CheckEligibilityType.WorkingFamilies)] CheckEligibilityType type,
        [Values(false, true)] bool downloadFromStatusPage,
        [Values("populated", "blank", "legacy", "empty")] string scenario)
    {
        var workingFamilies = type == CheckEligibilityType.WorkingFamilies;
        string?[] references = scenario == "populated"
            ? ["  MiXeD, \"Référence\"  ", "", " " + new string('a', 100) + " ", "  FIRST\nSECOND  "]
            : [null, "", "   ", ""];
        var statuses = workingFamilies
            ? new[] { "eligible", "notEligible", "notFound", "error" }
            : new[] { "eligible", "notEligible", "parentNotFound", "error" };
        var outcomes = workingFamilies
            ? new[] { "Code valid", "Code expired", "Information does not match records", "System error - try again later" }
            : new[] { "Eligible", "May not be eligible", "Information does not match records", "Try again" };
        var data = new JArray();
        if (scenario != "empty")
        {
            // Simulate API completion order differing from upload order.
            for (var i = 3; i >= 0; i--)
            {
                var item = new JObject
                {
                    ["Order"] = i + 1,
                    ["LastName"] = "Parent" + (i + 1),
                    ["NationalInsuranceNumber"] = "AB123456C",
                    ["DateOfBirth"] = workingFamilies ? "2022-01-01" : "1980-01-01",
                    ["Status"] = statuses[i],
                    ["EligibilityCode"] = "5017311019" + i,
                    ["ValidityStartDate"] = "2027-01-01",
                    ["ValidityEndDate"] = "2027-03-31",
                    ["GracePeriodEndDate"] = "2027-08-31"
                };
                if (scenario != "legacy")
                    item["Reference"] = references[i] == null ? JValue.CreateNull() : new JValue(references[i]);
                data.Add(item);
            }
        }

        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(new JObject { ["Data"] = data }.ToString())
        };
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get
                    && r.RequestUri!.AbsolutePath == "/bulk-check/reference-batch/"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        using var client = new HttpClient(handler.Object) { BaseAddress = new Uri("https://localhost:7000/") };
        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>());
        _httpContext.Object.Session.SetString("JwtToken", "test-token");
        _httpContext.Object.Session.SetString("JwtTokenExpiry", DateTime.UtcNow.AddHours(1).ToString("o"));
        _httpContext.Object.Session.SetString("Get_BulkCheck_Results", "reference-batch");
        var gateway = new CheckGateway(loggerFactory.Object, client, _configMock.Object,
            new HttpContextAccessor { HttpContext = _httpContext.Object });

        using var controller = new BulkCheckController(Mock.Of<ILogger<BulkCheckController>>(), gateway,
            _configMock.Object, Mock.Of<IWebHostEnvironment>(), Mock.Of<IParseBulkCheckFileUseCase>(),
            Mock.Of<IGetBulkCheckStatusesUseCase>(), Mock.Of<IDeleteBulkCheckFileUseCase>(),
            Mock.Of<IDfeSignInApiService>())
        {
            ControllerContext = new ControllerContext { HttpContext = _httpContext.Object },
            TempData = _tempData
        };
        controller.TempData["eligibilityType"] = type.ToString();

        var action = downloadFromStatusPage
            ? await controller.Bulk_check_file_download("reference-batch", type.ToString())
            : await controller.Bulk_check_download();

        var file = action.Should().BeOfType<FileStreamResult>().Which;
        file.ContentType.Should().Be("text/csv");
        var prefix = type switch
        {
            CheckEligibilityType.TwoYearOffer => "two-year-offer",
            CheckEligibilityType.EarlyYearPupilPremium => "early-year-pupil-premium",
            _ => "working-families"
        };
        file.FileDownloadName.Should().StartWith(prefix + "-outcomes-").And.EndWith(".csv");
        using var output = new MemoryStream();
        await file.FileStream.CopyToAsync(output);
        file.FileStream.Dispose();
        output.ToArray().Take(3).Should().Equal(new byte[] { 0xEF, 0xBB, 0xBF });
        output.Position = 0;
        using var reader = new StreamReader(output);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Read().Should().BeTrue();
        csv.ReadHeader();
        var headers = workingFamilies
            ? new[] { "Eligibility code", "National Insurance number", "Child date of birth", "Outcome",
                "Validity start date", "Validity end date", "Grace period ends", "Reference (optional)" }
            : new[] { "Parent Last Name", "Parent Date of Birth", "Parent National Insurance number",
                "Outcome", "Reference (optional)" };
        csv.HeaderRecord.Should().Equal(headers);
        var rows = new List<string[]>();
        while (csv.Read())
            rows.Add(csv.Parser.Record!.ToArray());
        rows.Should().HaveCount(scenario == "empty" ? 0 : 4);
        for (var i = 0; i < rows.Count; i++)
        {
            var reference = scenario == "legacy" ? "" : references[i]?.Trim() ?? "";
            var expected = workingFamilies
                ? new[] { "5017311019" + i, "AB123456C", "2022-01-01", outcomes[i],
                    "2027-01-01", "2027-03-31", "2027-08-31", reference }
                : new[] { "Parent" + (i + 1), "1980-01-01", "AB123456C", outcomes[i], reference };
            rows[i].Should().Equal(expected);
        }
        handler.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }
}
