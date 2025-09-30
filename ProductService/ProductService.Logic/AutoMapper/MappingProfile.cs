using AutoMapper;
using ProductService.DAL.Dtos;
using ProductService.DAL.Dtos.Requests;
using ProductService.DAL.Models;
using ProductService.DAL.Requests;

namespace ProductService.Logic.AutoMapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>();
        CreateMap<ProductMedia, ProductMediaDto>();
        CreateMap<ProductMediaCreateRequest, ProductMedia>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ProductId, opt => opt.Ignore());

        CreateMap<Review, ReviewDto>();
        CreateMap<CreateReviewRequest, Review>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ProductId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

        CreateMap<CreateProductRequest, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SellerId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<UpdateProductRequest, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SellerId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Reviews, opt => opt.Ignore()); // не трогаем связанные сущности


        CreateMap<ProductDto, Product>().ReverseMap();
        CreateMap<ProductMediaDto, ProductMedia>().ReverseMap();
        CreateMap<ReviewDto, Review>().ReverseMap();
    }
}