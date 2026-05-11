using Microsoft.Extensions.Configuration;
using InsuranceManager.Domain.ValueObjects;

namespace InsuranceManager.Application.Huey;

public interface IHueyTaskRunner
{
    Task EnqueueStatusChangeAsync(Guid proposalId, ProposalStatus newStatus, CancellationToken ct = default);
}

public class HueyTaskRunner : IHueyTaskRunner
{
    private readonly string _hueyDir;

    public HueyTaskRunner(IConfiguration configuration)
    {
        _hueyDir = configuration["Huey:QueuePath"] ?? "/app/huey_data";
    }

    public Task EnqueueStatusChangeAsync(Guid proposalId, ProposalStatus newStatus, CancellationToken ct = default)
    {
        var statusStr = newStatus switch
        {
            ProposalStatus.Aprovada => "Aprovada",
            ProposalStatus.Recusada => "Recusada",
            _ => throw new ArgumentException($"Invalid status for enqueue: {newStatus}")
        };

        Console.WriteLine($"HueyTaskRunner.EnqueueStatusChangeAsync: proposalId={proposalId}, statusStr={statusStr}, hueyDir={_hueyDir}");

        Directory.CreateDirectory(_hueyDir);

        var taskFile = Path.Combine(_hueyDir, $"enqueue_{proposalId}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.json");
        var taskData = System.Text.Json.JsonSerializer.Serialize(new
        {
            task = "process_status_change",
            args = new[] { proposalId.ToString(), statusStr },
            enqueued = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });

        Console.WriteLine($"Writing task file: {taskFile}");
        File.WriteAllText(taskFile, taskData);
        Console.WriteLine($"Task file written successfully");

        return Task.CompletedTask;
    }
}