using Oracle.ManagedDataAccess.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMvc();

// Register your SQL Query Service
builder.Services.AddScoped<ISqlQueryService, SqlQueryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapApiRoutes();
app.Run();

// ==================== SQL QUERY SERVICE ====================
public interface ISqlQueryService
{
    // ONE function to rule them all!
    Task<object> ExecuteSqlAsync(string sql, Dictionary<string, object>? parameters = null);
}

public class SqlQueryService : ISqlQueryService
{
    private readonly IConfiguration _configuration;

    public SqlQueryService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<object> ExecuteSqlAsync(string sql, Dictionary<string, object>? parameters = null)
    {
        var connectionString = _configuration.GetConnectionString("FruitsInv");

        using (var connection = new OracleConnection(connectionString))
        {
            await connection.OpenAsync();

            using (var command = new OracleCommand(sql, connection))
            {
                command.BindByName = true;

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.Add(
                            new OracleParameter(param.Key, param.Value ?? DBNull.Value));
                    }
                }

                try
                {
                    if (sql.Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                    {
                        var results = new List<Dictionary<string, object?>>();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var row = new Dictionary<string, object?>();

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    var columnName = reader.GetName(i);
                                    var value = reader[i];

                                    row[columnName] =
                                        value == DBNull.Value ? null : value;
                                }

                                results.Add(row);
                            }
                        }

                        return results;
                    }
                    else
                    {
                        var rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected;
                    }
                }
                catch (OracleException ex)
                {
                    throw new Exception(
                        $"Oracle Error: {ex.Message}\nSQL: {sql}", ex);
                }
            }
        }
    }
}