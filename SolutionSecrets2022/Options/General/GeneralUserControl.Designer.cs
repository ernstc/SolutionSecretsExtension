
namespace SolutionSecrets2022.Options.General
{
	partial class GeneralUserControl
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
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            this.cboxRepositoryType = new System.Windows.Forms.ComboBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(3, 3);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(181, 25);
            label1.TabIndex = 0;
            label1.Text = "Default repository";
            // 
            // label2
            // 
            label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            label2.Location = new System.Drawing.Point(3, 104);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(748, 62);
            label2.TabIndex = 1;
            label2.Text = "Determines the default cloud repository to use for solutions without custom setti" +
    "ngs.";
            // 
            // cboxRepositoryType
            // 
            this.cboxRepositoryType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxRepositoryType.FormattingEnabled = true;
            this.cboxRepositoryType.Items.AddRange(new object[] {
            "GitHub Gists",
            "Azure Key Vault"});
            this.cboxRepositoryType.Location = new System.Drawing.Point(8, 48);
            this.cboxRepositoryType.Name = "cboxRepositoryType";
            this.cboxRepositoryType.Size = new System.Drawing.Size(280, 33);
            this.cboxRepositoryType.TabIndex = 2;
            this.cboxRepositoryType.SelectedIndexChanged += new System.EventHandler(this.cboxRepositoryType_SelectedIndexChanged);
            // 
            // GeneralUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cboxRepositoryType);
            this.Controls.Add(label2);
            this.Controls.Add(label1);
            this.Name = "GeneralUserControl";
            this.Size = new System.Drawing.Size(754, 480);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ComboBox cboxRepositoryType;
	}
}
