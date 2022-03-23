using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using XgsPon.Workflows.Engine.Extensions;
using Autofac.Extensions.DependencyInjection;
using Autofac;
using XgsPon.MediatR;
using XgsPon.AutofacModules;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;


// Add services to the container.
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Autofac dependency injection 
builder.Host
    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(
        builder =>
        {
            // Declare your services with proper lifetime
            builder
                .AddWorkflowEngine(
                    baseUrl: configuration.GetValue<string>("Conductor:BaseUrl"),
                    apiPath: configuration.GetValue<string>("Conductor:ApiUrl"),
                    preventErrorOnBadRequest: configuration.GetValue<bool>(
                        "Conductor:PreventErrorOnBadRequest"
                    ),
                    createClient: () =>
                    {
                        var client = new RestClient();
                        client.UseNewtonsoftJson();

                        return client;
                    }
                )
                .AddExecutionManager(
                    maxConcurrentWorkers: configuration.GetValue<int>(
                        "Conductor:MaxConcurrentWorkers"
                    ),
                    sleepInterval: configuration.GetValue<int>("Conductor:SleepInterval"),
                    longPollInterval: configuration.GetValue<int>("Conductor:LongPollInterval"),
                    domain: configuration.GetValue<string>("Conductor:WorkerDomain")
                );
            builder.RegisterModule(new MediatorModule(typeof(Program).Assembly));
            builder.RegisterModule<ConductorTaskDefinitionsModule>();
            builder.RegisterModule<ConductorWorkflowDefinitionsModule>();
        }
    );


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();


app.UseAuthorization();


app.MapControllers();


app.Run();
