namespace FG.Utils.YSYard.Translations
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            MainWebView = new Microsoft.AspNetCore.Components.WebView.WindowsForms.BlazorWebView();
            MainFolderPicker = new FolderBrowserDialog();
            MainSaveFilePicker = new SaveFileDialog();
            MainOpenFilePicker = new OpenFileDialog();
            SuspendLayout();
            // 
            // MainWebView
            // 
            MainWebView.Dock = DockStyle.Fill;
            MainWebView.Location = new Point(0, 0);
            MainWebView.Name = "MainWebView";
            MainWebView.Size = new Size(1350, 729);
            MainWebView.StartPath = "/";
            MainWebView.TabIndex = 0;
            MainWebView.Text = "blazorWebView1";
            // 
            // MainOpenFilePicker
            // 
            MainOpenFilePicker.FileName = "openFileDialog1";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1350, 729);
            Controls.Add(MainWebView);
            MinimumSize = new Size(1366, 768);
            Name = "MainForm";
            Text = "Yog-Sothoth's Yard 翻訳アシスタント";
            ResumeLayout(false);
        }

        #endregion

        private Microsoft.AspNetCore.Components.WebView.WindowsForms.BlazorWebView MainWebView;
        private FolderBrowserDialog MainFolderPicker;
		private SaveFileDialog MainSaveFilePicker;
		private OpenFileDialog MainOpenFilePicker;
	}
}
