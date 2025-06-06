document.addEventListener('DOMContentLoaded', function () {
    const track = window.trackData;
    if (!track) {
        console.error('Không tìm thấy window.trackData');
        return;
    }
    console.log('Track data:', track);

    // Áp dụng gradient dựa trên ảnh bìa
    applyImageBasedGradient(track.imageUrl);

    // Xử lý nút play trong action-bar
    const playButton = document.querySelector('.action-bar .play-button');
    if (playButton) {
        playButton.addEventListener('click', function () {
            if (window.playerState.isPlaying && window.playerState.currentTrack?.trackId === track.trackId) {
                window.playerState.audio.pause();
                window.playerState.isPlaying = false;
                window.updatePlayPauseButton(false);
            } else {
                playTrack(track);
            }
        });
    } else {
        console.warn('Không tìm thấy .play-button');
    }

    // Xử lý nút "Xem thêm" cho lyrics
    const lyricsText = document.querySelector('.lyrics-text');
    const moreToggleBtn = document.querySelector('.more-toggle-btn');

    if (lyricsText && moreToggleBtn) {
        const fullLyrics = lyricsText.getAttribute('data-full-lyrics') || '';
        const rawLines = fullLyrics.split('\n').filter(line => line.trim() !== '');
        const lyricsLines = [];
        let currentLine = '';

        rawLines.forEach(segment => {
            if (segment.length < 5 || !/[.,!?]$/.test(segment.trim())) {
                currentLine += (currentLine ? ' ' : '') + segment.trim();
            } else {
                if (currentLine) {
                    lyricsLines.push(currentLine);
                }
                currentLine = segment.trim();
            }
        });
        if (currentLine) {
            lyricsLines.push(currentLine);
        }

        console.log('Processed lyrics lines:', lyricsLines);

        const initialLinesCount = Math.ceil(lyricsLines.length / 3);

        function renderLyrics(lines) {
            const midPoint = Math.ceil(lines.length / 2);
            const leftColumnLines = lines.slice(0, midPoint);
            const rightColumnLines = lines.slice(midPoint);

            lyricsText.innerHTML = `
            <div class="lyrics-column lyrics-left">
                ${leftColumnLines.map(line => `
                    <p class="lyrics-line">${line}</p>
                `).join('')}
            </div>
            <div class="lyrics-column lyrics-right">
                ${rightColumnLines.map(line => `
                    <p class="lyrics-line">${line}</p>
                `).join('')}
            </div>
        `;
        }

        renderLyrics(lyricsLines.slice(0, initialLinesCount));

        moreToggleBtn.addEventListener('click', function () {
            if (lyricsText.classList.contains('expanded')) {
                renderLyrics(lyricsLines.slice(0, initialLinesCount));
                lyricsText.classList.remove('expanded');
                this.querySelector('.toggle-text').textContent = '...Xem thêm';
            } else {
                renderLyrics(lyricsLines);
                lyricsText.classList.add('expanded');
                this.querySelector('.toggle-text').textContent = 'Hiển thị ít hơn';
            }
        });
    }

    // Xử lý nút More và menu ngữ cảnh
    setupContextMenu(track);

    // Xử lý form Add to Liked Songs
    const addToLikedForm = document.getElementById('addToLikedForm');
    if (addToLikedForm) {
        addToLikedForm.addEventListener('submit', function (event) {
            event.preventDefault();
            fetch(this.action, {
                method: 'POST',
                body: new FormData(this),
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                }
            })
                .then(response => {
                    alert(response.ok
                        ? 'Bài hát đã được thêm vào Bài hát đã thích!'
                        : 'Có lỗi xảy ra khi thêm bài hát vào Bài hát đã thích.');
                })
                .catch(error => {
                    console.error('Error:', error);
                    alert('Có lỗi xảy ra khi thêm bài hát: ' + error.message);
                });
        });
    }
});

// Áp dụng gradient dựa trên ảnh bìa
function applyImageBasedGradient(imageUrl) {
    if (typeof Vibrant === 'undefined' || !window.vibrantLoaded) {
        console.warn('Vibrant.js không tải, sử dụng gradient ngẫu nhiên:', { imageUrl });
        applyRandomGradient();
        return;
    }

    if (!imageUrl) {
        console.warn('Không có ảnh bìa hợp lệ, sử dụng gradient ngẫu nhiên:', { imageUrl });
        applyRandomGradient();
        return;
    }

    const img = new Image();
    img.crossOrigin = 'Anonymous';
    img.src = imageUrl;

    img.onload = function () {
        try {
            const vibrant = new Vibrant(img, { quality: 1 });
            const swatches = vibrant.swatches();
            const color = swatches.Vibrant?.getHex() || swatches.Muted?.getHex() || '#5a1a1a';
            const adjustedColor = adjustColor(color, 0.7, 0.5);
            const gradient = `linear-gradient(to bottom, ${adjustedColor} 0%, ${adjustedColor} 10%, #181818 50%)`;

            let styleElement = document.getElementById('dynamic-gradient');
            if (!styleElement) {
                styleElement = document.createElement('style');
                styleElement.id = 'dynamic-gradient';
                document.head.appendChild(styleElement);
            }
            styleElement.textContent = `.main-view { background: ${gradient} !important; }`;
            console.log('Gradient áp dụng từ ảnh:', gradient);
        } catch (err) {
            console.error('Lỗi xử lý ảnh hoặc lấy palette:', err);
            applyRandomGradient();
        }
    };

    img.onerror = function () {
        console.warn('Không tải được ảnh bìa, sử dụng gradient ngẫu nhiên:', imageUrl);
        applyRandomGradient();
    };
}

// Hàm gradient ngẫu nhiên
function applyRandomGradient() {
    const letters = '0123456789ABCDEF';
    let color = '#';
    for (let i = 0; i < 6; i++) {
        color += letters[Math.floor(Math.random() * 16)];
    }
    console.log('Màu ngẫu nhiên được tạo:', color);
    const gradient = `linear-gradient(to bottom, ${color} 0%, ${color} 10%, #181818 50%)`;

    let styleElement = document.getElementById('dynamic-gradient');
    if (!styleElement) {
        styleElement = document.createElement('style');
        styleElement.id = 'dynamic-gradient';
        document.head.appendChild(styleElement);
    }
    styleElement.textContent = `.main-view { background: ${gradient} !important; }`;
    console.log('Gradient ngẫu nhiên áp dụng:', gradient);
}

// Gradient dự phòng
function applyFallbackGradient() {
    const gradient = `linear-gradient(to bottom, #5a1a1a 0%, #181818 40%)`;
    let styleElement = document.getElementById('dynamic-gradient');
    if (!styleElement) {
        styleElement = document.createElement('style');
        styleElement.id = 'dynamic-gradient';
        document.head.appendChild(styleElement);
    }
    styleElement.textContent = `.main-view { background: ${gradient} !important; }`;
    console.log('Áp dụng gradient dự phòng:', gradient);
}

// Điều chỉnh màu
function adjustColor(hex, brightnessFactor, saturationFactor) {
    const rgb = hexToRgb(hex);
    const hsl = rgbToHsl(rgb.r, rgb.g, rgb.b);
    hsl.l = Math.max(0, Math.min(1, hsl.l * brightnessFactor));
    hsl.s = Math.max(0, Math.min(1, hsl.s * saturationFactor));
    const adjustedRgb = hslToRgb(hsl.h, hsl.s, hsl.l);
    return rgbToHex(adjustedRgb.r, adjustedRgb.g, adjustedRgb.b);
}

// Chuyển đổi màu
function hexToRgb(hex) {
    const bigint = parseInt(hex.slice(1), 16);
    return { r: (bigint >> 16) & 255, g: (bigint >> 8) & 255, b: bigint & 255 };
}

function rgbToHsl(r, g, b) {
    r /= 255; g /= 255; b /= 255;
    const max = Math.max(r, g, b), min = Math.min(r, g, b);
    let h, s, l = (max + min) / 2;
    if (max === min) {
        h = s = 0;
    } else {
        const d = max - min;
        s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
        switch (max) {
            case r: h = (g - b) / d + (g < b ? 6 : 0); break;
            case g: h = (b - r) / d + 2; break;
            case b: h = (r - g) / d + 4; break;
        }
        h /= 6;
    }
    return { h, s, l };
}

function hslToRgb(h, s, l) {
    let r, g, b;
    if (s === 0) {
        r = g = b = l;
    } else {
        const hue2rgb = (p, q, t) => {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1 / 6) return p + (q - p) * 6 * t;
            if (t < 1 / 2) return q;
            if (t < 2 / 3) return p + (q - p) * (2 / 3 - t) * 6;
            return p;
        };
        const q = l < 0.5 ? l * (1 + s) : l + s - l * s;
        const p = 2 * l - q;
        r = hue2rgb(p, q, h + 1 / 3);
        g = hue2rgb(p, q, h);
        b = hue2rgb(p, q, h - 1 / 3);
    }
    return { r: Math.round(r * 255), g: Math.round(g * 255), b: Math.round(b * 255) };
}

function rgbToHex(r, g, b) {
    return `#${((1 << 24) + (r << 16) + (g << 8) + b).toString(16).slice(1).toUpperCase()}`;
}

// Phát bài hát
function playTrack(track) {
    window.playerState.currentTrack = track;
    if (!window.updateNowPlayingBar(track)) return;

    window.playerState.audio.src = track.audioUrl;
    window.playerState.audio.play().then(() => {
        window.playerState.isPlaying = true;
        window.updatePlayPauseButton(true);
        console.log('Đang phát:', track.title);
    }).catch(error => {
        console.error('Lỗi phát bài:', error);
        alert('Lỗi khi phát bài hát: ' + error.message);
        window.playerState.isPlaying = false;
        window.updatePlayPauseButton(false);
    });
}

// Thiết lập menu ngữ cảnh
function setupContextMenu(track) {
    let moreMenu = document.getElementById('moreMenu');
    if (!moreMenu) {
        moreMenu = document.createElement('div');
        moreMenu.id = 'moreMenu';
        moreMenu.className = 'context-menu track-more-menu';
        moreMenu.style.display = 'none';
        moreMenu.innerHTML = `
            <ul class="context-menu-list">
                <li class="context-menu-item add-to-playlist-item">
                    <button class="add-to-playlist-btn">
                        <svg data-encore-id="icon" role="img" aria-hidden="true" class="e-9812-icon e-9812-baseline" viewBox="0 0 16 16">
                            <path d="M15.25 8a.75.75 0 0 1-.75.75H8.75v5.75a.75.75 0 0 1-1.5 0V8.75H1.5a.75.75 0 0 1 0-1.5h5.75V1.5a.75.75 0 0 1 1.5 0v5.75h5.75a.75.75 0 0 1 .75.75z"></path>
                        </svg>
                        Thêm vào danh sách phát
                    </button>
                </li>
                <li class="context-menu-item">
                    <button onclick="addToFavorites(${track.trackId})">
                        <svg data-encore-id="icon" role="img" aria-hidden="true" class="e-9890-icon e-9890-baseline" viewBox="0 0 16 16">
                            <path d="M8 1.5a6.5 6.5 0 1 0 0 13 6.5 6.5 0 0 0 0-13zM0 8a8 8 0 1 1 16 0A8 8 0 0 1 0 8z"></path>
                            <path d="M11.75 8a.75.75 0 0 1-.75.75H8.75V11a.75.75 0 0 1-1.5 0V8.75H5a.75.75 0 0 1 0-1.5h2.25V5a.75.75 0 0 1 1.5 0v2.25H11a.75.75 0 0 1 .75.75z"></path>
                        </svg>
                        Thêm vào mục yêu thích
                    </button>
                </li>
                <li class="context-menu-item">
                    <button onclick="addToQueue(${track.trackId})">
                        <svg data-encore-id="icon" role="img" aria-hidden="true" class="e-9812-icon e-9812-baseline" viewBox="0 0 16 16">
                            <path d="M16 15H2v-1.5h14V15zm0-4.5H2V9h14v1.5zm-8.034-6A5.484 5.484 0 0 1 7.187 6H13.5a2.5 2.5 0 0 0 0-5H7.966c.159.474.255.978.278 1.5H13.5a1 1 0 1 1 0 2H7.966zM2 2V0h1.5v2h2v1.5h-2v2H2v-2H0V2h2z"></path>
                        </svg>
                        Thêm vào danh sách chờ
                    </button>
                </li>
                <li class="context-menu-divider"></li>
                <li class="context-menu-item">
                    <button>
                        <svg data-encore-id="icon" role="img" aria-hidden="true" class="e-9890-icon e-9890-baseline" viewBox="0 0 16 16">
                            <path d="M5.624 3.886A4.748 4.748 0 0 0 3.25 8c0 1.758.955 3.293 2.375 4.114l.75-1.3a3.249 3.249 0 0 1 0-5.63l-.75-1.298zm4.001 1.299.75-1.3A4.748 4.748 0 0 1 12.75 8a4.748 4.748 0 0 1-2.375 4.114l-.75-1.3a3.249 3.249 0 0 0 0-5.63zM8 6.545a1.455 1.455 0 1 0 0 2.91 1.455 1.455 0 0 0 0-2.91z"></path>
                            <path d="M4 1.07A7.997 7.997 0 0 0 0 8a7.997 7.997 0 0 0 4 6.93l.75-1.3A6.497 6.497 0 0 1 1.5 8a6.497 6.497 0 0 1 3.25-5.63L4 1.07zm7.25 1.3.75-1.3A7.997 7.997 0 0 1 16 8a7.997 7.997 0 0 1-3.999 6.93l-.75-1.3A6.497 6.497 0 0 0 14.5 8a6.497 6.497 0 0 0-3.25-5.63z"></path>
                        </svg>
                        Chuyển đến radio theo bài hát
                    </button>
                </li>
                <li class="context-menu-item">
                    <button>
                        <svg data-encore-id="icon" role="img" aria-hidden="true" class="e-9890-icon e-9890-baseline" viewBox="0 0 16 16">
                            <path d="M11.757 2.987A4.356 4.356 0 0 0 7.618 0a4.362 4.362 0 0 0-4.139 2.987 5.474 5.474 0 0 0-.22 1.894 5.604 5.604 0 0 0 1.4 3.312l.125.152a.748.748 0 0 1-.2 1.128l-2.209 1.275A4.748 4.748 0 0 0 0 14.857v1.142h8.734A5.48 5.48 0 0 1 8.15 14.5H1.517a3.245 3.245 0 0 1 1.6-2.454l2.21-1.275a2.25 2.25 0 0 0 .6-3.386l-.128-.153a4.112 4.112 0 0 1-1.05-2.44A4.053 4.053 0 0 1 4.89 3.47a2.797 2.797 0 0 1 1.555-1.713 2.89 2.89 0 0 1 3.293.691c.265.296.466.644.589 1.022.12.43.169.876.144 1.322a4.12 4.12 0 0 1-1.052 2.44l-.127.153a2.239 2.239 0 0 0-.2 2.58c.338-.45.742-.845 1.2-1.173 0-.162.055-.32.156-.447l.126-.152a5.598 5.598 0 0 0 1.4-3.312 5.499 5.499 0 0 0-.218-1.894zm3.493 3.771a.75.75 0 0 0-.75.75v3.496h-1a2.502 2.502 0 0 0-2.31 1.542 2.497 2.497 0 0 0 1.822 3.406A2.502 2.502 0 0 0 16 13.502V7.508a.75.75 0 0 0-.75-.75zm-.75 6.744a.998.998 0 0 1-1.707.707 1 1 0 0 1 .707-1.706h1v1z"></path>
                        </svg>
                        Chuyển tới nghệ sĩ
                    </button>
                </li>
                <li class="context-menu-item">
                    <button>
                        <svg data-encore-id="icon" role="img" aria-hidden="true" class="e-9890-icon e-9890-baseline" viewBox="0 0 16 16">
                            <path d="M8 1.5a6.5 6.5 0 1 0 0 13 6.5 6.5 0 0 0 0-13zM0 8a8 8 0 1 1 16 0A8 8 0 0 1 0 8z"></path>
                            <path d="M8 6.5a1.5 1.5 0 1 0 0 3 1.5 1.5 0 0 0 0-3zM5 8a3 3 0 1 1 6 0 3 3 0 0 1-6 0z"></path>
                        </svg>
                        Chuyển đến Album
                    </button>
                </li>
                <li class="context-menu-item">
                    <button>
                        <svg data-encore-id="icon" role="img" aria-hidden="true" class="e-9890-icon e-9890-baseline" viewBox="0 0 16 16">
                            <path d="M16 8.328V1h-1.5v4.828h-1a2.5 2.5 0 1 0 2.5 2.5zm-2.5-1h1v1a1 1 0 1 1-1-1zm-4.5 3V4H7.5v3.828h-1a2.5 2.5 0 1 0 2.5 2.5zm-2.5-1h1v1a1 1 0 1 1-1-1zM0 14.5h16V16H0v-1.5zM2 10H0v1.5h2V10zM0 5.5h4V7H0V5.5zM12 1H0v1.5h12V1z"></path>
                        </svg>
                        Xem thông tin ghi công
                    </button>
                </li>
                <li class="context-menu-divider"></li>
                <li class="context-menu-item">
                    <button onclick="shareTrack(${track.trackId})">
                        <svg data-encore-id="icon" role="img" aria-hidden="true" class="e-9812-icon e-9812-baseline" viewBox="0 0 16 16">
                            <path d="M1 5.75A.75.75 0 0 1 1.75 5H4v1.5H2.5v8h11v-8H12V5h2.25a.75.75 0 0 1 .75.75v9.5a.75.75 0 0 1-.75.75H1.75a.75.75 0 0 1-.75-.75v-9.5z"></path>
                            <path d="M8 9.576a.75.75 0 0 0 .75-.75V2.903l1.454 1.454a.75.75 0 0 0 1.06-1.06L8 .03 4.735 3.296a.75.75 0 0 0 1.06 1.061L7.25 2.903v5.923c0 .414.336.75.75.75z"></path>
                        </svg>
                        Chia sẻ
                    </button>
                </li>
            </ul>
        `;
        document.body.appendChild(moreMenu);
    }

    // Menu danh sách phát
    let playlistMenu = document.getElementById('playlistMenu');
    if (!playlistMenu) {
        playlistMenu = document.createElement('div');
        playlistMenu.id = 'playlistMenu';
        playlistMenu.className = 'context-menu playlist-menu';
        playlistMenu.style.display = 'none';
        playlistMenu.innerHTML = `
            <div class="search-filter-section">
                <div class="filter-section">
                    <div class="sidebar-search-bar">
                        <input type="text" class="sidebar-search-input" placeholder="Tìm kiếm danh sách phát" maxlength="80" autocorrect="off" autocapitalize="off" spellcheck="false">
                    </div>
                </div>
            </div>
            <div class="playlist-grid" role="grid" aria-label="Danh sách phát" aria-rowcount="0" aria-colcount="1">
                <p class="no-playlist-message">Chưa có danh sách phát nào.</p>
            </div>
        `;
        document.body.appendChild(playlistMenu);
    }

    // Xử lý nút More
    const moreButton = document.querySelector('.more-button');
    if (moreButton) {
        moreButton.addEventListener('click', function (event) {
            showMoreMenu(event, track.trackId);
        });
    }

    // Ẩn menu khi click ra ngoài
    document.addEventListener('click', function (event) {
        if (!moreMenu.contains(event.target) && !event.target.closest('.more-button')) {
            moreMenu.style.display = 'none';
            hidePlaylistMenu();
        }
    });

    // Ẩn menu khi cuộn
    window.addEventListener('scroll', function () {
        moreMenu.style.display = 'none';
        hidePlaylistMenu();
    });

    // Xử lý hover cho "Thêm vào danh sách phát"
    const addToPlaylistItem = document.querySelector('.add-to-playlist-item');
    if (addToPlaylistItem) {
        addToPlaylistItem.addEventListener('mouseenter', showPlaylistMenu);
        addToPlaylistItem.addEventListener('mouseleave', function () {
            setTimeout(() => {
                if (!playlistMenu.matches(':hover') && !addToPlaylistItem.matches(':hover')) {
                    hidePlaylistMenu();
                }
            }, 100);
        });
    }

    // Giữ menu phụ hiển thị
    if (playlistMenu) {
        playlistMenu.addEventListener('mouseenter', function () {
            playlistMenu.style.display = 'block';
        });
        playlistMenu.addEventListener('mouseleave', function () {
            if (!addToPlaylistItem.matches(':hover')) {
                hidePlaylistMenu();
            }
        });
    }

    // Xử lý tìm kiếm playlist
    const playlistSearchInput = playlistMenu.querySelector('.sidebar-search-input');
    if (playlistSearchInput) {
        playlistSearchInput.addEventListener('input', function () {
            fetchPlaylists(this.value);
        });
        playlistSearchInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                fetchPlaylists(this.value);
            }
        });
    }
}

// Hiển thị menu chính
function showMoreMenu(event, trackId) {
    event.preventDefault();
    event.stopPropagation();
    const moreMenu = document.getElementById('moreMenu');
    if (!moreMenu) return;

    const menuWidth = moreMenu.offsetWidth || 220;
    const menuHeight = moreMenu.offsetHeight || 200;
    const windowWidth = window.innerWidth;
    const windowHeight = window.innerHeight;

    let posX = event.clientX;
    let posY = event.clientY + 5;

    if (posX + menuWidth > windowWidth) posX = windowWidth - menuWidth - 10;
    if (posY + menuHeight > windowHeight) posY = event.clientY - menuHeight - 5;

    posX = Math.max(10, posX);
    posY = Math.max(10, posY);

    moreMenu.style.display = 'block';
    moreMenu.style.left = posX + 'px';
    moreMenu.style.top = posY + 'px';
}

// Hiển thị menu playlist
function showPlaylistMenu() {
    const playlistMenu = document.getElementById('playlistMenu');
    if (!playlistMenu) return;

    const parentMenu = document.getElementById('moreMenu');
    if (!parentMenu) return;

    const parentRect = parentMenu.getBoundingClientRect();
    const menuWidth = playlistMenu.offsetWidth || 220;
    const menuHeight = playlistMenu.offsetHeight || 300;
    const windowWidth = window.innerWidth;
    const windowHeight = window.innerHeight;

    // Đặt menu danh sách phát bên phải của moreMenu
    let posX = parentRect.right + 5;
    let posY = parentRect.top;

    // Nếu menu vượt ra ngoài màn hình bên phải, đặt nó bên trái của moreMenu
    if (posX + menuWidth > windowWidth) {
        posX = parentRect.left - menuWidth - 5;
    }

    // Nếu menu vượt ra ngoài màn hình bên dưới, điều chỉnh vị trí Y
    if (posY + menuHeight > windowHeight) {
        posY = windowHeight - menuHeight - 10;
    }

    // Đảm bảo menu không bị tràn ra ngoài bên trái hoặc trên cùng
    posX = Math.max(10, posX);
    posY = Math.max(10, posY);

    playlistMenu.style.display = 'block';
    playlistMenu.style.left = posX + 'px';
    playlistMenu.style.top = posY + 'px';
    console.log('Hiển thị playlist menu tại:', { posX, posY });
}

// Ẩn menu playlist
function hidePlaylistMenu() {
    const playlistMenu = document.getElementById('playlistMenu');
    if (playlistMenu) {
        playlistMenu.style.display = 'none';
        console.log('Ẩn playlist menu');
    }
}

// Tìm kiếm danh sách phát (giả định)
function fetchPlaylists(query) {
    const playlistGrid = document.querySelector('.playlist-grid');
    if (!playlistGrid) return;

    // Giả định API endpoint để lấy danh sách phát
    const url = `/api/playlists?search=${encodeURIComponent(query)}`;
    fetch(url)
        .then(response => response.json())
        .then(data => {
            if (data.playlists && data.playlists.length > 0) {
                playlistGrid.innerHTML = data.playlists.map(playlist => `
                    <div class="playlist-item" role="gridcell">
                        <button onclick="addTrackToPlaylist(${playlist.id}, ${window.trackData.trackId})">
                            ${playlist.name}
                        </button>
                    </div>
                `).join('');
                playlistGrid.setAttribute('aria-rowcount', data.playlists.length);
            } else {
                playlistGrid.innerHTML = '<p class="no-playlist-message">Không tìm thấy danh sách phát.</p>';
                playlistGrid.setAttribute('aria-rowcount', '0');
            }
        })
        .catch(error => {
            console.error('Lỗi khi lấy danh sách phát:', error);
            playlistGrid.innerHTML = '<p class="no-playlist-message">Lỗi khi tải danh sách phát.</p>';
        });
}

// Hàm giả định để thêm bài hát vào danh sách phát
function addTrackToPlaylist(playlistId, trackId) {
    fetch(`/api/playlists/${playlistId}/tracks`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
        },
        body: JSON.stringify({ trackId })
    })
        .then(response => {
            alert(response.ok
                ? 'Bài hát đã được thêm vào danh sách phát!'
                : 'Có lỗi xảy ra khi thêm bài hát.');
        })
        .catch(error => {
            console.error('Lỗi:', error);
            alert('Có lỗi xảy ra: ' + error.message);
        });
}

// Hàm giả định để thêm vào mục yêu thích
function addToFavorites(trackId) {
    fetch('/api/favorites', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
        },
        body: JSON.stringify({ trackId })
    })
        .then(response => {
            alert(response.ok
                ? 'Đã thêm vào mục yêu thích!'
                : 'Có lỗi xảy ra khi thêm vào mục yêu thích.');
        })
        .catch(error => {
            console.error('Lỗi:', error);
            alert('Có lỗi xảy ra: ' + error.message);
        });
}

// Hàm giả định để thêm vào danh sách chờ
function addToQueue(trackId) {
    fetch('/api/queue', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
        },
        body: JSON.stringify({ trackId })
    })
        .then(response => {
            alert(response.ok
                ? 'Đã thêm vào danh sách chờ!'
                : 'Có lỗi xảy ra khi thêm vào danh sách chờ.');
        })
        .catch(error => {
            console.error('Lỗi:', error);
            alert('Có lỗi xảy ra: ' + error.message);
        });
}

// Hàm giả định để chia sẻ bài hát
function shareTrack(trackId) {
    const shareUrl = `${window.location.origin}/track/${trackId}`;
    navigator.clipboard.writeText(shareUrl)
        .then(() => {
            alert('Đã sao chép liên kết bài hát!');
        })
        .catch(error => {
            console.error('Lỗi khi sao chép liên kết:', error);
            alert('Có lỗi xảy ra khi chia sẻ bài hát.');
        });
}