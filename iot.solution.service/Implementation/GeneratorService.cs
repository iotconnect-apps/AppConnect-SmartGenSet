using component.helper;
using component.logger;
using iot.solution.common;
using iot.solution.entity;
using iot.solution.model.Repository.Interface;
using iot.solution.service.Interface;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Entity = iot.solution.entity;
using IOT = IoTConnect.Model;
using Model = iot.solution.model.Models;
using Response = iot.solution.entity.Response;

namespace iot.solution.service.Implementation
{
    public class GeneratorService : IGeneratorService
    {
        private readonly IGeneratorRepository _generatorRepository;
        private readonly IHardwareKitRepository _hardwareKitRepository;
        private readonly ILookupService _lookupService;
        private readonly IotConnectClient _iotConnectClient;
        private readonly ILogger _logger;
        public GeneratorService(IGeneratorRepository generatorRepository, ILookupService lookupService, IHardwareKitRepository hardwareKitRepository, ILogger logger)
        {
            _logger = logger;
            _generatorRepository = generatorRepository;
            _lookupService = lookupService;
            _hardwareKitRepository = hardwareKitRepository;
            _iotConnectClient = new IotConnectClient(SolutionConfiguration.BearerToken, SolutionConfiguration.Configuration.EnvironmentCode, SolutionConfiguration.Configuration.SolutionKey);
        }

        public List<Entity.Generator> Get()
        {
            try
            {
                return _generatorRepository.GetAll().Where(e => !e.IsDeleted && e.CompanyGuid == SolutionConfiguration.CompanyId).Select(p => Mapper.Configuration.Mapper.Map<Entity.Generator>(p)).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(Constants.ACTION_EXCEPTION, "GeneratorService.Get " + ex);
                return null;
            }
        }
        public Entity.Generator Get(Guid id)
        {
            try
            {
                Generator generator = _generatorRepository.FindBy(r => r.Guid == id).Select(p => Mapper.Configuration.Mapper.Map<Entity.Generator>(p)).FirstOrDefault();
                if (generator != null && generator.Guid.HasValue)
                {
                    generator.MediaFiles = _generatorRepository.GetMediaFiles(generator.Guid.Value);
                    var template = _lookupService.GetTemplate(false).FirstOrDefault();
                    if (template != null)
                        generator.TemplateGuid = new Guid(template.Value);
                }
                return generator;
            }
            catch (Exception ex)
            {
                _logger.Error(Constants.ACTION_EXCEPTION, "GeneratorService.Get " + ex);
                return null;
            }
        }
        public Response.GeneratorDetailResponse GetGeneratorDetail(Guid generatorId)
        {
            return new Response.GeneratorDetailResponse()
            {
                Engine = 2700,
                Current = 73,
                Voltage = 15,
                FuelLevel = 62,
                EngineOilLevel = 3800,
                BatteryStatus = 100
            };

        }
        public Entity.ActionStatus UploadFiles(List<Microsoft.AspNetCore.Http.IFormFile> files, string generatorId)
        {
            Entity.ActionStatus actionStatus = new Entity.ActionStatus(true);
            try
            {
                if (files.Count > 0)
                {
                    List<file> lstFileUploaded = new List<file>();
                    System.Text.StringBuilder strFileNotUploaded = new System.Text.StringBuilder();
                    foreach (var formFile in files)
                    {
                        file obj = new file();

                        string filePath = SaveGenaratorFiles(Guid.NewGuid(), formFile);
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            obj.path = filePath.ToString();
                            obj.desc = Path.GetFileNameWithoutExtension(formFile.FileName);
                            lstFileUploaded.Add(obj);
                        }
                        else
                        {
                            strFileNotUploaded.Append(formFile.FileName + " is invalid! ");
                        }
                    }
                    if (lstFileUploaded.Count > 0)
                    {
                        var xmlfiles = ObjectToXMLGeneric<List<file>>(lstFileUploaded);
                        xmlfiles = xmlfiles.Replace("ArrayOfFile", "files");
                        actionStatus = _generatorRepository.UploadFiles(xmlfiles, generatorId);
                    }
                    else
                    {
                        actionStatus.Success = false;
                        actionStatus.Message = strFileNotUploaded.ToString();
                    }
                }
                else
                {
                    actionStatus.Success = false;
                    actionStatus.Message = "Something Went Wrong!";
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Constants.ACTION_EXCEPTION, "GeneratorService.UploadFiles " + ex);
                return new Entity.ActionStatus
                {
                    Success = false,
                    Message = ex.Message
                };
            }
            return actionStatus;
        }
        public Entity.ActionStatus Manage(Entity.GeneratorModel request)
        {
            Entity.ActionStatus actionStatus = new Entity.ActionStatus(true);
            try
            {

                var dbGenerator = Mapper.Configuration.Mapper.Map<Entity.Generator, Model.Generator>(request);
                if (request.Guid == null || request.Guid == Guid.Empty)
                {
                    ////provision kit with kitcode and unique id 
                    var kitDeviceList = _generatorRepository.ProvisionKit(new ProvisionKitRequest { GeneratorGuid = new Guid(), KitCode = request.KitCode, UniqueId = request.UniqueId });
                    if (kitDeviceList != null && kitDeviceList.Data != null && kitDeviceList.Data.Any())
                    {
                        string templateGuid = _lookupService.GetIotTemplateGuidByName(kitDeviceList.Data.FirstOrDefault().TemplateName);
                        if (!string.IsNullOrEmpty(templateGuid))
                        {
                            request.TemplateGuid = new Guid(templateGuid);
                            var addDeviceResult = _iotConnectClient.Device.Add(Mapper.Configuration.Mapper.Map<IOT.AddDeviceModel>(request)).Result;
                            if (addDeviceResult != null && addDeviceResult.status && addDeviceResult.data != null)
                            {
                                request.Guid = Guid.Parse(addDeviceResult.data.newid.ToUpper());
                                IOT.DataResponse<IOT.AcquireDeviceResult> acquireResult = _iotConnectClient.Device.AcquireDevice(request.UniqueId, new IOT.AcquireDeviceModel()).Result;
                                if (request.ImageFile != null)
                                {
                                    // upload image                                     
                                    dbGenerator.Image = SaveGeneratorImage(request.Guid.Value, request.ImageFile);
                                }
                                dbGenerator.Guid = request.Guid.Value;
                                dbGenerator.IsProvisioned = acquireResult.status;
                                dbGenerator.IsActive = true;
                                dbGenerator.CompanyGuid = SolutionConfiguration.CompanyId;
                                dbGenerator.CreatedDate = DateTime.Now;
                                dbGenerator.CreatedBy = SolutionConfiguration.CurrentUserId;
                                actionStatus = _generatorRepository.Manage(dbGenerator);
                                actionStatus.Data = (Guid)(actionStatus.Data);
                                if (!actionStatus.Success)
                                {
                                    _logger.Error($"Generator is not added in solution database, Error: {actionStatus.Message}");
                                    var deleteEntityResult = _iotConnectClient.Device.Delete(request.Guid.Value.ToString()).Result;
                                    if (deleteEntityResult != null && deleteEntityResult.status)
                                    {
                                        _logger.Error($"Generator is not deleted from iotconnect");

                                        actionStatus.Success = false;
                                        actionStatus.Message = new UtilityHelper().IOTResultMessage(deleteEntityResult.errorMessages);
                                    }
                                }
                                else
                                {
                                    //Update companyid in hardware kit
                                    var hardwareKit = _hardwareKitRepository.GetByUniqueId(t => t.KitCode == request.KitCode && t.UniqueId == request.UniqueId);
                                    if (hardwareKit != null)
                                    {
                                        hardwareKit.CompanyGuid = SolutionConfiguration.CompanyId;
                                        _hardwareKitRepository.Update(hardwareKit);
                                    }
                                }
                            }
                            else
                            {

                                actionStatus.Data = Guid.Empty;
                                actionStatus.Success = false;
                                actionStatus.Message = new UtilityHelper().IOTResultMessage(addDeviceResult.errorMessages);

                            }
                        }
                        else
                        {
                            actionStatus.Success = false;
                            actionStatus.Data = Guid.Empty;
                            actionStatus.Message = "Unable To Locate Kit Type.";
                        }
                    }
                    else
                    {
                        _logger.Error($"Generator KitCode or UniqueId is not valid");
                        actionStatus.Data = Guid.Empty;
                        actionStatus.Success = false;
                        actionStatus.Message = "Generator KitCode or UniqueId is not valid!";
                    }
                }
                else
                {
                    var olddbDevice = _generatorRepository.FindBy(x => x.Guid.Equals(request.Guid)).FirstOrDefault();
                    if (olddbDevice == null)
                    {
                        throw new NotFoundCustomException($"{CommonException.Name.NoRecordsFound} : Generator");
                    }
                    var updateDeviceResult = _iotConnectClient.Device.Update(request.Guid.ToString(), Mapper.Configuration.Mapper.Map<IOT.UpdateDeviceModel>(request)).Result;
                    if (updateDeviceResult != null && updateDeviceResult.status)
                    {
                        if (request.ImageFile != null)
                        {
                            if (File.Exists(SolutionConfiguration.UploadBasePath + dbGenerator.Image) && request.ImageFile.Length > 0)
                            {
                                //if already exists image then delete  old image from server
                                File.Delete(SolutionConfiguration.UploadBasePath + dbGenerator.Image);
                            }
                            if (request.ImageFile.Length > 0)
                            {
                                // upload new image                                     
                                dbGenerator.Image = SaveGeneratorImage(dbGenerator.Guid, request.ImageFile);
                            }
                        }
                        else
                        {
                            //dbGreenHouse.Image = uniqGreenhouse.Image;
                        }
                        dbGenerator.CreatedDate = olddbDevice.CreatedDate;
                        dbGenerator.CreatedBy = olddbDevice.CreatedBy;
                        dbGenerator.UpdatedDate = DateTime.Now;
                        dbGenerator.UpdatedBy = SolutionConfiguration.CurrentUserId;
                        dbGenerator.CompanyGuid = SolutionConfiguration.CompanyId;
                        dbGenerator.TemplateGuid = olddbDevice.TemplateGuid;
                        actionStatus = _generatorRepository.Manage(dbGenerator);
                        actionStatus.Data = (Guid)(actionStatus.Data);
                        if (!actionStatus.Success)
                        {
                            _logger.Error($"Generator is not updated in solution database, Error: {actionStatus.Message}");
                            actionStatus.Success = false;
                            actionStatus.Message = "Something Went Wrong!";
                        }
                    }
                    else
                    {
                        _logger.Error($"Generator is not updated in iotconnect, Error: {updateDeviceResult.message}");
                        actionStatus.Success = false;
                        actionStatus.Message = new UtilityHelper().IOTResultMessage(updateDeviceResult.errorMessages);

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Constants.ACTION_EXCEPTION, "GeneratorService.Manage " + ex);
                return new Entity.ActionStatus
                {
                    Success = false,
                    Message = ex.Message
                };
            }
            return actionStatus;
        }
        public Entity.ActionStatus Delete(Guid id)
        {

            Entity.ActionStatus actionStatus = new Entity.ActionStatus(true);
            try
            {
                var dbDevice = _generatorRepository.FindBy(x => x.Guid.Equals(id)).FirstOrDefault();
                if (dbDevice == null)
                {
                    throw new NotFoundCustomException($"{CommonException.Name.NoRecordsFound} : Generator");
                }

                var deleteEntityResult = _iotConnectClient.Device.Delete(id.ToString()).Result;
                if (deleteEntityResult != null && deleteEntityResult.status)
                {
                    dbDevice.IsDeleted = true;
                    dbDevice.UpdatedDate = DateTime.Now;
                    dbDevice.UpdatedBy = SolutionConfiguration.CurrentUserId;
                    return _generatorRepository.Update(dbDevice);
                }
                else
                {
                    _logger.Error($"Generator is not deleted from iotconnect, Error: {deleteEntityResult.message}");
                    actionStatus.Success = false;
                    actionStatus.Message = new UtilityHelper().IOTResultMessage(deleteEntityResult.errorMessages);
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Constants.ACTION_EXCEPTION, "GeneratorService.Delete " + ex);
                actionStatus.Success = false;
                actionStatus.Message = ex.Message;
            }
            return actionStatus;
        }
        public Entity.ActionStatus DeleteMediaFile(Guid generatorId, Guid? fileId)
        {

            Entity.ActionStatus actionStatus = new Entity.ActionStatus(true);
            try
            {
                var dbMediaFile = _generatorRepository.FindBy(x => x.Guid.Equals(generatorId)).FirstOrDefault();
                if (dbMediaFile == null)
                {
                    throw new NotFoundCustomException($"{CommonException.Name.NoRecordsFound} : MediaFile");
                }
                return _generatorRepository.DeleteMediaFiles(generatorId, fileId);
            }
            catch (Exception ex)
            {
                _logger.Error(Constants.ACTION_EXCEPTION, "GeneratorService.DeleteMediaFile " + ex);
                actionStatus.Success = false;
                actionStatus.Message = ex.Message;
            }
            return actionStatus;
        }
        public Entity.ActionStatus UpdateStatus(Guid id, bool status)
        {
            Entity.ActionStatus actionStatus = new Entity.ActionStatus(true);
            try
            {
                var dbDevice = _generatorRepository.FindBy(x => x.Guid.Equals(id)).FirstOrDefault();
                if (dbDevice == null)
                {
                    throw new NotFoundCustomException($"{CommonException.Name.NoRecordsFound} : Generator");
                }

                var updatedbStatusResult = _iotConnectClient.Device.UpdateDeviceStatus(dbDevice.Guid.ToString(), new IOT.UpdateDeviceStatusModel() { IsActive = status }).Result;
                if (updatedbStatusResult != null && updatedbStatusResult.status)
                {
                    dbDevice.IsActive = status;
                    dbDevice.UpdatedDate = DateTime.Now;
                    dbDevice.UpdatedBy = SolutionConfiguration.CurrentUserId;
                    return _generatorRepository.Update(dbDevice);
                }
                else
                {
                    _logger.Error($"Generator status is not updated in iotconnect, Error: {updatedbStatusResult.message}");
                    actionStatus.Success = false;
                    actionStatus.Message = new UtilityHelper().IOTResultMessage(updatedbStatusResult.errorMessages);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Constants.ACTION_EXCEPTION, "GeneratorService.UpdateStatus " + ex);
                actionStatus.Success = false;
                actionStatus.Message = ex.Message;
            }
            return actionStatus;
        }
        public Entity.ActionStatus AcquireDevice(string deviceUniqueId)
        {
            Entity.ActionStatus actionStatus = new Entity.ActionStatus(true);
            try
            {
                IOT.DataResponse<IOT.AcquireDeviceResult> acquireResult = _iotConnectClient.Device.AcquireDevice(deviceUniqueId, new IOT.AcquireDeviceModel()).Result;
                if (acquireResult != null && acquireResult.status)
                {
                    var dbDevice = _generatorRepository.FindBy(x => x.UniqueId.Equals(deviceUniqueId)).FirstOrDefault();
                    dbDevice.IsProvisioned = true;
                    dbDevice.UpdatedDate = DateTime.Now;
                    dbDevice.UpdatedBy = SolutionConfiguration.CurrentUserId;
                    return _generatorRepository.Update(dbDevice);
                }
                else
                {
                    return new ActionStatus(false, acquireResult.message);
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Constants.ACTION_EXCEPTION, "DeviceService.AcquireDevice " + ex);
                actionStatus.Success = false;
                actionStatus.Message = ex.Message;
            }
            return actionStatus;
        }
        public Entity.SearchResult<List<Entity.Generator>> List(Entity.SearchRequest request)
        {
            try
            {
                Entity.SearchResult<List<Model.Generator>> result = _generatorRepository.List(request);
                return new Entity.SearchResult<List<Entity.Generator>>()
                {
                    Items = result.Items.Select(p => Mapper.Configuration.Mapper.Map<Entity.Generator>(p)).ToList(),
                    Count = result.Count
                };
            }
            catch (Exception ex)
            {
                _logger.Error(Constants.ACTION_EXCEPTION, $"GeneratorService.List, Error: {ex.Message}");
                return new Entity.SearchResult<List<Entity.Generator>>();
            }
        }
        public List<Response.LocationWiseGeneratorResponse> GetLocationWiseGenerators(Guid locationId)
        {
            try
            {
                return _generatorRepository.GetLocationWiseGenerators(locationId, null);
            }
            catch (Exception ex)
            {
                _logger.Error(Constants.ACTION_EXCEPTION, $"GeneratorService.GetLocationWiseGenerators, Error: {ex.Message}");
                return null;
            }
        }
        public List<Response.LocationWiseGeneratorResponse> GetLocationChildDevices(Guid deviceId)
        {
            try
            {
                return _generatorRepository.GetLocationWiseGenerators(null, deviceId);
            }
            catch (Exception ex)
            {
                _logger.Error(Constants.ACTION_EXCEPTION, $"GeneratorService.GetLocationChildDevices, Error: {ex.Message}");
                return null;
            }
        }
        public Entity.BaseResponse<int> ValidateKit(string kitCode)
        {
            Entity.BaseResponse<int> result = new Entity.BaseResponse<int>(true);
            try
            {
                return _generatorRepository.ValidateKit(kitCode);
            }
            catch (Exception ex)
            {
                _logger.Error(Constants.ACTION_EXCEPTION, $"GeneratorService.ValidateKit, Error: {ex.Message}");
                return null;
            }

        }
        public Entity.BaseResponse<bool> ProvisionKit(Entity.Generator request)
        {
            Entity.BaseResponse<bool> result = new Entity.BaseResponse<bool>(true);
            try
            {
                var repoResult = _generatorRepository.ProvisionKit(new ProvisionKitRequest { GeneratorGuid = new Guid(), KitCode = request.KitCode, UniqueId = request.UniqueId });
                if (repoResult != null && repoResult.Data != null && repoResult.Data.Any())
                {
                    Entity.HardwareKit device = repoResult.Data.OrderBy(d => d.KitCode == request.KitCode && d.UniqueId == request.UniqueId).FirstOrDefault();
                    IOT.AddDeviceModel iotDeviceDetail = new IOT.AddDeviceModel()
                    {
                        DisplayName = device.Name,
                        //entityGuid = request.GeneratorGuid.ToString(),
                        uniqueId = device.UniqueId,
                        deviceTemplateGuid = device.TemplateGuid.ToString(),
                        note = device.Note,
                        tag = device.Tag,
                        properties = new List<IOT.AddProperties>()
                    };
                    var addDeviceResult = _iotConnectClient.Device.Add(iotDeviceDetail).Result;
                    if (addDeviceResult != null && addDeviceResult.status && addDeviceResult.data != null)
                    {
                        Guid newDeviceId = Guid.Parse(addDeviceResult.data.newid.ToUpper());
                        Entity.ActionStatus actionStatus = _generatorRepository.Manage(new Model.Generator()
                        {
                            Guid = newDeviceId,
                            CompanyGuid = SolutionConfiguration.CompanyId,
                            Description = request.Description,
                            LocationGuid = new Guid(request.LocationGuid.ToString()),
                            Specification = request.Specification,
                            TemplateGuid = device.TemplateGuid.Value,
                            ParentGensetGuid = null,
                            TypeGuid = request.TypeGuid,
                            UniqueId = request.UniqueId,
                            Name = request.Name,
                            Note = request.Note,
                            Tag = request.Tag,
                            IsProvisioned = request.IsProvisioned,
                            IsActive = request.IsActive,
                            IsDeleted = false,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = SolutionConfiguration.CurrentUserId
                        });
                        if (!actionStatus.Success)
                        {
                            _logger.Error($"Generator is not added in solution database, Error: {actionStatus.Message}");
                            var deleteEntityResult = _iotConnectClient.Device.Delete(newDeviceId.ToString()).Result;
                            if (deleteEntityResult != null && deleteEntityResult.status)
                            {
                                _logger.Error($"Generator is not deleted from iotconnect");
                                actionStatus.Success = false;
                                actionStatus.Message = new UtilityHelper().IOTResultMessage(deleteEntityResult.errorMessages);
                            }
                        }
                        else
                        {
                            //Update companyid in hardware kit
                            var hardwareKit = _hardwareKitRepository.GetByUniqueId(t => t.KitCode == request.KitCode && t.UniqueId == request.UniqueId);
                            if (hardwareKit != null)
                            {
                                hardwareKit.CompanyGuid = SolutionConfiguration.CompanyId;
                                _hardwareKitRepository.Update(hardwareKit);
                            }
                            result.IsSuccess = true;
                        }
                    }
                    else
                    {
                        _logger.Error($"Kit is not added in iotconnect, Error: {addDeviceResult.message}");
                        result.IsSuccess = false;
                        result.Message = new UtilityHelper().IOTResultMessage(addDeviceResult.errorMessages);
                    }

                }
                else
                {
                    return new Entity.BaseResponse<bool>(false, repoResult.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Constants.ACTION_EXCEPTION, $"GeneratorService.ProvisionKit, Error: {ex.Message}");
                return null;
            }
            return result;
        }


        public Entity.BaseResponse<Entity.DeviceCounterResult> GetDeviceCounters()
        {
            Entity.BaseResponse<Entity.DeviceCounterResult> result = new Entity.BaseResponse<Entity.DeviceCounterResult >(true);
            try
            {
                IOT.DataResponse<List<IOT.DeviceCounterResult>> deviceCounterResult = _iotConnectClient.Device.GetDeviceCounters("").Result;
                if (deviceCounterResult != null && deviceCounterResult.status)
                {
                    result.Data = Mapper.Configuration.Mapper.Map<Entity.DeviceCounterResult>(deviceCounterResult.data.FirstOrDefault());
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorLog(ex, this.GetType().Name, MethodBase.GetCurrentMethod().Name);
                return new Entity.BaseResponse<Entity.DeviceCounterResult>(false, ex.Message);
            }
            return result;
        }


        #region private methods
        private string SaveGeneratorImage(Guid guid, IFormFile image)
        {
            var fileBasePath = SolutionConfiguration.UploadBasePath + SolutionConfiguration.CompanyFilePath;
            bool exists = System.IO.Directory.Exists(fileBasePath);
            if (!exists)
                System.IO.Directory.CreateDirectory(fileBasePath);
            string extension = Path.GetExtension(image.FileName);
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            string fileName = guid.ToString() + "_" + unixTimestamp;
            var filePath = Path.Combine(fileBasePath, fileName + extension);
            if (image != null && image.Length > 0 && SolutionConfiguration.AllowedImages.Contains(extension.ToLower()))
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    image.CopyTo(fileStream);
                }
                return Path.Combine(SolutionConfiguration.CompanyFilePath, fileName + extension);
            }
            return null;
        }
        private string SaveGenaratorFiles(Guid guid, IFormFile image)
        {
            var fileBasePath = SolutionConfiguration.UploadBasePath + SolutionConfiguration.CompanyFilePath;
            bool exists = System.IO.Directory.Exists(fileBasePath);
            if (!exists)
                Directory.CreateDirectory(fileBasePath);
            string extension = Path.GetExtension(image.FileName);
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            string fileName = guid.ToString() + "_" + unixTimestamp;
            var filePath = Path.Combine(fileBasePath, fileName + extension);

            if (image != null && image.Length > 0 && SolutionConfiguration.AllowedDocs.Contains(extension.ToLower()))
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    image.CopyTo(fileStream);
                }
                return Path.Combine(SolutionConfiguration.CompanyFilePath, fileName + extension);
            }
            return null;
        }
        private String ObjectToXMLGeneric<T>(T filter)
        {

            string xml = null;
            using (StringWriter sw = new StringWriter())
            {

                XmlSerializer xs = new XmlSerializer(typeof(T));
                xs.Serialize(sw, filter);
                try
                {
                    xml = sw.ToString();

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return xml;
        }
        #endregion
    }
}