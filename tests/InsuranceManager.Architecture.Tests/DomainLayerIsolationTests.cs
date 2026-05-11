using Xunit;
using System.Xml.Linq;
using System.Linq;
using System.IO;

namespace InsuranceManager.Architecture.Tests;

/// <summary>
/// Verifies that the Domain layer has ZERO references to Infrastructure or Application layers.
/// This is a critical requirement for hexagonal architecture: the Domain (innermost hexagon) must not
/// depend on outer layers (Infrastructure, Application).
/// </summary>
public class DomainLayerIsolationTests
{
    private readonly string _rootDir;

    public DomainLayerIsolationTests()
    {
        // Navigate from tests/InsuranceManager.Architecture.Tests/ to repo root
        _rootDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    }

    /// <summary>
    /// Recursively gets all .cs files in a directory, excluding bin and obj directories.
    /// </summary>
    private static List<string> GetAllCsFilesRecursive(string directory)
    {
        var result = new List<string>();

        try
        {
            // Add files in current directory
            result.AddRange(Directory.GetFiles(directory, "*.cs"));

            // Recursively search subdirectories (excluding bin and obj)
            foreach (var subDir in Directory.GetDirectories(directory))
            {
                var dirName = Path.GetFileName(subDir);
                if (dirName != "bin" && dirName != "obj")
                {
                    result.AddRange(GetAllCsFilesRecursive(subDir));
                }
            }
        }
        catch (UnauthorizedAccessException) { /* Skip inaccessible directories */ }
        catch (DirectoryNotFoundException) { /* Skip missing directories */ }

        return result;
    }

    [Fact]
    public void DomainProject_HasNoProjectReferences()
    {
        // ARRANGE
        var domainCsprojPath = Path.Combine(_rootDir, "src", "InsuranceManager.Domain", "InsuranceManager.Domain.csproj");
        Assert.True(File.Exists(domainCsprojPath), $"Domain project file not found at: {domainCsprojPath}");

        // ACT - Parse csproj XML
        var doc = XDocument.Load(domainCsprojPath);
        var projectReferences = doc.Descendants("ProjectReference");

        // ASSERT
        var references = projectReferences.Select(r => r.Attribute("Include")?.Value ?? "").ToList();

        Assert.True(references.Count == 0,
            $"Domain project should have NO project references. Found: {string.Join(", ", references)}");
    }

    [Fact]
    public void DomainProject_HasNoExternalPackageReferences()
    {
        // ARRANGE
        var domainCsprojPath = Path.Combine(_rootDir, "src", "InsuranceManager.Domain", "InsuranceManager.Domain.csproj");

        // ACT - Parse csproj XML to get PackageReference items
        var doc = XDocument.Load(domainCsprojPath);
        var packageReferences = doc.Descendants("PackageReference");

        // ASSERT - Domain should have NO external package references (only its own assembly)
        var packages = packageReferences
            .Select(p => p.Attribute("Include")?.Value ?? "")
            .Where(p => !string.IsNullOrEmpty(p))
            .ToList();

        Assert.True(packages.Count == 0,
            $"Domain project should have NO external package references. Found: {string.Join(", ", packages)}");
    }

    [Fact]
    public void DomainProject_ContainsOnlyCoreFiles()
    {
        // ARRANGE
        var domainDir = Path.Combine(_rootDir, "src", "InsuranceManager.Domain");
        Assert.True(Directory.Exists(domainDir), $"Domain directory not found at: {domainDir}");

        // ACT - Get all .cs files in Domain project directories
        var domainFiles = GetAllCsFilesRecursive(domainDir)
            .Select(f => Path.GetRelativePath(domainDir, f))
            .ToList();

        // ASSERT - Verify all files are in expected domain directories
        var validPrefixes = new[] { "Entities", "Ports", "ValueObjects", "Events" };
        foreach (var file in domainFiles)
        {
            var directory = file.Split(Path.DirectorySeparatorChar)[0];
            Assert.True(
                validPrefixes.Contains(directory) || file.EndsWith("InsuranceManager.Domain.csproj"),
                $"Domain files should only be in Entities, Ports, ValueObjects, or Events directories. Found: {file}");
        }
    }

    [Fact]
    public void DomainFiles_HaveNoInfrastructureUsings()
    {
        // ARRANGE
        var domainDir = Path.Combine(_rootDir, "src", "InsuranceManager.Domain");

        // ACT - Get all .cs files in Domain directory
        var csFiles = GetAllCsFilesRecursive(domainDir);

        // ASSERT - No Domain file should reference Infrastructure or Application namespaces
        var filesWithViolations = new List<string>();

        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            if (content.Contains("InsuranceManager.Infrastructure") || content.Contains("InsuranceManager.Application"))
            {
                filesWithViolations.Add(Path.GetRelativePath(domainDir, file));
            }
        }

        Assert.True(filesWithViolations.Count == 0,
            $"Domain files should NOT reference Infrastructure or Application namespaces. Found violations in: {string.Join(", ", filesWithViolations)}");
    }

    [Fact]
    public void DomainFiles_HaveNoExternalInfrastructureDependencies()
    {
        // ARRANGE
        var domainDir = Path.Combine(_rootDir, "src", "InsuranceManager.Domain");

        // ACT - Get all .cs files in Domain directory
        var csFiles = GetAllCsFilesRecursive(domainDir);

        // ASSERT - Domain should only reference its own namespaces and fundamental .NET types
        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);

            // Check for external infrastructure references via using statements
            var hasInfraRef = content.Contains("using InsuranceManager.Infrastructure") ||
                             content.Contains("using InsuranceManager.Application") ||
                             content.Contains("using Microsoft.EntityFrameworkCore") ||
                             content.Contains("using Microsoft.Extensions");

            Assert.False(hasInfraRef,
                $"Domain file should not reference external infrastructure. File: {Path.GetRelativePath(domainDir, file)}");
        }
    }
}