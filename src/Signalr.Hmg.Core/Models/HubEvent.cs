using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Signalr.Hmg.Core.Models
{
    public class HubEvent
    {
        public string HubName { get; set; }

        public string Name { get; set; }

        public ICollection<HubEventArgument> Arguments { get; set; }
    }
}
