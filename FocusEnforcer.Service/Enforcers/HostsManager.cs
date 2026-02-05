using FocusEnforcer.Core.Services;
using FocusEnforcer.Core.Models;
using System.IO;
using System.Text;

namespace FocusEnforcer.Service.Enforcers;

public class HostsManager
{
    private readonly DatabaseService _db;
    private readonly ILogger<HostsManager> _logger;
    private const string HostsPath = @"C:\Windows\System32\drivers\etc\hosts";
    private const string MarkersStart = "### FOCUS_ENFORCER_START ###";
    private const string MarkersEnd = "### FOCUS_ENFORCER_END ###";

    public HostsManager(DatabaseService db, ILogger<HostsManager> logger)
    {
        _db = db;
        _logger = logger;
    }

    public void ApplyRules()
    {
        try
        {
            var allRules = _db.GetActiveRules().Where(r => r.Type == BlockType.Website).ToList();
            var blockRules = allRules.Where(r => r.Behavior == RuleBehavior.Block).Select(r => r.Value.ToLowerInvariant()).ToHashSet();
            var allowRules = allRules.Where(r => r.Behavior == RuleBehavior.Allow).Select(r => r.Value.ToLowerInvariant()).ToHashSet();

            // Effective Block List = Blocked Items EXCEPT Allowed Items
            var rules = blockRules.Except(allowRules).ToList();
            
            if (!File.Exists(HostsPath)) return;

            var lines = File.ReadAllLines(HostsPath).ToList();
            var newLines = new List<string>();
            bool inBlock = false;

            // Retain original content
            foreach (var line in lines)
            {
                if (line.Trim() == MarkersStart) { inBlock = true; continue; }
                if (line.Trim() == MarkersEnd) { inBlock = false; continue; }
                if (!inBlock) newLines.Add(line);
            }

            // Append our block
            if (rules.Any())
            {
                newLines.Add(MarkersStart);
                foreach (var site in rules)
                {
                    newLines.Add($"127.0.0.1 {site}");
                    newLines.Add($"127.0.0.1 www.{site}");
                }
                newLines.Add(MarkersEnd);
            }

            File.WriteAllLines(HostsPath, newLines);
            File.AppendAllText(@"C:\ProgramData\FocusEnforcer\service_debug.log", $"[{DateTime.Now}] Hosts updated. Rules: {rules.Count}\n");
            _logger.LogInformation("Hosts file updated. Rules count: {count}", rules.Count);
        }
        catch (Exception ex)
        {
            File.AppendAllText(@"C:\ProgramData\FocusEnforcer\service_debug.log", $"[{DateTime.Now}] ERROR Hosts: {ex.Message}\n");
            _logger.LogError(ex, "Failed to update hosts file");
        }
    }
}
