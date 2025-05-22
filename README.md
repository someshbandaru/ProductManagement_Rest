# Product Management APIs

This project is an ASP.NET Core REST API designed including CRUD operations and stock management. It follows a clean architecture with the Repository pattern and uses Entity Framework Core's code-first approach for database management.

## Project Structure

The solution is organized into the following projects:

* **`ProductManagement.API`**: The main ASP.NET Core Web API project. It contains controllers, DTOs, and configures dependency injection.
* **`ProductManagement.Core`**: A class library containing domain entities (`Product`) and interfaces for repositories (`IProductRepository`). This layer represents the business logic core and is independent of data access or UI concerns.
* **`ProductManagement.Infrastructure`**: A class library responsible for data access. It contains the EF Core `DbContext` (`ProductDbContext`) and concrete implementations of the `IProductRepository`. It also handles EF Core migrations.
* **`ProductManagement.Tests`**: An xUnit test project for unit testing the repository and controller logic.

## Features

* **CRUD Operations for Products**:
    * `POST /api/products`: Create a new product.
    * `GET /api/products`: Retrieve all products.
    * `GET /api/products/{id}`: Retrieve a product by its unique ID.
    * `PUT /api/products/{id}`: Update an existing product.
    * `DELETE /api/products/{id}`: Delete a product.
* **Stock Management Endpoints**:
    * `PUT /api/products/decrement-stock/{id}/{quantity}`: Decrement the stock of a product.
    * `PUT /api/products/add-to-stock/{id}/{quantity}`: Add to the stock of a product.
* **Unique Auto-Generated Product ID**: Each product is assigned a unique 6-digit ID.
  
## Prerequisites

Before running the application, ensure you have the following installed:

* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/tools/sql-server-management-studio/download-sql-server-management-studio-ssms?view=sql-server-ver16#download-ssms) (typically comes with Visual Studio or SQL Server Express) or any other SQL Server instance.
* (Optional) [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or Visual Studio Code with C# extension.

## Getting Started

Follow these steps to set up and run the project locally:

1.  **Clone the Repository:**
    ```bash
    git clone <repository_url>
    cd ProductManagement
    ```

2.  **Open in Visual Studio (Optional):**
    Open the `ProductManagement.sln` file in Visual Studio.

3.  **Restore NuGet Packages:**
    If using command line, navigate to the solution root and run:
    ```bash
    dotnet restore
    ```
    Visual Studio usually restores them automatically upon opening.

4.  **Database Setup (Entity Framework Core Migrations):**
    The project uses SQL Server LocalDB by default. The connection string is defined in `ProductManagement.API/appsettings.json`.

    To create the database and apply migrations:
    * Open **Package Manager Console** in Visual Studio (`Tools` > `NuGet Package Manager` > `Package Manager Console`).
    * Set the **Default Project** to `ProductManagement.API`.
    * Run the following commands:
        ```powershell
        Add-Migration InitialCreate
        Update-Database
        ```
        (If you encounter issues, ensure `Microsoft.EntityFrameworkCore.Tools` is installed in `ProductManagement.API` and `Microsoft.EntityFrameworkCore.Design` is also referenced.)

    Alternatively, using the .NET CLI:
    Navigate to the `ProductManagement.API` directory in your terminal:
    ```bash
    cd ProductManagement.API
    dotnet ef migrations add InitialCreate
    dotnet ef database update
    ```

5.  **Run the API:**
    * **From Visual Studio:** Right-click on the `ProductManagement.API` project and select `Set as Startup Project`. Then press `F5` or click the `Run` button.
    * **From Command Line:** Navigate to the `ProductManagement.API` directory and run:
        ```bash
        dotnet run
        ```

    The API will typically run on `https://localhost:70XX` (the exact port may vary). Swagger UI will be available at `https://localhost:70XX/swagger`.

## API Endpoints

You can interact with the API using Swagger UI after running the application. Here are the available endpoints:

### Products

**1. Create Product**
* **Endpoint:** `POST /api/products`
* **Description:** Creates a new product.
* **Request Body Example (application/json):**
    ```json
    {
      "name": "Smartphone X",
      "description": "Latest model smartphone with advanced features.",
      "price": 799.99,
      "stockAvailable": 150
    }
    ```
* **Response (201 Created):**
    ```json
    {
      "id": "000001",
      "name": "Smartphone X",
      "description": "Latest model smartphone with advanced features.",
      "price": 799.99,
      "stockAvailable": 150
    }
    ```

**2. Get All Products**
* **Endpoint:** `GET /api/products`
* **Description:** Retrieves a list of all products.
* **Response (200 OK):**
    ```json
    [
      {
        "id": "000001",
        "name": "Smartphone X",
        "description": "Latest model smartphone with advanced features.",
        "price": 799.99,
        "stockAvailable": 150
      },
      {
        "id": "000002",
        "name": "Wireless Headphones",
        "description": "Noise-cancelling headphones.",
        "price": 199.50,
        "stockAvailable": 200
      }
    ]
    ```

**3. Get Product by ID**
* **Endpoint:** `GET /api/products/{id}`
* **Description:** Retrieves a single product by its unique ID.
* **Example:** `GET /api/products/000001`
* **Response (200 OK):**
    ```json
    {
      "id": "000001",
      "name": "Smartphone X",
      "description": "Latest model smartphone with advanced features.",
      "price": 799.99,
      "stockAvailable": 150
    }
    ```
* **Response (404 Not Found):** If the product does not exist.

**4. Update Product**
* **Endpoint:** `PUT /api/products/{id}`
* **Description:** Updates an existing product.
* **Example:** `PUT /api/products/000001`
* **Request Body Example (application/json):**
    ```json
    {
      "name": "Smartphone X Pro",
      "description": "Updated model with enhanced features.",
      "price": 849.99,
      "stockAvailable": 145
    }
    ```
* **Response (204 No Content):** On successful update.
* **Response (404 Not Found):** If the product does not exist.
* **Response (400 Bad Request):** If validation fails.

**5. Delete Product**
* **Endpoint:** `DELETE /api/products/{id}`
* **Description:** Deletes a product by its unique ID.
* **Example:** `DELETE /api/products/000001`
* **Response (204 No Content):** On successful deletion.
* **Response (404 Not Found):** If the product does not exist.

### Stock Management

**6. Decrement Product Stock**
* **Endpoint:** `PUT /api/products/decrement-stock/{id}/{quantity}`
* **Description:** Decrements the stock of a specific product by the given quantity.
* **Example:** `PUT /api/products/decrement-stock/000001/5`
* **Response (200 OK):**
    `Stock for product '000001' decremented by 5. New stock: 145`
* **Response (400 Bad Request):** If quantity is not positive or insufficient stock.
* **Response (404 Not Found):** If the product does not exist.

**7. Add to Product Stock**
* **Endpoint:** `PUT /api/products/add-to-stock/{id}/{quantity}`
* **Description:** Adds the given quantity to the stock of a specific product.
* **Example:** `PUT /api/products/add-to-stock/000001/10`
* **Response (200 OK):**
    `Stock for product '000001' increased by 10. New stock: 155`
* **Response (400 Bad Request):** If quantity is not positive.
* **Response (404 Not Found):** If the product does not exist.

## Running Tests

To run the unit tests:

* **From Visual Studio:** Open Test Explorer (`Test` > `Test Explorer`) and click `Run All Tests`.
* **From Command Line:** Navigate to the solution root and run:
    ```bash
    dotnet test
    ```

## Design Choices and Best Practices used

* **Repository Pattern**: Provides an abstraction layer over data access, making the application more modular, testable, and maintainable. It decouples the business logic from the persistence layer.
* **Separation of Concerns**: The solution is divided into `Core`, `Infrastructure`, and `API` layers, ensuring clear responsibilities for each part of the application.
* **Code-First Approach**: Entity Framework Core migrations are used to manage the database schema directly from the C# code, simplifying database evolution.
* **Dependency Injection**: `IProductRepository` and `ProductDbContext` are registered with the DI container, allowing for loose coupling and easy testing.
* **AutoMapper**: Used for mapping between domain entities (`Product`) and DTOs (`ProductDto`, `CreateProductDto`, `UpdateProductDto`), reducing boilerplate code.
* **Input Validation**: DTOs use `System.ComponentModel.DataAnnotations` for basic model validation, ensuring data integrity at the API boundary.
* **Error Handling**: Controllers include `try-catch` blocks and return specific HTTP status codes (e.g., 400, 404, 500) to communicate API errors effectively.
* **Concurrency for Stock Updates**: Stock modification methods (`DecrementStockAsync`, `AddStockAsync`) use database transactions to ensure atomicity, preventing data inconsistencies in concurrent scenarios.
