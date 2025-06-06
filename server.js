const express = require("express");
const cors = require("cors");
const { ZingMp3 } = require("zingmp3-api-full");

const app = express();
app.use(cors());

// Hàm tiện ích để xử lý lỗi async
const asyncHandler = (fn) => (req, res, next) =>
  Promise.resolve(fn(req, res, next)).catch((err) => {
    console.error("Lỗi ZingMP3 API:", err.message, err.stack);
    res.status(500).json({ error: "Lỗi máy chủ nội bộ", details: err.message });
  });

// Bộ nhớ cache cho thông tin thể loại
const genreCache = {
  "IWZ9Z08I": { id: "IWZ9Z08I", name: "Việt Nam", title: "Việt Nam", alias: "viet-nam", link: "/the-loai-album/Viet-Nam/IWZ9Z08I.html" },
  "IWZ97FCD": { id: "IWZ97FCD", name: "V-Pop", title: "V-Pop", alias: "v-pop", link: "/the-loai-album/V-Pop/IWZ97FCD.html" },
  "IWZ9Z08U": { id: "IWZ9Z08U", name: "Nhạc Hoa", title: "Nhạc Hoa", alias: "nhac-hoa", link: "/the-loai-album/Nhac-Hoa/IWZ9Z08U.html" },
  "IWZ9Z08O": { id: "IWZ9Z08O", name: "Âu Mỹ", title: "Âu Mỹ", alias: "au-my", link: "/the-loai-album/Au-My/IWZ9Z08O.html" },
  "IWZ9Z097": { id: "IWZ9Z097", name: "Pop", title: "Pop", alias: "pop", link: "/the-loai-album/Pop/IWZ9Z097.html" },
  "IWZ9Z08W": { id: "IWZ9Z08W", name: "Hàn Quốc", title: "Hàn Quốc", alias: "han-quoc", link: "/the-loai-album/Han-Quoc/IWZ9Z08W.html" }
};

// TÌM KIẾM
app.get("/api/search", asyncHandler(async (req, res) => {
  const keyword = req.query.q;
  if (!keyword) return res.status(400).json({ error: "Thiếu từ khóa tìm kiếm" });
  const data = await ZingMp3.search(keyword);
  res.json({
    err: 0,
    msg: "Thành công",
    data,
    timestamp: Date.now()
  });
}));

// LẤY URL FILE NHẠC VÀ THÔNG TIN CƠ BẢN
app.get("/api/song/:id", asyncHandler(async (req, res) => {
  const { id } = req.params;
  const data = await ZingMp3.getSong(id);

  if (data.err !== 0 || !data.data) {
    return res.status(404).json({ error: "Không tìm thấy bài hát" });
  }

  const song = {
    encodeId: data.data.encodeId,
    title: data.data.title,
    thumbnail: data.data.thumbnail,
    thumbnailM: data.data.thumbnailM,
    artistsNames: data.data.artistsNames,
    duration: data.data.duration,
    streaming: {
      mp3_128: data.data["128"],
      mp3_320: data.data["320"] || null
    },
    link: data.data.link
  };

  res.json({
    err: 0,
    msg: "Thành công",
    data: song,
    timestamp: Date.now()
  });
}));

// LẤY THÔNG TIN CHI TIẾT BÀI HÁT
app.get("/api/song-info/:id", asyncHandler(async (req, res) => {
  const { id } = req.params;

  const [songData, lyricData] = await Promise.all([
    ZingMp3.getInfoSong(id),
    ZingMp3.getLyric(id)
  ]);

  if (songData.err !== 0 || !songData.data) {
    return res.status(404).json({ error: "Không tìm thấy bài hát" });
  }

  const song = {
    encodeId: songData.data.encodeId,
    title: songData.data.title,
    alias: songData.data.alias,
    thumbnail: songData.data.thumbnail,
    thumbnailM: songData.data.thumbnailM,
    artists: songData.data.artists?.map(artist => ({
      id: artist.id,
      name: artist.name,
      thumbnail: artist.thumbnail,
      link: artist.link
    })) || [],
    artistsNames: songData.data.artistsNames,
    duration: songData.data.duration,
    releaseDate: songData.data.releaseDate,
    genreIds: songData.data.genreIds || [],
    genres: songData.data.genres?.map(genre => ({
      id: genre.id,
      name: genre.name
    })) || [],
    album: songData.data.album ? {
      encodeId: songData.data.album.encodeId,
      title: songData.data.album.title,
      thumbnail: songData.data.album.thumbnail,
      link: songData.data.album.link
    } : null,
    lyrics: lyricData.data?.sentences?.map(sentence => ({
      words: sentence.words.map(word => ({
        startTime: word.startTime,
        endTime: word.endTime,
        data: word.data
      }))
    })) || [],
    hasLyric: !!lyricData.data?.sentences,
    distributor: songData.data.distributor,
    streamingStatus: songData.data.streamingStatus,
    link: songData.data.link,
    mvlink: songData.data.mvlink || null
  };

  // Cập nhật genreCache từ thông tin bài hát
  if (song.genres && song.genres.length > 0) {
    song.genres.forEach(genre => {
      if (!genreCache[genre.id]) {
        genreCache[genre.id] = {
          id: genre.id,
          name: genre.name,
          title: genre.name,
          alias: genre.name.toLowerCase().replace(/\s+/g, "-"),
          link: `/the-loai-album/${genre.name.replace(/\s+/g, "-")}/${genre.id}.html`
        };
      }
    });
  }

  res.json({
    err: 0,
    msg: "Thành công",
    data: song,
    timestamp: Date.now()
  });
}));

// LẤY DANH SÁCH THỂ LOẠI
app.get("/api/genres", asyncHandler(async (req, res) => {
  console.log("[DEBUG] Fetching all genres");

  // Lấy tất cả thể loại từ genreCache
  const genres = Object.values(genreCache);

  if (genres.length === 0) {
    console.log("[DEBUG] No genres found in cache");
    return res.status(404).json({ error: "Không tìm thấy danh sách thể loại" });
  }

  res.json({
    err: 0,
    msg: "Thành công",
    data: { genres },
    timestamp: Date.now()
  });
}));

// LẤY THÔNG TIN THỂ LOẠI THEO ID CỤ THỂ
app.get("/api/genres/by-ids", asyncHandler(async (req, res) => {
  console.log("[DEBUG] Fetching genres by IDs:", req.query.ids);

  // Lấy danh sách genreIds từ query parameter
  const ids = (req.query.ids || "").split(",").filter(id => id.trim());

  if (ids.length === 0) {
    return res.status(400).json({ error: "No genre IDs provided" });
  }

  // Hàm lấy thông tin thể loại từ một album mẫu nếu chưa có trong cache
  const fetchGenresFromAlbum = async (genreId) => {
    try {
      // Sử dụng một album mẫu (có thể thay bằng album khác)
      const albumId = "6B9I7769"; // Ví dụ: NewJeans 'OMG' (EP)
      const albumData = await ZingMp3.getDetailPlaylist(albumId);

      if (albumData.err === 0 && albumData.data && albumData.data.genres) {
        const foundGenre = albumData.data.genres.find(g => g.id === genreId);
        if (foundGenre && !genreCache[genreId]) {
          genreCache[genreId] = {
            id: foundGenre.id,
            name: foundGenre.name,
            title: foundGenre.title,
            alias: foundGenre.alias,
            link: foundGenre.link
          };
        }
      }
    } catch (error) {
      console.log(`[DEBUG] Failed to fetch genre ${genreId} from album: ${error.message}`);
    }
  };

  // Kiểm tra và cập nhật cache nếu cần
  for (const id of ids) {
    if (!genreCache[id]) {
      await fetchGenresFromAlbum(id);
    }
  }

  // Lọc và lấy thông tin thể loại từ cache
  const genres = ids.map(id => genreCache[id]).filter(genre => genre !== undefined);

  if (genres.length === 0) {
    return res.status(404).json({ error: "No genres found for the provided IDs" });
  }

  res.json({
    err: 0,
    msg: "Thành công",
    data: { genres },
    timestamp: Date.now()
  });
}));

// LẤY CHI TIẾT ALBUM
app.get("/api/album/:id", asyncHandler(async (req, res) => {
  const { id } = req.params;
  const data = await ZingMp3.getDetailPlaylist(id);

  if (data.err !== 0 || !data.data) {
    return res.status(404).json({ error: "Không tìm thấy album" });
  }

  const album = {
    encodeId: data.data.encodeId,
    title: data.data.title,
    thumbnail: data.data.thumbnail,
    thumbnailM: data.data.thumbnailM,
    releaseDate: data.data.releaseDate,
    genres: data.data.genres?.map(genre => ({
      id: genre.id,
      name: genre.name,
      title: genre.title,
      alias: genre.alias,
      link: genre.link
    })) || [],
    artists: data.data.artists?.map(artist => ({
      id: artist.id,
      name: artist.name,
      thumbnail: artist.thumbnail,
      link: artist.link
    })) || [],
    songs: data.data.song?.items?.map(song => ({
      encodeId: song.encodeId,
      title: song.title,
      thumbnail: song.thumbnail,
      duration: song.duration,
      artistsNames: song.artistsNames,
      releaseDate: song.releaseDate,
      link: song.link
    })) || [],
    totalDuration: data.data.song?.totalDuration || 0,
    distributor: data.data.distributor,
    isAlbum: data.data.isAlbum,
    like: data.data.like || 0,
    listen: data.data.listen || 0
  };

  // Cập nhật genreCache từ thông tin album
  if (album.genres && album.genres.length > 0) {
    album.genres.forEach(genre => {
      if (!genreCache[genre.id]) {
        genreCache[genre.id] = {
          id: genre.id,
          name: genre.name,
          title: genre.title,
          alias: genre.alias,
          link: genre.link
        };
      }
    });
  }

  res.json({
    err: 0,
    msg: "Thành công",
    data: album,
    timestamp: Date.now()
  });
}));

// CÁC ENDPOINT KHÁC
app.get("/api/playlist/:id", asyncHandler(async (req, res) => {
  const { id } = req.params;
  const data = await ZingMp3.getDetailPlaylist(id);
  res.json({
    err: 0,
    msg: "Thành công",
    data,
    timestamp: Date.now()
  });
}));

app.get("/api/top100", asyncHandler(async (req, res) => {
  const data = await ZingMp3.getTop100();
  res.json({
    err: 0,
    msg: "Thành công",
    data,
    timestamp: Date.now()
  });
}));

app.get("/api/artist/:name", asyncHandler(async (req, res) => {
  const { name } = req.params;
  const data = await ZingMp3.getArtist(name);
  res.json({
    err: 0,
    msg: "Thành công",
    data,
    timestamp: Date.now()
  });
}));

// LẤY THÔNG TIN NGHỆ SĨ THEO ID (CHỈ LẤY THÔNG TIN CƠ BẢN)
app.get("/api/artist-by-id/:id", asyncHandler(async (req, res) => {
  const { id } = req.params;

  // Gọi ZingMp3.getArtist với id (nếu API hỗ trợ) hoặc tìm cách lấy tên nghệ sĩ trước
  // Hiện tại ZingMp3 API không hỗ trợ getArtist bằng id trực tiếp, cần lấy tên nghệ sĩ từ /api/video
  const videoData = await ZingMp3.getVideo(id); // Thử lấy thông tin từ video nếu cần
  if (videoData.err !== 0 || !videoData.data || !videoData.data.artists) {
    return res.status(404).json({ error: "Không tìm thấy thông tin nghệ sĩ từ video" });
  }

  const artist = videoData.data.artists.find(a => a.id === id);
  if (!artist) {
    return res.status(404).json({ error: "Không tìm thấy nghệ sĩ với ID này" });
  }

  const artistName = artist.name;
  console.log("[DEBUG] Found artist name:", artistName);

  const artistData = await ZingMp3.getArtist(artistName);
  if (artistData.err !== 0 || !artistData.data) {
    return res.status(404).json({ error: "Không tìm thấy thông tin nghệ sĩ" });
  }

  const artistInfo = artistData.data;
  const artistResponse = {
    id: artistInfo.id,
    name: artistInfo.name,
    link: artistInfo.link,
    thumbnail: artistInfo.thumbnail,
    thumbnailM: artistInfo.thumbnailM,
    national: artistInfo.national,
    bio: artistInfo.biography,
    sortBiography: artistInfo.sortBiography,
    birthday: artistInfo.birthday,
    realname: artistInfo.realname,
    totalFollow: artistInfo.totalFollow
  };

  res.json({
    err: 0,
    msg: "Thành công",
    data: artistResponse,
    timestamp: Date.now()
  });
}));

app.get("/api/artist-songs/:id", asyncHandler(async (req, res) => {
  const { id } = req.params;
  const page = req.query.page || 1;
  const count = req.query.count || 15;
  const data = await ZingMp3.getListArtistSong(id, page, count);
  res.json({
    err: 0,
    msg: "Thành công",
    data,
    timestamp: Date.now()
  });
}));

app.get("/api/lyric/:id", asyncHandler(async (req, res) => {
  const { id } = req.params;
  const data = await ZingMp3.getLyric(id);
  res.json({
    err: 0,
    msg: "Thành công",
    data,
    timestamp: Date.now()
  });
}));

app.get("/api/video/:id", asyncHandler(async (req, res) => {
  const { id } = req.params;
  const data = await ZingMp3.getVideo(id);
  console.log("[DEBUG] Raw video data:", JSON.stringify(data, null, 2)); // In dữ liệu thô để kiểm tra

  if (data.err !== 0 || !data.data) {
    return res.status(404).json({ error: "Không tìm thấy video MV" });
  }

  const video = {
    encodeId: data.data.encodeId,
    title: data.data.title,
    thumbnail: data.data.thumbnail,
    thumbnailM: data.data.thumbnailM,
    duration: data.data.duration,
    artists: data.data.artists?.map(artist => ({
      id: artist.id,
      name: artist.name
    })) || [],
    artistsNames: data.data.artistsNames,
    streaming: {
      mp4_480: data.data.streaming?.mp4?.["480p"] || null,
      mp4_720: data.data.streaming?.mp4?.["720p"] || null,
      mp4_1080: data.data.streaming?.mp4?.["1080p"] || null,
      hls: data.data.streaming?.hls || null // Thêm trường HLS nếu có
    },
    link: data.data.link
  };

  res.json({
    err: 0,
    msg: "Thành công",
    data: video,
    timestamp: Date.now()
  });
}));

// KHỞI ĐỘNG SERVER
const PORT = process.env.PORT || 5000;
app.listen(PORT, () => {
  console.log(`🎵 Server Zing MP3 API đang chạy tại: http://localhost:${PORT}`);
});