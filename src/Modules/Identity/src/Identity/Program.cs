﻿using BuildingBlocks.CAP;
using BuildingBlocks.Domain;
using BuildingBlocks.Exception;
using BuildingBlocks.Jwt;
using BuildingBlocks.Logging;
using BuildingBlocks.MediatR;
using BuildingBlocks.Web;
using Figgle;
using Hellang.Middleware.ProblemDetails;
using Identity;
using Serilog;
using BuildingBlocks.OpenApi;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var env = builder.Environment;

var appOptions = builder.Services.GetOptions<AppOptions>("AppOptions");
Console.WriteLine(FiggleFonts.Standard.Render(appOptions.Name));

builder.AddCustomSerilog(env);
builder.Services.AddJwt();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCustomCap();
builder.Services.AddTransient<IBusPublisher, BusPublisher>();
builder.Services.AddCustomVersioning();

builder.Services.AddAspnetOpenApi();

builder.Services.AddCustomProblemDetails();

builder.Services.AddIdentityModules(configuration, env);

builder.Services.AddEasyCaching(options => { options.UseInMemory(configuration, "mem"); });

builder.Services.AddCustomMediatR(
    typeof(IdentityRoot).Assembly
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseAspnetOpenApi();
}

app.UseSerilogRequestLogging();
app.UseCorrelationId();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();

app.UseIdentityModules();

app.UseProblemDetails();

app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

app.MapGet("/", x => x.Response.WriteAsync(appOptions.Name));

app.Run();
