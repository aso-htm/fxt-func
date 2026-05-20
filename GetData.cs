using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

public class GetData
{
    private readonly ILogger _logger;
    private readonly IConfiguration _config;

    public GetData(ILoggerFactory loggerFactory, IConfiguration config)
    {
        _logger = loggerFactory.CreateLogger<GetData>();
        _config = config;
    }

    [Function("GetData")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        var connStr = _config.GetConnectionString("Sql");

        using var conn = new SqlConnection(connStr);
        await conn.OpenAsync();

        var cmd = new SqlCommand(
            "SELECT Id, Name, CreatedAt FROM TestData ORDER BY Id DESC", conn);

        var reader = await cmd.ExecuteReaderAsync();

        var list = new List<object>();

        while (await reader.ReadAsync())
        {
            list.Add(new {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                CreatedAt = reader.GetDateTime(2)
            });
        }

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await response.WriteStringAsync(JsonSerializer.Serialize(list));
        return response;
    }
}
