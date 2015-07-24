namespace CallOut_CAD
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
            this.btnSample1 = new System.Windows.Forms.Button();
            this.btnJoinCAD = new System.Windows.Forms.Button();
            this.btnSample2 = new System.Windows.Forms.Button();
            this.btnSample3 = new System.Windows.Forms.Button();
            this.dgvIncident = new System.Windows.Forms.DataGridView();
            this.label2 = new System.Windows.Forms.Label();
            this.txtQuery = new System.Windows.Forms.TextBox();
            this.btnQuery = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvIncident)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(25, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(154, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "CAD Sample Incident Message";
            // 
            // btnSample1
            // 
            this.btnSample1.Location = new System.Drawing.Point(28, 43);
            this.btnSample1.Name = "btnSample1";
            this.btnSample1.Size = new System.Drawing.Size(75, 23);
            this.btnSample1.TabIndex = 2;
            this.btnSample1.Text = "Sample 1";
            this.btnSample1.UseVisualStyleBackColor = true;
            this.btnSample1.Click += new System.EventHandler(this.btnSample1_Click);
            // 
            // btnJoinCAD
            // 
            this.btnJoinCAD.Location = new System.Drawing.Point(688, 17);
            this.btnJoinCAD.Name = "btnJoinCAD";
            this.btnJoinCAD.Size = new System.Drawing.Size(75, 23);
            this.btnJoinCAD.TabIndex = 3;
            this.btnJoinCAD.Text = "Join CAD";
            this.btnJoinCAD.UseVisualStyleBackColor = true;
            this.btnJoinCAD.Click += new System.EventHandler(this.btnJoinCAD_Click);
            // 
            // btnSample2
            // 
            this.btnSample2.Location = new System.Drawing.Point(134, 43);
            this.btnSample2.Name = "btnSample2";
            this.btnSample2.Size = new System.Drawing.Size(75, 23);
            this.btnSample2.TabIndex = 4;
            this.btnSample2.Text = "Sample 2";
            this.btnSample2.UseVisualStyleBackColor = true;
            this.btnSample2.Click += new System.EventHandler(this.btnSample2_Click);
            // 
            // btnSample3
            // 
            this.btnSample3.Location = new System.Drawing.Point(233, 43);
            this.btnSample3.Name = "btnSample3";
            this.btnSample3.Size = new System.Drawing.Size(75, 23);
            this.btnSample3.TabIndex = 5;
            this.btnSample3.Text = "Sample 3";
            this.btnSample3.UseVisualStyleBackColor = true;
            this.btnSample3.Click += new System.EventHandler(this.btnSample3_Click);
            // 
            // dgvIncident
            // 
            this.dgvIncident.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvIncident.Location = new System.Drawing.Point(9, 135);
            this.dgvIncident.Name = "dgvIncident";
            this.dgvIncident.Size = new System.Drawing.Size(766, 187);
            this.dgvIncident.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(25, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(173, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "CAD Incident  Coding Status Query";
            // 
            // txtQuery
            // 
            this.txtQuery.Location = new System.Drawing.Point(28, 102);
            this.txtQuery.Name = "txtQuery";
            this.txtQuery.Size = new System.Drawing.Size(181, 20);
            this.txtQuery.TabIndex = 8;
            // 
            // btnQuery
            // 
            this.btnQuery.Location = new System.Drawing.Point(233, 99);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(75, 23);
            this.btnQuery.TabIndex = 9;
            this.btnQuery.Text = "Query";
            this.btnQuery.UseVisualStyleBackColor = true;
            this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 334);
            this.Controls.Add(this.btnQuery);
            this.Controls.Add(this.txtQuery);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.dgvIncident);
            this.Controls.Add(this.btnSample3);
            this.Controls.Add(this.btnSample2);
            this.Controls.Add(this.btnJoinCAD);
            this.Controls.Add(this.btnSample1);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Call Out CAD";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvIncident)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSample1;
        private System.Windows.Forms.Button btnJoinCAD;
        private System.Windows.Forms.Button btnSample2;
        private System.Windows.Forms.Button btnSample3;
        private System.Windows.Forms.DataGridView dgvIncident;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtQuery;
        private System.Windows.Forms.Button btnQuery;
    }
}

