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
        public void Test()
        {
            MessageBox.Show("presort_when_depth_gt : " + MyWeirdEngineMoveFinder.presort_when_depth_gt.ToString());
        }


        public int DistanceBetweenSquares(int i1, int j1, int i2, int j2)
        {
            int d = Math.Abs(i1 - i2);
            d += Math.Abs(j1 - j2);
            return d;
        }
        public cornerInfo CheckOneCorner(ref chessposition pposition, int i, int j, vector targetkingcoord)
        {
            cornerInfo myresult;
            myresult.cornercoord.x = i;
            myresult.cornercoord.y = j;
            myresult.DistanceToKing = DistanceBetweenSquares(i, j, targetkingcoord.x, targetkingcoord.y);
            myresult.BishopCanAttack = false;
            if (pposition.WhiteBareKing == true)
            {
                if (MyWeirdEngineMoveFinder.IsWhiteSquare(i, j) == true)
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
                if (MyWeirdEngineMoveFinder.IsWhiteSquare(i, j) == true)
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
        public cornerInfo PickCorner(ref chessposition pposition, vector targetkingcoord)
        {
            //Output: x, y, distance, bishop-aligned

            int bestci;

            cornerInfo myresult;

            cornerInfo[] AllCorners = new cornerInfo[4];
            AllCorners[0] = CheckOneCorner(ref pposition, 0, 0, targetkingcoord);
            AllCorners[1] = CheckOneCorner(ref pposition, pposition.boardwidth - 1, 0, targetkingcoord);
            AllCorners[2] = CheckOneCorner(ref pposition, 0, pposition.boardheight - 1, targetkingcoord);
            AllCorners[3] = CheckOneCorner(ref pposition, pposition.boardwidth - 1, pposition.boardheight - 1, targetkingcoord);

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
        public double MateBareKing(ref chessposition pposition)
        {
            //Handle the position where one has bare King and the other has mating material
            //(This is already validated before we enter here)
            vector targetkingcoord;
            int sumofsquareddistances;
            int numberofchasingpieces;
            int d;
            double AvgD;
            double AvgD2;

            if (pposition.WhiteBareKing == true)
            {
                targetkingcoord = pposition.whitekingcoord;
            }
            else
            {
                targetkingcoord = pposition.blackkingcoord;
            }

            cornerInfo bestcorner = PickCorner(ref pposition, targetkingcoord);

            sumofsquareddistances = 0;
            numberofchasingpieces = 0;
            for (int i = 0; i < pposition.boardwidth; i++)
            {
                for (int j = 0; j < pposition.boardheight; j++)
                {
                    if (pposition.squares[i, j] != 0)
                    {
                        if ((pposition.WhiteBareKing == true & pposition.squares[i, j] < 0) ||
                            (pposition.BlackBareKing == true & pposition.squares[i, j] > 0))
                        {
                            d = DistanceBetweenSquares(i, j, targetkingcoord.x, targetkingcoord.y);
                            numberofchasingpieces += 1;
                            sumofsquareddistances += (d * d);
                        }
                    }
                }
            }
            AvgD = sumofsquareddistances / numberofchasingpieces;
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
