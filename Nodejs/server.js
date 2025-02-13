const express = require('express');
const http = require('http');
// const WebSocket = require('ws'); // websocket 사용시
const { Server } = require('socket.io');
const net = require('net'); // TCP 서버 추가

const app = express();
const server = http.createServer(app);
// const wss = new WebSocket.Server({ server });
const io = new Server(server);

let clientCount = 0;
const connectedClients = new Map(); // 클라이언트 ID 관리

// 클라이언트 ID 생성 함수
function generateClientId() {
    clientCount++;
    return `user_${String(clientCount).padStart(3, '0')}`;
}

// 기본 라우터
app.get('/', (req, res) => {
    res.send('Socket.IO 서버가 실행 중입니다.');
});


// Socket.io 연결 처리리
io.on('connection', (socket) => {
    const clientId = generateClientId();
    connectedClients.set(clientId, socket)

    console.log(`✅ 새로운 클라이언트가 연결되었습니다: ${clientId}`);
    console.log('📋 현재 연결된 클라이언트:', Array.from(connectedClients.keys()));

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
        connectedClients.delete(clientId);
        console.log('📋 현재 연결된 클라이언트:', Array.from(connectedClients.keys()));

    });
});


//TCP 서버 설정
const tcpServer = net.createServer((socket) => {
    console.log('🔗 TCP 클라이언트 연결됨');

    socket.on('data', (data) => {
        const command = data.readUInt8(0); //명령어 (1바이트)
        const dataLength = data.readUInt32BE(1); // 데이터 길이 (4바이트)
        const payload = data.slice(5, 5 + dataLength).toString(); // 데이터 부분분

        console.log(`📩 수신된 명령어: ${command.toString(16).padStart(2, '0')}`);

        if (command === 0x01) { // 클라이언트 리스트 요청
            const clientIds = Array.from(connectedClients.keys()).join(',');
            const clientIdsBuffer = Buffer.from(clientIds, 'utf-8'); // 문자열을 Buffer로 변환
    
            const response = Buffer.alloc(1 + 4 + clientIdsBuffer.length); 
            response.writeUInt8(0x11, 0);                          // 응답 코드 (1바이트)
            response.writeUInt32BE(clientIdsBuffer.length, 1);     // 데이터 길이 (4바이트)
            clientIdsBuffer.copy(response, 5);                     // 실제 데이터 복사
    
            // 디버깅 용
            console.log("📤 서버가 클라이언트에게 보내는 데이터:", response);
            console.log("🔢 응답 코드:", response.readUInt8(0));  // 첫 바이트 (응답 코드)
            console.log("📏 데이터 길이:", response.readUInt32BE(1)); // 데이터 길이 (4바이트)
            console.log("💾 실제 데이터:", response.slice(5).toString());


            socket.write(response);                                // 클라이언트로 전송
            console.log(`👤 클라이언트 목록 전송: ${clientIds}`);

        } else if (command === 0x02) { // 개인 메시지 전송
            const payloadStr = payload.toString('utf-8');
            const parts = payloadStr.split('\x00');
            
            if (parts.length < 2) {
                console.log(`🚨 잘못된 데이터 형식! payload: "${payloadStr}"`);
                return;
            }

            const [targetClientId, message] = parts;
            console.log(`📩 개인 메시지 요청: 대상=${targetClientId}, 메시지="${message}"`);

            const targetSocket = connectedClients.get(targetClientId);

            if (targetSocket) {
                const messageBuffer = Buffer.from(message, 'utf-8');
                const response = Buffer.alloc(1 + 4 + messageBuffer.length);
                response.writeUInt8(0x12, 0);
                response.writeUInt32BE(messageBuffer.length, 1);
                messageBuffer.copy(response, 5);

                console.log(`📤 클라이언트(${targetClientId})에게 보낼 데이터:`, response);
                console.log(`📤 보낼 데이터 HEX: ${response.toString('hex')}`);
                console.log(`📤 보낼 데이터 크기: ${response.length}, 예상 크기: ${5 + messageBuffer.length}`);


                

            } else {
                console.log(`❌ 대상 클라이언트(${targetClientId})를 찾을 수 없음`);
            }

            // TCP 연결을 유지하는 지 확인용용
            console.log(`🔗 현재 연결된 클라이언트 목록:`, Array.from(connectedClients.keys()));
        } 
    });

    socket.on('close', () => {
        console.log('❌ TCP 클라이언트 연결 종료')
    });

    socket.on('error', (err) => {
        console.error('🚨 TCP 에러:', err);
    });
})

// 서버 시작
const HTTP_PORT = 3000;
const TCP_PORT = 4000;

server.listen(HTTP_PORT, () => {
    console.log(`✅ 서버가 http://localhost:${HTTP_PORT} 에서 실행 중입니다.`);
});

tcpServer.listen(TCP_PORT, () => {
    console.log(`✅ TCP 서버가 포트 ${TCP_PORT} 에서 실행 중입니다.`);
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


