﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.IO;

namespace TheWeirdEngine
{
    public partial class formWeirdEngineMain : Form
    {
        public string ResourcesFolder;
        public WeirdEngineBackend MyWeirdEngineBackend;
        public BoardPainter MyBoardPainter;
        public WeirdEngineMoveFinder MyWeirdEngineMoveFinder;
        public WeirdEngineJson MyWeirdEngineJson;

        formDeveloper MyformDeveloper;
        public formWeirdEngineMain()
        {
            InitializeComponent();
            MyformDeveloper = new formDeveloper();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            this.BringToFront();

            this.txtbEnterPosition.Height = 300;

            this.AllowDrop = true;
            this.pictureBox1.AllowDrop = true;

            pictureBox1.BackColor = Color.Black;

            MyWeirdEngineBackend = new WeirdEngineBackend(10, 8);
            MyWeirdEngineMoveFinder = new WeirdEngineMoveFinder();
            MyWeirdEngineJson = new WeirdEngineJson(this.MyWeirdEngineMoveFinder);

            //We want to do some logging from the MoveFinder object using the Json object:
            MyWeirdEngineMoveFinder.MyWeirdEngineJson = this.MyWeirdEngineJson;//Just a reference

            MyWeirdEngineBackend.SetInitialStandardBulldog();
            MyBoardPainter = new BoardPainter(this.MyWeirdEngineBackend, this.pictureBox1);
            this.SelectResourcesFolder();
            MyBoardPainter.resourcefolder = this.ResourcesFolder + "\\";

            MyformDeveloper.MyWeirdEngineBackend = this.MyWeirdEngineBackend;//REFERENCETYPE

            this.LoadEngineSettings();
            this.pictureBox1.Invalidate();
            this.RefreshInformation();
        }

        private void SelectResourcesFolder()
        {
            this.ResourcesFolder = "Q:\\Persoonlijk\\Wiskunde en programmeren\\C#\\WeirdEngine\\weirdengineresources";

            while (File.Exists(this.ResourcesFolder + "\\weirdenginesettings.xml") == false)
            {
                MessageBox.Show("Select the folder containing your application files by selecting the settingsfile or one of the piece images.");
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    this.ResourcesFolder = Path.GetDirectoryName(openFileDialog1.FileName);
                }
            }
        }

        private void LoadEngineSettings()
        {
            XElement MySettingsXML;

            try
            {
                MySettingsXML = XElement.Load(this.ResourcesFolder + "\\weirdenginesettings.xml");
                MyWeirdEngineBackend.GetEngineSettingsFromXML(MySettingsXML);
                this.RefreshInformation();
            }
            catch (System.Xml.XmlException)
            {
                MessageBox.Show("There is an issue with the XML in the settingsfile.");
            }
        }


        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            MyBoardPainter.Paint_PictureBox_Board(e);
        }

        private void DisableGUI()
        {
            this.tmrRefreshCalculationLine.Enabled = true;
            this.menuStrip1.Enabled = false;
            this.txtbEnterMove.Enabled = false;
            this.txtbEnterPosition.Enabled = false;
            this.btnAbort.Enabled = true;
        }
        private void EnableGUI()
        {
            this.tmrRefreshCalculationLine.Enabled = false;
            this.lblCalculationLine.Text = "";
            this.menuStrip1.Enabled = true;
            this.txtbEnterMove.Enabled = true;
            this.txtbEnterPosition.Enabled = true;
            this.btnAbort.Enabled = false;
        }

        private void RefreshInformation()
        {
            this.lblInformation.Text = this.MyBoardPainter.DisplayInformation();
            this.lblStatusMessage.Text = MyWeirdEngineBackend.MyGame.StatusMessage;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.lblCalculationLine.Text = this.MyWeirdEngineBackend.MyGame.CalculationLineMessage;
            this.lblCalculationLine.Refresh();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Currently only XML import and export is supported");
        }


        private void importFromXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            XElement MySquaresXML;

            openFileDialog1.Filter = "XML files (*.xml)|*.xml";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {

                this.DisableGUI();

                try
                {
                    MySquaresXML = XElement.Load(openFileDialog1.FileName);
                    MyWeirdEngineBackend.GetGameFromXML(MySquaresXML);
                }
                catch (System.Xml.XmlException)
                {
                    MessageBox.Show("There is an issue with the XML");
                }

                this.EnableGUI();
                this.pictureBox1.Invalidate();
                this.RefreshInformation();
            }
        }

        private void exportToXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            XElement MySquaresXML;

            saveFileDialog1.Filter = "XML files (*.xml)|*.xml";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog1.FileName != "")
                {
                    this.DisableGUI();
                    MySquaresXML = MyWeirdEngineBackend.GameAsXElement(MyWeirdEngineBackend.MyGame);
                    MySquaresXML.Save(saveFileDialog1.FileName);
                    this.EnableGUI();
                    this.RefreshInformation();
                }
            }
        }

        private void exportToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Text files (*.txt)|*.txt";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog1.FileName != "")
                {
                    this.DisableGUI();
                    string[] lines = MyWeirdEngineBackend.LastPositionAsBoardToolText();

                    System.IO.File.WriteAllLines(saveFileDialog1.FileName, lines);
                    this.RefreshInformation();
                    this.EnableGUI();
                }
            }
            this.pictureBox1.Invalidate();
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

        private void reloadSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DisableGUI();
            LoadEngineSettings();
            this.EnableGUI();
            this.pictureBox1.Invalidate();
            this.RefreshInformation();
        }

        private void showResourcesLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Current location of settings and image files : " + this.ResourcesFolder);
        }

        private void validateCurrentPositionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DisableGUI();
            if (MyWeirdEngineBackend.ValidateImportedGame() == false)
            {
                MessageBox.Show("Validation not passed");
            }
            else
            {
                MessageBox.Show("Valid position");
            }
            this.EnableGUI();
            this.pictureBox1.Invalidate();
            this.RefreshInformation();
        }

        private void suggestMoveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string myts;
            DateTime localDate;
            this.DisableGUI();
            localDate = DateTime.Now;
            myts = localDate.ToString();
            this.lblShowTimestamp.Text = "Started " + myts;
            if (MyWeirdEngineBackend.ValidateImportedGame() == false)
            {
                MessageBox.Show("Validation not passed");
            }
            else
            {
                MyWeirdEngineBackend.SuggestMove();
            }
            localDate = DateTime.Now;
            myts = localDate.ToString();
            this.lblShowTimestamp.Text += " ended " + myts;
            this.EnableGUI();
            this.pictureBox1.Invalidate();
            this.RefreshInformation();
        }

        private void developerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DisableGUI();
            this.MyformDeveloper.ShowDialog();
            this.EnableGUI();
            this.pictureBox1.Invalidate();
            this.RefreshInformation();
        }

        private void xMLMoveExampleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.txtbEnterMove.Text = File.ReadAllText(this.ResourcesFolder + "\\move_example.xml");
        }

        private void xMLPositionExampleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.txtbEnterPosition.Text = File.ReadAllText(this.ResourcesFolder + "\\position_example.xml");
        }

        private void validateMoveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            XElement MyMoveXML;

            this.DisableGUI();

            try
            {
                MyMoveXML = XElement.Parse(this.txtbEnterMove.Text);
                this.MyWeirdEngineBackend.MyGame.EnteredMove = this.MyWeirdEngineBackend.GetMoveFromXML(MyMoveXML);

                if (this.MyWeirdEngineBackend.ValidateEnteredMove(this.MyWeirdEngineBackend.MyGame.ActualCurrentPositionidx) == true)
                {
                    this.RefreshInformation();
                }
                else
                {
                    MessageBox.Show("Entered move is NOT valid");
                }
            }
            catch (System.Xml.XmlException)
            {
                MessageBox.Show("Please enter valid XML - see Help for an example");
            }

            this.EnableGUI();
            this.pictureBox1.Invalidate();
            this.RefreshInformation();
        }

        private void loadPositionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            XElement MyGameXML;

            this.DisableGUI();

            try
            {
                MyGameXML = XElement.Parse(this.txtbEnterPosition.Text);
                this.MyWeirdEngineBackend.GetGameFromXML(MyGameXML);
            }
            catch (System.Xml.XmlException)
            {
                MessageBox.Show("Please enter valid XML - see Help for an example");
            }

            this.EnableGUI();
            this.pictureBox1.Invalidate();
            this.RefreshInformation();
        }

        private void doMoveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            XElement MyMoveXML;

            this.DisableGUI();

            try
            {
                MyMoveXML = XElement.Parse(this.txtbEnterMove.Text);

                this.MyWeirdEngineBackend.MyGame.EnteredMove = this.MyWeirdEngineBackend.GetMoveFromXML(MyMoveXML);

                if (MyWeirdEngineBackend.ValidateImportedGame() == false)
                {
                    MessageBox.Show("The position is not valid.");
                }
                else
                {
                    if (this.MyWeirdEngineBackend.ValidateEnteredMove(this.MyWeirdEngineBackend.MyGame.ActualCurrentPositionidx) == true)
                    {
                        this.MyWeirdEngineBackend.DoEnteredMove(this.MyWeirdEngineBackend.MyGame.ActualCurrentPositionidx);
                        this.RefreshInformation();
                    }
                    else
                    {
                        MessageBox.Show("Entered move is NOT valid");
                    }
                }
            }
            catch (System.Xml.XmlException)
            {
                MessageBox.Show("Please enter valid XML - see Help for an example");
            }

            this.EnableGUI();
            this.pictureBox1.Invalidate();
            this.RefreshInformation();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string s;

            s = "TheWeirdEngine is a chess program, designed to handle chess with exotic pieces. At this moment it supports the Witch and Guard.\n"
                 + "Read more about the Witch here: https://www.chess.com/clubs/forum/view/witch-explained \n"
                 + "The GUI is for now highly dependent on XML, the XML-examples are self-explaining.\n\n"
                 + "This engine is work in progress. What works now is tactical calculation, and the option to import an opening book so that it will follow opening lines.";
            MessageBox.Show(s);
        }

        private void suggestMoveAndDoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string myts;
            DateTime localDate;
            this.DisableGUI();
            localDate = DateTime.Now;
            myts = localDate.ToString();
            this.lblShowTimestamp.Text = "Started " + myts;
            if (MyWeirdEngineBackend.ValidateImportedGame() == false)
            {
                MessageBox.Show("Validation not passed");
            }
            else
            {
                MyWeirdEngineBackend.SuggestMoveAndDo();
            }
            localDate = DateTime.Now;
            myts = localDate.ToString();
            this.lblShowTimestamp.Text += " ended " + myts;
            this.EnableGUI();
            this.pictureBox1.Invalidate();
            this.RefreshInformation();
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            this.MyWeirdEngineBackend.MyGame.ExternalAbort = true;
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Start test new");
            this.MyWeirdEngineJson.jsonsourcepath = "C:\\Users\\Evert Jan\\Documents\\GitHub\\chesspython\\";
            this.MyWeirdEngineJson.jsonworkpath = "C:\\Users\\Evert Jan\\pythonprojects\\chesspython_nogithub\\";
            string infilename = "maingame";
            MyWeirdEngineJson.LoadPieceTypesFromJson(infilename);
            MyWeirdEngineJson.SavePieceTypesAsJson(infilename);

            string positionname = "mate_in_4_for_black_hard_chesscom";
            MyWeirdEngineMoveFinder.presort_when_n_plies_gt = 4;
            MyWeirdEngineJson.LoadPositionJson("C:\\Users\\Evert Jan\\pythonprojects\\chesspython_nogithub\\positions", positionname);
            MyWeirdEngineJson.SavePositionAsJson(positionname);

            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_n_plies(8);
            string s = "posvalue " + a.posvalue.ToString();
            s += " moveidx " + a.moveidx.ToString();
            string mvstr = MyWeirdEngineJson.ShortNotation(MyWeirdEngineMoveFinder.mainposition.movelist[a.moveidx]);
            s += " ShortNotation " + mvstr;
            MessageBox.Show(s);

            //MyWeirdEngineJson.LoadPositionFromFEN("8/8/1p2PR2/5K2/1Pk1p1r1/2p2bpP/3P2n1/N4B2 b");
            //MyWeirdEngineJson.SavePositionAsJson("thiscamefromfen");
            //string myfen = MyWeirdEngineJson.PositionAsFEN();
            //MessageBox.Show("FEN : " + myfen);
            MessageBox.Show("End test new");
        }

        private void unittestsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mfunittests Mymfunittests;

            this.MyWeirdEngineJson.jsonsourcepath = "C:\\Users\\Evert Jan\\Documents\\GitHub\\chesspython\\";
            this.MyWeirdEngineJson.jsonworkpath = "C:\\Users\\Evert Jan\\pythonprojects\\chesspython_nogithub\\";
            string unittestpath = this.MyWeirdEngineJson.jsonsourcepath + "unittests";
            string infilename = "maingame";
            MyWeirdEngineJson.LoadPieceTypesFromJson(infilename);
            MyWeirdEngineJson.SavePieceTypesAsJson(infilename);
            Mymfunittests = new mfunittests(this.MyWeirdEngineMoveFinder, this.MyWeirdEngineJson);
            Mymfunittests.RunAllUnittests(unittestpath);
        }
    }
}
