using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TheWeirdEngine
{
    //Unittests for WeirdEngineMoveFinder
    public class PositionGenerator
    {
        public WeirdEngineMoveFinder MyWeirdEngineMoveFinder;
        public WeirdEngineJson MyWeirdEngineJson;
        Random myrandom;
        public PositionGenerator(WeirdEngineMoveFinder pWeirdEngineMoveFinder, WeirdEngineJson pWeirdEngineJson)
        {
            myrandom = new Random((int)DateTime.Now.Ticks);
            this.MyWeirdEngineMoveFinder = pWeirdEngineMoveFinder;
            this.MyWeirdEngineJson = pWeirdEngineJson;
            ClearMainPosition();
        }

        public int RandomColourToMove()
        {
            int i = 0;
            while (i == 0)
            {
                i = myrandom.Next(-1, 2);
            }
            return i;
        }
        public void ClearMainPosition()
        {
            this.MyWeirdEngineMoveFinder.init_positionstack(8, 8);
        }
        public void PutOnePiece(string psymbol, int min_i, int max_i, int min_j, int max_j)
        {
            int i;
            int j;
            i = myrandom.Next(min_i, max_i + 1);
            j = myrandom.Next(min_j, max_j + 1);
            while (MyWeirdEngineMoveFinder.positionstack[0].squares[i, j] != 0)
            {
                i = myrandom.Next(min_i, max_i + 1);
                j = myrandom.Next(min_j, max_j + 1);
            }
            MyWeirdEngineMoveFinder.positionstack[0].squares[i, j] = MyWeirdEngineJson.Str2PieceType(psymbol);
        }
        public bool IsTrivial(int n_plies)
        {
            calculationresponse a;
            a = MyWeirdEngineMoveFinder.Calculation_n_plies(n_plies);
            if (a.posvalue >= 100 || a.posvalue <= -100)
            {
                return true;
            }
            return false;
        }
        public void genone()
        {
            ClearMainPosition();
            PutOnePiece("-K", 0, 3, 4, 7);
            PutOnePiece("K", 2, 4, 3, 7);
            PutOnePiece("N", 0, 4, 2, 6);
            PutOnePiece("B", 0, 7, 0, 7);
            PutOnePiece("W", 0, 7, 0, 7);
            //MyWeirdEngineMoveFinder.positionstack[0].colourtomove = RandomColourToMove();
            MyWeirdEngineMoveFinder.positionstack[0].colourtomove = 1;
        }
        public void gennontrivial()
        {
            bool postrivial = true;
            while (postrivial == true)
            {
                genone();
                postrivial = IsTrivial(8);
            }
        }
        public void gengreat()
        {
            bool posgreat = false;
            while (posgreat == false)
            {
                gennontrivial();
                posgreat = IsTrivial(10);
            }
        }
        public void genmain()
        {
            gengreat();
            MessageBox.Show("DONE generating great position!!");
            MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "randompositions\\", "frompositiongenerator");
            MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "positions\\", "frompositiongenerator");
        }

    }
}
