using ExportFile.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace ExportFile.Repository
{
    public class ExportFileContext : DbContext
    {
        public ExportFileContext(DbContextOptions<ExportFileContext> options) : base(options) { }

        public DbSet<FileInfo> FileInfo { get; set; }
        public DbSet<Item> Items { get; set; }
    }
}
