using authservice.dataaccess.Data;
using authservice.dataaccess.Services;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("secrets/appsettings.secrets.json", true);

// Add db connection

var conStrBuilder = new MySqlConnectionStringBuilder(
    builder.Configuration.GetConnectionString("DBConnectionString"));
var connection = conStrBuilder.ConnectionString;

builder.Services.AddDbContext<AuthContext>(
    options => options.UseMySql(connection, ServerVersion.AutoDetect(connection)));

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<IDataAccessService, DataAccessService>();
builder.Services.AddSingleton<IMessagingService, MessagingService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

var dataAccessService = app.Services.GetRequiredService<IDataAccessService>();
dataAccessService.SubscribeToPersistence();

app.Run();
