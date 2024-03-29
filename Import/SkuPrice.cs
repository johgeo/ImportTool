﻿using System;

namespace ImportAndValidationTool.Import
{
    public class SkuPrice
    {
        public string SkuNumber { get; set; }
        public decimal Price { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime ValidFromDate { get; set; }
        public DateTime ValidToDate { get; set; }
        public string Market { get; set; }
    }
}
