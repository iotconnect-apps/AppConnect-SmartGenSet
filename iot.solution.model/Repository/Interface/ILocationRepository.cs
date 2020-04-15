using iot.solution.entity;
using System;
using System.Collections.Generic;
using Entity = iot.solution.entity;
using Model = iot.solution.model.Models;

namespace iot.solution.model.Repository.Interface
{
    public interface ILocationRepository : IGenericRepository<Model.Location>
    {
        Entity.SearchResult<List<Model.LocationWithCounts>> List(Entity.SearchRequest request);
        List<Entity.LookupItem> GetLookup(Guid companyId);
        ActionStatus Manage(Model.Location request);
        
    }
}
