using BlazorAPIProject.DataAccess.DataContext;
using BlazorAPIProject.Mappings;
using BlazorAPIProject.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile));
builder.Services.AddSingleton<HashingHelper>();

// JWT Authentication
var signingKey = builder.Configuration["Authentication:Bearer:SigningKey"];
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        SecurityKey? key = null;
        if (!string.IsNullOrWhiteSpace(signingKey))
        {
            key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(signingKey));
        }
        o.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = key,
            ValidateIssuerSigningKey = key is not null,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(60),
            ValidateAudience = false,
            ValidateIssuer = false,
        };
        o.MapInboundClaims = false;
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "api");
    });
}

// Global exception handler: returns the real exception message + stack trace as JSON
// in Development, and a generic 500 response in Production.
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        if (app.Environment.IsDevelopment() && exception is not null)
        {
            await context.Response.WriteAsJsonAsync(new
            {
                error = exception.Message,
                type = exception.GetType().Name,
                stackTrace = exception.StackTrace,
                inner = exception.InnerException?.Message
            });
        }
        else
        {
            await context.Response.WriteAsJsonAsync(new
            {
                error = "An unexpected error occurred."
            });
        }
    });
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
