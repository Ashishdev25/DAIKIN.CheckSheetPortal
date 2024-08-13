using AutoMapper;
using DAIKIN.CheckSheetPortal.API.Setup;
using DAIKIN.CheckSheetPortal.DataAccess;
using DAIKIN.CheckSheetPortal.Entities;
using DAIKIN.CheckSheetPortal.Infrastructure;
using DAIKIN.CheckSheetPortal.Infrastructure.DataAccess;
using DAIKIN.CheckSheetPortal.Infrastructure.Services;
using DAIKIN.CheckSheetPortal.Services;
using DAIKIN.CheckSheetPortal.Utils;
using Microsoft.AspNetCore.Server.HttpSys;
using MongoDB.Driver;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext().
    CreateLogger();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureSwagger();
builder.Services.ConfigureAuth(builder.Configuration);
builder.Host.UseSerilog();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICacheService, CacheService>();

var config = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfiles()));
var mapper = config.CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("MongoDb");
builder.Services.AddScoped<IRepository<Configuration>>(provider => new Repository<Configuration>(connectionString, "CheckSheetPortalDB", "configuration"));
builder.Services.AddScoped<IRepository<CheckSheet>>(provider => new Repository<CheckSheet>(connectionString, "CheckSheetPortalDB", "check_sheets"));
builder.Services.AddScoped<IRepository<User>>(provider => new Repository<User>(connectionString, "CheckSheetPortalDB", "users")); 
builder.Services.AddScoped<IRepository<MasterSettings>>(provider => new Repository<MasterSettings>(connectionString, "CheckSheetPortalDB", "settings"));
builder.Services.AddScoped<ICheckSheetTransactionRepository>(provider => new CheckSheetTransactionRepository(connectionString, "CheckSheetPortalDB", "check_sheet_transactions"));
builder.Services.AddScoped<IRepository<CheckSheetTransactionArchive>>(provider => new Repository<CheckSheetTransactionArchive>(connectionString, "CheckSheetPortalDB", "check_sheet_transactions_archive"));
builder.Services.AddScoped<ICheckSheetVersionRepository>(provider => new CheckSheetVersionRepository(connectionString, "CheckSheetPortalDB", "check_sheet_versions"));
builder.Services.AddScoped<IRepository<CheckSheetEmail>>(provider => new Repository<CheckSheetEmail>(connectionString, "CheckSheetPortalDB", "check_sheet_emails"));

builder.Services.AddTransient<IConfigurationService, ConfigurationService>();
builder.Services.AddTransient<ICheckSheetService, CheckSheetService>();
builder.Services.AddTransient<ICheckSheetTransactionService, CheckSheetTransactionService>();
builder.Services.AddTransient<ICheckSheetVersionService, CheckSheetVersionService>();
builder.Services.AddTransient<ICheckSheetImageService, CheckSheetImageService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IMasterSettingsService, MasterSettingsService>();
builder.Services.AddTransient<ITestService, TestService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService<User>>(); 
builder.Services.AddSingleton<JwtManager>();
builder.Services.AddAuthentication(HttpSysDefaults.AuthenticationScheme);
builder.Services.AddSingleton<StartupTableCreator>(sp =>
{
    var mongoClient = new MongoClient(connectionString);
    var mongoDatabase = mongoClient.GetDatabase("CheckSheetPortalDB");

    return new StartupTableCreator(mongoDatabase, builder.Environment.EnvironmentName, mapper);
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RoleAccess", policy => policy.RequireRole("Operator", "Validator", "Creator", "Reviewer", "Approver", "SuperAdmin"));
});

builder.Services.Configure<AppSettings>(options =>
{
    options.Env = builder.Environment.EnvironmentName;
    options.FolderPath = builder.Configuration.GetValue<string>("AppSettings:FolderPath");
    options.URL = builder.Configuration.GetValue<string>("AppSettings:URL");
    options.ApiKey = builder.Configuration.GetValue<string>("AppSettings:ApiKey");
});

var app = builder.Build();
app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed(origin => true).AllowCredentials());

if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var tableCreator = services.GetRequiredService<StartupTableCreator>();
    try
    {
        await tableCreator.CreateAndPopulateTables();
        tableCreator.CreateIndexes();
        Console.WriteLine("Startup tables created and populated successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine("An error occurred while creating and populating startup tables: " + ex.Message);
    }
}

app.Run();
