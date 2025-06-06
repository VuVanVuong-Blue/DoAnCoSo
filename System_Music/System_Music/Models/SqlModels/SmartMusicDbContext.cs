using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System_Music.Models.SqlModels;

namespace System_Music.Models.SqlModels
{
    public class SmartMusicDbContext : IdentityDbContext<User>
    {
        public SmartMusicDbContext(DbContextOptions<SmartMusicDbContext> options)
            : base(options)
        {
        }

        public DbSet<Artist> Artists { get; set; }
        public DbSet<Track> Tracks { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<PlaylistTrack> PlaylistTracks { get; set; }
        public DbSet<LikeTrack> LikeTracks { get; set; }
        public DbSet<AlbumArtist> AlbumArtists { get; set; }
        public DbSet<Follower> Followers { get; set; }
        public DbSet<ChartRanking> ChartRankings { get; set; }
        public DbSet<ChartInteraction> ChartInteractions { get; set; }
        public DbSet<UserMedia> UserMedias { get; set; }
        public DbSet<LyricsTiming> LyricsTimings { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<TrackGenre> TrackGenres { get; set; }
        public DbSet<PlayQueue> PlayQueues { get; set; }
        public DbSet<Download> Downloads { get; set; }
        public DbSet<BlockList> BlockList { get; set; }
        public DbSet<TrackArtist> TrackArtists { get; set; }
        public DbSet<ListenHistory> ListenHistories { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<VideoArtist> VideoArtists { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // PlaylistTrack
            modelBuilder.Entity<PlaylistTrack>()
                .HasKey(pt => new { pt.PlaylistId, pt.TrackId });

            modelBuilder.Entity<PlaylistTrack>()
                .HasOne(pt => pt.Playlist)
                .WithMany(p => p.PlaylistTracks)
                .HasForeignKey(pt => pt.PlaylistId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<PlaylistTrack>()
                .HasOne(pt => pt.Track)
                .WithMany(t => t.PlaylistTracks)
                .HasForeignKey(pt => pt.TrackId)
                .OnDelete(DeleteBehavior.NoAction);

            // Genre
            modelBuilder.Entity<Genre>()
                .HasIndex(g => g.ZingMp3GenreId)
                .IsUnique(false);

            // LikeTrack
            modelBuilder.Entity<LikeTrack>()
                .HasIndex(l => new { l.UserId, l.TrackId })
                .IsUnique();

            modelBuilder.Entity<LikeTrack>()
                .HasOne(lt => lt.User)
                .WithMany(u => u.LikedTracks)
                .HasForeignKey(lt => lt.UserId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<LikeTrack>()
                .HasOne(lt => lt.Track)
                .WithMany(t => t.LikeTracks)
                .HasForeignKey(lt => lt.TrackId)
                .OnDelete(DeleteBehavior.ClientCascade);

            // TrackGenre
            modelBuilder.Entity<TrackGenre>()
                .HasKey(tg => new { tg.TrackId, tg.GenreId });

            modelBuilder.Entity<TrackGenre>()
                .HasOne(tg => tg.Track)
                .WithMany(t => t.TrackGenres)
                .HasForeignKey(tg => tg.TrackId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<TrackGenre>()
                .HasOne(tg => tg.Genre)
                .WithMany(g => g.TrackGenres)
                .HasForeignKey(tg => tg.GenreId)
                .OnDelete(DeleteBehavior.ClientCascade);

            // Album và Track
            modelBuilder.Entity<Track>()
                .HasOne(t => t.Album)
                .WithMany(a => a.Tracks)
                .HasForeignKey(t => t.AlbumId)
                .OnDelete(DeleteBehavior.SetNull);

            // Artist
            modelBuilder.Entity<Artist>()
                .HasIndex(a => a.ZingMp3ArtistId)
                .IsUnique(false);

            // Album
            modelBuilder.Entity<Album>()
                .HasIndex(a => a.ZingMp3AlbumId)
                .IsUnique(false);

            // Playlist và User
            modelBuilder.Entity<Playlist>()
                .HasOne(p => p.User)
                .WithMany(u => u.Playlists)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // PlayQueue
            modelBuilder.Entity<PlayQueue>()
                .HasOne(pq => pq.User)
                .WithMany(u => u.PlayQueues)
                .HasForeignKey(pq => pq.UserId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<PlayQueue>()
                .HasOne(pq => pq.Track)
                .WithMany()
                .HasForeignKey(pq => pq.TrackId)
                .OnDelete(DeleteBehavior.ClientCascade);

            // Download
            modelBuilder.Entity<Download>()
                .HasOne(d => d.User)
                .WithMany(u => u.Downloads)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<Download>()
                .HasOne(d => d.Track)
                .WithMany()
                .HasForeignKey(d => d.TrackId)
                .OnDelete(DeleteBehavior.ClientCascade);

            // BlockList
            modelBuilder.Entity<BlockList>()
                .HasOne(b => b.User)
                .WithMany(u => u.BlockedItems)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<BlockList>()
                .HasOne(b => b.Artist)
                .WithMany()
                .HasForeignKey(b => b.ArtistId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<BlockList>()
                .HasOne(b => b.Track)
                .WithMany()
                .HasForeignKey(b => b.TrackId)
                .OnDelete(DeleteBehavior.ClientCascade);

            // Follower
            modelBuilder.Entity<Follower>()
                .HasOne(f => f.User)
                .WithMany(u => u.FollowedArtists)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<Follower>()
                .HasOne(f => f.Artist)
                .WithMany()
                .HasForeignKey(f => f.ArtistId)
                .OnDelete(DeleteBehavior.ClientCascade);

            // ChartRanking
            modelBuilder.Entity<ChartRanking>()
                .HasOne(cr => cr.Track)
                .WithMany()
                .HasForeignKey(cr => cr.TrackId)
                .OnDelete(DeleteBehavior.ClientCascade);

            // ChartInteraction
            modelBuilder.Entity<ChartInteraction>()
                .HasOne(ci => ci.User)
                .WithMany()
                .HasForeignKey(ci => ci.UserId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<ChartInteraction>()
                .HasOne(ci => ci.Track)
                .WithMany()
                .HasForeignKey(ci => ci.TrackId)
                .OnDelete(DeleteBehavior.ClientCascade);

            // LyricsTiming
            modelBuilder.Entity<LyricsTiming>()
                .HasOne(lt => lt.Track)
                .WithMany()
                .HasForeignKey(lt => lt.TrackId)
                .OnDelete(DeleteBehavior.ClientCascade);

            // UserMedia
            modelBuilder.Entity<UserMedia>()
                .HasOne(um => um.User)
                .WithMany(u => u.UserMedias)
                .HasForeignKey(um => um.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserMedia>()
                .HasOne(um => um.Playlist)
                .WithMany()
                .HasForeignKey(um => um.PlaylistId)
                .OnDelete(DeleteBehavior.NoAction);

            // TrackArtist
            modelBuilder.Entity<TrackArtist>()
                .HasKey(ta => new { ta.TrackId, ta.ArtistId });

            modelBuilder.Entity<TrackArtist>()
                .HasOne(ta => ta.Track)
                .WithMany(t => t.TrackArtists)
                .HasForeignKey(ta => ta.TrackId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<TrackArtist>()
                .HasOne(ta => ta.Artist)
                .WithMany(a => a.TrackArtists)
                .HasForeignKey(ta => ta.ArtistId)
                .OnDelete(DeleteBehavior.ClientCascade);

            // AlbumArtist
            modelBuilder.Entity<AlbumArtist>()
                .HasKey(aa => new { aa.AlbumId, aa.ArtistId });

            modelBuilder.Entity<AlbumArtist>()
                .HasOne(aa => aa.Album)
                .WithMany(a => a.AlbumArtists)
                .HasForeignKey(aa => aa.AlbumId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<AlbumArtist>()
                .HasOne(aa => aa.Artist)
                .WithMany(a => a.AlbumArtists)
                .HasForeignKey(aa => aa.ArtistId)
                .OnDelete(DeleteBehavior.ClientCascade);

            // ListenHistory
            modelBuilder.Entity<ListenHistory>()
                .HasOne(lh => lh.User)
                .WithMany(u => u.ListenHistories)
                .HasForeignKey(lh => lh.UserId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<ListenHistory>()
                .HasOne(lh => lh.Track)
                .WithMany(t => t.ListenHistories)
                .HasForeignKey(lh => lh.TrackId)
                .OnDelete(DeleteBehavior.ClientCascade);
            // VideoArtist
            modelBuilder.Entity<VideoArtist>()
                .HasKey(va => new { va.VideoEncodeId, va.ArtistId });

            modelBuilder.Entity<VideoArtist>()
                .HasOne(va => va.Video)
                .WithMany(v => v.VideoArtists)
                .HasForeignKey(va => va.VideoEncodeId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<VideoArtist>()
                .HasOne(va => va.Artist)
                .WithMany(a => a.VideoArtists) // Thêm navigation property trong Artist
                .HasForeignKey(va => va.ArtistId)
                .OnDelete(DeleteBehavior.ClientCascade);

            // Đảm bảo Video có khóa chính là EncodeId
            modelBuilder.Entity<Video>()
                .HasKey(v => v.EncodeId);
        }
    }
}