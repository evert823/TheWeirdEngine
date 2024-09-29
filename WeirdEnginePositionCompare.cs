using System;
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
    public class WeirdEnginePositionCompare
    {
        //Compare positions
        //Create hash
        //Transposition table
        //threefold repetition
        public WeirdEngineMoveFinder MyWeirdEngineMoveFinder;
        public WeirdEnginePositionCompare(WeirdEngineMoveFinder pWeirdEngineMoveFinder)
        {
            this.MyWeirdEngineMoveFinder = pWeirdEngineMoveFinder;
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
        public void TestCompare(int posidx)
        {
            for (int p1 = 0;p1 <= posidx; p1++)
            {
                for (int p2 = 0; p2 <= posidx; p2++)
                {
                    if (p1 != p2)
                    {
                        bool a = this.PositionsAreEqual(MyWeirdEngineMoveFinder.positionstack[p1],
                            MyWeirdEngineMoveFinder.positionstack[p2]);
                        if (a == true)
                        {
                            MessageBox.Show("Positions " + p1.ToString() + "," + p2.ToString() + " are equal");
                        }
                    }
                }
            }
        }
    }
}
