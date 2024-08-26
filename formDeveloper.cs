using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace TheWeirdEngine
{
    public partial class formDeveloper : Form
    {
        public WeirdEngineBackend MyWeirdEngineBackend;

        public formDeveloper()
        {
            InitializeComponent();
        }

        private void ProcessFileBackground(string pFileName, byte pNumberOfPlies)
        {
            XElement MySquaresXML;
            string myfolder;

            myfolder = "Q:\\Persoonlijk\\Wiskunde en programmeren\\C#\\WeirdEngine\\Regression testing\\";

            this.MyWeirdEngineBackend.NumberOfPliesToCalculate = pNumberOfPlies;
            MySquaresXML = XElement.Load(myfolder + pFileName);
            MyWeirdEngineBackend.GetGameFromXML(MySquaresXML);

            if (MyWeirdEngineBackend.ValidateImportedGame() == false)
            {
                MessageBox.Show("Validation not passed");
            }
            else
            {
                MyWeirdEngineBackend.SuggestMove();
            }
            MySquaresXML = MyWeirdEngineBackend.GameAsXElement(MyWeirdEngineBackend.MyGame);
            MySquaresXML.Save(myfolder + pFileName + "outv16.xml");

        }

        private void btnRegressiontestscript_Click(object sender, EventArgs e)
        {
            this.btnRegressiontestscript.Enabled = false;
            this.ProcessFileBackground("aa_trivial_01.xml", 3);
            this.ProcessFileBackground("aa_trivial_02.xml", 3);
            this.ProcessFileBackground("aa_trivial_03.xml", 2);
            this.ProcessFileBackground("rook_mate_7_plies_01.xml", 7);
            this.ProcessFileBackground("rook_mate_7_plies_02.xml", 7);
            this.ProcessFileBackground("rook_mate_7_plies_03.xml", 7);
            this.ProcessFileBackground("rook_mate_7_plies_04.xml", 7);
            this.ProcessFileBackground("castle_01.xml", 1);
            this.ProcessFileBackground("castle_02.xml", 1);
            this.ProcessFileBackground("castle_03.xml", 1);
            this.ProcessFileBackground("castle_04.xml", 1);
            this.ProcessFileBackground("ep_01.xml", 1);
            this.ProcessFileBackground("ep_02.xml", 1);
            this.ProcessFileBackground("ep_03.xml", 1);
            this.ProcessFileBackground("ep_04.xml", 1);
            this.ProcessFileBackground("mate_in_2_05.xml", 3);
            this.ProcessFileBackground("mate_in_2_10.xml", 3);
            this.ProcessFileBackground("mate_in_2_15.xml", 3);
            this.ProcessFileBackground("mate_in_2_20.xml", 3);
            this.ProcessFileBackground("mate_in_2_ep.xml", 3);
            this.ProcessFileBackground("mate_in_3_8x8_fide.xml", 5);
            this.ProcessFileBackground("stalemate_2plies_2.xml", 2);
            this.ProcessFileBackground("stalemate_4_plies_8x8_fide.xml", 4);
            this.ProcessFileBackground("witch_01.xml", 1);
            this.ProcessFileBackground("witch_02.xml", 1);
            this.ProcessFileBackground("advancedpawnaggressivity_01_4plies.xml", 4);
            this.ProcessFileBackground("advancedpawnaggressivity_02_1plies.xml", 1);
            this.ProcessFileBackground("advancedpawnaggressivity_03_5plies.xml", 5);
            MessageBox.Show("DONE");
            this.btnRegressiontestscript.Enabled = true;
        }
    }
}
