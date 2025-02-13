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
using System.Net;

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
                if (tcpClient == null || !tcpClient.Connected)
                {
                    string serverIP = txtServerIP.Text;
                    int serverPort = int.Parse(txtServerPort.Text);

                    tcpClient = new TcpClient(serverIP, serverPort);
                    tcpStream = tcpClient.GetStream();
                    lblTCPStatus.Text = $"✅ TCP 서버에 연결됨: {serverIP}:{serverPort}";

                    StartTCPReceiver();
                }

                string input = txtCommand.Text.Trim(); // 사용자가 입력한 명령어
                string[] parts = input.Split(new[] { ',' }, 3);


                if (parts.Length < 1)
                {
                    MessageBox.Show("🚨 올바른 명령어를 입력하세요!");
                    return;
                }


                byte command = Convert.ToByte(parts[0], 16);
                byte[] packet;

                if (command == 0x02)
                {
                    if(parts.Length < 3)
            {
                        MessageBox.Show("🚨 올바른 형식: 02,대상ID,메시지");
                        return;
                    }

                    string targetClientId = parts[1];
                    string message = parts[2];

                    byte[] clientIdBytes = Encoding.UTF8.GetBytes(targetClientId);
                    byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                    int dataLength = clientIdBytes.Length + 1 + messageBytes.Length;

                    packet = new byte[1 + 4 + dataLength];
                    packet[0] = command;
                    BitConverter.GetBytes(IPAddress.HostToNetworkOrder(dataLength)).CopyTo(packet, 1);
                    clientIdBytes.CopyTo(packet, 5);
                    packet[5 + clientIdBytes.Length] = 0x00;
                    messageBytes.CopyTo(packet, 6 + clientIdBytes.Length);
                } else
                {
                    byte[] data = Encoding.UTF8.GetBytes("데이터");
                    byte[] length = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data.Length));
                    packet = new byte[1 + 4 + data.Length];
                    packet[0] = command;
                    Array.Copy(length, 0, packet, 1, 4);
                    Array.Copy(data, 0, packet, 5, data.Length);
                }


                tcpStream.Write(packet, 0, packet.Length);

                lstMessages.Items.Add($"📤 명령 전송: 0x{command:X2}");


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
                byte[] buffer = new byte[1024]; // 충분한 버퍼 크기 확보

                this.Invoke((MethodInvoker)delegate
                {
                    lstMessages.Items.Add("✅ ReceiveTCPData 함수 실행됨");
                });

                while (tcpClient.Connected)
                {
                    if (tcpStream.DataAvailable)
                    {
                        int bytesRead = tcpStream.Read(buffer, 0, buffer.Length); // 🚨 DataAvailable 체크 없이 바로 읽기


                        if (bytesRead > 0)
                        {
                            byte responseCode = buffer[0];

                            // ✅ 수신된 RAW 데이터 로그 추가
                            string receivedHex = BitConverter.ToString(buffer, 0, bytesRead);
                            this.Invoke((MethodInvoker)delegate
                            {
                                lstMessages.Items.Add($"📥 수신된 RAW 데이터: {receivedHex}");
                            });

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

                            string receivedMessage = Encoding.UTF8.GetString(buffer, 5, dataLength);

                            if (responseCode == 0x11) // 클라이언트 목록 수신
                            {
                                this.Invoke((MethodInvoker)delegate
                                {
                                    lstMessages.Items.Add("✅ 연결된 클라이언트 목록:");
                                    foreach (var clientId in receivedMessage.Split(','))
                                    {
                                        lstMessages.Items.Add($"👤 {clientId}");
                                    }
                                });
                            }
                            else if (responseCode == 0x12) // 개인 메시지 수신
                            {
                                this.Invoke((MethodInvoker)delegate
                                {
                                    lstMessages.Items.Add($"📩 개인 메시지 수신: {receivedMessage}");
                                });
                            }
                            else
                            {
                                this.Invoke((MethodInvoker)delegate
                                {
                                    lstMessages.Items.Add($"❓ 알 수 없는 응답 코드: 0x{responseCode:X2}");
                                });
                            }
                        } else
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                lstMessages.Items.Add("⚠️ Read() 실행했지만 데이터 없음");
                            });
                        }

                    } else
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            lstMessages.Items.Add("⏳ 데이터 없음, 대기 중...");
                        });
                        Thread.Sleep(100); // CPU 점유율 방지
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


        private void StartTCPReceiver()
        {
            Task.Run(() =>
            {
                try
                {
                    while (tcpClient != null && tcpClient.Connected)
                    {
                        ReceiveTCPData();
                    }
                }

                catch (Exception ex)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        lstMessages.Items.Add($"❌ TCP 수신 오류: {ex.Message}");
                    });
                }
            });
        }

    }
}
