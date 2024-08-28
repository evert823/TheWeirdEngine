using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TheWeirdEngine
{
    //In WeirdEngineMoveFinder we re-implement a python code for a chess variant engine,
    //which eventually replaces the old WeirdEngineBackend
    public struct vector
    {
        public int x;
        public int y;
    }
    public struct chesspiecetype
    {
        public string symbol;
        public string name;
        public bool IsRoyal;
        public bool IsDivergent;
        public vector[] stepleapmovevectors;
        public vector[] slidemovevectors;
        public vector[] stepleapcapturevectors;
        public vector[] slidecapturevectors;
    }

    public struct chessposition
    {
        public int boardwidth;
        public int boardheight;
        public int colourtomove;
        public int[] precedingmove;
        public bool whitekinghasmoved;
        public bool whitekingsiderookhasmoved;
        public bool whitequeensiderookhasmoved;
        public bool blackkinghasmoved;
        public bool blackkingsiderookhasmoved;
        public bool blackqueensiderookhasmoved;
        public int[,] squares;//python square[j][i] becomes C# square[i, j]
    }

    public class WeirdEngineMoveFinder
    {
        public chesspiecetype[] piecetypes;
        public chessposition mainposition;
        public WeirdEngineMoveFinder()
        {
            this.mainposition = new chessposition();
        }
        public void ResetBoardsize(int pboardwidth, int pboardheight)
        {
            this.mainposition.boardwidth = pboardwidth;
            this.mainposition.boardheight = pboardheight;
            this.mainposition.squares = null;
            this.mainposition.squares = new int[pboardwidth, pboardheight];
        }
        public void DisableCastling()
        {
            this.mainposition.whitekinghasmoved = true;
            this.mainposition.whitekingsiderookhasmoved = true;
            this.mainposition.whitequeensiderookhasmoved = true;
            this.mainposition.blackkinghasmoved = true;
            this.mainposition.blackkingsiderookhasmoved = true;
            this.mainposition.blackqueensiderookhasmoved = true;
        }
    }
}
