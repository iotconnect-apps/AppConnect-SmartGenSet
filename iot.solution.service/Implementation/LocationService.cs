﻿using component.helper;
using component.logger;
using iot.solution.common;
using iot.solution.model.Repository.Interface;
using iot.solution.service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using Entity = iot.solution.entity;
using IOT = IoTConnect.Model;
using Model = iot.solution.model.Models;
using Response = iot.solution.entity.Response;
using LogHandler = component.services.loghandler;
using System.Reflection;

namespace iot.solution.service.Implementation
{
    public class LocationService : ILocationService
    {
        private readonly ILocationRepository _locationRepository;
        private readonly IotConnectClient _iotConnectClient;
        private readonly LogHandler.Logger _logger;
        private readonly IGeneratorRepository _generatorRepository;
        private readonly IGeneratorService _generatorService;
        public LocationService(ILocationRepository locationRepository, LogHandler.Logger logger, IGeneratorRepository generatorRepository, IGeneratorService generatorService)
        {
            _logger = logger;
            _locationRepository = locationRepository;
            _generatorRepository = generatorRepository;
            _iotConnectClient = new IotConnectClient(component.helper.SolutionConfiguration.BearerToken, component.helper.SolutionConfiguration.Configuration.EnvironmentCode, component.helper.SolutionConfiguration.Configuration.SolutionKey);
            _generatorService = generatorService;
        }
        public List<Entity.LocationWithCounts> Get()
        {
            try
            {
                return _locationRepository.GetAll().Where(e => !e.IsDeleted).Select(p => Mapper.Configuration.Mapper.Map<Entity.LocationWithCounts>(p)).ToList();
            }
            catch (Exception ex)
            {

                _logger.ErrorLog(ex, this.GetType().Name, MethodBase.GetCurrentMethod().Name);
                return new List<Entity.LocationWithCounts>();
            }
        }
        public Entity.Location Get(Guid id)
        {
            var result = new Entity.Location();
            try
            {
                result = _locationRepository.FindBy(r => r.Guid == id).Select(p => Mapper.Configuration.Mapper.Map<Entity.Location>(p)).FirstOrDefault();

            }
            catch (Exception ex)
            {
                _logger.ErrorLog(ex, this.GetType().Name, MethodBase.GetCurrentMethod().Name);
                return null;
            }
            return result;
        }
        public Entity.ActionStatus Manage(Entity.AddLocationRequest request)
        {
            Entity.ActionStatus actionStatus = new Entity.ActionStatus(true);
            try
            {
                if (request.Guid == null || request.Guid == Guid.Empty)
                {
                    var checkExisting = _locationRepository.FindBy(x => x.Name.Equals(request.Name) && x.IsActive == true && !x.IsDeleted && x.CompanyGuid.Equals(SolutionConfiguration.CompanyId)).FirstOrDefault();
                    if (checkExisting == null)
                    {
                        var addEntityResult = AsyncHelpers.RunSync<IOT.DataResponse<IOT.AddEntityResult>>(() =>
                       _iotConnectClient.Entity.Add(Mapper.Configuration.Mapper.Map<IOT.AddEntityModel>(request)));

                        if (addEntityResult != null && addEntityResult.status && addEntityResult.data != null)
                        {
                            request.Guid = Guid.Parse(addEntityResult.data.EntityGuid.ToUpper());
                            var dbLocation = Mapper.Configuration.Mapper.Map<Entity.AddLocationRequest, Model.Location>(request);
                            dbLocation.Guid = request.Guid;
                            dbLocation.CompanyGuid = component.helper.SolutionConfiguration.CompanyId;
                            dbLocation.CreatedDate = DateTime.Now;
                            dbLocation.CreatedBy = component.helper.SolutionConfiguration.CurrentUserId;
                            actionStatus = _locationRepository.Manage(dbLocation);
                            actionStatus.Data = (Guid)(actionStatus.Data);
                            if (!actionStatus.Success)
                            {
                                _logger.ErrorLog(new Exception($"Location is not added in solution database, Error: {actionStatus.Message}"), this.GetType().Name, MethodBase.GetCurrentMethod().Name);
                                var deleteEntityResult = _iotConnectClient.Entity.Delete(request.Guid.ToString()).Result;
                                if (deleteEntityResult != null && deleteEntityResult.status)
                                {
                                    _logger.ErrorLog(new Exception($"Location is not deleted from iotconnect, Error: {deleteEntityResult.message}"), this.GetType().Name, MethodBase.GetCurrentMethod().Name);
                                    actionStatus.Success = false;
                                    actionStatus.Message = new UtilityHelper().IOTResultMessage(deleteEntityResult.errorMessages);
                                }
                            }
                        }
                        else
                        {
                            _logger.ErrorLog(new Exception($"Location is not added in iotconnect, Error: {addEntityResult.message}"), this.GetType().Name, MethodBase.GetCurrentMethod().Name);
                            actionStatus.Success = false;
                            actionStatus.Message = new UtilityHelper().IOTResultMessage(addEntityResult.errorMessages);
                        }
                    }
                    else
                    {
                        _logger.InfoLog($"Location Already Exist !!");
                        actionStatus.Success = false;
                        actionStatus.Message = "Location Name Already Exists";
                    }
                }
                else
                {
                    var olddbLocation = _locationRepository.FindBy(x => x.Guid.Equals(request.Guid)).FirstOrDefault();
                    if (olddbLocation == null)
                    {
                        throw new NotFoundCustomException($"{CommonException.Name.NoRecordsFound} : Location");
                    }

                    var updateEntityResult = _iotConnectClient.Entity.Update(request.Guid.ToString(), Mapper.Configuration.Mapper.Map<IOT.UpdateEntityModel>(request)).Result;
                    if (updateEntityResult != null && updateEntityResult.status && updateEntityResult.data != null)
                    {
                        var dbLocation = Mapper.Configuration.Mapper.Map(request, olddbLocation);
                        dbLocation.UpdatedDate = DateTime.Now;
                        dbLocation.UpdatedBy = component.helper.SolutionConfiguration.CurrentUserId;
                        dbLocation.CompanyGuid = component.helper.SolutionConfiguration.CompanyId;
                        actionStatus = _locationRepository.Manage(dbLocation);
                        actionStatus.Data = (Guid)actionStatus.Data;
                        if (!actionStatus.Success)
                        {
                            _logger.ErrorLog(new Exception($"Location is not updated in solution database, Error: {actionStatus.Message}"), this.GetType().Name, MethodBase.GetCurrentMethod().Name);
                            actionStatus.Success = false;
                            actionStatus.Message = "Something Went Wrong!";
                        }
                    }
                    else
                    {
                        _logger.ErrorLog(new Exception($"Location is not added in iotconnect, Error: {updateEntityResult.message}"), this.GetType().Name, MethodBase.GetCurrentMethod().Name);
                        actionStatus.Success = false;
                        actionStatus.Message = new UtilityHelper().IOTResultMessage(updateEntityResult.errorMessages);
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.ErrorLog(ex, "LocationService.Manage " + ex);
                actionStatus.Success = false;
                actionStatus.Message = ex.Message;
            }
            return actionStatus;
        }
        public Entity.ActionStatus Delete(Guid id)
        {
            Entity.ActionStatus actionStatus = new Entity.ActionStatus(true);
            try
            {
                var dbLocation = _locationRepository.FindBy(x => x.Guid.Equals(id)).FirstOrDefault();
                if (dbLocation == null)
                {
                    throw new NotFoundCustomException($"{CommonException.Name.NoRecordsFound} : Location");
                }
                var dbGenerator = _generatorRepository.FindBy(x => x.LocationGuid.Equals(id)).FirstOrDefault();
                if (dbGenerator == null)
                {
                    var deleteEntityResult = _iotConnectClient.Entity.Delete(id.ToString()).Result;
                    if (deleteEntityResult != null && deleteEntityResult.status)
                    {
                        dbLocation.IsDeleted = true;
                        dbLocation.UpdatedDate = DateTime.Now;
                        dbLocation.UpdatedBy = component.helper.SolutionConfiguration.CurrentUserId;
                        return _locationRepository.Update(dbLocation);
                    }
                    else
                    {
                        _logger.ErrorLog(new Exception($"Location is not deleted from iotconnect, Error: {deleteEntityResult.message}"), this.GetType().Name, MethodBase.GetCurrentMethod().Name);
                        actionStatus.Success = false;
                        actionStatus.Message = new UtilityHelper().IOTResultMessage(deleteEntityResult.errorMessages);
                    }
                }
                else
                {
                    _logger.ErrorLog(new Exception($"Location is not deleted in solution database.Generator exists, Error: {actionStatus.Message}"), this.GetType().Name, MethodBase.GetCurrentMethod().Name);
                    actionStatus.Success = false;
                    actionStatus.Message = "Location is not deleted in solution database.Generator exists";
                }


            }
            catch (Exception ex)
            {
                _logger.InfoLog(Constants.ACTION_EXCEPTION, "Location.Delete " + ex);
                actionStatus.Success = false;
                actionStatus.Message = ex.Message;
            }
            return actionStatus;
        }
        public Entity.SearchResult<List<Entity.LocationWithCounts>> List(Entity.SearchRequest request)
        {
            try
            {
                var result = _locationRepository.List(request);
                return new Entity.SearchResult<List<Entity.LocationWithCounts>>()
                {
                    Items = result.Items.Select(p => Mapper.Configuration.Mapper.Map<Entity.LocationWithCounts>(p)).ToList(),
                    Count = result.Count
                };
            }
            catch (Exception ex)
            {
                _logger.InfoLog(Constants.ACTION_EXCEPTION, $"LocationService.List, Error: {ex.Message}");
                return new Entity.SearchResult<List<Entity.LocationWithCounts>>();
            }
        }
        public Entity.ActionStatus UpdateStatus(Guid id, bool status)
        {
            Entity.ActionStatus actionStatus = new Entity.ActionStatus(true);
            try
            {
                var dbLocation = _locationRepository.FindBy(x => x.Guid.Equals(id)).FirstOrDefault();
                if (dbLocation == null)
                {
                    throw new NotFoundCustomException($"{CommonException.Name.NoRecordsFound} : Location");
                }

                var dbGenerator = _generatorRepository.FindBy(x => x.LocationGuid.Equals(id)).FirstOrDefault();
                if (dbGenerator == null)
                {
                    dbLocation.IsActive = status;
                    dbLocation.UpdatedDate = DateTime.Now;
                    dbLocation.UpdatedBy = component.helper.SolutionConfiguration.CurrentUserId;
                    return _locationRepository.Update(dbLocation);
                }
                else
                {
                    _logger.ErrorLog(new Exception($"Location is not updated in solution database.Generator exists, Error: {actionStatus.Message}"), this.GetType().Name, MethodBase.GetCurrentMethod().Name);
                    actionStatus.Success = false;
                    actionStatus.Message = "Location is not updated in solution database.Generator exists";
                }

            }
            catch (Exception ex)
            {
                _logger.InfoLog(Constants.ACTION_EXCEPTION, "LocationService.UpdateStatus " + ex);
                actionStatus.Success = false;
                actionStatus.Message = ex.Message;
            }
            return actionStatus;
        }
        public Entity.BaseResponse<Entity.LocationStaticsResponse> GetLocationStatics(Guid locationId)
        {
            Entity.BaseResponse<List<Entity.LocationStaticsResponse>> listResult = new Entity.BaseResponse<List<Entity.LocationStaticsResponse>>();
            Entity.BaseResponse<Entity.LocationStaticsResponse> result = new Entity.BaseResponse<Entity.LocationStaticsResponse>();
            try
            {
                listResult = _locationRepository.GetLocationStatics(locationId);
                var deviceResult = _generatorService.GetDeviceCountersByEntity(locationId);

                if (listResult.Data.Count > 0)
                {
                    result.IsSuccess = true;
                    result.Data = listResult.Data[0];
                    result.LastSyncDate = listResult.LastSyncDate;
                    if (deviceResult.IsSuccess && deviceResult.Data != null)
                    {
                        result.Data.TotalGenerators = deviceResult.Data.counters.total;
                        result.Data.TotalOnGenerators = deviceResult.Data.counters.connected;
                        result.Data.TotalDisconnectedGenerators = deviceResult.Data.counters.disConnected;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.InfoLog(Constants.ACTION_EXCEPTION, ex);
            }
            return result;
        }
        public Response.LocationDetailResponse GetLocationDetail(Guid locationId)
        {
            return new Response.LocationDetailResponse()
            {
                EnergyUsage = 2700,
                Temperature = 73,
                Moisture = 15,
                Humidity = 62,
                WaterUsage = 3800,
                TotalDevices = 15
            };
        }

    }
}
