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
    public class WeirdEngineEval
    {

        //Handle the position where one has bare King and the other has mating material
        //(This is already validated before we enter here)

        public WeirdEngineMoveFinder MyWeirdEngineMoveFinder;
        public WeirdEngineEval(WeirdEngineMoveFinder pWeirdEngineMoveFinder)
        {
            this.MyWeirdEngineMoveFinder = pWeirdEngineMoveFinder;
        }
        public bool DrawByMaterial(ref chessposition pposition)
        {
            if (pposition.WhiteBareKing == true & pposition.BlackBareKing == true) { return true; }
            //NOT FINISHED for now good enough to handle KBN vs K
            //Two bare Kings was already excluded earlier
            if (pposition.WhiteBareKing == false & pposition.BlackBareKing == false) { return false; }

            //Now exactly one of the players has bare King
            if (pposition.WhiteHasMatingMaterial == true || pposition.BlackHasMatingMaterial == true)
            {
                return false;
            }
            return true;
        }

        public int DistanceBetweenSquares(int i1, int j1, int i2, int j2)
        {
            return MyWeirdEngineMoveFinder.MyBoardTopology.DistanceBetweenSquares[i1, j1, i2, j2];
        }
        public double AvgSquaredDistanceToEnemyKing(chessposition pposition, int forcolour)
        {
            vector targetkingcoord;
            int sumofsquareddistances;
            int numberofchasingpieces;
            int d;

            if (forcolour == -1)
            {
                targetkingcoord = pposition.whitekingcoord;
            }
            else
            {
                targetkingcoord = pposition.blackkingcoord;
            }

            sumofsquareddistances = 0;
            numberofchasingpieces = 0;
            for (int i = 0; i < pposition.boardwidth; i++)
            {
                for (int j = 0; j < pposition.boardheight; j++)
                {
                    if (pposition.squares[i, j] != 0)
                    {
                        if ((forcolour == -1 & pposition.squares[i, j] < 0) ||
                            (forcolour == 1 & pposition.squares[i, j] > 0))
                        {
                            d = DistanceBetweenSquares(i, j, targetkingcoord.x, targetkingcoord.y);
                            numberofchasingpieces += 1;
                            sumofsquareddistances += (d * d);
                        }
                    }
                }
            }
            return sumofsquareddistances / numberofchasingpieces;
        }

        public double EvaluationByCentralization(chessposition pposition)
        {
            double resultev;
            double dw = MyWeirdEngineMoveFinder.MyWeirdEngineEval.AvgSquaredDistanceToEnemyKing(
                                                  MyWeirdEngineMoveFinder.positionstack[0], 1);
            double db = MyWeirdEngineMoveFinder.MyWeirdEngineEval.AvgSquaredDistanceToEnemyKing(
                                                  MyWeirdEngineMoveFinder.positionstack[0], -1);
            resultev = db - dw;
            if (resultev > 80)
            {
                return 80.0;
            }
            if (resultev < -80)
            {
                return -80.0;
            }
            return resultev;
        }

        public double EvaluationByMaterial(chessposition pposition)
        {
            double materialbalance = 0.0;

            for (int i = 0; i < pposition.boardwidth; i++)
            {
                for (int j = 0; j < pposition.boardheight; j++)
                {
                    if (pposition.squares[i, j] != 0)
                    {
                        int pti = MyWeirdEngineMoveFinder.pieceTypeIndex(pposition.squares[i, j]);
                        if (MyWeirdEngineMoveFinder.piecetypes[pti].SpecialPiece_ind == SpecialPiece.King)
                        {
                            //no action
                        }
                        else
                        {
                            if (pposition.squares[i, j] > 0)
                            {
                                materialbalance += MyWeirdEngineMoveFinder.piecetypes[pti].EstimatedValue;
                            }
                            else
                            {
                                materialbalance -= MyWeirdEngineMoveFinder.piecetypes[pti].EstimatedValue;
                            }
                        }
                    }
                }
            }
            if (materialbalance > 8)
            {
                return 80.0;
            }
            if (materialbalance < -8)
            {
                return -80.0;
            }
            return materialbalance * 10;
        }
        public double EvaluationByAttack(chessposition pposition)
        {
            int AttackedByWhitetotal = 0;
            int AttackedByBlacktotal = 0;

            for (int i = 0; i < pposition.boardwidth; i++)
            {
                for (int j = 0; j < pposition.boardheight; j++)
                {
                    if (pposition.colourtomove == 1)
                    {
                        AttackedByWhitetotal += pposition.squareInfo[i, j].AttackedByPM;
                        AttackedByBlacktotal += pposition.squareInfo[i, j].AttackedByPO;
                    }
                    else
                    {
                        AttackedByWhitetotal += pposition.squareInfo[i, j].AttackedByPO;
                        AttackedByBlacktotal += pposition.squareInfo[i, j].AttackedByPM;
                    }
                }
            }
            double resultev = AttackedByWhitetotal - AttackedByBlacktotal;

            //Assigning points for giving check did not help at all!!!

            if (resultev > 80)
            {
                return 80.0;
            }
            if (resultev < -80)
            {
                return -80.0;
            }
            return resultev;
        }
        public double StaticEvaluation(chessposition pposition)
        {
            //Minimum/maximum score for 'soft' results should be -80/80 respectively !!!
            double myev;

            if (pposition.WhiteBareKing == true & pposition.BlackHasMatingMaterial == true)
            {
                myev = MyWeirdEngineMoveFinder.MyWeirdEngineBareKingMate.MateBareKing(pposition);
                return myev;
            }
            else if (pposition.BlackBareKing == true & pposition.WhiteHasMatingMaterial == true)
            {
                myev = MyWeirdEngineMoveFinder.MyWeirdEngineBareKingMate.MateBareKing(pposition);
                return myev;
            }

            //myev = EvaluationByMaterial(pposition);
            myev = EvaluationByAttack(pposition);
            //myev += EvaluationByCentralization(pposition);
            //myev = myev / 2;

            return myev;
        }
    }
}
