using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TheWeirdEngine
{
    public struct searchresult
    {
        public int naivematch;
        public int prio_ordermatch;
        public int reusematch;
    }
    public struct TransTableItem
    {
        public chessposition t_position;
        public int used_depth;
        public double used_alpha;
        public double used_beta;
        public double calculated_value;
        public chessmove bestmove;
    }
    public class WeirdEnginePositionCompare
    {
        //Compare positions
        //Create hash
        //Transposition table
        //threefold repetition
        public WeirdEngineMoveFinder MyWeirdEngineMoveFinder;
        public TransTableItem[] TransTable;
        public int TransTable_no_items_allocated;//memory allocated
        public int TransTable_no_items_available;//functionally available
        public int dumbcursor;
        public int TransTable_no_positions_reused;
        public WeirdEnginePositionCompare(WeirdEngineMoveFinder pWeirdEngineMoveFinder)
        {
            this.MyWeirdEngineMoveFinder = pWeirdEngineMoveFinder;
            TransTable_no_items_allocated = 100000;
            dumbcursor = 0;
        }
        public chessposition New_chessposition(int pboardwidth, int pboardheight)
        {
            chessposition topos = new chessposition();
            topos.boardwidth = pboardwidth;
            topos.boardheight = pboardheight;
            topos.squares = null;
            topos.squares = new int[pboardwidth, pboardheight];
            topos.squareInfo = null;
            topos.precedingmove = null;
            topos.precedingmove = new int[4] { -1, -1, -1, -1 };
            topos.WhiteJokerSubstitute_pti = -1;
            topos.BlackJokerSubstitute_pti = -1;
            topos.WhiteElfMoveType = MoveType.other;
            topos.BlackElfMoveType = MoveType.other;
            topos.movelist_totalfound = 0;
            return topos;
        }
        public void AllocateTransTableItem(ref TransTableItem ttitem, int pboardwidth, int pboardheight)
        {
            ttitem.t_position = New_chessposition(pboardwidth, pboardheight);

            ttitem.used_depth = 0;
            ttitem.used_alpha = 0;
            ttitem.used_beta = 0;
            ttitem.calculated_value = 0;

            ttitem.bestmove.MovingPiece = 0;
            ttitem.bestmove.coordinates = null;
            ttitem.bestmove.coordinates = new int[4] { 0, 0, 0, 0 };
            ttitem.bestmove.IsEnPassant = false;
            ttitem.bestmove.IsCapture = false;
            ttitem.bestmove.IsCastling = false;
            ttitem.bestmove.othercoordinates = null;
            ttitem.bestmove.othercoordinates = new int[4] { -1, -1, -1, -1 };
            ttitem.bestmove.PromoteToPiece = 0;
        }
        public void AllocateTransTable()
        {
            TransTable = null;
            TransTable = new TransTableItem[TransTable_no_items_allocated];

            int w = MyWeirdEngineMoveFinder.positionstack[0].boardwidth;
            int h = MyWeirdEngineMoveFinder.positionstack[0].boardheight;

            for (int p = 0; p < TransTable_no_items_allocated; p++)
            {
                AllocateTransTableItem(ref TransTable[p], w, h);
            }
            TransTable_no_items_available = 0;
        }
        public void StorePosition(chessposition frompos, int t_naive_match,
                                                         chessmove mv,
                                                         int used_depth,
                                                         double used_alpha,
                                                         double used_beta,
                                                         double calculated_value)
        {
            
            if (t_naive_match > -1)
            {
                //causing trouble --> comparing now
                //StoreIntoTransTable(frompos, t_naive_match, mv,
                //                    used_depth, used_alpha, used_beta, calculated_value);
                //return;
            }
            if (TransTable_no_items_available < TransTable_no_items_allocated)
            {
                StoreIntoTransTable(frompos, TransTable_no_items_available, mv,
                                    used_depth, used_alpha, used_beta, calculated_value);
                TransTable_no_items_available += 1;
                return;
            }
            if (dumbcursor >= TransTable_no_items_allocated)
            {
                dumbcursor = 0;
            }
            StoreIntoTransTable(frompos, dumbcursor, mv,
                                    used_depth, used_alpha, used_beta, calculated_value);
            dumbcursor += 1;
        }
        public void StoreIntoTransTable(chessposition frompos, int itemidx, chessmove mv,
                                                               int used_depth,
                                                               double used_alpha,
                                                               double used_beta,
                                                               double calculated_value)
        {
            TransTable[itemidx].t_position.boardwidth = frompos.boardwidth;
            TransTable[itemidx].t_position.boardheight = frompos.boardheight;
            TransTable[itemidx].t_position.colourtomove = frompos.colourtomove;

            for (int ci = 0;ci < 4;ci ++)
            {
                TransTable[itemidx].t_position.precedingmove[ci] = frompos.precedingmove[ci];
            }

            TransTable[itemidx].t_position.whitekinghasmoved = frompos.whitekinghasmoved;
            TransTable[itemidx].t_position.whitekingsiderookhasmoved = frompos.whitekingsiderookhasmoved;
            TransTable[itemidx].t_position.whitequeensiderookhasmoved = frompos.whitequeensiderookhasmoved;
            TransTable[itemidx].t_position.blackkinghasmoved = frompos.blackkinghasmoved;
            TransTable[itemidx].t_position.blackkingsiderookhasmoved = frompos.blackkingsiderookhasmoved;
            TransTable[itemidx].t_position.blackqueensiderookhasmoved = frompos.blackqueensiderookhasmoved;
            TransTable[itemidx].t_position.WhiteJokerSubstitute_pti = frompos.WhiteJokerSubstitute_pti;
            TransTable[itemidx].t_position.BlackJokerSubstitute_pti = frompos.BlackJokerSubstitute_pti;
            TransTable[itemidx].t_position.WhiteElfMoveType = frompos.WhiteElfMoveType;
            TransTable[itemidx].t_position.BlackElfMoveType = frompos.BlackElfMoveType;

            for (int i = 0; i < frompos.boardwidth; i++)
            {
                for (int j = 0; j < frompos.boardheight; j++)
                {
                    TransTable[itemidx].t_position.squares[i, j] = frompos.squares[i, j];
                }
            }
            MyWeirdEngineMoveFinder.SynchronizeChessmove(mv, ref TransTable[itemidx].bestmove);
            TransTable[itemidx].used_depth = used_depth;
            TransTable[itemidx].used_alpha = used_alpha;
            TransTable[itemidx].used_beta = used_beta;
            TransTable[itemidx].calculated_value = calculated_value;
        }
        public bool alpha_beta_compatible(int p, double current_alpha, double current_beta)
        {
            //Score was fail high or lowerbound
            if (TransTable[p].used_beta < TransTable[p].calculated_value &
                current_beta < TransTable[p].calculated_value)
            {
                return true;
            }
            //Score was fail low or upperbound
            if (TransTable[p].used_alpha > TransTable[p].calculated_value &
                current_alpha > TransTable[p].calculated_value)
            {
                return true;
            }
            //Score was exact
            if (TransTable[p].used_alpha <= TransTable[p].calculated_value &
                current_alpha <= TransTable[p].calculated_value &
                TransTable[p].used_beta >= TransTable[p].calculated_value &
                current_beta >= TransTable[p].calculated_value)
            {
                return true;
            }
            return false;
        }
        public searchresult SearchTransTable(chessposition pposition, int requested_depth,
                                             double current_alpha, double current_beta)
        {
            searchresult myresult = new searchresult();
            myresult.naivematch = -1;
            myresult.prio_ordermatch = -1;
            myresult.reusematch = -1;
            for (int p = 0; p < TransTable_no_items_available; p++)
            {
                if (PositionsAreEqual(pposition, TransTable[p].t_position))
                {
                    myresult.naivematch = p;
                    if (TransTable[p].used_depth > MyWeirdEngineMoveFinder.myenginesettings.presort_using_depth
                        & alpha_beta_compatible(p, current_alpha, current_beta) == true)
                    {
                        if (myresult.prio_ordermatch > -1)
                        {
                            if (TransTable[p].used_depth > TransTable[myresult.prio_ordermatch].used_depth)
                            {
                                myresult.prio_ordermatch = p;
                            }
                        }
                        else { myresult.prio_ordermatch = p; }
                    }
                    if (TransTable[p].used_depth >= requested_depth)
                    {
                        if (alpha_beta_compatible(p, current_alpha, current_beta) == true)
                        {
                            myresult.reusematch = p;
                            return myresult;
                        }
                    }
                }
            }
            return myresult;
        }
        public bool PotentialEnPassant(chessposition pposition)
        {
            int x_from = pposition.precedingmove[0];
            if (x_from == -1) { return false; }
            int y_from = pposition.precedingmove[1];
            int x_to = pposition.precedingmove[2];
            int y_to = pposition.precedingmove[3];
            int ptm = MyWeirdEngineMoveFinder.pieceTypeIndex(pposition.squares[x_to, y_to]);

            if (MyWeirdEngineMoveFinder.piecetypes[ptm].SpecialPiece_ind == SpecialPiece.Joker)
            {
                ptm = MyWeirdEngineMoveFinder.jokersubspti(ref pposition, x_to, y_to, ptm);
            }
            if (MyWeirdEngineMoveFinder.piecetypes[ptm].SpecialPiece_ind != SpecialPiece.Pawn)
            {
                return false;
            }
            int d = Math.Abs(y_to - y_from);
            if (d == 2) { return true; }
            return false;
        }
        public bool PositionsAreEqual(chessposition posA, chessposition posB)
        {
            //Error margin is accepted here w.r.t. en passant and Time Thief capture
            //Some positions are flagged as NOT equal while they are equal upon closer examination
            //But flagged equal here guarantees that they were really equal
            if (posA.colourtomove != posB.colourtomove) { return false; }
            if (posA.boardwidth != posB.boardwidth) { return false; }
            if (posA.boardheight != posB.boardheight) { return false; }
            if (posA.whitekinghasmoved != posB.whitekinghasmoved) { return false; }
            if (posA.whitekingsiderookhasmoved != posB.whitekingsiderookhasmoved) { return false; }
            if (posA.whitequeensiderookhasmoved != posB.whitequeensiderookhasmoved) { return false; }
            if (posA.blackkinghasmoved != posB.blackkinghasmoved) { return false; }
            if (posA.blackkingsiderookhasmoved != posB.blackkingsiderookhasmoved) { return false; }
            if (posA.blackqueensiderookhasmoved != posB.blackqueensiderookhasmoved) { return false; }
            if (posA.WhiteJokerSubstitute_pti != posB.WhiteJokerSubstitute_pti) { return false; }
            if (posA.BlackJokerSubstitute_pti != posB.BlackJokerSubstitute_pti) { return false; }
            if (posA.WhiteElfMoveType != posB.WhiteElfMoveType) { return false; }
            if (posA.BlackElfMoveType != posB.BlackElfMoveType) { return false; }

            for (int i = 0; i < posA.boardwidth; i++)
            {
                for (int j = 0; j < posA.boardheight; j++)
                {
                    if (posA.squares[i, j] != posB.squares[i, j]) { return false; }

                    if ((posA.colourtomove > 0 & posA.squares[i, j] > 0) ||
                        (posA.colourtomove < 0 & posA.squares[i, j] < 0))
                    {
                        int pti = MyWeirdEngineMoveFinder.pieceTypeIndex(posA.squares[i, j]);
                        if (MyWeirdEngineMoveFinder.piecetypes[pti].SpecialPiece_ind == SpecialPiece.TimeThief)
                        {
                            if (posA.precedingmove[0] != -1 || posB.precedingmove[0] != -1)
                            {
                                //Potential Time Thief capture so ALWAYS mark posA and posB unequal
                                return false;
                            }
                        }
                    }
                }
            }
            bool ep_A = PotentialEnPassant(posA);
            bool ep_B = PotentialEnPassant(posB);
            if (ep_A != ep_B) { return false; }
            if (ep_A == true & ep_B == true)
            {
                for (int ci = 0;ci < 4;ci ++)
                {
                    if (posA.precedingmove[ci] != posB.precedingmove[ci]) { return false; }
                }
            }

            return true;
        }
        public void InitRepetitionCounter()
        {
            for (int p = 0; p < WeirdEngineMoveFinder.positionstack_size; p++)
            {
                MyWeirdEngineMoveFinder.positionstack[p].RepetitionCounter = 0;
            }
        }
        public void SetRepetitionCounter(int posidx)
        {
            MyWeirdEngineMoveFinder.positionstack[posidx].RepetitionCounter = 0;
            for (int p = 0; p < posidx; p++)
            {
                bool eq = this.PositionsAreEqual(MyWeirdEngineMoveFinder.positionstack[p],
                    MyWeirdEngineMoveFinder.positionstack[posidx]);
                if (eq == true)
                {
                    MyWeirdEngineMoveFinder.positionstack[posidx].RepetitionCounter =
                        MyWeirdEngineMoveFinder.positionstack[p].RepetitionCounter + 1;
                }
            }
            if (MyWeirdEngineMoveFinder.positionstack[posidx].RepetitionCounter == 0)
            {
                MyWeirdEngineMoveFinder.positionstack[posidx].RepetitionCounter = 1;
            }
        }
        public bool MovesAreEqual(chessmove moveA, chessmove moveB)
        {
            if (moveA.MovingPiece != moveB.MovingPiece) { return false; }
            if (moveA.coordinates[0] != moveB.coordinates[0]) { return false; }
            if (moveA.coordinates[1] != moveB.coordinates[1]) { return false; }
            if (moveA.coordinates[2] != moveB.coordinates[2]) { return false; }
            if (moveA.coordinates[3] != moveB.coordinates[3]) { return false; }
            if (moveA.IsEnPassant != moveB.IsEnPassant) { return false; }
            if (moveA.IsCapture != moveB.IsCapture) { return false; }
            if (moveA.IsCastling != moveB.IsCastling) { return false; }
            if (moveA.othercoordinates[0] != moveB.othercoordinates[0]) { return false; }
            if (moveA.othercoordinates[1] != moveB.othercoordinates[1]) { return false; }
            if (moveA.othercoordinates[2] != moveB.othercoordinates[2]) { return false; }
            if (moveA.othercoordinates[3] != moveB.othercoordinates[3]) { return false; }
            if (moveA.PromoteToPiece != moveB.PromoteToPiece) { return false; }
            return true;
        }
        public void TestItemsIntoTransTable()
        {
            TransTable[0].t_position.squares[1, 1] = MyWeirdEngineMoveFinder.MyWeirdEngineJson.Str2PieceType("R");
            TransTable[0].t_position.squares[2, 3] = MyWeirdEngineMoveFinder.MyWeirdEngineJson.Str2PieceType("K");
            TransTable[0].t_position.squares[0, 5] = MyWeirdEngineMoveFinder.MyWeirdEngineJson.Str2PieceType("-K");
            TransTable[0].t_position.colourtomove = 1;
            TransTable[0].t_position.whitekinghasmoved = true;
            TransTable[0].t_position.blackkinghasmoved = true;
            TransTable[0].used_depth = 8;
            TransTable[0].used_alpha = -100;
            TransTable[0].used_beta = 100;
            TransTable[0].calculated_value = 99.3;
            TransTable[0].bestmove.MovingPiece = MyWeirdEngineMoveFinder.MyWeirdEngineJson.Str2PieceType("K");
            TransTable[0].bestmove.coordinates[0] = 2;
            TransTable[0].bestmove.coordinates[1] = 3;
            TransTable[0].bestmove.coordinates[2] = 2;
            TransTable[0].bestmove.coordinates[3] = 4;
            TransTable_no_items_available = 1;
        }
        public void SanityCheck()
        {
            int dupfound = 0;
            int exampledisplayed = 0;
            for (int p1 = 0;p1 < TransTable_no_items_available - 1; p1++)
            {
                for (int p2 = p1 + 1; p2 < TransTable_no_items_available; p2++)
                {
                    if (PositionsAreEqual(TransTable[p1].t_position, TransTable[p2].t_position))
                    {
                        if (TransTable[p1].calculated_value != 0 || TransTable[p2].calculated_value != 0)
                        {
                            if (TransTable[p1].calculated_value == 0 || TransTable[p2].calculated_value == 0)
                            {
                                if (exampledisplayed < 3)
                                {
                                    MessageBox.Show("p1 " + p1.ToString() + " p2 " + p2.ToString());
                                    exampledisplayed++;
                                }
                                dupfound++;
                            }
                        }
                    }
                }
            }
            MessageBox.Show("dupfound " + dupfound.ToString());
        }
    }
}
