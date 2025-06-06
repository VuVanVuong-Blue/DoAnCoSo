// wwwroot/js/sidebar.js

function toggleChatBubble() {
    const chatBubble = document.getElementById('chatBubble');
    if (chatBubble.classList.contains('show')) {
        chatBubble.classList.remove('show');
    } else {
        chatBubble.classList.add('show');
    }
}

function closeChatBubble() {
    const chatBubble = document.getElementById('chatBubble');
    chatBubble.classList.remove('show');
}