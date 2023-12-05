using Firebase.Auth;
using Firebase.Auth.Providers;
using FirebaseAdmin;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using MongoDB.Driver;
using userservice.Repositories;
using userservice.Services;
using authentication;

var builder = WebApplication.CreateBuilder(args);

if (FirebaseApp.DefaultInstance == null)
{
    builder.Services.AddSingleton(FirebaseApp.Create());
}

builder.Services.AddSingleton<IMongoClient>(s =>
        new MongoClient(builder.Configuration.GetValue<string>("MongoDbSettings:ConnectionString")));

builder.Services.AddSingleton<IFirebaseAuthClient>(s => new FirebaseAuthClient(new FirebaseAuthConfig
{
    ApiKey = builder.Configuration.GetValue<string>("FirebaseSettings:apiKey"),
    AuthDomain = builder.Configuration.GetValue<string>("FirebaseSettings:authDomain"),
    Providers = new FirebaseAuthProvider[] { new EmailProvider() }
}));

// Add services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddScheme<AuthenticationSchemeOptions, FirebaseAuthenticationHandler>(JwtBearerDefaults.AuthenticationScheme, (authScheme) =>
    {
        authScheme.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["token"];
                return Task.CompletedTask;
            }
        };
    }
    ).AddCookie(value =>
    {
        value.Cookie.Name = "token";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
