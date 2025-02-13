namespace ChatClient
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.ListBox lstMessages;
        private System.Windows.Forms.Label lblSocketStatus;

        // 추가된 요소
        private System.Windows.Forms.TextBox txtServerIP;
        private System.Windows.Forms.TextBox txtServerPort;
        private System.Windows.Forms.TextBox txtCommand;
        private System.Windows.Forms.Button btnSendCommand;
        private System.Windows.Forms.Label lblTCPStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.lstMessages = new System.Windows.Forms.ListBox();
            this.lblSocketStatus = new System.Windows.Forms.Label();
            

            // 추가된 요소 초기화
            this.txtServerIP = new System.Windows.Forms.TextBox();
            this.txtServerPort = new System.Windows.Forms.TextBox();
            this.txtCommand = new System.Windows.Forms.TextBox();
            this.btnSendCommand = new System.Windows.Forms.Button();
            this.lblTCPStatus = new System.Windows.Forms.Label();

            this.SuspendLayout();

            // btnConnect
            this.btnConnect.Location = new System.Drawing.Point(20, 20);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(100, 30);
            this.btnConnect.Text = "서버 연결";
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);

            // btnSend
            this.btnSend.Location = new System.Drawing.Point(130, 20);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(100, 30);
            this.btnSend.Text = "메시지 전송";
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);

            // btnDisconnect
            this.btnDisconnect.Location = new System.Drawing.Point(240, 20);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(100, 30);
            this.btnDisconnect.Text = "연결 종료";
            this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);

            // txtMessage
            this.txtMessage.Location = new System.Drawing.Point(20, 60);
            this.txtMessage.Size = new System.Drawing.Size(320, 21);

            // lstMessages
            this.lstMessages.ItemHeight = 12;
            this.lstMessages.Location = new System.Drawing.Point(20, 90);
            this.lstMessages.Size = new System.Drawing.Size(320, 200);

            // lblSocketStatus
            this.lblSocketStatus.Location = new System.Drawing.Point(20, 400);
            this.lblSocketStatus.Name = "lblSocketStatus";
            this.lblSocketStatus.Size = new System.Drawing.Size(320, 20);
            this.lblSocketStatus.Text = "소켓 상태: 연결되지 않음";


            // txtServerIP
            this.txtServerIP.Location = new System.Drawing.Point(20, 300);
            this.txtServerIP.Size = new System.Drawing.Size(150, 21);
            this.txtServerIP.Text = "127.0.0.1";

            // txtServerPort
            this.txtServerPort.Location = new System.Drawing.Point(180, 300);
            this.txtServerPort.Size = new System.Drawing.Size(80, 21);
            this.txtServerPort.Text = "4000";

            // txtCommand
            this.txtCommand.Location = new System.Drawing.Point(20, 330);
            this.txtCommand.Size = new System.Drawing.Size(240, 21);
            this.txtCommand.Text = "명령어 입력 (예: 0x01)";

            // btnSendCommand
            this.btnSendCommand.Location = new System.Drawing.Point(270, 330);
            this.btnSendCommand.Size = new System.Drawing.Size(70, 23);
            this.btnSendCommand.Text = "명령 전송";
            this.btnSendCommand.Click += new System.EventHandler(this.btnSendCommand_Click);

            // lblTCPStatus
            this.lblTCPStatus.Location = new System.Drawing.Point(20, 420);
            this.lblTCPStatus.Name = "lblTCPStatus";
            this.lblTCPStatus.Size = new System.Drawing.Size(320, 20);
            this.lblTCPStatus.Text = "TCP 상태: 연결되지 않음";


            // Form1
            this.ClientSize = new System.Drawing.Size(370, 450);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.btnDisconnect);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.lstMessages);
            this.Controls.Add(this.lblSocketStatus);
            this.Controls.Add(this.txtServerIP);
            this.Controls.Add(this.txtServerPort);
            this.Controls.Add(this.txtCommand);
            this.Controls.Add(this.btnSendCommand);
            this.Controls.Add(this.lblTCPStatus);

            this.Name = "Form1";
            this.Text = "C# Chat Client";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}

