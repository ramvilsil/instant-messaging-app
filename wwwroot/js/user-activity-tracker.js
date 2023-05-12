document.addEventListener('DOMContentLoaded', () => {

    let activityTimer;
    const timerDurationMilliseconds = 60000;

    const handleActivityTimer = () => {
        clearTimeout(activityTimer);
        activityTimer = setTimeout(() => {
            webSocket.send(JSON.stringify(
                {
                    MessageType: "UserActiveStatus",
                    UserIsActive: false
                }
            ));
        }, timerDurationMilliseconds);
    };

    const webSocketUrl = `ws://${window.location.host}/WebSocket`;

    let webSocket = new WebSocket(webSocketUrl);

    webSocket.addEventListener('open', () => {
        console.log('User Activity Tracker WebSocket connection established.');
        webSocket.send(JSON.stringify(
            {
                MessageType: "UserActiveStatus",
                UserIsActive: true
            }
        ));
        handleActivityTimer();
    });

    document.addEventListener('click', () => {
        handleActivityTimer();
    });

    document.addEventListener('keydown', () => {
        handleActivityTimer();
    });

    document.addEventListener('contextmenu', () => {
        // event.preventDefault();
        handleActivityTimer();
    });

    document.addEventListener('scroll', () => {
        handleActivityTimer();
    });

    webSocket.addEventListener('message', (event) => {
        console.log(`User Activity Tracker WebSocket message received: ${event.data}`);

        const message = JSON.parse(event.data);

        switch (message.MessageType) {
            case 'ActiveUsers':
                console.log(`Active users: ${JSON.stringify(message.Data)}`);
                break;

            case 'UserMessage':
                console.log(`User message: ${JSON.stringify(message.Data)}`);
                break;
        }

    });


});