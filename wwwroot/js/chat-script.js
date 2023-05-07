// Script for the chat page

document.addEventListener('DOMContentLoaded', () => {

    const messageInput = document.getElementById('message-input');
    const sendMessageButton = document.getElementById('send-message-button');
    const chatMessages = document.getElementById('chat-messages');

    const webSocketUrl = `ws://${window.location.host}/Messaging/WebsocketConnection`;

    let socket = new WebSocket(webSocketUrl);

    // Establish the WebSocket connection
    socket.addEventListener('open', () => {
        console.log('Connected to the server.');
    });

    // Receiving messages from the server
    socket.addEventListener('message', (event) => {
        let message = `@Model.RecipientUsername - ${event.data}`;
        addMessage(message, 'server');
    });

    const addMessage = (message, sender) => {
        const messageElement = document.createElement('div');
        if (sender === 'client') {
            messageElement.style.backgroundColor = 'lightblue';
            messageElement.style.alignSelf = 'flex-end';
        }
        messageElement.innerText = message;
        chatMessages.appendChild(messageElement);
    };

    // Sending messages to the server
    sendMessageButton.addEventListener('click', () => {
        const message = messageInput.value;
        if (message) {
            if (socket.readyState === WebSocket.CLOSING || socket.readyState === WebSocket.CLOSED) {
                console.log('Reconnecting to the server...');
                socket = new WebSocket(webSocketUrl);
            }
            socket.send(JSON.stringify(
                {
                    RecipientUserId: '@Model.RecipientUserId',
                    MessagePayload: message
                }
            ));
            addMessage(message, 'client');
            messageInput.value = '';
        }
    });

    messageInput.addEventListener('keydown', (event) => {
        if (event.key === 'Enter') {
            event.preventDefault();
            sendMessageButton.click();
        }
    });
});