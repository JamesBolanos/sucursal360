using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Sucursal360.Web.Domain.Enums;
using Sucursal360.Web.Integrations.Abstractions;
using Sucursal360.Web.Integrations.Demo;

namespace Sucursal360.Tests;

[TestClass]
public sealed class DemoPublicBranchDataProviderTests
{
    [TestMethod]
    public async Task ValidFixtureMapsToCanonicalData()
    {
        var provider = CreateProvider();

        var data = await provider.GetBranchAsync("DEMO-SUC-001", CancellationToken.None);

        Assert.AreEqual(PublicDataProvider.Demo, data.Provider);
        Assert.AreEqual("DEMO-SUC-001", data.ExternalPlaceId);
        Assert.AreEqual("Cafe Horizonte Centro", data.DisplayName);
        Assert.AreEqual(4.3m, data.Rating);
        Assert.AreEqual(128, data.ReviewCount);
        Assert.HasCount(3, data.Reviews);
        Assert.AreEqual(BusinessStatus.Operational, data.BusinessStatus);
    }

    [TestMethod]
    public async Task PartialFixtureNormalizesInvalidOptionalValues()
    {
        var provider = CreateProvider();

        var data = await provider.GetBranchAsync("PARTIAL-SUC", CancellationToken.None);

        Assert.IsNull(data.Rating);
        Assert.IsNull(data.ReviewCount);
        Assert.IsNull(data.Latitude);
        Assert.IsNull(data.Longitude);
        Assert.HasCount(1, data.Reviews);
        Assert.IsNull(data.Reviews[0].Rating);
    }

    [TestMethod]
    public async Task InvalidSchemaThrowsProviderException()
    {
        var provider = CreateProvider();

        try
        {
            await provider.GetBranchAsync("INVALID-SCHEMA", CancellationToken.None);
            Assert.Fail("Expected invalid schema to throw.");
        }
        catch (PublicDataProviderException exception)
        {
            Assert.AreEqual("INT-422-SCHEMA", exception.ErrorCode);
        }
    }

    private static DemoPublicBranchDataProvider CreateProvider()
    {
        var environment = new TestWebHostEnvironment
        {
            ContentRootPath = Path.Combine(AppContext.BaseDirectory, "ProviderContentRoot", Guid.NewGuid().ToString("N"))
        };

        Directory.CreateDirectory(environment.ContentRootPath);
        CopyFixtures(environment.ContentRootPath);

        return new DemoPublicBranchDataProvider(
            environment,
            Options.Create(new DemoPublicDataOptions()));
    }

    private static void CopyFixtures(string contentRootPath)
    {
        var sourceRoot = FindRepoRoot();
        var sourceFixtures = Path.Combine(sourceRoot, "src", "Sucursal360.Web", "Integrations", "Demo", "Fixtures");
        var targetFixtures = Path.Combine(contentRootPath, "Integrations", "Demo", "Fixtures");

        Directory.CreateDirectory(targetFixtures);
        foreach (var fixture in Directory.GetFiles(sourceFixtures, "*.json"))
        {
            File.Copy(fixture, Path.Combine(targetFixtures, Path.GetFileName(fixture)), overwrite: true);
        }
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Sucursal360.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";

        public string ApplicationName { get; set; } = "Sucursal360.Tests";

        public string WebRootPath { get; set; } = string.Empty;

        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();

        public string ContentRootPath { get; set; } = string.Empty;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
