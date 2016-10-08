namespace ClientDiProva
{
    partial class Form1
    {
        /// <summary>
        /// Variabile di progettazione necessaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Pulire le risorse in uso.
        /// </summary>
        /// <param name="disposing">ha valore true se le risorse gestite devono essere eliminate, false in caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Codice generato da Progettazione Windows Form

        /// <summary>
        /// Metodo necessario per il supporto della finestra di progettazione. Non modificare
        /// il contenuto del metodo con l'editor di codice.
        /// </summary>
        private void InitializeComponent()
        {
            this.connectButton = new System.Windows.Forms.Button();
            this.consoleBox = new System.Windows.Forms.TextBox();
            this.loginRequest = new System.Windows.Forms.Button();
            this.browse = new System.Windows.Forms.Button();
            this.sendFile = new System.Windows.Forms.Button();
            this.filenameLabel = new System.Windows.Forms.Label();
            this.fileRequest = new System.Windows.Forms.Button();
            this.SynchronizeButton = new System.Windows.Forms.Button();
            this.registerRequest = new System.Windows.Forms.Button();
            this.chooseDir = new System.Windows.Forms.Button();
            this.dirLabel = new System.Windows.Forms.Label();
            this.addNewVersion = new System.Windows.Forms.Button();
            this.restore = new System.Windows.Forms.Button();
            this.disconnectButton = new System.Windows.Forms.Button();
            this.testButton = new System.Windows.Forms.Button();
            this.checkVersion = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(17, -3);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(99, 30);
            this.connectButton.TabIndex = 0;
            this.connectButton.Text = "Connetti";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // consoleBox
            // 
            this.consoleBox.Location = new System.Drawing.Point(26, 158);
            this.consoleBox.Multiline = true;
            this.consoleBox.Name = "consoleBox";
            this.consoleBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.consoleBox.Size = new System.Drawing.Size(427, 152);
            this.consoleBox.TabIndex = 1;
            // 
            // loginRequest
            // 
            this.loginRequest.Location = new System.Drawing.Point(18, 60);
            this.loginRequest.Name = "loginRequest";
            this.loginRequest.Size = new System.Drawing.Size(98, 37);
            this.loginRequest.TabIndex = 2;
            this.loginRequest.Text = "LoginRequest";
            this.loginRequest.UseVisualStyleBackColor = true;
            this.loginRequest.Click += new System.EventHandler(this.loginRequest_Click);
            // 
            // browse
            // 
            this.browse.Location = new System.Drawing.Point(328, 32);
            this.browse.Name = "browse";
            this.browse.Size = new System.Drawing.Size(86, 25);
            this.browse.TabIndex = 3;
            this.browse.Text = "Browse";
            this.browse.UseVisualStyleBackColor = true;
            this.browse.Click += new System.EventHandler(this.browse_Click);
            // 
            // sendFile
            // 
            this.sendFile.Location = new System.Drawing.Point(328, 120);
            this.sendFile.Name = "sendFile";
            this.sendFile.Size = new System.Drawing.Size(85, 35);
            this.sendFile.TabIndex = 4;
            this.sendFile.Text = "Send File";
            this.sendFile.UseVisualStyleBackColor = true;
            this.sendFile.Click += new System.EventHandler(this.sendFile_Click);
            // 
            // filenameLabel
            // 
            this.filenameLabel.AutoSize = true;
            this.filenameLabel.Location = new System.Drawing.Point(325, 60);
            this.filenameLabel.Name = "filenameLabel";
            this.filenameLabel.Size = new System.Drawing.Size(72, 13);
            this.filenameLabel.TabIndex = 5;
            this.filenameLabel.Text = "filenameLabel";
            // 
            // fileRequest
            // 
            this.fileRequest.Location = new System.Drawing.Point(328, -3);
            this.fileRequest.Name = "fileRequest";
            this.fileRequest.Size = new System.Drawing.Size(81, 29);
            this.fileRequest.TabIndex = 6;
            this.fileRequest.Text = "FileRequest";
            this.fileRequest.UseVisualStyleBackColor = true;
            // 
            // SynchronizeButton
            // 
            this.SynchronizeButton.Location = new System.Drawing.Point(328, 85);
            this.SynchronizeButton.Name = "SynchronizeButton";
            this.SynchronizeButton.Size = new System.Drawing.Size(80, 29);
            this.SynchronizeButton.TabIndex = 7;
            this.SynchronizeButton.Text = "Synchronize";
            this.SynchronizeButton.UseVisualStyleBackColor = true;
            this.SynchronizeButton.Click += new System.EventHandler(this.synchronizeButton_Click);
            // 
            // registerRequest
            // 
            this.registerRequest.Location = new System.Drawing.Point(18, 108);
            this.registerRequest.Name = "registerRequest";
            this.registerRequest.Size = new System.Drawing.Size(104, 26);
            this.registerRequest.TabIndex = 8;
            this.registerRequest.Text = "RegisterRequest";
            this.registerRequest.UseVisualStyleBackColor = true;
            this.registerRequest.Click += new System.EventHandler(this.registerRequest_Click);
            // 
            // chooseDir
            // 
            this.chooseDir.Location = new System.Drawing.Point(171, 12);
            this.chooseDir.Name = "chooseDir";
            this.chooseDir.Size = new System.Drawing.Size(75, 31);
            this.chooseDir.TabIndex = 9;
            this.chooseDir.Text = "ChooseDir";
            this.chooseDir.UseVisualStyleBackColor = true;
            this.chooseDir.Click += new System.EventHandler(this.chooseDir_Click);
            // 
            // dirLabel
            // 
            this.dirLabel.AutoSize = true;
            this.dirLabel.Location = new System.Drawing.Point(177, 46);
            this.dirLabel.Name = "dirLabel";
            this.dirLabel.Size = new System.Drawing.Size(44, 13);
            this.dirLabel.TabIndex = 10;
            this.dirLabel.Text = "dirLabel";
            // 
            // addNewVersion
            // 
            this.addNewVersion.Location = new System.Drawing.Point(171, 65);
            this.addNewVersion.Name = "addNewVersion";
            this.addNewVersion.Size = new System.Drawing.Size(97, 26);
            this.addNewVersion.TabIndex = 11;
            this.addNewVersion.Text = "addNewVersion";
            this.addNewVersion.UseVisualStyleBackColor = true;
            this.addNewVersion.Click += new System.EventHandler(this.addNewVersion_Click);
            // 
            // restore
            // 
            this.restore.Location = new System.Drawing.Point(173, 97);
            this.restore.Name = "restore";
            this.restore.Size = new System.Drawing.Size(73, 23);
            this.restore.TabIndex = 12;
            this.restore.Text = "Restore";
            this.restore.UseVisualStyleBackColor = true;
            this.restore.Click += new System.EventHandler(this.restore_Click);
            // 
            // disconnectButton
            // 
            this.disconnectButton.Location = new System.Drawing.Point(18, 29);
            this.disconnectButton.Name = "disconnectButton";
            this.disconnectButton.Size = new System.Drawing.Size(98, 30);
            this.disconnectButton.TabIndex = 13;
            this.disconnectButton.Text = "Disconnetti";
            this.disconnectButton.UseVisualStyleBackColor = true;
            this.disconnectButton.Click += new System.EventHandler(this.disconnectButton_Click);
            // 
            // testButton
            // 
            this.testButton.Location = new System.Drawing.Point(431, 73);
            this.testButton.Name = "testButton";
            this.testButton.Size = new System.Drawing.Size(46, 24);
            this.testButton.TabIndex = 14;
            this.testButton.Text = "TEST";
            this.testButton.UseVisualStyleBackColor = true;
            this.testButton.Click += new System.EventHandler(this.testButton_Click);
            // 
            // checkVersion
            // 
            this.checkVersion.Location = new System.Drawing.Point(144, 127);
            this.checkVersion.Name = "checkVersion";
            this.checkVersion.Size = new System.Drawing.Size(178, 23);
            this.checkVersion.TabIndex = 15;
            this.checkVersion.Text = "checkIfCurrentVersionIsUpdated";
            this.checkVersion.UseVisualStyleBackColor = true;
            this.checkVersion.Click += new System.EventHandler(this.checkVersion_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(482, 351);
            this.Controls.Add(this.checkVersion);
            this.Controls.Add(this.testButton);
            this.Controls.Add(this.disconnectButton);
            this.Controls.Add(this.restore);
            this.Controls.Add(this.addNewVersion);
            this.Controls.Add(this.dirLabel);
            this.Controls.Add(this.chooseDir);
            this.Controls.Add(this.registerRequest);
            this.Controls.Add(this.SynchronizeButton);
            this.Controls.Add(this.fileRequest);
            this.Controls.Add(this.filenameLabel);
            this.Controls.Add(this.sendFile);
            this.Controls.Add(this.browse);
            this.Controls.Add(this.loginRequest);
            this.Controls.Add(this.consoleBox);
            this.Controls.Add(this.connectButton);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.TextBox consoleBox;
        private System.Windows.Forms.Button loginRequest;
        private System.Windows.Forms.Button browse;
        private System.Windows.Forms.Button sendFile;
        private System.Windows.Forms.Label filenameLabel;
        private System.Windows.Forms.Button fileRequest;
        private System.Windows.Forms.Button SynchronizeButton;
        private System.Windows.Forms.Button registerRequest;
        private System.Windows.Forms.Button chooseDir;
        private System.Windows.Forms.Label dirLabel;
        private System.Windows.Forms.Button addNewVersion;
        private System.Windows.Forms.Button restore;
        private System.Windows.Forms.Button disconnectButton;
        private System.Windows.Forms.Button testButton;
        private System.Windows.Forms.Button checkVersion;
    }
}

