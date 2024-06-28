using LogIntelligence.Client.Extensions;
using LogIntelligence.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Logging.AddLogIntelligence(options => 
{
    options.ApiKey = Guid.Parse("db7c007f-a323-4d78-9342-4322b7403bbe");
    options.LogID = Guid.Parse("db7c007f-a323-4d78-9342-4322b7403bbe");
    options.Application = "AspNetCore 8.0 Example WebApp using LogIntelligence.Extensions.Logging";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
