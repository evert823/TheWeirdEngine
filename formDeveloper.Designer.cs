namespace TheWeirdEngine
{
    partial class formDeveloper
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
            this.btnRegressiontestscript = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnRegressiontestscript
            // 
            this.btnRegressiontestscript.Location = new System.Drawing.Point(412, 136);
            this.btnRegressiontestscript.Name = "btnRegressiontestscript";
            this.btnRegressiontestscript.Size = new System.Drawing.Size(192, 32);
            this.btnRegressiontestscript.TabIndex = 0;
            this.btnRegressiontestscript.Text = "Regression test script";
            this.btnRegressiontestscript.UseVisualStyleBackColor = true;
            this.btnRegressiontestscript.Click += new System.EventHandler(this.btnRegressiontestscript_Click);
            // 
            // formDeveloper
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnRegressiontestscript);
            this.Name = "formDeveloper";
            this.Text = "Developer options";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnRegressiontestscript;
    }
}