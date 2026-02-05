using System.Diagnostics;
using FocusEnforcer.Core.Services;
using FocusEnforcer.Core.Models;

namespace FocusEnforcer.Service.Enforcers;

public class ProcessMonitor
{
    private readonly DatabaseService _db;
    private readonly ILogger<ProcessMonitor> _logger;
    private Timer? _timer;
    private HashSet<string> _blockedProcessNames = new();

    private HashSet<string> _allowedProcessNames = new();

    public ProcessMonitor(DatabaseService db, ILogger<ProcessMonitor> logger)
    {
        _db = db;
        _logger = logger;
    }

    public void Start()
    {
        try {
            File.AppendAllText(@"C:\ProgramData\FocusEnforcer\service_debug.log", $"[{DateTime.Now}] Service ProcessMonitor Starting...\n");
            RefreshRules();
             _timer = new Timer(CheckProcesses, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
        } catch (Exception ex) {
            File.AppendAllText(@"C:\ProgramData\FocusEnforcer\service_debug.log", $"[{DateTime.Now}] ERROR Start: {ex.Message}\n");
        }
    }

    public void RefreshRules()
    {
        try {
            var allRules = _db.GetActiveRules().Where(r => r.Type == BlockType.Process).ToList();
            
            var blocks = allRules.Where(r => r.Behavior == RuleBehavior.Block).Select(r => r.Value.ToLowerInvariant());
            var allows = allRules.Where(r => r.Behavior == RuleBehavior.Allow).Select(r => r.Value.ToLowerInvariant());

            _blockedProcessNames = new HashSet<string>(blocks);
            _allowedProcessNames = new HashSet<string>(allows);

            File.AppendAllText(@"C:\ProgramData\FocusEnforcer\service_debug.log", $"[{DateTime.Now}] Loaded {_blockedProcessNames.Count} blocks, {_allowedProcessNames.Count} allows.\n");
        } catch (Exception ex) {
             File.AppendAllText(@"C:\ProgramData\FocusEnforcer\service_debug.log", $"[{DateTime.Now}] ERROR RefreshRules: {ex.Message}\n");
        }
    }

    private void CheckProcesses(object? state)
    {
        // Re-fetch rules periodically or trigger via IPC
        RefreshRules(); 

        if (_blockedProcessNames.Count == 0 && _allowedProcessNames.Count == 0) return;

        var processes = Process.GetProcesses();
        foreach (var p in processes)
        {
            try
            {
                var pName = p.ProcessName.ToLowerInvariant();
                var pNameExe = pName + ".exe";

                // If explicitly allowed, skip
                if (_allowedProcessNames.Contains(pName) || _allowedProcessNames.Contains(pNameExe)) continue;

                // If explicitly blocked, kill
                if (_blockedProcessNames.Contains(pName) || _blockedProcessNames.Contains(pNameExe))
                {
                    _logger.LogWarning("Terminating blocked process: {name}", p.ProcessName);
                    p.Kill();
                }
            }
            catch (Exception ex)
            {
                // Ignore access denied for verification/system processes, log others
                // _logger.LogDebug("Failed to check process {name}: {msg}", p.ProcessName, ex.Message);
            }
        }
    }

    public void Stop()
    {
        _timer?.Change(Timeout.Infinite, 0);
    }
}
