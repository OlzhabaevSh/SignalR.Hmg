using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Signalr.Hmg.Core.Models
{
    public class HubMethod
    {
        public string HubName { get; set; }

        public string Name { get; set; }

        public ICollection<HubMethodArgument> Arguments { get; set; }
    }
}
