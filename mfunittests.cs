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
            MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "positions_verify\\", ppositionfilename);
            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_tree(1);

            bool queensidecastling_happened = false;
            bool kingsidecastling_happened = false;

            for (int movei = 0; movei < MyWeirdEngineMoveFinder.positionstack[0].movelist_totalfound; movei++)
            {
                string mvstr = MyWeirdEngineJson.ShortNotation(MyWeirdEngineMoveFinder.positionstack[0].movelist[movei]);
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
            MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "positions_verify\\", ppositionfilename);
            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_tree(1);

            bool castling_happened = false;

            for (int movei = 0; movei < MyWeirdEngineMoveFinder.positionstack[0].movelist_totalfound; movei++)
            {
                string mvstr = MyWeirdEngineJson.ShortNotation(MyWeirdEngineMoveFinder.positionstack[0].movelist[movei]);
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
            MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "positions_verify\\", ppositionfilename);
            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_tree(1);

            bool mymovehappened = false;
            for (int movei = 0; movei < MyWeirdEngineMoveFinder.positionstack[0].movelist_totalfound; movei++)
            {
                int pti = MyWeirdEngineMoveFinder.pieceTypeIndex(MyWeirdEngineMoveFinder.positionstack[0].movelist[movei].MovingPiece);
                if (MyWeirdEngineMoveFinder.piecetypes[pti].name == expectedmovingpiecename)
                {
                    if (MyWeirdEngineMoveFinder.positionstack[0].movelist[movei].coordinates[0] == pi1 &
                        MyWeirdEngineMoveFinder.positionstack[0].movelist[movei].coordinates[1] == pj1 &
                        MyWeirdEngineMoveFinder.positionstack[0].movelist[movei].coordinates[2] == pi2 &
                        MyWeirdEngineMoveFinder.positionstack[0].movelist[movei].coordinates[3] == pj2)
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
            MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "positions_verify\\", ppositionfilename);
            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_tree(1);

            bool mymovehappened = false;
            for (int movei = 0; movei < MyWeirdEngineMoveFinder.positionstack[0].movelist_totalfound; movei++)
            {
                int pti = MyWeirdEngineMoveFinder.pieceTypeIndex(MyWeirdEngineMoveFinder.positionstack[0].movelist[movei].MovingPiece);
                if (MyWeirdEngineMoveFinder.piecetypes[pti].name == "Pawn")
                {
                    if (MyWeirdEngineMoveFinder.positionstack[0].movelist[movei].coordinates[0] == pi1 &
                        MyWeirdEngineMoveFinder.positionstack[0].movelist[movei].coordinates[1] == pj1 &
                        MyWeirdEngineMoveFinder.positionstack[0].movelist[movei].coordinates[2] == pi2 &
                        MyWeirdEngineMoveFinder.positionstack[0].movelist[movei].coordinates[3] == pj2)
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
            MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "positions_verify\\", ppositionfilename);
            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_tree(1);

            bool mymovehappened = false;
            for (int movei = 0; movei < MyWeirdEngineMoveFinder.positionstack[0].movelist_totalfound; movei++)
            {
                int pti = MyWeirdEngineMoveFinder.pieceTypeIndex(MyWeirdEngineMoveFinder.positionstack[0].movelist[movei].MovingPiece);
                if (MyWeirdEngineMoveFinder.piecetypes[pti].name == "Pawn")
                {
                    if (MyWeirdEngineMoveFinder.positionstack[0].movelist[movei].coordinates[0] == pi1 &
                        MyWeirdEngineMoveFinder.positionstack[0].movelist[movei].coordinates[1] == pj1 &
                        MyWeirdEngineMoveFinder.positionstack[0].movelist[movei].coordinates[2] == pi2 &
                        MyWeirdEngineMoveFinder.positionstack[0].movelist[movei].coordinates[3] == pj2)
                    {
                        if (MyWeirdEngineMoveFinder.positionstack[0].movelist[movei].PromoteToPiece != 0)
                        {
                            int ptp = MyWeirdEngineMoveFinder.pieceTypeIndex(MyWeirdEngineMoveFinder.positionstack[0].movelist[movei].PromoteToPiece);
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
        public void TestNoPromote(string ppath, string ppositionfilename)
        {
            MyWeirdEngineJson.LoadPositionJson(ppath, ppositionfilename);
            MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "positions_verify\\", ppositionfilename);
            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_tree(1);

            bool promotehappened = false;
            for (int movei = 0; movei < MyWeirdEngineMoveFinder.positionstack[0].movelist_totalfound; movei++)
            {
                if (MyWeirdEngineMoveFinder.positionstack[0].movelist[movei].PromoteToPiece != 0)
                {
                    promotehappened = true;
                }
            }
            if (promotehappened == true)
            {
                MessageBox.Show(ppositionfilename + " promotion did happen but was not expected");
                AllTestsPassed = false;
            }
        }
        public void TestSelfCheck(string ppath, string ppositionfilename)
        {
            MyWeirdEngineJson.LoadPositionJson(ppath, ppositionfilename);
            MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "positions_verify\\", ppositionfilename);
            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_tree(0);

            if (MyWeirdEngineMoveFinder.POKingIsInCheck(ref MyWeirdEngineMoveFinder.positionstack[0]) == true)
            {
                //nothing
            }
            else
            {
                MessageBox.Show(ppositionfilename + " Check expected but there was no check.");
                AllTestsPassed = false;
            }
        }
        public void TestCheck(string ppath, string ppositionfilename)
        {
            MyWeirdEngineJson.LoadPositionJson(ppath, ppositionfilename);
            MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "positions_verify\\", ppositionfilename);
            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_tree(1);

            if (MyWeirdEngineMoveFinder.PMKingIsInCheck(ref MyWeirdEngineMoveFinder.positionstack[0]) == true)
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
            MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "positions_verify\\", ppositionfilename);
            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_tree(1);

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
            MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "positions_verify\\", ppositionfilename);
            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_tree(1);

            if (a.posvalue == 100 & MyWeirdEngineMoveFinder.positionstack[0].colourtomove == -1)
            {
                //nothing
            }
            else if (a.posvalue == -100 & MyWeirdEngineMoveFinder.positionstack[0].colourtomove == 1)
            {
                //nothing
            }
            else
            {
                MessageBox.Show(ppositionfilename + " Mate expected but there was no mate.");
                AllTestsPassed = false;
            }
        }
        public void TestNoMate(string ppath, string ppositionfilename)
        {
            MyWeirdEngineJson.LoadPositionJson(ppath, ppositionfilename);
            MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "positions_verify\\", ppositionfilename);
            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_tree(1);

            if (a.posvalue == 100 & MyWeirdEngineMoveFinder.positionstack[0].colourtomove == -1)
            {
                MessageBox.Show(ppositionfilename + " No mate expected but there was mate.");
                AllTestsPassed = false;
            }
            else if (a.posvalue == -100 & MyWeirdEngineMoveFinder.positionstack[0].colourtomove == 1)
            {
                MessageBox.Show(ppositionfilename + " No mate expected but there was mate.");
                AllTestsPassed = false;
            }
            else
            {
                //nothing
            }
        }
        public void TestNoMate_n(string ppath, string ppositionfilename, int depth)
        {
            MyWeirdEngineJson.LoadPositionJson(ppath, ppositionfilename);
            MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "positions_verify\\", ppositionfilename);
            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_tree(depth);

            if (a.posvalue >= 100 - (depth * 0.1) || a.posvalue <= (depth * 0.1) - 100)
            {
                MessageBox.Show(ppositionfilename + " No mate expected but there was mate.");
                AllTestsPassed = false;
            }
        }
        public void TestMate_n(string ppath, string ppositionfilename, int mate_in_n, int pi1, int pj1, int pi2, int pj2)
        {
            int depth;
            if (mate_in_n > 0 & mate_in_n < 5)
            {
                depth = mate_in_n * 2;
            }
            else
            {
                depth = 4;
            }

            MyWeirdEngineJson.LoadPositionJson(ppath, ppositionfilename);
            MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "positions_verify\\", ppositionfilename);
            
            DateTime startdatetime = DateTime.Now;
            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_tree(depth);
            DateTime enddatetime = DateTime.Now;

            int secondsneeded = (int)(enddatetime - startdatetime).TotalSeconds;
            if (depth < 5 & secondsneeded > 15)
            {
                MessageBox.Show(ppositionfilename + " Performance of calculation under acceptable levels.");
                AllTestsPassed = false;
            }

            if (pi1 != -1)
            {
                if (MyWeirdEngineMoveFinder.positionstack[0].movelist[a.moveidx].coordinates[0] == pi1 &
                    MyWeirdEngineMoveFinder.positionstack[0].movelist[a.moveidx].coordinates[1] == pj1 &
                    MyWeirdEngineMoveFinder.positionstack[0].movelist[a.moveidx].coordinates[2] == pi2 &
                    MyWeirdEngineMoveFinder.positionstack[0].movelist[a.moveidx].coordinates[3] == pj2)
                {
                    //nothing
                }
                else
                {
                    MessageBox.Show(ppositionfilename + " Mate expected, but the identified move is not correct.");
                    AllTestsPassed = false;
                }
            }
            if (a.posvalue >= 100 - (depth * 0.1) & MyWeirdEngineMoveFinder.positionstack[0].colourtomove == 1)
            {
                //nothing
            }
            else if (a.posvalue <= (depth * 0.1) - 100 & MyWeirdEngineMoveFinder.positionstack[0].colourtomove == -1)
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
            int depth;
            if (stalemate_in_n > 0 & stalemate_in_n < 5)
            {
                depth = (stalemate_in_n * 2) + 1;
            }
            else
            {
                depth = 5;
            }

            MyWeirdEngineJson.LoadPositionJson(ppath, ppositionfilename);
            MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "positions_verify\\", ppositionfilename);

            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_tree(depth);

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
        public void TestDraw_n(string ppath, string ppositionfilename, int depth)
        {
            MyWeirdEngineJson.LoadPositionJson(ppath, ppositionfilename);
            MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "positions_verify\\", ppositionfilename);

            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_tree(depth);

            if (a.posvalue == 0)
            {
                //nothing
            }
            else
            {
                MessageBox.Show(ppositionfilename + " Forced draw expected, but there was no forced draw.");
                AllTestsPassed = false;
            }
        }
        public void BaselinePerformance(string ppath, string ppositionfilename, int depth, int baseline_seconds)
        {
            MyWeirdEngineJson.LoadPositionJson(ppath, ppositionfilename);
            MyWeirdEngineJson.SavePositionAsJson(MyWeirdEngineJson.jsonworkpath + "positions_verify\\", ppositionfilename);

            DateTime startdatetime = DateTime.Now;
            calculationresponse a = MyWeirdEngineMoveFinder.Calculation_tree(depth);
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
            MyWeirdEngineMoveFinder.setting_SearchForFastestMate = true;
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
            MyWeirdEngineMoveFinder.setting_SearchForFastestMate = false;
            TestMate_n(ppath, "06D_huntermate_3_white", 3, 2, 4, 1, 4);
            TestMate_n(ppath, "06D_huntermate_3_black", 3, 2, 3, 1, 3);
            MyWeirdEngineMoveFinder.setting_SearchForFastestMate = true;
            TestMate_n(ppath, "06D_huntermate_3_white", 3, 2, 4, 1, 4);
            TestMate_n(ppath, "06D_huntermate_3_black", 3, 2, 3, 1, 3);

            TestStalemate_n(ppath, "08A_stalemate_2_white", 2);
            TestStalemate_n(ppath, "08A_stalemate_2_black", 2);

            TestStalemate_n(ppath, "08B_insufficient_material_1", 14);
            TestStalemate_n(ppath, "08B_insufficient_material_2", 14);
            TestStalemate_n(ppath, "08B_insufficient_material_3", 14);
            TestStalemate_n(ppath, "08B_insufficient_material_4", 14);
            TestStalemate_n(ppath, "08B_insufficient_material_5", 14);
            TestStalemate_n(ppath, "08B_insufficient_material_6", 14);
            TestStalemate_n(ppath, "08B_insufficient_material_7", 14);
            TestStalemate_n(ppath, "08B_insufficient_material_8", 14);
            TestStalemate_n(ppath, "08B_insufficient_material_9a", 14);
            TestStalemate_n(ppath, "08B_insufficient_material_9b", 14);

            TestMate_n(ppath, "08C_sufficient_material_mate_1_white_01", 1, 3, 4, 2, 6);
            TestMate_n(ppath, "08C_sufficient_material_mate_1_white_02", 1, 3, 4, 1, 5);
            TestMate_n(ppath, "08C_sufficient_material_mate_1_white_03", 1, 3, 4, 1, 5);
            TestMate_n(ppath, "08C_sufficient_material_mate_2_white_01", 2, 4, 3, 3, 4);
            TestMate_n(ppath, "08C_sufficient_material_mate_1_black_01", 1, 3, 3, 2, 1);
            TestMate_n(ppath, "08C_sufficient_material_mate_1_black_02", 1, 3, 3, 1, 2);
            TestMate_n(ppath, "08C_sufficient_material_mate_1_black_03", 1, 3, 3, 1, 2);
            TestMate_n(ppath, "08C_sufficient_material_mate_2_black_01", 2, 4, 4, 3, 3);
            TestMate_n(ppath, "08C_sufficient_material_mate_2_white_02", 2, 5, 5, 6, 5);
            TestMate_n(ppath, "08C_sufficient_material_mate_2_black_02", 2, 5, 2, 6, 2);
            TestDraw_n(ppath, "08D_forced_draw_white_01", 7);
            TestDraw_n(ppath, "08D_forced_draw_black_01", 7);
            TestDraw_n(ppath, "08D_forced_draw_white_02", 6);
            TestDraw_n(ppath, "08D_forced_draw_black_02", 6);

            MyWeirdEngineMoveFinder.setting_SearchForFastestMate = false;
            BaselinePerformance(ppath, "07A_mate_4_white_BN", 8, 3);
            BaselinePerformance(ppath, "07A_mate_4_black_BN", 8, 3);
            MyWeirdEngineMoveFinder.setting_SearchForFastestMate = true;
            BaselinePerformance(ppath, "07A_mate_4_white_BN", 8, 5);
            BaselinePerformance(ppath, "07A_mate_4_black_BN", 8, 5);

            TestMove(ppath, "09A_witch_white", "Rook", 3, 1, 3, 3, true);
            TestMove(ppath, "09A_witch_white", "Rook", 3, 1, 3, 4, true);
            TestMove(ppath, "09A_witch_white", "Rook", 3, 1, 3, 5, true);
            TestMove(ppath, "09A_witch_white", "Rook", 3, 1, 6, 2, false);
            TestMove(ppath, "09A_witch_white", "Rook", 3, 1, 3, 7, true);
            TestMove(ppath, "09A_witch_white", "Witch", 4, 3, 2, 3, true);
            TestMove(ppath, "09A_witch_white", "Witch", 4, 3, 3, 3, false);
            TestMove(ppath, "09A_witch_white", "Witch", 4, 3, 1, 3, false);

            TestMove(ppath, "09A_witch_black", "Rook", 3, 6, 3, 3, true);
            TestMove(ppath, "09A_witch_black", "Rook", 3, 6, 3, 4, true);
            TestMove(ppath, "09A_witch_black", "Rook", 3, 6, 3, 5, true);
            TestMove(ppath, "09A_witch_black", "Rook", 3, 6, 6, 2, false);
            TestMove(ppath, "09A_witch_black", "Rook", 3, 6, 3, 7, true);
            TestMove(ppath, "09A_witch_black", "Witch", 4, 4, 2, 4, true);
            TestMove(ppath, "09A_witch_black", "Witch", 4, 4, 3, 4, false);
            TestMove(ppath, "09A_witch_black", "Witch", 4, 4, 1, 4, false);

            TestStalemate(ppath, "10A_timethief_stalemate_white");
            TestStalemate(ppath, "10A_timethief_stalemate_black");
            TestSelfCheck(ppath, "10A_timethief_check_white");
            TestSelfCheck(ppath, "10A_timethief_check_black");
            TestNoMate(ppath, "10B_timethief_capture_black");
            TestNoMate(ppath, "10B_timethief_capture_white");
            TestNoMate(ppath, "10B_timethief_king_takes_black");
            TestNoMate(ppath, "10B_timethief_king_takes_white");
            TestMate_n(ppath, "10C_timethief_mate_1_black", 1, 3, 6, 6, 3);
            TestMate_n(ppath, "10C_timethief_mate_1_white", 1, 3, 1, 6, 4);
            TestMate_n(ppath, "10D_timethief_mate_2_black", 2, 4, 5, 8, 1);
            TestMate_n(ppath, "10D_timethief_mate_2_white", 2, 4, 2, 8, 6);
            TestMate_n(ppath, "10E_timethief_mate_3_black", 3, 6, 2, 5, 0);
            TestMate_n(ppath, "10E_timethief_mate_3_white", 3, 6, 5, 5, 7);

            TestMove(ppath, "11A_Joker_pawn_ep_1", "Joker", 4, 4, 3, 5, true);
            TestMove(ppath, "11A_Joker_pawn_ep_2", "Joker", 4, 4, 3, 5, true);
            TestMove(ppath, "11A_Joker_pawn_ep_3", "Pawn", 4, 4, 3, 5, true);
            TestMove(ppath, "11A_Joker_pawn_ep_4", "Joker", 4, 3, 3, 2, true);
            TestMove(ppath, "11A_Joker_pawn_ep_5", "Joker", 4, 3, 3, 2, true);
            TestMove(ppath, "11A_Joker_pawn_ep_6", "Pawn", 4, 3, 3, 2, true);
            TestMove(ppath, "11B_Joker_pawn_2move_1", "Joker", 3, 1, 3, 3, true);
            TestMove(ppath, "11B_Joker_pawn_2move_2", "Joker", 3, 6, 3, 4, true);

            TestNoPromote(ppath, "11C_Joker_pawn_nopromote_1");
            TestNoPromote(ppath, "11C_Joker_pawn_nopromote_2");
            TestNoMate_n(ppath, "11D_Joker_after_promote_1", 2);
            TestNoMate_n(ppath, "11D_Joker_after_promote_2", 2);
            TestMate_n(ppath, "11D_Joker_imitating_Joker_1", 1, 3, 0, 3, 7);
            TestMate_n(ppath, "11D_Joker_imitating_Joker_2", 1, 3, 7, 3, 0);
            TestMate_n(ppath, "11E_Joker_Hunter_1", 1, 4, 6, 5, 6);
            TestMate_n(ppath, "11E_Joker_Hunter_2", 1, 4, 1, 5, 1);

            if (AllTestsPassed == true)
            {
                MessageBox.Show("All unittests passed");
            }
            else
            {
                MessageBox.Show("Some unittests failed");
            }
        }
        public void RunNewUnittests(string ppath)
        {
            AllTestsPassed = true;
            MyWeirdEngineMoveFinder.setting_SearchForFastestMate = true;
            MessageBox.Show("Start with running new unittests");

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
