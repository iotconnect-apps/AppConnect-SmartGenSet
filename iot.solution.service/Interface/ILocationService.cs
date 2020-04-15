using System;
using System.Collections.Generic;
using Entity = iot.solution.entity;
using Response = iot.solution.entity.Response;

namespace iot.solution.service.Interface
{
    public interface ILocationService
    {
        List<Entity.LocationWithCounts> Get();
        Entity.Location Get(Guid id);
        Entity.ActionStatus Manage(Entity.AddLocationRequest request);
        Entity.ActionStatus Delete(Guid id);
        Entity.SearchResult<List<Entity.LocationWithCounts>> List(Entity.SearchRequest request);
        Entity.ActionStatus UpdateStatus(Guid id, bool status);

        Response.LocationDetailResponse GetLocationDetail(Guid locationId);
        
    }
}
