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
    private readonly string _pythonPath;

    public HueyTaskRunner(IConfiguration configuration)
    {
        _hueyDir = configuration["Huey:QueuePath"] ?? "./huey_data";
        _pythonPath = configuration["Huey:PythonPath"] ?? "python";
    }

    public async Task EnqueueStatusChangeAsync(Guid proposalId, ProposalStatus newStatus, CancellationToken ct = default)
    {
        var statusStr = newStatus switch
        {
            ProposalStatus.Aprovada => "Aprovada",
            ProposalStatus.Recusada => "Recusada",
            _ => throw new ArgumentException($"Invalid status for enqueue: {newStatus}")
        };

        // Ensure huey data directory exists
        Directory.CreateDirectory(_hueyDir);

        var pythonCode = $@"
import sys
import os
sys.path.insert(0, os.path.join(os.path.dirname(__file__), 'Application', 'Huey'))
from huey_config import huey

@huey.task()
def process_status_change(proposal_id, new_status):
    pass

huey.enqueue(process_status_change('{proposalId}', '{statusStr}'))
";

        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = _pythonPath,
            Arguments = $"-c \"{pythonCode.Replace("\"", "\\\"")}\"",
            WorkingDirectory = Directory.GetCurrentDirectory(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = System.Diagnostics.Process.Start(psi);
        if (process != null)
        {
            await process.WaitForExitAsync(ct);
            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync(ct);
                throw new InvalidOperationException($"Huey enqueue failed: {error}");
            }
        }
    }
}