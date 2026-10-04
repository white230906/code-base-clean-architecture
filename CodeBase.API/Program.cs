using CodeBase.API.Extensions;
using CodeBase.Application;
using CodeBase.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPresentation(builder.Configuration);

var app = builder.Build();

app.UseApiPipeline();

app.Run();

public partial class Program;
