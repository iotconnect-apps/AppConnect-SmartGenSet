using System;
using System.Collections.Generic;
using System.Text;

namespace iot.solution.entity.Structs.Routes
{
    public class DashboardRoute
    {
        public struct Name
        {
            public const string GetLocations = "dashboard.getcompanylocation";
            public const string GetOverview = "dashboard.getoverview";
            public const string GetLocationDetail = "dashboard.getlocationdetail";
            public const string GetGeneratorDetail = "dashboard.getgeneratordetail";
            public const string GetLocationCorp = "dashboard.getlocationcorp";
            public const string GetLocationDevices = "dashboard.getlocationdevices";
            public const string GetLocationChildDevices = "dashboard.getlocationchilddevices";
        }
        public struct Route
        {
            public const string Global = "api/dashboard";
            public const string GetLocations = "getcompanylocation/{companyId}";
            public const string GetOverview = "overview/{companyId}";
            public const string GetLocationDetail = "getlocationdetail/{locationId}";
            public const string GetGeneratorDetail = "getgeneratordetail/{generatorId}";
            public const string GetLocationCorp = "getlocationcorp/{locationId}";
            public const string GetLocationDevices = "getlocationdevices/{locationId}";
            public const string GetLocationChildDevices = "getlocationchilddevices/{deviceId}";
        }
    }
}
