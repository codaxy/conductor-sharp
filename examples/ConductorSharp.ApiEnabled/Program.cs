using ConductorSharp.ApiEnabled.Extensions;
using ConductorSharp.Engine.Health;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services
    .AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks().AddCheck<ConductorSharpHealthCheck>("running");
builder.Services.ConfigureApiEnabled(configuration);

//Autofac dependency injection
builder.Host.UseSerilog((ctx, services, lc) => lc.Enrich.FromLogContext().WriteTo.Seq("http://host.docker.internal:5341").WriteTo.Console());
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
