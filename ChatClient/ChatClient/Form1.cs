using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using SocketIOClient;
using System.Net.Sockets;
using System.Buffers.Binary;

namespace ChatClient
{
    public partial class Form1 : Form
    {
        private SocketIOClient.SocketIO socket;
        private string clientId;

        private TcpClient tcpClient;
        private NetworkStream tcpStream;


        public Form1()
        {
            InitializeComponent();
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            socket = new SocketIOClient.SocketIO("http://localhost:3000");

            socket.OnConnected += (s, ev) =>
            {
                Console.WriteLine("서버에 연결되었습니다.");
                lblSocketStatus.Invoke(new Action(() => lblSocketStatus.Text = "소켓 서버에 연결됨"));
            };

            socket.On("init", response =>
            {
                // JSON 객체에서 값을 가져오기 위해 JsonElement 사용
                var json = response.GetValue<System.Text.Json.JsonElement>();
                clientId = json.GetProperty("clientId").GetString();  // JSON 속성 접근
                lblSocketStatus.Invoke(new Action(() => lblSocketStatus.Text = $"서버에 연결됨: {clientId}"));
            });

            socket.On("message", response =>
            {
                // JSON 객체로부터 값 추출
                var json = response.GetValue<System.Text.Json.JsonElement>();
                string senderId = json.GetProperty("clientId").GetString();
                string message = json.GetProperty("message").GetString();

                lstMessages.Invoke(new Action(() =>
                {
                    lstMessages.Items.Add($"[{senderId}]: {message}");
                }));
            });

            try
            {
                await socket.ConnectAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"서버 연결 실패: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            if (socket == null || !socket.Connected)
            {
                MessageBox.Show("서버에 연결되지 않았습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            await SendMessageAsync(txtMessage.Text);
        }

        private async Task SendMessageAsync(string message)
        {
            var messageData = new
            {
                clientId = clientId,
                message = message
            };

            await socket.EmitAsync("message", messageData);
            txtMessage.Clear();
        }

        private async void btnDisconnect_Click(object sender, EventArgs e)
        {
            if (socket != null && socket.Connected)
            {
                await socket.DisconnectAsync();
                lblSocketStatus.Text = "서버 연결 해제됨";
            }

        }





        private void btnSendCommand_Click(object sender, EventArgs e)
        {
            try
            {
                if (tcpClient == null || !tcpClient.Connected )
                {
                    string serverIP = txtServerIP.Text;
                    int serverPort = int.Parse(txtServerPort.Text);

                    tcpClient = new TcpClient(serverIP, serverPort);
                    tcpStream = tcpClient.GetStream();
                    lblTCPStatus.Text = $"✅ TCP 서버에 연결됨: {serverIP}:{serverPort}";

                    // 데이터 수신 스레드 시작 ( UI 멈춤 현상 방지를 위해 따로 스레드를 배정해줌 )
                    //Thread receiveThread = new Thread(ReceiveTCPData);
                    //receiveThread.IsBackground = true;
                    //receiveThread.Start();
                }

                // ✅ 명령어를 확실하게 변환 (16진수 "01" -> 0x01)
                if (!byte.TryParse(txtCommand.Text, System.Globalization.NumberStyles.HexNumber, null, out byte command))
                {
                    MessageBox.Show("🚨 잘못된 명령어 입력! 16진수 형식으로 입력하세요 (예: 01, 02)");
                    return;
                }
                byte[] data = Encoding.UTF8.GetBytes(""); // 데이터는 비워둠
                byte[] length = BitConverter.GetBytes(data.Length);
                byte[] packet = new byte[1 + 4 + data.Length];
                packet[0] = command;
                Array.Copy(length, 0, packet, 1, 4);
                Array.Copy(data, 0, packet, 5, data.Length);

                tcpStream.Write(packet, 0, packet.Length);
                lstMessages.Items.Add($"📤 명령 전송: 0x{command:X2}");


                // ✅ 명령어 전송 후 수신 시작 (백그라운드 스레드)
                Task.Run(() => ReceiveTCPData());

            }
            catch (Exception ex) 
            {
                MessageBox.Show($"🚨 오류: {ex.Message}");
            }
        }


        // TCP 통신 수신 데이터
        private void ReceiveTCPData()
        {
            try
            {
                byte[] buffer = new byte[1024];

                while (tcpClient.Connected)
                {
                    int bytesRead = tcpStream.Read(buffer, 0, buffer.Length);

                    if (bytesRead > 5)
                    {
                        byte responseCode = buffer[0];

                        // ❌ 기존 코드 (리틀 엔디언으로 읽어서 문제 발생)
                        // int dataLength = BitConverter.ToInt32(buffer, 1);

                        // ✅ 수정 코드 (Big Endian 방식으로 읽기)
                        int dataLength = BinaryPrimitives.ReadInt32BigEndian(buffer.AsSpan(1));

                        // 데이터 길이 검증
                        if (dataLength < 0 || dataLength > buffer.Length - 5)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                lstMessages.Items.Add($"❌ 잘못된 데이터 길이 감지: {dataLength}");
                            });
                            return;
                        }

                        string clientList = Encoding.UTF8.GetString(buffer, 5, dataLength);

                        if (responseCode == 0x11)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                lstMessages.Items.Add("✅ 연결된 클라이언트 목록:");
                                foreach (var clientId in clientList.Split(','))
                                {
                                    lstMessages.Items.Add($"👤 {clientId}");
                                }
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    lstMessages.Items.Add($"❌ 데이터 수신 오류: {ex.Message}");
                });
            }
        }

    }
}
