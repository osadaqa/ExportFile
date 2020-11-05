namespace ExportFile.Repository.Entities
{
    public class FileInfo : BaseEntity
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int Size { get; set; }
    }
}
