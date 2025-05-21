﻿namespace ProductManagement.API.Models
{
	public class ProductDto
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public decimal Price { get; set; }
		public int StockAvailable { get; set; }
	}
}
