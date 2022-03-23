using Autofac;
using Autofac.Extensions.DependencyInjection;
using ConductorSharp.ApiEnabled;
using ConductorSharp.ApiEnabled.Extensions;
using ConductorSharp.Engine.Extensions;
using MediatR.Extensions.Autofac.DependencyInjection;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;


var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;


// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//Autofac dependency injection
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureApiEnabled(configuration);


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseAuthorization();
app.MapControllers();
app.Run();
