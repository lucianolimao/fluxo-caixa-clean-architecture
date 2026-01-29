using FluxoCaixa.API.Workers;
using FluxoCaixa.Application.Interfaces;
using FluxoCaixa.Application.UseCases;
using FluxoCaixa.Domain.Interfaces;
using FluxoCaixa.Infrastructure.Data;
using FluxoCaixa.Infrastructure.Messaging;
using FluxoCaixa.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "FluxoCaixa API", 
        Version = "v1",
        Description = "API para controle de fluxo de caixa diário"
    });
});

builder.Services.AddDbContext<FluxoCaixaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ILancamentoRepository, LancamentoRepository>();
builder.Services.AddScoped<IConsolidadoDiarioRepository, ConsolidadoDiarioRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddSingleton<IMessagePublisher>(sp =>
{
    var rabbitMqHost = builder.Configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost";
    return new RabbitMqMessagePublisher(rabbitMqHost);
});

builder.Services.AddScoped<CriarLancamentoUseCase>();
builder.Services.AddScoped<ObterLancamentosUseCase>();
builder.Services.AddScoped<ObterConsolidadoDiarioUseCase>();
builder.Services.AddScoped<ProcessarConsolidacaoUseCase>();

builder.Services.AddHostedService<ConsolidacaoWorker>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();