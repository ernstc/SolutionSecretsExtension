
namespace SolutionSecrets2019.Options.GitHubGists
{
	partial class GitHubGistsUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.Windows.Forms.GroupBox groupBox1;
            System.Windows.Forms.Label label9;
            System.Windows.Forms.Label label8;
            System.Windows.Forms.Label label7;
            System.Windows.Forms.Label label5;
            System.Windows.Forms.GroupBox grpEnctyptionKey;
            System.Windows.Forms.RadioButton rbtnPassphrase;
            System.Windows.Forms.RadioButton rbtnKeyFile;
            System.Windows.Forms.Label label4;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GitHubGistsUserControl));
            this.pnlContinueAuthorization = new System.Windows.Forms.Panel();
            this.btnUndoCompleteAuthorization = new System.Windows.Forms.Button();
            this.btnContinueAuthorization = new System.Windows.Forms.Button();
            this.txtDeviceCode = new System.Windows.Forms.TextBox();
            this.btnResetAuthorization = new System.Windows.Forms.Button();
            this.btnAuthorize = new System.Windows.Forms.Button();
            this.lblAuthorizationStatus = new System.Windows.Forms.Label();
            this.pnlCreateEncryptionKey = new System.Windows.Forms.Panel();
            this.btnUndoGenerateKey = new System.Windows.Forms.Button();
            this.btnGenerateKey = new System.Windows.Forms.Button();
            this.btnBrowseKeyFile = new System.Windows.Forms.Button();
            this.txtKeyFilePath = new System.Windows.Forms.TextBox();
            this.txtConfirmPassphrase = new System.Windows.Forms.TextBox();
            this.txtPassphrase = new System.Windows.Forms.TextBox();
            this.btnChangeKey = new System.Windows.Forms.Button();
            this.lblEncryptionKeyStatus = new System.Windows.Forms.Label();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label9 = new System.Windows.Forms.Label();
            label8 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            grpEnctyptionKey = new System.Windows.Forms.GroupBox();
            rbtnPassphrase = new System.Windows.Forms.RadioButton();
            rbtnKeyFile = new System.Windows.Forms.RadioButton();
            label4 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            groupBox1.SuspendLayout();
            this.pnlContinueAuthorization.SuspendLayout();
            grpEnctyptionKey.SuspendLayout();
            this.pnlCreateEncryptionKey.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox1.Controls.Add(this.pnlContinueAuthorization);
            groupBox1.Controls.Add(this.btnResetAuthorization);
            groupBox1.Controls.Add(this.btnAuthorize);
            groupBox1.Controls.Add(this.lblAuthorizationStatus);
            groupBox1.Controls.Add(label7);
            groupBox1.Controls.Add(label5);
            groupBox1.Location = new System.Drawing.Point(3, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(714, 252);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "GitHub Authorization";
            // 
            // pnlContinueAuthorization
            // 
            this.pnlContinueAuthorization.Controls.Add(this.btnUndoCompleteAuthorization);
            this.pnlContinueAuthorization.Controls.Add(this.btnContinueAuthorization);
            this.pnlContinueAuthorization.Controls.Add(this.txtDeviceCode);
            this.pnlContinueAuthorization.Controls.Add(label9);
            this.pnlContinueAuthorization.Controls.Add(label8);
            this.pnlContinueAuthorization.Location = new System.Drawing.Point(1, 110);
            this.pnlContinueAuthorization.Name = "pnlContinueAuthorization";
            this.pnlContinueAuthorization.Size = new System.Drawing.Size(702, 132);
            this.pnlContinueAuthorization.TabIndex = 7;
            this.pnlContinueAuthorization.Visible = false;
            // 
            // btnUndoCompleteAuthorization
            // 
            this.btnUndoCompleteAuthorization.Location = new System.Drawing.Point(489, 71);
            this.btnUndoCompleteAuthorization.Name = "btnUndoCompleteAuthorization";
            this.btnUndoCompleteAuthorization.Size = new System.Drawing.Size(180, 40);
            this.btnUndoCompleteAuthorization.TabIndex = 4;
            this.btnUndoCompleteAuthorization.Text = "Undo";
            this.btnUndoCompleteAuthorization.UseVisualStyleBackColor = true;
            this.btnUndoCompleteAuthorization.Click += new System.EventHandler(this.btnUndoCompleteAuthorization_Click);
            // 
            // btnContinueAuthorization
            // 
            this.btnContinueAuthorization.Location = new System.Drawing.Point(303, 71);
            this.btnContinueAuthorization.Name = "btnContinueAuthorization";
            this.btnContinueAuthorization.Size = new System.Drawing.Size(180, 40);
            this.btnContinueAuthorization.TabIndex = 3;
            this.btnContinueAuthorization.Text = "Continue...";
            this.btnContinueAuthorization.UseVisualStyleBackColor = true;
            this.btnContinueAuthorization.Click += new System.EventHandler(this.btnContinueAuthorization_Click);
            // 
            // txtDeviceCode
            // 
            this.txtDeviceCode.Location = new System.Drawing.Point(149, 76);
            this.txtDeviceCode.Name = "txtDeviceCode";
            this.txtDeviceCode.Size = new System.Drawing.Size(127, 31);
            this.txtDeviceCode.TabIndex = 2;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(6, 79);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(137, 25);
            label9.TabIndex = 1;
            label9.Text = "Device code:";
            // 
            // label8
            // 
            label8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            label8.Location = new System.Drawing.Point(6, 0);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(696, 73);
            label8.TabIndex = 0;
            label8.Text = "Copy the device code and click on the \"Continue...\" button for proceeding with th" +
    "e authorization process.";
            // 
            // btnResetAuthorization
            // 
            this.btnResetAuthorization.Location = new System.Drawing.Point(15, 157);
            this.btnResetAuthorization.Name = "btnResetAuthorization";
            this.btnResetAuthorization.Size = new System.Drawing.Size(200, 40);
            this.btnResetAuthorization.TabIndex = 8;
            this.btnResetAuthorization.Text = "Reset";
            this.btnResetAuthorization.UseVisualStyleBackColor = true;
            this.btnResetAuthorization.Visible = false;
            this.btnResetAuthorization.Click += new System.EventHandler(this.btnResetAuthorization_Click);
            // 
            // btnAuthorize
            // 
            this.btnAuthorize.Location = new System.Drawing.Point(12, 157);
            this.btnAuthorize.Name = "btnAuthorize";
            this.btnAuthorize.Size = new System.Drawing.Size(200, 40);
            this.btnAuthorize.TabIndex = 0;
            this.btnAuthorize.Text = "Authorize...";
            this.btnAuthorize.UseVisualStyleBackColor = true;
            this.btnAuthorize.Visible = false;
            this.btnAuthorize.Click += new System.EventHandler(this.btnAuthorize_Click);
            // 
            // lblAuthorizationStatus
            // 
            this.lblAuthorizationStatus.AutoSize = true;
            this.lblAuthorizationStatus.BackColor = System.Drawing.Color.White;
            this.lblAuthorizationStatus.Location = new System.Drawing.Point(92, 110);
            this.lblAuthorizationStatus.Name = "lblAuthorizationStatus";
            this.lblAuthorizationStatus.Padding = new System.Windows.Forms.Padding(4);
            this.lblAuthorizationStatus.Size = new System.Drawing.Size(115, 33);
            this.lblAuthorizationStatus.TabIndex = 6;
            this.lblAuthorizationStatus.Text = "Loading...";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(7, 114);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(79, 25);
            label7.TabIndex = 5;
            label7.Text = "Status:";
            // 
            // label5
            // 
            label5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            label5.Location = new System.Drawing.Point(7, 40);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(696, 77);
            label5.TabIndex = 0;
            label5.Text = "GitHub Gists is used as the repository for your encrypted secrets. Only secret gi" +
    "sts will be created.";
            // 
            // grpEnctyptionKey
            // 
            grpEnctyptionKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            grpEnctyptionKey.Controls.Add(this.pnlCreateEncryptionKey);
            grpEnctyptionKey.Controls.Add(this.btnChangeKey);
            grpEnctyptionKey.Controls.Add(this.lblEncryptionKeyStatus);
            grpEnctyptionKey.Controls.Add(label2);
            grpEnctyptionKey.Controls.Add(label1);
            grpEnctyptionKey.Location = new System.Drawing.Point(3, 261);
            grpEnctyptionKey.Name = "grpEnctyptionKey";
            grpEnctyptionKey.Size = new System.Drawing.Size(714, 371);
            grpEnctyptionKey.TabIndex = 1;
            grpEnctyptionKey.TabStop = false;
            grpEnctyptionKey.Text = "Encryption Key";
            // 
            // pnlCreateEncryptionKey
            // 
            this.pnlCreateEncryptionKey.Controls.Add(this.btnUndoGenerateKey);
            this.pnlCreateEncryptionKey.Controls.Add(this.btnGenerateKey);
            this.pnlCreateEncryptionKey.Controls.Add(rbtnPassphrase);
            this.pnlCreateEncryptionKey.Controls.Add(this.btnBrowseKeyFile);
            this.pnlCreateEncryptionKey.Controls.Add(rbtnKeyFile);
            this.pnlCreateEncryptionKey.Controls.Add(this.txtKeyFilePath);
            this.pnlCreateEncryptionKey.Controls.Add(label4);
            this.pnlCreateEncryptionKey.Controls.Add(this.txtConfirmPassphrase);
            this.pnlCreateEncryptionKey.Controls.Add(this.txtPassphrase);
            this.pnlCreateEncryptionKey.Location = new System.Drawing.Point(12, 153);
            this.pnlCreateEncryptionKey.Name = "pnlCreateEncryptionKey";
            this.pnlCreateEncryptionKey.Size = new System.Drawing.Size(691, 207);
            this.pnlCreateEncryptionKey.TabIndex = 11;
            this.pnlCreateEncryptionKey.Visible = false;
            // 
            // btnUndoGenerateKey
            // 
            this.btnUndoGenerateKey.Location = new System.Drawing.Point(353, 152);
            this.btnUndoGenerateKey.Name = "btnUndoGenerateKey";
            this.btnUndoGenerateKey.Size = new System.Drawing.Size(180, 40);
            this.btnUndoGenerateKey.TabIndex = 12;
            this.btnUndoGenerateKey.Text = "Undo";
            this.btnUndoGenerateKey.UseVisualStyleBackColor = true;
            this.btnUndoGenerateKey.Click += new System.EventHandler(this.btnUndoGenerateKey_Click);
            // 
            // btnGenerateKey
            // 
            this.btnGenerateKey.Location = new System.Drawing.Point(167, 152);
            this.btnGenerateKey.Name = "btnGenerateKey";
            this.btnGenerateKey.Size = new System.Drawing.Size(180, 40);
            this.btnGenerateKey.TabIndex = 11;
            this.btnGenerateKey.Text = "Generate key";
            this.btnGenerateKey.UseVisualStyleBackColor = true;
            this.btnGenerateKey.Click += new System.EventHandler(this.btnGenerateKey_Click);
            // 
            // rbtnPassphrase
            // 
            rbtnPassphrase.AutoSize = true;
            rbtnPassphrase.Checked = true;
            rbtnPassphrase.Location = new System.Drawing.Point(3, 3);
            rbtnPassphrase.Name = "rbtnPassphrase";
            rbtnPassphrase.Size = new System.Drawing.Size(157, 29);
            rbtnPassphrase.TabIndex = 4;
            rbtnPassphrase.TabStop = true;
            rbtnPassphrase.Text = "Passphrase";
            rbtnPassphrase.UseVisualStyleBackColor = false;
            rbtnPassphrase.CheckedChanged += new System.EventHandler(this.rbtnPassphrase_CheckedChanged);
            // 
            // btnBrowseKeyFile
            // 
            this.btnBrowseKeyFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseKeyFile.Enabled = false;
            this.btnBrowseKeyFile.Location = new System.Drawing.Point(577, 96);
            this.btnBrowseKeyFile.Name = "btnBrowseKeyFile";
            this.btnBrowseKeyFile.Size = new System.Drawing.Size(114, 31);
            this.btnBrowseKeyFile.TabIndex = 10;
            this.btnBrowseKeyFile.Text = "Browse...";
            this.btnBrowseKeyFile.UseVisualStyleBackColor = true;
            this.btnBrowseKeyFile.Click += new System.EventHandler(this.btnBrowseKeyFile_Click);
            // 
            // rbtnKeyFile
            // 
            rbtnKeyFile.AutoSize = true;
            rbtnKeyFile.Location = new System.Drawing.Point(3, 98);
            rbtnKeyFile.Name = "rbtnKeyFile";
            rbtnKeyFile.Size = new System.Drawing.Size(114, 29);
            rbtnKeyFile.TabIndex = 5;
            rbtnKeyFile.Text = "Key file";
            rbtnKeyFile.UseVisualStyleBackColor = true;
            rbtnKeyFile.CheckedChanged += new System.EventHandler(this.rbtnKeyFile_CheckedChanged);
            // 
            // txtKeyFilePath
            // 
            this.txtKeyFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtKeyFilePath.Enabled = false;
            this.txtKeyFilePath.Location = new System.Drawing.Point(167, 95);
            this.txtKeyFilePath.Name = "txtKeyFilePath";
            this.txtKeyFilePath.Size = new System.Drawing.Size(404, 31);
            this.txtKeyFilePath.TabIndex = 9;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(34, 44);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(86, 25);
            label4.TabIndex = 6;
            label4.Text = "Confirm";
            // 
            // txtConfirmPassphrase
            // 
            this.txtConfirmPassphrase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtConfirmPassphrase.Location = new System.Drawing.Point(167, 38);
            this.txtConfirmPassphrase.Name = "txtConfirmPassphrase";
            this.txtConfirmPassphrase.PasswordChar = '*';
            this.txtConfirmPassphrase.Size = new System.Drawing.Size(524, 31);
            this.txtConfirmPassphrase.TabIndex = 8;
            // 
            // txtPassphrase
            // 
            this.txtPassphrase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPassphrase.Location = new System.Drawing.Point(167, 0);
            this.txtPassphrase.Name = "txtPassphrase";
            this.txtPassphrase.PasswordChar = '*';
            this.txtPassphrase.Size = new System.Drawing.Size(524, 31);
            this.txtPassphrase.TabIndex = 7;
            // 
            // btnChangeKey
            // 
            this.btnChangeKey.Location = new System.Drawing.Point(12, 195);
            this.btnChangeKey.Name = "btnChangeKey";
            this.btnChangeKey.Size = new System.Drawing.Size(200, 40);
            this.btnChangeKey.TabIndex = 3;
            this.btnChangeKey.Text = "Change key...";
            this.btnChangeKey.UseVisualStyleBackColor = true;
            this.btnChangeKey.Click += new System.EventHandler(this.btnChangeKey_Click);
            // 
            // lblEncryptionKeyStatus
            // 
            this.lblEncryptionKeyStatus.AutoSize = true;
            this.lblEncryptionKeyStatus.BackColor = System.Drawing.Color.White;
            this.lblEncryptionKeyStatus.Location = new System.Drawing.Point(92, 153);
            this.lblEncryptionKeyStatus.Name = "lblEncryptionKeyStatus";
            this.lblEncryptionKeyStatus.Padding = new System.Windows.Forms.Padding(4);
            this.lblEncryptionKeyStatus.Size = new System.Drawing.Size(115, 33);
            this.lblEncryptionKeyStatus.TabIndex = 2;
            this.lblEncryptionKeyStatus.Text = "Loading...";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(7, 157);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(79, 25);
            label2.TabIndex = 1;
            label2.Text = "Status:";
            // 
            // label1
            // 
            label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            label1.Location = new System.Drawing.Point(7, 40);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(701, 107);
            label1.TabIndex = 0;
            label1.Text = resources.GetString("label1.Text");
            // 
            // GitHubGistsUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(grpEnctyptionKey);
            this.Controls.Add(groupBox1);
            this.MinimumSize = new System.Drawing.Size(720, 0);
            this.Name = "GitHubGistsUserControl";
            this.Size = new System.Drawing.Size(720, 633);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            this.pnlContinueAuthorization.ResumeLayout(false);
            this.pnlContinueAuthorization.PerformLayout();
            grpEnctyptionKey.ResumeLayout(false);
            grpEnctyptionKey.PerformLayout();
            this.pnlCreateEncryptionKey.ResumeLayout(false);
            this.pnlCreateEncryptionKey.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.Label lblEncryptionKeyStatus;
		private System.Windows.Forms.Button btnChangeKey;
		private System.Windows.Forms.TextBox txtConfirmPassphrase;
		private System.Windows.Forms.TextBox txtPassphrase;
		private System.Windows.Forms.Button btnBrowseKeyFile;
		private System.Windows.Forms.TextBox txtKeyFilePath;
		private System.Windows.Forms.Label lblAuthorizationStatus;
		private System.Windows.Forms.Button btnAuthorize;
		private System.Windows.Forms.Panel pnlContinueAuthorization;
		private System.Windows.Forms.TextBox txtDeviceCode;
		private System.Windows.Forms.Button btnContinueAuthorization;
		private System.Windows.Forms.Panel pnlCreateEncryptionKey;
		private System.Windows.Forms.Button btnUndoGenerateKey;
		private System.Windows.Forms.Button btnGenerateKey;
		private System.Windows.Forms.Button btnUndoCompleteAuthorization;
		private System.Windows.Forms.Button btnResetAuthorization;
	}
}
