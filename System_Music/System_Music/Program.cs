using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using System_Music.Interfaces;
using System_Music.Models.SqlModels;
using System_Music.Models.ViewModels;
using System_Music.Repositories.Implementations;
using System_Music.Repositories.Interfaces;
using System_Music.Services;
using System_Music.Services.Implementations;
using System_Music.Services.Interfaces;
using System_Music.Services.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Momo API Payment
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

// Đăng ký các repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>)); // Generic repository
builder.Services.AddScoped<ILikeTrackRepository, LikeTrackRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITrackRepository, TrackRepository>();
builder.Services.AddScoped<IAlbumRepository, AlbumRepository>();
builder.Services.AddScoped<IArtistRepository, ArtistRepository>();
builder.Services.AddScoped<IGenreRepository, GenreRepository>();
builder.Services.AddScoped<IListenHistoryRepository, ListenHistoryRepository>();
builder.Services.AddScoped<IPlaylistRepository, PlaylistRepository>();
builder.Services.AddScoped<IPlaylistTrackRepository, PlaylistTrackRepository>();
builder.Services.AddScoped<IUserMediaRepository, UserMediaRepository>();
builder.Services.AddScoped<ILyricsTimingRepository, LyricsTimingRepository>();
builder.Services.AddSingleton<ITempDataDictionaryFactory, TempDataDictionaryFactory>(); // Đăng ký TempDataFactory
builder.Services.AddScoped<ITrackArtistRepository, TrackArtistRepository>();
builder.Services.AddScoped<IVideoRepository, VideoRepository>();

// Đăng ký các service
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITrackService, TrackService>();
builder.Services.AddScoped<IAlbumService, AlbumService>();
builder.Services.AddScoped<IArtistService, ArtistService>();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<IListenHistoryService, ListenHistoryService>();
builder.Services.AddScoped<IPlaylistService, PlaylistService>();
builder.Services.AddScoped<IPlaylistTrackService, PlaylistTrackService>();
builder.Services.AddScoped<IUserMediaService, UserMediaService>();
builder.Services.AddScoped<ILikeTrackService, LikeTrackService>();
builder.Services.AddScoped<ILyricsTimingService, LyricsTimingService>();
builder.Services.AddScoped<IVideoService, VideoService>();

// Thêm logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

// Cấu hình DbContext
builder.Services.AddDbContext<SmartMusicDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Cấu hình Identity và chính sách phân quyền
builder.Services.AddIdentity<User, IdentityRole>()
    .AddDefaultTokenProviders()
    .AddDefaultUI()
    .AddEntityFrameworkStores<SmartMusicDbContext>();

// Thêm chính sách phân quyền cho vai trò Admin
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
});

builder.Services.AddRazorPages();

// Thêm cấu hình tùy chỉnh cho RazorViewEngine
builder.Services.Configure<RazorViewEngineOptions>(options =>
{

    options.ViewLocationFormats.Add("/Areas/Admin/Views/Video/{0}.cshtml");
    options.ViewLocationFormats.Add("~/Views/Home/{0}.cshtml");
    // Giữ các định dạng mặc định
    options.ViewLocationFormats.Add("~/Views/{1}/{0}.cshtml"); // {1} là controller name, {0} là view name
    options.ViewLocationFormats.Add("~/Views/Shared/{0}.cshtml");
    Console.WriteLine("RazorViewEngineOptions configured with ViewLocationFormats:");
    foreach (var format in options.ViewLocationFormats)
    {
        Console.WriteLine($" - {format}");
    }
});
// Cấu hình Cookie
builder.Services.ConfigureApplicationCookie(option =>
{
    option.LoginPath = $"/Identity/Account/Login";
    option.LogoutPath = $"/Identity/Account/Logout";
    option.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

// Cấu hình HttpClient
builder.Services.AddHttpClient();

var app = builder.Build();

// Tạo vai trò Admin và gán cho người dùng
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    // Tạo vai trò Admin nếu chưa tồn tại
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        var roleResult = await roleManager.CreateAsync(new IdentityRole("Admin"));
        if (!roleResult.Succeeded)
        {
            Console.WriteLine($"[ERROR] Failed to create Admin role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
        }
    }

    // Tạo tài khoản Admin nếu chưa tồn tại
    var adminUser = await userManager.FindByEmailAsync("admin@example.com");
    if (adminUser == null)
    {
        adminUser = new User
        {
            UserName = "admin@example.com",
            Email = "admin@example.com",
            EmailConfirmed = true
        };
        var userResult = await userManager.CreateAsync(adminUser, "Admin@123");
        if (!userResult.Succeeded)
        {
            Console.WriteLine($"[ERROR] Failed to create admin user: {string.Join(", ", userResult.Errors.Select(e => e.Description))}");
        }
    }

    // Gán vai trò Admin cho tài khoản
    if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
        if (!roleResult.Succeeded)
        {
            Console.WriteLine($"[ERROR] Failed to assign Admin role to user: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

// Thêm endpoint để đồng bộ thể loại từ Zing MP3
app.MapGet("/Admin/Genre/SyncFromZingMp3", async (IGenreService genreService) =>
{
    var genres = await genreService.SyncGenresFromZingMp3Async();
    return Results.Ok(genres);
}).RequireAuthorization("Admin");

// Thêm hỗ trợ attribute routing
app.MapControllers();

// Giữ lại các route mặc định
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);
app.MapControllerRoute(
    name: "lyrics",
    pattern: "Lyrics/{action}",
    defaults: new { controller = "Lyrics", action = "LyricsKaraoke" }
);
app.Run();