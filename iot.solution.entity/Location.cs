﻿using iot.solution.entity.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace iot.solution.entity
{
    public class Location
    {
        public Guid Guid { get; set; }
        public Guid CompanyGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }

        public string ParentEntityGuid { get; set; }
        public string City { get; set; }
        public string Zipcode { get; set; }
        public Guid? StateGuid { get; set; }
        public Guid? CountryGuid { get; set; }
        public string Image { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        //public int TotalUsers{ get; set; }
        public List<LocationWiseGeneratorResponse> Generators { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
    public class LocationWithCounts : Location
    {
        public int TotalGenerators { get; set; }
        public int TotalOnConnectedGenerators { get; set; }
        public int TotalOffGenerators { get; set; }
        public int TotalDisconnectedGenerators { get; set; }
        public int TotalEneryGenerated { get; set; }
        public int TotalFuelUsed { get; set; }
       
    }
}
