using Microsoft.EntityFrameworkCore;
using TemperatureApi.Data;
using TemperatureApi.Repositories;
using TemperatureApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- Dịch vụ (Dependency Injection) ----------

// EF Core + SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository
builder.Services.AddScoped<ITemperatureRepository, TemperatureRepository>();
builder.Services.AddScoped<IAlertRepository, AlertRepository>();

// Service
builder.Services.AddScoped<ITemperatureService, TemperatureService>();
builder.Services.AddScoped<IAlertService, AlertService>();

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS - cho phép Web/WinForm/Mobile gọi API từ origin khác.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// ---------- Tự động tạo Database khi khởi động ----------
// Phù hợp với môi trường đồ án: chạy là có DB ngay, không cần migrate thủ công.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// ---------- Pipeline ----------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

// Phục vụ Web Dashboard tĩnh trong wwwroot (index.html).
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
