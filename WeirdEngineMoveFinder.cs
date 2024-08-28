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
    public struct squareInfoItem
    {
        public bool AttackedByPM;
        public bool AttackedByPO;
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

    public struct chessmove
    {
        public int MovingPiece;
        public int[] coordinates;
        public bool IsEnPassant;
        public bool IsCapture;
        public bool IsCastling;
        //othercoordinates will give the Rook that co-moves with castling, or the pawn captured en passant
        public int[] othercoordinates;
        public int PromoteToPiece;
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
        public squareInfoItem[,] squareInfo;
        public vector whitekingcoord;
        public vector whitekingsiderookcoord;
        public vector whitequeensiderookcoord;
        public vector blackkingcoord;
        public vector blackkingsiderookcoord;
        public vector blackqueensiderookcoord;
        public int movelist_totalfound;
        public chessmove[] movelist;
    }

    public class WeirdEngineMoveFinder
    {
        public const int movelist_allocated = 500;
        public const int positionstack_size = 25;

        public int presort_when_n_plies_gt;
        public int presort_using_n_plies;
        public int display_when_n_plies_gt;
        public chesspiecetype[] piecetypes;
        public chessposition mainposition;
        public chessposition[] positionstack;
        public WeirdEngineMoveFinder()
        {
            this.presort_when_n_plies_gt = 7;
            this.presort_using_n_plies = 3;
            this.display_when_n_plies_gt = 8;
            this.mainposition = new chessposition();
            this.AllocateMovelist(ref this.mainposition);
        }
        public void ResetBoardsize(ref chessposition pposition, int pboardwidth, int pboardheight)
        {
            pposition.boardwidth = pboardwidth;
            pposition.boardheight = pboardheight;
            pposition.squares = null;
            pposition.squares = new int[pboardwidth, pboardheight];
            pposition.squareInfo = null;
            pposition.squareInfo = new squareInfoItem[pboardwidth, pboardheight];
            pposition.precedingmove = null;
            pposition.precedingmove = new int[4] { -1, -1, -1, -1 };
            this.ClearNonPersistent(ref pposition);
        }
        public void DisableCastling(ref chessposition pposition)
        {
            pposition.whitekinghasmoved = true;
            pposition.whitekingsiderookhasmoved = true;
            pposition.whitequeensiderookhasmoved = true;
            pposition.blackkinghasmoved = true;
            pposition.blackkingsiderookhasmoved = true;
            pposition.blackqueensiderookhasmoved = true;
        }
        public void ClearNonPersistent(ref chessposition pposition)
        {
            for (int i = 0;i < pposition.boardwidth;i++)
            {
                for (int j = 0; j < pposition.boardheight;j++)
                {
                    pposition.squareInfo[i, j].AttackedByPM = false;
                    pposition.squareInfo[i, j].AttackedByPO = false;
                }
            }
            pposition.whitekingcoord.x = -1;
            pposition.whitekingcoord.y = -1;
            pposition.whitekingsiderookcoord.x = -1;
            pposition.whitekingsiderookcoord.y = -1;
            pposition.whitequeensiderookcoord.x = -1;
            pposition.whitequeensiderookcoord.y = -1;
            pposition.blackkingcoord.x = -1;
            pposition.blackkingcoord.y = -1;
            pposition.blackkingsiderookcoord.x = -1;
            pposition.blackkingsiderookcoord.y = -1;
            pposition.blackqueensiderookcoord.x = -1;
            pposition.blackqueensiderookcoord.y = -1;
            pposition.movelist_totalfound = 0;
        }
        public void AllocateMovelist(ref chessposition pposition)
        {
            pposition.movelist = null;
            pposition.movelist = new chessmove[movelist_allocated];
            for (int mi = 0; mi < movelist_allocated; mi++)
            {
                pposition.movelist[mi].MovingPiece = 0;
                pposition.movelist[mi].coordinates = new int[4] { 0, 0, 0, 0};
                pposition.movelist[mi].IsEnPassant = false;
                pposition.movelist[mi].IsCapture = false;
                pposition.movelist[mi].IsCastling = false;
                pposition.movelist[mi].othercoordinates = new int[4] { -1, -1, -1, -1 };
                pposition.movelist[mi].PromoteToPiece = 0;
            }
        }
        public double PieceType2Value(int pti)
        {
            switch (this.piecetypes[pti].name)
            {
                case "King":
                    return 1000.0;
                case "Queen":
                    return 9.1;
                case "Rook":
                    return 5.0;
                case "Bishop":
                    return 3.01;
                case "Knight":
                    return 3.0;
                case "Pawn":
                    return 1.0;
                case "Archbishop":
                    return 8.3;
                case "Chancellor":
                    return 8.4;
                case "Guard":
                    return 4.0;
                case "Hunter":
                    return 3.9;
                default:
                    return 2.05;
            }
        }
        public int pieceTypeIndex(int psquare)
        {
            if (psquare > 0)
            {
                return psquare - 1;
            }
            if (psquare < 0)
            {
                return (psquare * -1) - 1;
            }
            return -1;
        }
        public void LocateKingsRooks(ref chessposition pposition)
        {
            //If we go from left to right then we should find queensiderooks first
            for (int i = 0; i < pposition.boardwidth; i++)
            {
                for (int j = 0; j < pposition.boardheight; j++)
                {
                    if (pposition.squares[i, j] != 0)
                    {
                        int pti = this.pieceTypeIndex(pposition.squares[i, j]);
                        if (this.piecetypes[pti].name == "King" & this.piecetypes[pti].IsRoyal == true)
                        {
                            if (pposition.squares[i, j] > 0)
                            {
                                pposition.whitekingcoord.x = i;
                                pposition.whitekingcoord.y = j;
                            }
                            else
                            {
                                pposition.blackkingcoord.x = i;
                                pposition.blackkingcoord.y = j;
                            }
                        }
                        if (this.piecetypes[pti].name == "Rook")
                        {
                            if (pposition.squares[i, j] > 0)
                            {
                                if (pposition.whitequeensiderookcoord.x == -1 & pposition.whitekingcoord.x == -1)
                                {
                                    pposition.whitequeensiderookcoord.x = i;
                                    pposition.whitequeensiderookcoord.y = j;
                                }
                                else
                                {
                                    pposition.whitekingsiderookcoord.x = i;
                                    pposition.whitekingsiderookcoord.y = j;
                                }
                            }
                            else
                            {
                                if (pposition.blackqueensiderookcoord.x == -1 & pposition.blackkingcoord.x == -1)
                                {
                                    pposition.blackqueensiderookcoord.x = i;
                                    pposition.blackqueensiderookcoord.y = j;
                                }
                                else
                                {
                                    pposition.blackkingsiderookcoord.x = i;
                                    pposition.blackkingsiderookcoord.y = j;
                                }
                            }

                        }
                    }
                }
            }

        }
        public double StaticEvaluation(ref chessposition pposition)
        {
            if (pposition.whitekingcoord.x == -1 & pposition.blackkingcoord.x == -1)
            {
                return -100.0 * pposition.colourtomove;
            }
            if (pposition.whitekingcoord.x == -1)
            {
                return -100.0;
            }
            if (pposition.blackkingcoord.x == -1)
            {
                return 100.0;
            }

            double materialbalance = 0.0;
            double myresult = 0.0;

            for (int i = 0; i < pposition.boardwidth; i++)
            {
                for (int j = 0; j < pposition.boardheight; j++)
                {
                    if (pposition.squares[i, j] != 0)
                    {
                        int pti = this.pieceTypeIndex(pposition.squares[i, j]);
                        if (this.piecetypes[pti].name == "King" & this.piecetypes[pti].IsRoyal == true)
                        {
                            //no action
                        }
                        else
                        {
                            if (pposition.squares[i, j] > 0)
                            {
                                materialbalance += this.PieceType2Value(pti);
                            }
                            else
                            {
                                materialbalance -= this.PieceType2Value(pti);
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
        public void init_positionstack()
        {
            this.positionstack = null;
            this.positionstack = new chessposition[positionstack_size];
            for (int pi = 0; pi < positionstack_size; pi++)
            {
                this.ResetBoardsize(ref this.positionstack[pi],
                                    this.mainposition.boardwidth,
                                    this.mainposition.boardheight);
                this.AllocateMovelist(ref this.positionstack[pi]);
            }
            this.SynchronizePosition(ref this.mainposition, ref this.positionstack[0]);
            for (int ci = 0;ci < 4; ci++)
            {
                this.positionstack[0].precedingmove[ci] = this.mainposition.precedingmove[ci];
            }
        }
        public void SynchronizePosition(ref chessposition frompos, ref chessposition topos)
        {
            //boardwidth MUST already be in sync
            //boardheight MUST already be in sync
            topos.colourtomove = frompos.colourtomove;
            //topos.precedingmove = frompos.precedingmove SKIPPED
            topos.whitekinghasmoved = frompos.whitekinghasmoved;
            topos.whitekingsiderookhasmoved = frompos.whitekingsiderookhasmoved;
            topos.whitequeensiderookhasmoved = frompos.whitequeensiderookhasmoved;
            topos.blackkinghasmoved = frompos.blackkinghasmoved;
            topos.blackkingsiderookhasmoved = frompos.blackkingsiderookhasmoved;
            topos.blackqueensiderookhasmoved = frompos.blackqueensiderookhasmoved;

            for (int i = 0; i < frompos.boardwidth; i++)
            {
                for (int j = 0; j < frompos.boardheight; j++)
                {
                    topos.squares[i, j] = frompos.squares[i, j];
                }
            }
            this.ClearNonPersistent(ref topos);
        }
    }
}
