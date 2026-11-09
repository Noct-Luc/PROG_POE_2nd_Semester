using Microsoft.AspNetCore.Authentication.Cookies;
using RaceDay.MVC.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Add session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "RaceDaySession";
});

// Add cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

// Add authorization policies for role-based access
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Manager", policy => policy.RequireClaim("UserRole", "Manager"))
    .AddPolicy("Participant", policy => policy.RequireClaim("UserRole", "Participant"))
    .AddPolicy("AuthenticatedUsers", policy => policy.RequireAuthenticatedUser());

// Add CORS for API communication
builder.Services.AddCors(options =>
{
    options.AddPolicy("ApiPolicy", policy =>
    {
        policy.WithOrigins(builder.Configuration["ApiSettings:AllowedOrigins"] ?? "https://localhost:7176")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});
builder.Services.AddHttpContextAccessor();

// Register HttpClient and ApiService for API communication

builder.Services.AddHttpClient<IApiService, ApiService>(client =>
    {
    client.BaseAddress = new Uri(
          builder.Configuration["ApiSettings:BaseUrl"]
            ?? "https://localhost:7176/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "RaceDay-MVC-Client/1.0");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigureHttpClient(client =>
{
    // Additional HttpClient configuration if needed
});

// Add anti-forgery token service
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.SuppressXFrameOptionsHeader = false;
});

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Use CORS if enabled
app.UseCors("ApiPolicy");

// Use session before authentication
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();
