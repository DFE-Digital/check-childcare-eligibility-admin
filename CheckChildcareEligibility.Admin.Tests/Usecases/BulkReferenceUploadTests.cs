using System.Globalization;
using System.Text;
using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Controllers;
using CheckChildcareEligibility.Admin.Domain.Enums;
using CheckChildcareEligibility.Admin.Domain.Validation;
using CheckChildcareEligibility.Admin.Gateways.Interfaces;
using CheckChildcareEligibility.Admin.Infrastructure;
using CheckChildcareEligibility.Admin.Usecases;
using CsvHelper;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CheckChildcareEligibility.Admin.Tests.Usecases;

[TestFixture]
public class BulkReferenceUploadTests
{
    private static readonly CheckEligibilityType[] Types =
    [
        CheckEligibilityType.TwoYearOffer,
        CheckEligibilityType.EarlyYearPupilPremium,
        CheckEligibilityType.WorkingFamilies
    ];

    private static IConfiguration Configuration => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?> { ["BulkEligibilityCheckLimit"] = "250" })
        .Build();

    private static IEnumerable<TestCaseData> AcceptedReferences()
    {
        var references = new[]
        {
            "", new string(' ', 101), new string('a', 99), new string('a', 100),
            " \t" + new string('a', 100) + "\t ", "  MiXeD  Case-001  ",
            "  Référence, \"quoted\"  "
        };
        foreach (var type in Types)
        foreach (var reference in references)
            yield return new TestCaseData(type, reference);
    }

    [TestCaseSource(nameof(AcceptedReferences))]
    public async Task Execute_Trims_Before_Validation_And_Preserves_Reference_In_Api_Payload(
        CheckEligibilityType type, string reference)
    {
        var result = await Parse(type, Header(type) + "\r\n" + Row(type, reference));

        result.ErrorMessage.Should().BeEmpty();
        result.Errors.Should().BeEmpty();
        var request = result.ValidRequests.Should().ContainSingle().Which;
        request.Reference.Should().Be(reference.Trim());
        request.Type.Should().Be(type);
        request.Order.Should().Be(1);
        request.NationalInsuranceNumber.Should().Be("AB123456C");

        // The gateway uses Newtonsoft.Json and the bulk DTO's interface-typed collection.
        var payload = JObject.Parse(JsonConvert.SerializeObject(new CheckEligibilityRequestBulk
        {
            Data = result.ValidRequests,
            Meta = new CheckEligibilityRequestBulkMeta { Filename = "references.csv" }
        }));
        payload["Data"]![0]!["Reference"]!.Value<string>().Should().Be(reference.Trim());
    }

    [TestCaseSource(nameof(Types))]
    public async Task Execute_Rejects_101_Characters_After_Trim_With_Row_Number_And_Keeps_Row_Association(
        CheckEligibilityType type)
    {
        var result = await Parse(type, Header(type) + "\r\n"
            + Row(type, "  FIRST  ") + "\r\n"
            + Row(type, "  " + new string('x', 101) + "  ") + "\r\n"
            + Row(type, "  THIRD  "));

        result.ErrorMessage.Should().BeEmpty();
        var error = result.Errors.Should().ContainSingle().Which;
        error.LineNumber.Should().Be(3);
        error.Message.Should().Be("Reference must be 100 characters or fewer.");
        result.ValidRequests.Select(x => x.Reference).Should().Equal("FIRST", "THIRD");
        result.ValidRequests.Select(x => x.Order).Should().Equal(1, 3);
    }

    [Test]
    public async Task Execute_Identifies_Missing_Exact_Reference_Header(
        [ValueSource(nameof(Types))] CheckEligibilityType type,
        [Values("", ",Reference", ",Reference(optional)")] string referenceHeader)
    {
        var legacyHeader = string.Join(",", Header(type).Split(',').Take(3));
        var result = await Parse(type, legacyHeader + referenceHeader + "\r\n" + Row(type, ""));

        result.ErrorMessage.Should().Be("The selected file must include the column heading Reference (optional).");
        result.ValidRequests.Should().BeEmpty();
    }

    [Test]
    public async Task Execute_Preserves_Strict_Header_Schema(
        [ValueSource(nameof(Types))] CheckEligibilityType type,
        [Values("reordered", "extra", "duplicate", "incorrect")] string variation)
    {
        var headers = Header(type).Split(',');
        var header = variation switch
        {
            "reordered" => string.Join(",", headers.Reverse()),
            "extra" => Header(type) + ",Extra",
            "duplicate" => Header(type) + ",Reference (optional)",
            _ => "Incorrect," + string.Join(",", headers.Skip(1))
        };

        var result = await Parse(type, header + "\r\n" + Row(type, ""));

        result.ErrorMessage.Should().Be("The column headings in the selected file must exactly match the template");
        result.ValidRequests.Should().BeEmpty();
    }

    [TestCaseSource(nameof(Types))]
    public async Task Execute_Allows_Omitted_Trailing_Value_When_Header_Is_Present(CheckEligibilityType type)
    {
        var result = await Parse(type, Header(type) + "\r\n" + Row(type, "").TrimEnd(','));

        result.ErrorMessage.Should().BeEmpty();
        result.Errors.Should().BeEmpty();
        result.ValidRequests.Should().ContainSingle().Which.Reference.Should().BeNullOrEmpty();
    }

    [TestCaseSource(nameof(Types))]
    public async Task Execute_Header_Only_Still_Returns_No_Requests(CheckEligibilityType type)
    {
        var result = await Parse(type, Header(type) + "\r\n");

        result.ErrorMessage.Should().BeEmpty();
        result.Errors.Should().BeEmpty();
        result.ValidRequests.Should().BeEmpty();
    }

    [TestCase("2YO", CheckEligibilityType.TwoYearOffer, "BulkCheckTemplate.csv")]
    [TestCase("EYPP", CheckEligibilityType.EarlyYearPupilPremium, "BulkCheckTemplate.csv")]
    [TestCase("WF", CheckEligibilityType.WorkingFamilies, "WFBulkCheckTemplate.csv")]
    public async Task DownloadTemplate_Preserves_Filename_And_Serves_Schema_Accepted_By_Parser(
        string serviceType, CheckEligibilityType type, string fileName)
    {
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(x => x.WebRootPath).Returns(Path.Combine(TestContext.CurrentContext.TestDirectory, "TemplateWebRoot"));
        using var controller = new BulkCheckController(
            Mock.Of<ILogger<BulkCheckController>>(), Mock.Of<ICheckGateway>(), Configuration,
            environment.Object, new ParseBulkCheckFileUseCase(new CheckEligibilityRequestDataValidator(), Configuration),
            Mock.Of<IGetBulkCheckStatusesUseCase>(), Mock.Of<IDeleteBulkCheckFileUseCase>(),
            Mock.Of<IDfeSignInApiService>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var file = controller.DownloadTemplate(serviceType).Should().BeOfType<PhysicalFileResult>().Which;
        file.FileDownloadName.Should().Be(fileName);
        file.ContentType.Should().Be("text/csv");
        using var reader = new StreamReader(file.FileName);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Read();
        csv.ReadHeader();
        csv.HeaderRecord.Should().Equal(Header(type).Split(','));
        csv.Read().Should().BeTrue();
        csv.Parser.Count.Should().Be(4);
        csv.GetField(3).Should().BeEmpty();

        // Approved example identities are illustrative; use valid synthetic data to check the schema.
        var parsed = await Parse(type, string.Join(",", csv.HeaderRecord!) + "\r\n" + Row(type, "  TEMPLATE-001  "));
        parsed.ErrorMessage.Should().BeEmpty();
        parsed.Errors.Should().BeEmpty();
        parsed.ValidRequests.Should().ContainSingle().Which.Reference.Should().Be("TEMPLATE-001");
    }

    private static string Header(CheckEligibilityType type) => type == CheckEligibilityType.WorkingFamilies
        ? "Eligibility code,National Insurance number,Child date of birth,Reference (optional)"
        : "Parent Last Name,Parent Date of Birth,Parent National Insurance Number,Reference (optional)";

    private static string Row(CheckEligibilityType type, string reference)
    {
        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        var fields = type == CheckEligibilityType.WorkingFamilies
            ? new[] { "50173110190", "ab123456c", "2022-01-01", reference }
            : new[] { "Smith", "1980-01-01", "ab123456c", reference };
        foreach (var field in fields)
            csv.WriteField(field);
        csv.NextRecord();
        return writer.ToString().TrimEnd('\r', '\n');
    }

    private static async Task<BulkCheckCsvResult> Parse(CheckEligibilityType type, string content)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        return await new ParseBulkCheckFileUseCase(new CheckEligibilityRequestDataValidator(), Configuration)
            .Execute(stream, type);
    }
}
