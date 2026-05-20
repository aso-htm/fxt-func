using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

public class InsertData
{
    private readonly ILogger _logger;
    private readonly IConfiguration _config;

    public InsertData(ILoggerFactory loggerFactory, IConfiguration config)
    {
        _logger = loggerFactory.CreateLogger<InsertData>();
        _config = config;
    }

    [Function("InsertData")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("InsertData started.");

            var body = await JsonSerializer.DeserializeAsync<MyData>(req.Body);
            _logger.LogInformation($"Received Name = {body?.Name}");

            var connStr = _config.GetConnectionString("Sql");
            _logger.LogInformation($"Connection string loaded? {connStr != null}");

            using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();
            _logger.LogInformation("SQL connection opened.");

            var cmd = new SqlCommand(
                "INSERT INTO TestData (Name) VALUES (@Name)", conn);

            cmd.Parameters.AddWithValue("@Name", body.Name);
            await cmd.ExecuteNonQueryAsync();

            _logger.LogInformation("SQL insert completed.");

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteStringAsync("Inserted!");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "InsertData failed with exception.");
            var response = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
            await response.WriteStringAsync(ex.ToString());
            return response;
        }
    }
}

public class MyData
{
    public string Name { get; set; }
}
