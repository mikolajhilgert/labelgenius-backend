using FirebaseAdmin;
using MongoDB.Driver;
using userservice.Services;
using userservice.Repositories;
using Google.Apis.Auth.OAuth2;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(FirebaseApp.Create(new AppOptions {
    Credential = GoogleCredential.FromFile("firebase.json")
}));

builder.Services.AddSingleton<IMongoClient>(s =>
        new MongoClient(builder.Configuration.GetValue<string>("UserDbSettings:ConnectionString")));

// Add services
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRegisterService, RegisterService>();

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
