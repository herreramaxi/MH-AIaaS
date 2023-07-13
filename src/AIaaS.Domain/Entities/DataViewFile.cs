﻿using AIaaS.Domain.Common;

namespace AIaaS.Domain.Entities
{
    public class DataViewFile: AuditableEntity
    {
        public int Id { get; set; }
        public int DatasetId { get; set; }
        public Dataset Dataset { get; set; } = null!;
        public long Size { get; set; }
        public byte[] Data { get; set; }
        public string? Name { get; set; }
    }
}