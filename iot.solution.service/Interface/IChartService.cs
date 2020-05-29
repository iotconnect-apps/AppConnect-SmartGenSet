using System;
using System.Collections.Generic;
using Entity = iot.solution.entity;
using Request = iot.solution.entity.Request;
using Response = iot.solution.entity.Response;

namespace iot.solution.service.Interface
{
    public interface IChartService
    {
        Entity.ActionStatus TelemetrySummary_DayWise();
        Entity.ActionStatus TelemetrySummary_HourWise();
        Entity.BaseResponse<List<Response.FuelUsageResponse>> GetFuelUsage(Request.ChartRequest request);
        List<Response.EnergyUsageResponse> GetEnergyUsage(Request.ChartRequest request);
        Entity.BaseResponse<List<Response.EnergyUsageResponse>> GetEnergyGenerated(Request.ChartRequest request);
        Entity.BaseResponse<List<Response.GeneratorUsageResponse>> GetGeneratorUsage(Request.ChartRequest request);
        Entity.BaseResponse<List<Response.GeneratorBatteryStatusResponse>> GetGeneratorBatteryStatus(Request.ChartRequest request);
    }
}
