
// Biến toàn cục
let playlistId = window.playlistId || 0;
let isMenuOpen = false;
let currentTrackId = null;

// Đảm bảo window.playerState được khởi tạo
window.playerState = window.playerState || {
    audio: document.getElementById('audio-player') || new Audio(),
    isPlaying: false,
    currentTrack: null,
    queue: [],
    currentIndex: -1,
    repeatMode: 'off'
};

// Hàm khởi tạo audio an toàn
function initializeAudio() {
    if (!window.playerState.audio) {
        console.warn("Audio element #audio-player not found, creating new Audio instance");
        window.playerState.audio = new Audio();
    }
    console.log("Audio element initialized successfully");
    return window.playerState.audio;
}

// Hàm phát bài hát
function playTrack(index) {
    if (index < 0 || index >= window.playerState.queue.length) {
        console.error('Chỉ số bài hát không hợp lệ:', index);
        alert('Không thể phát: Bài hát không hợp lệ');
        return;
    }

    window.playerState.currentIndex = index;
    window.playerState.currentTrack = { ...window.playerState.queue[index] }; // Sao chép để tránh thay đổi queue
    console.log('Đang phát bài:', window.playerState.currentTrack);

    if (!window.playerState.currentTrack.audioUrl) {
        console.error('audioUrl không hợp lệ:', window.playerState.currentTrack);
        alert('Không thể phát: Đường dẫn âm thanh không hợp lệ');
        return;
    }

    // Đảm bảo trackArtists hợp lệ
    if (!window.playerState.currentTrack.trackArtists) {
        console.warn('trackArtists không tồn tại, gán giá trị mặc định');
        window.playerState.currentTrack.trackArtists = [];
    }

    // Cập nhật giao diện
    if (!window.updateNowPlayingBar(window.playerState.currentTrack)) {
        console.error('Không thể cập nhật now-playing-bar');
        return;
    }

    window.playerState.audio.src = window.playerState.currentTrack.audioUrl;
    window.playerState.audio.play().then(() => {
        window.playerState.isPlaying = true;
        window.updatePlayPauseButton(true);
    }).catch(error => {

        alert('Lỗi khi phát bài hát: ' + error.message);
    });
}

// Hàm lấy danh sách bài hát từ API
async function fetchTracks() {
    try {
        const response = await fetch(`/Playlist/GetTracks/${playlistId}`);
        if (!response.ok) {
            throw new Error(`Không thể lấy danh sách bài hát: ${response.statusText}`);
        }
        const data = await response.json();
        console.log('Data from API:', data);

        window.playerState.queue = data.map(track => ({
            ...track,
            trackArtists: track.trackArtists || [], // Đơn giản hóa vì API đã trả về trackArtists
            id: track.id || '',
            title: track.title || 'Không có tiêu đề',
            audioUrl: track.audioUrl || '',
            imageUrl: track.imageUrl || 'https://via.placeholder.com/56',
            duration: track.duration || 0,
            addedDate: track.addedDate || null
        }));

        console.log('Formatted queue:', window.playerState.queue);
        if (!window.playerState.queue.length) {
            console.warn('Hàng đợi bài hát rỗng');
            alert('Không có bài hát trong playlist! Vui lòng thêm bài hát.');
        }

        renderTracklist();
    } catch (error) {
        console.error('Lỗi khi lấy bài hát:', error);
        alert(`Không thể tải danh sách bài hát: ${error.message}`);
        renderTracklist();
    }
}

document.addEventListener('DOMContentLoaded', function () {
    console.log("DOM fully loaded, initializing events in IndexPlayList");
    const menu = document.getElementById('contextMenuTracks');
    console.log('contextMenuTracks exists:', !!menu);
    if (!menu) {
        console.error('contextMenuTracks not found in DOM. Check IndexPlayList.cshtml');
    }

    // Khởi tạo audio
    initializeAudio();

    // Lấy danh sách bài hát
    fetchTracks();

    // Xử lý nút play lớn của playlist
    const playButton = document.querySelector('.play-button');
    if (playButton) {
        playButton.addEventListener('click', async () => {
            if (!window.playerState.queue.length) await fetchTracks();
            if (!window.playerState.queue.length) return;
            if (window.playerState.isPlaying) {
                window.playerState.audio.pause();
                window.playerState.isPlaying = false;
                window.updatePlayPauseButton(false);
            } else {
                playTrack(window.playerState.currentIndex >= 0 ? window.playerState.currentIndex : 0);
            }
        });
    } else {
        console.warn('Không tìm thấy .play-button');
    }

    // Xử lý nút play cho từng bài hát
    document.querySelectorAll('.track-play-button').forEach((button, index) => {
        button.addEventListener('click', async function (event) {
            event.preventDefault();
            const trackId = parseInt(button.getAttribute('data-track-id'));
            if (isNaN(trackId)) {
                console.error('data-track-id không hợp lệ:', button);
                return;
            }
            if (!window.playerState.queue.length) await fetchTracks();
            if (!window.playerState.queue.length) return;

            const trackIndex = window.playerState.queue.findIndex(track => track.id === trackId);
            if (trackIndex === -1) {
                console.error('Không tìm thấy bài hát với id:', trackId);
                return;
            }
            if (window.playerState.isPlaying && window.playerState.currentIndex === trackIndex) {
                window.playerState.audio.pause();
                window.playerState.isPlaying = false;
                window.updatePlayPauseButton(false);
            } else {
                playTrack(trackIndex);
            }
        });
    });

    // Xử lý nút next
    const nextBtn = document.querySelector('.next-btn');
    if (nextBtn) {
        nextBtn.addEventListener('click', () => {
            if (window.playerState.currentIndex < window.playerState.queue.length - 1) {
                playTrack(window.playerState.currentIndex + 1);
            } else if (window.playerState.repeatMode === 'repeat-all') {
                playTrack(0);
            }
        });
    } else {
        console.warn('Không tìm thấy .next-btn');
    }

    // Xử lý nút previous (đã sửa lỗi cú pháp)
    const prevBtn = document.querySelector('.previous-btn');
    if (prevBtn) {
        prevBtn.addEventListener('click', () => {
            if (window.playerState.currentIndex > 0) {
                playTrack(window.playerState.currentIndex - 1);
            } else if (window.playerState.repeatMode === 'repeat-all') {
                playTrack(window.playerState.queue.length - 1);
            }
        });
    } else {
        console.warn('Không tìm thấy .previous-btn');
    }

    // Xử lý nút repeat
    const repeatButton = document.querySelector('[data-testid="control-button-repeat"]');
    if (repeatButton) {
        repeatButton.addEventListener('click', () => {
            if (window.playerState.repeatMode === 'off') {
                window.playerState.repeatMode = 'repeat-all';
            } else if (window.playerState.repeatMode === 'repeat-all') {
                window.playerState.repeatMode = 'repeat-one';
            } else {
                window.playerState.repeatMode = 'off';
            }
            window.updateRepeatButton();
            console.log('Repeat mode updated to:', window.playerState.repeatMode);
        });
    } else {
        console.warn('Không tìm thấy nút repeat');
    }

    // ... (phần còn lại của sự kiện DOMContentLoaded giữ nguyên)

    // Xử lý chỉnh sửa playlist
    const editTitleButton = document.getElementById('editTitleButton');
    if (editTitleButton) editTitleButton.addEventListener('click', showEditModal);

    const editImageButton = document.getElementById('editImageButton');
    if (editImageButton) editImageButton.addEventListener('click', showEditModal);

    const closeModalButton = document.getElementById('closeModalButton');
    if (closeModalButton) closeModalButton.addEventListener('click', hideEditModal);

    const cancelModalButton = document.getElementById('cancelModalButton');
    if (cancelModalButton) cancelModalButton.addEventListener('click', hideEditModal);

    const imageUploadInput = document.getElementById('imageUploadInput');
    if (imageUploadInput) imageUploadInput.addEventListener('change', previewImage);

    const saveModalButton = document.getElementById('saveModalButton');
    if (saveModalButton) saveModalButton.addEventListener('click', savePlaylist);

    const modalOverlay = document.getElementById('editModal');
    if (modalOverlay) {
        modalOverlay.addEventListener('click', function (event) {
            if (event.target === modalOverlay) hideEditModal();
        });
    }

    const nameInput = document.getElementById('playlistNameInput');
    const nameCounter = document.getElementById('nameCounter');
    if (nameInput && nameCounter) {
        updateCharacterCounter(nameInput, nameCounter, 100);
        nameInput.addEventListener('input', () => updateCharacterCounter(nameInput, nameCounter, 100));
    }

    const descriptionInput = document.getElementById('playlistDescriptionInput');
    const descriptionCounter = document.getElementById('descriptionCounter');
    if (descriptionInput && descriptionCounter) {
        updateCharacterCounter(descriptionInput, descriptionCounter, 300);
        descriptionInput.addEventListener('input', () => updateCharacterCounter(descriptionInput, descriptionCounter, 300));
    }

    // Xử lý tìm kiếm
    initializeSearchEvents();

    const closeSectionButton = document.querySelector('.section-close-btn');
    const showSectionButton = document.querySelector('.search-show-btn');
    const searchContentWrapper = document.querySelector('.search-content-wrapper');
    if (closeSectionButton && showSectionButton && searchContentWrapper) {
        closeSectionButton.addEventListener('click', () => {
            searchContentWrapper.style.display = 'none';
            closeSectionButton.style.display = 'none';
            showSectionButton.style.display = 'block';
        });
        showSectionButton.addEventListener('click', () => {
            searchContentWrapper.style.display = 'flex';
            closeSectionButton.style.display = 'block';
            showSectionButton.style.display = 'none';
        });
    }

    const artistBackButton = document.querySelector('.artist-back-btn');
    if (artistBackButton) {
        artistBackButton.addEventListener('click', () => {
            document.querySelector('.artist-section').style.display = 'none';
            document.querySelector('.top-results').style.display = 'block';
            document.querySelector('.see-all-grid').style.display = 'block';
            document.querySelector('#artistSongs').innerHTML = '';
            document.querySelector('#artistAlbums').innerHTML = '';
        });
    }


    const menuItems = document.querySelectorAll('.context-menu-item');
    menuItems.forEach(item => {
        item.addEventListener('click', function () {
            const action = item.getAttribute('data-action');
            handleContextMenuAction(action);
        });
    });

    document.addEventListener('click', function (event) {
        const menu = document.getElementById('contextMenuTracks');
        if (menu && !menu.contains(event.target) && !event.target.closest('.track-more-button')) {
            menu.style.display = 'none';
            menu.classList.remove('active');
            isMenuOpen = false;
            currentTrackId = null;
        }
    });

    window.addEventListener('scroll', function () {
        const menu = document.getElementById('contextMenuTracks');
        if (menu && isMenuOpen) {
            menu.style.display = 'none';
            menu.classList.remove('active');
            isMenuOpen = false;
            currentTrackId = null;
        }
    });

    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape') {
            const menu = document.getElementById('contextMenuTracks');
            if (menu && isMenuOpen) {
                menu.style.display = 'none';
                menu.classList.remove('active');
                isMenuOpen = false;
                currentTrackId = null;
            }
        }
    });

    const contextMenuTracks = document.getElementById('contextMenuTracks');
    if (contextMenuTracks) contextMenuTracks.addEventListener('click', event => event.stopPropagation());
});

// Các hàm chỉnh sửa playlist
function showEditModal() {
    const modal = document.getElementById('editModal');
    if (modal) modal.style.display = 'flex';
    else console.error("Modal element not found");
}

function hideEditModal() {
    const modal = document.getElementById('editModal');
    if (modal) modal.style.display = 'none';
    else console.error("Modal element not found");
}

function previewImage(event) {
    const file = event.target.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = function (e) {
            const img = document.getElementById('previewImage');
            const previewContainer = document.getElementById('imagePreview');
            if (img) img.src = e.target.result;
            else if (previewContainer) previewContainer.innerHTML = `<img src="${e.target.result}" alt="Preview" id="previewImage" />`;
            else console.error("Image preview container not found");
        };
        reader.readAsDataURL(file);
    }
}

// Hàm cập nhật màu .playlist-header dựa trên ảnh
function applyImageBasedColor(imageElement) {
    const playlistHeader = document.querySelector(".playlist-header");
    if (!playlistHeader) {
        console.warn("Không tìm thấy .playlist-header");
        return;
    }

    if (typeof Vibrant === 'undefined') {
        console.warn("Vibrant.js không tải được, sử dụng gradient mặc định");
        playlistHeader.style.background = "linear-gradient(to bottom, #333333 0%, #121212 100%)";
        playlistHeader.style.color = "#ffffff";
        return;
    }

    // Nếu không có ảnh, dùng gradient mặc định và khôi phục ::before
    if (!imageElement || !imageElement.src) {
        playlistHeader.style.background = "linear-gradient(to bottom, #333333 0%, #121212 100%)";
        playlistHeader.style.color = "#ffffff";
        playlistHeader.style.setProperty('--custom-background', 'none'); // Khôi phục gradient mặc định
        return;
    }

    const img = new Image();
    img.crossOrigin = 'Anonymous';
    img.src = imageElement.src;

    img.onload = function () {
        try {
            const vibrant = new Vibrant(img, { quality: 1 });
            vibrant.getPalette().then((swatches) => {
                const color = swatches.Vibrant?.getHex() || swatches.Muted?.getHex() || "#333333";
                playlistHeader.style.background = color;

                // Tính độ sáng để điều chỉnh màu chữ
                const rgb = hexToRgb(color);
                const brightness = (rgb.r * 299 + rgb.g * 587 + rgb.b * 114) / 1000;
                const textColor = brightness > 128 ? "#000000" : "#ffffff";
                playlistHeader.style.color = textColor;

                // Vô hiệu hóa ::before để màu nền hiển thị
                playlistHeader.style.setProperty('--custom-background', color);

                console.log("Đã áp dụng màu từ ảnh:", color);
            }).catch((err) => {
                console.error("Lỗi khi trích xuất palette:", err);
                playlistHeader.style.background = "linear-gradient(to bottom, #333333 0%, #121212 100%)";
                playlistHeader.style.color = "#ffffff";
                playlistHeader.style.setProperty('--custom-background', 'none');
            });
        } catch (err) {
            console.error("Lỗi khi xử lý ảnh:", err);
            playlistHeader.style.background = "linear-gradient(to bottom, #333333 0%, #121212 100%)";
            playlistHeader.style.color = "#ffffff";
            playlistHeader.style.setProperty('--custom-background', 'none');
        }
    };

    img.onerror = function () {
        console.warn("Không tải được ảnh, sử dụng gradient mặc định:", img.src);
        playlistHeader.style.background = "linear-gradient(to bottom, #333333 0%, #121212 100%)";
        playlistHeader.style.color = "#ffffff";
        playlistHeader.style.setProperty('--custom-background', 'none');
    };
}

// Hàm chuyển đổi hex sang RGB
function hexToRgb(hex) {
    const bigint = parseInt(hex.slice(1), 16);
    return { r: (bigint >> 16) & 255, g: (bigint >> 8) & 255, b: bigint & 255 };
}

// Hàm lưu playlist (giữ nguyên logic)
function savePlaylist() {
    if (playlistId === 0) {
        alert("Không thể lưu playlist: Playlist ID không hợp lệ.");
        return;
    }
    const nameInput = document.getElementById('playlistNameInput')?.value;
    const descriptionInput = document.getElementById('playlistDescriptionInput')?.value;
    const imageInput = document.getElementById('imageUploadInput')?.files[0];
    const formData = new FormData();
    formData.append('name', nameInput || '');
    formData.append('description', descriptionInput || '');
    if (imageInput) formData.append('image', imageInput);

    fetch(`/Playlist/UpdatePlaylist/${playlistId}`, {
        method: 'POST',
        body: formData
    })
        .then(response => {
            if (!response.ok) throw new Error('Lỗi khi lưu playlist: ' + response.statusText);
            return response.json();
        })
        .then(data => {
            alert(data.message || 'Playlist đã được lưu!');

            const playlistImage = document.getElementById('playlistImage');
            if (imageInput && playlistImage) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    playlistImage.src = e.target.result;
                    applyImageBasedColor(playlistImage);
                };
                reader.readAsDataURL(imageInput);
            } else if (playlistImage && playlistImage.src) {
                applyImageBasedColor(playlistImage);
            } else {
                applyImageBasedColor(null);
            }
            location.reload();
        })
        .catch(error => {
            console.error('Lỗi khi lưu playlist:', error);
            alert('Có lỗi xảy ra khi lưu playlist: ' + error.message);
        });
}

// Phần còn lại của file giữ nguyên
function updateCharacterCounter(input, counter, maxLength) {
    if (input && counter) {
        const length = input.value.length;
        counter.textContent = `${length}/${maxLength}`;
    }
}

// Hàm tìm kiếm
function initializeSearchEvents() {
    const input = document.querySelector('.search-barPL input');
    const clearIcon = document.querySelector('.clear-iconPL');
    const searchResults = document.querySelector('.search-results');
    const topResultsContainer = document.querySelector('#topResults');

    if (input && clearIcon && searchResults && topResultsContainer) {
        function performSearch(searchTerm) {
            if (searchTerm.trim() === '') {
                searchResults.style.display = 'none';
                topResultsContainer.innerHTML = '';
                return;
            }

            fetch(`/Playlist/Search?searchTerm=${encodeURIComponent(searchTerm)}&playlistId=${playlistId}`, {
                method: 'GET',
                headers: {
                    'Accept': 'application/json'
                }
            })
                .then(response => {
                    if (!response.ok) {
                        return response.json().then(error => {
                            throw new Error(error.message || 'Lỗi khi tìm kiếm: ' + response.statusText);
                        });
                    }
                    return response.json();
                })
                .then(data => {
                    searchResults.style.display = 'block';

                    const topResults = [];
                    const addedIds = new Set();

                    let primaryType = 'song';
                    if (
                        data.artists &&
                        data.artists.length > 0 &&
                        data.artists[0] &&
                        typeof data.artists[0].name === 'string' &&
                        data.artists[0].name.toLowerCase() === searchTerm.toLowerCase()
                    ) {
                        primaryType = 'artist';
                    } else if (
                        data.albums &&
                        data.albums.length > 0 &&
                        data.albums[0] &&
                        typeof data.albums[0].name === 'string' &&
                        data.albums[0].name.toLowerCase() === searchTerm.toLowerCase()
                    ) {
                        primaryType = 'album';
                    }

                    if (primaryType === 'artist' && data.artists && data.artists.length > 0) {
                        const artist = { ...data.artists[0], type: 'artist' };
                        topResults.push(artist);
                        addedIds.add(`artist_${artist.id}`);
                    } else if (primaryType === 'album' && data.albums && data.albums.length > 0) {
                        const album = { ...data.albums[0], type: 'album' };
                        topResults.push(album);
                        addedIds.add(`album_${album.id}`);
                    } else if (data.songs && data.songs.length > 0) {
                        const song = { ...data.songs[0], type: 'song' };
                        topResults.push(song);
                        addedIds.add(`song_${song.id}`);
                    }

                    let remainingSlots = 6 - topResults.length;

                    if (data.songs && remainingSlots > 0) {
                        const otherSongs = data.songs.filter(song => !addedIds.has(`song_${song.id}`)).slice(0, remainingSlots);
                        otherSongs.forEach(song => {
                            topResults.push({ ...song, type: 'song' });
                            addedIds.add(`song_${song.id}`);
                        });
                        remainingSlots -= otherSongs.length;
                    }

                    if (data.artists && remainingSlots > 0) {
                        const otherArtists = data.artists.filter(artist => !addedIds.has(`artist_${artist.id}`)).slice(0, remainingSlots);
                        otherArtists.forEach(artist => {
                            topResults.push({ ...artist, type: 'artist' });
                            addedIds.add(`artist_${artist.id}`);
                        });
                        remainingSlots -= otherArtists.length;
                    }

                    if (data.albums && remainingSlots > 0) {
                        const otherAlbums = data.albums.filter(album => !addedIds.has(`album_${album.id}`)).slice(0, remainingSlots);
                        otherAlbums.forEach(album => {
                            topResults.push({ ...album, type: 'album' });
                            addedIds.add(`album_${album.id}`);
                        });
                    }

                    renderTopResults(topResults);
                })
                .catch(error => {
                    console.error('Lỗi khi tìm kiếm:', error);
                    searchResults.style.display = 'block';
                    topResultsContainer.innerHTML = `<p style="color: #B3B3B3;">Không thể tìm kiếm: ${error.message}</p>`;
                });
        }

        function renderTopResults(results) {
            topResultsContainer.innerHTML = '';
            results.forEach((item, index) => {
                const row = document.createElement('div');
                row.className = `grid-row ${item.type}`;
                row.setAttribute('aria-rowindex', index + 1);

                const imageSrc = item.imageUrl && typeof item.imageUrl === 'string' && (item.imageUrl.startsWith('http://') || item.imageUrl.startsWith('https://'))
                    ? `/Playlist/ProxyImage?url=${encodeURIComponent(item.imageUrl)}`
                    : (item.imageUrl || '/images/default-image.png');

                if (item.type === 'artist') {
                    row.innerHTML = `
                        <div class="grid-cell">
                            <div class="grid-cell-image artist">
                                ${imageSrc ? `<img src="${imageSrc}" alt="${item.name}">` : `
                                    <svg data-encore-id="icon" role="img" aria-hidden="true" viewBox="0 0 24 24">
                                        <path d="M12 2a5 5 0 0 0-5 5v2a5 5 0 0 0 3 4.563V16h4v-2.437A5 5 0 0 0 17 9V7a5 5 0 0 0-5-5zm0 2a3 3 0 0 1 3 3v2a3 3 0 0 1-3 3 3 3 0 0 1-3-3V7a3 3 0 0 1 3-3zm-7 9v6h14v-6h-2v2h-2v-2h-2v4h-2v-4H9v2H7v-2H5z"></path>
                                    </svg>
                                `}
                            </div>
                            <div class="grid-cell-info">
                                <p class="grid-cell-title">${item.name}</p>
                                <p class="grid-cell-subtitle">Nghệ sĩ</p>
                            </div>
                        </div>
                        <div class="grid-cell-action">
                            <button class="action-arrow-btn" aria-label="Xem thêm ${item.name}">
                                <svg data-encore-id="icon" role="img" aria-hidden="true" class="e-9812-icon e-9812-baseline e-9812-icon--auto-mirror" viewBox="0 0 24 24">
                                    <path d="M8.043 2.793a1 1 0 0 0 0 1.414L15.836 12l-7.793 7.793a1 1 0 1 0 1.414 1.414L18.664 12 9.457 2.793a1 1 0 0 0-1.414 0z"></path>
                                </svg>
                            </button>
                        </div>
                    `;
                } else if (item.type === 'song') {
                    row.innerHTML = `
                        <div class="grid-cell">
                            <div class="grid-cell-image">
                                ${imageSrc ? `<img src="${imageSrc}" alt="${item.title}">` : `
                                    <svg data-encore-id="icon" role="img" aria-hidden="true" viewBox="0 0 24 24">
                                        <path d="M6 3h15v15.167a3.5 3.5 0 1 1-3.5-3.5H19V5H8v13.167a3.5 3.5 0 1 1-3.5-3.5H6V3zm0 13.667H4.5a1.5 1.5 0 1 0 1.5 1.5v-1.5zm13 0h-1.5a1.5 1.5 0 1 0 1.5 1.5v-1.5z"></path>
                                    </svg>
                                `}
                                <div class="play-icon-overlay">
                                    <svg data-encore-id="icon" role="img" aria-hidden="true" class="e-9812-icon e-9812-baseline zOsKPnD_9x3KJqQCSmAq" viewBox="0 0 24 24">
                                        <path d="m7.05 3.606 13.49 7.788a.7.7 0 0 1 0 1.212L7.05 20.394A.7.7 0 0 1 6 19.788V4.212a.7.7 0 0 1 1.05-.606z"></path>
                                    </svg>
                                </div>
                            </div>
                            <div class="grid-cell-info">
                                <p class="grid-cell-title">${item.title}</p>
                                <p class="grid-cell-subtitle">${item.artist}</p>
                            </div>
                        </div>
                        <div class="grid-cell-title-repeat">${item.title}</div>
                        <div class="grid-cell-action">
                            <button class="action-add-btn" data-song-id="${item.id}">Thêm</button>
                        </div>
                    `;
                } else if (item.type === 'album') {
                    row.innerHTML = `
                        <div class="grid-cell">
                            <div class="grid-cell-image">
                                ${imageSrc ? `<img src="${imageSrc}" alt="${item.name}">` : `
                                    <svg data-encore-id="icon" role="img" aria-hidden="true" viewBox="0 0 24 24">
                                        <path d="M6 3h15v15.167a3.5 3.5 0 1 1-3.5-3.5H19V5H8v13.167a3.5 3.5 0 1 1-3.5-3.5H6V3zm0 13.667H4.5a1.5 1.5 0 1 0 1.5 1.5v-1.5zm13 0h-1.5a1.5 1.5 0 1 0 1.5 1.5v-1.5z"></path>
                                    </svg>
                                `}
                            </div>
                            <div class="grid-cell-info">
                                <p class="grid-cell-title">${item.name}</p>
                                <p class="grid-cell-subtitle">${item.artist}</p>
                            </div>
                        </div>
                        <div class="grid-cell-action">
                            <button class="action-arrow-btn" aria-label="Xem thêm ${item.name}">
                                <svg data-encore-id="icon" role="img" aria-hidden="true" class="e-9812-icon e-9812-baseline e-9812-icon--auto-mirror" viewBox="0 0 24 24">
                                    <path d="M8.043 2.793a1 1 0 0 0 0 1.414L15.836 12l-7.793 7.793a1 1 0 1 0 1.414 1.414L18.664 12 9.457 2.793a1 1 0 0 0-1.414 0z"></path>
                                </svg>
                            </button>
                        </div>
                    `;
                }

                topResultsContainer.appendChild(row);

                const addButton = row.querySelector('.action-add-btn');
                if (addButton) {
                    addButton.addEventListener('click', () => {
                        const songId = addButton.getAttribute('data-song-id');
                        addSongToPlaylist(songId);
                    });
                }

                const arrowButton = row.querySelector('.action-arrow-btn');
                if (arrowButton) {
                    arrowButton.addEventListener('click', () => {
                        console.log(`Arrow button clicked for ${item.type}: ${item.name}`);
                        if (item.type === 'album') {
                            fetchAlbumSongs(item.id, item.name);
                        } else if (item.type === 'artist') {
                            fetchArtistDetails(item.id, item.name);
                        }
                    });
                }
            });
        }

        function fetchAlbumSongs(albumId, albumName) {
            fetch(`/Playlist/GetAlbumSongs?albumId=${albumId}`, {
                method: 'GET',
                headers: {
                    'Accept': 'application/json'
                }
            })
                .then(response => {
                    if (!response.ok) {
                        return response.json().then(error => {
                            throw new Error(error.message || 'Không thể lấy bài hát: ' + response.statusText);
                        });
                    }
                    return response.json();
                })
                .then(data => {
                    document.querySelector('.top-results').style.display = 'none';
                    document.querySelector('.see-all-grid').style.display = 'none';
                    document.querySelector('.artist-section').style.display = 'none';

                    const albumSongsSection = document.querySelector('.album-songs-section');
                    albumSongsSection.style.display = 'block';

                    const albumSongsTitle = document.querySelector('.album-songs-title');
                    albumSongsTitle.textContent = albumName;

                    const albumSongsContainer = document.querySelector('#albumSongs');
                    albumSongsContainer.innerHTML = '';

                    data.forEach((song, index) => {
                        const row = document.createElement('div');
                        row.className = 'grid-row album-song';
                        row.setAttribute('aria-rowindex', index + 1);

                        const imageSrc = song.imageUrl && typeof song.imageUrl === 'string' && (song.imageUrl.startsWith('http://') || song.imageUrl.startsWith('https://'))
                            ? `/Playlist/ProxyImage?url=${encodeURIComponent(song.imageUrl)}`
                            : (song.imageUrl || '/images/default-image.png');

                        row.innerHTML = `
                            <div class="grid-cell">
                                <div class="grid-cell-image">
                                    ${imageSrc ? `<img src="${imageSrc}" alt="${song.title}">` : `
                                        <svg data-encore-id="icon" role="img" aria-hidden="true" viewBox="0 0 24 24">
                                            <path d="M6 3h15v15.167a3.5 3.5 0 1 1-3.5-3.5H19V5H8v13.167a3.5 3.5 0 1 1-3.5-3.5H6V3zm0 13.667H4.5a1.5 1.5 0 1 0 1.5 1.5v-1.5zm13 0h-1.5a1.5 1.5 0 1 0 1.5 1.5v-1.5z"></path>
                                        </svg>
                                    `}
                                    <div class="play-icon-overlay">
                                        <svg data-encore-id="icon" role="img" aria-hidden="true" class="e-9812-icon e-9812-baseline zOsKPnD_9x3KJqQCSmAq" viewBox="0 0 24 24">
                                            <path d="m7.05 3.606 13.49 7.788a.7.7 0 0 1 0 1.212L7.05 20.394A.7.7 0 0 1 6 19.788V4.212a.7.7 0 0 1 1.05-.606z"></path>
                                        </svg>
                                    </div>
                                </div>
                                <div class="grid-cell-info">
                                    <p class="grid-cell-title">${song.title}</p>
                                    <p class="grid-cell-subtitle">${song.artist}</p>
                                </div>
                            </div>
                            <div class="grid-cell-title-repeat">${albumName}</div>
                            <div class="grid-cell-action">
                                <button class="action-add-btn" data-song-id="${song.id}">Thêm</button>
                            </div>
                        `;

                        albumSongsContainer.appendChild(row);

                        const addButton = row.querySelector('.action-add-btn');
                        if (addButton) {
                            addButton.addEventListener('click', () => {
                                const songId = addButton.getAttribute('data-song-id');
                                addSongToPlaylist(songId);
                            });
                        }
                    });
                })
                .catch(error => {
                    console.error('Lỗi khi lấy danh sách bài hát của album:', error);
                    const albumSongsContainer = document.querySelector('#albumSongs');
                    albumSongsContainer.innerHTML = `<p style="color: #B3B3B3;">Không thể lấy bài hát: ${error.message}</p>`;
                });
        }

        function fetchArtistDetails(artistId, artistName) {
            document.querySelector('.top-results').style.display = 'none';
            document.querySelector('.see-all-grid').style.display = 'none';
            document.querySelector('.album-songs-section').style.display = 'none';

            const artistSection = document.querySelector('.artist-section');
            artistSection.style.display = 'block';

            const artistTitle = document.querySelector('.artist-title');
            artistTitle.textContent = artistName;

            fetch(`/Playlist/GetArtistDetails?artistId=${artistId}`, {
                method: 'GET',
                headers: {
                    'Accept': 'application/json'
                }
            })
                .then(response => {
                    if (!response.ok) {
                        return response.json().then(error => {
                            throw new Error(error.message || 'Không thể lấy thông tin nghệ sĩ: ' + response.statusText);
                        });
                    }
                    return response.json();
                })
                .then(data => {
                    const artistSongsContainer = document.querySelector('#artistSongs');
                    artistSongsContainer.innerHTML = '';
                    if (data.songs && data.songs.length > 0) {
                        data.songs.forEach((song, index) => {
                            const row = document.createElement('div');
                            row.className = 'grid-row song';
                            row.setAttribute('aria-rowindex', index + 1);

                            const imageSrc = song.imageUrl && typeof song.imageUrl === 'string' && (song.imageUrl.startsWith('http://') || song.imageUrl.startsWith('https://'))
                                ? `/Playlist/ProxyImage?url=${encodeURIComponent(song.imageUrl)}`
                                : (song.imageUrl || '/images/default-image.png');

                            row.innerHTML = `
                                <div class="grid-cell">
                                    <div class="grid-cell-image">
                                        ${imageSrc ? `<img src="${imageSrc}" alt="${song.title}">` : `
                                            <svg data-encore-id="icon" role="img" aria-hidden="true" viewBox="0 0 24 24">
                                                <path d="M6 3h15v15.167a3.5 3.5 0 1 1-3.5-3.5H19V5H8v13.167a3.5 3.5 0 1 1-3.5-3.5H6V3zm0 13.667H4.5a1.5 1.5 0 1 0 1.5 1.5v-1.5zm13 0h-1.5a1.5 1.5 0 1 0 1.5 1.5v-1.5z"></path>
                                            </svg>
                                        `}
                                        <div class="play-icon-overlay">
                                            <svg data-encore-id="icon" role="img" aria-hidden="true" class="e-9812-icon e-9812-baseline zOsKPnD_9x3KJqQCSmAq" viewBox="0 0 24 24">
                                                <path d="m7.05 3.606 13.49 7.788a.7.7 0 0 1 0 1.212L7.05 20.394A.7.7 0 0 1 6 19.788V4.212a.7.7 0 0 1 1.05-.606z"></path>
                                            </svg>
                                        </div>
                                    </div>
                                    <div class="grid-cell-info">
                                        <p class="grid-cell-title">${song.title || 'Không có tiêu đề'}</p>
                                        <p class="grid-cell-subtitle">${song.artist || 'Không có nghệ sĩ'}</p>
                                    </div>
                                </div>
                                <div class="grid-cell-action">
                                    <button class="action-add-btn" data-song-id="${song.id || ''}">Thêm</button>
                                </div>
                            `;

                            artistSongsContainer.appendChild(row);

                            const addButton = row.querySelector('.action-add-btn');
                            if (addButton) {
                                addButton.addEventListener('click', () => {
                                    const songId = addButton.getAttribute('data-song-id');
                                    addSongToPlaylist(songId);
                                });
                            }
                        });
                    } else {
                        artistSongsContainer.innerHTML = `<p style="color: #B3B3B3;">Không có bài hát nào.</p>`;
                    }

                    const artistAlbumsContainer = document.querySelector('#artistAlbums');
                    artistAlbumsContainer.innerHTML = '';
                    if (data.albums && data.albums.length > 0) {
                        data.albums.forEach((album, index) => {
                            const row = document.createElement('div');
                            row.className = 'grid-row album-song';
                            row.setAttribute('aria-rowindex', index + 1);

                            const imageSrc = album.imageUrl && typeof album.imageUrl === 'string' && (album.imageUrl.startsWith('http://') || album.imageUrl.startsWith('https://'))
                                ? `/Playlist/ProxyImage?url=${encodeURIComponent(album.imageUrl)}`
                                : (album.imageUrl || '/images/default-album.png');

                            row.innerHTML = `
                                <div class="grid-cell">
                                    <div class="grid-cell-image">
                                        ${imageSrc ? `<img src="${imageSrc}" alt="${album.name}">` : `
                                            <svg data-encore-id="icon" role="img" aria-hidden="true" viewBox="0 0 24 24">
                                                <path d="M6 3h15v15.167a3.5 3.5 0 1 1-3.5-3.5H19V5H8v13.167a3.5 3.5 0 1 1-3.5-3.5H6V3zm0 13.667H4.5a1.5 1.5 0 1 0 1.5 1.5v-1.5zm13 0h-1.5a1.5 1.5 0 1 0 1.5 1.5v-1.5z"></path>
                                            </svg>
                                        `}
                                        <div class="play-icon-overlay">
                                            <svg data-encore-id="icon" role="img" aria-hidden="true" class="e-9812-icon e-9812-baseline zOsKPnD_9x3KJqQCSmAq" viewBox="0 0 24 24">
                                                <path d="m7.05 3.606 13.49 7.788a.7.7 0 0 1 0 1.212L7.05 20.394A.7.7 0 0 1 6 19.788V4.212a.7.7 0 0 1 1.05-.606z"></path>
                                            </svg>
                                        </div>
                                    </div>
                                    <div class="grid-cell-info">
                                        <p class="grid-cell-title">${album.name || 'Không có tiêu đề'}</p>
                                        <p class="grid-cell-subtitle">${album.artist || 'Không có nghệ sĩ'}</p>
                                    </div>
                                </div>
                                <div class="grid-cell-action">
                                    <button class="action-arrow-btn" aria-label="Xem chi tiết album" data-album-id="${album.id || ''}">
                                        <svg data-encore-id="icon" role="img" aria-hidden="true" class="e-9812-icon e-9812-baseline e-9812-icon--auto-mirror" viewBox="0 0 24 24">
                                            <path d="M8.043 2.793a1 1 0 0 0 0 1.414L15.836 12l-7.793 7.793a1 1 0 1 0 1.414 1.414L18.664 12 9.457 2.793a1 1 0 0 0-1.414 0z"></path>
                                        </svg>
                                    </button>
                                </div>
                            `;

                            artistAlbumsContainer.appendChild(row);

                            const arrowButton = row.querySelector('.action-arrow-btn');
                            if (arrowButton) {
                                arrowButton.addEventListener('click', () => {
                                    const albumId = arrowButton.getAttribute('data-album-id');
                                    fetchAlbumSongs(albumId, album.name);
                                });
                            }
                        });
                    } else {
                        artistAlbumsContainer.innerHTML = `<p style="color: #B3B3B3;">Không có album nào.</p>`;
                    }
                })
                .catch(error => {
                    console.error('Lỗi khi lấy thông tin nghệ sĩ:', error);
                    const artistSongsContainer = document.querySelector('#artistSongs');
                    artistSongsContainer.innerHTML = `<p style="color: #B3B3B3;">Không thể lấy thông tin: ${error.message}</p>`;
                    const artistAlbumsContainer = document.querySelector('#artistAlbums');
                    artistAlbumsContainer.innerHTML = `<p style="color: #B3B3B3;">Không thể lấy thông tin: ${error.message}</p>`;
                });
        }

        function addSongToPlaylist(songId) {
            const token = localStorage.getItem('jwtToken'); // Example: Retrieve token from storage
            fetch(`/Playlist/AddSong?playlistId=${playlistId}&songId=${songId}`, {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': token ? `Bearer ${token}` : '' // Include JWT if available
                }
            })
                .then(response => {
                    if (!response.ok) {
                        return response.json().then(error => {
                            throw new Error(error.message || 'Không thể thêm bài hát');
                        });
                    }
                    return response.json();
                })
                .then(data => {
                    alert(data.message || 'Bài hát đã được thêm vào playlist!');
                    fetchTracks().then(() => {
                        renderTracklist();
                        if (window.playerState.currentTrack && window.playerState.currentTrack.id === songId) {
                            window.updateNowPlayingBar(window.playerState.currentTrack);
                        }
                    });
                })
                .catch(error => {
                    console.error('Lỗi khi thêm bài hát:', error);
                    alert('Không thể thêm bài hát: ' + error.message);
                });
        }
        let searchTimeout;
        input.addEventListener('input', () => {
            clearIcon.style.display = input.value ? 'block' : 'none';
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                performSearch(input.value);
            }, 500);
        });

        clearIcon.addEventListener('click', () => {
            input.value = '';
            clearIcon.style.display = 'none';
            searchResults.style.display = 'none';
            topResultsContainer.innerHTML = '';
        });

        clearIcon.style.display = input.value ? 'block' : 'none';
        if (input.value) {
            performSearch(input.value);
        }

        const backButton = document.querySelector('.album-back-btn');
        if (backButton) {
            backButton.addEventListener('click', () => {
                document.querySelector('.album-songs-section').style.display = 'none';
                document.querySelector('.artist-section').style.display = 'block';
                document.querySelector('#albumSongs').innerHTML = '';
            });
        } else {
            console.error("Back button not found in IndexPlayList");
        }
    } else {
        console.error("Search input, clear icon, search results, or top results container not found in IndexPlayList");
    }
}

// Hàm context menu
function showContextMenuTracks(event, trackId) {
    event.preventDefault(); // Ngăn chặn hành vi mặc định (như click chuột phải)
    event.stopPropagation(); // Ngăn sự kiện lan ra ngoài

    const menu = document.getElementById('contextMenuTracks');
    const button = event.currentTarget;

    if (!menu || !button) {
        console.error('Context menu or button not found:', { menu, button });
        return;
    }

    // Kiểm tra nếu menu đã mở và nhấp lại cùng trackId
    if (isMenuOpen && currentTrackId === trackId) {
        menu.style.display = 'none';
        menu.classList.remove('active');
        isMenuOpen = false;
        currentTrackId = null;
        return;
    }

    // Cập nhật trackId hiện tại
    currentTrackId = trackId;

    // Hiển thị menu tạm thời để lấy kích thước
    menu.style.display = 'block';
    menu.style.visibility = 'hidden';

    // Lấy vị trí của nút và kích thước menu
    const buttonRect = button.getBoundingClientRect();
    const menuWidth = menu.offsetWidth;
    const menuHeight = menu.offsetHeight;
    const scrollX = window.scrollX || window.pageXOffset;
    const scrollY = window.scrollY || window.pageYOffset;

    // Tính toán vị trí menu dựa trên vị trí nút
    let menuX = buttonRect.left + scrollX - menuWidth + buttonRect.width; // Xuất hiện bên phải nút
    let menuY = buttonRect.top + scrollY; // Xuất hiện từ đỉnh nút

    // Điều chỉnh nếu menu tràn ra ngoài cửa sổ
    if (menuX < scrollX) menuX = buttonRect.left + scrollX; // Đảm bảo không tràn trái
    if (menuY + menuHeight > scrollY + window.innerHeight) {
        menuY = buttonRect.bottom + scrollY - menuHeight; // Đặt trên nút nếu tràn dưới
    }
    if (menuY < scrollY) menuY = scrollY; // Đảm bảo không tràn trên

    // Áp dụng vị trí
    menu.style.left = `${menuX}px`;
    menu.style.top = `${menuY}px`;
    menu.style.visibility = 'visible';

    // Thêm class active sau một khoảng thời gian ngắn để tạo hiệu ứng
    setTimeout(() => {
        menu.classList.add('active');
    }, 10);

    isMenuOpen = true;
}

function hideContextMenuTracks() {
    const menu = document.getElementById('contextMenuTracks');
    if (menu) {
        menu.classList.remove('active');
        setTimeout(() => menu.style.display = 'none', 100);
    }
    currentTrackId = null;
}

function handleContextMenuAction(action) {
    if (!currentTrackId) {
        console.error("No track selected");
        return;
    }
    switch (action) {
        case 'remove-from-playlist':
            if (playlistId === 0) {
                alert("Không thể xóa: Playlist ID không hợp lệ.");
                return;
            }
            fetch(`/Playlist/RemoveSong?playlistId=${playlistId}&songId=${currentTrackId}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' }
            })
                .then(response => {
                    if (!response.ok) return response.json().then(error => { throw new Error(error.message || 'Không thể xóa bài hát'); });
                    return response.json();
                })
                .then(data => {
                    alert(data.message || 'Bài hát đã được xóa khỏi playlist!');
                    // Cập nhật queue và render lại tracklist
                    fetchTracks().then(() => {
                        renderTracklist();
                    });
                })
                .catch(error => {
                    console.error('Lỗi khi xóa bài hát:', error);
                    alert('Không thể xóa bài hát: ' + error.message);
                });
            break;
        // Các case khác giữ nguyên
        case 'add-to-playlist':
            alert("Chức năng thêm vào danh sách phát chưa được triển khai.");
            break;
        case 'add-to-liked-songs':
        case 'add-to-queue':
        case 'go-to-radio':
        case 'view-credits':
        case 'share':
        case 'open-in-desktop':
            alert(`Chức năng ${action} chưa được triển khai.`);
            break;
        case 'go-to-artist':
        case 'go-to-album':
            fetch(`/Playlist/GetTrackDetails?trackId=${currentTrackId}`)
                .then(response => response.json())
                .then(data => {
                    const id = action === 'go-to-artist' ? data.artistId : data.albumId;
                    if (id) window.location.href = `/${action === 'go-to-artist' ? 'artist' : 'album'}/${id}`;
                    else alert(`Không tìm thấy thông tin ${action === 'go-to-artist' ? 'nghệ sĩ' : 'album'}.`);
                })
                .catch(error => {
                    console.error('Lỗi khi lấy thông tin bài hát:', error);
                    alert(`Không thể chuyển tới ${action === 'go-to-artist' ? 'nghệ sĩ' : 'album'}: ${error.message}`);
                });
            break;
        default:
            console.warn("Unknown action:", action);
    }
    hideContextMenuTracks();
}
// Hàm render danh sách bài hát trong tracklist-body
function renderTracklist() {
    const tracklistBody = document.querySelector('.tracklist-body');
    if (!tracklistBody) {
        console.error('Không tìm thấy .tracklist-body');
        return;
    }

    tracklistBody.innerHTML = '';

    if (!window.playerState.queue || window.playerState.queue.length === 0) {
        tracklistBody.innerHTML = `
            <div class="empty-playlist-message">
                Danh sách phát trống. Hãy thêm một số bài hát!
            </div>
        `;
        return;
    }

    window.playerState.queue.forEach((track, index) => {
        let timeAgo = 'Chưa có thông tin';
        if (track.addedDate) {
            const addedAt = new Date(track.addedDate);
            if (!isNaN(addedAt.getTime())) {
                const timeDifference = (Date.now() - addedAt.getTime()) / 1000 / 60;
                if (timeDifference < 60) {
                    timeAgo = `${Math.floor(timeDifference)} phút trước`;
                } else if (timeDifference < 1440) {
                    timeAgo = `${Math.floor(timeDifference / 60)} giờ trước`;
                } else {
                    timeAgo = `${Math.floor(timeDifference / 1440)} ngày trước`;
                }
            }
        }

        const durationMinutes = Math.floor((track.duration || 0) / 60);
        const durationSeconds = (track.duration || 0) % 60;
        const durationFormatted = `${durationMinutes}:${durationSeconds.toString().padStart(2, '0')}`;

        // Hiển thị danh sách nghệ sĩ
        let artistsHtml = 'Không có nghệ sĩ';
        if (track.trackArtists && Array.isArray(track.trackArtists) && track.trackArtists.length > 0) {
            console.log(`Track ${track.title} artists:`, track.trackArtists); // Thêm log để debug
            artistsHtml = track.trackArtists.map((ta, i) => {
                const artistName = ta.artist?.name || 'Unknown';
                const artistId = ta.artist?.id || '';
                return `<a href="/artist/${artistId}">${artistName}</a>${i < track.trackArtists.length - 1 ? ', ' : ''}`;
            }).join('');
        }

        let albumHtml = 'Không có album';
        if (track.album && track.album.name) {
            albumHtml = `<a href="/Home/DetailAlbum/${track.album.id || ''}">${track.album.name}</a>`;
        }

        const row = document.createElement('div');
        row.className = 'tracklist-row';
        row.setAttribute('aria-rowindex', index + 1);
        row.innerHTML = `
            <div class="tracklist-col tracklist-col-index">
                <span class="index-number">${index + 1}</span>
                <button class="track-play-button" data-track-id="${track.id || ''}">
                    <svg data-encore-id="icon" role="img" aria-hidden="true" class="e-9812-icon e-9812-baseline zOsKPnD_9x3KJqQCSmAq" viewBox="0 0 24 24">
                        <path d="m7.05 3.606 13.49 7.788a.7.7 0 0 1 0 1.212L7.05 20.394A.7.7 0 0 1 6 19.788V4.212a.7.7 0 0 1 1.05-.606z"></path>
                    </svg>
                </button>
            </div>
            <div class="tracklist-col track-title">
                <a href="#" data-track-id="${track.id || ''}">${track.title || 'Không có tiêu đề'}</a>
            </div>
            <div class="tracklist-col track-artist">
                ${artistsHtml}
            </div>
            <div class="tracklist-col track-album">
                ${albumHtml}
            </div>
            <div class="tracklist-col track-date-added">${timeAgo}</div>
            <div class="tracklist-col tracklist-col-duration">
                <button class="track-action-button track-add-button" data-action="add-to-liked-songs" data-track-id="${track.id || ''}">
                    <svg data-encore-id="icon" role="img" aria-hidden="true" viewBox="0 0 16 16">
                        <path d="M1.69 2A4.582 4.582 0 0 1 8 2.023 4.583 4.583 0 0 1 11.88.817h.002a4.618 4.618 0 0 1 3.782 3.65v.003a4.543 4.543 0 0 1-1.011 3.84L8.35 14.629a1.765 1.765 0 0 1-2.093 0l-.094-.08-6.148-6.32A4.58 4.58 0 0 1 1.69 2z"></path>
                    </svg>
                </button>
                <span class="track-duration">${durationFormatted}</span>
                <button class="track-action-button track-more-button" data-track-id="${track.id || ''}">
                    <svg data-encore-id="icon" role="img" aria-hidden="true" viewBox="0 0 16 16">
                        <path d="M3 8a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0zm6.5 0a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0zM16 8a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0z"></path>
                    </svg>
                </button>
            </div>
        `;

        tracklistBody.appendChild(row);
    });

    // Gắn sự kiện delegated cho .track-more-button và .track-play-button (giữ nguyên)
    tracklistBody.addEventListener('click', async (event) => {
        const moreButton = event.target.closest('.track-more-button');
        if (moreButton) {
            event.stopPropagation();
            const trackId = moreButton.getAttribute('data-track-id');
            showContextMenuTracks(event, trackId);
        }

        const playButton = event.target.closest('.track-play-button');
        if (playButton) {
            event.preventDefault();
            const trackId = parseInt(playButton.getAttribute('data-track-id'));
            if (isNaN(trackId)) {
                console.error('data-track-id không hợp lệ:', playButton);
                return;
            }
            if (!window.playerState.queue.length) await fetchTracks();
            if (!window.playerState.queue.length) return;

            const trackIndex = window.playerState.queue.findIndex(t => t.id === trackId);
            if (trackIndex === -1) {
                console.error('Không tìm thấy bài hát với id:', trackId);
                return;
            }
            if (window.playerState.isPlaying && window.playerState.currentIndex === trackIndex) {
                window.playerState.audio.pause();
                window.playerState.isPlaying = false;
                window.updatePlayPauseButton(false);
            } else {
                playTrack(trackIndex);
            }
        }
        // Thêm sự kiện click cho cột Tiêu đề
        const titleLink = event.target.closest('.track-title a');
        if (titleLink) {
            event.preventDefault();
            const trackId = titleLink.getAttribute('data-track-id');
            if (trackId) {
                window.location.href = `/track/Detail/${trackId}`;
            }
        }
    });
}