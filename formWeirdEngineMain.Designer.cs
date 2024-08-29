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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formWeirdEngineMain));
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tmrRefreshCalculationLine = new System.Windows.Forms.Timer(this.components);
            this.lblCalculationLine = new System.Windows.Forms.Label();
            this.lblInformation = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importFromXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveBoardAsImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reloadSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showResourcesLocationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.engineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.validateCurrentPositionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.suggestMoveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.suggestMoveAndDoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.validateMoveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadPositionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.doMoveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.developerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.xMLMoveExampleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.xMLPositionExampleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testNewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.txtbEnterMove = new System.Windows.Forms.TextBox();
            this.lblUserMove = new System.Windows.Forms.Label();
            this.lblUserPosition = new System.Windows.Forms.Label();
            this.txtbEnterPosition = new System.Windows.Forms.TextBox();
            this.lblStatusMessage = new System.Windows.Forms.Label();
            this.btnAbort = new System.Windows.Forms.Button();
            this.lblShowTimestamp = new System.Windows.Forms.Label();
            this.unittestsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            // tmrRefreshCalculationLine
            // 
            this.tmrRefreshCalculationLine.Interval = 500;
            this.tmrRefreshCalculationLine.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // lblCalculationLine
            // 
            this.lblCalculationLine.AutoSize = true;
            this.lblCalculationLine.Location = new System.Drawing.Point(12, 87);
            this.lblCalculationLine.Name = "lblCalculationLine";
            this.lblCalculationLine.Size = new System.Drawing.Size(109, 16);
            this.lblCalculationLine.TabIndex = 8;
            this.lblCalculationLine.Text = "(Calculation Line)";
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
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem,
            this.testNewToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1275, 28);
            this.menuStrip1.TabIndex = 14;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importFromXMLToolStripMenuItem,
            this.exportToXMLToolStripMenuItem,
            this.exportToToolStripMenuItem,
            this.saveBoardAsImageToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // importFromXMLToolStripMenuItem
            // 
            this.importFromXMLToolStripMenuItem.Name = "importFromXMLToolStripMenuItem";
            this.importFromXMLToolStripMenuItem.Size = new System.Drawing.Size(231, 26);
            this.importFromXMLToolStripMenuItem.Text = "Import from XML";
            this.importFromXMLToolStripMenuItem.Click += new System.EventHandler(this.importFromXMLToolStripMenuItem_Click);
            // 
            // exportToXMLToolStripMenuItem
            // 
            this.exportToXMLToolStripMenuItem.Name = "exportToXMLToolStripMenuItem";
            this.exportToXMLToolStripMenuItem.Size = new System.Drawing.Size(231, 26);
            this.exportToXMLToolStripMenuItem.Text = "Export to XML";
            this.exportToXMLToolStripMenuItem.Click += new System.EventHandler(this.exportToXMLToolStripMenuItem_Click);
            // 
            // exportToToolStripMenuItem
            // 
            this.exportToToolStripMenuItem.Name = "exportToToolStripMenuItem";
            this.exportToToolStripMenuItem.Size = new System.Drawing.Size(231, 26);
            this.exportToToolStripMenuItem.Text = "Export to Board tool";
            this.exportToToolStripMenuItem.Click += new System.EventHandler(this.exportToToolStripMenuItem_Click);
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
            this.reloadSettingsToolStripMenuItem,
            this.showResourcesLocationToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(76, 24);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // reloadSettingsToolStripMenuItem
            // 
            this.reloadSettingsToolStripMenuItem.Name = "reloadSettingsToolStripMenuItem";
            this.reloadSettingsToolStripMenuItem.Size = new System.Drawing.Size(256, 26);
            this.reloadSettingsToolStripMenuItem.Text = "Reload settings";
            this.reloadSettingsToolStripMenuItem.Click += new System.EventHandler(this.reloadSettingsToolStripMenuItem_Click);
            // 
            // showResourcesLocationToolStripMenuItem
            // 
            this.showResourcesLocationToolStripMenuItem.Name = "showResourcesLocationToolStripMenuItem";
            this.showResourcesLocationToolStripMenuItem.Size = new System.Drawing.Size(256, 26);
            this.showResourcesLocationToolStripMenuItem.Text = "Show Resources location";
            this.showResourcesLocationToolStripMenuItem.Click += new System.EventHandler(this.showResourcesLocationToolStripMenuItem_Click);
            // 
            // engineToolStripMenuItem
            // 
            this.engineToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.validateCurrentPositionToolStripMenuItem,
            this.suggestMoveToolStripMenuItem,
            this.suggestMoveAndDoToolStripMenuItem});
            this.engineToolStripMenuItem.Name = "engineToolStripMenuItem";
            this.engineToolStripMenuItem.Size = new System.Drawing.Size(68, 24);
            this.engineToolStripMenuItem.Text = "Engine";
            // 
            // validateCurrentPositionToolStripMenuItem
            // 
            this.validateCurrentPositionToolStripMenuItem.Name = "validateCurrentPositionToolStripMenuItem";
            this.validateCurrentPositionToolStripMenuItem.Size = new System.Drawing.Size(254, 26);
            this.validateCurrentPositionToolStripMenuItem.Text = "Validate current position";
            this.validateCurrentPositionToolStripMenuItem.Click += new System.EventHandler(this.validateCurrentPositionToolStripMenuItem_Click);
            // 
            // suggestMoveToolStripMenuItem
            // 
            this.suggestMoveToolStripMenuItem.Name = "suggestMoveToolStripMenuItem";
            this.suggestMoveToolStripMenuItem.Size = new System.Drawing.Size(254, 26);
            this.suggestMoveToolStripMenuItem.Text = "Suggest move";
            this.suggestMoveToolStripMenuItem.Click += new System.EventHandler(this.suggestMoveToolStripMenuItem_Click);
            // 
            // suggestMoveAndDoToolStripMenuItem
            // 
            this.suggestMoveAndDoToolStripMenuItem.Name = "suggestMoveAndDoToolStripMenuItem";
            this.suggestMoveAndDoToolStripMenuItem.Size = new System.Drawing.Size(254, 26);
            this.suggestMoveAndDoToolStripMenuItem.Text = "SuggestMoveAndDo";
            this.suggestMoveAndDoToolStripMenuItem.Click += new System.EventHandler(this.suggestMoveAndDoToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.validateMoveToolStripMenuItem,
            this.loadPositionToolStripMenuItem,
            this.doMoveToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(49, 24);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // validateMoveToolStripMenuItem
            // 
            this.validateMoveToolStripMenuItem.Name = "validateMoveToolStripMenuItem";
            this.validateMoveToolStripMenuItem.Size = new System.Drawing.Size(187, 26);
            this.validateMoveToolStripMenuItem.Text = "Validate move";
            this.validateMoveToolStripMenuItem.Click += new System.EventHandler(this.validateMoveToolStripMenuItem_Click);
            // 
            // loadPositionToolStripMenuItem
            // 
            this.loadPositionToolStripMenuItem.Name = "loadPositionToolStripMenuItem";
            this.loadPositionToolStripMenuItem.Size = new System.Drawing.Size(187, 26);
            this.loadPositionToolStripMenuItem.Text = "Load position";
            this.loadPositionToolStripMenuItem.Click += new System.EventHandler(this.loadPositionToolStripMenuItem_Click);
            // 
            // doMoveToolStripMenuItem
            // 
            this.doMoveToolStripMenuItem.Name = "doMoveToolStripMenuItem";
            this.doMoveToolStripMenuItem.Size = new System.Drawing.Size(187, 26);
            this.doMoveToolStripMenuItem.Text = "Do move";
            this.doMoveToolStripMenuItem.Click += new System.EventHandler(this.doMoveToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.developerToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(58, 24);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // developerToolStripMenuItem
            // 
            this.developerToolStripMenuItem.Name = "developerToolStripMenuItem";
            this.developerToolStripMenuItem.Size = new System.Drawing.Size(161, 26);
            this.developerToolStripMenuItem.Text = "Developer";
            this.developerToolStripMenuItem.Click += new System.EventHandler(this.developerToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.xMLMoveExampleToolStripMenuItem,
            this.xMLPositionExampleToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(55, 24);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // xMLMoveExampleToolStripMenuItem
            // 
            this.xMLMoveExampleToolStripMenuItem.Name = "xMLMoveExampleToolStripMenuItem";
            this.xMLMoveExampleToolStripMenuItem.Size = new System.Drawing.Size(240, 26);
            this.xMLMoveExampleToolStripMenuItem.Text = "XML move example";
            this.xMLMoveExampleToolStripMenuItem.Click += new System.EventHandler(this.xMLMoveExampleToolStripMenuItem_Click);
            // 
            // xMLPositionExampleToolStripMenuItem
            // 
            this.xMLPositionExampleToolStripMenuItem.Name = "xMLPositionExampleToolStripMenuItem";
            this.xMLPositionExampleToolStripMenuItem.Size = new System.Drawing.Size(240, 26);
            this.xMLPositionExampleToolStripMenuItem.Text = "XML position example";
            this.xMLPositionExampleToolStripMenuItem.Click += new System.EventHandler(this.xMLPositionExampleToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(240, 26);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // testNewToolStripMenuItem
            // 
            this.testNewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.testToolStripMenuItem,
            this.unittestsToolStripMenuItem});
            this.testNewToolStripMenuItem.Name = "testNewToolStripMenuItem";
            this.testNewToolStripMenuItem.Size = new System.Drawing.Size(79, 24);
            this.testNewToolStripMenuItem.Text = "TestNew";
            // 
            // testToolStripMenuItem
            // 
            this.testToolStripMenuItem.Name = "testToolStripMenuItem";
            this.testToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.testToolStripMenuItem.Text = "Test";
            this.testToolStripMenuItem.Click += new System.EventHandler(this.testToolStripMenuItem_Click);
            // 
            // txtbEnterMove
            // 
            this.txtbEnterMove.AcceptsReturn = true;
            this.txtbEnterMove.AcceptsTab = true;
            this.txtbEnterMove.Font = new System.Drawing.Font("Courier New", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtbEnterMove.Location = new System.Drawing.Point(192, 326);
            this.txtbEnterMove.Multiline = true;
            this.txtbEnterMove.Name = "txtbEnterMove";
            this.txtbEnterMove.Size = new System.Drawing.Size(464, 120);
            this.txtbEnterMove.TabIndex = 15;
            this.txtbEnterMove.Text = "--> See Edit for more options / Help for examples";
            this.txtbEnterMove.WordWrap = false;
            // 
            // lblUserMove
            // 
            this.lblUserMove.AutoSize = true;
            this.lblUserMove.Location = new System.Drawing.Point(12, 326);
            this.lblUserMove.Name = "lblUserMove";
            this.lblUserMove.Size = new System.Drawing.Size(151, 16);
            this.lblUserMove.TabIndex = 16;
            this.lblUserMove.Text = "Enter your move as XML";
            // 
            // lblUserPosition
            // 
            this.lblUserPosition.AutoSize = true;
            this.lblUserPosition.Location = new System.Drawing.Point(12, 470);
            this.lblUserPosition.Name = "lblUserPosition";
            this.lblUserPosition.Size = new System.Drawing.Size(164, 16);
            this.lblUserPosition.TabIndex = 17;
            this.lblUserPosition.Text = "Enter your position as XML";
            // 
            // txtbEnterPosition
            // 
            this.txtbEnterPosition.AcceptsReturn = true;
            this.txtbEnterPosition.AcceptsTab = true;
            this.txtbEnterPosition.Font = new System.Drawing.Font("Courier New", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtbEnterPosition.Location = new System.Drawing.Point(12, 508);
            this.txtbEnterPosition.Multiline = true;
            this.txtbEnterPosition.Name = "txtbEnterPosition";
            this.txtbEnterPosition.Size = new System.Drawing.Size(641, 22);
            this.txtbEnterPosition.TabIndex = 18;
            this.txtbEnterPosition.Text = "--> See Edit for more options / Help for examples";
            this.txtbEnterPosition.WordWrap = false;
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
            this.btnAbort.Location = new System.Drawing.Point(578, 253);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(75, 23);
            this.btnAbort.TabIndex = 20;
            this.btnAbort.Text = "Abort";
            this.btnAbort.UseVisualStyleBackColor = true;
            this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
            // 
            // lblShowTimestamp
            // 
            this.lblShowTimestamp.AutoSize = true;
            this.lblShowTimestamp.Location = new System.Drawing.Point(12, 119);
            this.lblShowTimestamp.Name = "lblShowTimestamp";
            this.lblShowTimestamp.Size = new System.Drawing.Size(110, 16);
            this.lblShowTimestamp.TabIndex = 21;
            this.lblShowTimestamp.Text = "timing information";
            // 
            // unittestsToolStripMenuItem
            // 
            this.unittestsToolStripMenuItem.Name = "unittestsToolStripMenuItem";
            this.unittestsToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.unittestsToolStripMenuItem.Text = "Unittests";
            this.unittestsToolStripMenuItem.Click += new System.EventHandler(this.unittestsToolStripMenuItem_Click);
            // 
            // formWeirdEngineMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(1275, 534);
            this.Controls.Add(this.lblShowTimestamp);
            this.Controls.Add(this.btnAbort);
            this.Controls.Add(this.lblStatusMessage);
            this.Controls.Add(this.txtbEnterPosition);
            this.Controls.Add(this.lblUserPosition);
            this.Controls.Add(this.lblUserMove);
            this.Controls.Add(this.txtbEnterMove);
            this.Controls.Add(this.lblInformation);
            this.Controls.Add(this.lblCalculationLine);
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
        private System.Windows.Forms.Timer tmrRefreshCalculationLine;
        private System.Windows.Forms.Label lblCalculationLine;
        private System.Windows.Forms.Label lblInformation;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importFromXMLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToXMLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveBoardAsImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reloadSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showResourcesLocationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem engineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem validateCurrentPositionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem suggestMoveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem developerToolStripMenuItem;
        private System.Windows.Forms.TextBox txtbEnterMove;
        private System.Windows.Forms.Label lblUserMove;
        private System.Windows.Forms.Label lblUserPosition;
        private System.Windows.Forms.TextBox txtbEnterPosition;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem xMLMoveExampleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem xMLPositionExampleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem validateMoveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadPositionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem doMoveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem suggestMoveAndDoToolStripMenuItem;
        private System.Windows.Forms.Label lblStatusMessage;
        private System.Windows.Forms.Button btnAbort;
        private System.Windows.Forms.Label lblShowTimestamp;
        private System.Windows.Forms.ToolStripMenuItem testNewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem testToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem unittestsToolStripMenuItem;
    }
}

