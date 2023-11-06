using AutoMapper;
using Cursus.Constants;
using Cursus.DTO;
using Cursus.DTO.Course;
using Cursus.DTO.Payment;
using Cursus.Entities;
using Cursus.Services;
using Cursus.UnitOfWork;
using payment.DTO;
using payment.Services;
using static Google.Apis.Requests.BatchRequest;

namespace CodeMegaVNPay.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public VnPayService(IUnitOfWork unitOfWork, IMapper mapper,IConfiguration configuration, IUserService userService)
        {
            _configuration = configuration;
            _userService = userService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }
        public async Task<ResultDTO<CreatePaymentResDTO>> CreatePaymentUrl(CreatePaymentReqDTO model, HttpContext context)
        {
            if (model.Amount <5000 || model.Amount > 1000000000) {
                return ResultDTO<CreatePaymentResDTO>.Fail("Amount must be between 5000 and 1000000000");
            }

            try { 
            var user = await _userService.GetCurrentUser();
            var userId =Guid.Parse(user.Id);
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var tick = DateTime.Now.Ticks.ToString();
            var pay = new VnPayLibrary();
            var urlCallBack = _configuration["PaymentCallBack:ReturnUrl"];

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", ((double)model.Amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            pay.AddRequestData("vnp_OrderInfo", $"{user.FirstName} {user.LastName} da thanh toan so tien {model.Amount} voi hoa don {tick}");
            pay.AddRequestData("vnp_OrderType", "250000");
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", tick);
                
                var paymentUrl =
                pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);


            var _payment = new Order
            {
                ID = Guid.NewGuid(),
                Code = tick,
                PaymentUrl = paymentUrl,
                TotalPrice = model.Amount,
                Status = OrderStatus.Pending,
                UserID = userId,
            };
            foreach (var courseId in model.courseId)
                {
                    var course = await _unitOfWork.CourseRepository.GetAsync(c => c.ID.Equals(courseId));

                    if (course == null)
                    {
                        return ResultDTO<CreatePaymentResDTO>.Fail($"Course ID: {courseId} not found");
                    }
                    var _orderDetail = new OrderDetail
                    {
                        CourseID = course.ID,
                        OrderID = _payment.ID,
                    };
                    try
                    {
                        _unitOfWork.OrderDetailRepository.Add(_orderDetail);
                        await _unitOfWork.CommitAsync();
                    }catch (Exception ex)
                    {
                        return ResultDTO<CreatePaymentResDTO>.Fail("create order detail fail");
                    }
                    }
                _unitOfWork.OrderRepository.Add(_payment);
            try
            {
                await _unitOfWork.CommitAsync();
                var payment = _mapper.Map<CreatePaymentResDTO>(_payment);
                payment.courseId = model.courseId;
                return ResultDTO<CreatePaymentResDTO>.Success(payment);
            }

            catch (Exception ex)
            {
                return ResultDTO<CreatePaymentResDTO>.Fail("Failed to add infomation payment: " + ex.Message);
            }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ResultDTO<CreatePaymentResDTO>.Fail("service is not available");

            }
        }

        public async Task<ResultDTO<Order>> GetOrderByCode(string code)
        {
            var order = await _unitOfWork.OrderRepository.GetAsync(c => c.Code.Equals(code));
            if(order == null)
            {
                return ResultDTO<Order>.Fail("Order not found");
            }
            return ResultDTO<Order>.Success(order);
        }

        public async Task<ResultDTO<PaymentResponseModel>> PaymentExecute(IQueryCollection collections)
        {
            try
            {
                var pay = new VnPayLibrary();
                var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);

                if (response.VnPayResponseCode.Equals("00"))
                {
                    var order = _unitOfWork.OrderRepository.Get(c => c.Code == response.OrderId);

                    if (order != null)
                    {
                        order.Status = OrderStatus.Completed;
                        _unitOfWork.OrderRepository.Update(order);
                        _unitOfWork.Commit();
                        return ResultDTO<PaymentResponseModel>.Success(response);
                    }
                    return ResultDTO<PaymentResponseModel>.Fail("Update status order fail");
                }
                return ResultDTO<PaymentResponseModel>.Fail("order payment failed");
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                return ResultDTO<PaymentResponseModel>.Fail("service is not available");
            }
        }

    }
}
