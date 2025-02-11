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


namespace ChatClient
{
    public partial class Form1 : Form
    {
        private SocketIOClient.SocketIO socket;
        private string clientId;

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
                lblStatus.Invoke(new Action(() => lblStatus.Text = "서버에 연결됨"));
            };

            socket.On("init", response =>
            {
                // JSON 객체에서 값을 가져오기 위해 JsonElement 사용
                var json = response.GetValue<System.Text.Json.JsonElement>();
                clientId = json.GetProperty("clientId").GetString();  // JSON 속성 접근
                lblStatus.Invoke(new Action(() => lblStatus.Text = $"서버에 연결됨: {clientId}"));
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
                lblStatus.Text = "서버 연결 해제됨";
            }
        }
    }
}
