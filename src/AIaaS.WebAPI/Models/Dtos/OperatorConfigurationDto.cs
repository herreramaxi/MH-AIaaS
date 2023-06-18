﻿namespace AIaaS.WebAPI.Models.Dtos
{
    public class OperatorConfigurationDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public object Value { get; set; }
        public string? Default { get; set; }
    }
}
