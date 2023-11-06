using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Cursus.DTO;
using Cursus.DTO.Cart;
using Cursus.DTO.Catalog;
using Cursus.DTO.Course;
using Cursus.DTO.User;
using Cursus.DTO.CourseCatalog;
using Cursus.Entities;
using Cursus.Repositories;
using Cursus.DTO.Payment;

namespace Cursus.ObjectMapping
{
    public class CursusAutoMapperProfile : Profile
    {
        public CursusAutoMapperProfile()
        {
            CreateMap<Course, CourseDTO>().ReverseMap();
            CreateMap<Course, CreateCourseResDTO>().ReverseMap();
            CreateMap<Catalog, CatalogDTO>().ReverseMap();
            CreateMap<CourseCatalog, CourseCatalogResDTO>().ReverseMap();
            CreateMap<Course, UpdateCourseDTO>().ReverseMap();
            CreateMap<Cart, CartResponse>().ReverseMap();
            CreateMap<CartItem, CartItemsDTO>().ReverseMap();
            CreateMap<Course, CartItem>()
                .ForMember(dest => dest.CourseID, opt => opt.MapFrom(src => src.ID));
            CreateMap<User, UserDTO>().ReverseMap();
            CreateMap<User, UserProfileDTO>().ReverseMap();
            CreateMap<Order, CreatePaymentResDTO>().ReverseMap();
            CreateMap<Order, CreatePaymentReqDTO>().ReverseMap();
        }
    }
}