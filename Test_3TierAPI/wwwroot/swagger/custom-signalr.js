console.log("[Swagger] Custom SignalR script loaded.");

const connection = new signalR.HubConnectionBuilder()
    //.withUrl("http://172.16.32.83:6999/api/orchestration/ntorchestrationhub")
    .withUrl("http://172.16.32.50:6999/api/orchestration/ntorchestrationhub")      // 본인 ip로 변경
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

// 이벤트 핸들러 등록
registerPingReceiver(connection);

// Swagger 로드되자마자 SignalR 연결 시작
window.addEventListener("load", async function () {
    try {
        console.log("[SignalR] Starting connection...");
        await connection.start();
        console.log("[SignalR] Connection started.");

    } catch (err) {
        console.error("[SignalR] Connection error:", err);
    }
});

function registerPingReceiver(connection) {
    connection.on("Ping", (message) => {
        console.log("[SignalR] Received Ping:", message);
        alert("서버 Ping 수신: " + message);
    });

    connection.on("CompleteJob", (message) => {
        console.log("[SignalR] Complete Job Event Received:", message);
        alert("작업 완료됨: " + message);
    });
}
