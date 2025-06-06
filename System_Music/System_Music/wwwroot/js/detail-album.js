document.addEventListener('DOMContentLoaded', function () {
    // Ensure window.playerState uses audio from _NowPlayingBar.cshtml
    window.playerState = window.playerState || {
        audio: document.getElementById('audio-player'),
        isPlaying: false,
        currentTrack: null,
        queue: [],
        currentIndex: 0,
        repeatMode: 'off'
    };

    // Apply gradient for .album-header based on cover image
    const albumImage = document.querySelector('.album-image');
    if (albumImage) {
        applyImageBasedGradient(albumImage.src);
    } else {
        console.warn('Không tìm thấy ảnh bìa album, sử dụng gradient mặc định');
        applyFallbackGradient();
    }

    // Play a track and update NowPlayingBar
    function playTrack(track, index) {
        if (!track || !track.audioUrl) {
            console.error('Bài hát không hợp lệ:', track);
            alert('Vui lòng chọn một bài hát hợp lệ.');
            return;
        }

        console.log('Đang phát bài:', track.title);
        window.playerState.currentTrack = track;
        window.playerState.currentIndex = index;

        // Update NowPlayingBar UI
        if (!window.updateNowPlayingBar(track)) {
            console.error('Không thể cập nhật NowPlayingBar');
            return;
        }

        // Set audio source and play
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

    // Handle volume slider
    const volumeSlider = document.querySelector('#volume-slider');
    const volumeProgressBar = document.querySelector('.volume-progress-bar');
    const progressFill = document.querySelector('.progress-bar-fill');
    const progressThumb = document.querySelector('.progress-bar-thumb');
    const volumeIcon = document.querySelector('#volume-icon');

    if (volumeSlider && volumeProgressBar && progressFill && progressThumb && volumeIcon) {
        const initialVolume = parseFloat(volumeSlider.value) || 0.5;
        window.playerState.audio.volume = initialVolume;
        volumeSlider.value = initialVolume;
        updateVolumeIcon(initialVolume);
        updateProgressBar(initialVolume);
        console.log('Khởi tạo âm lượng:', initialVolume);

        function updateProgressBar(volume) {
            const percentage = volume * 100;
            volumeProgressBar.style.setProperty('--progress-bar-transform', `${percentage}%`);
            progressFill.style.width = `${percentage}%`;
            progressThumb.style.left = `calc(${percentage}% - 6px)`;
        }

        let isDragging = false;

        volumeProgressBar.addEventListener('mousedown', function (e) {
            isDragging = true;
            updateVolumeFromPosition(e);
        });

        document.addEventListener('mousemove', function (e) {
            if (isDragging) {
                updateVolumeFromPosition(e);
            }
        });

        document.addEventListener('mouseup', function () {
            isDragging = false;
        });

        function updateVolumeFromPosition(e) {
            const rect = volumeProgressBar.getBoundingClientRect();
            const posX = e.clientX - rect.left;
            let newVolume = posX / rect.width;
            newVolume = Math.max(0, Math.min(1, newVolume));

            window.playerState.audio.volume = newVolume;
            volumeSlider.value = newVolume;
            updateProgressBar(newVolume);
            updateVolumeIcon(newVolume);
            console.log('Cập nhật âm lượng:', newVolume);
        }

        volumeSlider.addEventListener('input', function () {
            const newVolume = parseFloat(this.value);
            if (newVolume >= 0 && newVolume <= 1) {
                window.playerState.audio.volume = newVolume;
                updateProgressBar(newVolume);
                updateVolumeIcon(newVolume);
                console.log('Cập nhật âm lượng:', newVolume);
            } else {
                console.warn('Giá trị âm lượng không hợp lệ:', newVolume);
                window.playerState.audio.volume = 0.5;
                volumeSlider.value = 0.5;
                updateProgressBar(0.5);
                updateVolumeIcon(0.5);
            }
        });

        const muteButton = document.querySelector('.volume-mute-btn');
        let lastVolume = window.playerState.audio.volume;
        if (muteButton) {
            muteButton.addEventListener('click', function () {
                if (window.playerState.audio.volume > 0) {
                    lastVolume = window.playerState.audio.volume;
                    window.playerState.audio.volume = 0;
                    volumeSlider.value = 0;
                    updateProgressBar(0);
                    updateVolumeIcon(0);
                    this.setAttribute('aria-label', 'Bật tiếng');
                    console.log('Tắt tiếng, âm lượng:', window.playerState.audio.volume);
                } else {
                    window.playerState.audio.volume = lastVolume || 0.5;
                    volumeSlider.value = window.playerState.audio.volume;
                    updateProgressBar(window.playerState.audio.volume);
                    updateVolumeIcon(window.playerState.audio.volume);
                    this.setAttribute('aria-label', 'Tắt tiếng');
                    console.log('Bật tiếng, âm lượng:', window.playerState.audio.volume);
                }
            });
        }
    } else {
        console.warn('Không tìm thấy các phần tử điều khiển âm lượng');
    }

    function updateVolumeIcon(volume) {
        const paths = {
            muted: `
                <path d="M13.86 5.47a.75.75 0 0 0-1.061 0l-1.47 1.47-1.47-1.47A.75.75 0 0 0 8.8 6.53L10.269 8l-1.47 1.47a.75.75 0 1 0 1.06 1.06l1.47-1.47 1.47 1.47a.75.75 0 0 0 1.06-1.06L12.39 8l1.47-1.47a.75.75 0 0 0 0-1.06z"></path>
                <path d="M10.116 1.5A.75.75 0 0 0 8.991.85l-6.925 4a3.642 3.642 0 0 0-1.33 4.967 3.639 3.639 0 0 0 1.33 1.332l6.925 4a.75.75 0 0 0 1.125-.649v-1.906a4.73 4.73 0 0 1-1.5-.694v1.3L2.817 9.852a2.141 2.141 0 0 1-.781-2.92c.187-.324.456-.594.78-.782l5.8-3.35v1.3c.45-.313.956-.55 1.5-.694V1.5z"></path>
            `,
            low: `
                <path d="M9.741.85a.75.75 0 0 1 .375.65v13a.75.75 0 0 1-1.125.65l-6.925-4a3.642 3.642 0 0 1-1.33-4.967 3.639 3.639 0 0 1 1.33-1.332l6.925-4a.75.75 0 0 1 .75 0zm-6.924 5.3a2.139 2.139 0 0 0 0 3.7l5.8 3.35V2.8l-5.8 3.35zm8.683 4.29V5.56a2.75 2.75 0 0 1 0 4.88z"></path>
            `,
            medium: `
                <path d="M9.741.85a.75.75 0 0 1 .375.65v13a.75.75 0 0 1-1.125.65l-6.925-4a3.642 3.642 0 0 1-1.33-4.967 3.639 3.639 0 0 1 1.33-1.332l6.925-4a.75.75 0 0 1 .75 0zm-6.924 5.3a2.139 2.139 0 0 0 0 3.7l5.8 3.35V2.8l-5.8 3.35zm8.683 6.087a4.502 4.502 0 0 0 0-8.474v1.65a2.999 2.999 0 0 1 0 5.175v1.649z"></path>
            `,
            high: `
                <path d="M9.741.85a.75.75 0 0 1 .375.65v13a.75.75 0 0 1-1.125.65l-6.925-4a3.642 3.642 0 0 1-1.33-4.967 3.639 3.639 0 0 1 1.33-1.332l6.925-4a.75.75 0 0 1 .75 0zm-6.924 5.3a2.139 2.139 0 0 0 0 3.7l5.8 3.35V2.8l-5.8 3.35zm8.683 4.29V5.56a2.75 2.75 0 0 1 0 4.88z"></path>
                <path d="M11.5 13.614a5.752 5.752 0 0 0 0-11.228v1.55a4.252 4.252 0 0 1 0 8.127v1.55z"></path>
            `
        };

        if (volume === 0) {
            volumeIcon.innerHTML = paths.muted;
            volumeIcon.setAttribute('aria-label', 'Tắt tiếng');
        } else if (volume > 0 && volume <= 0.33) {
            volumeIcon.innerHTML = paths.low;
            volumeIcon.setAttribute('aria-label', 'Âm lượng thấp');
        } else if (volume > 0.33 && volume <= 0.66) {
            volumeIcon.innerHTML = paths.medium;
            volumeIcon.setAttribute('aria-label', 'Âm lượng trung bình');
        } else {
            volumeIcon.innerHTML = paths.high;
            volumeIcon.setAttribute('aria-label', 'Âm lượng cao');
        }
    }

    // Handle playAlbumBtn
    const playAlbumBtn = document.getElementById('play-album-btn');
    if (playAlbumBtn) {
        const albumId = playAlbumBtn.getAttribute('data-album-id');
        fetch(`/api/public/tracks/by-album/${albumId}`)
            .then(response => response.json())
            .then(data => {
                window.playerState.queue = data;
                playAlbumBtn.addEventListener('click', function () {
                    if (window.playerState.queue.length > 0) {
                        playTrack(window.playerState.queue[0], 0);
                    } else {
                        alert('Không có bài hát nào trong album.');
                    }
                });
            })
            .catch(error => {
                console.error('Lỗi lấy danh sách bài hát:', error);
                alert('Không thể tải danh sách bài hát.');
            });
    }

    // Handle trackLinks
    const trackLinks = document.querySelectorAll('.play-btn');
    trackLinks.forEach((trackLink, index) => {
        trackLink.addEventListener('click', function (e) {
            e.preventDefault();
            if (window.playerState.queue[index]) {
                playTrack(window.playerState.queue[index], index);
            } else {
                console.warn('Không tìm thấy bài hát tại index:', index);
                alert('Không thể phát bài hát.');
            }
        });
    });

    // Handle nextBtn
    const nextBtn = document.querySelector('.next-btn');
    if (nextBtn) {
        nextBtn.addEventListener('click', function () {
            if (!window.playerState.currentTrack) {
                alert('Vui lòng chọn một bài hát trước.');
                return;
            }
            let nextIndex = window.playerState.currentIndex;
            if (window.playerState.repeatMode === 'repeat-one') {
                // Repeat current track
                playTrack(window.playerState.queue[nextIndex], nextIndex);
            } else if (window.playerState.repeatMode === 'repeat-all' && nextIndex >= window.playerState.queue.length - 1) {
                // Loop back to first track
                nextIndex = 0;
                playTrack(window.playerState.queue[nextIndex], nextIndex);
            } else if (nextIndex < window.playerState.queue.length - 1) {
                // Play next track
                nextIndex++;
                playTrack(window.playerState.queue[nextIndex], nextIndex);
            }
        });
    }

    // Handle previousBtn
    const previousBtn = document.querySelector('.previous-btn');
    if (previousBtn) {
        previousBtn.addEventListener('click', function () {
            if (!window.playerState.currentTrack) {
                alert('Vui lòng chọn một bài hát trước.');
                return;
            }
            let prevIndex = window.playerState.currentIndex;
            if (window.playerState.repeatMode === 'repeat-one') {
                // Repeat current track
                playTrack(window.playerState.queue[prevIndex], prevIndex);
            } else if (window.playerState.repeatMode === 'repeat-all' && prevIndex <= 0) {
                // Loop back to last track
                prevIndex = window.playerState.queue.length - 1;
                playTrack(window.playerState.queue[prevIndex], prevIndex);
            } else if (prevIndex > 0) {
                // Play previous track
                prevIndex--;
                playTrack(window.playerState.queue[prevIndex], prevIndex);
            }
        });
    }
    // Apply gradient based on image
    function applyImageBasedGradient(imageUrl) {
        if (typeof Vibrant === 'undefined' || !window.vibrantLoaded) {
            console.warn('Vibrant.js không tải, sử dụng gradient ngẫu nhiên');
            applyRandomGradient();
            return;
        }

        if (!imageUrl) {
            console.warn('Không có ảnh bìa hợp lệ, sử dụng gradient ngẫu nhiên');
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
                styleElement.textContent = `.album-header { background: ${gradient} !important; }`;
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

    // Random gradient
    function applyRandomGradient() {
        const letters = '0123456789ABCDEF';
        let color = '#';
        for (let i = 0; i < 6; i++) {
            color += letters[Math.floor(Math.random() * 16)];
        }
        const gradient = `linear-gradient(to bottom, ${color} 0%, ${color} 10%, #181818 20%)`;

        let styleElement = document.getElementById('dynamic-gradient');
        if (!styleElement) {
            styleElement = document.createElement('style');
            styleElement.id = 'dynamic-gradient';
            document.head.appendChild(styleElement);
        }
        styleElement.textContent = `.album-header { background: ${gradient} !important; }`;
        console.log('Gradient ngẫu nhiên áp dụng:', gradient);
    }

    // Fallback gradient
    function applyFallbackGradient() {
        const gradient = `linear-gradient(to bottom, #5a1a1a 0%, #181818 40%)`;
        let styleElement = document.getElementById('dynamic-gradient');
        if (!styleElement) {
            styleElement = document.createElement('style');
            styleElement.id = 'dynamic-gradient';
            document.head.appendChild(styleElement);
        }
        styleElement.textContent = `.album-header { background: ${gradient} !important; }`;
        console.log('Áp dụng gradient dự phòng:', gradient);
    }

    // Adjust color
    function adjustColor(hex, brightnessFactor, saturationFactor) {
        const rgb = hexToRgb(hex);
        const hsl = rgbToHsl(rgb.r, rgb.g, rgb.b);
        hsl.l = Math.max(0, Math.min(1, hsl.l * brightnessFactor));
        hsl.s = Math.max(0, Math.min(1, hsl.s * saturationFactor));
        const adjustedRgb = hslToRgb(hsl.h, hsl.s, hsl.l);
        return rgbToHex(adjustedRgb.r, adjustedRgb.g, adjustedRgb.b);
    }

    // Color conversion functions
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

    // Create custom menu
    function createCustomMenu(trackId) {
        const menu = document.createElement('div');
        menu.classList.add('custom-menu');
        menu.setAttribute('data-track-id', trackId);
        menu.innerHTML = `
            <ul class="menu-list">
                <li class="menu-item">
                    <button class="menu-button" data-action="add-to-playlist">
                        <svg data-encore-id="icon" role="img" aria-hidden="true" class="menu-icon" viewBox="0 0 16 16">
                            <path d="M15.25 8a.75.75 0 0 1-.75.75H8.75v5.75a.75.75 0 0 1-1.5 0V8.75H1.5a.75.75 0 0 1 0-1.5h5.75V1.5a.75.75 0 0 1 1.5 0v5.75h5.75a.75.75 0 0 1 .75.75z"></path>
                        </svg>
                        <span>Thêm vào danh sách phát</span>
                    </button>
                </li>
                <li class="menu-item">
                    <button class="menu-button" data-action="save-to-liked">
                        <svg data-encore-id="icon" role="img" aria-hidden="true" class="menu-icon" viewBox="0 0 16 16">
                            <path d="M8 1.5a6.5 6.5 0 1 0 0 13 6.5 6.5 0 0 0 0-13zM0 8a8 8 0 1 1 16 0A8 8 0 0 1 0 8z"></path>
                            <path d="M11.75 8a.75.75 0 0 1-.75.75H8.75V11a.75.75 0 0 1-1.5 0V8.75H5a.75.75 0 0 1 0-1.5h2.25V5a.75.75 0 0 1 1.5 0v2.25H11a.75.75 0 0 1 .75.75z"></path>
                        </svg>
                        <span>Lưu vào Bài hát đã thích của bạn</span>
                    </button>
                </li>
                <li class="menu-item">
                    <button class="menu-button" data-action="add-to-queue">
                        <svg data-encore-id="icon" role="img" aria-hidden="true" class="menu-icon" viewBox="0 0 16 16">
                            <path d="M16 15H2v-1.5h14V15zm0-4.5H2V9h14v1.5zm-8.034-6A5.484 5.484 0 0 1 7.187 6H13.5a2.5 2.5 0 0 0 0-5H7.966c.159.474.255.978.278 1.5H13.5a1 1 0 1 1 0 2H7.966zM2 2V0h1.5v2h2v1.5h-2v2H2v-2H0V2h2z"></path>
                        </svg>
                        <span>Thêm vào danh sách chờ</span>
                    </button>
                </li>
                <li class="menu-item">
                    <button class="menu-button" data-action="radio-track">
                        <svg data-encore-id="icon" role="img" aria-hidden="true" class="menu-icon" viewBox="0 0 16 16">
                            <path d="M5.624 3.886A4.748 4.748 0 0 0 3.25 8c0 1.758.955 3.293 2.375 4.114l.75-1.3a3.249 3.249 0 0 1 0-5.63l-.75-1.298zm4.001 1.299.75-1.3A4.748 4.748 0 0 1 12.75 8a4.748 4.748 0 0 1-2.375 4.114l-.75-1.3a3.249 3.249 0 0 0 0-5.63zM8 6.545a1.455 1.455 0 0 0 0 2.91 1.455 1.455 0 0 0 0-2.91z"></path>
                            <path d="M4 1.07A7.997 7.997 0 0 0 0 8a7.997 7.997 0 0 0 4 6.93l.75-1.3A6.497 6.497 0 0 1 1.5 8a6.497 6.497 0 0 1 3.25-5.63L4 1.07zm7.25 1.3.75-1.3A7.997 7.997 0 0 1 16 8a7.997 7.997 0 0 1-3.999 6.93l-.75-1.3A6.497 6.497 0 0 0 14.5 8a6.497 6.497 0 0 0-3.25-5.63z"></path>
                        </svg>
                        <span>Chuyển đến radio theo bài hát</span>
                    </button>
                </li>
                <li class="menu-item">
                    <a href="/artist/1WvNgEoB66jmHodcj15Zi9" class="menu-link" data-action="go-to-artist">
                        <svg data-encore-id="icon" role="img" aria-hidden="true" class="menu-icon" viewBox="0 0 16 16">
                            <path d="M11.757 2.987A4.356 4.356 0 0 0 7.618 0a4.362 4.362 0 0 0-4.139 2.987 5.474 5.474 0 0 0-.22 1.894 5.604 5.604 0 0 0 1.4 3.312l.125.152a.748.748 0 0 1-.2 1.128l-2.209 1.275A4.748 4.748 0 0 0 0 14.857v1.142h8.734A5.48 5.48 0 0 1 8.15 14.5H1.517a3.245 3.245 0 0 1 1.6-2.454l2.21-1.275a2.25 2.25 0 0 0 .6-3.386l-.128-.153a4.112 4.112 0 0 1-1.05-2.44A4.053 4.053 0 0 1 4.89 3.47a2.797 2.797 0 0 1 1.555-1.713 2.89 2.89 0 0 1 3.293.691c.265.296.466.644.589 1.022.12.43.169.876.144 1.322a4.12 4.12 0 0 1-1.052 2.44l-.127.153a2.239 2.239 0 0 0-.2 2.58c.338-.45.742-.845 1.2-1.173 0-.162.055-.32.156-.447l.126-.152a5.598 5.598 0 0 0 1.4-3.312 5.499 5.499 0 0 0-.218-1.894zm3.493 3.771a.75.75 0 0 0-.75.75v3.496h-1a2.502 2.502 0 0 0-2.31 1.542 2.497 2.497 0 0 0 1.822 3.406A2.502 2.502 0 0 0 16 13.502V7.508a.75.75 0 0 0-.75-.75zm-.75 6.744a.998.998 0 0 1-1.707.707 1 1 0 0 1 .707-1.706h1v1z"></path>
                        </svg>
                        <span>Chuyển tới nghệ sĩ</span>
                    </a>
                </li>
                <li class="menu-item">
                    <a href="/album/@Model.AlbumId?highlight=spotify:track:${trackId}" class="menu-link" data-action="go-to-album">
                        <svg data-encore-id="icon" role="img" aria-hidden="true" class="menu-icon" viewBox="0 0 16 16">
                            <path d="M8 1.5a6.5 6.5 0 1 0 0 13 6.5 6.5 0 0 0 0-13zM0 8a8 8 0 1 1 16 0A8 8 0 0 1 0 8z"></path>
                            <path d="M8 6.5a1.5 1.5 0 1 0 0 3 1.5 1.5 0 0 0 0-3zM5 8a3 3 0 1 1 6 0 3 3 0 0 1-6 0z"></path>
                        </svg>
                        <span>Chuyển đến album</span>
                    </a>
                </li>
                <li class="menu-item">
                    <button class="menu-button" data-action="view-credits">
                        <svg data-encore-id="icon" role="img" aria-hidden="true" class="menu-icon" viewBox="0 0 16 16">
                            <path d="M16 8.328V1h-1.5v4.828h-1a2.5 2.5 0 1 0 2.5 2.5zm-2.5-1h1v1a1 1 0 1 1-1-1zm-4.5 3V4H7.5v3.828h-1a2.5 2.5 0 1 0 2.5 2.5zm-2.5-1h1v1a1 1 0 1 1-1-1zM0 14.5h16V16H0v-1.5zM2 10H0v1.5h2V10zM0 5.5h4V7H0V5.5zM12 1H0v1.5h12V1z"></path>
                        </svg>
                        <span>Xem thông tin ghi công</span>
                    </button>
                </li>
                <li class="menu-item">
                    <button class="menu-button" data-action="share">
                        <svg data-encore-id="icon" role="img" aria-hidden="true" class="menu-icon" viewBox="0 0 16 16">
                            <path d="M1 5.75A.75.75 0 0 1 1.75 5H4v1.5H2.5v8h11v-8H12V5h2.25a.75.75 0 0 1 .75.75v9.5a.75.75 0 0 1-.75.75H1.75a.75.75 0 0 1-.75-.75v-9.5z"></path>
                            <path d="M8 9.576a.75.75 0 0 0 .75-.75V2.903l1.454 1.454a.75.75 0 0 0 1.06-1.06L8 .03 4.735 3.296a.75.75 0 0 0 1.06 1.061L7.25 2.903v5.923c0 .414.336.75.75.75z"></path>
                        </svg>
                        <span>Chia sẻ</span>
                    </button>
                </li>
                <li class="menu-item">
                    <button class="menu-button" data-action="open-app">
                        <svg data-encore-id="icon" role="img" aria-hidden="true" class="menu-icon" viewBox="0 0 16 16">
                            <path d="M8.319 0.006A8.003 8.003 0 0 0 0.006 7.683a7.998 7.998 0 0 0 7.677 8.31A8 8 0 0 0 8.319 0.006Zm3.377 11.72a.478.478 0 0 1-0.652 0.179 9.612 9.612 0 0 0-3.426-1.165 9.599 9.599 0 0 0-3.613 0.176.479.479 0 0 1-0.226-0.93c1.3-0.316 2.637-0.38 3.972-0.193 1.336 0.188 2.602 0.62 3.765 1.28 0.228 0.13 0.309 0.422 0.178 0.652l0.002 0.001Zm1.05-2.1a.62.62 0 0 1-0.841 0.25A11.793 11.793 0 0 0 7.923 8.57a11.775 11.775 0 0 0-4.188 0.158.622.622 0 0 1-0.74-0.473.62.62 0 0 1 0.473-0.739 13.032 13.032 0 0 1 4.626-0.176c1.552 0.217 3.031 0.704 4.4 1.444a.62.62 0 0 1 0.25 0.842h0.003Zm1.166-2.367a.765.765 0 0 1-1.031 0.326 14.307 14.307 0 0 0-4.612-1.473a14.285 14.285 0 0 0-4.84 0.145.764.764 0 1 1-0.303-1.499 15.812 15.812 0 0 1 5.356-0.16c1.791 0.252 3.51 0.8 5.104 1.63 0.374 0.194 0.520 0.656 0.326 1.03Z"></path>
                        </svg>
                        <span>Mở trong ứng dụng dành cho máy tính</span>
                    </button>
                </li>
            </ul>
        `;
        return menu;
    }

    // Handle context menu display
    const moreButtons = document.querySelectorAll('.track-action-button[aria-label="More options"]');
    moreButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            const allMenus = document.querySelectorAll('.custom-menu');
            allMenus.forEach(menu => {
                menu.classList.add('hidden');
                if (menu.parentNode) {
                    menu.parentNode.removeChild(menu);
                }
            });

            const songRow = button.closest('.song-row');
            if (!songRow) {
                console.warn('Không tìm thấy song-row');
                return;
            }

            const trackId = songRow.getAttribute('data-track-id');
            if (!trackId) {
                console.warn('Không tìm thấy track-id');
                return;
            }

            const menu = createCustomMenu(trackId);
            document.body.appendChild(menu);
            menu.classList.remove('hidden');

            const buttonRect = button.getBoundingClientRect();
            const songRowRect = songRow.getBoundingClientRect();
            const menuRect = menu.getBoundingClientRect();
            const mainView = document.querySelector('.main-view');
            const mainViewRect = mainView ? mainView.getBoundingClientRect() : { left: 0, top: 0, width: window.innerWidth, height: window.innerHeight };

            let menuLeft = buttonRect.right + window.scrollX + 2;
            let menuTop = buttonRect.bottom + window.scrollY + 2;

            const maxLeft = mainViewRect.left + mainViewRect.width - menuRect.width - 4;
            menuLeft = Math.max(songRowRect.left + window.scrollX, Math.min(menuLeft, maxLeft));
            if (menuLeft < mainViewRect.left + 4) {
                menuLeft = mainViewRect.left + 4;
            }

            const maxTop = mainViewRect.top + mainViewRect.height - menuRect.height - 4;
            if (menuTop + menuRect.height > maxTop) {
                menuTop = buttonRect.top + window.scrollY - menuRect.height - 2;
            }
            if (menuTop < mainViewRect.top) {
                menuTop = mainViewRect.top + 4;
            }

            menu.style.position = 'absolute';
            menu.style.top = `${menuTop}px`;
            menu.style.left = `${1180}px`;
            menu.style.zIndex = '1000';

            console.log('Menu hiển thị cho track ID:', trackId);
        });
    });

    // Hide menu on click outside
    document.addEventListener('click', function (e) {
        const menus = document.querySelectorAll('.custom-menu');
        const moreButtons = document.querySelectorAll('.track-action-button[aria-label="More options"]');
        let isClickInsideMenuOrButton = false;

        menus.forEach(menu => {
            if (menu.contains(e.target)) {
                isClickInsideMenuOrButton = true;
            }
        });
        moreButtons.forEach(button => {
            if (button.contains(e.target)) {
                isClickInsideMenuOrButton = true;
            }
        });

        if (!isClickInsideMenuOrButton) {
            menus.forEach(menu => {
                menu.classList.add('hidden');
                if (menu.parentNode) {
                    menu.parentNode.removeChild(menu);
                }
            });
        }
    });
});