using iot.solution.entity.Structs.Routes;
using iot.solution.service.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Entity = iot.solution.entity;

namespace host.iot.solution.Controllers
{
    [Route(GeneratorRoute.Route.Global)]
    public class GeneratorController : BaseController
    {
        private readonly IGeneratorService _service;

        public GeneratorController(IGeneratorService generatorService)
        {
            _service = generatorService;
        }

        [HttpGet]
        [Route(GeneratorRoute.Route.GetList, Name = GeneratorRoute.Name.GetList)]
        public Entity.BaseResponse<List<Entity.Generator>> Get()
        {
            Entity.BaseResponse<List<Entity.Generator>> response = new Entity.BaseResponse<List<Entity.Generator>>(true);
            try
            {
                response.Data = _service.Get();
            }
            catch (Exception ex)
            {
                return new Entity.BaseResponse<List<Entity.Generator>>(false, ex.Message);
            }
            return response;
        }

        [HttpGet]
        [Route(GeneratorRoute.Route.GetById, Name = GeneratorRoute.Name.GetById)]
        public Entity.BaseResponse<Entity.Generator> Get(Guid id)
        {
            Entity.BaseResponse<Entity.Generator> response = new Entity.BaseResponse<Entity.Generator>(true);
            try
            {
                response.Data = _service.Get(id);
            }
            catch (Exception ex)
            {
                return new Entity.BaseResponse<Entity.Generator>(false, ex.Message);
            }
            return response;
        }

        [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.OK)]
        [HttpPost]
        [Route(GeneratorRoute.Route.Manage, Name = GeneratorRoute.Name.Add)]
        public Entity.BaseResponse<Guid> Manage([FromForm]Entity.GeneratorModel request)
        {
            Entity.BaseResponse<Guid> response = new Entity.BaseResponse<Guid>(true);
            try
            {
                var status = _service.Manage(request);
                response.IsSuccess = status.Success;
                response.Message = status.Message;
                response.Data = status.Data;
            }
            catch (Exception ex)
            {
                return new Entity.BaseResponse<Guid>(false, ex.Message);
            }
            return response;
        }

        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        [HttpPost]
        [Route(GeneratorRoute.Route.FileUpload, Name = GeneratorRoute.Name.FileUpload)]
        public Entity.BaseResponse<bool> Upload(List<Microsoft.AspNetCore.Http.IFormFile> files, string generatorId)
        {
            Entity.BaseResponse<bool> response = new Entity.BaseResponse<bool>(true);
            try
            {
                if(files.Count > 0)
                {
                    Entity.ActionStatus status = _service.UploadFiles(files, generatorId);
                    
                    response.IsSuccess = status.Success;
                    response.Message = status.Message;
                    response.Data = status.Success; 
                }
            }
            catch (Exception ex)
            {
                return new Entity.BaseResponse<bool>(false, ex.Message);
            }
            return response;
        }

        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        [HttpPut]
        [Route(GeneratorRoute.Route.Delete, Name = GeneratorRoute.Name.Delete)]
        public Entity.BaseResponse<bool> Delete(Guid id)
        {
            Entity.BaseResponse<bool> response = new Entity.BaseResponse<bool>(true);
            try
            {
                var status = _service.Delete(id);
                response.IsSuccess = status.Success;
                response.Message = status.Message;
                response.Data = status.Success;
            }
            catch (Exception ex)
            {
                return new Entity.BaseResponse<bool>(false, ex.Message);
            }
            return response;
        }

        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        [HttpPut]
        [Route(GeneratorRoute.Route.DeleteMediaFile, Name = GeneratorRoute.Name.DeleteMediaFile)]
        public Entity.BaseResponse<bool> DeleteMediaFile(Guid generatorId, Guid? fileId)
        {
            Entity.BaseResponse<bool> response = new Entity.BaseResponse<bool>(true);
            try
            {
                var status = _service.DeleteMediaFile(generatorId,fileId);
                response.IsSuccess = status.Success;
                response.Message = status.Message;
                response.Data = status.Success;
            }
            catch (Exception ex)
            {
                return new Entity.BaseResponse<bool>(false, ex.Message);
            }
            return response;
        }
        [HttpGet]
        [Route(GeneratorRoute.Route.BySearch, Name = GeneratorRoute.Name.BySearch)]
        public Entity.BaseResponse<Entity.SearchResult<List<Entity.Generator>>> GetBySearch(string searchText = "", int? pageNo = 1, int? pageSize = 10, string orderBy = "")
        {
            Entity.BaseResponse<Entity.SearchResult<List<Entity.Generator>>> response = new Entity.BaseResponse<Entity.SearchResult<List<Entity.Generator>>>(true);
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
                return new Entity.BaseResponse<Entity.SearchResult<List<Entity.Generator>>>(false, ex.Message);
            }
            return response;
        }

        [HttpPost]
        [Route(GeneratorRoute.Route.UpdateStatus, Name = GeneratorRoute.Name.UpdateStatus)]
        public Entity.BaseResponse<bool> UpdateStatus(Guid id, bool status)
        {
            Entity.BaseResponse<bool> response = new Entity.BaseResponse<bool>(true);
            try
            {
                Entity.ActionStatus result = _service.UpdateStatus(id, status);
                response.IsSuccess = result.Success;
                response.Message = result.Message;
                response.Data = result.Success;
            }
            catch (Exception ex)
            {
                return new Entity.BaseResponse<bool>(false, ex.Message);
            }
            return response;
        }

        [HttpPost]
        [Route(GeneratorRoute.Route.AcquireDevice, Name = GeneratorRoute.Name.AcquireDevice)]
        public Entity.BaseResponse<bool> AcquireDevice(string deviceUniqueId)
        {
            Entity.BaseResponse<bool> response = new Entity.BaseResponse<bool>(true);
            try
            {
                Entity.ActionStatus result = _service.AcquireDevice(deviceUniqueId);
                response.IsSuccess = result.Success;
                response.Message = result.Message;
                response.Data = result.Success;
            }
            catch (Exception ex)
            {
                return new Entity.BaseResponse<bool>(false, ex.Message);
            }
            return response;
        }

        [HttpGet]
        [Route(GeneratorRoute.Route.ValidateKit, Name = GeneratorRoute.Name.ValidateKit)]
        public Entity.BaseResponse<int> ValidateKit(string kitCode)
        {
            Entity.BaseResponse<int> response = new Entity.BaseResponse<int>(true);
            try
            {
                response = _service.ValidateKit(kitCode);
            }
            catch (Exception ex)
            {
                return new Entity.BaseResponse<int>(false, ex.Message);
            }
            return response;
        }

        [HttpGet]
        [Route(GeneratorRoute.Route.DeviceCounters, Name = GeneratorRoute.Name.DeviceCounters)]
        public Entity.BaseResponse<Entity.DeviceCounterResult> DeviceCounters()
        {
            try
            {
                return _service.GetDeviceCounters();
            }
            catch (Exception ex)
            {
                return new Entity.BaseResponse<Entity.DeviceCounterResult>(false, ex.Message);
            }
        }

    }
}