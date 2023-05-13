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

            case 'AllUsers':
                console.log(`All Users: ${JSON.stringify(message)}`);
                displayAllUsers(message.Users);
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

    // ---------- Handle all users ----------
    const allUsersContainerElement = document.getElementById('all-users-container');

    const displayAllUsers = (allUsers) => {
        allUsersContainerElement.innerHTML = '';
        for (const user of allUsers) {
            const userElement = document.createElement('a');
            userElement.innerText = user.UserName;
            userElement.href = `/Main/UserProfile?userId=${user.UserId}`;

            switch (user.UserActiveStatus) {
                case true:
                    userElement.style.color = 'blue';
                    break;
                case false:
                    userElement.style.color = 'red';
            }

            allUsersContainerElement.appendChild(userElement);

        };
    };

});