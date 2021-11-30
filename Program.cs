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

app.UseHttpsRedirection();

var daprPort = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT") ?? "3500";
var stateStoreName = "statestore";
var stateUrl = $"http://localhost:${daprPort}/v1.0/state/${stateStoreName}";
var client = new HttpClient();

app.MapGet("/{key}", async (string key) =>
{
    return await client.GetAsync(stateUrl + "/" + key);
})
.WithName("GetState");

app.MapPost("/", async (string req) =>
{
    return await client.PostAsJsonAsync(stateUrl, req);
})
.WithName("SetState");

app.Run();