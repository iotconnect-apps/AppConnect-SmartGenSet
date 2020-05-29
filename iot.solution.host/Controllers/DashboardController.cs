using iot.solution.entity.Structs.Routes;
using iot.solution.host.Filter;
using iot.solution.service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using Entity = iot.solution.entity;
using Response = iot.solution.entity.Response;

namespace host.iot.solution.Controllers
{
    [Route(DashboardRoute.Route.Global)]
    [ApiController]
    public class DashboardController : BaseController
    {
        private readonly IDashboardService _service;
        private readonly ILocationService _locationService;
        private readonly IGeneratorService _generatorService;

        public DashboardController(IDashboardService dashboardService, ILocationService locationService, IGeneratorService deviceService)
        {
            _service = dashboardService;
            _locationService = locationService;
            _generatorService = deviceService;
        }

        [HttpGet]
        [Route(DashboardRoute.Route.GetLocations, Name = DashboardRoute.Name.GetLocations)]
        [EnsureGuidParameterAttribute("companyId", "Dashboard")]
        public Entity.BaseResponse<List<Entity.LookupItem>> GetLocations(string companyId)
        {
            Entity.BaseResponse<List<Entity.LookupItem>> response = new Entity.BaseResponse<List<Entity.LookupItem>>(true);
            try
            {
                response.Data = _service.GetLocationLookup(Guid.Parse(companyId));
            }
            catch (Exception ex)
            {
                base.LogException(ex);
                return new Entity.BaseResponse<List<Entity.LookupItem>>(false, ex.Message);
            }
            return response;
        }

        [HttpGet]
        [Route(DashboardRoute.Route.GetOverview, Name = DashboardRoute.Name.GetOverview)]
        public Entity.BaseResponse<Entity.DashboardOverviewResponse> GetOverview(Guid companyId)
        {
            Entity.BaseResponse<Entity.DashboardOverviewResponse> response = new Entity.BaseResponse<Entity.DashboardOverviewResponse>(true);
            try
            {
                response = _service.GetOverview();
            }
            catch (Exception ex)
            {
                base.LogException(ex);
                return new Entity.BaseResponse<Entity.DashboardOverviewResponse>(false, ex.Message);
            }
            return response;
        }

        [HttpGet]
        [Route(DashboardRoute.Route.GetLocationDetail, Name = DashboardRoute.Name.GetLocationDetail)]
        [EnsureGuidParameterAttribute("locationId", "Dashboard")]
        public Entity.BaseResponse<Response.LocationDetailResponse> GetLocationDetail(string locationId)
        {
            if (locationId == null || Guid.Parse(locationId) == Guid.Empty)
            {
                return new Entity.BaseResponse<Response.LocationDetailResponse>(false, "Invalid Request");
            }

            Entity.BaseResponse<Response.LocationDetailResponse> response = new Entity.BaseResponse<Response.LocationDetailResponse>(true);
            try
            {
                response.Data = _locationService.GetLocationDetail(Guid.Parse(locationId));
            }
            catch (Exception ex)
            {
                base.LogException(ex);
                return new Entity.BaseResponse<Response.LocationDetailResponse>(false, ex.Message);
            }
            return response;
        }

        [HttpGet]
        [Route(DashboardRoute.Route.GetLocationDevices, Name = DashboardRoute.Name.GetLocationDevices)]
        [EnsureGuidParameterAttribute("locationId", "Dashboard")]
        public Entity.BaseResponse<List<Response.LocationWiseGeneratorResponse>> GetLocationDevices(string locationId)
        {
            if (locationId == null || Guid.Parse(locationId) == Guid.Empty)
            {
                return new Entity.BaseResponse<List<Response.LocationWiseGeneratorResponse>>(false, "Invalid Request");
            }

            Entity.BaseResponse<List<Response.LocationWiseGeneratorResponse>> response = new Entity.BaseResponse<List<Response.LocationWiseGeneratorResponse>>(true);
            try
            {
                response.Data = _generatorService.GetLocationWiseGenerators(Guid.Parse(locationId));
            }
            catch (Exception ex)
            {
                base.LogException(ex);
                return new Entity.BaseResponse<List<Response.LocationWiseGeneratorResponse>>(false, ex.Message);
            }
            return response;
        }

        [HttpGet]
        [Route(DashboardRoute.Route.GetLocationChildDevices, Name = DashboardRoute.Name.GetLocationChildDevices)]
        [EnsureGuidParameterAttribute("deviceId", "Dashboard")]
        public Entity.BaseResponse<List<Response.LocationWiseGeneratorResponse>> GetLocationChildDevices(string deviceId)
        {
            if (deviceId == null || Guid.Parse(deviceId) == Guid.Empty)
            {
                return new Entity.BaseResponse<List<Response.LocationWiseGeneratorResponse>>(false, "Invalid Request");
            }

            Entity.BaseResponse<List<Response.LocationWiseGeneratorResponse>> response = new Entity.BaseResponse<List<Response.LocationWiseGeneratorResponse>>(true);
            try
            {
                response.Data = _generatorService.GetLocationChildDevices(Guid.Parse(deviceId));
            }
            catch (Exception ex)
            {
                base.LogException(ex);
                return new Entity.BaseResponse<List<Response.LocationWiseGeneratorResponse>>(false, ex.Message);
            }
            return response;
        }

        [HttpGet]
        [Route(DashboardRoute.Route.GetGeneratorDetail, Name = DashboardRoute.Name.GetGeneratorDetail)]
        [EnsureGuidParameterAttribute("generatorId", "Dashboard")]
        public Entity.BaseResponse<Response.GeneratorDetailResponse> GetGeneratorDetail(string generatorId)
        {
            if (generatorId == null || Guid.Parse(generatorId) == Guid.Empty)
            {
                return new Entity.BaseResponse<Response.GeneratorDetailResponse>(false, "Invalid Request");
            }

            Entity.BaseResponse<Response.GeneratorDetailResponse> response = new Entity.BaseResponse<Response.GeneratorDetailResponse>(true);
            try
            {
                response = _generatorService.GetGeneratorDetail(Guid.Parse(generatorId));
            }
            catch (Exception ex)
            {
                base.LogException(ex);
                return new Entity.BaseResponse<Response.GeneratorDetailResponse>(false, ex.Message);
            }
            return response;
        }
    }
}