using iot.solution.data;
using iot.solution.model.Models;
using iot.solution.model.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Entity = iot.solution.entity;
using component.logger;
using component.helper;

namespace iot.solution.model.Repository.Implementation
{
    public class DashboardRepository : GenericRepository<Generator>, IDashboardRepository
    {
        private readonly ILogger _logger;
        public DashboardRepository(IUnitOfWork unitOfWork, ILogger logManager) : base(unitOfWork, logManager)
        {
            _logger = logManager;
            _uow = unitOfWork;
        }
        public List<Entity.DashboardOverviewResponse> GetStatistics()
        {
            List<Entity.DashboardOverviewResponse> result = new List<Entity.DashboardOverviewResponse>();
            try
            {
                _logger.Information(Constants.ACTION_ENTRY, "GeneratorRepository.Get");
                using (var sqlDataAccess = new SqlDataAccess(ConnectionString))
                {
                    List<DbParameter> parameters = sqlDataAccess.CreateParams(SolutionConfiguration.CurrentUserId, SolutionConfiguration.Version);
                    parameters.Add(sqlDataAccess.CreateParameter("guid", SolutionConfiguration.CompanyId, DbType.Guid, ParameterDirection.Input));
                    DbDataReader dbDataReader = sqlDataAccess.ExecuteReader(sqlDataAccess.CreateCommand("[CompanyStatistics_Get]", CommandType.StoredProcedure, null), parameters.ToArray());
                    // DataUtils.DataReaderToObject(dbDataReader, result);
                    result = DataUtils.DataReaderToList<Entity.DashboardOverviewResponse>(dbDataReader, null);

                }
                _logger.Information(Constants.ACTION_EXIT, "GeneratorRepository.Get");
            }
            catch (Exception ex)
            {
                _logger.Error(Constants.ACTION_EXCEPTION, ex);
            }
            return result;
        }
    }
}
