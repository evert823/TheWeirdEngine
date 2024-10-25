using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.ComponentModel.Com2Interop;

namespace TheWeirdEngine
{
    //In WeirdEngineMoveFinder we re-implement a python code for a chess variant engine,
    //which eventually replaces the old WeirdEngineBackend

    public struct enginesettings
    {
        public int presort_when_depth_gt;
        public bool setting_SearchForFastestMate;
        public int presort_using_depth;
        public int display_when_depth_gt;
        public int consult_tt_when_depth_gt;
        public int store_in_tt_when_depth_gt;
    }
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
        public bool adjacent_whitefemmefatale;
        public bool adjacent_blackfemmefatale;
    }
    public struct FreezeType
    {
        public bool capturefreeze;
        public bool noncapturefreeze;
    }
    public enum MoveType
    {
        //MoveType - mainly for the Elf
        Noncapture,
        Capture,
        other,
    }
    public enum SpecialPiece
    {
        //Any piece that has special functionality assigned to it, so that we can flag it during the calculations
        normalpiece,
        King,
        Rook,
        Bishop,
        Knight,
        Pawn,
        Amazon,
        Witch,
        FemmeFatale,
        TimeThief,
        Joker,
        Elf
    }
    public struct chesspiecetype
    {
        public string symbol;
        public string name;
        public SpecialPiece SpecialPiece_ind;
        public bool IsDivergent;
        public bool CheckDuplicateMoves;
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
        public double calculatedvalue;
    }
    public struct BoardTopology
    {
        public int boardwidth;
        public int boardheight;
        public bool[,] IsWhiteSquare;
        public int[,,,] DistanceBetweenSquares;
        public bool[,,,] SquaresAdjacent;
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
        public MoveType WhiteElfMoveType;
        public MoveType BlackElfMoveType;
        public int[,] squares;//python square[j][i] becomes C# square[i, j]
        public squareInfoItem[,] squareInfo;
        public vector whitekingcoord;
        public vector whitekingsiderookcoord;
        public vector whitequeensiderookcoord;
        public vector blackkingcoord;
        public vector blackkingsiderookcoord;
        public vector blackqueensiderookcoord;

        public bool WhiteBareKing;
        public bool BlackBareKing;
        public bool WhiteBishoponWhite;
        public bool WhiteBishoponBlack;
        public bool BlackBishoponWhite;
        public bool BlackBishoponBlack;
        public bool WhiteHasMatingMaterial;
        public bool BlackHasMatingMaterial;
        public bool WhiteHasWitch;
        public bool BlackHasWitch;
        public bool WhiteHasFemmeFatale;
        public bool BlackHasFemmeFatale;
        public bool WhiteHasJoker;
        public bool BlackHasJoker;
        public bool WhiteHasElf;
        public bool BlackHasElf;

        public int movelist_totalfound;
        public chessmove[] movelist;
        public int[] moveprioindex;
        public bool POKingInCheckTimeThief;

        public int RepetitionCounter;
    }

    public class WeirdEngineMoveFinder
    {
        public const int movelist_allocated = 500;
        public const int positionstack_size = 25;
        public const int defaultboardwidth = 8;
        public const int defaultboardheight = 8;

        public WeirdEngineJson MyWeirdEngineJson;//reference to Json object that can do some logging
        public WeirdEngineBareKingMate MyWeirdEngineBareKingMate;
        public WeirdEnginePositionCompare MyWeirdEnginePositionCompare;
        public WeirdEngineMoveGenerator MyWeirdEngineMoveGenerator;

        public enginesettings myenginesettings;
        public int nodecount;
        public bool externalabort;
        public chesspiecetype[] piecetypes;
        public chessposition[] positionstack;
        public BoardTopology MyBoardTopology;
        public WeirdEngineMoveFinder()
        {
            this.MyWeirdEngineBareKingMate = new WeirdEngineBareKingMate(this);
            this.MyWeirdEnginePositionCompare = new WeirdEnginePositionCompare(this);
            this.MyWeirdEngineMoveGenerator = new WeirdEngineMoveGenerator(this);
            this.myenginesettings.presort_when_depth_gt = 4;
            this.myenginesettings.consult_tt_when_depth_gt = 3;
            this.myenginesettings.store_in_tt_when_depth_gt = 4;
            this.myenginesettings.setting_SearchForFastestMate = true;
            this.myenginesettings.presort_using_depth = 3;
            this.myenginesettings.display_when_depth_gt = 7;
            this.externalabort = false;
            this.init_positionstack(defaultboardwidth, defaultboardheight);
        }
        public void Set_SpecialPiece_ind()
        {
            //Any piece that has special functionality assigned to it, so that we can flag it during the calculations
            for (int pti = 0; pti < piecetypes.Length; pti++)
            {
                if (this.piecetypes[pti].name == "King") { piecetypes[pti].SpecialPiece_ind = SpecialPiece.King; }
                else if (this.piecetypes[pti].name == "Rook") { piecetypes[pti].SpecialPiece_ind = SpecialPiece.Rook; }
                else if (this.piecetypes[pti].name == "Bishop") { piecetypes[pti].SpecialPiece_ind = SpecialPiece.Bishop; }
                else if (this.piecetypes[pti].name == "Knight") { piecetypes[pti].SpecialPiece_ind = SpecialPiece.Knight; }
                else if (this.piecetypes[pti].name == "Pawn") { piecetypes[pti].SpecialPiece_ind = SpecialPiece.Pawn; }
                else if (this.piecetypes[pti].name == "Amazon") { piecetypes[pti].SpecialPiece_ind = SpecialPiece.Amazon; }
                else if (this.piecetypes[pti].name == "Witch") { piecetypes[pti].SpecialPiece_ind = SpecialPiece.Witch; }
                else if (this.piecetypes[pti].name == "FemmeFatale") { piecetypes[pti].SpecialPiece_ind = SpecialPiece.FemmeFatale; }
                else if (this.piecetypes[pti].name == "TimeThief") { piecetypes[pti].SpecialPiece_ind = SpecialPiece.TimeThief; }
                else if (this.piecetypes[pti].name == "Joker") { piecetypes[pti].SpecialPiece_ind = SpecialPiece.Joker; }
                else if (this.piecetypes[pti].name == "Elf") { piecetypes[pti].SpecialPiece_ind = SpecialPiece.Elf; }
                else { piecetypes[pti].SpecialPiece_ind = SpecialPiece.normalpiece; }
            }
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
            pposition.WhiteJokerSubstitute_pti = -1;
            pposition.BlackJokerSubstitute_pti = -1;
            pposition.WhiteElfMoveType = MoveType.other;
            pposition.BlackElfMoveType = MoveType.other;
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
                    pposition.squareInfo[i, j].adjacent_whitefemmefatale = false;
                    pposition.squareInfo[i, j].adjacent_blackfemmefatale = false;
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
            pposition.POKingInCheckTimeThief = false;

            pposition.precedingmove[0] = -1;
            pposition.precedingmove[1] = -1;
            pposition.precedingmove[2] = -1;
            pposition.precedingmove[3] = -1;

            pposition.WhiteBareKing = true;//true not false !!!
            pposition.BlackBareKing = true;//true not false !!!
            pposition.WhiteBishoponWhite = false;
            pposition.WhiteBishoponBlack = false;
            pposition.BlackBishoponWhite = false;
            pposition.BlackBishoponBlack = false;
            pposition.WhiteHasMatingMaterial = false;
            pposition.BlackHasMatingMaterial = false;
            pposition.WhiteHasWitch = false;
            pposition.BlackHasWitch = false;
            pposition.WhiteHasFemmeFatale = false;
            pposition.BlackHasFemmeFatale = false;
            pposition.WhiteHasJoker = false;
            pposition.BlackHasJoker = false;
            pposition.WhiteHasElf = false;
            pposition.BlackHasElf = false;
        }
        public void AllocateMovelist(ref chessposition pposition)
        {
            pposition.movelist = null;
            pposition.movelist = new chessmove[movelist_allocated];
            pposition.moveprioindex = new int[movelist_allocated];
            for (int mi = 0; mi < movelist_allocated; mi++)
            {
                pposition.movelist[mi].MovingPiece = 0;
                pposition.movelist[mi].coordinates = new int[4] { 0, 0, 0, 0};
                pposition.movelist[mi].IsEnPassant = false;
                pposition.movelist[mi].IsCapture = false;
                pposition.movelist[mi].IsCastling = false;
                pposition.movelist[mi].othercoordinates = new int[4] { -1, -1, -1, -1 };
                pposition.movelist[mi].PromoteToPiece = 0;
                pposition.movelist[mi].calculatedvalue = 0;
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
                    return 4.9;
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
        public void LocatePieces(ref chessposition pposition)
        {
            bool whitesquare;
            int whiteknightcount = 0;
            int blackknightcount = 0;

            //If we go from left to right then we should find queensiderooks first
            for (int i = 0; i < pposition.boardwidth; i++)
            {
                for (int j = 0; j < pposition.boardheight; j++)
                {

                    whitesquare = MyBoardTopology.IsWhiteSquare[i, j];

                    if (pposition.squares[i, j] != 0)
                    {
                        int pti = this.pieceTypeIndex(pposition.squares[i, j]);
                        if (this.piecetypes[pti].SpecialPiece_ind == SpecialPiece.King)
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
                        else if (this.piecetypes[pti].SpecialPiece_ind == SpecialPiece.Rook)
                        {
                            if (pposition.squares[i, j] > 0)
                            {
                                pposition.WhiteHasMatingMaterial = true;
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
                                pposition.BlackHasMatingMaterial = true;
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
                        else if (this.piecetypes[pti].SpecialPiece_ind == SpecialPiece.Bishop)
                        {
                            if (pposition.squares[i, j] > 0)
                            {
                                if (whitesquare) { pposition.WhiteBishoponWhite = true; }
                                else { pposition.WhiteBishoponBlack = true; }
                            }
                            else
                            {
                                if (whitesquare) { pposition.BlackBishoponWhite = true; }
                                else { pposition.BlackBishoponBlack = true; }
                            }
                        }
                        else if (this.piecetypes[pti].SpecialPiece_ind == SpecialPiece.Knight)
                        {
                            if (pposition.squares[i, j] > 0)
                            {
                                whiteknightcount += 1;
                            }
                            else
                            {
                                blackknightcount += 1;
                            }
                        }
                        else if (this.piecetypes[pti].SpecialPiece_ind == SpecialPiece.Witch)
                        {
                            if (pposition.squares[i, j] > 0)
                            {
                                pposition.WhiteHasWitch = true;
                            }
                            else
                            {
                                pposition.BlackHasWitch = true;
                            }
                        }
                        else if (this.piecetypes[pti].SpecialPiece_ind == SpecialPiece.Elf)
                        {
                            if (pposition.squares[i, j] > 0)
                            {
                                pposition.WhiteHasElf = true;
                            }
                            else
                            {
                                pposition.BlackHasElf = true;
                            }
                        }
                        else
                        {
                            //Now other piece, not King, not Rook, not Bishop, not Knight, not Witch, not Elf:
                            if (pposition.squares[i, j] > 0)
                            {
                                pposition.WhiteHasMatingMaterial = true;
                            }
                            else
                            {
                                pposition.BlackHasMatingMaterial = true;
                            }
                        }
                        //Also detect (lack of) bare King situation
                        if (this.piecetypes[pti].SpecialPiece_ind != SpecialPiece.King)
                        {
                            if (pposition.squares[i, j] > 0)
                            {
                                pposition.WhiteBareKing = false;
                            }
                            else
                            {
                                pposition.BlackBareKing = false;
                            }
                        }
                        if (this.piecetypes[pti].SpecialPiece_ind == SpecialPiece.Joker)
                        {
                            if (pposition.squares[i, j] > 0)
                            {
                                pposition.WhiteHasJoker = true;
                            }
                            else
                            {
                                pposition.BlackHasJoker = true;
                            }
                        }
                        if (this.piecetypes[pti].SpecialPiece_ind == SpecialPiece.FemmeFatale)
                        {
                            if (pposition.squares[i, j] > 0)
                            {
                                pposition.WhiteHasFemmeFatale = true;
                            }
                            else
                            {
                                pposition.BlackHasFemmeFatale = true;
                            }
                        }
                    }
                }
            }
            if (pposition.WhiteBishoponWhite == true & pposition.WhiteBishoponBlack == true)
            {
                pposition.WhiteHasMatingMaterial = true;
            }
            if (pposition.BlackBishoponWhite == true & pposition.BlackBishoponBlack == true)
            {
                pposition.BlackHasMatingMaterial = true;
            }
            if (pposition.WhiteBishoponWhite == true || pposition.WhiteBishoponBlack == true)
            {
                if (whiteknightcount > 0)
                {
                    pposition.WhiteHasMatingMaterial = true;
                }
            }
            if (pposition.BlackBishoponWhite == true || pposition.BlackBishoponBlack == true)
            {
                if (blackknightcount > 0)
                {
                    pposition.BlackHasMatingMaterial = true;
                }
            }
            if (whiteknightcount > 1)
            {
                pposition.WhiteHasMatingMaterial = true;
            }
            if (blackknightcount > 1)
            {
                pposition.BlackHasMatingMaterial = true;
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
                        if (this.piecetypes[pti].SpecialPiece_ind == SpecialPiece.King)
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

            //Assigning points for giving check did not help at all!!!
            //if (WhiteKingIsInCheck(ref pposition))
            //{
            //    resultev -= 5;
            //}
            //if (BlackKingIsInCheck(ref pposition))
            //{
            //    resultev += 5;
            //}

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
            //Minimum/maximum score for 'soft' results should be -80/80 respectively !!!
            double myev;

            if (pposition.WhiteBareKing == true & pposition.BlackHasMatingMaterial == true)
            {
                myev = MyWeirdEngineBareKingMate.MateBareKing(ref pposition);
                return myev;
            }
            else if (pposition.BlackBareKing == true & pposition.WhiteHasMatingMaterial == true)
            {
                myev = MyWeirdEngineBareKingMate.MateBareKing(ref pposition);
                return myev;
            }

            //double myev = EvaluationByMaterial(ref pposition);
            myev = EvaluationByAttack(ref pposition);
            return myev;
        }
        public void SetBoardTopology(int pboardwidth, int pboardheight)
        {
            MyBoardTopology.boardwidth = pboardwidth;
            MyBoardTopology.boardheight = pboardheight;
            MyBoardTopology.IsWhiteSquare = null;
            MyBoardTopology.IsWhiteSquare = new bool[pboardwidth, pboardheight];
            for (int i = 0; i < pboardwidth; i++)
            {
                for (int j = 0; j < pboardheight; j++)
                {
                    if ((i + j) % 2 == 0) { MyBoardTopology.IsWhiteSquare[i, j] = true; }
                    else { MyBoardTopology.IsWhiteSquare[i, j] = false; }
                }
            }

            MyBoardTopology.DistanceBetweenSquares = null;
            MyBoardTopology.DistanceBetweenSquares = new int[pboardwidth, pboardheight, pboardwidth, pboardheight];
            MyBoardTopology.SquaresAdjacent = null;
            MyBoardTopology.SquaresAdjacent = new bool[pboardwidth, pboardheight, pboardwidth, pboardheight];
            for (int i1 = 0; i1 < pboardwidth; i1++)
            {
                for (int j1 = 0; j1 < pboardheight; j1++)
                {
                    for (int i2 = 0; i2 < pboardwidth; i2++)
                    {
                        for (int j2 = 0; j2 < pboardheight; j2++)
                        {
                            int di = Math.Abs(i1 - i2);
                            int dj = Math.Abs(j1 - j2);
                            int d = di + dj;
                            MyBoardTopology.DistanceBetweenSquares[i1, j1, i2, j2] = d;
                            if (di <= 1 & dj <= 1) { MyBoardTopology.SquaresAdjacent[i1, j1, i2, j2] = true; }
                            else { MyBoardTopology.SquaresAdjacent[i1, j1, i2, j2] = false; }
                        }
                    }
                }
            }
        }
        public void init_positionstack(int pboardwidth, int pboardheight)
        {
            this.SetBoardTopology(pboardwidth, pboardheight);
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
            topos.WhiteJokerSubstitute_pti = frompos.WhiteJokerSubstitute_pti;
            topos.BlackJokerSubstitute_pti = frompos.BlackJokerSubstitute_pti;
            topos.WhiteElfMoveType = frompos.WhiteElfMoveType;
            topos.BlackElfMoveType = frompos.BlackElfMoveType;

            for (int i = 0; i < frompos.boardwidth; i++)
            {
                for (int j = 0; j < frompos.boardheight; j++)
                {
                    topos.squares[i, j] = frompos.squares[i, j];
                }
            }
            this.ClearNonPersistent(ref topos);
        }
        public bool IsValidPosition(ref chessposition pposition)
        {
            int whitekingcount = 0;
            int blackkingcount = 0;
            for (int i = 0; i < pposition.boardwidth; i++)
            {
                for (int j = 0; j < pposition.boardheight; j++)
                {
                    if (pposition.squares[i, j] != 0)
                    {
                        int pti = this.pieceTypeIndex(pposition.squares[i, j]);
                        if (this.piecetypes[pti].SpecialPiece_ind == SpecialPiece.King)
                        {
                            if (pposition.squares[i, j] > 0)
                            {
                                whitekingcount++;
                                if (whitekingcount > 1)
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                blackkingcount++;
                                if (blackkingcount > 1)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }
        public calculationresponse Calculation_tree(int requested_depth)
        {
            this.Set_SpecialPiece_ind();
            this.MyWeirdEnginePositionCompare.InitRepetitionCounter();
            this.MyWeirdEnginePositionCompare.AllocateTransTable();
            this.MyWeirdEnginePositionCompare.TransTable_no_positions_reused = 0;
            //this.MyWeirdEnginePositionCompare.TestItemsIntoTransTable();
            this.MyWeirdEngineJson.SetLogfilename();
            calculationresponse myresult;

            if (myenginesettings.display_when_depth_gt < requested_depth - 2 & requested_depth > 8)
            {
                myenginesettings.display_when_depth_gt = requested_depth - 2;
            }

            if (myenginesettings.consult_tt_when_depth_gt > myenginesettings.store_in_tt_when_depth_gt)
            {
                MyWeirdEngineJson.writelog("Invalid settings");
                MessageBox.Show("Invalid settings");
                myresult.posvalue = 0;
                myresult.moveidx = -1;
                myresult.POKingIsInCheck = false;
                return myresult;
            }
            if (IsValidPosition(ref positionstack[0]) == false)
            {
                MyWeirdEngineJson.writelog("Invalid position in method Calculation_tree");
                MessageBox.Show("Invalid position in method Calculation_tree");
                myresult.posvalue = 0;
                myresult.moveidx = -1;
                myresult.POKingIsInCheck = false;
                return myresult;
            }

            MyWeirdEngineJson.LogAllSettings();

            if (HasPreviousPosition() == true)
            {
                this.LocatePieces(ref positionstack[positionstack.Length - 1]);
                MyWeirdEngineMoveGenerator.SetWitchInfluence(ref positionstack[positionstack.Length - 1]);
                MyWeirdEngineMoveGenerator.SetFemmeFataleInfluence(ref positionstack[positionstack.Length - 1]);
            }

            this.nodecount = 0;
            this.externalabort = false;
            myresult = this.Calculation_tree_internal(0, -100, 100, requested_depth,
                                      this.myenginesettings.setting_SearchForFastestMate);

            if (requested_depth > myenginesettings.display_when_depth_gt)
            {
                MyWeirdEngineJson.writelog("End of calculation --> nodecount " + this.nodecount.ToString());
                MyWeirdEngineJson.writelog("Reused from transposition table " +
                    this.MyWeirdEnginePositionCompare.TransTable_no_positions_reused.ToString()
                    + " available " + MyWeirdEnginePositionCompare.TransTable_no_items_available.ToString());

                MyWeirdEngineJson.DumpTranspositionTable();
            }
            return myresult;
        }
        public int FindMove(chessposition pposition, chessmove mv)
        {
            for (int movei = 0; movei < pposition.movelist_totalfound; movei++)
            {
                if (MyWeirdEnginePositionCompare.MovesAreEqual(mv, pposition.movelist[movei]))
                {
                    return movei;
                }
            }
            return -1;
        }
        public void prioritize_one_move(int posidx, chessmove mv)
        {
            int movei = FindMove(positionstack[posidx], mv);
            if (movei == -1)
            {
                return;
            }
            else
            {
                int movecount = positionstack[posidx].movelist_totalfound;
                int[] worklist = new int[movecount];
                for (int iw = 0; iw < movecount; iw++)
                {
                    worklist[iw] = positionstack[posidx].moveprioindex[iw];
                }
                positionstack[posidx].moveprioindex[0] = movei;
                int iw2 = 0;
                for (int ic = 1; ic < movecount; ic++)
                {
                    if (worklist[iw2] == movei)
                    {
                        iw2++;
                    }
                    positionstack[posidx].moveprioindex[ic] = worklist[iw2];
                    iw2++;
                }
            }
        }
        public void set_moveprioindex(int posidx)
        {
            int movecount = positionstack[posidx].movelist_totalfound;
            movePrioItem[] workarray = new movePrioItem[movecount];

            for (int i = 0; i < movecount; i++)
            {
                workarray[i].moveidx = i;
                workarray[i].movevalue = positionstack[posidx].movelist[i].calculatedvalue;
            }

            if (positionstack[posidx].colourtomove == 1)
            {
                //order list by movevalue descending so best move for white first
                Array.Sort<movePrioItem>(workarray, (x, y) => y.movevalue.CompareTo(x.movevalue));
            }
            else
            {
                //order list by movevalue ascending so best move for black first
                Array.Sort<movePrioItem>(workarray, (x, y) => x.movevalue.CompareTo(y.movevalue));
            }
            for (int movei = 0; movei < movecount; movei++)
            {
                positionstack[posidx].moveprioindex[movei] = workarray[movei].moveidx;
            }
        }
        public void reprioritize_movelist(int posidx, double alpha, double beta, int prevposidx)
        {
            int stopi = -1;
            int movecount = positionstack[posidx].movelist_totalfound;

            for (int i = 0; i < movecount; i++)
            {
                int newposidx = ExecuteMove(posidx, positionstack[posidx].movelist[i], prevposidx);
                calculationresponse newresponse = Calculation_tree_internal(newposidx, alpha, beta,
                                                                     myenginesettings.presort_using_depth, false);
                positionstack[posidx].movelist[i].calculatedvalue = newresponse.posvalue;
                if (positionstack[posidx].colourtomove == 1)
                {
                    if (newresponse.posvalue >= 100)
                    {
                        stopi = i;
                        break;
                    }
                }
                if (positionstack[posidx].colourtomove == -1)
                {
                    if (newresponse.posvalue <= -100)
                    {
                        stopi = i;
                        break;
                    }
                }
            }
            if (stopi > -1)
            {
                for (int j = stopi + 1;j < movecount; j++)
                {
                    if (positionstack[posidx].colourtomove == 1)
                               { positionstack[posidx].movelist[j].calculatedvalue = -100; }
                    if (positionstack[posidx].colourtomove == -1)
                               { positionstack[posidx].movelist[j].calculatedvalue = 100; }
                }
            }
            set_moveprioindex(posidx);
        }
        public int Adjusted_newdepth(int newdepth, int colourtomove, double foundvalue)
        {
            int adjusteddepth;
            if (colourtomove == 1)
            {
                if (foundvalue <= 95 || foundvalue >= 100)
                {
                    return newdepth;
                }
                adjusteddepth = (int)Math.Round(((100 - foundvalue) * 10) + 1);
                return Math.Min(newdepth, adjusteddepth);
            }
            else
            {
                if (foundvalue >= -95 || foundvalue <= -100)
                {
                    return newdepth;
                }
                adjusteddepth = (int)Math.Round(((foundvalue + 100) * 10) + 1);
                return Math.Min(newdepth, adjusteddepth);
            }
        }
        public int newdepth_if_presort_found_mate(int posidx, int pdepth)
        {
            int suggesteddepth = myenginesettings.presort_using_depth + 1;
            if (pdepth <= suggesteddepth) { return pdepth; }
            if (positionstack[posidx].colourtomove == 1 &
                positionstack[posidx].movelist[positionstack[posidx].moveprioindex[0]].calculatedvalue >= 100)
            {
                return suggesteddepth;
            }
            if (positionstack[posidx].colourtomove == -1 &
                positionstack[posidx].movelist[positionstack[posidx].moveprioindex[0]].calculatedvalue <= -100)
            {
                return suggesteddepth;
            }
            return pdepth;
        }
        public calculationresponse Calculation_tree_internal(int posidx, double alpha, double beta,
                                                                int pdepth, bool SearchForFastestMate)
        {
            if (pdepth > 2) { Application.DoEvents(); }
            this.nodecount += 1;
            calculationresponse myresult;
            myresult.posvalue = 0.0;
            myresult.moveidx = -1;
            myresult.POKingIsInCheck = false;

            MyWeirdEnginePositionCompare.SetRepetitionCounter(posidx);
            if (positionstack[posidx].RepetitionCounter >= 2)
            {
                //MessageBox.Show("Found 2fold rep situation");
                myresult.posvalue = 0.0;
                return myresult;
            }

            this.LocatePieces(ref positionstack[posidx]);

            myresult.posvalue = CheckKingsPresent(ref positionstack[posidx]);
            if (myresult.posvalue == 100 || myresult.posvalue == -100)
            {
                return myresult;
            }

            MyWeirdEngineMoveGenerator.SetWitchInfluence(ref positionstack[posidx]);
            MyWeirdEngineMoveGenerator.SetFemmeFataleInfluence(ref positionstack[posidx]);

            int prevposidx = posidx - 1;
            if (prevposidx == -1)
            {
                if (HasPreviousPosition())
                {
                    prevposidx = positionstack.Length - 1;
                }
            }
            MyWeirdEngineMoveGenerator.GetAttacksMoves(ref positionstack[posidx], pdepth, prevposidx);

            if (MyWeirdEngineMoveGenerator.POKingIsInCheck(ref positionstack[posidx]) == true)
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

            if (DrawByMaterial(ref positionstack[posidx]) == true)
            {
                myresult.posvalue = 0.0;
                return myresult;
            }

            if (pdepth == 0)
            {
                myresult.posvalue = StaticEvaluation(ref positionstack[posidx]);
                return myresult;
            }

            int movecount = positionstack[posidx].movelist_totalfound;

            //Here search the transposition table for current position
            int t_naive_match = -1;
            int t_reuse_nr = -1;
            int t_prio_ordermatch = -1;
            if (pdepth > myenginesettings.consult_tt_when_depth_gt)
            {
                if (pdepth > this.myenginesettings.display_when_depth_gt)
                {
                    MyWeirdEngineJson.writelog("Start search in transposition table | available "
                        + MyWeirdEnginePositionCompare.TransTable_no_items_available.ToString()
                        + " used " + MyWeirdEnginePositionCompare.TransTable_no_positions_reused.ToString());
                }
                searchresult a = MyWeirdEnginePositionCompare.SearchTransTable(positionstack[posidx],
                                                                               pdepth, alpha, beta);
                t_naive_match = a.naivematch;
                t_prio_ordermatch = a.prio_ordermatch;
                t_reuse_nr = a.reusematch;
                if (t_reuse_nr > -1)
                {
                    int movei = FindMove(positionstack[posidx], MyWeirdEnginePositionCompare.TransTable[t_reuse_nr].bestmove);
                    if (movei > -1)
                    {
                        this.MyWeirdEnginePositionCompare.TransTable_no_positions_reused += 1;
                        myresult.posvalue = MyWeirdEnginePositionCompare.TransTable[t_reuse_nr].calculated_value;
                        myresult.moveidx = movei;
                        //myresult.POKingIsInCheck = false; Never store a position for which POKingIsInCheck == true!!!
                        return myresult;
                    }
                    MessageBox.Show("IMPOSSIBLE stored move not found amongst generated moves!!!");
                }
            }

            //this.MyWeirdEngineJson.writelog(this.MyWeirdEngineJson.DisplayMovelist(ref positionstack[posidx]));
            //MessageBox.Show(this.MyWeirdEngineJson.DisplayMovelist(ref positionstack[posidx]));
            //MessageBox.Show(this.MyWeirdEngineJson.DisplayAttacks(ref positionstack[posidx]));

            double new_alpha = alpha;
            double new_beta = beta;

            //presort BEGIN
            if (pdepth > this.myenginesettings.presort_when_depth_gt)
            {
                if (this.externalabort == true)
                {
                    return myresult;
                }
                if (pdepth > this.myenginesettings.display_when_depth_gt)
                {
                    string s = "List before sorting : ";
                    s += this.MyWeirdEngineJson.DisplayMovelist(positionstack[posidx], false);
                    this.MyWeirdEngineJson.writelog(s);
                }

                reprioritize_movelist(posidx, new_alpha, new_beta, prevposidx);

                if (pdepth > this.myenginesettings.display_when_depth_gt)
                {
                    string s = "List after sorting : ";
                    s += this.MyWeirdEngineJson.DisplayMovelist(positionstack[posidx], true);
                    this.MyWeirdEngineJson.writelog(s);
                }
            }
            //presort END
            if (t_prio_ordermatch > -1)
            {
                if (pdepth > this.myenginesettings.display_when_depth_gt)
                {
                    string s = "pdepth " + pdepth.ToString() + " t_prio_ordermatch " + t_prio_ordermatch.ToString();
                    s += " stored depth : " + MyWeirdEnginePositionCompare.TransTable[t_prio_ordermatch].used_depth.ToString();
                    s += " List before apply best move from transition table : ";
                    s += this.MyWeirdEngineJson.DisplayMovelist(positionstack[posidx], true);
                    this.MyWeirdEngineJson.writelog(s);
                }

                prioritize_one_move(posidx, MyWeirdEnginePositionCompare.TransTable[t_prio_ordermatch].bestmove);

                if (pdepth > this.myenginesettings.display_when_depth_gt)
                {
                    string s = "List after apply best move from transition table : ";
                    s += this.MyWeirdEngineJson.DisplayMovelist(positionstack[posidx], true);
                    this.MyWeirdEngineJson.writelog(s);
                }
            }

            int bestmoveidx = -1;
            double bestmovevalue = 0;

            int newdepth = newdepth_if_presort_found_mate(posidx, pdepth);//returns pdepth by default

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
                int newposidx = ExecuteMove(posidx, positionstack[posidx].movelist[positionstack[posidx].moveprioindex[i]], prevposidx);
                calculationresponse newresponse = Calculation_tree_internal(newposidx, new_alpha, new_beta,
                                                                               newdepth - 1, SearchForFastestMate);
                positionstack[posidx].movelist[positionstack[posidx].moveprioindex[i]].calculatedvalue = newresponse.posvalue;
                if (pdepth > this.myenginesettings.display_when_depth_gt)
                {
                    string mvstr = MyWeirdEngineJson.ShortNotation(positionstack[posidx].movelist[positionstack[posidx].moveprioindex[i]], false);
                    MyWeirdEngineJson.writelog("pdepth " + pdepth.ToString() + " newdepth " + newdepth.ToString() + " DONE checking move "
                        + mvstr + " alpha " + new_alpha.ToString() + " beta " + new_beta.ToString()
                        + " posvalue " + newresponse.posvalue.ToString());
                }
                if (newresponse.POKingIsInCheck == false)
                {
                    noescapecheck = false;
                }

                if (this.positionstack[posidx].colourtomove == 1)
                {
                    if (newresponse.posvalue > bestmovevalue )
                    {
                        bestmovevalue = newresponse.posvalue;
                        bestmoveidx = positionstack[posidx].moveprioindex[i];
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
                        bestmoveidx = positionstack[posidx].moveprioindex[i];
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
                newdepth = Adjusted_newdepth(newdepth, this.positionstack[posidx].colourtomove, newresponse.posvalue);
            }

            //Mate
            if (MyWeirdEngineMoveGenerator.PMKingIsInCheck(ref positionstack[posidx]) == true & noescapecheck == true)
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
            if (MyWeirdEngineMoveGenerator.PMKingIsInCheck(ref positionstack[posidx]) == false & noescapecheck == true)
            {
                myresult.posvalue = 0;
                return myresult;
            }

            myresult.posvalue = bestmovevalue;

            //Mate      (in 0 plies) requires 1 ply score +/-100
            //Mate in 1 (in 1 ply) requires 2 plies score +/-99.9
            //Mate      (in 2 plies) requires 3 plies score +/-99.8
            //Mate in 2 (in 3 plies) requires 4 plies score +/-99.7
            //Mate      (in 4 plies) requires 5 plies score +/-99.6
            //Mate in 3 (in 5 plies) requires 6 plies score +/-99.5
            //Mate      (in 6 plies) requires 7 plies score +/-99.4
            //Mate in 4 (in 7 plies) requires 8 plies score +/-99.3
            //etc
            //-80 and 80 are the lower/upper limits for soft evaluation results
            if (SearchForFastestMate == true)
            {
                //This comes with SLOWNESS!!!! because now it keeps looking for a faster forced mate
                if (myresult.posvalue > 95)
                {
                    myresult.posvalue = Math.Round(myresult.posvalue - 0.1, 3);
                }
                if (myresult.posvalue < -95)
                {
                    myresult.posvalue = Math.Round(myresult.posvalue + 0.1, 3);
                }
            }

            myresult.moveidx = bestmoveidx;

            //Here store into transposition table
            if (pdepth > myenginesettings.store_in_tt_when_depth_gt)
            {
                MyWeirdEnginePositionCompare.StorePosition(positionstack[posidx], t_naive_match,
                                                           positionstack[posidx].movelist[bestmoveidx],
                                                           pdepth, alpha, beta, myresult.posvalue);
            }

            return myresult;
        }
        public void ApplyImitators(int posidx, int newposidx, chessmove pmove, int pti)
        {
            //JokerInfo begin
            if (positionstack[posidx].colourtomove == 1)
            {
                if (piecetypes[pti].SpecialPiece_ind == SpecialPiece.Joker)
                {
                    positionstack[newposidx].BlackJokerSubstitute_pti = positionstack[posidx].WhiteJokerSubstitute_pti;
                }
                else if (positionstack[posidx].BlackHasJoker == false)
                {
                    positionstack[newposidx].BlackJokerSubstitute_pti = -1;
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
                if (piecetypes[pti].SpecialPiece_ind == SpecialPiece.Joker)
                {
                    positionstack[newposidx].WhiteJokerSubstitute_pti = positionstack[posidx].BlackJokerSubstitute_pti;
                }
                else if (positionstack[posidx].WhiteHasJoker == false)
                {
                    positionstack[newposidx].WhiteJokerSubstitute_pti = -1;
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

            //ElfInfo begin
            if (positionstack[posidx].colourtomove == 1)
            {
                if (this.piecetypes[pti].SpecialPiece_ind == SpecialPiece.TimeThief & pmove.IsCapture == true)
                {
                    //special move
                    positionstack[newposidx].BlackElfMoveType = MoveType.other;
                }
                else if (pmove.IsCapture == true)
                {
                    positionstack[newposidx].BlackElfMoveType = MoveType.Capture;
                }
                else if (positionstack[posidx].BlackHasElf == false)
                {
                    positionstack[newposidx].BlackElfMoveType = MoveType.other;
                }
                else
                {
                    positionstack[newposidx].BlackElfMoveType = MoveType.Noncapture;
                }
            }
            else
            {
                if (this.piecetypes[pti].SpecialPiece_ind == SpecialPiece.TimeThief & pmove.IsCapture == true)
                {
                    //special move
                    positionstack[newposidx].WhiteElfMoveType = MoveType.other;
                }
                else if (pmove.IsCapture == true)
                {
                    positionstack[newposidx].WhiteElfMoveType = MoveType.Capture;
                }
                else if (positionstack[posidx].WhiteHasElf == false)
                {
                    positionstack[newposidx].WhiteElfMoveType = MoveType.other;
                }
                else
                {
                    positionstack[newposidx].WhiteElfMoveType = MoveType.Noncapture;
                }
            }
            //ElfInfo end

        }
        public int ExecuteMove(int posidx, chessmove pmove, int prevposidx)
        {
            int newposidx = posidx + 1;
            int pti = pieceTypeIndex(pmove.MovingPiece);

            if (this.piecetypes[pti].SpecialPiece_ind == SpecialPiece.TimeThief & prevposidx >= 0 & pmove.IsCapture == true)
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
            if (this.piecetypes[pti].SpecialPiece_ind == SpecialPiece.King)
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
            else if (this.piecetypes[pti].SpecialPiece_ind == SpecialPiece.Rook)
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

            ApplyImitators(posidx, newposidx, pmove, pti);


            return newposidx;
        }
    }
}
