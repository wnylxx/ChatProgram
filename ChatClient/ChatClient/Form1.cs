using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Windows.Forms;
using Newtonsoft.Json;


namespace ChatClient
{
    public partial class Form1 : Form
    {

        private ClientWebSocket clientWebSocket;
        private const string serverUri = "ws://localhost:3000";
        private string clientId;


        public Form1()
        {
            InitializeComponent();
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            clientWebSocket = new ClientWebSocket();

            try
            {
                await clientWebSocket.ConnectAsync(new Uri(serverUri), CancellationToken.None);
                ReceiveMessages();
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"연결 실패: {ex.Message}";
            }

        }

        private async void ReceiveMessages()
        {
            byte[] buffer = new byte[1024];

            while (clientWebSocket.State == WebSocketState.Open)
            {
                var result = await clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string jsonMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    try
                    {
                        var messageData = JsonConvert.DeserializeObject<MessageData>(jsonMessage);

                        if (messageData.type == "init")
                        {
                            clientId = messageData.clientId;
                            lblStatus.Invoke(new Action(() => lblStatus.Text = $"서버에 연결됨: {clientId}"));
                            continue; // 초기화 메시지는 리스트에 표시하지 않음

                        }

                        lstMessages.Invoke(new Action(() =>
                        {
                            lstMessages.Items.Add($"[{messageData.clientId}]: {messageData.message}");
                        }));
                    }
                    catch (JsonReaderException ex)
                    {
                        Console.WriteLine($"JSON 파싱 오류: {ex.Message}");
                    }
                }
            }
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            if (clientWebSocket.State != WebSocketState.Open)
            {
                MessageBox.Show("서버에 연결되지 않았습니다", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            await SendMessage(txtMessage.Text);
        }

        private async Task SendMessage(string message)
        {
            var messageData = new MessageData { clientId = clientId, message = message };
            string jsonMessage = JsonConvert.SerializeObject(messageData);
            byte[] buffer = Encoding.UTF8.GetBytes(jsonMessage);

            await clientWebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            // lstMessages.Items.Add($"[나] {message}");
            txtMessage.Clear();

        }

        private async void btnDisconnect_Click(object sender, EventArgs e)
        {
            if (clientWebSocket != null && clientWebSocket.State == WebSocketState.Open)
            {
                await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "클라이언트 종료", CancellationToken.None);
                lblStatus.Text = "서버 연결 해제됨";
            } 
        }



        public class MessageData
        {
            public string type { get; set; }
            public string clientId { get; set; }
            public string message { get; set; }
        }
    }
}
