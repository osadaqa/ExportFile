using ExportFile.Service.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ExportFile.WebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ExcelFileController : ControllerBase
    {
        private readonly IItemService _itemService;
        public ExcelFileController(IItemService itemService)
        {
            _itemService = itemService;
        }

        [HttpPost("Import")]
        public IActionResult Import(IFormFile file)
        {
            try
            {
                var items = _itemService.UploadFile(file);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
