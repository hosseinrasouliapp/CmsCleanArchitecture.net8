using System.Text;
using Cms.Api.Presenter;
using Cms.Core.Interfaces.Repository;
using Cms.Core.Queries;
using Cms.Core.UseCase;
using Cms.Infrastructure.Data;
using Cms.Infrastructure.Data.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// تنظیمات خدمات
builder.Services.AddDbContext<CmsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CmsDatabase")));

builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddMediatR(typeof(Program));
builder.Services.AddMediatR(typeof(GetLatestPostsQuery).Assembly);

builder.Services.AddScoped<IEditPostUseCase, EditPostUseCase>();
builder.Services.AddScoped(typeof(PostApiPresenter<>));
builder.Services.AddControllers();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(option =>
    {
        option.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration.GetSection("Appsettings:SecretKey").Value)),
            ValidateIssuer = false,
            ValidIssuer = builder.Configuration.GetSection("Appsettings:ValidIssuer").Value,
            ValidateAudience = false,
            ValidAudience = builder.Configuration.GetSection("Appsettings:ValidAudience").Value,
            ValidateLifetime = false
        };
    });

// اضافه کردن خدمات Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});


var app = builder.Build();

// تنظیمات لوله میانی
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty; // برای دسترسی به Swagger UI در ریشه
    });
}


app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();