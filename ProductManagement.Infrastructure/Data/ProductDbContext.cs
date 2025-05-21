using Microsoft.EntityFrameworkCore;
using ProductManagement.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ProductManagement.Infrastructure.Data
{
	public class ProductDbContext : DbContext
	{
		public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
		{
		}
		public DbSet<Product> Products { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Product>()
				.HasKey(p => p.Id);
		}
	}
}
