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
    public struct cornerInfo
    {
        public vector cornercoord;
        public int DistanceToKing;
        public bool BishopCanAttack;
    }
    public class WeirdEngineBareKingMate
    {

        //Handle the position where one has bare King and the other has mating material
        //(This is already validated before we enter here)

        public WeirdEngineMoveFinder MyWeirdEngineMoveFinder;
        public WeirdEngineBareKingMate(WeirdEngineMoveFinder pWeirdEngineMoveFinder)
        {
            this.MyWeirdEngineMoveFinder = pWeirdEngineMoveFinder;
        }
        public cornerInfo CheckOneCorner(chessposition pposition, int i, int j, vector targetkingcoord)
        {
            cornerInfo myresult;
            myresult.cornercoord.x = i;
            myresult.cornercoord.y = j;
            myresult.DistanceToKing = MyWeirdEngineMoveFinder.MyWeirdEngineEval.DistanceBetweenSquares(i, j, targetkingcoord.x, targetkingcoord.y);
            myresult.BishopCanAttack = false;
            if (pposition.WhiteBareKing == true)
            {
                if (MyWeirdEngineMoveFinder.MyBoardTopology.IsWhiteSquare[i, j] == true)
                {
                    if (pposition.BlackBishoponWhite == true) { myresult.BishopCanAttack = true; }
                }
                else
                {
                    if (pposition.BlackBishoponBlack == true) { myresult.BishopCanAttack = true; }
                }
            }
            else
            {
                if (MyWeirdEngineMoveFinder.MyBoardTopology.IsWhiteSquare[i, j] == true)
                {
                    if (pposition.WhiteBishoponWhite == true) { myresult.BishopCanAttack = true; }
                }
                else
                {
                    if (pposition.WhiteBishoponBlack == true) { myresult.BishopCanAttack = true; }
                }
            }
            return myresult;
        }
        public cornerInfo PickCorner(chessposition pposition, vector targetkingcoord)
        {
            //Output: x, y, distance, bishop-aligned

            int bestci;

            cornerInfo myresult;

            cornerInfo[] AllCorners = new cornerInfo[4];
            AllCorners[0] = CheckOneCorner(pposition, 0, 0, targetkingcoord);
            AllCorners[1] = CheckOneCorner(pposition, pposition.boardwidth - 1, 0, targetkingcoord);
            AllCorners[2] = CheckOneCorner(pposition, 0, pposition.boardheight - 1, targetkingcoord);
            AllCorners[3] = CheckOneCorner(pposition, pposition.boardwidth - 1, pposition.boardheight - 1, targetkingcoord);

            bestci = 0;
            for (int ci = 1;ci < 4;ci ++)
            {
                if (AllCorners[ci].BishopCanAttack == true & AllCorners[bestci].BishopCanAttack == false)
                {
                    bestci = ci;
                }
                else if (AllCorners[ci].BishopCanAttack == AllCorners[bestci].BishopCanAttack
                       & AllCorners[ci].DistanceToKing < AllCorners[bestci].DistanceToKing)
                {
                    bestci = ci;
                }
            }

            myresult = AllCorners[bestci];
            return myresult;
        }
        public double MateBareKing(chessposition pposition)
        {
            //Handle the position where one has bare King and the other has mating material
            //(This is already validated before we enter here)
            vector targetkingcoord;
            double AvgD;
            double AvgD2;

            if (pposition.WhiteBareKing == true)
            {
                targetkingcoord = pposition.whitekingcoord;
                AvgD = MyWeirdEngineMoveFinder.MyWeirdEngineEval.AvgSquaredDistanceToEnemyKing(pposition, -1);
            }
            else
            {
                targetkingcoord = pposition.blackkingcoord;
                AvgD = MyWeirdEngineMoveFinder.MyWeirdEngineEval.AvgSquaredDistanceToEnemyKing(pposition, 1);
            }

            cornerInfo bestcorner = PickCorner(pposition, targetkingcoord);

            AvgD2 = ((bestcorner.DistanceToKing * bestcorner.DistanceToKing) + AvgD) / 2;
            double MaxAvgD2 = ((double)pposition.boardheight * (double)pposition.boardheight) +
                              ((double)pposition.boardwidth * (double)pposition.boardwidth);

            double score = 95 - (AvgD2 * (15 / MaxAvgD2));
            if (score >= 94.9) { score = 94.9; }
            if (score <= 80.1) { score = 80.1; }

            if (pposition.BlackBareKing == true)
            {
                return score;
            }
            if (pposition.WhiteBareKing == true)
            {
                return -score;
            }
            return 0.0;
        }
    }
}
