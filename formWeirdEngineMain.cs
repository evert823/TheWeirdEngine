﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace TheWeirdEngine
{
    public partial class formWeirdEngineMain : Form
    {
        public string ResourcesFolder;
        public BoardPainter MyBoardPainter;
        public WeirdEngineMoveFinder MyWeirdEngineMoveFinder;
        public WeirdEngineJson MyWeirdEngineJson;

        public formWeirdEngineMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            this.BringToFront();

            this.AllowDrop = true;
            this.pictureBox1.AllowDrop = true;

            pictureBox1.BackColor = Color.Black;

            MyWeirdEngineMoveFinder = new WeirdEngineMoveFinder();
            MyWeirdEngineJson = new WeirdEngineJson(this.MyWeirdEngineMoveFinder);

            //We want to do some logging from the MoveFinder object using the Json object:
            MyWeirdEngineMoveFinder.MyWeirdEngineJson = this.MyWeirdEngineJson;//Just a reference

            this.MyWeirdEngineJson.jsonsourcepath = "C:\\Users\\Evert Jan\\Documents\\GitHub\\chesspython\\";
            this.MyWeirdEngineJson.jsonworkpath = "C:\\Users\\Evert Jan\\pythonprojects\\chesspython_nogithub\\";

            string infilename = "maingame";
            MyWeirdEngineJson.LoadPieceTypesFromJson(infilename);
            MyWeirdEngineJson.SavePieceTypesAsJson(infilename);
            MyWeirdEngineJson.LoadPositionJson(MyWeirdEngineJson.jsonsourcepath + "positions", "bulldogmainposition");
            MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "positions_verify\\", "bulldogmainposition");
            MyWeirdEngineJson.LoadEngineSettingsFromJson("enginesettings");

            MyBoardPainter = new BoardPainter(this.MyWeirdEngineMoveFinder, this.pictureBox1);
            this.SelectResourcesFolder();
            MyBoardPainter.resourcefolder = this.ResourcesFolder + "\\";

            this.pictureBox1.Invalidate();
            this.RefreshInformation();
        }

        private void SelectResourcesFolder()
        {
            this.ResourcesFolder = "C:\\Users\\Evert Jan\\Documents\\GitHub\\TheWeirdEngine\\weirdengineresources";

            while (File.Exists(this.ResourcesFolder + "\\vacantonwhite.jpg") == false)
            {
                MessageBox.Show("Select the folder containing your application files by selecting the settingsfile or one of the piece images.");
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    this.ResourcesFolder = Path.GetDirectoryName(openFileDialog1.FileName);
                }
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            MyBoardPainter.Paint_PictureBox_Board(e);
        }

        private void DisableGUI()
        {
            this.menuStrip1.Enabled = false;
            this.txtbEnterMove.Enabled = false;
            this.txtbOtherValues.Enabled = false;
            this.btnAbort.Enabled = true;
        }
        private void EnableGUI()
        {
            this.menuStrip1.Enabled = true;
            this.txtbEnterMove.Enabled = true;
            this.txtbOtherValues.Enabled = true;
            this.btnAbort.Enabled = false;
        }

        private void RefreshInformation()
        {
            this.lblInformation.Text = this.MyBoardPainter.DisplayInformation();
            this.lblStatusMessage.Text = "";
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Currently only JSON import and export is supported");
        }

        private void saveBoardAsImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Images|*.png";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog1.FileName != "")
                {
                    this.DisableGUI();
                    this.MyBoardPainter.SaveBoardAsImage(saveFileDialog1.FileName);
                    this.RefreshInformation();
                    this.EnableGUI();
                }
            }
        }

        private void showResourcesLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Current location of settings and image files : " + this.ResourcesFolder);
        }

        private void suggestMoveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DisableGUI();
            //this.MyWeirdEngineMoveFinder.display_when_n_plies_gt = 0;
            string[] calculaterequest = this.txtbEnterMove.Lines[0].ToString().Split('/');
            string positionname = calculaterequest[0];
            int n_plies = int.Parse(calculaterequest[1]);
            //MessageBox.Show("positionname " + positionname + " n_plies " + n_plies.ToString());
            MyWeirdEngineJson.LoadPositionJson(MyWeirdEngineJson.jsonworkpath + "positions", positionname);
            this.pictureBox1.Invalidate();
            this.RefreshInformation();
            MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "positions_verify\\", positionname);

            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_tree(n_plies);
            this.pictureBox1.Invalidate();
            this.RefreshInformation();
            //MessageBox.Show(this.MyWeirdEngineJson.DisplayMovelist(ref MyWeirdEngineMoveFinder.positionstack[0]));
            //MessageBox.Show(this.MyWeirdEngineJson.DisplayAttacks(ref MyWeirdEngineMoveFinder.positionstack[0]));

            string s;
            s = MyWeirdEngineMoveFinder.FinalResponseLogString(a);
            MessageBox.Show(s);
            //MyWeirdEngineJson.LoadPositionFromFEN("8/8/1p2PR2/5K2/1Pk1p1r1/2p2bpP/3P2n1/N4B2 b");
            //MyWeirdEngineJson.SavePositionAsJson("thiscamefromfen");
            //string myfen = MyWeirdEngineJson.PositionAsFEN();
            //MessageBox.Show("FEN : " + myfen);
            this.EnableGUI();
        }
        private void loadPositionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string[] calculaterequest = this.txtbEnterMove.Lines[0].ToString().Split('/');
            string positionname = calculaterequest[0];
            int n_plies = int.Parse(calculaterequest[1]);
            MyWeirdEngineJson.LoadPositionJson(MyWeirdEngineJson.jsonworkpath + "positions", positionname);
            this.pictureBox1.Invalidate();
            this.RefreshInformation();
            MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "positions_verify\\", positionname);
        }
        private void unittestsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mfunittests Mymfunittests;

            string infilename = "unittestgame";
            MyWeirdEngineJson.LoadPieceTypesFromJson(infilename);
            MyWeirdEngineJson.SavePieceTypesAsJson(infilename);

            this.DisableGUI();
            string unittestpath = this.MyWeirdEngineJson.jsonsourcepath + "unittests";
            Mymfunittests = new mfunittests(this.MyWeirdEngineMoveFinder, this.MyWeirdEngineJson);
            Mymfunittests.RunAllUnittests(unittestpath);
            this.pictureBox1.Invalidate();
            this.RefreshInformation();
            this.EnableGUI();
        }

        private void scenario1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PositionGenerator MyPositionGenerator;
            MyPositionGenerator = new PositionGenerator(this.MyWeirdEngineMoveFinder, this.MyWeirdEngineJson);

            this.DisableGUI();
            MyPositionGenerator.genmain();
            this.pictureBox1.Invalidate();
            this.RefreshInformation();
            this.EnableGUI();
        }

        private void changeFromJsonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MyWeirdEngineJson.LoadEngineSettingsFromJson("enginesettings");
            this.pictureBox1.Invalidate();
            this.RefreshInformation();
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            MyWeirdEngineMoveFinder.externalabort = true;
        }

        private void manyNonTrivialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DisableGUI();
            MessageBox.Show("Under construction");
            this.EnableGUI();
        }

        private void newUnittestsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mfunittests Mymfunittests;

            string infilename = "unittestgame";
            MyWeirdEngineJson.LoadPieceTypesFromJson(infilename);
            MyWeirdEngineJson.SavePieceTypesAsJson(infilename);

            this.DisableGUI();
            string unittestpath = this.MyWeirdEngineJson.jsonsourcepath + "unittests";
            Mymfunittests = new mfunittests(this.MyWeirdEngineMoveFinder, this.MyWeirdEngineJson);
            Mymfunittests.RunNewUnittests(unittestpath);
            this.pictureBox1.Invalidate();
            this.RefreshInformation();
            this.EnableGUI();
        }

        private void showLegalMovesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DisableGUI();
            //this.MyWeirdEngineMoveFinder.display_when_n_plies_gt = 0;
            string[] calculaterequest = this.txtbEnterMove.Lines[0].ToString().Split('/');
            string positionname = calculaterequest[0];
            int n_plies = int.Parse(calculaterequest[1]);
            //MessageBox.Show("positionname " + positionname + " n_plies " + n_plies.ToString());
            MyWeirdEngineJson.LoadPositionJson(MyWeirdEngineJson.jsonworkpath + "positions", positionname);
            this.pictureBox1.Invalidate();
            this.RefreshInformation();
            MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "positions_verify\\", positionname);

            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_tree(1);
            MessageBox.Show(this.MyWeirdEngineJson.DisplayMovelist(MyWeirdEngineMoveFinder.positionstack[0], false));
            string s = "Check info :";
            if (MyWeirdEngineMoveFinder.MyWeirdEngineMoveGenerator.WhiteKingIsInCheck(ref MyWeirdEngineMoveFinder.positionstack[0]) == true)
            {
                s += " white in check";
            }
            if (MyWeirdEngineMoveFinder.MyWeirdEngineMoveGenerator.BlackKingIsInCheck(ref MyWeirdEngineMoveFinder.positionstack[0]) == true)
            {
                s += " black in check";
            }
            if (MyWeirdEngineMoveFinder.MyWeirdEngineMoveGenerator.PMKingIsInCheck(ref MyWeirdEngineMoveFinder.positionstack[0]) == true)
            {
                s += " PM in check";
            }
            if (MyWeirdEngineMoveFinder.MyWeirdEngineMoveGenerator.POKingIsInCheck(ref MyWeirdEngineMoveFinder.positionstack[0]) == true)
            {
                s += " PO in check";
            }
            MessageBox.Show(s);
            this.pictureBox1.Invalidate();
            this.RefreshInformation();
            this.EnableGUI();
        }

        private void loadFENToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DisableGUI();
            string a = txtbOtherValues.Lines[0].ToString();
            MessageBox.Show("Loading FEN and saving to Json : " + a);
            MyWeirdEngineJson.LoadPositionFromFEN(a);
            MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "positions\\", "fromfen");
            this.pictureBox1.Invalidate();
            this.RefreshInformation();
            this.EnableGUI();
        }

        private void getFENToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DisableGUI();
            string myfen = MyWeirdEngineJson.PositionAsFEN();
            txtbOtherValues.AppendText("\r\n" + myfen + "\r\n");
            this.pictureBox1.Invalidate();
            this.RefreshInformation();
            this.EnableGUI();
        }

        private void otherTestsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DisableGUI();


            this.pictureBox1.Invalidate();
            this.RefreshInformation();
            this.EnableGUI();
        }

        private void switchPieceTypesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PositionGenerator MyPositionGenerator;
            MyPositionGenerator = new PositionGenerator(this.MyWeirdEngineMoveFinder, this.MyWeirdEngineJson);

            string infilename = this.txtbOtherValues.Lines[0].ToString();
            this.DisableGUI();
            MyWeirdEngineJson.LoadPieceTypesFromJson(infilename);
            MyWeirdEngineJson.SavePieceTypesAsJson(infilename);

            this.pictureBox1.Invalidate();
            this.RefreshInformation();
            this.EnableGUI();
        }
    }
}
