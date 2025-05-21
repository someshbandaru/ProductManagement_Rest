using Microsoft.EntityFrameworkCore;
using ProductManagement.Core.Entities;
using ProductManagement.Core.Interfaces;
using ProductManagement.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductManagement.Infrastructure.Repositories
{
	public class ProductRepository : IProductRepository
	{
		private readonly ProductDbContext _context;

		public ProductRepository(ProductDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(Product product)
		{
			await _context.Products.AddAsync(product);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteAsync(string id)
		{
			var product = await _context.Products.FindAsync(id);
			if (product != null)
			{
				_context.Products.Remove(product);
				await _context.SaveChangesAsync();
			}
		}

		public async Task<IEnumerable<Product>> GetAllAsync()
		{
			return await _context.Products.ToListAsync();
		}

		public async Task<Product> GetByIdAsync(string id)
		{
			return await _context.Products.FindAsync(id);
		}

		public async Task UpdateAsync(Product product)
		{
			_context.Entry(product).State = EntityState.Modified;
			await _context.SaveChangesAsync();
		}

		public async Task<bool> ProductExistsAsync(string id)
		{
			return await _context.Products.AnyAsync(p => p.Id == id);
		}

		public async Task<string> GenerateUniqueProductIdAsync()
		{
			var lastProduct = await _context.Products
											.OrderByDescending(p => p.Id)
											.FirstOrDefaultAsync();

			if (lastProduct == null)
			{
				return "000001";
			}

			if (int.TryParse(lastProduct.Id, out int lastIdNum))
			{
				return (lastIdNum + 1).ToString("D6");
			}

			return Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
		}

		public async Task<bool> DecrementStockAsync(string id, int quantity)
		{
			var product = await _context.Products.FindAsync(id);
			if (product == null)
			{
				return false;
			}

			if (product.StockAvailable < quantity)
			{
				return false;
			}

			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				product.StockAvailable -= quantity;
				_context.Products.Update(product);
				await _context.SaveChangesAsync();
				await transaction.CommitAsync();
				return true;
			}
			catch
			{
				await transaction.RollbackAsync();
				throw;
			}
		}

		public async Task<bool> AddStockAsync(string id, int quantity)
		{
			var product = await _context.Products.FindAsync(id);
			if (product == null)
			{
				return false;
			}

			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				product.StockAvailable += quantity;
				_context.Products.Update(product);
				await _context.SaveChangesAsync();
				await transaction.CommitAsync();
				return true;
			}
			catch
			{
				await transaction.RollbackAsync();
				throw;
			}
		}
	}
}
