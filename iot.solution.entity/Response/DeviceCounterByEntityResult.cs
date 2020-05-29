using System;
using System.Collections.Generic;
using System.Text;

namespace iot.solution.entity
{
    public class DeviceCounterByEntityResult
    {
        public Counters counters { get; set; }
    }

    public class Counters
    {
        public int active { get; set; }
        public int inActive { get; set; }
        public int connected { get; set; }
        public int disConnected { get; set; }
        public int acquired { get; set; }
        public int available { get; set; }
        public int total { get; set; }
    }
}
