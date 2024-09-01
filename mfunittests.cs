﻿using System;
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
    public class mfunittests
    {
        private bool AllTestsPassed;
        public WeirdEngineMoveFinder MyWeirdEngineMoveFinder;
        public WeirdEngineJson MyWeirdEngineJson;
        public mfunittests(WeirdEngineMoveFinder pWeirdEngineMoveFinder, WeirdEngineJson myWeirdEngineJson)
        {
            this.MyWeirdEngineMoveFinder = pWeirdEngineMoveFinder;
            this.MyWeirdEngineJson = myWeirdEngineJson;
        }

        public void TestCastle(string ppath, string ppositionfilename)
        {
            MyWeirdEngineJson.LoadPositionJson(ppath, ppositionfilename);
            MyWeirdEngineJson.SavePositionAsJson(ppositionfilename);
            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_n_plies(1);

            bool queensidecastling_happened = false;
            bool kingsidecastling_happened = false;

            for (int movei = 0; movei < MyWeirdEngineMoveFinder.mainposition.movelist_totalfound; movei++)
            {
                string mvstr = MyWeirdEngineJson.ShortNotation(MyWeirdEngineMoveFinder.mainposition.movelist[movei]);
                if (mvstr == "0-0")
                {
                    kingsidecastling_happened = true;
                }
                if (mvstr == "0-0-0")
                {
                    queensidecastling_happened = true;
                }
            }
            if (queensidecastling_happened == false)
            {
                MessageBox.Show(ppositionfilename + "Queenside castling expected but did not happen");
                AllTestsPassed = false;
            }
            if (kingsidecastling_happened == false)
            {
                MessageBox.Show(ppositionfilename + "Kingside castling expected but did not happen");
                AllTestsPassed = false;
            }

        }
        public void TestNoCastle(string ppath, string ppositionfilename)
        {
            MyWeirdEngineJson.LoadPositionJson(ppath, ppositionfilename);
            MyWeirdEngineJson.SavePositionAsJson(ppositionfilename);
            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_n_plies(1);

            bool castling_happened = false;

            for (int movei = 0; movei < MyWeirdEngineMoveFinder.mainposition.movelist_totalfound; movei++)
            {
                string mvstr = MyWeirdEngineJson.ShortNotation(MyWeirdEngineMoveFinder.mainposition.movelist[movei]);
                if (mvstr == "0-0")
                {
                    castling_happened = true;
                }
                if (mvstr == "0-0-0")
                {
                    castling_happened = true;
                }
            }
            if (castling_happened == true)
            {
                MessageBox.Show(ppositionfilename + "Castling happened but not expected");
                AllTestsPassed = false;
            }

        }
        public void TestMove(string ppath, string ppositionfilename, string expectedmovingpiecename,
                                                                     int pi1, int pj1, int pi2, int pj2,
                                                                     bool IsExpected)
        {
            MyWeirdEngineJson.LoadPositionJson(ppath, ppositionfilename);
            MyWeirdEngineJson.SavePositionAsJson(ppositionfilename);
            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_n_plies(1);

            bool mymovehappened = false;
            for (int movei = 0; movei < MyWeirdEngineMoveFinder.mainposition.movelist_totalfound; movei++)
            {
                int pti = MyWeirdEngineMoveFinder.pieceTypeIndex(MyWeirdEngineMoveFinder.mainposition.movelist[movei].MovingPiece);
                if (MyWeirdEngineMoveFinder.piecetypes[pti].name == expectedmovingpiecename)
                {
                    if (MyWeirdEngineMoveFinder.mainposition.movelist[movei].coordinates[0] == pi1 &
                        MyWeirdEngineMoveFinder.mainposition.movelist[movei].coordinates[1] == pj1 &
                        MyWeirdEngineMoveFinder.mainposition.movelist[movei].coordinates[2] == pi2 &
                        MyWeirdEngineMoveFinder.mainposition.movelist[movei].coordinates[3] == pj2)
                    {
                        mymovehappened = true;
                    }
                }
            }
            string givenmvstr;
            givenmvstr = expectedmovingpiecename + "|" + pi1.ToString()
                                                 + "|" + pj1.ToString()
                                                 + "|" + pi2.ToString()
                                                 + "|" + pj2.ToString();
            if (mymovehappened == false & IsExpected == true)
            {
                MessageBox.Show(ppositionfilename + givenmvstr + " did not happen but was expected");
                AllTestsPassed = false;
            }
            if (mymovehappened == true & IsExpected == false)
            {
                MessageBox.Show(ppositionfilename + givenmvstr + " did happen but was not expected");
                AllTestsPassed = false;
            }
        }
        public void TestPawn(string ppath, string ppositionfilename, int pi1, int pj1, int pi2, int pj2)
        {
            MyWeirdEngineJson.LoadPositionJson(ppath, ppositionfilename);
            MyWeirdEngineJson.SavePositionAsJson(ppositionfilename);
            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_n_plies(1);

            bool mymovehappened = false;
            for (int movei = 0; movei < MyWeirdEngineMoveFinder.mainposition.movelist_totalfound; movei++)
            {
                int pti = MyWeirdEngineMoveFinder.pieceTypeIndex(MyWeirdEngineMoveFinder.mainposition.movelist[movei].MovingPiece);
                if (MyWeirdEngineMoveFinder.piecetypes[pti].name == "Pawn")
                {
                    if (MyWeirdEngineMoveFinder.mainposition.movelist[movei].coordinates[0] == pi1 &
                        MyWeirdEngineMoveFinder.mainposition.movelist[movei].coordinates[1] == pj1 &
                        MyWeirdEngineMoveFinder.mainposition.movelist[movei].coordinates[2] == pi2 &
                        MyWeirdEngineMoveFinder.mainposition.movelist[movei].coordinates[3] == pj2)
                    {
                        mymovehappened = true;
                    }
                }

            }
            string givenmvstr;
            givenmvstr = "Pawn" + "|" + pi1.ToString()
                                + "|" + pj1.ToString()
                                + "|" + pi2.ToString()
                                + "|" + pj2.ToString();
            if (mymovehappened == false)
            {
                MessageBox.Show(ppositionfilename + givenmvstr + " did not happen but was expected");
                AllTestsPassed = false;
            }
        }
        public void TestPawnPromote(string ppath, string ppositionfilename, int pi1, int pj1, int pi2, int pj2)
        {
            MyWeirdEngineJson.LoadPositionJson(ppath, ppositionfilename);
            MyWeirdEngineJson.SavePositionAsJson(ppositionfilename);
            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_n_plies(1);

            bool mymovehappened = false;
            for (int movei = 0; movei < MyWeirdEngineMoveFinder.mainposition.movelist_totalfound; movei++)
            {
                int pti = MyWeirdEngineMoveFinder.pieceTypeIndex(MyWeirdEngineMoveFinder.mainposition.movelist[movei].MovingPiece);
                if (MyWeirdEngineMoveFinder.piecetypes[pti].name == "Pawn")
                {
                    if (MyWeirdEngineMoveFinder.mainposition.movelist[movei].coordinates[0] == pi1 &
                        MyWeirdEngineMoveFinder.mainposition.movelist[movei].coordinates[1] == pj1 &
                        MyWeirdEngineMoveFinder.mainposition.movelist[movei].coordinates[2] == pi2 &
                        MyWeirdEngineMoveFinder.mainposition.movelist[movei].coordinates[3] == pj2)
                    {
                        if (MyWeirdEngineMoveFinder.mainposition.movelist[movei].PromoteToPiece != 0)
                        {
                            int ptp = MyWeirdEngineMoveFinder.pieceTypeIndex(MyWeirdEngineMoveFinder.mainposition.movelist[movei].PromoteToPiece);
                            if (MyWeirdEngineMoveFinder.piecetypes[ptp].name == "Hunter")
                            {
                                mymovehappened = true;
                            }
                        }
                    }
                }
            }
            string givenmvstr;
            givenmvstr = "Pawn" + "|" + pi1.ToString()
                                + "|" + pj1.ToString()
                                + "|" + pi2.ToString()
                                + "|" + pj2.ToString() + " --> promote to hunter ";
            if (mymovehappened == false)
            {
                MessageBox.Show(ppositionfilename + givenmvstr + " did not happen but was expected");
                AllTestsPassed = false;
            }
        }
        public void TestCheck(string ppath, string ppositionfilename)
        {
            MyWeirdEngineJson.LoadPositionJson(ppath, ppositionfilename);
            MyWeirdEngineJson.SavePositionAsJson(ppositionfilename);
            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_n_plies(1);

            if (MyWeirdEngineMoveFinder.PMKingIsInCheck(ref MyWeirdEngineMoveFinder.mainposition) == true)
            {
                //nothing
            }
            else
            {
                MessageBox.Show(ppositionfilename + " Check expected but there was no check.");
                AllTestsPassed = false;
            }
        }
        public void TestStalemate(string ppath, string ppositionfilename)
        {
            MyWeirdEngineJson.LoadPositionJson(ppath, ppositionfilename);
            MyWeirdEngineJson.SavePositionAsJson(ppositionfilename);
            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_n_plies(1);

            if (a.posvalue == 0)
            {
                //nothing
            }
            else
            {
                MessageBox.Show(ppositionfilename + " Stalemate expected but there was no stalemate.");
                AllTestsPassed = false;
            }
        }
        public void TestMate(string ppath, string ppositionfilename)
        {
            MyWeirdEngineJson.LoadPositionJson(ppath, ppositionfilename);
            MyWeirdEngineJson.SavePositionAsJson(ppositionfilename);
            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_n_plies(1);

            if (a.posvalue == 100 & MyWeirdEngineMoveFinder.mainposition.colourtomove == -1)
            {
                //nothing
            }
            else if (a.posvalue == -100 & MyWeirdEngineMoveFinder.mainposition.colourtomove == 1)
            {
                //nothing
            }
            else
            {
                MessageBox.Show(ppositionfilename + " Mate expected but there was no mate.");
                AllTestsPassed = false;
            }
        }
        public void TestMate_n(string ppath, string ppositionfilename, int mate_in_n, int pi1, int pj1, int pi2, int pj2)
        {
            int n_plies;
            if (mate_in_n > 0 & mate_in_n < 5)
            {
                n_plies = mate_in_n * 2;
            }
            else
            {
                n_plies = 4;
            }

            MyWeirdEngineJson.LoadPositionJson(ppath, ppositionfilename);
            MyWeirdEngineJson.SavePositionAsJson(ppositionfilename);
            
            DateTime startdatetime = DateTime.Now;
            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_n_plies(n_plies);
            DateTime enddatetime = DateTime.Now;

            int secondsneeded = (int)(enddatetime - startdatetime).TotalSeconds;
            if (n_plies < 5 & secondsneeded > 15)
            {
                MessageBox.Show(ppositionfilename + " Performance of calculation under acceptable levels.");
                AllTestsPassed = false;
            }

            if (pi1 != -1)
            {
                if (MyWeirdEngineMoveFinder.mainposition.movelist[a.moveidx].coordinates[0] == pi1 &
                    MyWeirdEngineMoveFinder.mainposition.movelist[a.moveidx].coordinates[1] == pj1 &
                    MyWeirdEngineMoveFinder.mainposition.movelist[a.moveidx].coordinates[2] == pi2 &
                    MyWeirdEngineMoveFinder.mainposition.movelist[a.moveidx].coordinates[3] == pj2)
                {
                    //nothing
                }
                else
                {
                    MessageBox.Show(ppositionfilename + " Mate expected, but the identified move is not correct.");
                    AllTestsPassed = false;
                }
            }
            if (a.posvalue == 100 & MyWeirdEngineMoveFinder.mainposition.colourtomove == 1)
            {
                //nothing
            }
            else if (a.posvalue == -100 & MyWeirdEngineMoveFinder.mainposition.colourtomove == -1)
            {
                //nothing
            }
            else
            {
                MessageBox.Show(ppositionfilename + " Mate expected but there was no mate.");
                AllTestsPassed = false;
            }
        }
        public void TestStalemate_n(string ppath, string ppositionfilename, int stalemate_in_n)
        {
            int n_plies;
            if (stalemate_in_n > 0 & stalemate_in_n < 5)
            {
                n_plies = (stalemate_in_n * 2) + 1;
            }
            else
            {
                n_plies = 5;
            }

            MyWeirdEngineJson.LoadPositionJson(ppath, ppositionfilename);
            MyWeirdEngineJson.SavePositionAsJson(ppositionfilename);

            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_n_plies(n_plies);

            if (a.posvalue == 0)
            {
                //nothing
            }
            else
            {
                MessageBox.Show(ppositionfilename + " Stalemate expected, but there was no stalemate.");
                AllTestsPassed = false;
            }
        }
        public void BaselinePerformance(string ppath, string ppositionfilename, int n_plies, int baseline_seconds)
        {
            MyWeirdEngineJson.LoadPositionJson(ppath, ppositionfilename);
            MyWeirdEngineJson.SavePositionAsJson(ppositionfilename);

            DateTime startdatetime = DateTime.Now;
            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_n_plies(n_plies);
            DateTime enddatetime = DateTime.Now;
            int secondsneeded = (int)(enddatetime - startdatetime).TotalSeconds;

            MessageBox.Show(ppositionfilename + " observed secondsneeded " + secondsneeded.ToString()
                                              + " baseline_seconds " + baseline_seconds.ToString());

            if (secondsneeded > baseline_seconds)
            {
                MessageBox.Show(ppositionfilename + " Performance of calculation under acceptable levels. "
                                                  + startdatetime.ToString() + "|" + enddatetime.ToString());
                AllTestsPassed = false;
            }
        }
        public void RunAllUnittests(string ppath)
        {
            AllTestsPassed = true;
            MessageBox.Show("Start with running all unittests");

            TestCastle(ppath, "01A_castle_white_01");
            TestMove(ppath, "01A_castle_white_02", "King", 4, 0, 6, 0, true);
            TestMove(ppath, "01A_castle_white_03", "King", 4, 0, 2, 0, true);
            TestCastle(ppath, "01B_castle_black_01");
            TestMove(ppath, "01B_castle_black_02", "King", 4, 7, 6, 7, true);
            TestMove(ppath, "01B_castle_black_03", "King", 4, 7, 2, 7, true);
            TestNoCastle(ppath, "01C_nocastle_white");
            TestNoCastle(ppath, "01D_nocastle_black");
            TestNoCastle(ppath, "01E_nocastle_white");
            TestNoCastle(ppath, "01F_nocastle_black");
            TestNoCastle(ppath, "01G_nocastle_white");
            TestNoCastle(ppath, "01H_nocastle_black");

            TestNoCastle(ppath, "01I_nocastle_white_01");
            TestNoCastle(ppath, "01I_nocastle_white_02");
            TestNoCastle(ppath, "01J_nocastle_black_01");
            TestNoCastle(ppath, "01J_nocastle_black_02");

            TestPawn(ppath, "02A_pawn_white", 3, 4, 3, 5);
            TestPawn(ppath, "02A_pawn_black", 4, 3, 4, 2);
            TestPawn(ppath, "02B_pawn_white", 2, 1, 2, 3);
            TestPawn(ppath, "02B_pawn_black", 1, 6, 1, 4);
            TestPawn(ppath, "02C_pawn_white", 5, 3, 4, 4);
            TestPawn(ppath, "02C_pawn_white", 5, 3, 6, 4);
            TestPawn(ppath, "02C_pawn_black", 2, 3, 3, 2);
            TestPawn(ppath, "02C_pawn_black", 2, 3, 1, 2);
            TestPawn(ppath, "02D_pawn_white", 1, 4, 2, 5);
            TestPawn(ppath, "02D_pawn_black", 2, 3, 1, 2);
            TestPawnPromote(ppath, "02E_pawn_white", 1, 6, 1, 7);
            TestPawnPromote(ppath, "02E_pawn_white", 1, 6, 0, 7);
            TestPawnPromote(ppath, "02E_pawn_black", 6, 1, 6, 0);
            TestPawnPromote(ppath, "02E_pawn_black", 6, 1, 7, 0);

            TestMove(ppath, "03A_divergent_white", "Hunter", 4, 5, 4, 6, true);
            TestMove(ppath, "03A_divergent_white", "Hunter", 4, 5, 2, 6, true);
            TestMove(ppath, "03A_divergent_black", "Hunter", 2, 3, 2, 2, true);
            TestMove(ppath, "03A_divergent_black", "Hunter", 2, 3, 4, 4, true);
            TestMove(ppath, "03A_divergent_white", "Hunter", 4, 5, 4, 4, false);
            TestMove(ppath, "03A_divergent_white", "Hunter", 4, 5, 2, 5, false);
            TestMove(ppath, "03A_divergent_black", "Hunter", 2, 3, 2, 4, false);
            TestMove(ppath, "03A_divergent_black", "Hunter", 2, 3, 4, 3, false);

            TestCheck(ppath, "04A_check_white");
            TestCheck(ppath, "04A_check_black");
            TestStalemate(ppath, "05A_stalemate_white");
            TestStalemate(ppath, "05A_stalemate_black");
            TestMate(ppath, "06A_mate_0_white");
            TestMate(ppath, "06A_mate_0_black");

            TestMate_n(ppath, "06B_mate_1_white", 1, 5, 1, 2, 4);
            TestMate_n(ppath, "06B_mate_1_black", 1, 5, 6, 2, 3);
            TestMate_n(ppath, "06C_mate_2_white_01", 2, 0, 1, 6, 7);
            TestMate_n(ppath, "06C_mate_2_white_02", 2, 7, 1, 1, 7);
            TestMate_n(ppath, "06C_mate_2_black_01", 2, 7, 6, 1, 0);
            TestMate_n(ppath, "06C_mate_2_black_02", 2, 0, 6, 6, 0);
            TestMate_n(ppath, "06D_huntermate_3_white", 3, 2, 4, 1, 4);
            TestMate_n(ppath, "06D_huntermate_3_black", 3, 2, 3, 1, 3);

            TestStalemate_n(ppath, "08A_stalemate_2_white", 2);
            TestStalemate_n(ppath, "08A_stalemate_2_black", 2);

            BaselinePerformance(ppath, "07A_mate_4_white_BN", 8, 5);
            BaselinePerformance(ppath, "07A_mate_4_black_BN", 8, 5);

            if (AllTestsPassed == true)
            {
                MessageBox.Show("All unittests passed");
            }
            else
            {
                MessageBox.Show("Some unittests failed");
            }
        }

    }
}