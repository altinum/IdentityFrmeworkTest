using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebAppIdentityTest.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();
//builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapControllers();
app.UseDefaultFiles();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = (context) =>
    {
        if (context.Context.User.Identity.IsAuthenticated)
        {
            //A kérés küldője be van jelentkezve
            if (context.Context.Request.Path.Value == "/index.html")
            {
                context.Context.Response.Redirect("/titkos/logged_in.html");
            }
        }
        else
        {
            //A kérés küldője nincs bejelentkezve
            if (context.Context.Request.Path.Value.Contains("titkos"))
            {
                context.Context.Response.Redirect("/index.html");
            }
        }
    }
});

app.UseRouting();

app.UseAuthorization();

//app.MapRazorPages();

app.Run();
