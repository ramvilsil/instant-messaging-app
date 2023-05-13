document.addEventListener('DOMContentLoaded', () => {

    let userIsActive;
    let activityTimer;
    const activityTimerDurationMs = 60000;

    const webSocketUrl = `ws://${window.location.host}/WebSocket`;
    let webSocket = new WebSocket(webSocketUrl);

    webSocket.addEventListener('open', () => {
        console.log('WebSocket connection established.');
        webSocket.send(JSON.stringify(
            {
                MessageType: "UserActiveStatus",
                UserIsActive: true
            }
        ));
        handleActivityTimer();
    });

    const handleActivityTimer = () => {

        clearTimeout(activityTimer);

        activityTimer = setTimeout(() => {
            webSocket.send(JSON.stringify(
                {
                    MessageType: "UserActiveStatus",
                    UserIsActive: false
                }
            ));
        }, activityTimerDurationMs);

    };

    window.addEventListener('beforeunload', () => {
        webSocket.send(JSON.stringify(
            {
                MessageType: "UserActiveStatus",
                UserIsActive: false
            }
        ));
    });

    document.addEventListener('click', () => {
        handleActivityTimer();
    });

    document.addEventListener('keydown', () => {
        handleActivityTimer();
    });

    document.addEventListener('contextmenu', (event) => {
        event.preventDefault();
        handleActivityTimer();
    });

    document.addEventListener('scroll', () => {
        handleActivityTimer();
    });

    webSocket.addEventListener('message', (event) => {

        const message = JSON.parse(event.data);

        console.log(`Received Message: ${JSON.stringify(message)}`);

        switch (message.MessageType) {
            case 'UserActiveStatus':
                console.log(`User Active Status: ${JSON.stringify(message)}`);
                userIsActive = message.UserIsActive;
                displayConnectionMessage();
                break;

            case 'ActiveUsers':
                console.log(`Active Users: ${JSON.stringify(message)}`);
                displayActiveUsers(message.Users);
                break;
        }

    });

    const navbar = document.getElementsByClassName('navbar')[0];
    const mainNavbar = document.getElementsByClassName('main-navbar')[0];

    const displayConnectionMessage = () => {

        const oldConnectedMessage = document.getElementById('connected-message');
        const oldDisconnectedMessage = document.getElementById('disconnected-message');

        if (oldConnectedMessage) oldConnectedMessage.remove();

        if (oldDisconnectedMessage) oldDisconnectedMessage.remove();

        const connectedMessage = document.createElement('div');
        connectedMessage.id = 'connected-message';
        connectedMessage.innerText = 'Connected.';

        const disconnectedMessage = document.createElement('div');
        disconnectedMessage.id = 'disconnected-message';
        disconnectedMessage.innerText = 'Disconnected due to inactivity.';

        switch (userIsActive) {
            case true:
                navbar.insertBefore(connectedMessage, mainNavbar);
                break;

            case false:
                navbar.insertBefore(disconnectedMessage, mainNavbar);
                break;
        }
    };

    // ---------- Handle active users ----------
    const activeUsersContainerElement = document.getElementById('active-users-container');

    const displayActiveUsers = (activeUsers) => {
        activeUsersContainerElement.innerHTML = '';
        for (const activeUser of activeUsers) {
            const activeUserElement = document.createElement('a');
            activeUserElement.innerText = activeUser.UserName;
            activeUserElement.href = `/Main/UserProfile?userId=${activeUser.UserId}`;
            activeUsersContainerElement.appendChild(activeUserElement);
        };
    };

});