namespace Smartline.Server {
    partial class MainForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.тестоваяФормочкаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.button_start = new System.Windows.Forms.Button();
            this.button_stop = new System.Windows.Forms.Button();
            this.label_onlineTrackers = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button_createAdminUser = new System.Windows.Forms.Button();
            this.textBox_password = new System.Windows.Forms.TextBox();
            this.label_ReadMemcachedValue = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.тестоваяФормочкаToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(807, 24);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // тестоваяФормочкаToolStripMenuItem
            // 
            this.тестоваяФормочкаToolStripMenuItem.Name = "тестоваяФормочкаToolStripMenuItem";
            this.тестоваяФормочкаToolStripMenuItem.Size = new System.Drawing.Size(85, 20);
            this.тестоваяФормочкаToolStripMenuItem.Text = "под Каховку";
            this.тестоваяФормочкаToolStripMenuItem.Click += new System.EventHandler(this.тестоваяФормочкаToolStripMenuItem_Click);
            // 
            // button_start
            // 
            this.button_start.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button_start.ForeColor = System.Drawing.Color.Green;
            this.button_start.Location = new System.Drawing.Point(12, 40);
            this.button_start.Name = "button_start";
            this.button_start.Size = new System.Drawing.Size(91, 41);
            this.button_start.TabIndex = 4;
            this.button_start.Text = "СТАРТ";
            this.button_start.UseVisualStyleBackColor = true;
            this.button_start.Click += new System.EventHandler(this.ButtonStartClick);
            // 
            // button_stop
            // 
            this.button_stop.Enabled = false;
            this.button_stop.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button_stop.ForeColor = System.Drawing.Color.Red;
            this.button_stop.Location = new System.Drawing.Point(12, 87);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(91, 41);
            this.button_stop.TabIndex = 5;
            this.button_stop.Text = "СТОП";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.ButtonStopClick);
            // 
            // label_onlineTrackers
            // 
            this.label_onlineTrackers.AutoSize = true;
            this.label_onlineTrackers.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label_onlineTrackers.Location = new System.Drawing.Point(176, 52);
            this.label_onlineTrackers.Name = "label_onlineTrackers";
            this.label_onlineTrackers.Size = new System.Drawing.Size(216, 16);
            this.label_onlineTrackers.TabIndex = 6;
            this.label_onlineTrackers.Text = "Подключенных треккеров: 0";
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button1.ForeColor = System.Drawing.Color.Red;
            this.button1.Location = new System.Drawing.Point(12, 190);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(91, 41);
            this.button1.TabIndex = 7;
            this.button1.Text = "Start save thread";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1Click);
            // 
            // button_createAdminUser
            // 
            this.button_createAdminUser.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button_createAdminUser.Location = new System.Drawing.Point(329, 190);
            this.button_createAdminUser.Name = "button_createAdminUser";
            this.button_createAdminUser.Size = new System.Drawing.Size(126, 41);
            this.button_createAdminUser.TabIndex = 8;
            this.button_createAdminUser.Text = "Create admin user";
            this.button_createAdminUser.UseVisualStyleBackColor = true;
            this.button_createAdminUser.Click += new System.EventHandler(this.ButtonCreateAdminUserClick);
            // 
            // textBox_password
            // 
            this.textBox_password.Location = new System.Drawing.Point(329, 168);
            this.textBox_password.Name = "textBox_password";
            this.textBox_password.Size = new System.Drawing.Size(126, 20);
            this.textBox_password.TabIndex = 9;
            // 
            // label_ReadMemcachedValue
            // 
            this.label_ReadMemcachedValue.AutoSize = true;
            this.label_ReadMemcachedValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label_ReadMemcachedValue.Location = new System.Drawing.Point(533, 202);
            this.label_ReadMemcachedValue.Name = "label_ReadMemcachedValue";
            this.label_ReadMemcachedValue.Size = new System.Drawing.Size(216, 16);
            this.label_ReadMemcachedValue.TabIndex = 10;
            this.label_ReadMemcachedValue.Text = "Подключенных треккеров: 0";
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button2.ForeColor = System.Drawing.Color.Red;
            this.button2.Location = new System.Drawing.Point(561, 40);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(91, 41);
            this.button2.TabIndex = 11;
            this.button2.Text = "Вкл реле 1";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.Button2Click);
            // 
            // button3
            // 
            this.button3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button3.ForeColor = System.Drawing.Color.Red;
            this.button3.Location = new System.Drawing.Point(658, 40);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(91, 41);
            this.button3.TabIndex = 12;
            this.button3.Text = "Выкл реле 1";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.Button3Click);
            // 
            // button4
            // 
            this.button4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button4.ForeColor = System.Drawing.Color.Red;
            this.button4.Location = new System.Drawing.Point(658, 87);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(91, 41);
            this.button4.TabIndex = 14;
            this.button4.Text = "Выкл реле 2";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.Button4Click);
            // 
            // button5
            // 
            this.button5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button5.ForeColor = System.Drawing.Color.Red;
            this.button5.Location = new System.Drawing.Point(561, 87);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(91, 41);
            this.button5.TabIndex = 13;
            this.button5.Text = "Вкл реле 2";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.Button5Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(807, 270);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label_ReadMemcachedValue);
            this.Controls.Add(this.textBox_password);
            this.Controls.Add(this.button_createAdminUser);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label_onlineTrackers);
            this.Controls.Add(this.button_stop);
            this.Controls.Add(this.button_start);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Tracker Server";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.Button button_start;
        private System.Windows.Forms.Button button_stop;
        private System.Windows.Forms.Label label_onlineTrackers;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button_createAdminUser;
        private System.Windows.Forms.TextBox textBox_password;
        private System.Windows.Forms.ToolStripMenuItem тестоваяФормочкаToolStripMenuItem;
        private System.Windows.Forms.Label label_ReadMemcachedValue;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
    }
}