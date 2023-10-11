using System.Text.Json.Serialization;
using DSP_API.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);



// add view
builder.Services.AddControllersWithViews();
// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

// db context
var connectString = builder.Configuration.GetConnectionString("AppDb"); // chuuoi ket noi
builder.Services.AddDbContext<DspApiContext>(o =>
{
    o.UseSqlServer(connectString).LogTo(Console.WriteLine, LogLevel.None);
});


// add session

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{

});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// Use file
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(Path.Combine(
          Directory.GetCurrentDirectory(), "wwwroot"
     )),
    RequestPath = "/wwwroot"

});


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action}/");

// add session
app.UseSession();



app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
