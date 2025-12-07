using System;
using System.Collections.Generic;

namespace SharedModels
{
    public class BlockedApp
    {
        public string ProcessName { get; set; }
        public string ExePath { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedAt { get; set; }

        public override bool Equals(object obj)
        {
            return obj is BlockedApp other &&
                   string.Equals(ProcessName, other.ProcessName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ExePath, other.ExePath, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(ProcessName) ^
                   StringComparer.OrdinalIgnoreCase.GetHashCode(ExePath);
        }
    }

    public class BlockList
    {
        public List<BlockedApp> BlockedProcesses { get; set; } = new List<BlockedApp>();
    }
}