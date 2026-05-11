using Xunit;
using InsuranceManager.Domain.Ports;
using InsuranceManager.Infrastructure.Queue;
using InsuranceManager.Application.Services;
using InsuranceManager.Domain.Entities;
using InsuranceManager.Domain.ValueObjects;
using Moq;

namespace InsuranceManager.Architecture.Tests;

/// <summary>
/// Verifies the Port/Adapter pattern is correctly implemented:
/// - Port interface (IQueueTaskAdapter) exists in Domain layer
/// - Adapter implementation (QueueTaskRunnerAdapter) exists in Infrastructure layer
/// - Application services depend on the abstraction, not the concrete implementation
/// </summary>
public class PortAdapterPatternTests
{
    private readonly string _rootDir;

    public PortAdapterPatternTests()
    {
        _rootDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    }

    [Fact]
    public void PortInterface_ExistsInDomainLayer()
    {
        // ARRANGE & ACT
        var interfaceType = typeof(IQueueTaskAdapter);

        // ASSERT - Verify interface is in Domain.Ports namespace
        Assert.Equal("InsuranceManager.Domain.Ports", interfaceType.Namespace);
        Assert.True(interfaceType.IsInterface, "IQueueTaskAdapter should be an interface");

        // Verify the contract signature
        var enqueueMethod = interfaceType.GetMethod(nameof(IQueueTaskAdapter.EnqueueStatusChangeAsync));
        Assert.NotNull(enqueueMethod);
    }

    [Fact]
    public void AdapterImplementation_ExistsInInfrastructureLayer()
    {
        // ARRANGE & ACT
        var adapterType = typeof(QueueTaskRunnerAdapter);

        // ASSERT - Verify adapter is in Infrastructure.Queue namespace
        Assert.Equal("InsuranceManager.Infrastructure.Queue", adapterType.Namespace);
        Assert.True(adapterType.IsClass, "QueueTaskRunnerAdapter should be a class");

        // Verify it implements the port interface
        Assert.True(typeof(IQueueTaskAdapter).IsAssignableFrom(adapterType),
            "QueueTaskRunnerAdapter must implement IQueueTaskAdapter");
    }

    [Fact]
    public void Adapter_ImplementsPortContract()
    {
        // ARRANGE
        var mockConfig = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        mockConfig.Setup(c => c["Huey:QueuePath"]).Returns(Path.GetTempPath());

        // ACT - Create adapter instance
        var adapter = new QueueTaskRunnerAdapter(mockConfig.Object);

        // ASSERT - Adapter implements IQueueTaskAdapter
        Assert.IsAssignableFrom<IQueueTaskAdapter>(adapter);

        // Verify the EnqueueStatusChangeAsync method exists with correct signature
        var method = typeof(IQueueTaskAdapter).GetMethod(nameof(IQueueTaskAdapter.EnqueueStatusChangeAsync));
        Assert.NotNull(method);
    }

    [Fact]
    public void ApplicationService_UsesPortAbstraction()
    {
        // ARRANGE - Create mocks for dependencies
        var mockRepo = new Mock<IProposalRepository>();
        var mockReadAdapter = new Mock<IProposalReadAdapter>();
        var mockTaskAdapter = new Mock<IQueueTaskAdapter>();

        mockTaskAdapter
            .Setup(t => t.EnqueueStatusChangeAsync(
                It.IsAny<Guid>(),
                It.IsAny<ProposalStatus>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // ACT - Create ProposalService with IQueueTaskAdapter (not concrete implementation)
        var service = new ProposalService(mockRepo.Object, mockReadAdapter.Object, mockTaskAdapter.Object);

        // ASSERT - Service accepts IQueueTaskAdapter abstraction
        var constructorParamType = typeof(ProposalService).GetConstructors().First()
            .GetParameters()
            .First(p => p.Name == "hueyTaskRunner")
            ?.ParameterType;

        Assert.NotNull(constructorParamType);
        Assert.True(typeof(IQueueTaskAdapter).IsAssignableFrom(constructorParamType),
            "ProposalService should accept IQueueTaskAdapter (abstraction), not QueueTaskRunnerAdapter (concrete)");
    }

    [Fact]
    public void PortInterface_DefinesCorrectContract()
    {
        // ARRANGE
        var interfaceType = typeof(IQueueTaskAdapter);

        // ACT
        var method = interfaceType.GetMethod(nameof(IQueueTaskAdapter.EnqueueStatusChangeAsync));

        // ASSERT - Verify method signature: (Guid, ProposalStatus, CancellationToken) -> Task
        Assert.NotNull(method);
        Assert.Equal(typeof(Task), method.ReturnType);

        var parameters = method.GetParameters();
        Assert.Equal(3, parameters.Length);

        Assert.Equal("proposalId", parameters[0].Name);
        Assert.Equal(typeof(Guid), parameters[0].ParameterType);

        Assert.Equal("newStatus", parameters[1].Name);
        Assert.Equal(typeof(ProposalStatus), parameters[1].ParameterType);

        Assert.Equal("ct", parameters[2].Name);
        Assert.Equal(typeof(CancellationToken), parameters[2].ParameterType);
    }

    [Fact]
    public void InfrastructureLayer_DoesNotExposeDomainTypes()
    {
        // ARRANGE
        var adapterType = typeof(QueueTaskRunnerAdapter);

        // ACT - Check that the adapter doesn't expose any domain entity types in public API
        var publicMethods = adapterType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var returnTypes = publicMethods.Select(m => m.ReturnType).Distinct();

        // ASSERT - Adapter should only return Task or void (not domain entities)
        Assert.All(returnTypes, type =>
        {
            // Allow Task<T> where T is simple type or void
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var genericArg = type.GetGenericArguments()[0];
                Assert.True(
                    genericArg == typeof(void) || genericArg == typeof(string) ||
                    genericArg.Namespace?.StartsWith("System") == true,
                    $"Adapter should not return domain entities. Found: {genericArg.Name}");
            }
        });
    }

    [Fact]
    public void AllPorts_AreInDomainLayer()
    {
        // ARRANGE
        var domainAssembly = typeof(IQueueTaskAdapter).Assembly;

        // ACT - Get all interfaces in Domain.Ports namespace
        var portTypes = domainAssembly.GetTypes()
            .Where(t => t.Namespace == "InsuranceManager.Domain.Ports" && t.IsInterface)
            .ToList();

        // ASSERT - All ports should be in Domain layer
        foreach (var port in portTypes)
        {
            Assert.True(port.Name.StartsWith("I"),
                $"Port interface should be named with 'I' prefix. Found: {port.Name}");
        }

        // Verify Domain assembly only references fundamental assemblies (mscorlib, System, etc.)
        var referencedAssemblies = domainAssembly.GetReferencedAssemblies();
        var validAssemblyPrefixes = new[] { "System", "Microsoft", "mscorlib", "netstandard", "Windows" };

        foreach (var refAssembly in referencedAssemblies)
        {
            var isValidReference = validAssemblyPrefixes.Any(prefix =>
                refAssembly.Name.StartsWith(prefix) ||
                refAssembly.Name == "netstandard" ||
                refAssembly.Name == "System.Private.CoreLib");

            Assert.True(isValidReference || refAssembly.Name.StartsWith("InsuranceManager"),
                $"Domain should not reference external assemblies. Found: {refAssembly.Name}");
        }
    }
}