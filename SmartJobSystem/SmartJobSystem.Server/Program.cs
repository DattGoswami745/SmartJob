using SmartJobAPI.Helpers;
using SmartJobSystem.Server.Data;

var builder = WebApplication.CreateBuilder(args);

/* Controllers */
builder.Services.AddControllers();

/* DB Helper */
builder.Services.AddScoped<DbHelper>();
builder.Services.AddSingleton<GeminiHelper>();
builder.Services.AddHttpClient<GeminiChatHelper>();

/* Session */
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

/* CORS */
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

/* Swagger */
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowVueApp");
app.UseSession();
app.UseAuthorization();
app.MapControllers();

app.Run();
