namespace TheWeirdEngine
{
    partial class formWeirdEngineMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formWeirdEngineMain));
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblInformation = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveBoardAsImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showResourcesLocationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeFromJsonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.engineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.suggestMoveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadPositionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testNewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unittestsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.positionGeneratorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scenario1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manyNonTrivialToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.txtbEnterMove = new System.Windows.Forms.TextBox();
            this.lblUserMove = new System.Windows.Forms.Label();
            this.lblStatusMessage = new System.Windows.Forms.Label();
            this.btnAbort = new System.Windows.Forms.Button();
            this.txtbOtherValues = new System.Windows.Forms.TextBox();
            this.lblOtherValues = new System.Windows.Forms.Label();
            this.newUnittestsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showLegalMovesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(690, 76);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(435, 262);
            this.pictureBox1.TabIndex = 7;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            // 
            // lblInformation
            // 
            this.lblInformation.AutoSize = true;
            this.lblInformation.Location = new System.Drawing.Point(12, 154);
            this.lblInformation.Name = "lblInformation";
            this.lblInformation.Size = new System.Drawing.Size(123, 16);
            this.lblInformation.TabIndex = 9;
            this.lblInformation.Text = "Position information";
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.engineToolStripMenuItem,
            this.editToolStripMenuItem,
            this.testNewToolStripMenuItem,
            this.positionGeneratorToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1275, 28);
            this.menuStrip1.TabIndex = 14;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveBoardAsImageToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // saveBoardAsImageToolStripMenuItem
            // 
            this.saveBoardAsImageToolStripMenuItem.Name = "saveBoardAsImageToolStripMenuItem";
            this.saveBoardAsImageToolStripMenuItem.Size = new System.Drawing.Size(231, 26);
            this.saveBoardAsImageToolStripMenuItem.Text = "Save board as image";
            this.saveBoardAsImageToolStripMenuItem.Click += new System.EventHandler(this.saveBoardAsImageToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showResourcesLocationToolStripMenuItem,
            this.changeFromJsonToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(76, 24);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // showResourcesLocationToolStripMenuItem
            // 
            this.showResourcesLocationToolStripMenuItem.Name = "showResourcesLocationToolStripMenuItem";
            this.showResourcesLocationToolStripMenuItem.Size = new System.Drawing.Size(256, 26);
            this.showResourcesLocationToolStripMenuItem.Text = "Show Resources location";
            this.showResourcesLocationToolStripMenuItem.Click += new System.EventHandler(this.showResourcesLocationToolStripMenuItem_Click);
            // 
            // changeFromJsonToolStripMenuItem
            // 
            this.changeFromJsonToolStripMenuItem.Name = "changeFromJsonToolStripMenuItem";
            this.changeFromJsonToolStripMenuItem.Size = new System.Drawing.Size(256, 26);
            this.changeFromJsonToolStripMenuItem.Text = "Change from Json";
            this.changeFromJsonToolStripMenuItem.Click += new System.EventHandler(this.changeFromJsonToolStripMenuItem_Click);
            // 
            // engineToolStripMenuItem
            // 
            this.engineToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.suggestMoveToolStripMenuItem,
            this.showLegalMovesToolStripMenuItem});
            this.engineToolStripMenuItem.Name = "engineToolStripMenuItem";
            this.engineToolStripMenuItem.Size = new System.Drawing.Size(68, 24);
            this.engineToolStripMenuItem.Text = "Engine";
            // 
            // suggestMoveToolStripMenuItem
            // 
            this.suggestMoveToolStripMenuItem.Name = "suggestMoveToolStripMenuItem";
            this.suggestMoveToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.suggestMoveToolStripMenuItem.Text = "Suggest move";
            this.suggestMoveToolStripMenuItem.Click += new System.EventHandler(this.suggestMoveToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadPositionToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(49, 24);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // loadPositionToolStripMenuItem
            // 
            this.loadPositionToolStripMenuItem.Name = "loadPositionToolStripMenuItem";
            this.loadPositionToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.loadPositionToolStripMenuItem.Text = "Load position";
            this.loadPositionToolStripMenuItem.Click += new System.EventHandler(this.loadPositionToolStripMenuItem_Click);
            // 
            // testNewToolStripMenuItem
            // 
            this.testNewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.unittestsToolStripMenuItem,
            this.newUnittestsToolStripMenuItem});
            this.testNewToolStripMenuItem.Name = "testNewToolStripMenuItem";
            this.testNewToolStripMenuItem.Size = new System.Drawing.Size(79, 24);
            this.testNewToolStripMenuItem.Text = "TestNew";
            // 
            // unittestsToolStripMenuItem
            // 
            this.unittestsToolStripMenuItem.Name = "unittestsToolStripMenuItem";
            this.unittestsToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.unittestsToolStripMenuItem.Text = "Unittests";
            this.unittestsToolStripMenuItem.Click += new System.EventHandler(this.unittestsToolStripMenuItem_Click);
            // 
            // positionGeneratorToolStripMenuItem
            // 
            this.positionGeneratorToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.scenario1ToolStripMenuItem,
            this.manyNonTrivialToolStripMenuItem});
            this.positionGeneratorToolStripMenuItem.Name = "positionGeneratorToolStripMenuItem";
            this.positionGeneratorToolStripMenuItem.Size = new System.Drawing.Size(141, 24);
            this.positionGeneratorToolStripMenuItem.Text = "PositionGenerator";
            // 
            // scenario1ToolStripMenuItem
            // 
            this.scenario1ToolStripMenuItem.Name = "scenario1ToolStripMenuItem";
            this.scenario1ToolStripMenuItem.Size = new System.Drawing.Size(198, 26);
            this.scenario1ToolStripMenuItem.Text = "Scenario1";
            this.scenario1ToolStripMenuItem.Click += new System.EventHandler(this.scenario1ToolStripMenuItem_Click);
            // 
            // manyNonTrivialToolStripMenuItem
            // 
            this.manyNonTrivialToolStripMenuItem.Name = "manyNonTrivialToolStripMenuItem";
            this.manyNonTrivialToolStripMenuItem.Size = new System.Drawing.Size(198, 26);
            this.manyNonTrivialToolStripMenuItem.Text = "Many non trivial";
            this.manyNonTrivialToolStripMenuItem.Click += new System.EventHandler(this.manyNonTrivialToolStripMenuItem_Click);
            // 
            // txtbEnterMove
            // 
            this.txtbEnterMove.AcceptsReturn = true;
            this.txtbEnterMove.AcceptsTab = true;
            this.txtbEnterMove.Font = new System.Drawing.Font("Courier New", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtbEnterMove.Location = new System.Drawing.Point(88, 382);
            this.txtbEnterMove.Multiline = true;
            this.txtbEnterMove.Name = "txtbEnterMove";
            this.txtbEnterMove.Size = new System.Drawing.Size(464, 49);
            this.txtbEnterMove.TabIndex = 15;
            this.txtbEnterMove.Text = "mate_in_5_for_white_BN/10";
            this.txtbEnterMove.WordWrap = false;
            // 
            // lblUserMove
            // 
            this.lblUserMove.AutoSize = true;
            this.lblUserMove.Location = new System.Drawing.Point(12, 351);
            this.lblUserMove.Name = "lblUserMove";
            this.lblUserMove.Size = new System.Drawing.Size(136, 16);
            this.lblUserMove.TabIndex = 16;
            this.lblUserMove.Text = "Positionname/n_plies";
            // 
            // lblStatusMessage
            // 
            this.lblStatusMessage.AutoSize = true;
            this.lblStatusMessage.Location = new System.Drawing.Point(12, 47);
            this.lblStatusMessage.Name = "lblStatusMessage";
            this.lblStatusMessage.Size = new System.Drawing.Size(151, 16);
            this.lblStatusMessage.TabIndex = 19;
            this.lblStatusMessage.Text = "Specify settings location";
            // 
            // btnAbort
            // 
            this.btnAbort.Enabled = false;
            this.btnAbort.Location = new System.Drawing.Point(15, 469);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(75, 23);
            this.btnAbort.TabIndex = 20;
            this.btnAbort.Text = "ABORT";
            this.btnAbort.UseVisualStyleBackColor = true;
            this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
            // 
            // txtbOtherValues
            // 
            this.txtbOtherValues.Location = new System.Drawing.Point(88, 553);
            this.txtbOtherValues.Multiline = true;
            this.txtbOtherValues.Name = "txtbOtherValues";
            this.txtbOtherValues.Size = new System.Drawing.Size(464, 39);
            this.txtbOtherValues.TabIndex = 21;
            this.txtbOtherValues.Text = "7/100";
            // 
            // lblOtherValues
            // 
            this.lblOtherValues.AutoSize = true;
            this.lblOtherValues.Location = new System.Drawing.Point(23, 523);
            this.lblOtherValues.Name = "lblOtherValues";
            this.lblOtherValues.Size = new System.Drawing.Size(82, 16);
            this.lblOtherValues.TabIndex = 22;
            this.lblOtherValues.Text = "Other values";
            // 
            // newUnittestsToolStripMenuItem
            // 
            this.newUnittestsToolStripMenuItem.Name = "newUnittestsToolStripMenuItem";
            this.newUnittestsToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.newUnittestsToolStripMenuItem.Text = "New unittests";
            this.newUnittestsToolStripMenuItem.Click += new System.EventHandler(this.newUnittestsToolStripMenuItem_Click);
            // 
            // showLegalMovesToolStripMenuItem
            // 
            this.showLegalMovesToolStripMenuItem.Name = "showLegalMovesToolStripMenuItem";
            this.showLegalMovesToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.showLegalMovesToolStripMenuItem.Text = "Show legal moves";
            this.showLegalMovesToolStripMenuItem.Click += new System.EventHandler(this.showLegalMovesToolStripMenuItem_Click);
            // 
            // formWeirdEngineMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(1275, 619);
            this.Controls.Add(this.lblOtherValues);
            this.Controls.Add(this.txtbOtherValues);
            this.Controls.Add(this.btnAbort);
            this.Controls.Add(this.lblStatusMessage);
            this.Controls.Add(this.lblUserMove);
            this.Controls.Add(this.txtbEnterMove);
            this.Controls.Add(this.lblInformation);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "formWeirdEngineMain";
            this.Text = "The Weird Engine";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblInformation;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveBoardAsImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showResourcesLocationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem engineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem suggestMoveToolStripMenuItem;
        private System.Windows.Forms.TextBox txtbEnterMove;
        private System.Windows.Forms.Label lblUserMove;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadPositionToolStripMenuItem;
        private System.Windows.Forms.Label lblStatusMessage;
        private System.Windows.Forms.ToolStripMenuItem testNewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem unittestsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem positionGeneratorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scenario1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeFromJsonToolStripMenuItem;
        private System.Windows.Forms.Button btnAbort;
        private System.Windows.Forms.ToolStripMenuItem manyNonTrivialToolStripMenuItem;
        private System.Windows.Forms.TextBox txtbOtherValues;
        private System.Windows.Forms.Label lblOtherValues;
        private System.Windows.Forms.ToolStripMenuItem newUnittestsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showLegalMovesToolStripMenuItem;
    }
}

