using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Signalr.Hmg.Core.Models
{
    public class SignalrMetadata
    {
        public HubMethod[] Methods { get; set; }

        public HubEvent[] Events { get; set; }

        public Entity[] Entities { get; set; }
    }
}
