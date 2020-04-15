using component.logger;
using iot.solution.model.Repository.Interface;
using iot.solution.service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using Entity = iot.solution.entity;

namespace iot.solution.service.Implementation
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardrepository;
        private readonly ILocationRepository _locationRepository;
        private readonly ILogger _logger;
        private readonly IGeneratorService _generatorService;
        public DashboardService(ILocationRepository locationRepository, IDashboardRepository dashboardrepository, ILogger logManager, IGeneratorService generatorService)
        {
            _locationRepository = locationRepository;
            _dashboardrepository = dashboardrepository;
            _logger = logManager;
            _generatorService = generatorService;
        }

        public List<Entity.LookupItem> GetLocationLookup(Guid companyId)
        {
            List<Entity.LookupItem> lstResult = new List<Entity.LookupItem>();
            lstResult = (from g in _locationRepository.FindBy(r => r.CompanyGuid == companyId)
                         select new Entity.LookupItem()
                         {
                             Text = g.Name,
                             Value = g.Guid.ToString().ToUpper()
                         }).ToList();
            return lstResult;
        }

        public Entity.DashboardOverviewResponse GetOverview()
        {

            List<Entity.DashboardOverviewResponse> listResult = new List<Entity.DashboardOverviewResponse>();
            Entity.DashboardOverviewResponse result = new Entity.DashboardOverviewResponse();

            try
            {
                listResult = _dashboardrepository.GetStatistics();
                if (listResult.Count > 0)
                {
                    result = listResult[0];
                }

                var deviceResult = _generatorService.GetDeviceCounters();

                if (deviceResult.IsSuccess && deviceResult.Data != null)
                {
                    result.TotalDisconnectedGenerators = deviceResult.Data.disConnected;
                    result.TotalGenerators = deviceResult.Data.total;
                    result.TotalOffGenerators = 0;
                    result.TotalOnGenerators = deviceResult.Data.connected;
                    
                }
                else
                {
                    result.TotalDisconnectedGenerators = 0;
                    result.TotalGenerators = 0;
                    result.TotalOffGenerators = 0;
                    result.TotalOnGenerators = 0;
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Constants.ACTION_EXCEPTION, ex);
            }

            return result;
        }
    }
}
