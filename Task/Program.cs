using Microsoft.Extensions.DependencyInjection;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Task_Interview.BackgroundJobs;
using Task_Interview.Data;
using Task_Interview.Services;
using Task_Interview.Validation;
using FluentValidation;
using FluentValidation.AspNetCore;
using HangfireBasicAuthenticationFilter;

namespace Task_Interview
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddScoped<StockTransferService>();
            builder.Services.AddScoped<LowStockNotifier>();

            builder.Services.AddDbContext<AppDbContext>(options =>
                                    options.UseSqlServer(builder.Configuration.GetConnectionString("DEV")));

            //hangfire 
            builder.Services.AddHangfire(config =>
                   config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DEV")));

            builder.Services.AddHangfireServer();

            //jwt token
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            builder.Services.AddSingleton<TokenService>(sp =>
                new TokenService(
                     secretKey: jwtSettings["SecretKey"]!,
                     issuer: jwtSettings["Issuer"]!,
                     audience: jwtSettings["Audience"]! 
                     ));
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!))
                    };
                });
            builder.Services.AddControllers()
                 .AddJsonOptions(options =>
                 {
                     options.JsonSerializerOptions.Converters.Add(new JsonDateTimeConverter("yyyy-MM-dd HH:mm:ss"));
                 });


            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();


            using (var scope = app.Services.CreateScope())
            {
                var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

                recurringJobManager.AddOrUpdate<LowStockNotifier>(
                    "low stock check",
                    notifier => notifier.NotifyLowStockItems(),
                    Cron.Minutely);

                recurringJobManager.AddOrUpdate<TransactionArchiver>(
                    "archive old transactions",
                    archiver => archiver.ArchiveOldTransactions(),
                    Cron.Daily);
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHangfireDashboard("/Hangfire" , new DashboardOptions
            {
                Authorization =
                [
                    new HangfireCustomBasicAuthenticationFilter
                    {
                        User = app.Configuration.GetValue<string>("HangireSetting:Username"),
                        Pass = app.Configuration.GetValue<string>("HangireSetting:Password")
                    }
                ],
                DashboardTitle = "Inventory Background jobs Dash"
            });
            app.UseHttpsRedirection();

            app.UseAuthorization(); 


            app.MapControllers();

            app.Run();
        }
    }
}
