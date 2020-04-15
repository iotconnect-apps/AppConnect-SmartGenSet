using component.helper;
using component.logger;
using iot.solution.model.Repository.Interface;
using iot.solution.service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Entity = iot.solution.entity;

namespace iot.solution.service.Data
{
    public class LookupService : ILookupService
    {
        private readonly IGeneratorRepository _deviceRepository;
        private readonly IHardwareKitRepository _hardwareKitRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ILogger _logger;
        private readonly IotConnectClient _iotConnectClient;
        private readonly IKitTypeRepository _kitTypeRepository;
        private readonly IKitTypeAttributeRepository _kitTypeAttributeRepository;
        private readonly IkitTypeCommandRepository _kitTypeCommandRepository;
        public LookupService(ILogger logManager, IGeneratorRepository deviceRepository, IkitTypeCommandRepository kitTypeCommandRepository
           , ILocationRepository locationRepository, IHardwareKitRepository hardwareKitRepository
             , ICompanyRepository companyRepository
           , IKitTypeRepository kitTypeRepository, IKitTypeAttributeRepository kitTypeAttributeRepository)
        {
            _logger = logManager;
            _deviceRepository = deviceRepository;
            _kitTypeCommandRepository = kitTypeCommandRepository;
            _locationRepository = locationRepository;
            _hardwareKitRepository = hardwareKitRepository;
            _companyRepository = companyRepository;
            _kitTypeAttributeRepository = kitTypeAttributeRepository;
            _kitTypeRepository = kitTypeRepository;
            _iotConnectClient = new IotConnectClient(component.helper.SolutionConfiguration.BearerToken, component.helper.SolutionConfiguration.Configuration.EnvironmentCode, component.helper.SolutionConfiguration.Configuration.SolutionKey);
        }
        public string GetIotTemplateGuidByName(string templateName)
        {
            string templateGuid = string.Empty;
            var templates = _iotConnectClient.Template.All(new IoTConnect.Model.PagingModel() { SearchText = templateName, PageNo = 1, PageSize = 1000 }).Result;
            if (templates != null && templates.data != null && templates.data.Any())
            {
                templateGuid = templates.data[0].Guid;
            }
            return templateGuid;
        }
        public List<Entity.LookupItem> Get(string type, string param)
        {
            List<Entity.LookupItem> result = new List<Entity.LookupItem>();
            switch (type.ToLower())
            {
                case "device":
                    result = _deviceRepository.GetGeneratorLookup();
                    break;
                case "templates":
                    var templates = _iotConnectClient.Template.All(new IoTConnect.Model.PagingModel() { PageNo = 1, PageSize = 1000 }).Result;
                    if (templates != null && templates.data != null && templates.data.Any())
                    {
                        result = (from r in templates.data.Where(t => (!string.IsNullOrWhiteSpace(t.Tag)) == false) select new Entity.LookupItem() { Value = r.Guid, Text = r.Name }).ToList();
                    }
                    break;
                case "devicetags":
                    if (string.IsNullOrWhiteSpace(param)) { throw new System.Exception("templateGuid is missing in request"); }
                    var tags = _iotConnectClient.Master.AllAttributeLookup(param).Result;
                    if (tags != null && tags.data != null && tags.data.Any())
                    {
                        result = (from r in tags.data select new Entity.LookupItem() { Value = r.guid, Text = r.localname }).ToList();
                    }
                    break;
                case "templatecommand":
                    var templatecommands = _iotConnectClient.Template.AllTemplateCommand(param, new IoTConnect.Model.PagingModel() { PageNo = 1, PageSize = 100000 }).Result;
                    if (templatecommands != null && templatecommands.data != null && templatecommands.data.Any())
                    {
                        result = (from r in templatecommands.data select new Entity.LookupItem() { Value = r.guid, Text = r.name }).ToList();
                    }
                    break;
                //case "gateway":
                //    result = _generatorRepository.GetGeneratorLookup();
                //    break;
                case "location":
                    if (string.IsNullOrWhiteSpace(param)) { throw new System.Exception("Companyid is missing in request"); }
                    result = _locationRepository.GetLookup(System.Guid.Parse(param));
                    break;
                case "type":
                    result = _deviceRepository.GetGeneratorLookup();
                    break;
                case "role":
                    var roles = _iotConnectClient.User.AllRoleLookup().Result;
                    if (roles != null && roles.data != null && roles.data.Any())
                    {
                        result = (from r in roles.data.Where(t => t.IsActive) select new Entity.LookupItem() { Value = r.Guid, Text = r.Name }).ToList();
                    }
                    break;
                case "country":
                    var countries = _iotConnectClient.Master.Countries().Result;
                    if (countries != null && countries.data != null && countries.data.Any())
                    {
                        result = (from r in countries.data select new Entity.LookupItem() { Value = r.guid.ToLower(), Text = r.name }).ToList();
                    }
                    break;
                case "state":
                    if (string.IsNullOrWhiteSpace(param)) { throw new System.Exception("CountryId is missing in request"); }
                    var states = _iotConnectClient.Master.States(param).Result;
                    if (states != null && states.data != null && states.data.Any())
                    {
                        result = (from r in states.data select new Entity.LookupItem() { Value = r.guid.ToLower(), Text = r.name }).ToList();
                    }
                    break;
                case "timezone":
                    var timeZones = _iotConnectClient.Master.TimeZones().Result;

                    if (timeZones != null && timeZones.data != null && timeZones.data.Any())
                    {
                        result = (from r in timeZones.data select new Entity.LookupItem() { Value = r.guid, Text = r.name }).ToList();
                    }
                    break;
                default:
                    result = new List<Entity.LookupItem>();
                    break;
            }
            return result;
        }
        //public List<Entity.LookupItem> GetAllTemplate()
        //{
        //    List<Entity.LookupItem> result = new List<Entity.LookupItem>();
        //    result = (from template in _kitTypeRepository.FindBy(r => r.IsActive && !r.IsDeleted)
        //              join company in _companyRepository.FindBy(c => c.IsActive.HasValue && c.IsActive.Value && !c.IsDeleted) on template.CompanyGuid equals company.Guid
        //              select new Entity.LookupItem()
        //              {
        //                  Text = string.Format("{0} - {1}", company.Name, template.Name),
        //                  Value = template.Guid.ToString().ToUpper()
        //              }).ToList();
        //    return result;
        //}
        public List<Entity.LookupItem> GetTemplate(bool isGateway)
        {
            List<Entity.LookupItem> result = new List<Entity.LookupItem>();

            result = (from t in _kitTypeRepository.FindBy(r => r.IsActive && !r.IsDeleted)
                      select new Entity.LookupItem()
                      {
                          Text = t.Name,
                          Value = t.Guid.ToString().ToUpper()
                      }).ToList();

            //var templates = _iotConnectClient.Template.All(new IoTConnect.Model.PagingModel() { PageNo = 1, PageSize = 1000 }).Result;
            //if (templates != null && templates.data != null && templates.data.Any())
            //{
            //    result = (from r in templates.data.Where(t => (!string.IsNullOrWhiteSpace(t.Tag)) == isGateway) select new Entity.LookupItem() { Value = r.Guid, Text = r.Name }).ToList();
            //}
            return result;
        }
        public List<Entity.TagLookup> GetTagLookup(Guid templateId)
        {
            List<Entity.TagLookup> result = new List<Entity.TagLookup>();

            var template = _kitTypeRepository.FindBy(t => t.Guid == templateId).FirstOrDefault();
            if (template != null)
            {
                result.Add(new Entity.TagLookup() { tag = template.Tag, templateTag = true });

                result.AddRange(from t in _kitTypeAttributeRepository.FindBy(t => t.TemplateGuid == templateId)
                                select new Entity.TagLookup()
                                {
                                    tag = t.LocalName,
                                    templateTag = false
                                });
            }
            //var taglookup = _iotConnectClient.Template.TagLookUp(templateID).Result;
            //if (taglookup != null && taglookup.data != null && taglookup.data.Any())
            //{
            //    result = (from r in taglookup.data.Where(t => (!string.IsNullOrWhiteSpace(t.tag))) select new Entity.TagLookup() { tag = r.tag, templateTag = r.templateTag }).ToList();
            //}
            return result;
        }
        public List<Entity.LookupItem> GetSensors(Guid templateId, Guid deviceId)
        {
            List<Entity.LookupItem> result = new List<Entity.LookupItem>();
            try
            {
                var template = _kitTypeRepository.FindBy(t => !t.IsDeleted).FirstOrDefault();//.Name == SolutionConfiguration.DefaultIoTTemplateName
                if (template != null)
                {
                    var childAttribute = (from child in _kitTypeAttributeRepository.FindBy(t => t.TemplateGuid == template.Guid && t.ParentTemplateAttributeGuid != null)
                                          select child.ParentTemplateAttributeGuid).ToList();


                    result.AddRange(from device in _deviceRepository.FindBy(t => t.Guid == deviceId)
                                    join attribute in _kitTypeAttributeRepository.FindBy(t => t.TemplateGuid == template.Guid) on device.Tag equals attribute.Tag
                                    where !childAttribute.Contains(attribute.Guid)
                                    select new Entity.LookupItem()
                                    {
                                        Text = string.Format("{0}", attribute.LocalName),
                                        Value = attribute.Guid.ToString()
                                    });
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorLog(ex, this.GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
            return result;
        }
        public List<Entity.LookupItem> GetTemplateAttribute(Guid templateId)
        {
            List<Entity.LookupItem> result = new List<Entity.LookupItem>();
            try
            {
                var template = _kitTypeRepository.FindBy(t => t.Guid.Equals(templateId)).FirstOrDefault();
                if (template != null)
                {
                    result.AddRange(from t in _kitTypeAttributeRepository.FindBy(t => t.TemplateGuid == templateId).ToList()
                                    select new Entity.LookupItem()
                                    {
                                        Text = (string.IsNullOrWhiteSpace(t.Tag)) ? t.LocalName : string.Format("{0}({1})", t.LocalName, t.Tag),
                                        Value = t.Guid.ToString().ToUpper()
                                    });
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorLog(ex, this.GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
            return result;
        }
        public List<Entity.LookupItem> GetTemplateCommands(Guid templateId)
        {
            List<Entity.LookupItem> result = new List<Entity.LookupItem>();
            try
            {
                var template = _kitTypeRepository.FindBy(t => t.Guid == templateId).FirstOrDefault();
                if (template != null)
                {
                    result = (from t in _kitTypeCommandRepository.GetAll()
                              select new Entity.LookupItem()
                              {
                                  Text = t.Name,
                                  Value = t.Guid.ToString().ToUpper()
                              }).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorLog(ex, this.GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
            return result;
        }
        public List<Entity.LookupItem> GetAllTemplateFromIoT()
        {
            List<Entity.LookupItem> result = new List<Entity.LookupItem>();
            try
            {
                var templateList = _iotConnectClient.Template.All(new IoTConnect.Model.PagingModel()
                {
                    PageNo = 1,
                    PageSize = 50,
                    SearchText = "",
                    SortBy = ""
                }).Result.data;

                result = templateList.Select(x => new Entity.LookupItem()
                {
                    Text = x.Name,
                    Value = x.Guid.ToString().ToUpper()
                }).ToList();

            }
            catch (Exception ex)
            {
                _logger.ErrorLog(ex, this.GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
            return result;
        }


        public List<Entity.KitTypeAttribute> GetAllAttributesFromIoT(string templateGuid)
        {
            List<Entity.KitTypeAttribute> result = new List<Entity.KitTypeAttribute>();
            try
            {
                List<IoTConnect.Model.AttributeResult> attributeList = _iotConnectClient.Template.AllAttribute(templateGuid, new IoTConnect.Model.PagingModel() { }, "").Result.data;

                result = attributeList.Select(x => new Entity.KitTypeAttribute()
                {
                    LocalName = x.localName,
                    Guid = new Guid(x.guid),
                    Tag = x.tag.ToString()
                }).ToList();

            }
            catch (Exception ex)
            {
                _logger.ErrorLog(ex, this.GetType().Name, MethodBase.GetCurrentMethod().Name);
            }

            return result;
        }


        public List<Entity.LookupItem> GetAllCommandsFromIoT(string templateGuid)
        {
            List<Entity.LookupItem> result = new List<Entity.LookupItem>();
            try
            {
                List<IoTConnect.Model.AllCommandResult> attributeList = _iotConnectClient.Template.AllTemplateCommand(templateGuid, new IoTConnect.Model.PagingModel() { }).Result.data;

                return attributeList.Select(x => new Entity.LookupItem()
                {
                    Text = x.name,
                    Value = x.guid.ToUpper()
                }).ToList();

            }
            catch (Exception ex)
            {
                _logger.ErrorLog(ex, this.GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
            return result;
        }
    }
}