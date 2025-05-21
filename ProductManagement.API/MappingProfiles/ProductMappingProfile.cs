using AutoMapper;
using ProductManagement.API.Models;
using ProductManagement.Core.Entities;

namespace ProductManagement.API.MappingProfiles
{
	public class ProductMappingProfile : Profile
	{
		public ProductMappingProfile()
		{
			CreateMap<Product, ProductDto>();
			CreateMap<CreateProductDto, Product>();
			CreateMap<UpdateProductDto, Product>();
		}
	}
}
