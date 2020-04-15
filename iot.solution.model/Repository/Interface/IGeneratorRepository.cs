using iot.solution.entity;
using System;
using System.Collections.Generic;
using Entity = iot.solution.entity;
using Model = iot.solution.model.Models;
using Response = iot.solution.entity.Response;

namespace iot.solution.model.Repository.Interface
{
    public interface IGeneratorRepository : IGenericRepository<Model.Generator>
    {
        Model.Generator Get(string device);
        Entity.ActionStatus Manage(Model.Generator request);
        Entity.ActionStatus Delete(Guid id);
        Entity.SearchResult<List<Model.Generator>> List(Entity.SearchRequest request);
        List<Response.LocationWiseGeneratorResponse> GetLocationWiseGenerators(Guid? locationId, Guid? deviceId);
        Entity.BaseResponse<int> ValidateKit(string kitCode);
        Entity.BaseResponse<List<Entity.HardwareKit>> ProvisionKit(Entity.ProvisionKitRequest request);
        Entity.ActionStatus UploadFiles(string xmlString, string generatorId);
        Entity.ActionStatus DeleteMediaFiles(Guid generatorId,Guid? fileId);
        List<Entity.GeneratorFiles> GetMediaFiles(Guid generatorId);
        List<LookupItem> GetGeneratorLookup();
    }
}
