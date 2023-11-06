using Cursus.DTO.Catalog;
using Cursus.DTO.Course;
using Cursus.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cursus.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class CatalogController : ControllerBase
{
    private readonly ICatalogService _catalogService;

    public CatalogController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllCatalog()
    {
        var result = await _catalogService.GetAll();
        return StatusCode(result._statusCode, result);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Post([FromBody] CatalogCreateDTO catalogRequest)
    {
        if (catalogRequest == null)
        {
            return NoContent();
        }

        var result = await _catalogService.AddCatalog(catalogRequest);
        return StatusCode(result._statusCode, result);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateCatalog([FromBody] CatalogDTO catalog)
    {
        var result = await _catalogService.UpdateCatalog(catalog);
        return StatusCode(result._statusCode, result);
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteCatalog(Guid ID)
    {
        var result = await _catalogService.DeleteCatalog(ID);
        return StatusCode(result._statusCode, result);
    }
}