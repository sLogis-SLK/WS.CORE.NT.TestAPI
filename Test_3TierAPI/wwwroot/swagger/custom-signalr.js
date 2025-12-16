console.log("[Swagger] Custom SignalR script loaded.");

function createAuthorizedSignalRConnection(token) {
    return new signalR.HubConnectionBuilder()
        .withUrl(
            `http://172.16.32.50:6999/api/orchestration/ntorchestrationhub?access_token=${encodeURIComponent(token)}`,
            {
                withCredentials: true
            }
        )
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();
}

function registerPingReceiver(connection) {
    connection.on("Ping", (message) => {
        console.log("[SignalR] Received Ping:", message);
    });
    connection.on("CompleteJob", (message) => {
        console.log("[SignalR] CompleteJob Event:", message);
    });
    connection.on("BatchFailed", (message) => {
        console.log("[SignalR] BatchFailed Event:", message);
    });
}

let connection = null;

async function connectSignalR(token) {
    if (!token) {
        console.warn("[SignalR] No token provided");
        return;
    }

    if (connection?.state === signalR.HubConnectionState.Connected) {
        await connection.stop();
    }

    try {
        connection = createAuthorizedSignalRConnection(token);
        registerPingReceiver(connection);
        console.log("[SignalR] Starting connection...");
        await connection.start();
        console.log("[SignalR] Connection started successfully");
    } catch (err) {
        console.error("[SignalR] Connection failed:", err);
    }
}

/**
 * SignalR Connect UI 생성
 */
function createSignalRConnectUI() {
    // 컨테이너
    const container = document.createElement("div");
    container.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 9999;
        background: white;
        padding: 15px;
        border-radius: 4px;
        box-shadow: 0 2px 8px rgba(0,0,0,0.2);
    `;

    // 레이블
    const label = document.createElement("label");
    label.textContent = "SignalR Token:";
    label.style.cssText = `
        display: block;
        margin-bottom: 8px;
        font-weight: bold;
        font-size: 12px;
    `;

    // 입력 필드
    const input = document.createElement("input");
    input.type = "password";
    input.placeholder = "Paste JWT token here";
    input.style.cssText = `
        width: 300px;
        padding: 8px;
        border: 1px solid #ddd;
        border-radius: 4px;
        margin-bottom: 8px;
        font-size: 12px;
        font-family: monospace;
    `;

    // 버튼 컨테이너
    const buttonContainer = document.createElement("div");
    buttonContainer.style.cssText = `
        display: flex;
        gap: 8px;
    `;

    // Connect 버튼
    const connectBtn = document.createElement("button");
    connectBtn.textContent = "Connect";
    connectBtn.style.cssText = `
        flex: 1;
        padding: 8px 12px;
        background-color: #4CAF50;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 12px;
        font-weight: bold;
    `;

    connectBtn.addEventListener("click", () => {
        const token = input.value.trim();
        if (token) {
            console.log("[SignalR] Connecting...");
            connectSignalR(token);
            connectBtn.textContent = "✓ Connected";
            connectBtn.style.backgroundColor = "#2196F3";
            setTimeout(() => {
                connectBtn.textContent = "Connect";
                connectBtn.style.backgroundColor = "#4CAF50";
            }, 2000);
        } else {
            console.warn("[SignalR] Please paste token first!");
            connectBtn.textContent = "✗ No Token";
            connectBtn.style.backgroundColor = "#f44336";
            setTimeout(() => {
                connectBtn.textContent = "Connect";
                connectBtn.style.backgroundColor = "#4CAF50";
            }, 2000);
        }
    });

    // Disconnect 버튼
    const disconnectBtn = document.createElement("button");
    disconnectBtn.textContent = "Disconnect";
    disconnectBtn.style.cssText = `
        padding: 8px 12px;
        background-color: #ff9800;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 12px;
        font-weight: bold;
    `;

    disconnectBtn.addEventListener("click", async () => {
        if (connection?.state === signalR.HubConnectionState.Connected) {
            await connection.stop();
            connection = null;
            console.log("[SignalR] Disconnected");
            disconnectBtn.textContent = "✓ Disconnected";
            setTimeout(() => {
                disconnectBtn.textContent = "Disconnect";
            }, 1500);
        }
    });

    buttonContainer.appendChild(connectBtn);
    buttonContainer.appendChild(disconnectBtn);

    container.appendChild(label);
    container.appendChild(input);
    container.appendChild(buttonContainer);
    document.body.appendChild(container);

    console.log("[SignalR] SignalR Connect UI added");
}

window.addEventListener("load", () => {
    console.log("[SignalR] Initializing...");

    setTimeout(() => {
        createSignalRConnectUI();
    }, 1000);
});