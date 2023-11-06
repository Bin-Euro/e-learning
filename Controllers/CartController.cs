using Amazon.Runtime.Internal;
using Cursus.DTO.Cart;
using Cursus.Entities;
using Cursus.Repositories.Interfaces;
using Cursus.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Cursus.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Cursus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _service;

        public CartController(ICartService service)
        {
            _service = service;
        }

        [HttpGet("")]
        public async Task<ActionResult<CartResponse>> Get()
        {
            var result = await _service.GetByUserID();
            return StatusCode(result._statusCode, result);
        }

        // [HttpPost("confirm-cart")]
        // public async Task<ActionResult<CartResponse>> ConfirmCart([FromBody] ConfirmCartRequest request)
        // {
        //     var confirmCart = await _service.ConfirmCart(request.UserId, request.CourseIds);
        //     if (!confirmCart._isSuccess)
        //     {
        //         return NotFound(request);
        //     }
        //     return Ok(confirmCart);
        // }

        [HttpPut("add-to-cart")]
        public async Task<ActionResult<CreateCart>> AddToCart(AddOrRemoveCartRequest request)
        {
            var addToCart = await _service.AddToCart(request);
            if (!addToCart._isSuccess)
            {
                return NotFound(addToCart);
            }
            return Ok(addToCart);
        }

        [HttpDelete("remove-item")]
        public async Task<ActionResult<CreateCart>> RemoveItem(AddOrRemoveCartRequest request)
        {
            var remove = await _service.RemoveItem(request);
            if (!remove._isSuccess)
            {
                return NotFound(remove);
            }
            return Ok(remove);
        }
    }
}