using MyRACIT.Data;
using MyRACIT.Services;
using MyRACIT.Strategies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Реєстрація DbContext
builder.Services.AddDbContext<MyRacitDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Реєстрація Generic Repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Реєстрація сервісів
builder.Services.AddScoped<GradeService>();

// Реєстрація стратегій (за замовчуванням - середній бал)
builder.Services.AddScoped<IGradeStrategy, AverageGradeStrategy>();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Додаємо CoreAdmin - автоматична адмін-панель для CRUD операцій
// ПРИМІТКА: CoreAdmin вимагає авторизації. Для демонстрації використовуємо власні контролери.
// builder.Services.AddCoreAdmin();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// CoreAdmin - доступ через /admin
// ПРИМІТКА: CoreAdmin закоментовано через вимогу авторизації
// app.UseCoreAdminCustomUrl("admin");

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
