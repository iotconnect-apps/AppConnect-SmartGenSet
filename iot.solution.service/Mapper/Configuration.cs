using Model = iot.solution.model.Models;
using Entity = iot.solution.entity;
using IOTUserProvider = IoTConnect.UserProvider;
using AutoMapper;
using System;
using IOT = IoTConnect.Model;
using iot.solution.service.Mapper.Mapping;
using System.Collections.Generic;
using System.Linq;

namespace iot.solution.service.Mapper
{
    public class Configuration
    {
        public static IMapper Mapper { get; private set; }

        public static void Initialize()
        {
            var config = new MapperConfiguration(mc =>
            {
                mc.CreateMap<Model.User, Entity.User>().ReverseMap();
                mc.CreateMap<Model.User, Entity.AddUserRequest>()
                .ForMember(au => au.EntityGuid, o => o.MapFrom(u => u.GensetGuid)).ReverseMap();
                mc.CreateMap<Model.Role, Entity.Role>().ReverseMap();
                mc.CreateMap<Model.Company, Entity.Company>().ReverseMap();
                mc.CreateMap<Model.Location, Entity.Location>().ReverseMap();
                mc.CreateMap<Model.LocationWithCounts, Entity.LocationWithCounts>().ReverseMap();
                
                mc.CreateMap<Model.Location, Entity.AddLocationRequest>().ReverseMap();
                 mc.CreateMap<Model.Generator, Entity.Generator>().ReverseMap();
                mc.CreateMap<Entity.GeneratorModel, Entity.Generator>().ReverseMap();
                mc.CreateMap<Model.User, Entity.UserResponse>().ReverseMap();
               
                mc.CreateMap<Model.KitTypeAttribute, Entity.KitTypeAttribute>().ReverseMap();
                mc.CreateMap<Model.KitTypeCommand, Entity.KitTypeCommand>().ReverseMap();
                mc.CreateMap<Model.HardwareKit, Entity.HardwareKit>().ReverseMap();
                mc.CreateMap<List<Entity.HardwareKit>, Entity.HardwareKitDTO>().ReverseMap();
                mc.CreateMap<Model.KitType, Entity.KitType>().ReverseMap();
                mc.CreateMap<IOT.AllRuleResult, Entity.AllRuleResponse>().ReverseMap();
                mc.CreateMap<IOT.SingleRuleResult, Entity.SingleRuleResponse>().ReverseMap();
                mc.CreateMap<Entity.Rule, IOT.AddRuleModel>().ReverseMap();
                mc.CreateMap<Entity.Rule, IOT.UpdateRuleModel>().ReverseMap();
                mc.CreateMap<Entity.HardwareKitDTO, Model.HardwareKit>().ReverseMap();

                #region " IOT Connect Mapping"

                mc.CreateMap<Entity.AddLocationRequest, IOT.AddEntityModel>().ConvertUsing(new LocationToAddEntityModelMapping());
                mc.CreateMap<Entity.AddLocationRequest, IOT.UpdateEntityModel>().ConvertUsing(new LocationToUpdateEntityModelMapping());
                mc.CreateMap<Entity.Generator, IOT.AddDeviceModel>().ConvertUsing(new GeneratorToAddDeviceModelMapping());
                mc.CreateMap<Entity.Generator, IOT.UpdateDeviceModel>().ConvertUsing(new GeneratorToUpdateDeviceModelMapping());

                mc.CreateMap<Entity.AddUserRequest, IOT.AddUserModel>().ConvertUsing(new AddUserRequestToAddUserModelMapping());
                mc.CreateMap<Entity.AddUserRequest, IOT.UpdateUserModel>().ConvertUsing(new AddUserRequestToUpdateUserModelMapping());
                mc.CreateMap<Entity.ChangePasswordRequest, IOT.ChangePasswordModel>().ConvertUsing(new ChangePasswordRequestToChangePasswordModel());
                mc.CreateMap<Entity.DeviceCounterResult, IOT.DeviceCounterResult>().ReverseMap();

                #endregion

                #region "AdminUser Mapping"

                mc.CreateMap<Model.AdminUser, Entity.AddAdminUserRequest>().ReverseMap();
                mc.CreateMap<Model.AdminUser, Entity.UserResponse>().ReverseMap();
                mc.CreateMap<Model.AdminUser, Entity.AdminUserResponse>().ReverseMap();

                #endregion
            });

            Mapper = config.CreateMapper();
        }

    }


}