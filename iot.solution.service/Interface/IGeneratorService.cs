using System;
using System.Collections.Generic;
using Entity = iot.solution.entity;
using Response = iot.solution.entity.Response;

namespace iot.solution.service.Interface
{
    public interface IGeneratorService
    {
        List<Entity.Generator> Get();
        Entity.Generator Get(Guid id);
        Entity.ActionStatus Manage(Entity.GeneratorModel device);
        Entity.ActionStatus Delete(Guid id);
        Entity.ActionStatus DeleteImage(Guid id);
        Entity.ActionStatus DeleteMediaFile(Guid generatorId, Guid? fileId);
        Entity.SearchResult<List<Entity.Generator>> List(Entity.SearchRequest request);
        Entity.ActionStatus UpdateStatus(Guid id, bool status);
        Entity.ActionStatus AcquireDevice(string deviceUniqueId);
        Entity.BaseResponse<Response.GeneratorDetailResponse> GetGeneratorDetail(Guid deviceId);
        List<Response.LocationWiseGeneratorResponse> GetLocationWiseGenerators(Guid locationId);
        List<Response.LocationWiseGeneratorResponse> GetLocationChildDevices(Guid deviceId);
        Entity.BaseResponse<int> ValidateKit(string kitCode);
        Entity.BaseResponse<bool> ProvisionKit(Entity.Generator request);
        public Entity.ActionStatus UploadFiles(List<Microsoft.AspNetCore.Http.IFormFile> files, string generatorId);
        Entity.BaseResponse<Entity.DeviceCounterResult> GetDeviceCounters();
        Entity.BaseResponse<List<Entity.DeviceTelemetryDataResult>> GetTelemetryData(Guid generatorId);
        Entity.BaseResponse<Entity.DeviceCounterByEntityResult> GetDeviceCountersByEntity(Guid entityGuid);
        Entity.BaseResponse<Entity.DeviceConnectionStatusResult> GetConnectionStatus(string uniqueId);


    }
}
