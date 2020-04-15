using component.logger;
using iot.solution.data;
using iot.solution.model.Repository.Interface;
using iot.solution.service.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using Request = iot.solution.entity.Request;
using Response = iot.solution.entity.Response;

namespace iot.solution.service.Implementation
{
    public class ChartService : IChartService
    {
        private readonly ILocationRepository _locationRepository;
        private readonly ILogger _logger;
        public string ConnectionString = component.helper.SolutionConfiguration.Configuration.ConnectionString;
        //private readonly LogHandler.Logger _logger;
        public ChartService(ILocationRepository locationRepository, ILogger logger)//, LogHandler.Logger logger)
        {
            _locationRepository = locationRepository;
            _logger = logger;
        }
        public List<Response.GeneratorUsageResponse> GetGeneratorUsage(Request.ChartRequest request)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("companyguid", request.CompanyGuid.ToString());
            parameters.Add("greenhouseguid", request.EntityGuid.ToString());
            parameters.Add("hardwarekitguid", request.DeviceGuid.ToString());
            return _locationRepository.ExecuteStoredProcedure<Response.GeneratorUsageResponse>("[GensetUsage_Get]", parameters);
        }
        public List<Response.EnergyUsageResponse> GetEnergyUsage(Request.ChartRequest request)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("companyguid", request.CompanyGuid.ToString());
            parameters.Add("greenhouseguid", request.EntityGuid.ToString());
            parameters.Add("hardwarekitguid", request.DeviceGuid.ToString());
            return _locationRepository.ExecuteStoredProcedure<Response.EnergyUsageResponse>("[ChartDate]", parameters);
        }

        public List<Response.EnergyUsageResponse> GetEnergyGenerated(Request.ChartRequest request)
        {
            List<Response.EnergyUsageResponse> result = new List<Response.EnergyUsageResponse>();
            try
            {
                _logger.Information(Constants.ACTION_ENTRY, "Chart_EnergyGenerated.Get");
                using (var sqlDataAccess = new SqlDataAccess(ConnectionString))
                {
                    List<DbParameter> parameters = sqlDataAccess.CreateParams(component.helper.SolutionConfiguration.CurrentUserId, component.helper.SolutionConfiguration.Version);
                    parameters.Add(sqlDataAccess.CreateParameter("entityguid", request.EntityGuid, DbType.Guid, ParameterDirection.Input));
                    parameters.Add(sqlDataAccess.CreateParameter("guid", request.DeviceGuid, DbType.Guid, ParameterDirection.Input));
                    parameters.Add(sqlDataAccess.CreateParameter("enableDebugInfo", component.helper.SolutionConfiguration.EnableDebugInfo, DbType.String, ParameterDirection.Input));
                    DbDataReader dbDataReader = sqlDataAccess.ExecuteReader(sqlDataAccess.CreateCommand("[Chart_EnergyGenerated]", CommandType.StoredProcedure, null), parameters.ToArray());
                    result = DataUtils.DataReaderToList<Response.EnergyUsageResponse>(dbDataReader, null);
                }
                _logger.InfoLog(Constants.ACTION_EXIT, null, "", "", this.GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
            catch (Exception ex)
            {
                _logger.ErrorLog(ex, this.GetType().Name, MethodBase.GetCurrentMethod().Name);
            }

            return result;
        }

        public List<Response.GeneratorBatteryStatusResponse> GetGeneratorBatteryStatus(Request.ChartRequest request)
        {
            List<Response.GeneratorBatteryStatusResponse> result = new List<Response.GeneratorBatteryStatusResponse>();
            try
            {
                _logger.Information(Constants.ACTION_ENTRY, "Chart_GeneratorBatteryStatus.Get");
                using (var sqlDataAccess = new SqlDataAccess(ConnectionString))
                {
                    List<DbParameter> parameters = sqlDataAccess.CreateParams(component.helper.SolutionConfiguration.CurrentUserId, component.helper.SolutionConfiguration.Version);
                    parameters.Add(sqlDataAccess.CreateParameter("companyguid", request.CompanyGuid, DbType.Guid, ParameterDirection.Input));
                    parameters.Add(sqlDataAccess.CreateParameter("entityguid", request.EntityGuid, DbType.Guid, ParameterDirection.Input));
                    parameters.Add(sqlDataAccess.CreateParameter("guid", request.DeviceGuid, DbType.Guid, ParameterDirection.Input));
                    parameters.Add(sqlDataAccess.CreateParameter("enableDebugInfo", component.helper.SolutionConfiguration.EnableDebugInfo, DbType.String, ParameterDirection.Input));
                    DbDataReader dbDataReader = sqlDataAccess.ExecuteReader(sqlDataAccess.CreateCommand("[Chart_GeneratorBatteryStatus]", CommandType.StoredProcedure, null), parameters.ToArray());
                    result = DataUtils.DataReaderToList<Response.GeneratorBatteryStatusResponse>(dbDataReader, null);
                }
                _logger.InfoLog(Constants.ACTION_EXIT, null, "", "", this.GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
            catch (Exception ex)
            {
                _logger.ErrorLog(ex, this.GetType().Name, MethodBase.GetCurrentMethod().Name);
            }

            return result;
        }

        public List<Response.FuelUsageResponse> GetFuelUsage(Request.ChartRequest request)
        {
            List<Response.FuelUsageResponse> result = new List<Response.FuelUsageResponse>();
            try
            {
                _logger.Information(Constants.ACTION_ENTRY, "Chart_FuelUsed.Get");
                using (var sqlDataAccess = new SqlDataAccess(ConnectionString))
                {
                    List<DbParameter> parameters = sqlDataAccess.CreateParams(component.helper.SolutionConfiguration.CurrentUserId, component.helper.SolutionConfiguration.Version);
                    parameters.Add(sqlDataAccess.CreateParameter("entityguid", request.EntityGuid, DbType.Guid, ParameterDirection.Input));
                    parameters.Add(sqlDataAccess.CreateParameter("guid", request.DeviceGuid, DbType.Guid, ParameterDirection.Input));
                    parameters.Add(sqlDataAccess.CreateParameter("enableDebugInfo", component.helper.SolutionConfiguration.EnableDebugInfo, DbType.String, ParameterDirection.Input));
                    DbDataReader dbDataReader = sqlDataAccess.ExecuteReader(sqlDataAccess.CreateCommand("[Chart_FuelUsed]", CommandType.StoredProcedure, null), parameters.ToArray());
                    result = DataUtils.DataReaderToList<Response.FuelUsageResponse>(dbDataReader, null);
                }
                _logger.InfoLog(Constants.ACTION_EXIT, null, "", "", this.GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
            catch (Exception ex)
            {
                _logger.ErrorLog(ex, this.GetType().Name, MethodBase.GetCurrentMethod().Name);
            }

            return result;
        }
    }
}
