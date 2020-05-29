using iot.solution.entity.Structs.Routes;
using iot.solution.service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using Entity = iot.solution.entity;
using Response = iot.solution.entity.Response;
using Request = iot.solution.entity.Request;

namespace host.iot.solution.Controllers
{
    [Route(ChartRoute.Route.Global)]
    [ApiController]
    public class ChartController : BaseController
    {
        private readonly IChartService _chartService;

        public ChartController(IChartService chartService)
        {
            _chartService = chartService;
        }
        [HttpPost]
        [Route(ChartRoute.Route.GeneratorUsage, Name = ChartRoute.Name.GeneratorUsage)]
        public Entity.BaseResponse<List<Response.GeneratorUsageResponse>> GeneratorUsage(Request.ChartRequest request)
        {
            Entity.BaseResponse<List<Response.GeneratorUsageResponse>> response = new Entity.BaseResponse<List<Response.GeneratorUsageResponse>>(true);
            try
            {
                response = _chartService.GetGeneratorUsage(request);
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                base.LogException(ex);
                return new Entity.BaseResponse<List<Response.GeneratorUsageResponse>>(false, ex.Message);
            }
            return response;
        }

        [HttpPost]
        [Route(ChartRoute.Route.EnergyGenerated, Name = ChartRoute.Name.EnergyGenerated)]
        public Entity.BaseResponse<List<Response.EnergyUsageResponse>> GetEnergyGenerated(Request.ChartRequest request)
        {
            Entity.BaseResponse<List<Response.EnergyUsageResponse>> response = new Entity.BaseResponse<List<Response.EnergyUsageResponse>>(true);
            try
            {
                response = _chartService.GetEnergyGenerated(request);
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                base.LogException(ex);
                return new Entity.BaseResponse<List<Response.EnergyUsageResponse>>(false, ex.Message);
            }
            return response;
        }

        [HttpPost]
        [Route(ChartRoute.Route.FuelUsage, Name = ChartRoute.Name.FuelUsage)]
        public Entity.BaseResponse<List<Response.FuelUsageResponse>> GetFuelUsage(Request.ChartRequest request)
        {
            Entity.BaseResponse<List<Response.FuelUsageResponse>> response = new Entity.BaseResponse<List<Response.FuelUsageResponse>>(true);
            try
            {
                response = _chartService.GetFuelUsage(request);
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                base.LogException(ex);
                return new Entity.BaseResponse<List<Response.FuelUsageResponse>>(false, ex.Message);
            }
            return response;
        }

        [HttpPost]
        [Route(ChartRoute.Route.GeneratorBatteryStatus, Name = ChartRoute.Name.GeneratorBatteryStatus)]
        public Entity.BaseResponse<List<Response.GeneratorBatteryStatusResponse>> GeneratorBatteryStatus(Request.ChartRequest request)
        {
            Entity.BaseResponse<List<Response.GeneratorBatteryStatusResponse>> response = new Entity.BaseResponse<List<Response.GeneratorBatteryStatusResponse>>(true);
            try
            {
                response = _chartService.GetGeneratorBatteryStatus(request);
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                base.LogException(ex);
                return new Entity.BaseResponse<List<Response.GeneratorBatteryStatusResponse>>(false, ex.Message);
            }
            return response;

        }
    }
}