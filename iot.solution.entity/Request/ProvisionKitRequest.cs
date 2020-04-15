using System;
using System.Collections.Generic;

namespace iot.solution.entity
{
    public class ProvisionKitRequest
    {
        public string KitCode { get; set; }
        public Guid GeneratorGuid { get; set; }
      public string UniqueId { get; set; }
    }
}
