using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var daprPort = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT") ?? "3500";
var stateStoreName = "statestore";
var stateUrl = $"http://localhost:{daprPort}/v1.0/state/{stateStoreName}";
var client = new HttpClient();

app.MapGet("/state/{key}", async (string key) =>
{
    Console.WriteLine("Request key: " + key);
    var res = await client.GetAsync(stateUrl + "/" + key);
    return res.Content.ReadAsStringAsync();
})
.WithName("GetState");

app.MapPost("/state", async (HttpContext context) =>
{
    var body = await JsonSerializer.DeserializeAsync<dynamic>(context.Request.Body);
    Console.WriteLine("Request body: " + body);
    using MemoryStream memoryStream = new();
    await JsonSerializer.SerializeAsync<dynamic>(memoryStream, body);
    var res = await client.PostAsync(stateUrl, new StringContent(Encoding.UTF8.GetString(memoryStream.ToArray()), Encoding.UTF8, "application/json"));
    await memoryStream.DisposeAsync();
    return res.StatusCode;
})
.WithName("SetState");

app.Run();