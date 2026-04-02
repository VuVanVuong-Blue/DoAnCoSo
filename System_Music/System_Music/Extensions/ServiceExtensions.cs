using System_Music.Repositories.Implementations;
using System_Music.Repositories.Interfaces;
using System_Music.Services.Implementations;
using System_Music.Services.Interfaces;

namespace System_Music.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Repositories
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<ILikeTrackRepository, LikeTrackRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITrackRepository, TrackRepository>();
            services.AddScoped<IAlbumRepository, AlbumRepository>();
            services.AddScoped<IArtistRepository, ArtistRepository>();
            services.AddScoped<IGenreRepository, GenreRepository>();
            services.AddScoped<IListenHistoryRepository, ListenHistoryRepository>();
            services.AddScoped<IPlaylistRepository, PlaylistRepository>();
            services.AddScoped<IPlaylistTrackRepository, PlaylistTrackRepository>();
            services.AddScoped<IUserMediaRepository, UserMediaRepository>();
            services.AddScoped<ILyricsTimingRepository, LyricsTimingRepository>();
            services.AddScoped<ITrackArtistRepository, TrackArtistRepository>();
            services.AddScoped<ITrackGenreRepository, TrackGenreRepository>();
            services.AddScoped<IVideoRepository, VideoRepository>();
            services.AddScoped<IChartRankingRepository, ChartRankingRepository>();

            // Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITrackService, TrackService>();
            services.AddScoped<IAlbumService, AlbumService>();
            services.AddScoped<IArtistService, ArtistService>();
            services.AddScoped<IGenreService, GenreService>();
            services.AddScoped<IListenHistoryService, ListenHistoryService>();
            services.AddScoped<IPlaylistService, PlaylistService>();
            services.AddScoped<IPlaylistTrackService, PlaylistTrackService>();
            services.AddScoped<IUserMediaService, UserMediaService>();
            services.AddScoped<ILikeTrackService, LikeTrackService>();
            services.AddScoped<ILyricsTimingService, LyricsTimingService>();
            services.AddScoped<IVideoService, VideoService>();
            services.AddScoped<IMomoService, MomoService>();
            services.AddScoped<IChartRankingService, ChartRankingService>();
            
            // New Services
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<ISearchService, SearchService>();
            services.AddScoped<IMediaService, MediaService>();

            return services;
        }
    }
}
