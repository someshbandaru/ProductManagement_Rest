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

		// PUT: api/products/decrement-stock/{id}/{quantity}
		[HttpPut("decrement-stock/{id}/{quantity}")]
		public async Task<ActionResult> DecrementStock(string id, int quantity)
		{
			if (quantity <= 0)
			{
				return BadRequest("Quantity must be positive.");
			}

			try
			{
				var product = await _productRepository.GetByIdAsync(id);
				if (product == null)
				{
					return NotFound($"Product with ID '{id}' not found.");
				}

				if (product.StockAvailable < quantity)
				{
					return BadRequest($"Not enough stock for product '{id}'. Available only: {product.StockAvailable}, but Requested: {quantity}");
				}

				var success = await _productRepository.DecrementStockAsync(id, quantity);
				if (success)
				{
					var updatedProduct = await _productRepository.GetByIdAsync(id);
					return Ok($"Stock for product '{id}' decremented by {quantity}. New stock: {updatedProduct.StockAvailable}");
				}
				else
				{
					return StatusCode(500, "Failed to decrement stock due to an internal error.");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error decrementing stock for product ID: {id}");
				return StatusCode(500, "Internal server error.");
			}
		}

		// PUT: api/products/add-to-stock/{id}/{quantity}
		[HttpPut("add-to-stock/{id}/{quantity}")]
		public async Task<ActionResult> AddToStock(string id, int quantity)
		{
			if (quantity <= 0)
			{
				return BadRequest("Quantity must be positive as you are adding product.");
			}

			try
			{
				var product = await _productRepository.GetByIdAsync(id);
				if (product == null)
				{
					return NotFound($"Product with ID '{id}' not found.");
				}

				var success = await _productRepository.AddStockAsync(id, quantity);
				if (success)
				{
					var updatedProduct = await _productRepository.GetByIdAsync(id);
					return Ok($"Stock for product '{id}' increased by {quantity}. New stock: {updatedProduct.StockAvailable}");
				}
				else
				{
					return StatusCode(500, "Failed to add stock due to an internal error.");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error adding stock for product ID: {id}");
				return StatusCode(500, "Internal server error.");
			}
		}
	}
}
