using AutoFixture;
using CheckChildcareEligibility.Admin.UseCases;
using CheckChildcareEligibility.Admin.ViewModels;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace CheckChildcareEligibility.Admin.Tests.UseCases;

[TestFixture]
public class LoadParentAndChildDetailsUseCaseTests
{
    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<LoadParentAndChildDetailsUseCase>>();
        _sut = new LoadParentAndChildDetailsUseCase(_loggerMock.Object);
        _fixture = new Fixture();
    }

    private LoadParentAndChildDetailsUseCase _sut;
    private Mock<ILogger<LoadParentAndChildDetailsUseCase>> _loggerMock;
    private Fixture _fixture;

    [Test]
    public async Task Execute_WithValidData_ReturnsParentAndErrors()
    {
        // Arrange
        var parentAndChild = _fixture.Create<ParentAndChildViewModel>();
        var errors = new Dictionary<string, List<string>>
        {
            { "TestError", new List<string> { "Error message" } }
        };
        var parentAndChildJson = JsonConvert.SerializeObject(parentAndChild);
        var errorsJson = JsonConvert.SerializeObject(errors);

        // Act
        var result = await _sut.Execute(parentAndChildJson, errorsJson);
        var (resultParentAndChild, resultErrors) = result;

        // Assert
        resultParentAndChild.Should().BeEquivalentTo(parentAndChild);
        resultErrors.Should().BeEquivalentTo(errors);
    }

    [Test]
    public async Task Execute_WithNullData_ReturnsNullValues()
    {
        // Act
        var result = await _sut.Execute();
        var (resultParentAndChild, resultErrors) = result;

        // Assert
        resultParentAndChild.Should().BeNull();
        resultErrors.Should().BeNull();
    }

    [Test]
    public async Task Execute_WithInvalidParentAndChildJson_ReturnsNullParentAndChild()
    {
        // Arrange
        string invalidJson = "{invalid_json}";
        var errors = new Dictionary<string, List<string>>
        {
            { "TestError", new List<string> { "Error message" } }
        };
        var errorsJson = JsonConvert.SerializeObject(errors);

        // Act
        var result = await _sut.Execute(invalidJson, errorsJson);
        var (resultParentAndChild, resultErrors) = result;

        // Assert
        resultParentAndChild.Should().BeNull();
        resultErrors.Should().BeEquivalentTo(errors);
    }

    [Test]
    public async Task Execute_WithInvalidErrorsJson_ReturnsNullErrors()
    {
        // Arrange
        var parentAndChild = _fixture.Create<ParentAndChildViewModel>();
        var parentAndChildJson = JsonConvert.SerializeObject(parentAndChild);
        string invalidErrorsJson = "{invalid_json}";

        // Act
        var result = await _sut.Execute(parentAndChildJson, invalidErrorsJson);
        var (resultParentAndChild, resultErrors) = result;

        // Assert
        resultParentAndChild.Should().BeEquivalentTo(parentAndChild);
        resultErrors.Should().BeNull();
    }
}