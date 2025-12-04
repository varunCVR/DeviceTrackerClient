using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTrackerClient.Core.Models
{
    public class ActivityLog
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string EventType { get; set; }
        public DateTime Timestamp { get; set; }
        public string Description { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
        public string ClientId { get; set; }
    }
}
