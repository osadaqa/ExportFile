using System;
using System.ComponentModel.DataAnnotations;

namespace ExportFile.Repository.Entities
{
    public class BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public DateTime CreationDate { get; set; }
        public int CreatedBy { get; set; }
    }
}
