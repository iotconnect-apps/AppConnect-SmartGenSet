﻿using System;
using System.Collections.Generic;

namespace iot.solution.entity.Response
{
    public class GeneratorUsageResponse
    {
        public string UniqueId { get; set; }
        public string Value { get; set; }
    }
    public class EnergyUsageResponse
    {
        public string Month { get; set; }
        public string Value { get; set; }
    }
    public class FuelUsageResponse
    {
        public string Month { get; set; }
        public string Value { get; set; }
    }

    public class GeneratorBatteryStatusResponse
    {
        public string Month { get; set; }
        public string Value { get; set; }
   
    }

    public class ConfgurationResponse
    {
        public string cpId { get; set; }
        public string host { get; set; }
        public int isSecure { get; set; }
        public string password { get; set; }
        public int port { get; set; }
        public string url { get; set; }
        public string user { get; set; }
        public string vhost { get; set; }
    }
}
