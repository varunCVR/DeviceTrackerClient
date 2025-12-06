using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels
{
    public class InstalledApp
    {
        public string DisplayName { get; set; }
        public string ExePath { get; set; }
        public string Source { get; set; }
        public bool IsSystem { get; set; }
        public DateTime DiscoveredAt { get; set; }
    }

    public class Inventory
    {
        public List<InstalledApp> Apps { get; set; } = new List<InstalledApp>();
        public DateTime LastScanned { get; set; }
    }
}
