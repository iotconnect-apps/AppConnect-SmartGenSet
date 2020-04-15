﻿using System;
using System.Collections.Generic;

namespace iot.solution.model.Models
{
    public partial class Generator
    {
        public Guid Guid { get; set; }
        public Guid CompanyGuid { get; set; }
        public Guid LocationGuid { get; set; }
        public Guid TemplateGuid { get; set; }
        public Guid? ParentGensetGuid { get; set; }
        public Guid TypeGuid { get; set; }
        public string UniqueId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Specification { get; set; }
        public string Note { get; set; }
        public string Tag { get; set; }
        public string Image { get; set; }
        public bool IsProvisioned { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
}
