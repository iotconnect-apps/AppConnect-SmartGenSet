using AutoMapper;
using Entity = iot.solution.entity;
using IOT = IoTConnect.Model;

namespace iot.solution.service.Mapper.Mapping
{
    class GeneratorToAddDeviceModelMapping : ITypeConverter<Entity.Generator, IOT.AddDeviceModel>
    {
        public IOT.AddDeviceModel Convert(Entity.Generator source, IOT.AddDeviceModel destination, ResolutionContext context)
        {
            if (source == null)
            {
                return null;
            }

            if (destination == null)
            {
                destination = new IOT.AddDeviceModel();
            }

            //destination.DisplayName = source.Name;
            //destination.uniqueId = source.UniqueId;
            //destination.entityGuid = source.EntityGuid.ToString().ToUpper();
            // destination.deviceTemplateGuid = source.DeviceTemplateGuid.ToString().ToUpper();
            // destination.parentDeviceGuid = source.ParentDeviceGuid.ToString().ToUpper();
            // destination.note = source.Note;
            // destination.tag = source.Tag;
            //destination.primaryThumbprint = ;
            //destination.secondaryThumbprint = ;
            //destination.endorsementKey = ;

            destination.DisplayName = source.Name;
            destination.uniqueId = source.UniqueId;
            destination.entityGuid = source.LocationGuid.ToString();
            destination.deviceTemplateGuid = source.TemplateGuid.ToString().ToUpper();
            // destination.parentDeviceGuid = source.ParentGensetGuid?.ToString().ToUpper();
            destination.note = source.Note;
            destination.tag = source.Tag;
            //destination.primaryThumbprint = ;
            //destination.secondaryThumbprint = ;
            //destination.endorsementKey = ;
            return destination;            
        }
    }
}
