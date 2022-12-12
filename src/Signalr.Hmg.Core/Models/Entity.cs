﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Signalr.Hmg.Core.Models
{
    public class Entity
    {
        public string Name { get; set; }

        public ICollection<EntityProperty> Properties { get; set; }
    }
}
