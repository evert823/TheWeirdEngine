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
    public class WeirdEngineMoveGenerator
    {
        //Everything w.r.t. generation of legal moves
        public WeirdEngineMoveFinder MyWeirdEngineMoveFinder;
        public WeirdEngineMoveGenerator(WeirdEngineMoveFinder pWeirdEngineMoveFinder)
        {
            this.MyWeirdEngineMoveFinder = pWeirdEngineMoveFinder;
        }


        public void SetWitchInfluence(ref chessposition pposition)
        {
            if (pposition.WhiteHasWitch == false & pposition.BlackHasWitch == false)
            {
                return;
            }
            for (int i = 0; i < pposition.boardwidth; i++)
            {
                for (int j = 0; j < pposition.boardheight; j++)
                {
                    if (pposition.squares[i, j] != 0)
                    {
                        int pti = MyWeirdEngineMoveFinder.pieceTypeIndex(pposition.squares[i, j]);
                        if (MyWeirdEngineMoveFinder.piecetypes[pti].SpecialPiece_ind == SpecialPiece.Witch)
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
        public void SetFemmeFataleInfluence(ref chessposition pposition)
        {
            if (pposition.WhiteHasFemmeFatale == false & pposition.BlackHasFemmeFatale == false)
            {
                return;
            }
            for (int i = 0; i < pposition.boardwidth; i++)
            {
                for (int j = 0; j < pposition.boardheight; j++)
                {
                    if (pposition.squares[i, j] != 0)
                    {
                        int pti = MyWeirdEngineMoveFinder.pieceTypeIndex(pposition.squares[i, j]);
                        if (this.MyWeirdEngineMoveFinder.piecetypes[pti].SpecialPiece_ind == SpecialPiece.FemmeFatale)
                        {
                            for (int i2 = i - 1; i2 < i + 2; i2++)
                            {
                                for (int j2 = j - 1; j2 < j + 2; j2++)
                                {
                                    if (i2 >= 0 & i2 < pposition.boardwidth & j2 >= 0 & j2 < pposition.boardheight)
                                    {
                                        if (pposition.squares[i, j] > 0)
                                        {
                                            pposition.squareInfo[i2, j2].adjacent_whitefemmefatale = true;
                                        }
                                        else
                                        {
                                            pposition.squareInfo[i2, j2].adjacent_blackfemmefatale = true;
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
                    pposition.squareInfo[x, y].AttackedByPM += 1;
                }
                else
                {
                    pposition.squareInfo[x, y].AttackedByPO += 1;
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
        public void AssignCapturedValue(chessposition pposition, ref chessmove mv, int i2, int j2)
        {
            int pti2 = MyWeirdEngineMoveFinder.pieceTypeIndex(pposition.squares[i2, j2]);
            mv.CapturedValue = MyWeirdEngineMoveFinder.piecetypes[pti2].EstimatedValue;
        }
        public void Default_moveprioindex(ref chessposition pposition)
        {
            for (int movei = 0; movei < pposition.movelist_totalfound; movei++)
            {
                pposition.moveprioindex[movei] = movei;
            }
        }
        public void GetAttacksMoves(ref chessposition pposition, int depth, int prevposidx)
        {
            pposition.movelist_totalfound = 0;
            pposition.POKingInCheckTimeThief = false;
            for (int i = 0; i < pposition.boardwidth; i++)
            {
                for (int j = 0; j < pposition.boardheight; j++)
                {
                    if (pposition.squares[i, j] != 0)
                    {
                        this.GetSlideAttacksMoves(ref pposition, i, j, depth);
                    }
                    if (depth > 0)
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
                        GetTimeThiefCapture(ref pposition, i, j, prevposidx, depth);
                    }
                }
            }
            if (depth > 0)
            {
                GetCastling(ref pposition);
            }
            Default_moveprioindex(ref pposition);
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
        public FreezeType GetFreezeType(chessposition pposition, int i, int j, int pti_self)
        {
            FreezeType myresponse;
            myresponse.capturefreeze = false;
            myresponse.noncapturefreeze = false;
            if (pposition.squares[i, j] > 0)
            {
                if (MyWeirdEngineMoveFinder.piecetypes[pti_self].SpecialPiece_ind == SpecialPiece.Elf)
                {
                    if (pposition.WhiteElfMoveType == MoveType.other)
                    {
                        myresponse.capturefreeze = true;
                        myresponse.noncapturefreeze = true;
                    }
                    else if (pposition.WhiteElfMoveType == MoveType.Capture) { myresponse.noncapturefreeze = true; }
                    else { myresponse.capturefreeze = true; }
                }
                if (pposition.squareInfo[i, j].adjacent_blackfemmefatale == true)
                {
                    myresponse.capturefreeze = true;
                }
            }
            else
            {
                if (MyWeirdEngineMoveFinder.piecetypes[pti_self].SpecialPiece_ind == SpecialPiece.Elf)
                {
                    if (pposition.BlackElfMoveType == MoveType.other)
                    {
                        myresponse.capturefreeze = true;
                        myresponse.noncapturefreeze = true;
                    }
                    else if (pposition.BlackElfMoveType == MoveType.Capture) { myresponse.noncapturefreeze = true; }
                    else { myresponse.capturefreeze = true; }
                }
                if (pposition.squareInfo[i, j].adjacent_whitefemmefatale == true)
                {
                    myresponse.capturefreeze = true;
                }
            }
            return myresponse;
        }
        public bool SquareIsTransparent(ref chessposition pposition, int i, int j, int i2, int j2, int pti)
        {
            //A Witch is transparent for pieces of her own colour.
            //A Witch makes adjecent pieces (of any colour) transparent for pieces of her own colour.
            //A Witch does not make adjacent pieces transparent for herself. But another allied Witch can do that for her.
            bool IsTransparent = false;
            if (pposition.squares[i, j] > 0 & pposition.squareInfo[i2, j2].n_adjacent_whitewitches > 0)
            {
                if (MyWeirdEngineMoveFinder.piecetypes[pti].SpecialPiece_ind == SpecialPiece.Witch
                    & pposition.squareInfo[i2, j2].n_adjacent_whitewitches > 1)
                {
                    IsTransparent = true;
                }
                else
                {
                    if (MyWeirdEngineMoveFinder.piecetypes[pti].SpecialPiece_ind != SpecialPiece.Witch) { IsTransparent = true; }
                    if (MyWeirdEngineMoveFinder.MyBoardTopology.SquaresAdjacent[i, j, i2, j2] == false) { IsTransparent = true; }
                }
            }
            if (pposition.squares[i, j] < 0 & pposition.squareInfo[i2, j2].n_adjacent_blackwitches > 0)
            {
                if (MyWeirdEngineMoveFinder.piecetypes[pti].SpecialPiece_ind == SpecialPiece.Witch
                    & pposition.squareInfo[i2, j2].n_adjacent_blackwitches > 1)
                {
                    IsTransparent = true;
                }
                else
                {
                    if (MyWeirdEngineMoveFinder.piecetypes[pti].SpecialPiece_ind != SpecialPiece.Witch) { IsTransparent = true; }
                    if (MyWeirdEngineMoveFinder.MyBoardTopology.SquaresAdjacent[i, j, i2, j2] == false) { IsTransparent = true; }
                }
            }
            return IsTransparent;
        }
        public bool SquareSlideAttackedFromSquare(ref chessposition pposition, int i, int j, int i3, int j3,
                                                  vector v, int pti)
        {
            //Establish if [i3,j3] is attacked from [i,j] using Slide vector v yes or no
            // TODO: check code duplication w.r.t. other method GetSlideAttacksMovesPerVector
            // TODO: implement maxrange just like in GetSlideAttacksMovesPerVector
            //    (but Time Thief doesn't have maxrange so NOT NOW)

            FreezeType ft = GetFreezeType(pposition, i, j, pti);
            if (ft.capturefreeze == true) { return false; }

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
        public bool maxrange_exceeded(int maxrangecounter, vector v)
        {
            if (v.maxrange < 1) { return false; }
            if (maxrangecounter <= v.maxrange) { return false; }
            return true;
        }
        public void GetSlideAttacksMovesPerVector(ref chessposition pposition, int i, int j, vector v,
                                                  bool getcaptures, bool getnoncaptures, int depth, int pti,
                                                  int pti_self, FreezeType ft)
        {
            int i2;
            int j2;
            int movei;
            bool blocked;
            int maxrangecounter;

            //Efficiency 08-06-2025
            if (getcaptures == false)
            {
                if ((pposition.squares[i, j] < 0 & pposition.colourtomove > 0) ||
                    (pposition.squares[i, j] > 0 & pposition.colourtomove < 0))
                {
                    return;
                }
            }

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
            maxrangecounter = 1;

            while (i2 >= 0 & i2 < pposition.boardwidth & j2 >= 0 & j2 < pposition.boardheight
                & blocked == false & maxrange_exceeded(maxrangecounter, v) == false)
            {
                if (getcaptures == true & ft.capturefreeze == false)
                {
                    this.MarkAttacked(ref pposition, i2, j2, pposition.squares[i, j]);
                    if (depth > 0)
                    {
                        if ((pposition.squares[i2, j2] > 0 & pposition.squares[i, j] < 0 & pposition.colourtomove < 0) ||
                            (pposition.squares[i2, j2] < 0 & pposition.squares[i, j] > 0 & pposition.colourtomove > 0))
                        {
                            movei = pposition.movelist_totalfound;
                            InitializeMove(ref pposition, movei, i, j, i2, j2);
                            pposition.movelist[movei].MovingPiece = pposition.squares[i, j];
                            pposition.movelist[movei].IsCapture = true;
                            AssignCapturedValue(pposition, ref pposition.movelist[movei], i2, j2);
                            GetPromotion(ref pposition, movei, pti, pti_self);
                        }
                    }
                }
                if (getnoncaptures == true & depth > 0 & ft.noncapturefreeze == false)
                {
                    if ((pposition.squares[i2, j2] == 0 & pposition.squares[i, j] < 0 & pposition.colourtomove < 0) ||
                             (pposition.squares[i2, j2] == 0 & pposition.squares[i, j] > 0 & pposition.colourtomove > 0))
                    {
                        movei = pposition.movelist_totalfound;
                        InitializeMove(ref pposition, movei, i, j, i2, j2);
                        pposition.movelist[movei].MovingPiece = pposition.squares[i, j];
                        GetPromotion(ref pposition, movei, pti, pti_self);
                    }
                }
                if (pposition.squares[i2, j2] != 0)
                {
                    bool IsTransparent = SquareIsTransparent(ref pposition, i, j, i2, j2, pti_self);
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
                maxrangecounter++;
            }
        }
        public void GetSlideAttacksMoves(ref chessposition pposition, int i, int j, int depth)
        {
            int pti = MyWeirdEngineMoveFinder.pieceTypeIndex(pposition.squares[i, j]);
            int pti_self = pti;
            FreezeType ft = GetFreezeType(pposition, i, j, pti_self);
            if (MyWeirdEngineMoveFinder.piecetypes[pti].SpecialPiece_ind == SpecialPiece.Joker)
            {
                pti = jokersubspti(ref pposition, i, j, pti);
            }

            if (MyWeirdEngineMoveFinder.piecetypes[pti].IsDivergent == false)
            {
                foreach (vector v in MyWeirdEngineMoveFinder.piecetypes[pti].slidemovevectors)
                {
                    GetSlideAttacksMovesPerVector(ref pposition, i, j, v, true, true, depth, pti, pti_self, ft);
                }
            }
            else
            {
                foreach (vector v in MyWeirdEngineMoveFinder.piecetypes[pti].slidemovevectors)
                {
                    GetSlideAttacksMovesPerVector(ref pposition, i, j, v, false, true, depth, pti, pti_self, ft);
                }
                foreach (vector v in MyWeirdEngineMoveFinder.piecetypes[pti].slidecapturevectors)
                {
                    GetSlideAttacksMovesPerVector(ref pposition, i, j, v, true, false, depth, pti, pti_self, ft);
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
            tomove.CapturedValue = frommove.CapturedValue;
            tomove.IsCastling = frommove.IsCastling;
            tomove.othercoordinates[0] = frommove.othercoordinates[0];
            tomove.othercoordinates[1] = frommove.othercoordinates[1];
            tomove.othercoordinates[2] = frommove.othercoordinates[2];
            tomove.othercoordinates[3] = frommove.othercoordinates[3];
            tomove.PromoteToPiece = frommove.PromoteToPiece;
            tomove.calculatedvalue = frommove.calculatedvalue;
            tomove.number_of_no_selfcheck_resp = frommove.number_of_no_selfcheck_resp;
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
            pposition.movelist[movei].CapturedValue = 0;
            pposition.movelist[movei].IsCastling = false;
            pposition.movelist[movei].othercoordinates[0] = -1;
            pposition.movelist[movei].othercoordinates[1] = -1;
            pposition.movelist[movei].othercoordinates[2] = -1;
            pposition.movelist[movei].othercoordinates[3] = -1;
            pposition.movelist[movei].PromoteToPiece = 0;
        }
        public void DeleteLatestMoveIfDuplicate(ref chessposition pposition, int pti)
        {
            //Only because of duplication of vectors in inefficient piece definitions
            if (MyWeirdEngineMoveFinder.piecetypes[pti].CheckDuplicateMoves == false) { return; }

            bool IsDuplicateMove = false;
            int lmi = pposition.movelist_totalfound - 1;
            for (int movei = lmi - 1; movei >= 0; movei--)
            {
                if (MyWeirdEngineMoveFinder.MyWeirdEnginePositionCompare.MovesAreEqual(pposition.movelist[movei], pposition.movelist[lmi]) == true)
                {
                    IsDuplicateMove = true;
                    break;
                }
            }
            if (IsDuplicateMove == true)
            {
                MyWeirdEngineMoveFinder.Init_chessmove(ref pposition.movelist[lmi]);
                pposition.movelist_totalfound = lmi;
            }
        }
        public void GetPromotion(ref chessposition pposition, int movei, int pti, int pti_self)
        {
            bool includepromote = false;
            bool includenonpromote = false;

            if (MyWeirdEngineMoveFinder.piecetypes[pti_self].SpecialPiece_ind == SpecialPiece.Pawn)
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
            }
            else
            {
                includepromote = false;
                includenonpromote = true;
            }
            if (includenonpromote == true)
            {
                pposition.movelist_totalfound += 1;
                DeleteLatestMoveIfDuplicate(ref pposition, pti);
            }
            if (includepromote == true)
            {
                for (int pi = 0; pi < MyWeirdEngineMoveFinder.piecetypes.Length; pi++)
                {
                    if (pi == pti_self) { }//nothing
                    else if (MyWeirdEngineMoveFinder.piecetypes[pi].SpecialPiece_ind == SpecialPiece.King) { }//nothing
                    else if (MyWeirdEngineMoveFinder.piecetypes[pi].SpecialPiece_ind == SpecialPiece.Amazon) { }//nothing
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
                        DeleteLatestMoveIfDuplicate(ref pposition, pti);
                    }
                }
            }
        }
        public void GetTimeThiefCapture(ref chessposition pposition, int i, int j, int prevposidx, int depth)
        {
            int movei;
            int pti = MyWeirdEngineMoveFinder.pieceTypeIndex(pposition.squares[i, j]);
            FreezeType ft = GetFreezeType(pposition, i, j, pti);
            if (ft.capturefreeze == true)
            {
                return;
            }
            if (MyWeirdEngineMoveFinder.piecetypes[pti].SpecialPiece_ind != SpecialPiece.TimeThief)
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
            foreach (vector v in MyWeirdEngineMoveFinder.piecetypes[pti].slidemovevectors)
            {
                if (SquareSlideAttackedFromSquare(ref MyWeirdEngineMoveFinder.positionstack[prevposidx], i, j, i2, j2, v, pti) == true)
                {
                    int i3 = pposition.precedingmove[2];
                    int j3 = pposition.precedingmove[3];
                    int pti3 = MyWeirdEngineMoveFinder.pieceTypeIndex(pposition.squares[i3, j3]);
                    if (MyWeirdEngineMoveFinder.piecetypes[pti3].SpecialPiece_ind == SpecialPiece.King)
                    {
                        //King moved out of attack range of TimeThief
                        //That is equivalent with moving into check, and here we must detect this
                        pposition.POKingInCheckTimeThief = true;
                    }

                    if (depth > 0)
                    {
                        movei = pposition.movelist_totalfound;
                        InitializeMove(ref pposition, movei, i, j, i2, j2);
                        pposition.movelist[movei].IsCapture = true;
                        AssignCapturedValue(MyWeirdEngineMoveFinder.positionstack[prevposidx], ref pposition.movelist[movei], i2, j2);
                        pposition.movelist[movei].MovingPiece = pposition.squares[i, j];
                        pposition.movelist_totalfound += 1;
                        //DeleteLatestMoveIfDuplicate(ref pposition, pti);
                    }
                }
            }
        }
        public void GetPawn2StepMoves(ref chessposition pposition, int i, int j)
        {
            int pti = MyWeirdEngineMoveFinder.pieceTypeIndex(pposition.squares[i, j]);

            if (MyWeirdEngineMoveFinder.piecetypes[pti].SpecialPiece_ind == SpecialPiece.Joker)
            {
                pti = jokersubspti(ref pposition, i, j, pti);
            }

            if (MyWeirdEngineMoveFinder.piecetypes[pti].SpecialPiece_ind != SpecialPiece.Pawn)
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
                //DeleteLatestMoveIfDuplicate(ref pposition, pti);
            }
        }
        public void GetPawnEnPassantMoves(ref chessposition pposition, int i, int j)
        {
            int pti = MyWeirdEngineMoveFinder.pieceTypeIndex(pposition.squares[i, j]);

            if (MyWeirdEngineMoveFinder.piecetypes[pti].SpecialPiece_ind == SpecialPiece.Joker)
            {
                pti = jokersubspti(ref pposition, i, j, pti);
            }

            if (MyWeirdEngineMoveFinder.piecetypes[pti].SpecialPiece_ind != SpecialPiece.Pawn)
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
            int ptm = MyWeirdEngineMoveFinder.pieceTypeIndex(pposition.squares[x_to, y_to]);

            if (MyWeirdEngineMoveFinder.piecetypes[ptm].SpecialPiece_ind == SpecialPiece.Joker)
            {
                ptm = jokersubspti(ref pposition, x_to, y_to, ptm);
            }

            int movei;

            if (MyWeirdEngineMoveFinder.piecetypes[ptm].SpecialPiece_ind != SpecialPiece.Pawn)
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
                AssignCapturedValue(pposition, ref pposition.movelist[movei], x_to, y_to);
                pposition.movelist_totalfound += 1;
                //DeleteLatestMoveIfDuplicate(ref pposition, pti);
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
                AssignCapturedValue(pposition, ref pposition.movelist[movei], x_to, y_to);
                pposition.movelist_totalfound += 1;
                //DeleteLatestMoveIfDuplicate(ref pposition, pti);
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
                for (int i = 0; i < pposition.boardwidth; i++)
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
                //DeleteLatestMoveIfDuplicate(ref pposition, pti);
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
                //DeleteLatestMoveIfDuplicate(ref pposition, pti);
            }
        }

        public void ApplyImitators(int posidx, int newposidx, chessmove pmove, int pti)
        {
            //JokerInfo begin
            if (MyWeirdEngineMoveFinder.positionstack[posidx].colourtomove == 1)
            {
                if (MyWeirdEngineMoveFinder.piecetypes[pti].SpecialPiece_ind == SpecialPiece.Joker)
                {
                    MyWeirdEngineMoveFinder.positionstack[newposidx].BlackJokerSubstitute_pti = MyWeirdEngineMoveFinder.positionstack[posidx].WhiteJokerSubstitute_pti;
                }
                else if (MyWeirdEngineMoveFinder.positionstack[posidx].BlackHasJoker == false)
                {
                    MyWeirdEngineMoveFinder.positionstack[newposidx].BlackJokerSubstitute_pti = -1;
                }
                else if (pmove.PromoteToPiece != 0)
                {
                    MyWeirdEngineMoveFinder.positionstack[newposidx].BlackJokerSubstitute_pti = MyWeirdEngineMoveFinder.pieceTypeIndex(pmove.PromoteToPiece);
                }
                else
                {
                    MyWeirdEngineMoveFinder.positionstack[newposidx].BlackJokerSubstitute_pti = pti;
                }
            }
            else
            {
                if (MyWeirdEngineMoveFinder.piecetypes[pti].SpecialPiece_ind == SpecialPiece.Joker)
                {
                    MyWeirdEngineMoveFinder.positionstack[newposidx].WhiteJokerSubstitute_pti = MyWeirdEngineMoveFinder.positionstack[posidx].BlackJokerSubstitute_pti;
                }
                else if (MyWeirdEngineMoveFinder.positionstack[posidx].WhiteHasJoker == false)
                {
                    MyWeirdEngineMoveFinder.positionstack[newposidx].WhiteJokerSubstitute_pti = -1;
                }
                else if (pmove.PromoteToPiece != 0)
                {
                    MyWeirdEngineMoveFinder.positionstack[newposidx].WhiteJokerSubstitute_pti = MyWeirdEngineMoveFinder.pieceTypeIndex(pmove.PromoteToPiece);
                }
                else
                {
                    MyWeirdEngineMoveFinder.positionstack[newposidx].WhiteJokerSubstitute_pti = pti;
                }
            }
            //JokerInfo end

            //ElfInfo begin
            if (MyWeirdEngineMoveFinder.positionstack[posidx].colourtomove == 1)
            {
                if (MyWeirdEngineMoveFinder.piecetypes[pti].SpecialPiece_ind == SpecialPiece.TimeThief & pmove.IsCapture == true)
                {
                    //special move
                    MyWeirdEngineMoveFinder.positionstack[newposidx].BlackElfMoveType = MoveType.other;
                }
                else if (pmove.IsCapture == true)
                {
                    MyWeirdEngineMoveFinder.positionstack[newposidx].BlackElfMoveType = MoveType.Capture;
                }
                else if (MyWeirdEngineMoveFinder.positionstack[posidx].BlackHasElf == false)
                {
                    MyWeirdEngineMoveFinder.positionstack[newposidx].BlackElfMoveType = MoveType.other;
                }
                else
                {
                    MyWeirdEngineMoveFinder.positionstack[newposidx].BlackElfMoveType = MoveType.Noncapture;
                }
            }
            else
            {
                if (MyWeirdEngineMoveFinder.piecetypes[pti].SpecialPiece_ind == SpecialPiece.TimeThief & pmove.IsCapture == true)
                {
                    //special move
                    MyWeirdEngineMoveFinder.positionstack[newposidx].WhiteElfMoveType = MoveType.other;
                }
                else if (pmove.IsCapture == true)
                {
                    MyWeirdEngineMoveFinder.positionstack[newposidx].WhiteElfMoveType = MoveType.Capture;
                }
                else if (MyWeirdEngineMoveFinder.positionstack[posidx].WhiteHasElf == false)
                {
                    MyWeirdEngineMoveFinder.positionstack[newposidx].WhiteElfMoveType = MoveType.other;
                }
                else
                {
                    MyWeirdEngineMoveFinder.positionstack[newposidx].WhiteElfMoveType = MoveType.Noncapture;
                }
            }
            //ElfInfo end

        }
        public int ExecuteMove(int posidx, chessmove pmove, int prevposidx)
        {
            int newposidx = posidx + 1;
            int pti = MyWeirdEngineMoveFinder.pieceTypeIndex(pmove.MovingPiece);

            if (MyWeirdEngineMoveFinder.piecetypes[pti].SpecialPiece_ind == SpecialPiece.TimeThief & prevposidx >= 0 & pmove.IsCapture == true)
            {
                MyWeirdEngineMoveFinder.SynchronizePosition(ref MyWeirdEngineMoveFinder.positionstack[prevposidx], ref MyWeirdEngineMoveFinder.positionstack[newposidx]);
            }
            else
            {
                MyWeirdEngineMoveFinder.SynchronizePosition(ref MyWeirdEngineMoveFinder.positionstack[posidx], ref MyWeirdEngineMoveFinder.positionstack[newposidx]);
            }

            int i1 = pmove.coordinates[0];
            int j1 = pmove.coordinates[1];
            int i2 = pmove.coordinates[2];
            int j2 = pmove.coordinates[3];
            int i_qr = -1;
            int i_kr = -1;

            MyWeirdEngineMoveFinder.positionstack[newposidx].precedingmove[0] = i1;
            MyWeirdEngineMoveFinder.positionstack[newposidx].precedingmove[1] = j1;
            MyWeirdEngineMoveFinder.positionstack[newposidx].precedingmove[2] = i2;
            MyWeirdEngineMoveFinder.positionstack[newposidx].precedingmove[3] = j2;

            if (pmove.PromoteToPiece != 0)
            {
                MyWeirdEngineMoveFinder.positionstack[newposidx].squares[i2, j2] = pmove.PromoteToPiece;
            }
            else
            {
                MyWeirdEngineMoveFinder.positionstack[newposidx].squares[i2, j2] = pmove.MovingPiece;
            }
            this.MyWeirdEngineMoveFinder.positionstack[newposidx].squares[i1, j1] = 0;

            //Set castling info for new position BEGIN
            if (MyWeirdEngineMoveFinder.piecetypes[pti].SpecialPiece_ind == SpecialPiece.King)
            {
                if (MyWeirdEngineMoveFinder.positionstack[posidx].colourtomove == 1)
                {
                    MyWeirdEngineMoveFinder.positionstack[newposidx].whitekinghasmoved = true;
                }
                else
                {
                    MyWeirdEngineMoveFinder.positionstack[newposidx].blackkinghasmoved = true;
                }
            }
            else if (MyWeirdEngineMoveFinder.piecetypes[pti].SpecialPiece_ind == SpecialPiece.Rook)
            {
                if (this.MyWeirdEngineMoveFinder.positionstack[posidx].colourtomove == 1)
                {
                    i_qr = MyWeirdEngineMoveFinder.positionstack[posidx].whitequeensiderookcoord.x;
                    i_kr = MyWeirdEngineMoveFinder.positionstack[posidx].whitekingsiderookcoord.x;
                    if (i1 == i_qr) { MyWeirdEngineMoveFinder.positionstack[newposidx].whitequeensiderookhasmoved = true; }
                    else if (i1 == i_kr) { MyWeirdEngineMoveFinder.positionstack[newposidx].whitekingsiderookhasmoved = true; }
                }
                else
                {
                    i_qr = MyWeirdEngineMoveFinder.positionstack[posidx].blackqueensiderookcoord.x;
                    i_kr = MyWeirdEngineMoveFinder.positionstack[posidx].blackkingsiderookcoord.x;
                    if (i1 == i_qr) { MyWeirdEngineMoveFinder.positionstack[newposidx].blackqueensiderookhasmoved = true; }
                    else if (i1 == i_kr) { MyWeirdEngineMoveFinder.positionstack[newposidx].blackkingsiderookhasmoved = true; }

                }
            }
            //Set castling info for new position END

            if (pmove.IsEnPassant == true)
            {
                int io1 = pmove.othercoordinates[0];
                int jo1 = pmove.othercoordinates[1];
                MyWeirdEngineMoveFinder.positionstack[newposidx].squares[io1, jo1] = 0;
            }
            if (pmove.IsCastling == true)
            {
                int io1 = pmove.othercoordinates[0];
                int jo1 = pmove.othercoordinates[1];
                int io2 = pmove.othercoordinates[2];
                int jo2 = pmove.othercoordinates[3];
                int otherpiece = this.MyWeirdEngineMoveFinder.positionstack[newposidx].squares[io1, jo1];
                if (io1 != i2)
                {
                    MyWeirdEngineMoveFinder.positionstack[newposidx].squares[io1, jo1] = 0;
                }
                MyWeirdEngineMoveFinder.positionstack[newposidx].squares[io2, jo2] = otherpiece;
            }

            if (MyWeirdEngineMoveFinder.positionstack[posidx].colourtomove == 1)
            {
                MyWeirdEngineMoveFinder.positionstack[newposidx].colourtomove = -1;
            }
            else
            {
                MyWeirdEngineMoveFinder.positionstack[newposidx].colourtomove = 1;
            }

            ApplyImitators(posidx, newposidx, pmove, pti);


            return newposidx;
        }
    }
}
