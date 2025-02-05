using LogIntelligence.Client;
using LogIntelligence.Client.Extensions;
using LogIntelligence.Extensions.Logging;
using Microsoft.Extensions.Options;
using LogIntelligence.Extensions.Logging.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
// Register LogIntelligenceOptions from configuration
//builder.Logging.Services.Configure<LogIntelligenceOptions>(builder.Configuration.GetSection("LogIntelligence"));

//builder.Services.AddLogIntelligenceClient(options =>
//{
//    options.ApiKey = Guid.Parse("db7c007f-a323-4d78-9342-4322b7403bbe");
//    options.LogID = Guid.Parse("db7c007f-a323-4d78-9342-4322b7403bbe");
//    options.Application = "AspNetCore 8.0 Example WebApp using LogIntelligence.Extensions.Logging";
//});

// Configure logging to use LogIntelligenceLoggerProvider
//builder.Logging.ClearProviders();
//builder.Logging.AddProvider(new LogIntelligenceLoggerProvider(
//    builder.Services.BuildServiceProvider().GetRequiredService<LogIntelligenceClient>(),
//    builder.Services.BuildServiceProvider().GetRequiredService<IOptions<LogIntelligenceOptions>>().Value));

//builder.Logging.AddLogIntelligence();
builder.Services.AddOptions<LogIntelligenceOptions>().Bind(builder.Configuration.GetSection("LogIntelligence"));
//builder.Logging.ClearProviders();
builder.Logging.AddLogIntelligence(options =>
{
    options.ApiKey = Guid.Parse("db7c007f-a323-4d78-9342-4322b7403bbe");
    options.LogID = Guid.Parse("db7c007f-a323-4d78-9342-4322b7403bbe");
    options.Application = "AspNetCore 8.0 Example WebApp using LogIntelligence.Extensions.Logging";
});

//Configure the LogIntelligence logger using configuration from appsettings.json
//builder.Logging.AddLogIntelligence(options =>
//{
//    builder.Configuration.GetSection("LogIntelligence");
//});

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
