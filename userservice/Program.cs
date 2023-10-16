using Firebase.Auth;
using Firebase.Auth.Providers;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using MongoDB.Driver;
using userservice.Repositories;
using userservice.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(FirebaseApp.Create(new AppOptions
{
    Credential = GoogleCredential.FromFile("firebase.json")
}));

builder.Services.AddSingleton<IMongoClient>(s =>
        new MongoClient(builder.Configuration.GetValue<string>("UserDbSettings:ConnectionString")));

builder.Services.AddSingleton<IFirebaseAuthClient>(s => new FirebaseAuthClient(new FirebaseAuthConfig
{
    ApiKey = builder.Configuration.GetValue<string>("FirebaseSettings:apiKey"),
    AuthDomain = builder.Configuration.GetValue<string>("FirebaseSettings:authDomain"),
    Providers = new FirebaseAuthProvider[] { new EmailProvider() }
}));

// Add services
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); 

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
