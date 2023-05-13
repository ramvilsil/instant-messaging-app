document.addEventListener('DOMContentLoaded', () => {

    let activityTimer;
    const timerDurationMilliseconds = 60000;

    let userIsActive;

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
        console.log('WebSocket connection established.');
        webSocket.send(JSON.stringify(
            {
                MessageType: "UserActiveStatus",
                UserIsActive: true
            }
        ));
        displayConnectionMessage();
        handleActivityTimer();
    });

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

    document.addEventListener('contextmenu', () => {
        // event.preventDefault();
        handleActivityTimer();
    });

    document.addEventListener('scroll', () => {
        handleActivityTimer();
    });

    webSocket.addEventListener('message', (event) => {

        console.log(`WebSocket message received: ${event.data}`);

        const message = JSON.parse(event.data);

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


    const html = document.documentElement;
    const body = document.body;

    const navbar = document.getElementsByClassName('navbar')[0];

    const mainNavbar = document.getElementsByClassName('main-navbar')[0];

    const displayConnectionMessage = () => {

        const oldConnectedMessage = document.getElementById('connected-message');
        const oldDisconnectedMessage = document.getElementById('disconnected-message');

        if (oldConnectedMessage) oldConnectedMessage.remove();

        if (oldDisconnectedMessage) oldDisconnectedMessage.remove();

        const connectedMessage = document.createElement('div');
        connectedMessage.id = 'connected-message';
        connectedMessage.style.position = 'sticky';
        connectedMessage.style.width = '100%';
        connectedMessage.style.padding = '1rem';
        connectedMessage.style.backgroundColor = 'blue';
        connectedMessage.style.color = 'white';
        connectedMessage.style.textAlign = 'center';
        connectedMessage.style.fontWeight = 'bold';
        connectedMessage.style.fontSize = '1.5rem';
        connectedMessage.innerText = 'Connected.';

        const disconnectedMessage = document.createElement('div');
        disconnectedMessage.id = 'disconnected-message';
        disconnectedMessage.style.position = 'sticky';
        disconnectedMessage.style.width = '100%';
        disconnectedMessage.style.padding = '1rem';
        disconnectedMessage.style.backgroundColor = 'red';
        disconnectedMessage.style.color = 'white';
        disconnectedMessage.style.textAlign = 'center';
        disconnectedMessage.style.fontWeight = 'bold';
        disconnectedMessage.style.fontSize = '1.5rem';
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

});