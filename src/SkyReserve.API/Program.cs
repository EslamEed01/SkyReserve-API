using Amazon;
using Amazon.SimpleEmail;
using Amazon.SQS;
using Elastic.Clients.Elasticsearch;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SkyReserve.API.Extensions;
using SkyReserve.Application.Flight.DTOS;
using SkyReserve.Application.Helpers;
using SkyReserve.Application.Interfaces;
using SkyReserve.Application.Repository;
using SkyReserve.Application.Services;
using SkyReserve.Application.Settings;
using SkyReserve.Domain.Entities;
using SkyReserve.Infrastructure.Authentication;
using SkyReserve.Infrastructure.Authorization;
using SkyReserve.Infrastructure.Email;
using SkyReserve.Infrastructure.Persistence;
using SkyReserve.Infrastructure.Repository;
using SkyReserve.Infrastructure.Repository.implementation;
using SkyReserve.Infrastructure.Services;
using SkyReserve.Infrastructure.SMS;
using StackExchange.Redis;
using Stripe;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace SkyReserve.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // Add DbContext configuration
            builder.Services.AddDbContext<SkyReserveDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
                       .AddInterceptors(new QueryTimingInterceptor(), new QueryInterceptor()));

            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
                     .AddEntityFrameworkStores<SkyReserveDbContext>()
                     .AddDefaultTokenProviders();

            // Register FluentValidation
            builder.Services.AddValidatorsFromAssembly(typeof(SkyReserve.Application.Flight.DTOS.FlightDto).Assembly);

            #region Swagger Config
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SkyReserve API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.OperationFilter<AuthResponsesOperationFilter>();
            });


            builder.Services.AddSwaggerGen(c =>
            {
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                
                if (System.IO.File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
            });

            #endregion



            #region Google OAuth Config

            builder.Services.Configure<GoogleOAuthSettings>(
                builder.Configuration.GetSection(GoogleOAuthSettings.SectionName));

            // Register HttpClient for OAuth
            builder.Services.AddHttpClient<GoogleOAuthService>();

            // Register Google OAuth service
            builder.Services.AddScoped<IGoogleOAuthService, GoogleOAuthService>();

            #endregion

            #region MediatR Config

            var mediatrLicenseKey = builder.Configuration["MediatR:LicenseKey"];
            var automapperLicenseKey = builder.Configuration["AutoMapper:LicenseKey"];

            builder.Services.AddMediatR(cfg =>
            {
                if (!string.IsNullOrEmpty(mediatrLicenseKey))
                {
                    cfg.LicenseKey = mediatrLicenseKey;
                }

                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                cfg.RegisterServicesFromAssembly(typeof(SkyReserve.Application.Flight.DTOS.FlightDto).Assembly);
            });

            #endregion

            #region AWS Config

            builder.Services.Configure<SESsettings>(
                builder.Configuration.GetSection(SESsettings.SectionName));

            // Register AWS SES client
            builder.Services.AddScoped<IAmazonSimpleEmailService>(provider =>
            {
                var awsSettings = builder.Configuration.GetSection(SESsettings.SectionName).Get<SESsettings>();

                if (!string.IsNullOrEmpty(awsSettings?.AccessKey) && !string.IsNullOrEmpty(awsSettings.SecretKey))
                {
                    return new AmazonSimpleEmailServiceClient(
                        awsSettings.AccessKey,
                        awsSettings.SecretKey,
                        RegionEndpoint.GetBySystemName(awsSettings.Region));
                }
                else
                {
                    return new AmazonSimpleEmailServiceClient(RegionEndpoint.GetBySystemName(awsSettings?.Region ?? "us-east-1"));
                }
            });

            #endregion

            #region Twilio SMS Config

            builder.Services.Configure<TwilioSettings>(
                builder.Configuration.GetSection(TwilioSettings.SectionName));

            #endregion

            #region AWS SQS Config

            builder.Services.Configure<SqsSettings>(
                builder.Configuration.GetSection(SqsSettings.SectionName));

            // Register AWS SQS client
            builder.Services.AddSingleton<IAmazonSQS>(provider =>
            {
                var sqsSettings = builder.Configuration.GetSection(SqsSettings.SectionName).Get<SqsSettings>();

                if (!string.IsNullOrEmpty(sqsSettings?.AccessKey) && !string.IsNullOrEmpty(sqsSettings.SecretKey))
                {
                    return new AmazonSQSClient(
                        sqsSettings.AccessKey,
                        sqsSettings.SecretKey,
                        Amazon.RegionEndpoint.GetBySystemName(sqsSettings.Region));
                }
                else
                {
                    return new AmazonSQSClient(Amazon.RegionEndpoint.GetBySystemName(sqsSettings?.Region ?? "us-east-1"));
                }
            });

            #endregion

            #region Elasticsearch Config

            builder.Services.Configure<ElasticsearchSettings>(
                builder.Configuration.GetSection(ElasticsearchSettings.SectionName));

            // Register Elasticsearch client
            builder.Services.AddSingleton<ElasticsearchClient>(provider =>
            {
                var elasticsearchSettings = builder.Configuration
                    .GetSection(ElasticsearchSettings.SectionName)
                    .Get<ElasticsearchSettings>();

                var url = elasticsearchSettings?.Url ?? "http://localhost:9201";
                var defaultIndex = elasticsearchSettings?.DefaultIndex ?? "flights";
                //var serverVersion = elasticsearchSettings?.ServerVersion ?? "8.15.0";

                // Validate the URL
                if (!Uri.TryCreate(url, UriKind.Absolute, out var elasticsearchUri))
                {
                    throw new InvalidOperationException($"Invalid Elasticsearch URL: {url}");
                }

                // Create the settings with proper version compatibility
                var settings = new ElasticsearchClientSettings(elasticsearchUri)
                    .DefaultIndex(defaultIndex)
                    .DisableDirectStreaming() // For debugging - can be removed in production
                    .RequestTimeout(TimeSpan.FromSeconds(30)); // Add timeout configuration
                    //.ApiVersioningHeader(ApiVersioningOptions.ServerDefault); // Use server's default API version

                // Configure default mappings for your domain entities
                settings = settings.DefaultMappingFor<FlightDto>(m => m.IndexName("flights"));

                return new ElasticsearchClient(settings);
            });

            #endregion

            #region register services
            // Register repositories
            builder.Services.AddScoped<IFlightRepository, FlightRepository>();
            builder.Services.AddScoped<IPassengerRepository, PassengerRepository>();
            builder.Services.AddScoped<IBookingRepository, BookingRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<IStripePayment, StripePaymentService>();
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
            builder.Services.AddScoped<IReviewService, Application.Services.ReviewService>();
            builder.Services.AddScoped<IPriceRepository, PriceRepository>();
            builder.Services.AddScoped<EmailNotificationSender>();
            builder.Services.AddScoped<SmsNotificationSender>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
            builder.Services.AddScoped<INotificationProducer, NotificationProducer>();
            builder.Services.AddScoped<IRedisService, RedisService>();
            builder.Services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ITemplateService, TemplateService>();
            builder.Services.AddScoped<IElasticsearchService, ElasticsearchService>();
            builder.Services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();
            builder.Services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IJwtProvider, JwtProvider>();
            builder.Services.AddOptions<JwtOptions>()
                        .BindConfiguration(JwtOptions.SectionName)
                        .ValidateDataAnnotations()
                        .ValidateOnStart();

            var jwtSettings = builder.Configuration
                                     .GetSection(JwtOptions.SectionName).Get<JwtOptions>();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.SaveToken = true;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,   
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.Key!)),
                    ValidIssuer = jwtSettings?.Issuer,
                    ValidAudience = jwtSettings?.Audience,
                    RoleClaimType = ClaimTypes.Role,
                };
            });
            #endregion

            builder.Services.AddSwaggerGen();
            builder.Services.AddOpenApi();

            if (!string.IsNullOrEmpty(automapperLicenseKey))
            {
                builder.Services.AddAutoMapper(cfg =>
                {
                    cfg.LicenseKey = automapperLicenseKey;
                }, typeof(Program).Assembly, typeof(SkyReserve.Application.Flight.DTOS.FlightDto).Assembly);
            }

            // Register the background service
            builder.Services.AddHostedService<NotificationBackgroundService>();
            // Redis configuration
            builder.Services.Configure<RedisSettings>(
                builder.Configuration.GetSection(RedisSettings.SectionName));

            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<RedisSettings>>().Value;
                return ConnectionMultiplexer.Connect(settings.ConnectionString);
            });

            var app = builder.Build();

            app.UseGlobalExceptionHandling();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowAll");

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var elasticsearchService = scope.ServiceProvider.GetRequiredService<IElasticsearchService>();

            }

            app.Run();
        }
    }
}
