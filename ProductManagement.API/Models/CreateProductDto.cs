using System.ComponentModel.DataAnnotations;

namespace ProductManagement.API.Models
{
	public class CreateProductDto
	{
		[Required(ErrorMessage = "Product name is required.")]
		[MaxLength(100, ErrorMessage = "Product name cannot exceed 100 characters.")]
		public string Name { get; set; }

		[MaxLength(500, ErrorMessage = "Product description cannot exceed 500 characters.")]
		public string Description { get; set; }

		[Range(0.01, 100000.00, ErrorMessage = "Price must be greater than 0.")]
		public decimal Price { get; set; }

		[Range(0, int.MaxValue, ErrorMessage = "Stock available cannot be negative.")]
		public int StockAvailable { get; set; }
	}
}
