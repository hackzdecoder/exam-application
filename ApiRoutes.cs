using Oracle.ManagedDataAccess.Client;

public static class ApiRoutes
{
    public static void MapApiRoutes(this WebApplication app)
    {
        app.MapPost("/api/hello", () => "Hello, World!");

        app.MapGet("/test-database-connection", (IConfiguration configuration) =>
        {
            var connectionString = configuration.GetConnectionString("FRUITSINV");
            if (string.IsNullOrEmpty(connectionString))
                return "Connection string not found";

            try
            {
                using (var connection = new OracleConnection(connectionString))
                {
                    connection.Open();
                    return "Database connection successful!";
                }
            }
            catch (Exception ex)
            {
                return $"Database connection failed: {ex.Message}";
            }
        });

        // View Fruit Inventory
        app.MapGet("/fruits-inventories", async (ISqlQueryService sql) =>
        {
            try
            {
                var results = await sql.ExecuteSqlAsync("SELECT * FROM FRUITS ORDER BY id");

                // Cast to the correct type
                var fruits = results as List<Dictionary<string, object?>>;

                return Results.Ok(new
                {
                    success = true,
                    count = fruits?.Count ?? 0,
                    data = fruits,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error: {ex.Message}");
            }
        });

        // Add Fruit Inventory
        app.MapPost("/add-fruits-inventory", async (HttpContext context, ISqlQueryService sql) =>
        {
            try
            {
                var request = await context.Request.ReadFromJsonAsync<Dictionary<string, object>>();
                if (request == null)
                {
                    return Results.BadRequest(new { error = "Invalid request body" });
                }

                var parameters = new Dictionary<string, object>
                {
                    ["name"] = request.GetValueOrDefault("name")?.ToString() ?? "",
                    ["type"] = request.GetValueOrDefault("type")?.ToString() ?? "",
                    ["stock"] = int.TryParse(request.GetValueOrDefault("stock")?.ToString(), out var stock) ? stock : 0,
                    ["price"] = decimal.TryParse(request.GetValueOrDefault("price")?.ToString(), out var price) ? price : 0m
                };

                var sqlQuery = @"INSERT INTO FRUITS (name, type, stock, price) VALUES (:name, :type, :stock, :price)";

                var result = await sql.ExecuteSqlAsync(sqlQuery, parameters);
                return Results.Ok(new { success = true, message = "Fruit inventory added successfully", data = result });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });


        // Add Fruit Inventory
        app.MapPost("/edit-fruits-inventory", async (HttpContext context, ISqlQueryService sql) =>
        {
            try
            {
                var request = await context.Request.ReadFromJsonAsync<Dictionary<string, object>>();
                if (request == null)
                {
                    return Results.BadRequest(new { error = "Invalid request body" });
                }

                var parameters = new Dictionary<string, object>
                {
                    ["id"] = int.TryParse(request.GetValueOrDefault("id")?.ToString(), out var id) ? id : 0,
                    ["name"] = request.GetValueOrDefault("name")?.ToString() ?? "",
                    ["type"] = request.GetValueOrDefault("type")?.ToString() ?? "",
                    ["stock"] = int.TryParse(request.GetValueOrDefault("stock")?.ToString(), out var stock) ? stock : 0,
                    ["price"] = decimal.TryParse(request.GetValueOrDefault("price")?.ToString(), out var price) ? price : 0m
                };

                var sqlQuery = @"SELECT * FROM FRUITS WHERE id = :id";

                var result = await sql.ExecuteSqlAsync(sqlQuery, parameters);

                return Results.Ok(new { success = true, message = "Fruit inventory edited successfully", data = result });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // Update Fruit Inventory
        app.MapPost("/update-fruits-inventory", async (HttpContext context, ISqlQueryService sql) =>
        {
            try
            {
                var request = await context.Request.ReadFromJsonAsync<Dictionary<string, object>>();
                if (request == null)
                {
                    return Results.BadRequest(new { error = "Invalid request body" });
                }

                var parameters = new Dictionary<string, object>
                {
                    ["id"] = int.TryParse(request.GetValueOrDefault("id")?.ToString(), out var id) ? id : 0,
                    ["name"] = request.GetValueOrDefault("name")?.ToString() ?? "",
                    ["type"] = request.GetValueOrDefault("type")?.ToString() ?? "",
                    ["stock"] = int.TryParse(request.GetValueOrDefault("stock")?.ToString(), out var stock) ? stock : 0,
                    ["price"] = decimal.TryParse(request.GetValueOrDefault("price")?.ToString(), out var price) ? price : 0m
                };

                var sqlQuery = @"UPDATE FRUITS SET name = :name, type = :type, stock = :stock, price = :price WHERE id = :id";

                var result = await sql.ExecuteSqlAsync(sqlQuery, parameters);
                return Results.Ok(new { success = true, message = "Fruit inventory updated successfully", data = result });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });
    }

}