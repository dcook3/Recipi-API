using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Recipi_API.Models;
using Recipi_API.Models.Data_Models;
using Recipi_API.Services;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// set correct issuer and audience in appsettings
string? HOST_ENV = System.Environment.GetEnvironmentVariable("ASPNETCORE_HOST_ENVIRONMENT");
HOST_ENV ??= "Docker";
builder.Configuration.AddJsonFile($"appsettings.{HOST_ENV}.json");


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
#pragma warning disable CS8604 // Possible null reference argument.
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateActor = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
#pragma warning restore CS8604 // Possible null reference argument.
});  
builder.Services.AddAuthentication();


builder.Services.AddControllers().AddJsonOptions(x => 
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

//configure swagger to take bearer token
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Bearer Authentication with JWT Token (Gotten from logging in)",
        Type = SecuritySchemeType.Http
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
    });
});
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IModeratorService, ModeratorService>();
builder.Services.AddSingleton<IPostInteractionsService, PostInteractionsService>();
builder.Services.AddSingleton<IPostService, PostService>();
builder.Services.AddSingleton<IPostFetchService, PostFetchService>();
builder.Services.AddSingleton<IRecipeService, RecipeService>();
builder.Services.AddSingleton<IIngredientsService, IngredientsService>();

var app = builder.Build();

builder.Services.AddCors();
app.UseCors(options =>
    options.AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();

app.MapControllers()
    .RequireAuthorization(policy =>
    {
        policy
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .RequireClaim("Id")
        .RequireClaim("Username")
        .RequireClaim("Email")
        .RequireClaim(ClaimTypes.Role, new string[] { "User", "Admin" })
        .Build();
    });

app.Run();
