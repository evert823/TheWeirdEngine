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
        public void Default_moveprioindex(ref chessposition pposition)
        {
            for (int i = 0; i < pposition.movelist_totalfound; i++)
            {
                pposition.moveprioindex[i] = i;
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
                        this.GetStepLeapAttacksMoves(ref pposition, i, j, depth);
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
        public void GetStepLeapAttacksMovesPerVector(ref chessposition pposition, int i, int j, vector v,
                                                     bool getcaptures, bool getnoncaptures, int depth, int pti,
                                                     int pti_self, FreezeType ft)
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
        public void GetStepLeapAttacksMoves(ref chessposition pposition, int i, int j, int depth)
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
                foreach (vector v in MyWeirdEngineMoveFinder.piecetypes[pti].stepleapmovevectors)
                {
                    GetStepLeapAttacksMovesPerVector(ref pposition, i, j, v, true, true, depth, pti, pti_self, ft);
                }
            }
            else
            {
                foreach (vector v in MyWeirdEngineMoveFinder.piecetypes[pti].stepleapmovevectors)
                {
                    GetStepLeapAttacksMovesPerVector(ref pposition, i, j, v, false, true, depth, pti, pti_self, ft);
                }
                foreach (vector v in MyWeirdEngineMoveFinder.piecetypes[pti].stepleapcapturevectors)
                {
                    GetStepLeapAttacksMovesPerVector(ref pposition, i, j, v, true, false, depth, pti, pti_self, ft);
                }
            }
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
        public bool SquareStepLeapAttackedFromSquare(ref chessposition pposition, int i, int j, int i3, int j3,
                                                     vector v)
        {
            //Establish if [i3,j3] is attacked from [i,j] using StepLeap vector v yes or no
            //We would need this if we would have combined Time Thief power with step-leap vectors

            int pti = MyWeirdEngineMoveFinder.pieceTypeIndex(pposition.squares[i, j]);
            FreezeType ft = GetFreezeType(pposition, i, j, pti);
            if (ft.capturefreeze == true) { return false; }

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
        public void GetSlideAttacksMovesPerVector(ref chessposition pposition, int i, int j, vector v,
                                                  bool getcaptures, bool getnoncaptures, int depth, int pti,
                                                  int pti_self, FreezeType ft)
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
            tomove.IsCastling = frommove.IsCastling;
            tomove.othercoordinates[0] = frommove.othercoordinates[0];
            tomove.othercoordinates[1] = frommove.othercoordinates[1];
            tomove.othercoordinates[2] = frommove.othercoordinates[2];
            tomove.othercoordinates[3] = frommove.othercoordinates[3];
            tomove.PromoteToPiece = frommove.PromoteToPiece;
            tomove.calculatedvalue = frommove.calculatedvalue;
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
                pposition.movelist[lmi].MovingPiece = 0;
                pposition.movelist[lmi].coordinates[0] = 0;
                pposition.movelist[lmi].coordinates[1] = 0;
                pposition.movelist[lmi].coordinates[2] = 0;
                pposition.movelist[lmi].coordinates[3] = 0;
                pposition.movelist[lmi].IsEnPassant = false;
                pposition.movelist[lmi].IsCapture = false;
                pposition.movelist[lmi].IsCastling = false;
                pposition.movelist[lmi].othercoordinates[0] = -1;
                pposition.movelist[lmi].othercoordinates[1] = -1;
                pposition.movelist[lmi].othercoordinates[2] = -1;
                pposition.movelist[lmi].othercoordinates[3] = -1;
                pposition.movelist[lmi].PromoteToPiece = 0;
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
    }
}
