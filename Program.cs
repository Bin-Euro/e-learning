using AutoMapper;
using Cursus.Data;
using Cursus.Data.Interface;
using Cursus.Entities;
using Cursus.GlobalExceptionHandler;
using Cursus.ObjectMapping;
using Cursus.Repositories;
using Cursus.Repositories.Interfaces;
using Cursus.Services;
using Cursus.Services.Interfaces;
using Cursus.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.Text;
using Cursus.DTO;
using CodeMegaVNPay.Services;
using payment.Services;

var builder = WebApplication.CreateBuilder(args);

// connect mongodb
builder.Services.Configure<CartDatabaseSettings>(
    builder.Configuration.GetSection(nameof(CartDatabaseSettings)));

builder.Services.AddSingleton<ICartDatabaseSettings>(sp =>
    sp.GetRequiredService<IOptions<CartDatabaseSettings>>().Value);

builder.Services.AddSingleton<IMongoClient>(s =>
    new MongoClient(builder.Configuration.GetValue<string>("DatabaseSettings:ConnectionString")));
builder.Services.AddScoped<ICartDbContext, CartDbContext>();
//....

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(options =>
        options.WithOrigins(builder.Configuration["AllowedHosts"].Split())
            .AllowAnyMethod()
            .AllowAnyHeader());
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter the Bearer Authorization string as following: `Bearer Generated-JWT-Token`",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme()
            {
                In = ParameterLocation.Header,
                Name = "Bearer",
                Reference = new OpenApiReference()
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                },
            },
            new List<string>()
        }
    });
});

builder.Services.AddDbContext<MyDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("MyDbContext"));
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionHandler>();
});

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<MyDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })

    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JWT:ValidAudience"],
            ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
        };
        options.Events = new JwtBearerEvents()
        {
            OnChallenge = context =>
            {
                var ex = context.AuthenticateFailure;

                context.Response.OnStarting(async () =>
                {
                    var message = "";
                    if (ex is not null)
                    {
                        message = "Invalid token";
                        if (ex.GetType() == typeof(SecurityTokenExpiredException))
                            message = "Expired token";
                    }

                    context.Response.WriteAsJsonAsync(ResultDTO<string>.Fail(message, context.Response.StatusCode));
                });

                return Task.CompletedTask;
            },
            OnForbidden = context =>
            {
                context.Response.OnStarting(async () =>
                {
                    await context.Response.WriteAsJsonAsync(ResultDTO<string>.Fail(
                            "You are not allow to access this resource", context.Response.StatusCode
                        )
                    );
                });

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartItemRepository, CartItemRepository>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddAutoMapper(typeof(CursusAutoMapperProfile).Assembly);

builder.Services.AddTransient<ICourseService, CourseService>();
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<ITokenService, TokenService>();
builder.Services.AddTransient<ICourseCatalogService, CourseCatalogService>();
builder.Services.AddTransient<ICatalogService, CatalogService>();
builder.Services.AddTransient<ICartService, CartService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IGoogleService, GoogleService>();
builder.Services.AddScoped<IVnPayService, VnPayService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Support APP API");
    });
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();