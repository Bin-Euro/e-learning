﻿using Cursus.DTO;
using Cursus.DTO.Payment;
using Cursus.Entities;
using payment.DTO;

namespace payment.Services
{
    public interface IVnPayService
    {
        Task<ResultDTO<CreatePaymentResDTO>> CreatePaymentUrl(CreatePaymentReqDTO model, HttpContext context);
        Task<ResultDTO<PaymentResponseModel>> PaymentExecute(IQueryCollection collections);
        Task<ResultDTO<Order>> GetOrderByCode(string code);
    }
}
