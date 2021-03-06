﻿using iot.solution.entity.Structs.Routes;
using iot.solution.host.Filter;
using iot.solution.service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using Entity = iot.solution.entity;

namespace host.iot.solution.Controllers
{
    [Route(LocationRoute.Route.Global)]
    [ApiController]
    public class LocationController : BaseController
    {
        private readonly ILocationService _service;
        private readonly IGeneratorService _generatorService;

        public LocationController(ILocationService locationService, IGeneratorService generatorService)
        {
            _service = locationService;
            _generatorService = generatorService;
        }

        [HttpGet]
        [Route(LocationRoute.Route.GetList, Name = LocationRoute.Name.GetList)]
        public Entity.BaseResponse<List<Entity.LocationWithCounts>> Get()
        {
            Entity.BaseResponse<List<Entity.LocationWithCounts>> response = new Entity.BaseResponse<List<Entity.LocationWithCounts>>(true);
            try
            {
                response.Data = _service.Get();

                foreach (var item in response.Data)
                {
                    item.TotalGenerators = 11;
                    //item.TotalUsers = 15;
                }
            }
            catch (Exception ex)
            {
                return new Entity.BaseResponse<List<Entity.LocationWithCounts>>(false, ex.Message);
            }
            return response;
        }

        [HttpGet]
        [Route(LocationRoute.Route.GetById, Name = LocationRoute.Name.GetById)]
        [EnsureGuidParameterAttribute("id", "Location")]
        public Entity.BaseResponse<Entity.Location> Get(string id)
        {
            if (id == null || Guid.Parse(id) == Guid.Empty)
            {
                return new Entity.BaseResponse<Entity.Location>(false, "Invalid Request");
            }

            Entity.BaseResponse<Entity.Location> response = new Entity.BaseResponse<Entity.Location>(true);
            try
            {
                response.Data = _service.Get(Guid.Parse(id));

                //response.Data.TotalDisconnectedGenerators = 1;
                //response.Data.TotalEneryGenerated = 3000;
                //response.Data.TotalFuelUsed = 30;
                //response.Data.TotalGenerators = _generatorService.Get()?;
                //response.Data.TotalOffGenerators = 1;
                //response.Data.TotalOnConnectedGenerators = 3;
                response.Data.Generators = _generatorService.GetLocationWiseGenerators(Guid.Parse(id));
                
            }
            catch (Exception ex)
            {
                base.LogException(ex);
                return new Entity.BaseResponse<Entity.Location>(false, ex.Message);
            }
            return response;
        }

        [HttpGet]
        [Route(LocationRoute.Route.GetLocationStatics, Name = LocationRoute.Name.GetLocationStatics)]
        [EnsureGuidParameterAttribute("locationId", "Location")]
        public Entity.BaseResponse<Entity.LocationStaticsResponse> GetLocationStatics(string locationId)
        {
            if (locationId == null || Guid.Parse(locationId) == Guid.Empty)
            {
                return new Entity.BaseResponse<Entity.LocationStaticsResponse>(false, "Invalid Request");
            }

            Entity.BaseResponse<Entity.LocationStaticsResponse> response = new Entity.BaseResponse<Entity.LocationStaticsResponse>(true);
            try
            {
               
                response = _service.GetLocationStatics(Guid.Parse(locationId));

            }
            catch (Exception ex)
            {
                base.LogException(ex);
                return new Entity.BaseResponse<Entity.LocationStaticsResponse>(false, ex.Message);
            }
            return response;
        }

        [HttpPost]
        [Route(LocationRoute.Route.Manage, Name = LocationRoute.Name.Add)]
        public Entity.BaseResponse<Guid> Manage([FromBody]Entity.AddLocationRequest request)
        {

            Entity.BaseResponse<Guid> response = new Entity.BaseResponse<Guid>(false);
            try
            {

                var status = _service.Manage(request);
                if(status.Success)
                {
                    response.IsSuccess = status.Success;
                    response.Message = status.Message;
                    response.Data = status.Data;
                }
                else
                {
                    response.IsSuccess = status.Success;
                    response.Message = status.Message;
                }
               
            }
            catch (Exception ex)
            {
                base.LogException(ex);
                return new Entity.BaseResponse<Guid>(false, ex.Message);
            }
            return response;
        }

        [HttpPut]
        [Route(LocationRoute.Route.Delete, Name = LocationRoute.Name.Delete)]
        [EnsureGuidParameterAttribute("id", "Location")]
        public Entity.BaseResponse<bool> Delete(string id)
        {
            if (id == null || Guid.Parse(id) == Guid.Empty)
            {
                return new Entity.BaseResponse<bool>(false, "Invalid Request");
            }

            Entity.BaseResponse<bool> response = new Entity.BaseResponse<bool>(true);
            try
            {
                var status = _service.Delete(Guid.Parse(id));
                response.IsSuccess = status.Success;
                response.Message = status.Message;
                response.Data = status.Success;
            }
            catch (Exception ex)
            {
                base.LogException(ex);
                return new Entity.BaseResponse<bool>(false, ex.Message);
            }
            return response;
        }

        [HttpGet]
        [Route(LocationRoute.Route.BySearch, Name = LocationRoute.Name.BySearch)]
        public Entity.BaseResponse<Entity.SearchResult<List<Entity.LocationWithCounts>>> GetBySearch(string searchText = "", int? pageNo = 1, int? pageSize = 10, string orderBy = "")
        {
            Entity.BaseResponse<Entity.SearchResult<List<Entity.LocationWithCounts>>> response = new Entity.BaseResponse<Entity.SearchResult<List<Entity.LocationWithCounts>>>(true);
            try
            {
                response.Data = _service.List(new Entity.SearchRequest()
                {
                    SearchText = searchText,
                    PageNumber = pageNo.Value,
                    PageSize = pageSize.Value,
                    OrderBy = orderBy
                });
            }
            catch (Exception ex)
            {
                base.LogException(ex);
                return new Entity.BaseResponse<Entity.SearchResult<List<Entity.LocationWithCounts>>>(false, ex.Message);
            }
            return response;
        }

        [HttpPost]
        [Route(LocationRoute.Route.UpdateStatus, Name = LocationRoute.Name.UpdateStatus)]
        [EnsureGuidParameterAttribute("id", "Location")]
        public Entity.BaseResponse<bool> UpdateStatus(string id, bool status)
        {
            Entity.BaseResponse<bool> response = new Entity.BaseResponse<bool>(true);
            try
            {
                Entity.ActionStatus result = _service.UpdateStatus(Guid.Parse(id), status);
                response.IsSuccess = result.Success;
                response.Message = result.Message;
                response.Data = result.Success;
            }
            catch (Exception ex)
            {
                base.LogException(ex);
                return new Entity.BaseResponse<bool>(false, ex.Message);
            }
            return response;
        }
       
    }
}