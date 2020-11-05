using ExportFile.Service.DTO;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace ExportFile.Service.Services.Interface
{
    public interface IItemService
    {
        List<ItemModel> UploadFile(IFormFile file);
    }
}
