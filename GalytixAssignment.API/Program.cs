
using FluentValidation;
using GalytixAssignment.API.Middleware;
using GalytixAssignment.API.Models;
using GalytixAssignment.API.Repositories;
using GalytixAssignment.API.Services;
using GalytixAssignment.API.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddMemoryCache();

//Custom Service registrations
builder.Services.AddScoped<IValidator<CountryGwpRequest>, CountryGwpRequestValidator>();
builder.Services.AddSingleton<ICountryGwpRepository, CountryGwpRepository>();
builder.Services.AddScoped<ICountryGwpService, CountryGwpService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
