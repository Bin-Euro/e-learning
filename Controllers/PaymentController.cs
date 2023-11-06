using Cursus.DTO.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using payment.DTO;
using payment.Services;

namespace payment.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IVnPayService _vnPayService;

        public PaymentController(IVnPayService vnPayService)
        {
            _vnPayService = vnPayService;
        }
        [HttpPost("create-payment-url")]
        [Authorize]
        public async Task<IActionResult> CreatePaymentUrl([FromBody] CreatePaymentReqDTO model)
        {
            if(model == null) { 
            return NoContent();
            }
            var payment = await _vnPayService.CreatePaymentUrl(model, HttpContext);
            if (!payment._isSuccess)
            {
                return NotFound(payment);
            }
            return Ok(payment);
        }

        [HttpGet("payment-callback")]
        public async Task<IActionResult> PaymentCallback()
        {
            var response = await _vnPayService.PaymentExecute(Request.Query);
            if (!response._isSuccess)
            {
                return NotFound(response);
            }
            return Ok(response);
        }
        [HttpGet("get-payment-by-id")]
        public async Task<IActionResult> GetOrder(string code)
        {
            var response = await _vnPayService.GetOrderByCode(code);
            if (!response._isSuccess)
            {
                return NotFound(response);
            }
            return Ok(response);
        }
    }
}
