using AutoMapper;
using System_Music.Models.SqlModels;
using System_Music.Models.DTOs;

namespace System_Music.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.AvatarPath, opt => opt.MapFrom(src => src.AvatarMediaId.HasValue ? src.AvatarMedia.MediaPath : null));
            
            CreateMap<UserRegisterRequest, User>();

            CreateMap<Playlist, PlaylistDto>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageMedia != null ? src.ImageMedia.MediaPath : null))
                .ForMember(dest => dest.Tracks, opt => opt.MapFrom(src => src.PlaylistTracks));

            CreateMap<PlaylistTrack, TrackDto>()
                .ForMember(dest => dest.TrackId, opt => opt.MapFrom(src => src.TrackId))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Track.Title))
                .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Track.Duration))
                .ForMember(dest => dest.AudioUrl, opt => opt.MapFrom(src => src.Track.AudioUrl))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Track.ImageUrl))
                .ForMember(dest => dest.AlbumId, opt => opt.MapFrom(src => src.Track.AlbumId))
                .ForMember(dest => dest.AlbumName, opt => opt.MapFrom(src => src.Track.Album != null ? src.Track.Album.Name : null))
                .ForMember(dest => dest.Artists, opt => opt.MapFrom(src => src.Track.TrackArtists != null ? src.Track.TrackArtists.Select(ta => ta.Artist).ToList() : new List<Artist>()))
                .ForMember(dest => dest.AddedDate, opt => opt.MapFrom(src => src.AddedDate))
                .ForMember(dest => dest.ZingMp3TrackId, opt => opt.MapFrom(src => src.Track.ZingMp3TrackId));

            CreateMap<Track, TrackDto>()
                .ForMember(dest => dest.AlbumName, opt => opt.MapFrom(src => src.Album != null ? src.Album.Name : null))
                .ForMember(dest => dest.Artists, opt => opt.MapFrom(src => src.TrackArtists != null ? src.TrackArtists.Select(ta => ta.Artist).ToList() : new List<Artist>()))
                .ForMember(dest => dest.Genres, opt => opt.MapFrom(src => src.TrackGenres != null ? src.TrackGenres.Select(tg => tg.Genre).ToList() : new List<Genre>()))
                .ForMember(dest => dest.ZingMp3TrackId, opt => opt.MapFrom(src => src.ZingMp3TrackId));

            CreateMap<Album, AlbumDto>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Image))
                .ForMember(dest => dest.Artists, opt => opt.MapFrom(src => src.AlbumArtists != null ? src.AlbumArtists.Select(aa => aa.Artist).ToList() : new List<Artist>()))
                .ForMember(dest => dest.Tracks, opt => opt.MapFrom(src => src.Tracks))
                .ForMember(dest => dest.ZingMp3AlbumId, opt => opt.MapFrom(src => src.ZingMp3AlbumId));

            CreateMap<Artist, ArtistDto>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Image))
                .ForMember(dest => dest.Biography, opt => opt.MapFrom(src => src.Bio))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(dest => dest.Tracks, opt => opt.MapFrom(src => src.TrackArtists != null ? src.TrackArtists.Select(ta => ta.Track).ToList() : new List<Track>()))
                .ForMember(dest => dest.Albums, opt => opt.MapFrom(src => src.AlbumArtists != null ? src.AlbumArtists.Select(aa => aa.Album).ToList() : new List<Album>()))
                .ForMember(dest => dest.ZingMp3ArtistId, opt => opt.MapFrom(src => src.ZingMp3ArtistId));

            CreateMap<Genre, GenreDto>()
                .ForMember(dest => dest.Tracks, opt => opt.MapFrom(src => src.TrackGenres != null ? src.TrackGenres.Select(tg => tg.Track).ToList() : new List<Track>()));

            CreateMap<Video, VideoDto>()
                .ForMember(dest => dest.Artists, opt => opt.MapFrom(src => src.VideoArtists != null ? src.VideoArtists.Select(va => va.Artist).ToList() : new List<Artist>()));

            CreateMap<LikeTrack, LikeTrackDto>().ReverseMap();
        }
    }
}
