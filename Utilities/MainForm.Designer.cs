﻿namespace Utilities {
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
            this.button_recreateUsers = new System.Windows.Forms.Button();
            this.button_createIncrements = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button_recreateUsers
            // 
            this.button_recreateUsers.Location = new System.Drawing.Point(12, 12);
            this.button_recreateUsers.Name = "button_recreateUsers";
            this.button_recreateUsers.Size = new System.Drawing.Size(171, 23);
            this.button_recreateUsers.TabIndex = 0;
            this.button_recreateUsers.Text = "Пересоздать пользователей";
            this.button_recreateUsers.UseVisualStyleBackColor = true;
            this.button_recreateUsers.Click += new System.EventHandler(this.ButtonRecreateUsersClick);
            // 
            // button_createIncrements
            // 
            this.button_createIncrements.Location = new System.Drawing.Point(12, 41);
            this.button_createIncrements.Name = "button_createIncrements";
            this.button_createIncrements.Size = new System.Drawing.Size(171, 23);
            this.button_createIncrements.TabIndex = 1;
            this.button_createIncrements.Text = "Создать Increments";
            this.button_createIncrements.UseVisualStyleBackColor = true;
            this.button_createIncrements.Click += new System.EventHandler(this.ButtonCreateIncrementsClick);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(3, 119);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(171, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Увеличить Tracker Increments";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 103);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "label1";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(12, 185);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(171, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "Connect to service";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(307, 222);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(171, 23);
            this.button3.TabIndex = 5;
            this.button3.Text = "Connect to service";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(307, 29);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(171, 23);
            this.button4.TabIndex = 6;
            this.button4.Text = "check online";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(490, 266);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.button_createIncrements);
            this.Controls.Add(this.button_recreateUsers);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "Утилиты";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_recreateUsers;
        private System.Windows.Forms.Button button_createIncrements;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
    }
}

