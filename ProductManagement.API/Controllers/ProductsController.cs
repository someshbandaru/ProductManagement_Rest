using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.API.Models;
using ProductManagement.Core.Entities;
using ProductManagement.Core.Interfaces;

namespace ProductManagement.API.Controllers
{
	[Route("api/products")]
	[ApiController]
	public class ProductsController : ControllerBase
	{
		private readonly IProductRepository _productRepository;
		private readonly IMapper _mapper;
		private readonly ILogger<ProductsController> _logger;

		public ProductsController(IProductRepository productRepository, IMapper mapper, ILogger<ProductsController> logger)
		{
			_productRepository = productRepository;
			_mapper = mapper;
			_logger = logger;
		}

		// GET: api/products
		[HttpGet]
		public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
		{
			try
			{
				var products = await _productRepository.GetAllAsync();
				var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
				return Ok(productDtos);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting all products.");
				return StatusCode(500, "Internal server error.");
			}
		}

		// GET: api/products/{id}
		[HttpGet("{id}")]
		public async Task<ActionResult<ProductDto>> GetProduct(string id)
		{
			try
			{
				var product = await _productRepository.GetByIdAsync(id);
				if (product == null)
				{
					return NotFound($"Product with ID '{id}' not found.");
				}
				var productDto = _mapper.Map<ProductDto>(product);
				return Ok(productDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error getting product with ID: {id}");
				return StatusCode(500, "Internal server error.");
			}
		}

		// POST: api/products
		[HttpPost]
		public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto createProductDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				var product = _mapper.Map<Product>(createProductDto);
				product.Id = await _productRepository.GenerateUniqueProductIdAsync();

				await _productRepository.AddAsync(product);

				var productDto = _mapper.Map<ProductDto>(product);
				return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating product.");
				return StatusCode(500, "Internal server error.");
			}
		}

		// PUT: api/products/{id}
		[HttpPut("{id}")]
		public async Task<ActionResult> UpdateProduct(string id, [FromBody] UpdateProductDto updateProductDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				var existingProduct = await _productRepository.GetByIdAsync(id);
				if (existingProduct == null)
				{
					return NotFound($"Product with ID '{id}' not found.");
				}

				_mapper.Map(updateProductDto, existingProduct);

				await _productRepository.UpdateAsync(existingProduct);
				return NoContent();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error updating product with ID: {id}");
				return StatusCode(500, "Internal server error.");
			}
		}

		// DELETE: api/products/{id}
		[HttpDelete("{id}")]
		public async Task<ActionResult> DeleteProduct(string id)
		{
			try
			{
				var productExists = await _productRepository.ProductExistsAsync(id);
				if (!productExists)
				{
					return NotFound($"Product with ID '{id}' not found.");
				}

				await _productRepository.DeleteAsync(id);
				return NoContent();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error deleting product with ID: {id}");
				return StatusCode(500, "Internal server error.");
			}
		}
	}

}
