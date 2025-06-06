using System.Globalization;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using CheckChildcareEligibility.Admin;
using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Domain.Validation;
using CheckChildcareEligibility.Admin.Infrastructure;
using CheckChildcareEligibility.Admin.Usecases;
using CheckChildcareEligibility.Admin.UseCases;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-GB");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-GB");

builder.Services.AddApplicationInsightsTelemetry();
if (Environment.GetEnvironmentVariable("CHILDCARE_ADMIN_KEY_VAULT_NAME") != null)
{
    var keyVaultName = Environment.GetEnvironmentVariable("CHILDCARE_ADMIN_KEY_VAULT_NAME");
    var kvUri = $"https://{keyVaultName}.vault.azure.net";

    builder.Configuration.AddAzureKeyVault(
        new Uri(kvUri),
        new DefaultAzureCredential(),
        new AzureKeyVaultConfigurationOptions
        {
            ReloadInterval = TimeSpan.FromSeconds(60 * 10)
        }
    );
}

// Add services to the container.
builder.Services.AddServices(builder.Configuration);
builder.Services.AddSession();

builder.Services.AddScoped<ILoadParentDetailsUseCase, LoadParentDetailsUseCase>();
builder.Services.AddScoped<IPerform2YoEligibilityCheckUseCase, Perform2YoEligibilityCheckUseCase>();
builder.Services.AddScoped<IPerformEyppEligibilityCheckUseCase, PerformEyppEligibilityCheckUseCase>();
builder.Services.AddScoped<IGetCheckStatusUseCase, GetCheckStatusUseCase>();
builder.Services.AddScoped<IValidateParentDetailsUseCase, ValidateParentDetailsUseCase>();
builder.Services.AddScoped<IParseBulkCheckFileUseCase, ParseBulkCheckFileUseCase>();
builder.Services.AddScoped<IValidator<CheckEligibilityRequestData>, CheckEligibilityRequestDataValidator>();
builder.Services.AddSession();



var dfeSignInConfiguration = new DfeSignInConfiguration();
builder.Configuration.GetSection("DfeSignIn").Bind(dfeSignInConfiguration);
builder.Services.AddDfeSignInAuthentication(dfeSignInConfiguration);

//builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
//builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Error");

app.Use(async (ctx, next) =>
{
    await next();
    if (ctx.Response.StatusCode == 404 && !ctx.Response.HasStarted)
    {
        //Re-execute the request so the user gets the error page
        ctx.Request.Path = "/Error/NotFound";
        await next();
    }
});

app.MapHealthChecks("/healthcheck");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();
app.Use((context, next) =>
{
    context.Response.Headers["strict-transport-security"] = "max-age=31536000; includeSubDomains";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self' https://*.clarity.ms https://c.bing.com";
    context.Response.Headers["X-Frame-Options"] = "sameorigin";
    context.Response.Headers["Cache-Control"] = "Private";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    return next.Invoke();
});
app.UseSession();
app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");


app.Run();