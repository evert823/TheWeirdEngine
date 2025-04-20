using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public string[] str_othervalues;
        Random myrandom;
        public PositionGenerator(WeirdEngineMoveFinder pWeirdEngineMoveFinder, WeirdEngineJson pWeirdEngineJson)
        {
            myrandom = new Random((int)DateTime.Now.Ticks);
            this.MyWeirdEngineMoveFinder = pWeirdEngineMoveFinder;
            this.MyWeirdEngineJson = pWeirdEngineJson;
            ClearMainPosition(8, 8);
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
        public void ClearMainPosition(int newboardwidth, int newboardheight)
        {
            this.MyWeirdEngineMoveFinder.init_positionstack(newboardwidth, newboardheight);
            this.MyWeirdEngineMoveFinder.DisableCastling(ref MyWeirdEngineMoveFinder.positionstack[0]);
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
        public bool IsTrivial(int depth)
        {
            calculationresponse a;
            a = MyWeirdEngineMoveFinder.Calculation_tree(depth);
            if (a.posvalue >= 97.0 || a.posvalue <= -97.0)
            {
                return true;
            }
            return false;
        }
        public void genone()
        {
            ClearMainPosition(8, 8);
            PutOnePiece("-K", 0, 7, 5, 7);
            PutOnePiece("K", 0, 7, 0, 2);

            PutOnePiece("p", 0, 2, 2, 5);
            PutOnePiece("p", 0, 2, 2, 5);
            PutOnePiece("p", 3, 5, 2, 5);
            PutOnePiece("p", 3, 5, 2, 5);
            PutOnePiece("p", 6, 7, 2, 5);
            PutOnePiece("p", 0, 7, 2, 5);
            PutOnePiece("-p", 0, 2, 2, 5);
            PutOnePiece("-p", 0, 2, 2, 5);
            PutOnePiece("-p", 3, 5, 2, 5);
            PutOnePiece("-p", 3, 5, 2, 5);
            PutOnePiece("-p", 6, 7, 2, 5);
            PutOnePiece("-p", 0, 7, 2, 5);

            PutOnePiece("N", 0, 7, 0, 5);
            PutOnePiece("B", 0, 7, 0, 5);
            PutOnePiece("Q", 0, 7, 0, 5);
            PutOnePiece("-N", 0, 7, 2, 7);
            PutOnePiece("-B", 0, 7, 2, 7);
            PutOnePiece("-Q", 0, 7, 2, 7);

            PutOnePiece("F", 0, 7, 0, 5);
            PutOnePiece("-R", 0, 7, 2, 7);
            MyWeirdEngineMoveFinder.positionstack[0].colourtomove = RandomColourToMove();
        }
        public void genone_puzzle()
        {
            ClearMainPosition(8, 8);
            PutOnePiece("K", 0, 0, 7, 7);
            PutOnePiece("-K", 3, 4, 3, 4);
            PutOnePiece("p", 0, 0, 1, 5);
            PutOnePiece("p", 1, 1, 1, 5);
            PutOnePiece("p", 2, 2, 1, 5);
            PutOnePiece("p", 3, 3, 1, 5);
            PutOnePiece("p", 4, 4, 1, 5);
            PutOnePiece("p", 5, 5, 1, 5);
            PutOnePiece("p", 6, 6, 1, 5);
            PutOnePiece("p", 7, 7, 1, 5);
            PutOnePiece("-p", 0, 0, 2, 6);
            PutOnePiece("-p", 1, 1, 2, 6);
            PutOnePiece("-p", 2, 2, 2, 6);
            PutOnePiece("-p", 3, 3, 2, 6);
            PutOnePiece("-p", 4, 4, 2, 6);
            PutOnePiece("-p", 5, 5, 2, 6);
            PutOnePiece("-p", 6, 6, 2, 6);
            PutOnePiece("-p", 7, 7, 2, 6);
            PutOnePiece("R", 0, 7, 0, 7);
            PutOnePiece("R", 0, 7, 0, 7);
            PutOnePiece("N", 0, 7, 0, 7);
            PutOnePiece("N", 0, 7, 0, 7);
            PutOnePiece("B", 0, 7, 0, 7);
            PutOnePiece("B", 0, 7, 0, 7);
            PutOnePiece("Q", 0, 7, 0, 7);
            PutOnePiece("-R", 0, 7, 0, 7);
            PutOnePiece("-R", 0, 7, 0, 7);
            PutOnePiece("-N", 0, 7, 0, 7);
            PutOnePiece("-N", 0, 7, 0, 7);
            PutOnePiece("-B", 0, 7, 0, 7);
            PutOnePiece("-B", 0, 7, 0, 7);
            PutOnePiece("-Q", 0, 7, 0, 7);
            //MyWeirdEngineMoveFinder.positionstack[0].colourtomove = RandomColourToMove();
            MyWeirdEngineMoveFinder.positionstack[0].colourtomove = 1;
        }
        public void gennontrivial()
        {
            bool postrivial = true;
            while (postrivial == true)
            {
                genone();
                postrivial = IsTrivial(3);
            }
        }
        public void gengreat_puzzle()
        {
            calculationresponse a;
            int counter = 0;
            bool searchdone = false;
            while (searchdone == false)
            {
                genone_puzzle();

                if (MyWeirdEngineMoveFinder.positionstack[0].squares[4, 7] == 0)
                {
                    counter += 1;
                    if (counter % 20 == 0)
                    {
                        MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "randompositions\\",
                                                            "failed_gen_" + counter.ToString());
                    }
                    a = MyWeirdEngineMoveFinder.Calculation_tree(4);
                    if (a.moveidx > -1)
                    {
                        int pi = MyWeirdEngineMoveFinder.positionstack[0].movelist[a.moveidx].PromoteToPiece;
                        int pti = MyWeirdEngineMoveFinder.pieceTypeIndex(pi);
                        if (pti > -1)
                        {
                            if (a.posvalue == 99.7 | a.posvalue == 99.9)
                            {
                                MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "randompositions\\",
                                                                    "random_mate_2_" + counter.ToString());
                                if (MyWeirdEngineMoveFinder.piecetypes[pti].name == "Knight")
                                {
                                    searchdone = true;
                                    MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "randompositions\\", "frompositiongenerator");
                                }
                            }
                        }
                    }
                }
            }
        }
        public void genmain()
        {
            //MyWeirdEngineMoveFinder.setting_SearchForFastestMate = false;
            MyWeirdEngineJson.LoadPieceTypesFromJson("fide");
            MyWeirdEngineMoveFinder.myenginesettings.display_when_depth_gt = 8;
            gengreat_puzzle();
            MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "positions\\", "frompositiongenerator");
            MessageBox.Show("DONE generating great position!!");
        }
        public void genmany()
        {
            double totvalue = 0;

            int depth;
            int totalnumber;
            int.TryParse(str_othervalues[0], out depth);
            int.TryParse(str_othervalues[1], out totalnumber);
            MessageBox.Show("depth : " + depth.ToString());
            MessageBox.Show("totalnumber : " + totalnumber.ToString());

            for (int i = 0; i < totalnumber; i++)
            {
                gennontrivial();
                calculationresponse a = MyWeirdEngineMoveFinder.Calculation_tree(depth);
                MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "randompositions\\",
                                                     "rnd_pos_" + i.ToString());
                totvalue += a.posvalue;
            }
            MyWeirdEngineJson.writelog("totvalue : " + totvalue.ToString());
            //MessageBox.Show("totvalue : " + totvalue.ToString());
        }
        public void genveryverylarge()
        {
            ClearMainPosition(60, 60);
            PutOnePiece("K", 0, 59, 0, 59);
            PutOnePiece("-K", 0, 59, 0, 59);
            for (int i = 0; i < 60; i++)
            {
                PutOnePiece("p", i, i, 4, 35);
                PutOnePiece("-p", i, i, 25, 55);
            }
            for (int i = 0; i < 6; i++)
            {
                PutOnePiece("Q", 0, 59, 0, 59);
                PutOnePiece("-Q", 0, 59, 0, 59);
                PutOnePiece("R", 0, 59, 0, 59);
                PutOnePiece("-R", 0, 59, 0, 59);
                PutOnePiece("B", 0, 59, 0, 59);
                PutOnePiece("-B", 0, 59, 0, 59);
                PutOnePiece("N", 0, 59, 0, 59);
                PutOnePiece("-N", 0, 59, 0, 59);
                PutOnePiece("L", 0, 59, 0, 59);
                PutOnePiece("-L", 0, 59, 0, 59);
                PutOnePiece("D", 0, 59, 0, 59);
                PutOnePiece("-D", 0, 59, 0, 59);
                PutOnePiece("T", 0, 59, 0, 59);
                PutOnePiece("-T", 0, 59, 0, 59);
                PutOnePiece("A", 0, 59, 0, 59);
                PutOnePiece("-A", 0, 59, 0, 59);
                PutOnePiece("C", 0, 59, 0, 59);
                PutOnePiece("-C", 0, 59, 0, 59);
                PutOnePiece("W", 0, 59, 0, 59);
                PutOnePiece("-W", 0, 59, 0, 59);
                PutOnePiece("X", 0, 59, 0, 59);
                PutOnePiece("-X", 0, 59, 0, 59);
                PutOnePiece("H", 0, 59, 0, 59);
                PutOnePiece("-H", 0, 59, 0, 59);
                PutOnePiece("J", 0, 59, 0, 59);
                PutOnePiece("-J", 0, 59, 0, 59);
                PutOnePiece("V", 0, 59, 0, 59);
                PutOnePiece("-V", 0, 59, 0, 59);
            }
            MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "positions\\", "frompositiongenerator");
        }

    }
}
