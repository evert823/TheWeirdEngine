using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.ComponentModel.Com2Interop;

namespace TheWeirdEngine
{
    //In WeirdEngineMoveFinder we re-implement a python code for a chess variant engine,
    //which eventually replaces the old WeirdEngineBackend

    public struct movePrioItem
    {
        public int moveidx;
        public double movevalue;
    }
    public struct calculationresponse
    {
        public double posvalue;
        public int moveidx;
        public bool POKingIsInCheck;
    }
    public struct vector
    {
        public int x;
        public int y;
    }
    public struct squareInfoItem
    {
        public int AttackedByPM;
        public int AttackedByPO;
        public byte n_adjacent_whitewitches;
        public byte n_adjacent_blackwitches;
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
        public int WhiteJokerSubstitute_pti;
        public int BlackJokerSubstitute_pti;
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
        public bool POKingInCheckTimeThief;
    }

    public class WeirdEngineMoveFinder
    {
        public const int movelist_allocated = 500;
        public const int positionstack_size = 25;
        public const int defaultboardwidth = 8;
        public const int defaultboardheight = 8;

        public WeirdEngineJson MyWeirdEngineJson;//reference to Json object that can do some logging

        public int presort_when_n_plies_gt;
        public int presort_using_n_plies;
        public int display_when_n_plies_gt;
        public bool externalabort;
        public chesspiecetype[] piecetypes;
        public chessposition[] positionstack;
        public WeirdEngineMoveFinder()
        {
            this.presort_when_n_plies_gt = 4;
            this.presort_using_n_plies = 3;
            this.display_when_n_plies_gt = 7;
            this.externalabort = false;
            this.init_positionstack(defaultboardwidth, defaultboardheight);
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
        public bool HasPreviousPosition()
        {
            int posidx = this.positionstack.Length - 1;
            if (posidx < positionstack_size - 1)
            {
                return false;
            }
            for (int i = 0;i < positionstack[posidx].boardwidth; i++)
                for (int j = 0; j < positionstack[posidx].boardheight; j++)
                {
                    if (positionstack[posidx].squares[i, j] != 0)
                    {
                        return true;
                    }
                }
            return false;
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
                    pposition.squareInfo[i, j].AttackedByPM = 0;
                    pposition.squareInfo[i, j].AttackedByPO = 0;
                    pposition.squareInfo[i, j].n_adjacent_whitewitches = 0;
                    pposition.squareInfo[i, j].n_adjacent_blackwitches = 0;
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
            pposition.WhiteJokerSubstitute_pti = -1;
            pposition.BlackJokerSubstitute_pti = -1;
            pposition.movelist_totalfound = 0;
            pposition.POKingInCheckTimeThief = false;

            pposition.precedingmove[0] = -1;
            pposition.precedingmove[1] = -1;
            pposition.precedingmove[2] = -1;
            pposition.precedingmove[3] = -1;
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
        public double CheckKingsPresent(ref chessposition pposition)
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
            return 0.0;
        }
        public double EvaluationByMaterial(ref chessposition pposition)
        {
            double materialbalance = 0.0;

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
        public double EvaluationByAttack(ref chessposition pposition)
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
            double resultev = (AttackedByWhitetotal - AttackedByBlacktotal) / 2.0;

            if (WhiteKingIsInCheck(ref pposition))
            {
                resultev -= 5;
            }
            if (BlackKingIsInCheck(ref pposition))
            {
                resultev += 5;
            }

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
        public double StaticEvaluation(ref chessposition pposition)
        {
            //double myev = EvaluationByMaterial(ref pposition);
            double myev = EvaluationByAttack(ref pposition);
            return myev;
        }
        public void init_positionstack(int pboardwidth, int pboardheight)
        {
            this.positionstack = null;
            this.positionstack = new chessposition[positionstack_size];
            for (int pi = 0; pi < positionstack_size; pi++)
            {
                this.ResetBoardsize(ref this.positionstack[pi], pboardwidth, pboardheight);
                this.AllocateMovelist(ref this.positionstack[pi]);
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
        public void SetWitchInfluence(ref chessposition pposition)
        {
            for (int i = 0; i < pposition.boardwidth; i++)
            {
                for (int j = 0; j < pposition.boardheight; j++)
                {
                    if (pposition.squares[i, j] != 0)
                    {
                        int pti = pieceTypeIndex(pposition.squares[i, j]);
                        if (this.piecetypes[pti].name == "Witch")
                        {
                            for (int i2 = i - 1; i2 < i + 2; i2++)
                            {
                                for (int j2 = j - 1; j2 < j + 2; j2++)
                                {
                                    if (i2 >= 0 & i2 < pposition.boardwidth & j2 >= 0 & j2 < pposition.boardheight)
                                    {
                                        if (pposition.squares[i, j] > 0)
                                        {
                                            pposition.squareInfo[i2, j2].n_adjacent_whitewitches += 1;
                                        }
                                        else
                                        {
                                            pposition.squareInfo[i2, j2].n_adjacent_blackwitches += 1;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public bool WhiteKingIsInCheck(ref chessposition pposition)
        {
            if (pposition.colourtomove == 1)
            {
                return PMKingIsInCheck(ref pposition);
            }
            else
            {
                return POKingIsInCheck(ref pposition);
            }
        }
        public bool BlackKingIsInCheck(ref chessposition pposition)
        {
            if (pposition.colourtomove == 1)
            {
                return POKingIsInCheck(ref pposition);
            }
            else
            {
                return PMKingIsInCheck(ref pposition);
            }
        }
        public bool PMKingIsInCheck(ref chessposition pposition)
        {
            if (pposition.colourtomove == 1)
            {
                if (pposition.squareInfo[pposition.whitekingcoord.x, pposition.whitekingcoord.y].AttackedByPO > 0)
                {
                    return true;
                }
            }
            else
            {
                if (pposition.squareInfo[pposition.blackkingcoord.x, pposition.blackkingcoord.y].AttackedByPO > 0)
                {
                    return true;
                }
            }
            return false;
        }
        public bool POKingIsInCheck(ref chessposition pposition)
        {
            if (pposition.colourtomove == 1)
            {
                if (pposition.squareInfo[pposition.blackkingcoord.x, pposition.blackkingcoord.y].AttackedByPM > 0)
                {
                    return true;
                }
            }
            else
            {
                if (pposition.squareInfo[pposition.whitekingcoord.x, pposition.whitekingcoord.y].AttackedByPM > 0)
                {
                    return true;
                }
            }
            if (pposition.POKingInCheckTimeThief == true)
            {
                return true;
            }
            return false;
        }
        public void MarkAttacked(ref chessposition pposition, int x, int y, int pmovingpiece)
        {
            if (pmovingpiece > 0)
            {
                if (pposition.colourtomove == 1)
                {
                    pposition.squareInfo[x, y].AttackedByPM +=1;
                }
                else
                {
                    pposition.squareInfo[x, y].AttackedByPO +=1;
                }
            }
            else
            {
                if (pposition.colourtomove == 1)
                {
                    pposition.squareInfo[x, y].AttackedByPO += 1;
                }
                else
                {
                    pposition.squareInfo[x, y].AttackedByPM += 1;
                }
            }
        }
        public void GetAttacksMoves(ref chessposition pposition, int n_plies, int prevposidx)
        {
            pposition.movelist_totalfound = 0;
            pposition.POKingInCheckTimeThief = false;
            for (int i = 0; i < pposition.boardwidth; i++)
            {
                for (int j = 0; j < pposition.boardheight; j++)
                {
                    if (pposition.squares[i, j] != 0)
                    {
                        this.GetStepLeapAttacksMoves(ref pposition, i, j, n_plies);
                        this.GetSlideAttacksMoves(ref pposition, i, j, n_plies);
                    }
                    if (n_plies > 0)
                    {
                        if ((pposition.squares[i, j] > 0 & pposition.colourtomove > 0) ||
                            (pposition.squares[i, j] < 0 & pposition.colourtomove < 0))
                        {
                            GetPawn2StepMoves(ref pposition, i, j);
                            GetPawnEnPassantMoves(ref pposition, i, j);
                        }
                    }
                    if ((pposition.squares[i, j] > 0 & pposition.colourtomove > 0) ||
                        (pposition.squares[i, j] < 0 & pposition.colourtomove < 0))
                    {
                        GetTimeThiefCapture(ref pposition, i, j, prevposidx, n_plies);
                    }
                }
            }
            if (n_plies > 0)
            {
                GetCastling(ref pposition);
            }
        }
        public void GetStepLeapAttacksMovesPerVector(ref chessposition pposition, int i, int j, vector v,
                                                     bool getcaptures, bool getnoncaptures, int n_plies)
        {
            int i2;
            int j2;
            int movei;
            i2 = i + v.x;
            if (pposition.squares[i, j] > 0)
            {
                j2 = j + v.y;
            }
            else
            {
                j2 = j - v.y;
            }
            if (i2 >= 0 & i2 < pposition.boardwidth & j2 >= 0 & j2 < pposition.boardheight)
            {
                if (getcaptures == true)
                {
                    this.MarkAttacked(ref pposition, i2, j2, pposition.squares[i, j]);
                    if (n_plies > 0)
                    {
                        if ((pposition.squares[i2, j2] > 0 & pposition.squares[i, j] < 0 & pposition.colourtomove < 0) ||
                            (pposition.squares[i2, j2] < 0 & pposition.squares[i, j] > 0 & pposition.colourtomove > 0))
                        {
                            movei = pposition.movelist_totalfound;
                            InitializeMove(ref pposition, movei, i, j, i2, j2);
                            pposition.movelist[movei].MovingPiece = pposition.squares[i, j];
                            pposition.movelist[movei].IsCapture = true;
                            GetPromotion(ref pposition, movei);
                        }
                    }
                }
                if (getnoncaptures == true & n_plies > 0)
                {
                    if ((pposition.squares[i2, j2] == 0 & pposition.squares[i, j] < 0 & pposition.colourtomove < 0) ||
                        (pposition.squares[i2, j2] == 0 & pposition.squares[i, j] > 0 & pposition.colourtomove > 0))
                    {
                        movei = pposition.movelist_totalfound;
                        InitializeMove(ref pposition, movei, i, j, i2, j2);
                        pposition.movelist[movei].MovingPiece = pposition.squares[i, j];
                        GetPromotion(ref pposition, movei);
                    }
                }
            }
        }
        public int jokersubspti(ref chessposition pposition, int i, int j, int pti)
        {
            if (pposition.squares[i, j] > 0)
            {
                if (pposition.WhiteJokerSubstitute_pti > -1)
                {
                    return pposition.WhiteJokerSubstitute_pti;
                }
            }
            else
            {
                if (pposition.BlackJokerSubstitute_pti > -1)
                {
                    return pposition.BlackJokerSubstitute_pti;
                }
            }
            return pti;
        }
        public void GetStepLeapAttacksMoves(ref chessposition pposition, int i, int j, int n_plies)
        {
            int pti = this.pieceTypeIndex(pposition.squares[i, j]);
            int pti_self = pti;
            if (this.piecetypes[pti].name == "Joker")
            {
                pti = jokersubspti(ref pposition, i, j, pti);
            }

            if (this.piecetypes[pti].IsDivergent == false)
            {
                foreach (vector v in this.piecetypes[pti].stepleapmovevectors)
                {
                    GetStepLeapAttacksMovesPerVector(ref pposition, i, j, v, true, true, n_plies);
                }
            }
            else
            {
                foreach (vector v in this.piecetypes[pti].stepleapmovevectors)
                {
                    GetStepLeapAttacksMovesPerVector(ref pposition, i, j, v, false, true, n_plies);
                }
                foreach (vector v in this.piecetypes[pti].stepleapcapturevectors)
                {
                    GetStepLeapAttacksMovesPerVector(ref pposition, i, j, v, true, false, n_plies);
                }
            }
        }
        public bool SquareIsTransparent(ref chessposition pposition, int i, int j, int i2, int j2, int pti)
        {
            bool IsTransparent = false;
            if (pposition.squares[i, j] > 0 & pposition.squareInfo[i2, j2].n_adjacent_whitewitches > 0)
            {
                if (piecetypes[pti].name == "Witch"
                    & pposition.squareInfo[i2, j2].n_adjacent_whitewitches > 1)
                {
                    IsTransparent = true;
                }
                else
                {
                    if (piecetypes[pti].name != "Witch")
                    {
                        IsTransparent = true;
                    }
                }
            }
            if (pposition.squares[i, j] < 0 & pposition.squareInfo[i2, j2].n_adjacent_blackwitches > 0)
            {
                if (piecetypes[pti].name == "Witch"
                    & pposition.squareInfo[i2, j2].n_adjacent_blackwitches > 1)
                {
                    IsTransparent = true;
                }
                else
                {
                    if (piecetypes[pti].name != "Witch")
                    {
                        IsTransparent = true;
                    }
                }
            }
            return IsTransparent;
        }
        public bool SquareStepLeapAttackedFromSquare(ref chessposition pposition, int i, int j, int i3, int j3,
                                                     vector v)
        {
            //Establish if [i3,j3] is attacked from [i,j] using StepLeap vector v yes or no
            int i2;
            int j2;
            i2 = i + v.x;
            if (pposition.squares[i, j] > 0)
            {
                j2 = j + v.y;
            }
            else
            {
                j2 = j - v.y;
            }
            if (i2 == i3 & j2 == j3)
            {
                return true;
            }
            return false;
        }
        public bool SquareSlideAttackedFromSquare(ref chessposition pposition, int i, int j, int i3, int j3,
                                                  vector v, int pti)
        {
            //Establish if [i3,j3] is attacked from [i,j] using Slide vector v yes or no
            int i2;
            int j2;
            bool blocked;

            i2 = i + v.x;
            if (pposition.squares[i, j] > 0)
            {
                j2 = j + v.y;
            }
            else
            {
                j2 = j - v.y;
            }
            blocked = false;
            while (i2 >= 0 & i2 < pposition.boardwidth & j2 >= 0 & j2 < pposition.boardheight & blocked == false)
            {
                if (i2 == i3 & j2 == j3)
                {
                    return true;
                }
                if (pposition.squares[i2, j2] != 0)
                {
                    bool IsTransparent = SquareIsTransparent(ref pposition, i, j, i2, j2, pti);
                    if (IsTransparent == false)
                    {
                        blocked = true;
                    }
                }
                i2 = i2 + v.x;
                if (pposition.squares[i, j] > 0)
                {
                    j2 = j2 + v.y;
                }
                else
                {
                    j2 = j2 - v.y;
                }
            }
            return false;
        }
        public void GetSlideAttacksMovesPerVector(ref chessposition pposition, int i, int j, vector v,
                                                  bool getcaptures, bool getnoncaptures, int n_plies, int pti)
        {
            int i2;
            int j2;
            int movei;
            bool blocked;

            i2 = i + v.x;
            if (pposition.squares[i, j] > 0)
            {
                j2 = j + v.y;
            }
            else
            {
                j2 = j - v.y;
            }
            blocked = false;
            while (i2 >= 0 & i2 < pposition.boardwidth & j2 >= 0 & j2 < pposition.boardheight & blocked == false)
            {
                if (getcaptures == true)
                {
                    this.MarkAttacked(ref pposition, i2, j2, pposition.squares[i, j]);
                    if (n_plies > 0)
                    {
                        if ((pposition.squares[i2, j2] > 0 & pposition.squares[i, j] < 0 & pposition.colourtomove < 0) ||
                            (pposition.squares[i2, j2] < 0 & pposition.squares[i, j] > 0 & pposition.colourtomove > 0))
                        {
                            movei = pposition.movelist_totalfound;
                            InitializeMove(ref pposition, movei, i, j, i2, j2);
                            pposition.movelist[movei].MovingPiece = pposition.squares[i, j];
                            pposition.movelist[movei].IsCapture = true;
                            GetPromotion(ref pposition, movei);
                        }
                    }
                }
                if (getnoncaptures == true & n_plies > 0)
                {
                    if ((pposition.squares[i2, j2] == 0 & pposition.squares[i, j] < 0 & pposition.colourtomove < 0) ||
                             (pposition.squares[i2, j2] == 0 & pposition.squares[i, j] > 0 & pposition.colourtomove > 0))
                    {
                        movei = pposition.movelist_totalfound;
                        InitializeMove(ref pposition, movei, i, j, i2, j2);
                        pposition.movelist[movei].MovingPiece = pposition.squares[i, j];
                        GetPromotion(ref pposition, movei);
                    }
                }
                if (pposition.squares[i2, j2] != 0)
                {
                    bool IsTransparent = SquareIsTransparent(ref pposition, i, j, i2, j2, pti);
                    if (IsTransparent == false)
                    {
                        blocked = true;
                    }
                }
                i2 = i2 + v.x;
                if (pposition.squares[i, j] > 0)
                {
                    j2 = j2 + v.y;
                }
                else
                {
                    j2 = j2 - v.y;
                }
            }
        }
        public void GetSlideAttacksMoves(ref chessposition pposition, int i, int j, int n_plies)
        {
            int pti = this.pieceTypeIndex(pposition.squares[i, j]);
            int pti_self = pti;
            if (this.piecetypes[pti].name == "Joker")
            {
                pti = jokersubspti(ref pposition, i, j, pti);
            }

            if (this.piecetypes[pti].IsDivergent == false)
            {
                foreach (vector v in this.piecetypes[pti].slidemovevectors)
                {
                    GetSlideAttacksMovesPerVector(ref pposition, i, j, v, true, true, n_plies, pti_self);
                }
            }
            else
            {
                foreach (vector v in this.piecetypes[pti].slidemovevectors)
                {
                    GetSlideAttacksMovesPerVector(ref pposition, i, j, v, false, true, n_plies, pti_self);
                }
                foreach (vector v in this.piecetypes[pti].slidecapturevectors)
                {
                    GetSlideAttacksMovesPerVector(ref pposition, i, j, v, true, false, n_plies, pti_self);
                }
            }
        }
        public void SynchronizeChessmove(chessmove frommove, ref chessmove tomove)
        {
            tomove.MovingPiece = frommove.MovingPiece;
            tomove.coordinates[0] = frommove.coordinates[0];
            tomove.coordinates[1] = frommove.coordinates[1];
            tomove.coordinates[2] = frommove.coordinates[2];
            tomove.coordinates[3] = frommove.coordinates[3];
            tomove.IsEnPassant = frommove.IsEnPassant;
            tomove.IsCapture = frommove.IsCapture;
            tomove.IsCastling = frommove.IsCastling;
            tomove.othercoordinates[0] = frommove.othercoordinates[0];
            tomove.othercoordinates[1] = frommove.othercoordinates[1];
            tomove.othercoordinates[2] = frommove.othercoordinates[2];
            tomove.othercoordinates[3] = frommove.othercoordinates[3];
            tomove.PromoteToPiece = frommove.PromoteToPiece;
        }
        public void InitializeMove(ref chessposition pposition, int movei, int pi1, int pj1, int pi2, int pj2)
        {
            pposition.movelist[movei].MovingPiece = 0;
            pposition.movelist[movei].coordinates[0] = pi1;
            pposition.movelist[movei].coordinates[1] = pj1;
            pposition.movelist[movei].coordinates[2] = pi2;
            pposition.movelist[movei].coordinates[3] = pj2;
            pposition.movelist[movei].IsEnPassant = false;
            pposition.movelist[movei].IsCapture = false;
            pposition.movelist[movei].IsCastling = false;
            pposition.movelist[movei].othercoordinates[0] = -1;
            pposition.movelist[movei].othercoordinates[1] = -1;
            pposition.movelist[movei].othercoordinates[2] = -1;
            pposition.movelist[movei].othercoordinates[3] = -1;
            pposition.movelist[movei].PromoteToPiece = 0;
        }
        public void GetPromotion(ref chessposition pposition, int movei)
        {
            bool includepromote = false;
            bool includenonpromote = false;
            int pti = this.pieceTypeIndex(pposition.movelist[movei].MovingPiece);
            
            if (this.piecetypes[pti].name == "Pawn")
            {
                if (pposition.movelist[movei].MovingPiece > 0 &
                    pposition.movelist[movei].coordinates[3] == pposition.boardheight - 1)
                {
                    includepromote = true;
                    includenonpromote = false;
                }
                else if (pposition.movelist[movei].MovingPiece < 0 &
                         pposition.movelist[movei].coordinates[3] == 0)
                {
                    includepromote = true;
                    includenonpromote = false;
                }
                else
                {
                    includepromote = false;
                    includenonpromote = true;
                }
            } else
            {
                includepromote = false;
                includenonpromote = true;
            }
            if (includenonpromote == true)
            {
                pposition.movelist_totalfound += 1;
            }
            if (includepromote == true)
            {
                for (int pi = 0; pi < this.piecetypes.Length; pi++)
                {
                    if (pi == pti) { }//nothing
                    else if (this.piecetypes[pi].name == "King" & this.piecetypes[pi].IsRoyal == true) { }//nothing
                    else if (this.piecetypes[pi].name == "Amazon") { }//nothing
                    else
                    {
                        int movei2 = pposition.movelist_totalfound;
                        this.SynchronizeChessmove(pposition.movelist[movei], ref pposition.movelist[movei2]);
                        if (pposition.movelist[movei].MovingPiece < 0)
                        {
                            pposition.movelist[movei2].PromoteToPiece = (pi + 1) * -1;
                        }
                        else
                        {
                            pposition.movelist[movei2].PromoteToPiece = pi + 1;
                        }
                        pposition.movelist_totalfound += 1;
                    }
                }
            }
        }
        public void GetTimeThiefCapture(ref chessposition pposition, int i, int j, int prevposidx, int n_plies)
        {
            int movei;
            int pti = this.pieceTypeIndex(pposition.squares[i, j]);
            if (this.piecetypes[pti].name != "TimeThief")
            {
                return;
            }
            if (pposition.precedingmove[0] == -1)
            {
                return;
            }
            if (prevposidx < 0)
            {
                return;
            }
            int i2 = pposition.precedingmove[0];
            int j2 = pposition.precedingmove[1];
            //we assume that TimeThief-capture is following the TimeThief's own slidemovevectors
            //if not then we must change this code and extend the json structure
            foreach (vector v in this.piecetypes[pti].slidemovevectors)
            {
                if (SquareSlideAttackedFromSquare(ref positionstack[prevposidx], i, j, i2, j2, v, pti) == true)
                {
                    int i3 = pposition.precedingmove[2];
                    int j3 = pposition.precedingmove[3];
                    int pti3 = this.pieceTypeIndex(pposition.squares[i3, j3]);
                    if (this.piecetypes[pti3].name == "King")
                    {
                        //King moved out of attack range of TimeThief
                        //That is equivalent with moving into check, and here we must detect this
                        pposition.POKingInCheckTimeThief = true;
                    }

                    if (n_plies > 0)
                    {
                        movei = pposition.movelist_totalfound;
                        InitializeMove(ref pposition, movei, i, j, i2, j2);
                        pposition.movelist[movei].IsCapture = true;
                        pposition.movelist[movei].MovingPiece = pposition.squares[i, j];
                        pposition.movelist_totalfound += 1;
                    }
                }
            }
        }
        public void GetPawn2StepMoves(ref chessposition pposition, int i, int j)
        {
            int pti = this.pieceTypeIndex(pposition.squares[i, j]);

            if (this.piecetypes[pti].name == "Joker")
            {
                pti = jokersubspti(ref pposition, i, j, pti);
            }

            if (this.piecetypes[pti].name != "Pawn")
            {
                return;
            }
            if (pposition.colourtomove > 0 & j != 1)
            {
                return;
            }
            if (pposition.colourtomove < 0 & j != pposition.boardheight - 2)
            {
                return;
            }
            int i2;
            int i_skip;
            int j2;
            int j_skip;
            int movei;
            i2 = i;
            i_skip = i;
            if (pposition.colourtomove > 0)
            {
                j_skip = j + 1;
                j2 = j + 2;
            }
            else
            {
                j_skip = j - 1;
                j2 = j - 2;
            }
            if (pposition.squares[i_skip, j_skip] == 0 & pposition.squares[i2, j2] == 0)
            {
                movei = pposition.movelist_totalfound;
                InitializeMove(ref pposition, movei, i, j, i2, j2);
                pposition.movelist[movei].MovingPiece = pposition.squares[i, j];
                pposition.movelist_totalfound += 1;
            }
        }
        public void GetPawnEnPassantMoves(ref chessposition pposition, int i, int j)
        {
            int pti = this.pieceTypeIndex(pposition.squares[i, j]);

            if (this.piecetypes[pti].name == "Joker")
            {
                pti = jokersubspti(ref pposition, i, j, pti);
            }

            if (this.piecetypes[pti].name != "Pawn")
            {
                return;
            }
            if (pposition.precedingmove[3] != j)
            {
                return;
            }
            int x_from = pposition.precedingmove[0];
            int y_from = pposition.precedingmove[1];
            int x_to = pposition.precedingmove[2];
            int y_to = pposition.precedingmove[3];
            int ptm = this.pieceTypeIndex(pposition.squares[x_to, y_to]);

            if (this.piecetypes[ptm].name == "Joker")
            {
                ptm = jokersubspti(ref pposition, x_to, y_to, ptm);
            }

            int movei;

            if (this.piecetypes[ptm].name != "Pawn")
            {
                return;
            }
            if (x_from - i == 1 || x_from - i == -1)
            {
                //nothing
            }
            else
            {
                return;
            }
            if (pposition.colourtomove > 0)
            {
                if (pposition.squares[x_to, y_to] > 0) { return; }
                if (j != pposition.boardheight - 4) { return; }
                if (y_from != y_to + 2) { return; }
                movei = pposition.movelist_totalfound;
                InitializeMove(ref pposition, movei, i, j, x_from, y_to + 1);
                pposition.movelist[movei].MovingPiece = pposition.squares[i, j];
                pposition.movelist[movei].IsEnPassant = true;
                pposition.movelist[movei].othercoordinates[0] = x_to;
                pposition.movelist[movei].othercoordinates[1] = y_to;
                pposition.movelist[movei].othercoordinates[2] = -1;
                pposition.movelist[movei].othercoordinates[3] = -1;
                pposition.movelist[movei].IsCapture = true;
                pposition.movelist_totalfound += 1;
            }
            if (pposition.colourtomove < 0)
            {
                if (pposition.squares[x_to, y_to] < 0) { return; }
                if (j != 3) { return; }
                if (y_from != y_to - 2) { return; }
                movei = pposition.movelist_totalfound;
                InitializeMove(ref pposition, movei, i, j, x_from, y_to - 1);
                pposition.movelist[movei].MovingPiece = pposition.squares[i, j];
                pposition.movelist[movei].IsEnPassant = true;
                pposition.movelist[movei].othercoordinates[0] = x_to;
                pposition.movelist[movei].othercoordinates[1] = y_to;
                pposition.movelist[movei].othercoordinates[2] = -1;
                pposition.movelist[movei].othercoordinates[3] = -1;
                pposition.movelist[movei].IsCapture = true;
                pposition.movelist_totalfound += 1;
            }

        }
        public void GetCastling(ref chessposition pposition)
        {
            int j = -1;
            int i_k = -1;
            int i_qr = -1;
            int i_kr = -1;
            int i_k_new = -1;
            int i_qr_new = -1;
            int i_kr_new = -1;
            int movei;
            if (pposition.colourtomove == 1)
            {
                if (pposition.whitekinghasmoved == true) { return; }
                j = 0;
                if (pposition.whitekingcoord.y != j) { return; }
                i_k = pposition.whitekingcoord.x;
                i_qr = pposition.whitequeensiderookcoord.x;
                i_kr = pposition.whitekingsiderookcoord.x;
            }
            else if (pposition.colourtomove == -1)
            {
                if (pposition.blackkinghasmoved == true) { return; }
                j = pposition.boardheight - 1;
                if (pposition.blackkingcoord.y != j) { return; }
                i_k = pposition.blackkingcoord.x;
                i_qr = pposition.blackqueensiderookcoord.x;
                i_kr = pposition.blackkingsiderookcoord.x;
            }

            bool queensidepossible = true;
            bool kingsidepossible = true;

            if (pposition.colourtomove == 1 & pposition.whitequeensiderookcoord.y != j)
            { queensidepossible = false; }
            if (pposition.colourtomove == -1 & pposition.blackqueensiderookcoord.y != j)
            { queensidepossible = false; }
            if (pposition.colourtomove == 1 & pposition.whitekingsiderookcoord.y != j)
            { kingsidepossible = false; }
            if (pposition.colourtomove == -1 & pposition.blackkingsiderookcoord.y != j)
            { kingsidepossible = false; }

            if (pposition.colourtomove == 1 & pposition.whitequeensiderookhasmoved == true)
            { queensidepossible = false; }
            if (pposition.colourtomove == -1 & pposition.blackqueensiderookhasmoved == true)
            { queensidepossible = false; }
            if (pposition.colourtomove == 1 & pposition.whitekingsiderookhasmoved == true)
            { kingsidepossible = false; }
            if (pposition.colourtomove == -1 & pposition.blackkingsiderookhasmoved == true)
            { kingsidepossible = false; }

            if (i_qr > -1 & i_k > i_qr) { }//nothing
            else { queensidepossible = false; }

            if (i_k > -1 & i_kr > i_k) { }//nothing
            else { kingsidepossible = false; }

            if (queensidepossible)
            {
                i_k_new = 2;
                i_qr_new = i_k_new + 1;
                for (int i = 0;i < pposition.boardwidth; i++)
                {
                    if (((i > i_qr & i <= i_qr_new) || (i < i_qr & i >= i_qr_new)) & i != i_k)
                    {
                        if (pposition.squares[i, j] != 0)
                        { queensidepossible = false; }
                    }
                    if (((i > i_k & i <= i_k_new) || (i < i_k & i >= i_k_new)) & i != i_qr)
                    {
                        if (pposition.squares[i, j] != 0)
                        { queensidepossible = false; }
                    }
                    if (((i >= i_k & i <= i_k_new) || (i <= i_k & i >= i_k_new))
                        & pposition.squareInfo[i, j].AttackedByPO > 0)
                    {
                        queensidepossible = false;
                    }
                }
            }
            if (queensidepossible)
            {
                movei = pposition.movelist_totalfound;
                InitializeMove(ref pposition, movei, i_k, j, i_k_new, j);
                pposition.movelist[movei].MovingPiece = pposition.squares[i_k, j];
                pposition.movelist[movei].IsCastling = true;
                pposition.movelist[movei].othercoordinates[0] = i_qr;
                pposition.movelist[movei].othercoordinates[1] = j;
                pposition.movelist[movei].othercoordinates[2] = i_qr_new;
                pposition.movelist[movei].othercoordinates[3] = j;
                pposition.movelist_totalfound += 1;
            }
            if (kingsidepossible)
            {
                i_k_new = pposition.boardwidth - 2;
                i_kr_new = i_k_new - 1;
                for (int i = 0; i < pposition.boardwidth; i++)
                {
                    if (((i > i_kr & i <= i_kr_new) || (i < i_kr & i >= i_kr_new)) & i != i_k)
                    {
                        if (pposition.squares[i, j] != 0)
                        { kingsidepossible = false; }
                    }
                    if (((i > i_k & i <= i_k_new) || (i < i_k & i >= i_k_new)) & i != i_kr)
                    {
                        if (pposition.squares[i, j] != 0)
                        { kingsidepossible = false; }
                    }
                    if (((i >= i_k & i <= i_k_new) || (i <= i_k & i >= i_k_new))
                        & pposition.squareInfo[i, j].AttackedByPO > 0)
                    {
                        kingsidepossible = false;
                    }
                }
            }
            if (kingsidepossible)
            {
                movei = pposition.movelist_totalfound;
                InitializeMove(ref pposition, movei, i_k, j, i_k_new, j);
                pposition.movelist[movei].MovingPiece = pposition.squares[i_k, j];
                pposition.movelist[movei].IsCastling = true;
                pposition.movelist[movei].othercoordinates[0] = i_kr;
                pposition.movelist[movei].othercoordinates[1] = j;
                pposition.movelist[movei].othercoordinates[2] = i_kr_new;
                pposition.movelist[movei].othercoordinates[3] = j;
                pposition.movelist_totalfound += 1;
            }
        }
        public calculationresponse Calculation_n_plies(int n_plies)
        {
            this.MyWeirdEngineJson.SetLogfilename();

            if (HasPreviousPosition() == true)
            {
                SetWitchInfluence(ref positionstack[positionstack.Length - 1]);
            }

            this.externalabort = false;
            calculationresponse myresult = this.Calculation_n_plies_internal(0, -100, 100, n_plies);

            return myresult;
        }
        public calculationresponse Calculation_n_plies_internal(int posidx, double alpha, double beta, int n_plies)
        {
            calculationresponse myresult;
            myresult.posvalue = 0.0;
            myresult.moveidx = -1;
            myresult.POKingIsInCheck = false;

            this.LocateKingsRooks(ref positionstack[posidx]);
            myresult.posvalue = CheckKingsPresent(ref positionstack[posidx]);
            if (myresult.posvalue == 100 || myresult.posvalue == -100)
            {
                return myresult;
            }
            SetWitchInfluence(ref positionstack[posidx]);

            int prevposidx = posidx - 1;
            if (prevposidx == -1)
            {
                if (HasPreviousPosition())
                {
                    prevposidx = positionstack.Length - 1;
                }
            }
            GetAttacksMoves(ref positionstack[posidx], n_plies, prevposidx);

            if (POKingIsInCheck(ref positionstack[posidx]) == true)
            {
                if (positionstack[posidx].colourtomove == 1)
                {
                    myresult.posvalue = 100;
                }
                else
                {
                    myresult.posvalue = -100;
                }
                myresult.POKingIsInCheck = true;
                return myresult;
            }

            if (n_plies == 0)
            {
                myresult.posvalue = StaticEvaluation(ref positionstack[posidx]);
                return myresult;
            }

            int movecount = positionstack[posidx].movelist_totalfound;
            //this.MyWeirdEngineJson.writelog(this.MyWeirdEngineJson.DisplayMovelist(ref positionstack[posidx]));
            //MessageBox.Show(this.MyWeirdEngineJson.DisplayMovelist(ref positionstack[posidx]));
            //MessageBox.Show(this.MyWeirdEngineJson.DisplayAttacks(ref positionstack[posidx]));

            double new_alpha = alpha;
            double new_beta = beta;

            //presort BEGIN
            if (n_plies > this.presort_when_n_plies_gt)
            {
                Application.DoEvents();
                if (this.externalabort == true)
                {
                    return myresult;
                }
                if (n_plies > this.display_when_n_plies_gt)
                {
                    string s = "List before sorting : ";
                    s += this.MyWeirdEngineJson.DisplayMovelist(ref positionstack[posidx]);
                    this.MyWeirdEngineJson.writelog(s);
                }

                chessmove[] movelist2 = new chessmove[movecount];
                for (int movei = 0; movei < movecount; movei++)
                {
                    movelist2[movei].coordinates = new int[4];
                    movelist2[movei].othercoordinates = new int[4];
                    SynchronizeChessmove(positionstack[posidx].movelist[movei], ref movelist2[movei]);
                }
                movePrioItem[] subresults_presort = new movePrioItem[movecount];

                for (int i = 0; i < movecount; i++)
                {
                    int newposidx = ExecuteMove(posidx, movelist2[i], prevposidx);
                    calculationresponse newresponse_presort = Calculation_n_plies_internal(newposidx, new_alpha, new_beta,
                                                                                   presort_using_n_plies);
                    subresults_presort[i].moveidx = i;
                    subresults_presort[i].movevalue = newresponse_presort.posvalue;
                    //MyWeirdEngineJson.writelog("Value during presoring moveidx " + i.ToString()
                    //    + " movevalue " + newresponse_presort.posvalue.ToString());
                }

                if (positionstack[posidx].colourtomove == 1)
                {
                    //order list by movevalue descending so best move for white first
                    Array.Sort<movePrioItem>(subresults_presort, (x, y) => y.movevalue.CompareTo(x.movevalue));
                }
                else
                {
                    //order list by movevalue ascending so best move for black first
                    Array.Sort<movePrioItem>(subresults_presort, (x, y) => x.movevalue.CompareTo(y.movevalue));
                }
                for (int movei = 0; movei < movecount; movei++)
                {
                    SynchronizeChessmove(movelist2[subresults_presort[movei].moveidx], ref positionstack[posidx].movelist[movei]);
                }
                if (n_plies > this.display_when_n_plies_gt)
                {
                    string s = "List after sorting : ";
                    s += this.MyWeirdEngineJson.DisplayMovelist(ref positionstack[posidx]);
                    this.MyWeirdEngineJson.writelog(s);
                }
            }
            //presort END

            movePrioItem[] subresults = new movePrioItem[movecount];
            int bestmoveidx = -1;
            double bestmovevalue = 0;
            if (positionstack[posidx].colourtomove == 1)
            {
                bestmovevalue = -120;
            }
            else
            {
                bestmovevalue = 120;
            }
            bool noescapecheck = true;

            for (int i = 0; i < movecount; i++)
            {
                int newposidx = ExecuteMove(posidx, positionstack[posidx].movelist[i], prevposidx);
                calculationresponse newresponse = Calculation_n_plies_internal(newposidx, new_alpha, new_beta,
                                                                               n_plies - 1);
                if (n_plies > this.display_when_n_plies_gt)
                {
                    string mvstr = MyWeirdEngineJson.ShortNotation(positionstack[posidx].movelist[i]);
                    MyWeirdEngineJson.writelog("n_plies " + n_plies.ToString() + " DONE checking move "
                        + mvstr + " alpha " + new_alpha.ToString() + " beta " + new_beta.ToString()
                        + " posvalue " + newresponse.posvalue.ToString());
                }
                if (newresponse.POKingIsInCheck == false)
                {
                    noescapecheck = false;
                }
                subresults[i].moveidx = i;
                subresults[i].movevalue = newresponse.posvalue;

                if (this.positionstack[posidx].colourtomove == 1)
                {
                    if (newresponse.posvalue > bestmovevalue )
                    {
                        bestmovevalue = newresponse.posvalue;
                        bestmoveidx = i;
                    }
                    if (new_alpha < newresponse.posvalue)
                    {
                        new_alpha = newresponse.posvalue;
                    }
                    if (newresponse.posvalue >= new_beta)
                    {
                        break;
                    }
                }
                else
                {
                    if (newresponse.posvalue < bestmovevalue)
                    {
                        bestmovevalue = newresponse.posvalue;
                        bestmoveidx = i;
                    }
                    if (new_beta > newresponse.posvalue)
                    {
                        new_beta = newresponse.posvalue;
                    }
                    if (newresponse.posvalue <= new_alpha)
                    {
                        break;
                    }

                }

            }

            //Mate
            if (PMKingIsInCheck(ref positionstack[posidx]) == true & noescapecheck == true)
            {
                if (positionstack[posidx].colourtomove == 1)
                {
                    myresult.posvalue = -100;
                }
                else
                {
                    myresult.posvalue = 100;
                }
                return myresult;
            }
            //Stalemate
            if (PMKingIsInCheck(ref positionstack[posidx]) == false & noescapecheck == true)
            {
                myresult.posvalue = 0;
                return myresult;
            }

            myresult.posvalue = bestmovevalue;
            myresult.moveidx = bestmoveidx;
            return myresult;
        }
        public int ExecuteMove(int posidx, chessmove pmove, int prevposidx)
        {
            int newposidx = posidx + 1;
            int pti = pieceTypeIndex(pmove.MovingPiece);

            if (this.piecetypes[pti].name == "TimeThief" & prevposidx >= 0 & pmove.IsCapture == true)
            {
                SynchronizePosition(ref positionstack[prevposidx], ref positionstack[newposidx]);
            }
            else
            {
                SynchronizePosition(ref positionstack[posidx], ref positionstack[newposidx]);
            }

            int i1 = pmove.coordinates[0];
            int j1 = pmove.coordinates[1];
            int i2 = pmove.coordinates[2];
            int j2 = pmove.coordinates[3];
            int i_qr = -1;
            int i_kr = -1;

            positionstack[newposidx].precedingmove[0] = i1;
            positionstack[newposidx].precedingmove[1] = j1;
            positionstack[newposidx].precedingmove[2] = i2;
            positionstack[newposidx].precedingmove[3] = j2;

            if (pmove.PromoteToPiece != 0)
            {
                positionstack[newposidx].squares[i2, j2] = pmove.PromoteToPiece;
            }
            else
            {
                positionstack[newposidx].squares[i2, j2] = pmove.MovingPiece;
            }
            this.positionstack[newposidx].squares[i1, j1] = 0;

            //Set castling info for new position BEGIN
            if (this.piecetypes[pti].name == "King" & this.piecetypes[pti].IsRoyal == true)
            {
                if (positionstack[posidx].colourtomove == 1)
                {
                    positionstack[newposidx].whitekinghasmoved = true;
                }
                else
                {
                    positionstack[newposidx].blackkinghasmoved = true;
                }
            }
            else if (this.piecetypes[pti].name == "Rook")
            {
                if (this.positionstack[posidx].colourtomove == 1)
                {
                    i_qr = positionstack[posidx].whitequeensiderookcoord.x;
                    i_kr = positionstack[posidx].whitekingsiderookcoord.x;
                    if (i1 == i_qr) { positionstack[newposidx].whitequeensiderookhasmoved = true; }
                    else if (i1 == i_kr) { positionstack[newposidx].whitekingsiderookhasmoved = true; }
                }
                else
                {
                    i_qr = positionstack[posidx].blackqueensiderookcoord.x;
                    i_kr = positionstack[posidx].blackkingsiderookcoord.x;
                    if (i1 == i_qr) { positionstack[newposidx].blackqueensiderookhasmoved = true; }
                    else if (i1 == i_kr) { positionstack[newposidx].blackkingsiderookhasmoved = true; }

                }
            }
            //Set castling info for new position END

            if (pmove.IsEnPassant == true)
            {
                int io1 = pmove.othercoordinates[0];
                int jo1 = pmove.othercoordinates[1];
                positionstack[newposidx].squares[io1, jo1] = 0;
            }
            if (pmove.IsCastling == true)
            {
                int io1 = pmove.othercoordinates[0];
                int jo1 = pmove.othercoordinates[1];
                int io2 = pmove.othercoordinates[2];
                int jo2 = pmove.othercoordinates[3];
                int otherpiece = this.positionstack[newposidx].squares[io1, jo1];
                if (io1 != i2)
                {
                    positionstack[newposidx].squares[io1, jo1] = 0;
                }
                positionstack[newposidx].squares[io2, jo2] = otherpiece;
            }

            if (positionstack[posidx].colourtomove == 1)
            {
                positionstack[newposidx].colourtomove = -1;
            }
            else
            {
                positionstack[newposidx].colourtomove = 1;
            }

            //JokerInfo begin
            if (positionstack[posidx].colourtomove == 1)
            {
                if (piecetypes[pti].name == "Joker")
                {
                    positionstack[newposidx].BlackJokerSubstitute_pti = positionstack[posidx].WhiteJokerSubstitute_pti;
                }
                else if (pmove.PromoteToPiece != 0)
                {
                    positionstack[newposidx].BlackJokerSubstitute_pti = pieceTypeIndex(pmove.PromoteToPiece);
                }
                else
                {
                    positionstack[newposidx].BlackJokerSubstitute_pti = pti;
                }
            }
            else
            {
                if (piecetypes[pti].name == "Joker")
                {
                    positionstack[newposidx].WhiteJokerSubstitute_pti = positionstack[posidx].BlackJokerSubstitute_pti;
                }
                else if (pmove.PromoteToPiece != 0)
                {
                    positionstack[newposidx].WhiteJokerSubstitute_pti = pieceTypeIndex(pmove.PromoteToPiece);
                }
                else
                {
                    positionstack[newposidx].WhiteJokerSubstitute_pti = pti;
                }
            }
            //JokerInfo end


            return newposidx;
        }
    }
}
