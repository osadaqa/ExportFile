﻿using System;

namespace ExportFile.Service.DTO
{
    public class ItemModel
    {
        public int FileId { get; set; } = 1;
        public string Key { get; set; }
        public string ItemCode { get; set; }
        public string ColorCode { get; set; }
        public string Description { get; set; }
        public string Price { get; set; }
        public string DiscountPrice { get; set; }
        public string DeliveredIn { get; set; }
        public string Q1 { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public int CreatedBy { get; set; } = 1;
    }
}
