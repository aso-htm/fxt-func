using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

public class InsertData
{
    private readonly ILogger<InsertData> _logger;
    private readonly IConfiguration _config;

    public InsertData(ILogger<InsertData> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }
[Function("InsertData")]
public async Task<HttpResponseData> Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
{
    try
    {
        _logger.LogInformation("CONN = " + _config.GetConnectionString("Sql"));

        using var reader = new StreamReader(req.Body);
var raw = await reader.ReadToEndAsync();
_logger.LogInformation($"RAW BODY = '{raw}'");
        var body = await JsonSerializer.DeserializeAsync<MyData>(req.Body);

        var connStr = _config.GetConnectionString("Sql");

        using var conn = new SqlConnection(connStr);
        await conn.OpenAsync();

        var cmd = new SqlCommand(
            "INSERT INTO TestData (Name) VALUES (@Name)", conn);

        cmd.Parameters.AddWithValue("@Name", body.Name);

        await cmd.ExecuteNonQueryAsync();

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await response.WriteStringAsync("Inserted!");
        return response;
    }
    catch (Exception ex)
    {
        var response = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
        await response.WriteStringAsync("ERROR: " + ex.ToString());
        return response;
    }
}

}

public class MyData
{
    public string Name { get; set; }
}
