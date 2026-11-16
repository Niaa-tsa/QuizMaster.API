using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using QuizMaster.API.Middleware;
using QuizMaster.API.Seed;
using QuizMaster.API.Storage;
using QuizMaster.Domain.Entities;
using QuizMaster.Infrastructure.Data;
using QuizMaster.Infrastructure.Identity;
using QuizMaster.Infrastructure.Repositories;
using QuizMaster.Infrastructure.Validators;
using QuizMaster.Service.Interfaces;
using QuizMaster.Service.Interfaces.Repositories;
using QuizMaster.Service.Services;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Controllers & Validation
builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<CreateQuestionDTOValidator>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500", "http://localhost:5500", "https://localhost:5001", "http://localhost:5000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Swagger + JWT Authorization
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Description = "Enter your JWT token."
    });

    options.AddSecurityRequirement(document =>
        new Microsoft.OpenApi.OpenApiSecurityRequirement
        {
            [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", document)] = []
        });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

// Database
builder.Services.AddDbContext<QuizMasterDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// Repository registrations
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IAnswerOptionRepository, AnswerOptionRepository>();
builder.Services.AddScoped<IQuizAttemptRepository, QuizAttemptRepository>();
builder.Services.AddScoped<IStudentAnswerRepository, StudentAnswerRepository>();
builder.Services.AddScoped<IQuizResultRepository, QuizResultRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Password hasher and authentication service
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Service registrations
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IAnswerOptionService, AnswerOptionService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddSingleton<IProfilePhotoStorage, LocalProfilePhotoStorage>();
builder.Services.AddScoped<IQuizAttemptService, QuizAttemptService>();
builder.Services.AddScoped<IStudentAnswerService, StudentAnswerService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ISuperAdminService, SuperAdminService>();

// JWT configuration
var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey) ||
    Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "JWT key is missing or shorter than 32 characters. " +
        "Set it with: dotnet user-secrets set \"Jwt:Key\" \"<random string>\" " +
        "--project QuizMaster.API");
}
var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT Issuer is missing.");
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT Audience is missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
   .AddJwtBearer(options =>
   {
       options.TokenValidationParameters = new TokenValidationParameters
       {
           ValidateIssuer = true,
           ValidIssuer = jwtIssuer,

           ValidateAudience = true,
           ValidAudience = jwtAudience,

           ValidateLifetime = true,
           ValidateIssuerSigningKey = true,

           IssuerSigningKey = new SymmetricSecurityKey(
               Encoding.UTF8.GetBytes(jwtKey)
           ),

           ClockSkew = TimeSpan.Zero
       };

       // CHECK TOKEN VERSION
       options.Events = new JwtBearerEvents
       {
           OnTokenValidated = async context =>
           {
               var userIdClaim = context.Principal?
                   .FindFirst(ClaimTypes.NameIdentifier)?.Value;

               var tokenVersionClaim = context.Principal?
                   .FindFirst("TokenVersion")?.Value;

               if (!int.TryParse(userIdClaim, out var userId) ||
                   !int.TryParse(tokenVersionClaim, out var tokenVersion))
               {
                   context.Fail("Invalid token.");
                   return;
               }

               var dbContext = context.HttpContext.RequestServices
                   .GetRequiredService<QuizMasterDbContext>();
               var userData = await dbContext.Users
                   .AsNoTracking()
                   .Where(u => u.Id == userId)
                   .Select(u => new
                   {
                       u.TokenVersion,
                       u.IsDeleted
                   })
                   .FirstOrDefaultAsync();

               if (userData == null ||
                   userData.IsDeleted ||
                   userData.TokenVersion != tokenVersion)
               {
                   context.Fail("Token has been revoked.");
               }

           }
       };
   });

builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware Pipeline
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

await DbSeeder.SeedAsync(app.Services);

app.MapControllers();

app.Run();