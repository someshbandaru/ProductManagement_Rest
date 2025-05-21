using ProductManagement.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductManagement.Core.Interfaces
{
	public interface IProductRepository
	{
		Task<IEnumerable<Product>> GetAllAsync();
		Task<Product> GetByIdAsync(string id);
		Task AddAsync(Product product);
		Task UpdateAsync(Product product);
		Task DeleteAsync(string id);
		Task<bool> ProductExistsAsync(string id);
		Task<string> GenerateUniqueProductIdAsync();
		Task<bool> DecrementStockAsync(string id, int quantity);
		Task<bool> AddStockAsync(string id, int quantity);
	}
}
