using Autofac.Extensions.DependencyInjection;
using ConductorSharp.Engine.Health;
using ConductorSharp.Engine.Util;
using ConductorSharp.Proxy.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks().AddCheck<ConductorSharpHealthCheck>("running");

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
