using Microsoft.EntityFrameworkCore;
using ProductManagement.Core.Entities;
using ProductManagement.Infrastructure.Data;
using ProductManagement.Infrastructure.Repositories;

namespace ProductManagement.Tests
{
	public class ProductRepositoryTests
	{
		private ProductDbContext GetInMemoryDbContext()
		{
			var options = new DbContextOptionsBuilder<ProductDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;
			return new ProductDbContext(options);
		}

		[Fact]
		public async Task AddAsync_AddsProductToDatabase_Positive()
		{
			// Arrange
			using var context = GetInMemoryDbContext();
			var repository = new ProductRepository(context);
			var newProduct = new Product
			{
				Id = "P00001",
				Name = "Test Product",
				Description = "A product for testing purposes.",
				Price = 10.00m,
				StockAvailable = 100
			};

			// Act
			await repository.AddAsync(newProduct);

			// Assert
			var productInDb = await context.Products.FindAsync("P00001");
			Assert.NotNull(productInDb);
			Assert.Equal("Test Product", productInDb.Name);
			Assert.Equal(100, productInDb.StockAvailable);
			Assert.Equal("A product for testing purposes.", productInDb.Description);
		}

		[Fact]
		public async Task DeleteAsync_RemovesProductFromDatabase_Positive()
		{
			// Arrange
			using var context = GetInMemoryDbContext();
			var repository = new ProductRepository(context);
			var productToDelete = new Product { Id = "P00001", Name = "To Delete", Description = "Will be deleted", Price = 5, StockAvailable = 50 };
			await context.Products.AddAsync(productToDelete);
			await context.SaveChangesAsync();

			// Act
			await repository.DeleteAsync("P00001");

			// Assert
			var productInDb = await context.Products.FindAsync("P00001");
			Assert.Null(productInDb);
		}

		[Fact]
		public async Task DeleteAsync_DoesNothing_WhenProductNotFound_Negative()
		{
			// Arrange
			using var context = GetInMemoryDbContext();
			var repository = new ProductRepository(context);
			var initialProduct = new Product { Id = "P00001", Name = "Existing Product", Description = "This will remain", Price = 10, StockAvailable = 10 };
			await context.Products.AddAsync(initialProduct);
			await context.SaveChangesAsync();
			context.Entry(initialProduct).State = EntityState.Detached;

			// Act
			await repository.DeleteAsync("NonExistentId");

			// Assert
			var productCount = await context.Products.CountAsync();
			Assert.Equal(1, productCount);
			var productInDb = await context.Products.FindAsync("P00001");
			Assert.NotNull(productInDb);
		}

		[Fact]
		public async Task GetAllAsync_ReturnsAllProducts_Positive()
		{
			// Arrange
			using var context = GetInMemoryDbContext();
			var repository = new ProductRepository(context);
			var products = new List<Product>
			{
				new Product { Id = "P00001", Name = "Product A", Description = "Desc A", Price = 10, StockAvailable = 10 },
                new Product { Id = "P00002", Name = "Product B", Description = "Desc B", Price = 20, StockAvailable = 20 } 
            };
			await context.Products.AddRangeAsync(products);
			await context.SaveChangesAsync();

			// Act
			var result = await repository.GetAllAsync();

			// Assert
			Assert.NotNull(result);
			var resultList = result.ToList();
			Assert.Equal(2, resultList.Count);
			Assert.Contains(resultList, p => p.Id == "P00001");
			Assert.Contains(resultList, p => p.Id == "P00002");
		}

		[Fact]
		public async Task GetAllAsync_ReturnsEmptyList_WhenNoProductsExist_Negative()
		{
			// Arrange
			using var context = GetInMemoryDbContext();
			var repository = new ProductRepository(context);

			// Act
			var result = await repository.GetAllAsync();

			// Assert
			Assert.NotNull(result);
			Assert.Empty(result);
		}

		[Fact]
		public async Task GetByIdAsync_ReturnsProduct_WhenFound_Positive()
		{
			// Arrange
			using var context = GetInMemoryDbContext();
			var repository = new ProductRepository(context);
			var product = new Product { Id = "P00001", Name = "Specific Product", Description = "Detailed description", Price = 100, StockAvailable = 10 };
			await context.Products.AddAsync(product);
			await context.SaveChangesAsync();

			// Act
			var result = await repository.GetByIdAsync("P00001");

			// Assert
			Assert.NotNull(result);
			Assert.Equal("P00001", result.Id);
			Assert.Equal("Specific Product", result.Name);
			Assert.Equal("Detailed description", result.Description);
		}

		[Fact]
		public async Task GetByIdAsync_ReturnsNull_WhenNotFound_Negative()
		{
			// Arrange
			using var context = GetInMemoryDbContext();
			var repository = new ProductRepository(context);

			// Act
			var result = await repository.GetByIdAsync("NonExistentId");

			// Assert
			Assert.Null(result);
		}

		[Fact]
		public async Task UpdateAsync_UpdatesProductInDatabase_Positive()
		{
			// Arrange
			using var context = GetInMemoryDbContext();
			var repository = new ProductRepository(context);
			var initialProduct = new Product { Id = "P00001", Name = "Old Name", Description = "Old Desc", Price = 10, StockAvailable = 10 };
			await context.Products.AddAsync(initialProduct);
			await context.SaveChangesAsync();
			context.Entry(initialProduct).State = EntityState.Detached;

			var updatedProduct = new Product { Id = "P00001", Name = "New Name", Description = "New Desc", Price = 20, StockAvailable = 20 };

			// Act
			await repository.UpdateAsync(updatedProduct);

			// Assert
			var productInDb = await context.Products.FindAsync("P00001");
			Assert.NotNull(productInDb);
			Assert.Equal("New Name", productInDb.Name);
			Assert.Equal(20, productInDb.StockAvailable);
			Assert.Equal(20.00m, productInDb.Price);
			Assert.Equal("New Desc", productInDb.Description);
		}

		[Fact]
		public async Task ProductExistsAsync_ReturnsTrue_WhenProductExists_Positive()
		{
			// Arrange
			using var context = GetInMemoryDbContext();
			var repository = new ProductRepository(context);
			await context.Products.AddAsync(new Product { Id = "P00001", Name = "Exists", Description = "This exists", Price = 1, StockAvailable = 1 });
			await context.SaveChangesAsync();

			// Act
			var result = await repository.ProductExistsAsync("P00001");

			// Assert
			Assert.True(result);
		}

		[Fact]
		public async Task ProductExistsAsync_ReturnsFalse_WhenProductDoesNotExist_Negative()
		{
			// Arrange
			using var context = GetInMemoryDbContext();
			var repository = new ProductRepository(context);

			// Act
			var result = await repository.ProductExistsAsync("NonExistentId");

			// Assert
			Assert.False(result);
		}

		[Fact]
		public async Task GenerateUniqueProductIdAsync_ReturnsFirstId_WhenNoProductsExist_Positive()
		{
			// Arrange
			using var context = GetInMemoryDbContext();
			var repository = new ProductRepository(context);

			// Act
			var newId = await repository.GenerateUniqueProductIdAsync();

			// Assert
			Assert.Equal("000001", newId);
		}

		[Fact]
		public async Task GenerateUniqueProductIdAsync_ReturnsNextId_WhenProductsExist_Positive()
		{
			// Arrange
			using var context = GetInMemoryDbContext();
			var repository = new ProductRepository(context);
			await context.Products.AddAsync(new Product { Id = "000010", Name = "Last Product", Description = "Last one", Price = 1, StockAvailable = 1 }); 
			await context.Products.AddAsync(new Product { Id = "000005", Name = "Mid Product", Description = "Middle one", Price = 1, StockAvailable = 1 });
			await context.SaveChangesAsync();

			// Act
			var newId = await repository.GenerateUniqueProductIdAsync();

			// Assert
			Assert.Equal("000011", newId);
		}

		[Fact]
		public async Task GenerateUniqueProductIdAsync_ReturnsGuidSubstring_WhenLastProductIdIsInvalid_Negative()
		{
			// Arrange
			using var context = GetInMemoryDbContext();
			var repository = new ProductRepository(context);
			await context.Products.AddAsync(new Product { Id = "ABCDEF", Name = "Bad ID Product", Description = "Invalid ID", Price = 1, StockAvailable = 1 }); // Added Description
			await context.SaveChangesAsync();

			// Act
			var newId = await repository.GenerateUniqueProductIdAsync();

			// Assert
			Assert.NotNull(newId);
			Assert.Equal(6, newId.Length);
			Assert.DoesNotContain("000001", newId);
			Assert.DoesNotContain("ABCDEF", newId);												
			Assert.Matches("^[0-9A-F]{6}$", newId);
		}

		[Fact]
		public async Task DecrementStockAsync_ReturnsFalse_WhenProductNotFound_Negative()
		{
			// Arrange
			using var context = GetInMemoryDbContext();
			var repository = new ProductRepository(context);

			// Act
			var result = await repository.DecrementStockAsync("NonExistent", 10);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public async Task DecrementStockAsync_ReturnsFalse_WhenInsufficientStock_Negative()
		{
			// Arrange
			using var context = GetInMemoryDbContext();
			var repository = new ProductRepository(context);
			var product = new Product { Id = "P00001", Name = "Stock Item", Description = "Low stock", Price = 10, StockAvailable = 5 };
			await context.Products.AddAsync(product);
			await context.SaveChangesAsync();
			context.Entry(product).State = EntityState.Detached;

			// Act
			var result = await repository.DecrementStockAsync("P00001", 10);

			// Assert
			Assert.False(result);
			var productInDb = await context.Products.FindAsync("P00001");
			Assert.NotNull(productInDb);
			Assert.Equal(5, productInDb.StockAvailable);
		}

		[Fact]
		public async Task AddStockAsync_ReturnsFalse_WhenProductNotFound_Negative()
		{
			// Arrange
			using var context = GetInMemoryDbContext();
			var repository = new ProductRepository(context);

			// Act
			var result = await repository.AddStockAsync("NonExistent", 10);

			// Assert
			Assert.False(result);
		}
	}
}