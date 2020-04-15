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
            response.Data = _chartService.GetGeneratorUsage(request);
            return response;
        }
        [HttpPost]
        [Route(ChartRoute.Route.EnergyUsage, Name = ChartRoute.Name.EnergyUsage)]
        public Entity.BaseResponse<List<Response.EnergyUsageResponse>> EnergyUsage(Request.ChartRequest request)
        {
            Entity.BaseResponse<List<Response.EnergyUsageResponse>> response = new Entity.BaseResponse<List<Response.EnergyUsageResponse>>(true);
            response.Data = _chartService.GetEnergyUsage(request);
            return response;
        }

        [HttpPost]
        [Route(ChartRoute.Route.EnergyGenerated, Name = ChartRoute.Name.EnergyGenerated)]
        public Entity.BaseResponse<List<Response.EnergyUsageResponse>> EnergyGenerated(Request.ChartRequest request)
        {
            Entity.BaseResponse<List<Response.EnergyUsageResponse>> response = new Entity.BaseResponse<List<Response.EnergyUsageResponse>>(true);
            try
            {
                response.Data = _chartService.GetEnergyGenerated(request);
            }
            catch (Exception ex)
            {
                return new Entity.BaseResponse<List<Response.EnergyUsageResponse>>(false, ex.Message);
            }
            return response;
        }

        [HttpPost]
        [Route(ChartRoute.Route.FuelUsage, Name = ChartRoute.Name.FuelUsage)]
        public Entity.BaseResponse<List<Response.FuelUsageResponse>> FuelUsage(Request.ChartRequest request)
        {
            Entity.BaseResponse<List<Response.FuelUsageResponse>> response = new Entity.BaseResponse<List<Response.FuelUsageResponse>>(true);
            try
            {
                response.Data = _chartService.GetFuelUsage(request);
            }
            catch (Exception ex)
            {
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
                response.Data = _chartService.GetGeneratorBatteryStatus(request);
            }
            catch (Exception ex)
            {
                return new Entity.BaseResponse<List<Response.GeneratorBatteryStatusResponse>>(false, ex.Message);
            }
            return response;

        }
    }
}