using CloudinaryDotNet;
using DinkToPdf;
using DinkToPdf.Contracts;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using LogiSimEduProject_BE_API.Controllers.Cloudinary;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OfficeOpenXml;
using QuestPDF.Infrastructure;
using Repositories;
using Repositories.DBContext;
using Services;
using Services.IServices;
using Services.Jobs;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

// -----------------------
// 🔐 Firebase Initialization
// -----------------------
var firebasePath = "Credentials/logisimedu-firebase-adminsdk-fbsvc-cea79f44be.json";
if (File.Exists(firebasePath) && FirebaseApp.DefaultInstance == null)
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromFile(firebasePath)
    });
}

// -----------------------
// 🔧 Service Registrations
// -----------------------
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
    //options.JsonSerializerOptions.PropertyNamingPolicy = null; (PayOS)
});

builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<FirebaseStorageService>();

// Email & Cloudinary
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddSingleton<Cloudinary>(sp =>
{
    var config = sp.GetRequiredService<IOptions<CloudinarySettings>>().Value;
    var account = new Account(config.CloudName, config.ApiKey, config.ApiSecret);
    return new Cloudinary(account);
});

builder.Services.AddScoped<IPdfService, PdfService>();

// -----------------------
// 📦 Dependency Injection: Services + Repositories
// -----------------------
// Core Services
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<AccountRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();

ExcelPackage.License.SetNonCommercialPersonal("Phan Canh Tuan");

// Course & Learning Modules
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<CourseRepository>();
builder.Services.AddScoped<ICourseProgressService, CourseProgressService>();
builder.Services.AddScoped<CourseProgressRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<ClassRepository>();
builder.Services.AddScoped<TopicRepository>();
builder.Services.AddScoped<ITopicService, TopicService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ReviewRepository>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<LessonRepository>();
builder.Services.AddScoped<ILessonProgressService, LessonProgressService>();
builder.Services.AddScoped<LessonProgressRepository>();

// Organization + Workspace + Scenes + Scenarios + plan
builder.Services.AddScoped<OrganizationRepository>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
builder.Services.AddScoped<WorkspaceRepository>();
builder.Services.AddScoped<ISceneService, SceneService>();
builder.Services.AddScoped<SceneRepository>();
builder.Services.AddScoped<IScenarioService, ScenarioService>();
builder.Services.AddScoped<ScenarioRepository>();
builder.Services.AddScoped<IAccountOfWorkSpaceService, AccountOfWorkSpaceService>();
builder.Services.AddScoped<AccountOfWorkSpaceRepository>();
builder.Services.AddScoped<ISceneOfWorkSpaceService, SceneOfWorkSpaceService>();
builder.Services.AddScoped<SceneOfWorkSpaceRepository>();
builder.Services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();
builder.Services.AddScoped<SubscriptionPlanRepository>();

// Quiz & Submission
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<QuizRepository>();
builder.Services.AddScoped<QuestionRepository>();
builder.Services.AddScoped<IAnswerService, AnswerService>();
builder.Services.AddScoped<AnswerRepository>();
builder.Services.AddScoped<IQuizSubmissionService, QuizSubmissionService>();
builder.Services.AddScoped<QuizSubmissionRepository>();
builder.Services.AddScoped<ILessonSubmissionService, LessonSubmissionService>();
builder.Services.AddScoped<LessonSubmissionRepository>();

// Notification & Enrollment
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<NotificationRepository>();
builder.Services.AddScoped<IEnrollmentRequestService, EnrollmentRequestService>();
builder.Services.AddScoped<EnrollmentRequestRepository>();

// Certificate & Certificate Templete
builder.Services.AddScoped<ICertificateTemplateService, CertificateTemplateService>();
builder.Services.AddScoped<CertificateTemplateRepository>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<CertificateRepository>();

//Order & Payment
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<PaymentRepository>();

builder.Services.AddScoped<IDashboardService, DashboardService>();

// -----------------------
// 🧠 EF DbContext
// -----------------------
builder.Services.AddDbContext<LogiSimEduContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// -----------------------
// 🌍 CORS
// -----------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// -----------------------
// 🔐 Authentication: JWT
// -----------------------
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// -----------------------
// 📘 Swagger + JWT Support
// -----------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "LogiSimEduProject_BE_API", Version = "v1" });
    option.EnableAnnotations();
    option.DescribeAllParametersInCamelCase();
    option.ResolveConflictingActions(conf => conf.First());

    option.CustomSchemaIds(t => t.FullName);


    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});
builder.Services.AddHostedService<DailyExpireOrganizationsJob>();

// -----------------------
// 🚀 Build & Run App
// -----------------------
var app = builder.Build();

app.UseCors("AllowAllOrigins");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "LogiSimEduProject_BE_API v1");
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
