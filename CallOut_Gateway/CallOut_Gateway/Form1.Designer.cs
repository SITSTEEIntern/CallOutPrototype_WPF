namespace CallOut_Gateway
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.txtConn = new System.Windows.Forms.TextBox();
            this.btnSetCAD = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.dgvCoding = new System.Windows.Forms.DataGridView();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dgvMessage = new System.Windows.Forms.DataGridView();
            this.button4 = new System.Windows.Forms.Button();
            this.comboBox4 = new System.Windows.Forms.ComboBox();
            this.comboBox3 = new System.Windows.Forms.ComboBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.dgvStation = new System.Windows.Forms.DataGridView();
            this.btnStationBroadcast = new System.Windows.Forms.Button();
            this.btnStationTarget = new System.Windows.Forms.Button();
            this.txtTestMsg = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.btnSetConsole = new System.Windows.Forms.Button();
            this.btnJoinCAD = new System.Windows.Forms.Button();
            this.btnJoinConsole = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCoding)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMessage)).BeginInit();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStation)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(137, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "CAD WCF Endpoint";
            // 
            // txtConn
            // 
            this.txtConn.Location = new System.Drawing.Point(244, 13);
            this.txtConn.Name = "txtConn";
            this.txtConn.Size = new System.Drawing.Size(166, 20);
            this.txtConn.TabIndex = 1;
            // 
            // btnSetCAD
            // 
            this.btnSetCAD.Location = new System.Drawing.Point(416, 12);
            this.btnSetCAD.Name = "btnSetCAD";
            this.btnSetCAD.Size = new System.Drawing.Size(75, 23);
            this.btnSetCAD.TabIndex = 2;
            this.btnSetCAD.Text = "Set";
            this.btnSetCAD.UseVisualStyleBackColor = true;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPage1);
            this.tabControl.Controls.Add(this.tabPage2);
            this.tabControl.Controls.Add(this.tabPage3);
            this.tabControl.Location = new System.Drawing.Point(12, 69);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(742, 257);
            this.tabControl.TabIndex = 3;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.dgvCoding);
            this.tabPage1.Controls.Add(this.textBox1);
            this.tabPage1.Controls.Add(this.btnSearch);
            this.tabPage1.Controls.Add(this.comboBox1);
            this.tabPage1.Controls.Add(this.comboBox2);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(734, 231);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Coding";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // dgvCoding
            // 
            this.dgvCoding.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCoding.Location = new System.Drawing.Point(8, 6);
            this.dgvCoding.Name = "dgvCoding";
            this.dgvCoding.Size = new System.Drawing.Size(720, 190);
            this.dgvCoding.TabIndex = 8;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(132, 205);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(179, 20);
            this.textBox1.TabIndex = 4;
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(515, 202);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(75, 23);
            this.btnSearch.TabIndex = 7;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = true;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(317, 204);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(94, 21);
            this.comboBox1.TabIndex = 5;
            // 
            // comboBox2
            // 
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Location = new System.Drawing.Point(417, 204);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(92, 21);
            this.comboBox2.TabIndex = 6;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dgvMessage);
            this.tabPage2.Controls.Add(this.button4);
            this.tabPage2.Controls.Add(this.comboBox4);
            this.tabPage2.Controls.Add(this.comboBox3);
            this.tabPage2.Controls.Add(this.textBox3);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(734, 231);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Message";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dgvMessage
            // 
            this.dgvMessage.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMessage.Location = new System.Drawing.Point(75, 8);
            this.dgvMessage.Name = "dgvMessage";
            this.dgvMessage.Size = new System.Drawing.Size(584, 191);
            this.dgvMessage.TabIndex = 4;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(542, 205);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 3;
            this.button4.Text = "Search";
            this.button4.UseVisualStyleBackColor = true;
            // 
            // comboBox4
            // 
            this.comboBox4.FormattingEnabled = true;
            this.comboBox4.Location = new System.Drawing.Point(435, 207);
            this.comboBox4.Name = "comboBox4";
            this.comboBox4.Size = new System.Drawing.Size(88, 21);
            this.comboBox4.TabIndex = 2;
            // 
            // comboBox3
            // 
            this.comboBox3.FormattingEnabled = true;
            this.comboBox3.Location = new System.Drawing.Point(318, 207);
            this.comboBox3.Name = "comboBox3";
            this.comboBox3.Size = new System.Drawing.Size(98, 21);
            this.comboBox3.TabIndex = 1;
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(143, 207);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(159, 20);
            this.textBox3.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.dgvStation);
            this.tabPage3.Controls.Add(this.btnStationBroadcast);
            this.tabPage3.Controls.Add(this.btnStationTarget);
            this.tabPage3.Controls.Add(this.txtTestMsg);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(734, 231);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Station";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // dgvStation
            // 
            this.dgvStation.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvStation.Location = new System.Drawing.Point(136, 11);
            this.dgvStation.Name = "dgvStation";
            this.dgvStation.Size = new System.Drawing.Size(470, 173);
            this.dgvStation.TabIndex = 3;
            // 
            // btnStationBroadcast
            // 
            this.btnStationBroadcast.Location = new System.Drawing.Point(514, 196);
            this.btnStationBroadcast.Name = "btnStationBroadcast";
            this.btnStationBroadcast.Size = new System.Drawing.Size(78, 23);
            this.btnStationBroadcast.TabIndex = 2;
            this.btnStationBroadcast.Text = "Broadcast";
            this.btnStationBroadcast.UseVisualStyleBackColor = true;
            this.btnStationBroadcast.Click += new System.EventHandler(this.btnStationBroadcast_Click);
            // 
            // btnStationTarget
            // 
            this.btnStationTarget.Location = new System.Drawing.Point(411, 196);
            this.btnStationTarget.Name = "btnStationTarget";
            this.btnStationTarget.Size = new System.Drawing.Size(91, 23);
            this.btnStationTarget.TabIndex = 1;
            this.btnStationTarget.Text = "Send Test Msg";
            this.btnStationTarget.UseVisualStyleBackColor = true;
            this.btnStationTarget.Click += new System.EventHandler(this.btnStationTarget_Click);
            // 
            // txtTestMsg
            // 
            this.txtTestMsg.Location = new System.Drawing.Point(147, 198);
            this.txtTestMsg.Name = "txtTestMsg";
            this.txtTestMsg.Size = new System.Drawing.Size(232, 20);
            this.txtTestMsg.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(121, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(117, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Console WCF Endpoint";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(244, 45);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(166, 20);
            this.textBox2.TabIndex = 5;
            // 
            // btnSetConsole
            // 
            this.btnSetConsole.Location = new System.Drawing.Point(416, 44);
            this.btnSetConsole.Name = "btnSetConsole";
            this.btnSetConsole.Size = new System.Drawing.Size(75, 23);
            this.btnSetConsole.TabIndex = 6;
            this.btnSetConsole.Text = "Set";
            this.btnSetConsole.UseVisualStyleBackColor = true;
            // 
            // btnJoinCAD
            // 
            this.btnJoinCAD.Location = new System.Drawing.Point(497, 12);
            this.btnJoinCAD.Name = "btnJoinCAD";
            this.btnJoinCAD.Size = new System.Drawing.Size(96, 23);
            this.btnJoinCAD.TabIndex = 7;
            this.btnJoinCAD.Text = "Join CAD";
            this.btnJoinCAD.UseVisualStyleBackColor = true;
            this.btnJoinCAD.Click += new System.EventHandler(this.btnJoinCAD_Click);
            // 
            // btnJoinConsole
            // 
            this.btnJoinConsole.Location = new System.Drawing.Point(497, 43);
            this.btnJoinConsole.Name = "btnJoinConsole";
            this.btnJoinConsole.Size = new System.Drawing.Size(96, 23);
            this.btnJoinConsole.TabIndex = 8;
            this.btnJoinConsole.Text = "Join Console";
            this.btnJoinConsole.UseVisualStyleBackColor = true;
            this.btnJoinConsole.Click += new System.EventHandler(this.btnJoinConsole_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(766, 338);
            this.Controls.Add(this.btnJoinConsole);
            this.Controls.Add(this.btnJoinCAD);
            this.Controls.Add(this.btnSetConsole);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.btnSetCAD);
            this.Controls.Add(this.txtConn);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Call Out Gateway";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tabControl.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCoding)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMessage)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStation)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtConn;
        private System.Windows.Forms.Button btnSetCAD;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.DataGridView dgvCoding;
        private System.Windows.Forms.DataGridView dgvMessage;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.ComboBox comboBox4;
        private System.Windows.Forms.ComboBox comboBox3;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.DataGridView dgvStation;
        private System.Windows.Forms.Button btnStationBroadcast;
        private System.Windows.Forms.Button btnStationTarget;
        private System.Windows.Forms.TextBox txtTestMsg;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button btnSetConsole;
        private System.Windows.Forms.Button btnJoinCAD;
        private System.Windows.Forms.Button btnJoinConsole;
    }
}

