using System;
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

            MyBoardPainter = new BoardPainter(this.MyWeirdEngineMoveFinder, this.pictureBox1);
            this.SelectResourcesFolder();
            MyBoardPainter.resourcefolder = this.ResourcesFolder + "\\";

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
            //this.MyWeirdEngineMoveFinder.display_when_n_plies_gt = 1;
            string[] calculaterequest = this.txtbEnterMove.Lines[0].ToString().Split('/');
            string positionname = calculaterequest[0];
            int n_plies = int.Parse(calculaterequest[1]);
            MessageBox.Show("positionname " + positionname + " n_plies " + n_plies.ToString());
            MyWeirdEngineJson.LoadPositionJson("C:\\Users\\Evert Jan\\pythonprojects\\chesspython_nogithub\\positions", positionname);
            this.pictureBox1.Invalidate();
            this.RefreshInformation();
            MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "positions_verify\\", positionname);

            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_n_plies(n_plies);
            //MessageBox.Show(this.MyWeirdEngineJson.DisplayMovelist(ref MyWeirdEngineMoveFinder.positionstack[0]));
            //MessageBox.Show(this.MyWeirdEngineJson.DisplayAttacks(ref MyWeirdEngineMoveFinder.positionstack[0]));
            string s = "posvalue " + a.posvalue.ToString();
            s += " moveidx " + a.moveidx.ToString();
            string mvstr;
            if (a.moveidx > -1)
            {
                mvstr = MyWeirdEngineJson.ShortNotation(MyWeirdEngineMoveFinder.positionstack[0].movelist[a.moveidx]);
            }
            else
            {
                mvstr = "There was no move";
            }
            s += " ShortNotation " + mvstr;
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
            MyWeirdEngineJson.LoadPositionJson("C:\\Users\\Evert Jan\\pythonprojects\\chesspython_nogithub\\positions", positionname);
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
            this.EnableGUI();
        }

        private void scenario1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PositionGenerator MyPositionGenerator;
            MyPositionGenerator = new PositionGenerator(this.MyWeirdEngineMoveFinder, this.MyWeirdEngineJson);

            string infilename = "maingame";
            MyWeirdEngineJson.LoadPieceTypesFromJson(infilename);
            MyWeirdEngineJson.SavePieceTypesAsJson(infilename);

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
            PositionGenerator MyPositionGenerator;
            MyPositionGenerator = new PositionGenerator(this.MyWeirdEngineMoveFinder, this.MyWeirdEngineJson);

            MyPositionGenerator.str_othervalues = this.txtbOtherValues.Lines[0].ToString().Split('/');

            string infilename = "maingame";
            MyWeirdEngineJson.LoadPieceTypesFromJson(infilename);
            MyWeirdEngineJson.SavePieceTypesAsJson(infilename);

            this.DisableGUI();
            MyPositionGenerator.genmany();
            this.pictureBox1.Invalidate();
            this.RefreshInformation();
            this.EnableGUI();
        }
    }
}
