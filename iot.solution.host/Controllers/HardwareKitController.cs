﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using host.iot.solution.Controllers;
using iot.solution.entity.Structs.Routes;
using iot.solution.service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Entity = iot.solution.entity;
using Model = iot.solution.model.Models;

namespace iot.solution.host.Controllers
{
    [Route(HardwareKitRoute.Route.Global)]
    public class HardwareKitController : BaseController
    {
        private readonly IHardwareKitService _service;

        public HardwareKitController(IHardwareKitService hardwareKitService)
        {
            _service = hardwareKitService;
        }

        /// <summary>
        /// Search Across Hardware Kits
        /// </summary>
        /// <param name="isAssigned">true or false</param>
        /// <param name="searchText">text to be searched</param>
        /// <param name="pageNo">page no</param>
        /// <param name="pageSize">page size</param>
        /// <param name="orderBy">Order by clause</param>
        /// <returns></returns>
        [HttpGet]
        [Route(HardwareKitRoute.Route.BySearch, Name = HardwareKitRoute.Name.BySearch)]
        public Entity.BaseResponse<Entity.SearchResult<List<Entity.HardwareKitResponse>>> GetBySearch(bool isAssigned, string searchText = "", int? pageNo = 1, int? pageSize = 10, string orderBy = "")
        {
            Entity.BaseResponse<Entity.SearchResult<List<Entity.HardwareKitResponse>>> response = new Entity.BaseResponse<Entity.SearchResult<List<Entity.HardwareKitResponse>>>(true);
            try
            {
                response.Data = _service.List(
                    new Entity.SearchRequest()
                    {
                        SearchText = searchText,
                        PageNumber = pageNo.Value,
                        PageSize = pageSize.Value,
                        OrderBy = orderBy
                    }, isAssigned);
            }
            catch (Exception ex)
            {
                return new Entity.BaseResponse<Entity.SearchResult<List<Entity.HardwareKitResponse>>>(false, ex.Message);
            }
            return response;
        }

        /// <summary>
        /// Get Hardware Kit details 
        /// </summary>
        /// <param name="id">Guid of Hardware Kit</param>
        /// <returns></returns>
        [HttpGet]
        [Route(HardwareKitRoute.Route.GetById, Name = HardwareKitRoute.Name.GetById)]
        public Entity.BaseResponse<Entity.HardwareKitDTO> Get(Guid id)
        {
            Entity.BaseResponse<Entity.HardwareKitDTO> response = new Entity.BaseResponse<Entity.HardwareKitDTO>(true);
            try
            {
                response.Data = _service.Get(id);
            }
            catch (Exception ex)
            {
                return new Entity.BaseResponse<Entity.HardwareKitDTO>(false, ex.Message);
            }
            return response;
        }

        /// <summary>
        /// Add/Update Hardware Kit
        /// </summary>
        /// <param name="request">Request contains Hardware Kit Details</param>
        /// <param name="isEdit"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(HardwareKitRoute.Route.Manage, Name = HardwareKitRoute.Name.Manage)]
        public Entity.BaseResponse<List<Entity.BulkUploadResponse>> Manage([FromBody]Entity.KitVerifyRequest request, bool isEdit = false)
        {
            Entity.BaseResponse<List<Entity.BulkUploadResponse>> response = new Entity.BaseResponse<List<Entity.BulkUploadResponse>>(true);
            try
            {    
                var status = _service.Manage(request, isEdit);
                response.IsSuccess = status.Success;
                response.Message = status.Message;
                response.Data = status.Data;
            }
            catch (Exception ex)
            {
                return new Entity.BaseResponse<List<Entity.BulkUploadResponse>>(false, ex.Message);
            }
            return response;
        }

        /// <summary>
        /// Delete Hardware Kit
        /// </summary>
        /// <param name="id">Guid of Hardware Kit</param>
        /// <returns></returns>
        [HttpPut]
        [Route(HardwareKitRoute.Route.Delete, Name = HardwareKitRoute.Name.Delete)]
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


        /// <summary>
        /// Download Sample File For Bulk Upload
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(HardwareKitRoute.Route.DownloadSampleJson, Name = HardwareKitRoute.Name.DownloadSampleJson)]
        public string GetJsonSampleFile()
        {
            var result = string.Empty;
            try
            {
                List<Entity.HardwareKitRequest> list = new List<Entity.HardwareKitRequest>();
                Entity.HardwareKitRequest kitObj = new Entity.HardwareKitRequest();
                kitObj.KitCode = "KIT CODE";
                kitObj.Name = "KIT NAME";
                kitObj.Note = "KIT NOTE";
                //kitObj.Tag = "KIT TAG";
                //kitObj.AttributeName = "DEVICE ATTRIBUTE NAME";
                kitObj.UniqueId = "KIT UNIQUEID";
                list.Add(kitObj);
                result = JsonConvert.SerializeObject(list);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        /// <summary>
        /// Verify Kit list for Bulk Upload
        /// </summary>
        /// <param name="request">request contains H/W Kit Details</param>
        /// <returns></returns>
        [HttpPost]
        [Route(HardwareKitRoute.Route.VerifyKit, Name = HardwareKitRoute.Name.VerifyKit)]
        public Entity.BaseResponse<List<Entity.BulkUploadResponse>> VerifyKit(Entity.KitVerifyRequest request)
        {
            Entity.BaseResponse<List<Entity.BulkUploadResponse>> response = new Entity.BaseResponse<List<Entity.BulkUploadResponse>>(true);
            try
            {
                List<Entity.KitVerifyResponse> lstTestData = new List<Entity.KitVerifyResponse>();
                //TODO

                var result = _service.VerifyKit(request);

                if (result.Success)
                    response.IsSuccess = true;
                else
                {
                    response.IsSuccess = false;
                    response.Message = result.Message;
                }

                response.Data = result.Data;

            }
            catch (Exception ex)
            {
                return new Entity.BaseResponse<List<Entity.BulkUploadResponse>>(false, ex.Message);
            }
            return response;
        }

        /// <summary>
        /// Bulk Upload Kits after Verify
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route(HardwareKitRoute.Route.UploadKit, Name = HardwareKitRoute.Name.UploadKit)]
        public Entity.BaseResponse<List<Entity.BulkUploadResponse>> UploadKit(Entity.KitVerifyRequest request)
        {
            Entity.BaseResponse<List<Entity.BulkUploadResponse>> response = new Entity.BaseResponse<List<Entity.BulkUploadResponse>>();
            Entity.ActionStatus result = new Entity.ActionStatus();
            try
            {
                result = _service.UploadKit(request);
                if (result.Success)
                    response.IsSuccess = true;
                else
                {
                    response.IsSuccess = false;
                    response.Message = result.Message;
                }

                response.Data = result.Data;
            }
            catch (Exception ex)
            {
                return new Entity.BaseResponse<List<Entity.BulkUploadResponse>>(false, ex.Message);
            }
            return response;
        }
    }
}