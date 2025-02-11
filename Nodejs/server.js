const express = require('express');
const http = require('http');
// const WebSocket = require('ws');
const { Server } = require('socket.io');

const app = express();
const server = http.createServer(app);
// const wss = new WebSocket.Server({ server });
const io = new Server(server);

let clientCount = 0; 

// 클라이언트 ID 생성 함수
function generateClientId() {
    clientCount++;
    return `user_${String(clientCount).padStart(3, '0')}`;
}

// 기본 라우터
app.get('/', (req, res) => {
    res.send('Socket.IO 서버가 실행 중입니다.');
});

io.on('connection', (socket) => {
    const clientId = generateClientId();
    console.log(`✅ 새로운 클라이언트가 연결되었습니다: ${clientId}`);

    // 클라이언트들에게 초기화 메시지 전송(init)
    socket.emit('init', { clientId: clientId });

    // 메시지 수신 처리
    socket.on('message', (data) => {
        console.log(`💬 [${clientId}] 수신된 메시지: ${data.message}`);

        // 모든 클라이언트에게 메시지 전송
        io.emit('message', {
            clientId: clientId,
            message: data.message
        });

    });


    socket.on('disconnect', () => {
        console.log(`❌ 클라이언트 연결 해제: ${clientId}`);
    });


});






// wss.on('connection', (ws) => {
//     const clientId = generateClientId();
//     console.log(`✅ 새로운 클라이언트가 연결되었습니다: ${clientId}`);
//     ws.clientId = clientId;

//     ws.send(JSON.stringify({type: 'init', clientId}));

//     // 메시지 수신
//     ws.on('message', (message) => {
//         // 🛠️ 버퍼를 문자열로 변환
//         const textMessage = message.toString();  

//         console.log(`💬 [${clientId}] 수신된 메시지: ${textMessage}`);

//         // 클라이언트로부터 받은 메시지를 파싱
//         const parsedMessage = JSON.parse(message);

//         // 모든 클라이언트에게 JSON 객체 그대로 전송
//         const messageData = JSON.stringify({
//             clientId: clientId,
//             message: parsedMessage.message
//         });

//         // 모든 클라이언트에게 JSON 형식으로 메시지 전송
//         wss.clients.forEach((client) => {
//             if (client.readyState === WebSocket.OPEN) {
//                 client.send(messageData);  // 직렬화된 JSON 전송
//             }
//         });
//     });

//     // 연결 종료 시 로그 출력
//     ws.on('close', () => {
//         console.log(`❌ 클라이언트 연결 해제: ${clientId}`);
//     });
// });

const PORT = 3000;
server.listen(PORT, () => {
    console.log(`✅ 서버가 http://localhost:${PORT} 에서 실행 중입니다.`);
});
