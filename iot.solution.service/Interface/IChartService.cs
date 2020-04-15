using System;
using System.Collections.Generic;
using Entity = iot.solution.entity;
using Request = iot.solution.entity.Request;
using Response = iot.solution.entity.Response;

namespace iot.solution.service.Interface
{
    public interface IChartService
    {
        List<Response.FuelUsageResponse> GetFuelUsage(Request.ChartRequest request);
        List<Response.EnergyUsageResponse> GetEnergyUsage(Request.ChartRequest request);
        List<Response.EnergyUsageResponse> GetEnergyGenerated(Request.ChartRequest request);
        List<Response.GeneratorUsageResponse> GetGeneratorUsage(Request.ChartRequest request);
        List<Response.GeneratorBatteryStatusResponse> GetGeneratorBatteryStatus(Request.ChartRequest request);
    }
}
