﻿using System;
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
            if (a.posvalue >= 99.0 || a.posvalue <= -99.0)
            {
                return true;
            }
            return false;
        }
        public bool IsMate(int depth)
        {
            calculationresponse a;
            a = MyWeirdEngineMoveFinder.Calculation_tree(depth);
            if (a.posvalue >= (100 - (depth * 0.1)) || a.posvalue <= ((depth * 0.1) - 100))
            {
                return true;
            }
            return false;
        }
        public void genone_puzzle()
        {
            ClearMainPosition(8, 8);
            PutOnePiece("-K", 6, 6, 7, 7);
            PutOnePiece("-p", 6, 6, 6, 6);
            PutOnePiece("-p", 7, 7, 6, 6);
            PutOnePiece("K", 3, 7, 3, 7);
            PutOnePiece("XH", 4, 7, 4, 7);
            PutOnePiece("B", 4, 7, 4, 7);
            //MyWeirdEngineMoveFinder.positionstack[0].colourtomove = RandomColourToMove();
            MyWeirdEngineMoveFinder.positionstack[0].colourtomove = 1;
        }
        public void gennontrivial_puzzle()
        {
            bool postrivial = true;
            while (postrivial == true)
            {
                genone_puzzle();
                postrivial = IsTrivial(6);
            }
        }
        public void gengreat_puzzle()
        {
            int counter = 0;
            bool posgreat = false;
            while (posgreat == false)
            {
                counter += 1;
                if (counter % 20 == 0)
                {
                    MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "randompositions\\",
                                                        "failed_gen_" + counter.ToString());
                }
                gennontrivial_puzzle();
                posgreat = (IsMate(8));
            }
        }
        public void genmain()
        {
            string infilename = "maingame_2char";
            MyWeirdEngineJson.LoadPieceTypesFromJson(infilename);
            MyWeirdEngineJson.SavePieceTypesAsJson(infilename);

            MyWeirdEngineMoveFinder.myenginesettings.display_when_depth_gt = 8;
            gengreat_puzzle();
            MessageBox.Show("DONE generating great position!!");
            MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "randompositions\\", "frompositiongenerator");
            MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "positions\\", "frompositiongenerator");
        }
    }
}
