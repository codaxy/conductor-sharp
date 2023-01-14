using Autofac.Extensions.DependencyInjection;
using ConductorSharp.Engine.Health;
using ConductorSharp.Engine.Util;
using ConductorSharp.Proxy.Extensions;
using Serilog;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services
    .AddControllers()
    .AddNewtonsoftJson(config =>
    {
        config.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(config =>
{
    config.CustomSchemaIds(x => x.FullName);
});
builder.Services.AddHealthChecks().AddCheck<ConductorSharpHealthCheck>("running");
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

//Autofac dependency injection
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory()).ConfigureProxy(configuration);
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
