using System;
using System.Collections.Generic;
using System.Text;

namespace iot.solution.entity.Structs.Routes
{
    public class ChartRoute
    {
        public struct Name
        {
            public const string GeneratorUsage = "chart.generatorusage";
            public const string EnergyUsage = "chart.energyusage";
            public const string FuelUsage = "chart.fuelusage";
            public const string GeneratorBatteryStatus = "chart.generatorbatterystatus";
            public const string EnergyGenerated = "chart.energygenerated";
        }

        public struct Route
        {
            public const string Global = "api/chart";
            public const string GeneratorUsage = "getgeneratorusage";
            public const string EnergyUsage = "getenergyusage";
            public const string FuelUsage = "getfuelusage";
            public const string GeneratorBatteryStatus = "getgeneratorbatterystatus";
            public const string EnergyGenerated = "getenergygenerated";
        }
    }
}
