/*
 * Требуется разработать сервис, предоставляющий следующее REST API
 *
 *   1. POST /task
 *      Без параметров
 *      Создает запись в БД (любой) с сгенерированным GUID, текущим временем и статусов "created"
 *      Возвращает клиенту код 202 и GUID задачи
 *      Обновляет в БД для данного GUID текущее время и меняет статус на "running"
 *      Ждет 2 минуты
 *      Обновляет в БД для данного GUID текущее время и меняет статус на "finished"
 *   2. GET /task/{id}
 *      Параметр id: GUID созданной задачи
 *      Возвращает код 200 и статус запрошенной задачи:
 *      {
 *      }
 *      Возвращает 404, если такой задачи нет
 *      Возвращает 400, если передан не GUID
 *   Необходимо предоставить ссылку на исходный код.
 * 
 */


using Microsoft.EntityFrameworkCore;
using TaskRestApi.DbContexts;
using TaskRestApi.HostedServices;
using TaskRestApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TaskDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
});
builder.Services.AddScoped<TaskService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<TaskUpdaterHostedService>((provider) =>
{
    if (!int.TryParse(builder.Configuration["MillisecondsToFinishTask"], out var millisecondsToFinishTask))
    {
        millisecondsToFinishTask = 120000;
    }
    return new TaskUpdaterHostedService(
        provider.GetService<IBackgroundTaskQueue>()!,
        provider.GetService<ILogger<TaskUpdaterHostedService>>()!,
        provider.GetService<IServiceScopeFactory>()!,
        millisecondsToFinishTask
    );
});
builder.Services.AddSingleton<IBackgroundTaskQueue>(ctx =>
{
    if (!int.TryParse(builder.Configuration["QueueCapacity"], out var queueCapacity))
    {
        queueCapacity = 1000;
    }
    return new BackgroundTaskQueue(queueCapacity);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    using var context = services.GetRequiredService<TaskDbContext>();
    context.Database.EnsureCreated();
}

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