using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProductManagement.API.Controllers;
using ProductManagement.API.Models;
using ProductManagement.Core.Entities;
using ProductManagement.Core.Interfaces;

namespace ProductManagement.Tests
{
	public class ProductsControllerTests
	{
		private readonly Mock<IProductRepository> _mockRepo;
		private readonly Mock<IMapper> _mockMapper;
		private readonly Mock<ILogger<ProductsController>> _mockLogger;
		private readonly ProductsController _controller;

		public ProductsControllerTests()
		{
			_mockRepo = new Mock<IProductRepository>();
			_mockMapper = new Mock<IMapper>();
			_mockLogger = new Mock<ILogger<ProductsController>>();
			_controller = new ProductsController(_mockRepo.Object, _mockMapper.Object, _mockLogger.Object);
		}

		[Fact]
		public async Task GetProducts_ReturnsOkResult_WithListOfProducts()
		{
			// Arrange
			var products = new List<Product> { new Product { Id = "P00001", Name = "Test1", StockAvailable = 10 } };
			var productDtos = new List<ProductDto> { new ProductDto { Id = "P00001", Name = "Test1", StockAvailable = 10 } };

			_mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(products);
			_mockMapper.Setup(mapper => mapper.Map<IEnumerable<ProductDto>>(It.IsAny<IEnumerable<Product>>()))
					   .Returns(productDtos);

			// Act
			var result = await _controller.GetProducts();

			// Assert
			var okResult = Assert.IsType<OkObjectResult>(result.Result);
			var returnValue = Assert.IsType<List<ProductDto>>(okResult.Value);
			Assert.Single(returnValue);
		}

		[Fact]
		public async Task GetProduct_ReturnsNotFound_WhenProductDoesNotExist()
		{
			// Arrange
			_mockRepo.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((Product)null);

			// Act
			var result = await _controller.GetProduct("NonExistentId");

			// Assert
			Assert.IsType<NotFoundObjectResult>(result.Result);
		}

		[Fact]
		public async Task GetProduct_ReturnsOkResult_WithProduct()
		{
			// Arrange
			var product = new Product { Id = "P00001", Name = "Test Product", StockAvailable = 10 };
			var productDto = new ProductDto { Id = "P00001", Name = "Test Product", StockAvailable = 10 };

			_mockRepo.Setup(repo => repo.GetByIdAsync("P00001")).ReturnsAsync(product);
			_mockMapper.Setup(mapper => mapper.Map<ProductDto>(It.IsAny<Product>())).Returns(productDto);

			// Act
			var result = await _controller.GetProduct("P00001");

			// Assert
			var okResult = Assert.IsType<OkObjectResult>(result.Result);
			var returnValue = Assert.IsType<ProductDto>(okResult.Value);
			Assert.Equal("P00001", returnValue.Id);
		}

		[Fact]
		public async Task CreateProduct_ReturnsCreatedAtAction_WithProduct()
		{
			// Arrange
			var createDto = new CreateProductDto { Name = "New Product", Description = "Desc", Price = 100, StockAvailable = 50 };
			var product = new Product { Id = "000001", Name = "New Product", Description = "Desc", Price = 100, StockAvailable = 50 };
			var productDto = new ProductDto { Id = "000001", Name = "New Product", Description = "Desc", Price = 100, StockAvailable = 50 };

			_mockMapper.Setup(mapper => mapper.Map<Product>(It.IsAny<CreateProductDto>())).Returns(product);
			_mockRepo.Setup(repo => repo.GenerateUniqueProductIdAsync()).ReturnsAsync("000001");
			_mockRepo.Setup(repo => repo.AddAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
			_mockMapper.Setup(mapper => mapper.Map<ProductDto>(It.IsAny<Product>())).Returns(productDto);

			// Act
			var result = await _controller.CreateProduct(createDto);

			// Assert
			var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
			Assert.Equal(nameof(_controller.GetProduct), createdAtActionResult.ActionName);
			Assert.Equal("000001", ((ProductDto)createdAtActionResult.Value).Id);
		}

		[Fact]
		public async Task UpdateProduct_ReturnsNoContent_WhenProductExists()
		{
			// Arrange
			var id = "P00001";
			var updateDto = new UpdateProductDto { Name = "Updated Name", Description = "Updated Desc", Price = 150, StockAvailable = 60 };
			var existingProduct = new Product { Id = id, Name = "Old Name", Description = "Old Desc", Price = 100, StockAvailable = 50 };

			_mockRepo.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(existingProduct);
			_mockMapper.Setup(mapper => mapper.Map(updateDto, existingProduct));
			_mockRepo.Setup(repo => repo.UpdateAsync(existingProduct)).Returns(Task.CompletedTask);

			// Act
			var result = await _controller.UpdateProduct(id, updateDto);

			// Assert
			Assert.IsType<NoContentResult>(result);
		}

		[Fact]
		public async Task UpdateProduct_ReturnsNotFound_WhenProductDoesNotExist()
		{
			// Arrange
			_mockRepo.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((Product)null);

			// Act
			var result = await _controller.UpdateProduct("NonExistentId", new UpdateProductDto());

			// Assert
			Assert.IsType<NotFoundObjectResult>(result);
		}

		[Fact]
		public async Task DeleteProduct_ReturnsNoContent_WhenProductExists()
		{
			// Arrange
			_mockRepo.Setup(repo => repo.ProductExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
			_mockRepo.Setup(repo => repo.DeleteAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

			// Act
			var result = await _controller.DeleteProduct("P00001");

			// Assert
			Assert.IsType<NoContentResult>(result);
		}

		[Fact]
		public async Task DeleteProduct_ReturnsNotFound_WhenProductDoesNotExist()
		{
			// Arrange
			_mockRepo.Setup(repo => repo.ProductExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

			// Act
			var result = await _controller.DeleteProduct("NonExistentId");

			// Assert
			Assert.IsType<NotFoundObjectResult>(result);
		}

		[Fact]
		public async Task DecrementStock_ReturnsOk_WhenStockIsSufficient()
		{
			// Arrange
			var id = "P00001";
			var quantity = 5;
			var product = new Product { Id = id, Name = "Test Product", StockAvailable = 10 };
			_mockRepo.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(product);
			_mockRepo.Setup(repo => repo.DecrementStockAsync(id, quantity)).ReturnsAsync(true);

			// Act
			var result = await _controller.DecrementStock(id, quantity);

			// Assert
			var okResult = Assert.IsType<OkObjectResult>(result);
			Assert.Contains("decremented", okResult.Value.ToString());
		}

		[Fact]
		public async Task DecrementStock_ReturnsBadRequest_WhenInsufficientStock()
		{
			// Arrange
			var id = "P00001";
			var quantity = 15;
			var product = new Product { Id = id, Name = "Test Product", StockAvailable = 10 };
			_mockRepo.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(product);

			// Act
			var result = await _controller.DecrementStock(id, quantity);

			// Assert
			var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
			Assert.Contains("Not enough stock", badRequestResult.Value.ToString());
		}

		[Fact]
		public async Task DecrementStock_ReturnsNotFound_WhenProductDoesNotExist()
		{
			// Arrange
			_mockRepo.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((Product)null);

			// Act
			var result = await _controller.DecrementStock("NonExistentId", 5);

			// Assert
			Assert.IsType<NotFoundObjectResult>(result);
		}

		[Fact]
		public async Task AddToStock_ReturnsOk_WhenProductExists()
		{
			// Arrange
			var id = "P00001";
			var quantity = 5;
			var product = new Product { Id = id, Name = "Test Product", StockAvailable = 10 };
			_mockRepo.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(product);
			_mockRepo.Setup(repo => repo.AddStockAsync(id, quantity)).ReturnsAsync(true);

			// Act
			var result = await _controller.AddToStock(id, quantity);

			// Assert
			var okResult = Assert.IsType<OkObjectResult>(result);
			Assert.Contains("increased", okResult.Value.ToString());
		}

		[Fact]
		public async Task AddToStock_ReturnsNotFound_WhenProductDoesNotExist()
		{
			// Arrange
			_mockRepo.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((Product)null);

			// Act
			var result = await _controller.AddToStock("NonExistentId", 5);

			// Assert
			Assert.IsType<NotFoundObjectResult>(result);
		}

		[Fact]
		public async Task AddToStock_ReturnsBadRequest_WhenQuantityIsNegative()
		{
			// Arrange
			var id = "P00001";
			var quantity = -5;
			// No need to mock repository for this, as validation happens before repo call.

			// Act
			var result = await _controller.AddToStock(id, quantity);

			// Assert
			var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
			Assert.Contains("Quantity must be positive as you are adding product.", badRequestResult.Value.ToString());
		}
	}
}