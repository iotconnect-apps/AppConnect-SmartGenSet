using component.logger;
using iot.solution.model.Repository.Interface;
using iot.solution.service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using Entity = iot.solution.entity;
using LogHandler = component.services.loghandler;

namespace iot.solution.service.Implementation
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardrepository;
        private readonly ILocationRepository _locationRepository;
        private readonly LogHandler.Logger _logger;
        private readonly IGeneratorService _generatorService;
        public DashboardService(ILocationRepository locationRepository, IDashboardRepository dashboardrepository, LogHandler.Logger logManager, IGeneratorService generatorService)
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

        public Entity.BaseResponse<Entity.DashboardOverviewResponse> GetOverview()
        {

            Entity.BaseResponse <List<Entity.DashboardOverviewResponse>> listResult = new Entity.BaseResponse<List<Entity.DashboardOverviewResponse>>();
            Entity.BaseResponse <Entity.DashboardOverviewResponse> result = new Entity.BaseResponse<Entity.DashboardOverviewResponse>();

            try
            {
                listResult = _dashboardrepository.GetStatistics();
                if (listResult.Data.Count > 0)
                {
                    result.IsSuccess = true;
                    result.Data = listResult.Data[0];
                    result.LastSyncDate = listResult.LastSyncDate;
                }

                var deviceResult = _generatorService.GetDeviceCounters();

                if (deviceResult.IsSuccess && deviceResult.Data != null)
                {
                    result.Data.TotalDisconnectedGenerators = deviceResult.Data.disConnected;
                    result.Data.TotalGenerators = deviceResult.Data.total;
                    result.Data.TotalOffGenerators = 0;
                    result.Data.TotalOnGenerators = deviceResult.Data.connected;
                    
                }
                else
                {
                    result.Data.TotalDisconnectedGenerators = 0;
                    result.Data.TotalGenerators = 0;
                    result.Data.TotalOffGenerators = 0;
                    result.Data.TotalOnGenerators = 0;
                }

            }
            catch (Exception ex)
            {
                _logger.InfoLog(Constants.ACTION_EXCEPTION, ex);
            }

            return result;
        }
    }
}
