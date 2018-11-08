namespace myClientServer
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        /*protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }*/

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.notifyIcon1.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.buttonServer = new System.Windows.Forms.Button();
            this.textBoxClientIp = new System.Windows.Forms.TextBox();
            this.textBoxMessage = new System.Windows.Forms.TextBox();
            this.labelMessage = new System.Windows.Forms.Label();
            this.numericUpDownClient = new System.Windows.Forms.NumericUpDown();
            this.buttonSearchPort = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.StatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripSplitButton1 = new System.Windows.Forms.ToolStripSplitButton();
            this.StatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.buttonStopRun = new System.Windows.Forms.Button();
            this.comboBoxAction = new System.Windows.Forms.ComboBox();
            this.buttonAction = new System.Windows.Forms.Button();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.panelRichTextBox = new System.Windows.Forms.Panel();
            this.richTextBoxEcho = new System.Windows.Forms.RichTextBox();
            this.buttonSwitchPanels = new System.Windows.Forms.Button();
            this.panelOpenPorts = new System.Windows.Forms.Panel();
            this.dgView = new System.Windows.Forms.DataGridView();
            this.labelTarget = new System.Windows.Forms.Label();
            this.labelAction = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownClient)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.panelRichTextBox.SuspendLayout();
            this.panelOpenPorts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgView)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonServer
            // 
            this.buttonServer.Location = new System.Drawing.Point(13, 13);
            this.buttonServer.Name = "buttonServer";
            this.buttonServer.Size = new System.Drawing.Size(48, 47);
            this.buttonServer.TabIndex = 0;
            this.buttonServer.Text = "Server";
            this.buttonServer.UseVisualStyleBackColor = true;
            this.buttonServer.Visible = false;
            this.buttonServer.Click += new System.EventHandler(this.buttonServer_Click);
            // 
            // textBoxClientIp
            // 
            this.textBoxClientIp.Location = new System.Drawing.Point(336, 15);
            this.textBoxClientIp.Name = "textBoxClientIp";
            this.textBoxClientIp.Size = new System.Drawing.Size(178, 20);
            this.textBoxClientIp.TabIndex = 4;
            this.textBoxClientIp.Leave += new System.EventHandler(this.textBoxClientIp_Leave);
            // 
            // textBoxMessage
            // 
            this.textBoxMessage.Location = new System.Drawing.Point(336, 41);
            this.textBoxMessage.Name = "textBoxMessage";
            this.textBoxMessage.Size = new System.Drawing.Size(248, 20);
            this.textBoxMessage.TabIndex = 7;
            this.textBoxMessage.Click += new System.EventHandler(this.textBoxMessage_Click);
            this.textBoxMessage.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxMessage_KeyDown);
            // 
            // labelMessage
            // 
            this.labelMessage.AutoSize = true;
            this.labelMessage.Location = new System.Drawing.Point(287, 44);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new System.Drawing.Size(50, 13);
            this.labelMessage.TabIndex = 8;
            this.labelMessage.Text = "Message";
            // 
            // numericUpDownClient
            // 
            this.numericUpDownClient.Location = new System.Drawing.Point(530, 15);
            this.numericUpDownClient.Maximum = new decimal(new int[] {
            1400,
            0,
            0,
            0});
            this.numericUpDownClient.Minimum = new decimal(new int[] {
            1200,
            0,
            0,
            0});
            this.numericUpDownClient.Name = "numericUpDownClient";
            this.numericUpDownClient.Size = new System.Drawing.Size(52, 20);
            this.numericUpDownClient.TabIndex = 11;
            this.numericUpDownClient.Value = new decimal(new int[] {
            1200,
            0,
            0,
            0});
            this.numericUpDownClient.Visible = false;
            this.numericUpDownClient.ValueChanged += new System.EventHandler(this.numericUpDownClient_ValueChanged);
            // 
            // buttonSearchPort
            // 
            this.buttonSearchPort.Location = new System.Drawing.Point(13, 65);
            this.buttonSearchPort.Name = "buttonSearchPort";
            this.buttonSearchPort.Size = new System.Drawing.Size(76, 23);
            this.buttonSearchPort.TabIndex = 12;
            this.buttonSearchPort.Text = "Scan ports";
            this.buttonSearchPort.UseVisualStyleBackColor = true;
            this.buttonSearchPort.Click += new System.EventHandler(this.buttonSearchPort_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel1,
            this.toolStripSplitButton1,
            this.StatusLabel2});
            this.statusStrip1.Location = new System.Drawing.Point(0, 339);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(600, 22);
            this.statusStrip1.TabIndex = 15;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // StatusLabel1
            // 
            this.StatusLabel1.Name = "StatusLabel1";
            this.StatusLabel1.Size = new System.Drawing.Size(73, 17);
            this.StatusLabel1.Text = "StatusLabel1";
            // 
            // toolStripSplitButton1
            // 
            this.toolStripSplitButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSplitButton1.Image = global::myClientServer.Properties.Resources.RYIKpng;
            this.toolStripSplitButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButton1.Name = "toolStripSplitButton1";
            this.toolStripSplitButton1.Size = new System.Drawing.Size(32, 20);
            this.toolStripSplitButton1.Text = "toolStripSplitButton1";
            // 
            // StatusLabel2
            // 
            this.StatusLabel2.Name = "StatusLabel2";
            this.StatusLabel2.Size = new System.Drawing.Size(480, 17);
            this.StatusLabel2.Spring = true;
            this.StatusLabel2.Text = "StatusLabel2";
            this.StatusLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonStopRun
            // 
            this.buttonStopRun.Location = new System.Drawing.Point(106, 65);
            this.buttonStopRun.Name = "buttonStopRun";
            this.buttonStopRun.Size = new System.Drawing.Size(80, 23);
            this.buttonStopRun.TabIndex = 17;
            this.buttonStopRun.Text = "Stop/Run Client";
            this.buttonStopRun.UseVisualStyleBackColor = true;
            this.buttonStopRun.Click += new System.EventHandler(this.buttonStopRun_Click);
            // 
            // comboBoxAction
            // 
            this.comboBoxAction.FormattingEnabled = true;
            this.comboBoxAction.Location = new System.Drawing.Point(336, 67);
            this.comboBoxAction.Name = "comboBoxAction";
            this.comboBoxAction.Size = new System.Drawing.Size(178, 21);
            this.comboBoxAction.TabIndex = 18;
            // 
            // buttonAction
            // 
            this.buttonAction.Location = new System.Drawing.Point(520, 65);
            this.buttonAction.Name = "buttonAction";
            this.buttonAction.Size = new System.Drawing.Size(64, 23);
            this.buttonAction.TabIndex = 19;
            this.buttonAction.Text = "Action";
            this.buttonAction.UseVisualStyleBackColor = true;
            this.buttonAction.Click += new System.EventHandler(this.buttonAction_Click);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.DoubleClick += new System.EventHandler(this.notifyIcon1_DoubleClick);
            // 
            // panelRichTextBox
            // 
            this.panelRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelRichTextBox.Controls.Add(this.richTextBoxEcho);
            this.panelRichTextBox.Location = new System.Drawing.Point(0, 110);
            this.panelRichTextBox.Name = "panelRichTextBox";
            this.panelRichTextBox.Size = new System.Drawing.Size(600, 225);
            this.panelRichTextBox.TabIndex = 20;
            // 
            // richTextBoxEcho
            // 
            this.richTextBoxEcho.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxEcho.Location = new System.Drawing.Point(3, 3);
            this.richTextBoxEcho.Name = "richTextBoxEcho";
            this.richTextBoxEcho.Size = new System.Drawing.Size(594, 219);
            this.richTextBoxEcho.TabIndex = 14;
            this.richTextBoxEcho.Text = "";
            // 
            // buttonSwitchPanels
            // 
            this.buttonSwitchPanels.Location = new System.Drawing.Point(201, 65);
            this.buttonSwitchPanels.Name = "buttonSwitchPanels";
            this.buttonSwitchPanels.Size = new System.Drawing.Size(80, 23);
            this.buttonSwitchPanels.TabIndex = 21;
            this.buttonSwitchPanels.Text = "Panel";
            this.buttonSwitchPanels.UseVisualStyleBackColor = true;
            this.buttonSwitchPanels.Click += new System.EventHandler(this.buttonSwitchPanels_Click);
            // 
            // panelOpenPorts
            // 
            this.panelOpenPorts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelOpenPorts.Controls.Add(this.dgView);
            this.panelOpenPorts.Location = new System.Drawing.Point(0, 110);
            this.panelOpenPorts.Name = "panelOpenPorts";
            this.panelOpenPorts.Size = new System.Drawing.Size(600, 225);
            this.panelOpenPorts.TabIndex = 21;
            // 
            // dgView
            // 
            this.dgView.AllowUserToOrderColumns = true;
            this.dgView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgView.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgView.Location = new System.Drawing.Point(3, 3);
            this.dgView.Name = "dgView";
            this.dgView.ReadOnly = true;
            this.dgView.Size = new System.Drawing.Size(594, 219);
            this.dgView.TabIndex = 15;
            // 
            // labelTarget
            // 
            this.labelTarget.AutoSize = true;
            this.labelTarget.Location = new System.Drawing.Point(292, 18);
            this.labelTarget.Name = "labelTarget";
            this.labelTarget.Size = new System.Drawing.Size(38, 13);
            this.labelTarget.TabIndex = 22;
            this.labelTarget.Text = "Target";
            // 
            // labelAction
            // 
            this.labelAction.AutoSize = true;
            this.labelAction.Location = new System.Drawing.Point(293, 70);
            this.labelAction.Name = "labelAction";
            this.labelAction.Size = new System.Drawing.Size(37, 13);
            this.labelAction.TabIndex = 23;
            this.labelAction.Text = "Action";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "*.*";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 361);
            this.Controls.Add(this.panelRichTextBox);
            this.Controls.Add(this.panelOpenPorts);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.numericUpDownClient);
            this.Controls.Add(this.comboBoxAction);
            this.Controls.Add(this.textBoxClientIp);
            this.Controls.Add(this.textBoxMessage);
            this.Controls.Add(this.buttonServer);
            this.Controls.Add(this.buttonAction);
            this.Controls.Add(this.buttonSwitchPanels);
            this.Controls.Add(this.buttonStopRun);
            this.Controls.Add(this.buttonSearchPort);
            this.Controls.Add(this.labelMessage);
            this.Controls.Add(this.labelAction);
            this.Controls.Add(this.labelTarget);
            this.Name = "Form1";
            this.Text = "Client-Server Communicator2";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownClient)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panelRichTextBox.ResumeLayout(false);
            this.panelOpenPorts.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonServer;
        private System.Windows.Forms.TextBox textBoxClientIp;
        private System.Windows.Forms.TextBox textBoxMessage;
        private System.Windows.Forms.Label labelMessage;
        private System.Windows.Forms.NumericUpDown numericUpDownClient;
        private System.Windows.Forms.Button buttonSearchPort;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel1;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButton1;
        private System.Windows.Forms.Button buttonStopRun;
        private System.Windows.Forms.ComboBox comboBoxAction;
        private System.Windows.Forms.Button buttonAction;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.Panel panelRichTextBox;
        private System.Windows.Forms.Button buttonSwitchPanels;
        private System.Windows.Forms.Panel panelOpenPorts;
        private System.Windows.Forms.DataGridView dgView;
        private System.Windows.Forms.Label labelTarget;
        private System.Windows.Forms.Label labelAction;
        public System.Windows.Forms.RichTextBox richTextBoxEcho;
        public System.Windows.Forms.ToolStripStatusLabel StatusLabel2;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}

