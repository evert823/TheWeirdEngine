using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.ComponentModel.Com2Interop;
using System.Xml.Linq;

namespace TheWeirdEngine
{

    public struct PosEvaluationResult
    {
        public bool MeInCheck;
        public bool IsStaleMate;
        public bool IsMate;
        public bool IsDrawByMaterial;
        public decimal PositionAdvantage;
        public int BestMoveidx;

        //PositionAdvantageLegenda:
        //120 White to move can already take Black's King = illegal
        //100 Mate, White wins (also indicated by the separate boolean)
        //99 Mate in 1 ply
        //98 Mate in 2 plies
        //97 Mate in 3 plies (Mate in 2 moves)
        //96 Mate in 4 plies
        //95 Mate in 5 plies (Mate in 3 moves)
        //94 Mate in 6 plies
        //93 Mate in 7 plies (Mate in 4 moves)
        //92 Mate in 8 plies
        //91 Mate in 9 plies (Mate in 5 moves)
        //90 - 1 Advantage for White
        //0 Draw
        //Corresponding negative numbers for Black's perspective

    }

    public struct Move
    {
        public byte from_i;
        public byte from_j;
        public byte to_i;
        public byte to_j;
        public sbyte PromoteToPiece;

        //MyResult will contain the result of evaluation of the position that would result from this move
        //MyResult.BestMoveidx is not relevant to fill in here, as it would be the best response to this move
        public PosEvaluationResult MyResult;
    }

    public struct Square
    {
        public sbyte PieceTypeColour;
        //Numbers are hard-coded reserved for pieces
        //0 Vacant
        //1 King K
        //2 Queen Q
        //3 Rook R
        //4 Knight N
        //5 Bishop B
        //6 Guard G
        //7 Witch W
        //8 Pawn p
        //Positive number for White piece
        //Corresponding negative number for Black piece

        public bool EnPassantLeftAllowed;
        public bool EnPassantRightAllowed;
        //Left-Right is understood from perspective of White player
        //Indicates that the pawn, located on this square, is allowed to take en passant from the current position

        //Enriched information
        public bool ExtraWhiteWitchInfluence;
        public bool ExtraBlackWitchInfluence;
        //A Witch cannot make an adjacent piece transparent for herself. But an extra friendly Witch could do this.
        //So these extra flags are to detect this situation.

        public bool WhiteWitchInfluence;
        public bool BlackWitchInfluence;
        public bool IsCloseToWhiteKing;
        public bool IsCloseToBlackKing;
        public bool IsAttackedByWhite;
        public bool IsAttackedByBlack;
        public bool AdvancePawnPotentialCounted;
        //This can be derived from the primary position information, which is done during enrichment of the position
    }
    public struct Position
    {
        public Square [,] MySquare;
        //With an 8x8 FIDE chess board:
        //(0,0) = a1
        //(7,0) = h1
        //(0,7) = a8
        //(7,7) = h8

        public sbyte ColourToMove;//1 White to move -1 Black to move

        public bool CastleWhiteRightBlockedPerm;
        public bool CastleWhiteLeftBlockedPerm;
        public bool CastleBlackRightBlockedPerm;
        public bool CastleBlackLeftBlockedPerm;
        //Perm --> Castle permanently not allowed e.g. after Rook or King has moved
        //Also here, left/right is understood from Wite's perspective

        //Enriched information
        public byte WhiteKingLoci;
        public byte WhiteKingLocj;
        public byte BlackKingLoci;
        public byte BlackKingLocj;

        public bool WhiteHasWitch;
        public bool BlackHasWitch;

        public bool CastleWhiteRightBlockedTemp;
        public bool CastleWhiteLeftBlockedTemp;
        public bool CastleBlackRightBlockedTemp;
        public bool CastleBlackLeftBlockedTemp;
        //Also here, left/right is understood from Wite's perspective

        public bool WhiteIsInCheck;
        public bool BlackIsInCheck;

        public int AdvancePawnPotential;
        public int BlackCloseToWhiteKingScore;
        public int WhiteCloseToBlackKingScore;
        public int Distance_OpponentsKing_balance;

        public Move[] MovesFromHere;
        public int NumberOfFoundMoves;

        //Temp --> Castle temporarily not allowed because of the current position
        //This can be derived from the primary position information, which is done during enrichment of the position

    }

    public struct Game
    {
        public int NumberOfFiles;//Determins the width of the board
        public int NumberOfRanks;
        //e.g. for the standard 10x8 Bulldog chess board NumberOfFiles = 10, NumberOfRanks = 8

        public int CastleDistance;
        //The number of squares that a King is displaced with castling

        public int NumberOfPositionsInGame;
        public int ActualCurrentPositionidx;
        public Position[] MyPosition;
        //A game contains all past positions from the start till the current position of the game
        //Even when you want to consider one position, it will be stored as a Game containing only your position
        //Position[0] is the starting position
        //The calculation procedures will do and undo moves and by doing so alter NumberOfPositionsInGame
        //MyPosition[NumberOfPositionsInGame - 1] is supposed to be the current position, also within a hypothetical
        //calculation line
        //ActualCurrentPositionidx is supposed to indicate the current position without additional calculation lines
        //MyPosition[ActualCurrentPositionidx] is supposed to be the current position, NOT taking into account
        //hypothetical calculation lines

        public string CalculationLineMessage;
        public string StatusMessage;
        public bool ExternalAbort;
        public Move EnteredMove;
        public int EnteredMoveIdentifiedidx;

    }
    public class WeirdEngineBackend
    {
        public const int MaxNumberOfPositions = 1000;

        //FindOnly1stMate_n_line tells the Engine to stop evaluating more moves from the current position and calculation context
        //once one mating move has been found
        public bool FindOnly1stMate_n_line;

        public byte NumberOfPliesToCalculate;

        public bool BoardFromWhitePerspective;

        public Game MyGame;

        public WeirdEngineBackend(int pNumberOfFiles, int pNumberOfRanks)
        {
            this.MyGame = new Game();
            this.FindOnly1stMate_n_line = true;//Can be overruled from settings file
            this.NumberOfPliesToCalculate = 3;//Can be overruled from settings file
            this.BoardFromWhitePerspective = true;
            this.ResetGame(pNumberOfFiles, pNumberOfRanks, 1);
        }

        private void ResetGame(int pNumberOfFiles, int pNumberOfRanks, int pNumberOfPositionsInGame)
        {
            int i;
            this.MyGame.MyPosition = null;
            this.MyGame.NumberOfFiles = pNumberOfFiles;
            this.MyGame.NumberOfRanks = pNumberOfRanks;
            this.MyGame.NumberOfPositionsInGame = pNumberOfPositionsInGame;
            this.MyGame.ActualCurrentPositionidx = pNumberOfPositionsInGame - 1;
            this.MyGame.MyPosition = new Position[MaxNumberOfPositions];
            this.MyGame.EnteredMoveIdentifiedidx = -1;
            this.MyGame.ExternalAbort = false;
 
            for (i = 0; i < MaxNumberOfPositions; i++)
            { 
                this.MyGame.MyPosition[i].MySquare = new Square[this.MyGame.NumberOfFiles, this.MyGame.NumberOfRanks];
                this.MyGame.MyPosition[i].MovesFromHere = new Move[10000];
                this.MyGame.MyPosition[i].CastleWhiteLeftBlockedPerm = false;
                this.MyGame.MyPosition[i].CastleWhiteRightBlockedPerm = false;
                this.MyGame.MyPosition[i].CastleBlackLeftBlockedPerm = false;
                this.MyGame.MyPosition[i].CastleBlackRightBlockedPerm = false;
            }
        }

        private void EnrichCloseToOtherKingScore(int pPositionNumber)
        {
            int i;
            int j;
            int distance;
            int whitescore;
            int blackscore;

            whitescore = 0;
            blackscore = 0;

            for (i = 0; i < this.MyGame.NumberOfFiles; i++)
            {
                for (j = 0; j < MyGame.NumberOfRanks; j++)
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 1 |
                        this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 4 |
                        this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 6)
                    {
                        distance = Math.Abs(this.MyGame.MyPosition[pPositionNumber].BlackKingLoci - i) +
                                   Math.Abs(this.MyGame.MyPosition[pPositionNumber].BlackKingLocj - j);
                        whitescore = whitescore + (576 - (distance * distance));
                    }
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == -1 |
                        this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == -4 |
                        this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == -6)
                    {
                        distance = Math.Abs(this.MyGame.MyPosition[pPositionNumber].WhiteKingLoci - i) +
                                   Math.Abs(this.MyGame.MyPosition[pPositionNumber].WhiteKingLocj - j);
                        blackscore = blackscore + (576 - (distance * distance));
                    }
                }
            }
            this.MyGame.MyPosition[pPositionNumber].WhiteCloseToBlackKingScore = whitescore;
            this.MyGame.MyPosition[pPositionNumber].BlackCloseToWhiteKingScore = blackscore;
            this.MyGame.MyPosition[pPositionNumber].Distance_OpponentsKing_balance = whitescore - blackscore;
        }

        private PosEvaluationResult LoneKingChaser(int pPositionNumber, PosEvaluationResult pResultSoFar, sbyte pChasingColour)
        {
            PosEvaluationResult output;
            int i;
            int j;
            int preferredcorner_i;
            int preferredcorner_j;
            int distance_to_corner;
            int closetokingscore;
            sbyte RelativePieceTypeColour;
            bool WeHaveBishopOnWhite;
            bool WeHaveBishopOnBlack;
            bool BishopsMatter;
            bool RelevantPiece;
            int kingloci;
            int kinglocj;

            output = pResultSoFar;

            WeHaveBishopOnWhite = false;
            WeHaveBishopOnBlack = false;
            BishopsMatter = true;
            RelevantPiece = false;

            for (i = 0; i < this.MyGame.NumberOfFiles; i++)
            {
                for (j = 0; j < MyGame.NumberOfRanks; j++)
                {
                    RelativePieceTypeColour = (sbyte)(pChasingColour *
                        this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour);
                    if (RelativePieceTypeColour < -1 & RelativePieceTypeColour != -7)
                    {
                        return pResultSoFar;
                    }
                    if (RelativePieceTypeColour > 0 & RelativePieceTypeColour != 1 & RelativePieceTypeColour != 8)
                    {
                        RelevantPiece = true;
                    }
                    if (RelativePieceTypeColour == 5)
                    {
                        if ((i + j) % 2 == 0)
                        {
                            WeHaveBishopOnWhite = true;
                        }
                        else
                        {
                            WeHaveBishopOnBlack = true;
                        }
                    }
                    if (RelativePieceTypeColour == 2 | RelativePieceTypeColour == 3)
                    {
                        BishopsMatter = false;
                    }
                }
            }

            if (RelevantPiece == false)
            {
                return pResultSoFar;
            }

            if (WeHaveBishopOnWhite == WeHaveBishopOnBlack)
            {
                BishopsMatter = false;
            }

            if (pChasingColour == 1)
            {
                kingloci = this.MyGame.MyPosition[pPositionNumber].BlackKingLoci;
                kinglocj = this.MyGame.MyPosition[pPositionNumber].BlackKingLocj;
            }
            else
            {
                kingloci = this.MyGame.MyPosition[pPositionNumber].WhiteKingLoci;
                kinglocj = this.MyGame.MyPosition[pPositionNumber].WhiteKingLocj;
            }

            if (kingloci >= this.MyGame.NumberOfFiles / 2)
            {
                preferredcorner_i = this.MyGame.NumberOfFiles - 1;
            }
            else
            {
                preferredcorner_i = 0;
            }
            if (kinglocj >= this.MyGame.NumberOfRanks / 2)
            {
                preferredcorner_j = this.MyGame.NumberOfRanks - 1;
            }
            else
            {
                preferredcorner_j = 0;
            }

            if (BishopsMatter == true)
            {
                if (WeHaveBishopOnWhite == true & (preferredcorner_i + preferredcorner_j) % 2 == 1)
                {
                    preferredcorner_i = 0;
                    preferredcorner_j = 0;
                }
                if (WeHaveBishopOnBlack == true & (preferredcorner_i + preferredcorner_j) % 2 == 0)
                {
                    preferredcorner_i = 0;
                    preferredcorner_j = this.MyGame.NumberOfFiles - 1;
                }
                //For the situation that a board has only squares of the wrong colour, e.g. a 9x9 board
                //extra coding is required to respond with draw
            }

            //Now what we want is return a result scoped between 80 and 89 reflecting the distance between
            //target King and preferred corner

            distance_to_corner = Math.Abs(kingloci - preferredcorner_i) + Math.Abs(kinglocj - preferredcorner_j);

            if (pChasingColour == 1)
            {
                output.PositionAdvantage = (85 - ((decimal)distance_to_corner / 5));
                closetokingscore = this.MyGame.MyPosition[pPositionNumber].WhiteCloseToBlackKingScore;
                if (closetokingscore > 4000)
                {
                    closetokingscore = 4000;
                }
                output.PositionAdvantage = output.PositionAdvantage + ((decimal)closetokingscore / 1000);
            }
            else
            {
                output.PositionAdvantage = (((decimal)distance_to_corner / 5) - 85);
                closetokingscore = this.MyGame.MyPosition[pPositionNumber].BlackCloseToWhiteKingScore;
                if (closetokingscore > 4000)
                {
                    closetokingscore = 4000;
                }
                output.PositionAdvantage = output.PositionAdvantage - ((decimal)closetokingscore / 1000);
            }

            return output;
        }


        private bool IsDrawByInsufficientMaterial(int pPositionNumber)
        {
            int i;
            int j;
            int WhiteBishopCountOnWhite;
            int WhiteBishopCountOnBlack;
            int WhiteKnightCount;
            int BlackBishopCountOnWhite;
            int BlackBishopCountOnBlack;
            int BlackKnightCount;

            WhiteBishopCountOnWhite = 0;
            WhiteBishopCountOnBlack = 0;
            WhiteKnightCount = 0;
            BlackBishopCountOnWhite = 0;
            BlackBishopCountOnBlack = 0;
            BlackKnightCount = 0;
            for (i = 0; i < this.MyGame.NumberOfFiles; i++)
            {
                for (j = 0; j < MyGame.NumberOfRanks; j++)
                {
                    //4 Knight N
                    //5 Bishop B
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 2 |
                        this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == -2 |
                        this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 3 |
                        this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == -3 |
                        this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 6 |
                        this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == -6 |
                        this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 8 |
                        this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == -8)
                    {
                        return false;
                    }
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 4)
                    {
                        WhiteKnightCount++;
                    }
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 5)
                    {
                        if ((i + j) % 2 == 0)
                        {
                            WhiteBishopCountOnWhite++;
                        } else
                        {
                            WhiteBishopCountOnBlack++;
                        }
                    }
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == -4)
                    {
                        BlackKnightCount++;
                    }
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == -5)
                    {
                        if ((i + j) % 2 == 0)
                        {
                            BlackBishopCountOnWhite++;
                        }
                        else
                        {
                            BlackBishopCountOnBlack++;
                        }
                    }
                }
            }

            if (WhiteBishopCountOnWhite > 0 & WhiteKnightCount > 0)
            {
                return false;
            }
            if (WhiteBishopCountOnBlack > 0 & WhiteKnightCount > 0)
            {
                return false;
            }
            if (WhiteBishopCountOnWhite > 0 & WhiteBishopCountOnBlack > 0)
            {
                return false;
            }
            if (BlackBishopCountOnWhite > 0 & BlackKnightCount > 0)
            {
                return false;
            }
            if (BlackBishopCountOnBlack > 0 & BlackKnightCount > 0)
            {
                return false;
            }
            if (BlackBishopCountOnWhite > 0 & BlackBishopCountOnBlack > 0)
            {
                return false;
            }

            if (WhiteKnightCount > 2 | BlackKnightCount > 2)
            {
                return false;
            }

            return true;
        }


        private void PrioritizeMovesFromHere(int pPositionNumber)
        {
            int mn1;
            int mn2;
            Move swapmove;


            for (mn1 = 0; mn1 < this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1; mn1++)
            {
                for (mn2 = mn1 + 1; mn2 < this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves; mn2++)
                {
                    if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == 1)
                    {
                        if (this.MyGame.MyPosition[pPositionNumber].MovesFromHere[mn1].MyResult.PositionAdvantage <
                            this.MyGame.MyPosition[pPositionNumber].MovesFromHere[mn2].MyResult.PositionAdvantage)
                        {
                            swapmove = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[mn1];
                            this.MyGame.MyPosition[pPositionNumber].MovesFromHere[mn1] =
                                        this.MyGame.MyPosition[pPositionNumber].MovesFromHere[mn2];
                            this.MyGame.MyPosition[pPositionNumber].MovesFromHere[mn2] = swapmove;
                        }
                    }
                    else
                    {
                        if (this.MyGame.MyPosition[pPositionNumber].MovesFromHere[mn1].MyResult.PositionAdvantage >
                            this.MyGame.MyPosition[pPositionNumber].MovesFromHere[mn2].MyResult.PositionAdvantage)
                        {
                            swapmove = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[mn1];
                            this.MyGame.MyPosition[pPositionNumber].MovesFromHere[mn1] =
                                        this.MyGame.MyPosition[pPositionNumber].MovesFromHere[mn2];
                            this.MyGame.MyPosition[pPositionNumber].MovesFromHere[mn2] = swapmove;
                        }
                    }

                }
            }
        }

        //EvaluationByCalculation and EvaluationByPosition have a lot of code in common
        //But are also each doing really different things
        private PosEvaluationResult EvaluationByCalculation(int pPositionNumber, byte pNumberOfPlies, decimal pGuaranteedScorePreviousCaller,
                                                             bool pApplyAlphaBetaLogic)
        {
            PosEvaluationResult output;
            PosEvaluationResult dummyoutput;
            int mn;
            bool LegalMoveFound;
            bool MatingMoveFound;
            bool AlphaBetaStop;
            decimal BestFoundAdvantage;
            decimal RelativeBestFoundAdvantage;
            string prevCalculationLineMessage;
            byte NumberOfPliesFromHere;

            output.MeInCheck = false;
            output.IsStaleMate = false;
            output.IsMate = false;
            output.IsDrawByMaterial = false;
            output.PositionAdvantage = 0;
            output.BestMoveidx = 0;


            this.MyGame.MyPosition[pPositionNumber].WhiteIsInCheck = false;
            this.MyGame.MyPosition[pPositionNumber].BlackIsInCheck = false;

            //When already at deepest calculation level, evaluate the position only
            if (pNumberOfPlies == 0)
            {
                output = this.EvaluationByPosition(pPositionNumber);
                return output;
            }

            NumberOfPliesFromHere = pNumberOfPlies;

            //Enrich this position
            if (pNumberOfPlies >= 5)
            {
                dummyoutput = this.EvaluationByCalculation(pPositionNumber, 3, this.MyGame.MyPosition[pPositionNumber].ColourToMove * 120, false);
                this.PrioritizeMovesFromHere(pPositionNumber);
                this.Init_Move_Evaluation_Results(pPositionNumber);
            }
            else
            {
                this.ListMovesAndEnrich(pPositionNumber, 0);
            }

            //Detect an illegal position where own King is in check
            if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == 1 &
                this.MyGame.MyPosition[pPositionNumber].BlackIsInCheck)
            {
                output.MeInCheck = true;
                output.PositionAdvantage = 120;
                return output;
            }
            if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == -1 &
                this.MyGame.MyPosition[pPositionNumber].WhiteIsInCheck)
            {
                output.MeInCheck = true;
                output.PositionAdvantage = -120;
                return output;
            }

            BestFoundAdvantage = (decimal)(this.MyGame.MyPosition[pPositionNumber].ColourToMove * -120);

            LegalMoveFound = false;

            MatingMoveFound = false;

            AlphaBetaStop = false;
            mn = 0;
            while (mn < this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves & MatingMoveFound == false
                            & AlphaBetaStop == false & this.MyGame.ExternalAbort == false)
            {
                prevCalculationLineMessage = this.MyGame.CalculationLineMessage;

                this.MyGame.CalculationLineMessage = this.MyGame.CalculationLineMessage + "|" +
                         this.MoveAsString(this.MyGame.MyPosition[pPositionNumber], mn);

                Application.DoEvents();

                //Build the positions resulting from each of the moves
                this.DoMove(pPositionNumber, mn);

                //Recursive call to the same procedure
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[mn].MyResult =
                          this.EvaluationByCalculation(pPositionNumber + 1, (byte)(NumberOfPliesFromHere - 1), BestFoundAdvantage, true);
                //This also enriches this position resulting from this move

                this.MyGame.CalculationLineMessage = prevCalculationLineMessage;

                //Detect mate or stalemate
                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == 1 &
                    this.MyGame.MyPosition[pPositionNumber + 1].WhiteIsInCheck == false)
                {
                    LegalMoveFound = true;
                }
                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == -1 &
                    this.MyGame.MyPosition[pPositionNumber + 1].BlackIsInCheck == false)
                {
                    LegalMoveFound = true;
                }
                this.UndoLastMove(pPositionNumber + 1);


                //Keep track of best so far
                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == 1)
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MovesFromHere[mn].MyResult.PositionAdvantage > BestFoundAdvantage)
                    {
                        output.BestMoveidx = mn;
                        BestFoundAdvantage = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[mn].MyResult.PositionAdvantage;
                    }
                    if (BestFoundAdvantage > pGuaranteedScorePreviousCaller & pApplyAlphaBetaLogic == true & LegalMoveFound == true)
                    {
                        AlphaBetaStop = true;
                    }
                } else
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MovesFromHere[mn].MyResult.PositionAdvantage < BestFoundAdvantage)
                    {
                        output.BestMoveidx = mn;
                        BestFoundAdvantage = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[mn].MyResult.PositionAdvantage;
                    }
                    if (BestFoundAdvantage < pGuaranteedScorePreviousCaller & pApplyAlphaBetaLogic == true & LegalMoveFound == true)
                    {
                        AlphaBetaStop = true;
                    }
                }

                //Now reduce the search depth when a forced mate has already been found
                RelativeBestFoundAdvantage = (decimal)(BestFoundAdvantage * this.MyGame.MyPosition[pPositionNumber].ColourToMove);
                if (FindOnly1stMate_n_line == true)
                {
                    switch (RelativeBestFoundAdvantage)
                    {
                        case 100:
                            MatingMoveFound = true;
                            break;
                        case 98:
                            NumberOfPliesFromHere = 1;
                            break;
                        case 96:
                            NumberOfPliesFromHere = 3;
                            break;
                        case 94:
                            NumberOfPliesFromHere = 5;
                            break;
                        case 92:
                            NumberOfPliesFromHere = 7;
                            break;
                    }
                }
                mn++;
            }

            //Detect mate or stalemate
            if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == 1 & LegalMoveFound == false)
            {
                if (this.MyGame.MyPosition[pPositionNumber].WhiteIsInCheck == true)
                {
                    //White is mated
                    output.IsMate = true;
                    output.PositionAdvantage = -100;
                    return output;
                }
                else
                {
                    //White is stalemated
                    output.IsStaleMate = true;
                    output.PositionAdvantage = 0;
                    return output;
                }
            }
            if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == -1 & LegalMoveFound == false)
            {
                if (this.MyGame.MyPosition[pPositionNumber].BlackIsInCheck == true)
                {
                    //Black is mated
                    output.IsMate = true;
                    output.PositionAdvantage = 100;
                    return output;
                }
                else
                {
                    //Black is stalemated
                    output.IsStaleMate = true;
                    output.PositionAdvantage = 0;
                    return output;
                }
            }

            //Extra check for insufficient material on each level of calculation
            if (this.IsDrawByInsufficientMaterial(pPositionNumber))
            {
                output.IsDrawByMaterial = true;
                output.PositionAdvantage = 0;
                return output;
            }

            mn = output.BestMoveidx;

            output.PositionAdvantage = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[mn].MyResult.PositionAdvantage;
            if (output.PositionAdvantage > 90)
            {
                output.PositionAdvantage = output.PositionAdvantage - 1;
            }
            if (output.PositionAdvantage < -90)
            {
                output.PositionAdvantage = output.PositionAdvantage + 1;
            }

            return output;
        }

        private PosEvaluationResult EvaluationByMaterial(int pPositionNumber)
        {
            PosEvaluationResult output;
            int i;
            int j;
            int NetMaterialValue;
            decimal NetActivityScore;
            decimal CloseToKingBalance;
            int r;

            NetMaterialValue = 0;
            NetActivityScore = 0;
            for (i = 0; i < this.MyGame.NumberOfFiles; i++)
            {
                for (j = 0; j < this.MyGame.NumberOfRanks; j++)
                {


                    //Count activity score BEGIN
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].IsAttackedByWhite == true)
                    {

                        if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].IsCloseToBlackKing)
                        {
                            NetActivityScore = NetActivityScore + 20;
                        } else
                        {
                            NetActivityScore = NetActivityScore + j;
                        }
                    }

                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].IsAttackedByBlack == true)
                    {
                        if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].IsCloseToWhiteKing)
                        {
                            NetActivityScore = NetActivityScore - 20;
                        }
                        else
                        {
                            r = (this.MyGame.NumberOfRanks - 1) - j;
                            NetActivityScore = NetActivityScore - r;
                        }
                    }
                    //Count activity score END

                    switch (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour)
                    {
                        case 2: NetMaterialValue = NetMaterialValue + 9; break;
                        case -2: NetMaterialValue = NetMaterialValue - 9; break;
                        case 3: NetMaterialValue = NetMaterialValue + 5; break;
                        case -3: NetMaterialValue = NetMaterialValue - 5; break;
                        case 4: NetMaterialValue = NetMaterialValue + 3; break;
                        case -4: NetMaterialValue = NetMaterialValue - 3; break;
                        //                        case 5: NetMaterialValue = NetMaterialValue + 3; break;
                        //                        case -5: NetMaterialValue = NetMaterialValue - 3; break;

                        case 5:
                            if (this.MyGame.MyPosition[pPositionNumber].WhiteHasWitch == true)
                            {
                                NetMaterialValue = NetMaterialValue + 4;
                            }
                            else
                            {
                                NetMaterialValue = NetMaterialValue + 3;
                            }
                            break;
                        case -5:
                            if (this.MyGame.MyPosition[pPositionNumber].BlackHasWitch == true)
                            {
                                NetMaterialValue = NetMaterialValue - 4;
                            }
                            else
                            {
                                NetMaterialValue = NetMaterialValue - 3;
                            }
                            break;

                        case 6: NetMaterialValue = NetMaterialValue + 4; break;
                        case -6: NetMaterialValue = NetMaterialValue - 4; break;
                        case 7: NetMaterialValue = NetMaterialValue + 3; break;
                        case -7: NetMaterialValue = NetMaterialValue - 3; break;
                        case 8: NetMaterialValue = NetMaterialValue + 1; break;
                        case -8: NetMaterialValue = NetMaterialValue - 1; break;
                    }
                }
            }

            CloseToKingBalance = this.MyGame.MyPosition[pPositionNumber].Distance_OpponentsKing_balance;
            if (CloseToKingBalance > 2990)
            {
                CloseToKingBalance = 2990;
            }
            if (CloseToKingBalance < -2990)
            {
                CloseToKingBalance = -2990;
            }
            CloseToKingBalance = CloseToKingBalance / 600;

            if (NetActivityScore > 399)
            {
                NetActivityScore = 399;
            }
            if (NetActivityScore < -399)
            {
                NetActivityScore = -399;
            }

            NetActivityScore = NetActivityScore / 80;

            output.MeInCheck = false;
            output.IsStaleMate = false;
            output.IsMate = false;
            output.IsDrawByMaterial = false;
            output.PositionAdvantage = 0;
            output.BestMoveidx = 0;

            NetMaterialValue = NetMaterialValue + this.MyGame.MyPosition[pPositionNumber].AdvancePawnPotential;

            //MessageBox.Show("NetMaterialValue : " + NetMaterialValue.ToString());
            //MessageBox.Show("NetActivityScore : " + NetActivityScore.ToString());
            //MessageBox.Show("CloseToKingBalance : " + CloseToKingBalance.ToString());

            if (NetMaterialValue > 9)
            {
                output.PositionAdvantage = (decimal)(70 + NetActivityScore + CloseToKingBalance);
                return output;
            }
            if (NetMaterialValue < -9)
            {
                output.PositionAdvantage = (decimal)(NetActivityScore + CloseToKingBalance - 70);
                return output;
            }

            output.PositionAdvantage = (decimal)((NetMaterialValue * 7) + NetActivityScore + CloseToKingBalance);

            if (output.PositionAdvantage == 0)
            {
                output.PositionAdvantage = this.MyGame.MyPosition[pPositionNumber].ColourToMove;
            }
            return output;
        }
        private PosEvaluationResult EvaluationByPosition(int pPositionNumber)
        {
            PosEvaluationResult output;
            PosEvaluationResult material;
            int mn;
            bool LegalMoveFound;

            output.MeInCheck = false;
            output.IsStaleMate = false;
            output.IsMate = false;
            output.IsDrawByMaterial = false;
            output.PositionAdvantage = 0;
            output.BestMoveidx = 0;


            //Enrich this position
            this.ListMovesAndEnrich(pPositionNumber, 0);


            //Detect an illegal position where own King is in check
            if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == 1 &
                this.MyGame.MyPosition[pPositionNumber].BlackIsInCheck)
            {
                output.MeInCheck = true;
                output.PositionAdvantage = 120;
                return output;
            }
            if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == -1 &
                this.MyGame.MyPosition[pPositionNumber].WhiteIsInCheck)
            {
                output.MeInCheck = true;
                output.PositionAdvantage = -120;
                return output;
            }


            LegalMoveFound = false;
            mn = 0;
            while (mn < this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves & LegalMoveFound == false)
            {
                //Build the positions resulting from each of the moves
                this.DoMove(pPositionNumber, mn);

                //Enrich the position resulting from the moves
                this.ListMovesAndEnrich(pPositionNumber + 1, 1);

                //Detect mate or stalemate
                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == 1 &
                    this.MyGame.MyPosition[pPositionNumber + 1].WhiteIsInCheck == false)
                {
                    LegalMoveFound = true;
                }
                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == -1 &
                    this.MyGame.MyPosition[pPositionNumber + 1].BlackIsInCheck == false)
                {
                    LegalMoveFound = true;
                }
                this.UndoLastMove(pPositionNumber + 1);
                mn++;
            }

            //Detect mate or stalemate
            if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == 1 & LegalMoveFound == false)
            {
                if (this.MyGame.MyPosition[pPositionNumber].WhiteIsInCheck == true)
                {
                    //White is mated
                    output.IsMate = true;
                    output.PositionAdvantage = -100;
                    return output;
                } else
                {
                    //White is stalemated
                    output.IsStaleMate = true;
                    output.PositionAdvantage = 0;
                    return output;
                }
            }
            if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == -1 & LegalMoveFound == false)
            {
                if (this.MyGame.MyPosition[pPositionNumber].BlackIsInCheck == true)
                {
                    //Black is mated
                    output.IsMate = true;
                    output.PositionAdvantage = 100;
                    return output;
                }
                else
                {
                    //Black is stalemated
                    output.IsStaleMate = true;
                    output.PositionAdvantage = 0;
                    return output;
                }
            }

            if (this.IsDrawByInsufficientMaterial(pPositionNumber))
            {
                output.IsDrawByMaterial = true;
                output.PositionAdvantage = 0;
                return output;
            }

            //From here, material advantage and positional advantage should be calculated, and it is Work in Progress
            //This should lead to a resulting number between 90 and 1 (depending for the advantage for White)
            // or between -1 and -90 (depending on the advantage for Black)
            material = this.EvaluationByMaterial(pPositionNumber);

            output.PositionAdvantage = material.PositionAdvantage;

            output = this.LoneKingChaser(pPositionNumber, output, 1);
            output = this.LoneKingChaser(pPositionNumber, output, -1);

            return output;
        }


        private void PutOneRandomPiece(sbyte pPieceTypeColour, int pmin_i, int pmax_i, int pmin_j, int pmax_j)
        {
            int i;
            int j;
            Random MyRandom;

            int min_i;
            int max_i;
            int min_j;
            int max_j;

            if (pPieceTypeColour == 8 | pPieceTypeColour == -8)
            {
                min_j = 1;
                max_j = this.MyGame.NumberOfRanks - 2;
            }
            else
            {
                min_j = 0;
                max_j = this.MyGame.NumberOfRanks - 1;
            }

            min_i = 0;
            max_i = this.MyGame.NumberOfFiles - 1;

            //Here min_i, max_i, min_j, max_j have been put to the normal limits
            //But from outside additional limits have been put

            if (min_i < pmin_i)
            {
                min_i = pmin_i;
            }
            if (min_j < pmin_j)
            {
                min_j = pmin_j;
            }
            if (max_i > pmax_i)
            {
                max_i = pmax_i;
            }
            if (max_j > pmax_j)
            {
                max_j = pmax_j;
            }

            MyRandom = new Random();

            i = MyRandom.Next(min_i, max_i + 1);
            j = MyRandom.Next(min_j, max_j + 1);
            while (this.MyGame.MyPosition[0].MySquare[i, j].PieceTypeColour != 0)
            {
                i = MyRandom.Next(min_i, max_i + 1);
                j = MyRandom.Next(min_j, max_j + 1);
            }
            this.MyGame.MyPosition[0].MySquare[i, j].PieceTypeColour = pPieceTypeColour;
            this.MyGame.MyPosition[0].MySquare[i, j].EnPassantLeftAllowed = false;
            this.MyGame.MyPosition[0].MySquare[i, j].EnPassantRightAllowed = false;
        }
        private void GenerateRandomPosition()
        {
            Random MyRandom;

            this.ResetGame(8, 8, 1);
            this.MyGame.CastleDistance = 2;

            MyRandom = new Random();
            
            this.MyGame.MyPosition[0].ColourToMove = (sbyte)(MyRandom.Next(0, 2));

            if (this.MyGame.MyPosition[0].ColourToMove == 0)
            {
                this.MyGame.MyPosition[0].ColourToMove = -1;
            }
            this.MyGame.MyPosition[0].ColourToMove = 1;

            this.MyGame.MyPosition[0].CastleWhiteRightBlockedPerm = true;
            this.MyGame.MyPosition[0].CastleWhiteLeftBlockedPerm = true;
            this.MyGame.MyPosition[0].CastleBlackRightBlockedPerm = true;
            this.MyGame.MyPosition[0].CastleBlackLeftBlockedPerm = true;

            this.MyGame.MyPosition[0].MySquare[0, 4].PieceTypeColour = 5;
            this.MyGame.MyPosition[0].MySquare[0, 6].PieceTypeColour = -1;
            this.MyGame.MyPosition[0].MySquare[2, 6].PieceTypeColour = 1;
            //this.MyGame.MyPosition[0].MySquare[6, 5].PieceTypeColour = 1;

            //this.PutOneRandomPiece(-1, 0, 1, 6, 7);
            //this.PutOneRandomPiece(1, 0, 3, 4, 7);
            this.PutOneRandomPiece(4, 0, 3, 4, 7);
            //this.PutOneRandomPiece(5, 0, 3, 4, 7);
            this.PutOneRandomPiece(7, 0, 3, 4, 7);
        }

        public void SetInitialStandardBulldog()
        {
            int i;
            int j;

            this.ResetGame(10, 8, 1);
            this.MyGame.CastleDistance = 3;

            this.MyGame.MyPosition[0].ColourToMove = 1;

            this.MyGame.MyPosition[0].CastleWhiteRightBlockedPerm = false;
            this.MyGame.MyPosition[0].CastleWhiteLeftBlockedPerm = false;
            this.MyGame.MyPosition[0].CastleBlackRightBlockedPerm = false;
            this.MyGame.MyPosition[0].CastleBlackLeftBlockedPerm = false;

            
            for (i = 0; i < this.MyGame.NumberOfFiles; i++)
            {
                this.MyGame.MyPosition[0].MySquare[i, 1].PieceTypeColour = 8;
                this.MyGame.MyPosition[0].MySquare[i, this.MyGame.NumberOfRanks - 2].PieceTypeColour = -8;

                for (j = 0; j < this.MyGame.NumberOfRanks; j++)
                {
                    this.MyGame.MyPosition[0].MySquare[i, j].EnPassantLeftAllowed = false;
                    this.MyGame.MyPosition[0].MySquare[i, j].EnPassantRightAllowed = false;
                }
            }

            this.MyGame.MyPosition[0].MySquare[0, 0].PieceTypeColour = 3;
            this.MyGame.MyPosition[0].MySquare[1, 0].PieceTypeColour = 7;
            this.MyGame.MyPosition[0].MySquare[2, 0].PieceTypeColour = 4;
            this.MyGame.MyPosition[0].MySquare[3, 0].PieceTypeColour = 5;
            this.MyGame.MyPosition[0].MySquare[4, 0].PieceTypeColour = 2;
            this.MyGame.MyPosition[0].MySquare[5, 0].PieceTypeColour = 1;
            this.MyGame.MyPosition[0].MySquare[6, 0].PieceTypeColour = 5;
            this.MyGame.MyPosition[0].MySquare[7, 0].PieceTypeColour = 4;
            this.MyGame.MyPosition[0].MySquare[8, 0].PieceTypeColour = 6;
            this.MyGame.MyPosition[0].MySquare[9, 0].PieceTypeColour = 3;

            this.MyGame.MyPosition[0].MySquare[0, this.MyGame.NumberOfRanks - 1].PieceTypeColour = -3;
            this.MyGame.MyPosition[0].MySquare[1, this.MyGame.NumberOfRanks - 1].PieceTypeColour = -6;
            this.MyGame.MyPosition[0].MySquare[2, this.MyGame.NumberOfRanks - 1].PieceTypeColour = -4;
            this.MyGame.MyPosition[0].MySquare[3, this.MyGame.NumberOfRanks - 1].PieceTypeColour = -5;
            this.MyGame.MyPosition[0].MySquare[4, this.MyGame.NumberOfRanks - 1].PieceTypeColour = -1;
            this.MyGame.MyPosition[0].MySquare[5, this.MyGame.NumberOfRanks - 1].PieceTypeColour = -2;
            this.MyGame.MyPosition[0].MySquare[6, this.MyGame.NumberOfRanks - 1].PieceTypeColour = -5;
            this.MyGame.MyPosition[0].MySquare[7, this.MyGame.NumberOfRanks - 1].PieceTypeColour = -4;
            this.MyGame.MyPosition[0].MySquare[8, this.MyGame.NumberOfRanks - 1].PieceTypeColour = -7;
            this.MyGame.MyPosition[0].MySquare[9, this.MyGame.NumberOfRanks - 1].PieceTypeColour = -3;
        }


        public void GenerateRandomMateIn_n()
        {
            PosEvaluationResult MyStaticEvaluation;

            int p;
            decimal InitialGSPC;

            MyStaticEvaluation.MeInCheck = false;
            MyStaticEvaluation.IsStaleMate = false;
            MyStaticEvaluation.IsMate = false;
            MyStaticEvaluation.IsDrawByMaterial = false;
            MyStaticEvaluation.PositionAdvantage = 0;
            MyStaticEvaluation.BestMoveidx = 0;

            this.MyGame.ExternalAbort = false;
            while (MyStaticEvaluation.PositionAdvantage != 100 - this.NumberOfPliesToCalculate &
                   MyStaticEvaluation.PositionAdvantage != this.NumberOfPliesToCalculate - 100 &
                   this.MyGame.ExternalAbort == false)
            {
                this.GenerateRandomPosition();
                p = this.MyGame.NumberOfPositionsInGame - 1;
                
                if (this.MyGame.MyPosition[p].ColourToMove == 1)
                {
                    InitialGSPC = 120;
                } else
                {
                    InitialGSPC = -120;
                }
                
                MyStaticEvaluation = EvaluationByCalculation(p, this.NumberOfPliesToCalculate, InitialGSPC, true);
            }
            MessageBox.Show(PosEvaluationResultAsString(MyStaticEvaluation));
        }
        public void SuggestMove()
        {
            PosEvaluationResult MyStaticEvaluation;
                        
            int p;
            decimal InitialGSPC;
            string s;

            p = this.MyGame.NumberOfPositionsInGame - 1;

            if (this.MyGame.MyPosition[p].ColourToMove == 1)
            {
                InitialGSPC = 120;
            }
            else
            {
                InitialGSPC = -120;
            }

            this.MyGame.ExternalAbort = false;
            this.MyGame.CalculationLineMessage = "";
            MyStaticEvaluation = EvaluationByCalculation(p, this.NumberOfPliesToCalculate, InitialGSPC, true);

            s = "SuggestMove finished - " + this.MoveAsString(this.MyGame.MyPosition[p], MyStaticEvaluation.BestMoveidx)
                    + "|" + PosEvaluationResultAsString(MyStaticEvaluation);
            
            this.MyGame.StatusMessage = s;
        }

        public void SuggestMoveAndDo()
        {
            PosEvaluationResult MyStaticEvaluation;

            int p;
            decimal InitialGSPC;
            string s;

            p = this.MyGame.NumberOfPositionsInGame - 1;

            if (this.MyGame.MyPosition[p].ColourToMove == 1)
            {
                InitialGSPC = 120;
            }
            else
            {
                InitialGSPC = -120;
            }

            this.MyGame.ExternalAbort = false;
            this.MyGame.CalculationLineMessage = "";
            MyStaticEvaluation = EvaluationByCalculation(p, this.NumberOfPliesToCalculate, InitialGSPC, true);

            s = "SuggestMoveAndDo finished - " + this.MoveAsString(this.MyGame.MyPosition[p], MyStaticEvaluation.BestMoveidx)
                    + "|" + PosEvaluationResultAsString(MyStaticEvaluation);

            if (this.MyGame.ExternalAbort == false)
            {
                this.DoMove(p, MyStaticEvaluation.BestMoveidx);
                this.MyGame.ActualCurrentPositionidx++;
            }

            this.MyGame.StatusMessage = s;
        }

        //Here the section starts that makes a move effective in the next position

        private void UndoLastMove(int pFromPositionNumber)
        {
            //int i;
            //int j;

            //I presume that we could skip this for-loop, information is always completely overwritten
            //for (i = 0; i < this.MyGame.NumberOfFiles; i++)
            //{
            //    for (j = 0; j < this.MyGame.NumberOfRanks; j++)
            //    {
            //        this.MyGame.MyPosition[pFromPositionNumber].MySquare[i, j].PieceTypeColour = 0;
            //        this.MyGame.MyPosition[pFromPositionNumber].MySquare[i, j].EnPassantLeftAllowed = false;
            //        this.MyGame.MyPosition[pFromPositionNumber].MySquare[i, j].EnPassantRightAllowed = false;
            //    }
            //}

            this.MyGame.MyPosition[pFromPositionNumber].ColourToMove = 0;

            this.MyGame.MyPosition[pFromPositionNumber].CastleWhiteRightBlockedPerm = false;
            this.MyGame.MyPosition[pFromPositionNumber].CastleWhiteLeftBlockedPerm = false;
            this.MyGame.MyPosition[pFromPositionNumber].CastleBlackRightBlockedPerm = false;
            this.MyGame.MyPosition[pFromPositionNumber].CastleBlackLeftBlockedPerm = false;

            this.MyGame.NumberOfPositionsInGame--;
        }


        public void DoEnteredMove(int pFromPositionNumber)
        {
            this.DoMove(pFromPositionNumber, this.MyGame.EnteredMoveIdentifiedidx);
            this.MyGame.EnteredMoveIdentifiedidx = -1;
            this.MyGame.ActualCurrentPositionidx++;
            this.MyGame.StatusMessage = "DoEnteredMove finished";
        }
        private void DoMove(int pFromPositionNumber, int pMoveidx)
        {
            byte from_i;
            byte from_j;
            byte to_i;
            byte to_j;
            bool MoveDone;

            MoveDone = false;

            //First we copy the content of the current position to the next entry of array MyPosition
            //This initializes the e.p. flags and already switches the ColourToMove
            this.CopyPositionFrom(pFromPositionNumber);

            //Increase NumberOfPositionsInGame
            this.MyGame.NumberOfPositionsInGame++;

            from_i = this.MyGame.MyPosition[pFromPositionNumber].MovesFromHere[pMoveidx].from_i;
            from_j = this.MyGame.MyPosition[pFromPositionNumber].MovesFromHere[pMoveidx].from_j;
            to_i = this.MyGame.MyPosition[pFromPositionNumber].MovesFromHere[pMoveidx].to_i;
            to_j = this.MyGame.MyPosition[pFromPositionNumber].MovesFromHere[pMoveidx].to_j;


            //Now set the e.p. flags in the new position
            if (this.MyGame.MyPosition[pFromPositionNumber].MySquare[from_i, from_j].PieceTypeColour == 8 |
                this.MyGame.MyPosition[pFromPositionNumber].MySquare[from_i, from_j].PieceTypeColour == -8)
            {
                if (Math.Abs(from_j - to_j) == 2)
                {
                    if (to_i < this.MyGame.NumberOfFiles - 1)
                    {
                        if (this.MyGame.MyPosition[pFromPositionNumber].MySquare[from_i, from_j].PieceTypeColour *
                            this.MyGame.MyPosition[pFromPositionNumber].MySquare[to_i + 1, to_j].PieceTypeColour == -64)
                        {
                            this.MyGame.MyPosition[pFromPositionNumber + 1].MySquare[to_i + 1, to_j].EnPassantLeftAllowed = true;
                        }
                    }
                    if (to_i > 0)
                    {
                        if (this.MyGame.MyPosition[pFromPositionNumber].MySquare[from_i, from_j].PieceTypeColour *
                            this.MyGame.MyPosition[pFromPositionNumber].MySquare[to_i - 1, to_j].PieceTypeColour == -64)
                        {
                            this.MyGame.MyPosition[pFromPositionNumber + 1].MySquare[to_i - 1, to_j].EnPassantRightAllowed = true;
                        }
                    }
                }
            }

            //Do the en passant capture
            if (this.MyGame.MyPosition[pFromPositionNumber].MySquare[from_i, from_j].EnPassantLeftAllowed == true &
               to_i == from_i - 1 & MoveDone == false)
            {
                this.DoMoveEnPassant(pFromPositionNumber + 1, from_i, from_j, to_i, to_j);
                MoveDone = true;
            }
            if (this.MyGame.MyPosition[pFromPositionNumber].MySquare[from_i, from_j].EnPassantRightAllowed == true &
               to_i == from_i + 1 & MoveDone == false)
            {
                this.DoMoveEnPassant(pFromPositionNumber + 1, from_i, from_j, to_i, to_j);
                MoveDone = true;
            }


            //Do the castling
            if (this.MyGame.MyPosition[pFromPositionNumber].MySquare[from_i, from_j].PieceTypeColour == 1 |
                this.MyGame.MyPosition[pFromPositionNumber].MySquare[from_i, from_j].PieceTypeColour == -1)
            {
                if (Math.Abs(from_i - to_i) == this.MyGame.CastleDistance & MoveDone == false)
                {
                    this.DoMoveCastle(pFromPositionNumber + 1, from_i, from_j, to_i, to_j);
                    MoveDone = true;
                }
            }

            //Do the other moves
            if (MoveDone == false)
            {
                this.DoMoveGeneric(pFromPositionNumber + 1, from_i, from_j, to_i, to_j);
                MoveDone = true;
            }
            //(Overwriting the PieceTypeColour implicitly removes the captured piece)


            //Promote a pawn moving to the last rank
            if (this.MyGame.MyPosition[pFromPositionNumber].MySquare[from_i, from_j].PieceTypeColour == 8 &
                to_j == this.MyGame.NumberOfRanks - 1)
            {
                this.MyGame.MyPosition[pFromPositionNumber + 1].MySquare[to_i, to_j].PieceTypeColour =
                this.MyGame.MyPosition[pFromPositionNumber].MovesFromHere[pMoveidx].PromoteToPiece;
            }
            if (this.MyGame.MyPosition[pFromPositionNumber].MySquare[from_i, from_j].PieceTypeColour == -8 &
                to_j == 0)
            {
                this.MyGame.MyPosition[pFromPositionNumber + 1].MySquare[to_i, to_j].PieceTypeColour =
                this.MyGame.MyPosition[pFromPositionNumber].MovesFromHere[pMoveidx].PromoteToPiece;
            }


            //Detect that castle is no longer allowed after a Rook move or King move
            if (from_i == 0 & from_j == 0 & 
                this.MyGame.MyPosition[pFromPositionNumber].MySquare[from_i, from_j].PieceTypeColour == 3)
            {
                this.MyGame.MyPosition[pFromPositionNumber + 1].CastleWhiteLeftBlockedPerm = true;
            }
            if (from_i == this.MyGame.NumberOfFiles - 1 & from_j == 0 & 
                this.MyGame.MyPosition[pFromPositionNumber].MySquare[from_i, from_j].PieceTypeColour == 3)
            {
                this.MyGame.MyPosition[pFromPositionNumber + 1].CastleWhiteRightBlockedPerm = true;
            }
            if (from_i == 0 & from_j == this.MyGame.NumberOfRanks - 1 & 
                this.MyGame.MyPosition[pFromPositionNumber].MySquare[from_i, from_j].PieceTypeColour == -3)
            {
                this.MyGame.MyPosition[pFromPositionNumber + 1].CastleBlackLeftBlockedPerm = true;
            }
            if (from_i == this.MyGame.NumberOfFiles - 1 & from_j == this.MyGame.NumberOfRanks - 1 & 
                this.MyGame.MyPosition[pFromPositionNumber].MySquare[from_i, from_j].PieceTypeColour == -3)
            {
                this.MyGame.MyPosition[pFromPositionNumber + 1].CastleBlackRightBlockedPerm = true;
            }
            if (this.MyGame.MyPosition[pFromPositionNumber].MySquare[from_i, from_j].PieceTypeColour == 1)
            {
                this.MyGame.MyPosition[pFromPositionNumber + 1].CastleWhiteLeftBlockedPerm = true;
                this.MyGame.MyPosition[pFromPositionNumber + 1].CastleWhiteRightBlockedPerm = true;
            }
            if (this.MyGame.MyPosition[pFromPositionNumber].MySquare[from_i, from_j].PieceTypeColour == -1)
            {
                this.MyGame.MyPosition[pFromPositionNumber + 1].CastleBlackLeftBlockedPerm = true;
                this.MyGame.MyPosition[pFromPositionNumber + 1].CastleBlackRightBlockedPerm = true;
            }

        }

        private void CopyPositionFrom(int pFromPositionNumber)
        {
            int i;
            int j;
            int p;
            int q;

            p = pFromPositionNumber;
            q = p + 1;

            for (i = 0; i < this.MyGame.NumberOfFiles; i++)
            {
                for (j = 0; j < this.MyGame.NumberOfRanks; j++)
                {
                    this.MyGame.MyPosition[q].MySquare[i, j].PieceTypeColour = this.MyGame.MyPosition[p].MySquare[i, j].PieceTypeColour;
                    this.MyGame.MyPosition[q].MySquare[i, j].EnPassantLeftAllowed = false;
                    this.MyGame.MyPosition[q].MySquare[i, j].EnPassantRightAllowed = false;
                }
            }
            this.MyGame.MyPosition[q].ColourToMove = (sbyte)((-1) * this.MyGame.MyPosition[p].ColourToMove);

            this.MyGame.MyPosition[q].CastleWhiteRightBlockedPerm = this.MyGame.MyPosition[p].CastleWhiteRightBlockedPerm;
            this.MyGame.MyPosition[q].CastleWhiteLeftBlockedPerm = this.MyGame.MyPosition[p].CastleWhiteLeftBlockedPerm;
            this.MyGame.MyPosition[q].CastleBlackRightBlockedPerm = this.MyGame.MyPosition[p].CastleBlackRightBlockedPerm;
            this.MyGame.MyPosition[q].CastleBlackLeftBlockedPerm = this.MyGame.MyPosition[p].CastleBlackLeftBlockedPerm;
        }

        private void DoMoveEnPassant(int pPositionNumber, byte pfrom_i, byte pfrom_j, byte pto_i, byte pto_j)
        {
            this.MyGame.MyPosition[pPositionNumber].MySquare[pto_i, pto_j].PieceTypeColour =
                this.MyGame.MyPosition[pPositionNumber].MySquare[pfrom_i, pfrom_j].PieceTypeColour;
            this.MyGame.MyPosition[pPositionNumber].MySquare[pfrom_i, pfrom_j].PieceTypeColour = 0;
            this.MyGame.MyPosition[pPositionNumber].MySquare[pto_i, pfrom_j].PieceTypeColour = 0;
        }

        private void DoMoveCastle(int pPositionNumber, byte pfrom_i, byte pfrom_j, byte pto_i, byte pto_j)
        {
            this.MyGame.MyPosition[pPositionNumber].MySquare[pto_i, pto_j].PieceTypeColour =
                this.MyGame.MyPosition[pPositionNumber].MySquare[pfrom_i, pfrom_j].PieceTypeColour;
            this.MyGame.MyPosition[pPositionNumber].MySquare[pfrom_i, pfrom_j].PieceTypeColour = 0;
            if (pto_i < pfrom_i)
            {
                this.MyGame.MyPosition[pPositionNumber].MySquare[pto_i + 1, pto_j].PieceTypeColour =
                    this.MyGame.MyPosition[pPositionNumber].MySquare[0, pfrom_j].PieceTypeColour;
                this.MyGame.MyPosition[pPositionNumber].MySquare[0, pfrom_j].PieceTypeColour = 0;
            }
            else
            {
                this.MyGame.MyPosition[pPositionNumber].MySquare[pto_i - 1, pto_j].PieceTypeColour =
                    this.MyGame.MyPosition[pPositionNumber].MySquare[this.MyGame.NumberOfFiles - 1, pfrom_j].PieceTypeColour;
                this.MyGame.MyPosition[pPositionNumber].MySquare[MyGame.NumberOfFiles - 1, pfrom_j].PieceTypeColour = 0;
            }
        }

        private void DoMoveGeneric(int pPositionNumber, byte pfrom_i, byte pfrom_j, byte pto_i, byte pto_j)
        {
            this.MyGame.MyPosition[pPositionNumber].MySquare[pto_i, pto_j].PieceTypeColour =
                this.MyGame.MyPosition[pPositionNumber].MySquare[pfrom_i, pfrom_j].PieceTypeColour;
            this.MyGame.MyPosition[pPositionNumber].MySquare[pfrom_i, pfrom_j].PieceTypeColour = 0;
        }

        //Here the section ENDS that makes a move effective in the next position


        //Here the section starts that generates a list of moves from a given position
        private void Init_Move_Evaluation_Results(int pPositionNumber)
        {
            int nm;
            for (nm = 0; nm < this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves; nm++)
            {
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].MyResult.MeInCheck = false;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].MyResult.IsStaleMate = false;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].MyResult.IsMate = false;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].MyResult.IsDrawByMaterial = false;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].MyResult.PositionAdvantage = 0;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].MyResult.BestMoveidx = -1;
            }
        }

        private void ListMovesAndEnrich(int pPositionNumber, int pPurpose)
        {
            //pPurpose: 0 = full, 1 = scan if opponent is in check, 2 = scan opponent's attacks

            byte i1;
            byte j1;

            if (pPurpose == 0 | pPurpose == 1)
            {
                this.Enrich_Initialize(pPositionNumber);
                this.EnrichWitchInfluence(pPositionNumber);
            }

            //We want to mark the squares attacked by the opponent by looking at all possible opponent's moves:
            if (pPurpose == 0)
            {
                this.ListMovesAndEnrich(pPositionNumber, 2);
            }

            //This has been added in MovesFromHere, but now resetting the counter to overwrite it

            this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves = 0;
            //The content of the array MovesFromHere is not erased because no fast method exists for this.
            //The initialize method does not accomplish this
            //So possibly, beyond NumberOfFoundMoves - 1, values are still in place from previous calls to ListMoves

            if (pPurpose == 2)
            {
                //swap ColourToMove
                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == -1)
                {
                    this.MyGame.MyPosition[pPositionNumber].ColourToMove = 1;
                }
                else
                {
                    this.MyGame.MyPosition[pPositionNumber].ColourToMove = -1;
                }
            }

            for (i1 = 0; i1 < this.MyGame.NumberOfFiles; i1++)
            {
                for (j1 = 0; j1 < this.MyGame.NumberOfRanks; j1++)
                {
                    switch (this.MyGame.MyPosition[pPositionNumber].MySquare[i1, j1].PieceTypeColour)
                    {
                        case 1:
                            this.MyGame.MyPosition[pPositionNumber].WhiteKingLoci = i1;
                            this.MyGame.MyPosition[pPositionNumber].WhiteKingLocj = j1;
                            break;
                        case -1:
                            this.MyGame.MyPosition[pPositionNumber].BlackKingLoci = i1;
                            this.MyGame.MyPosition[pPositionNumber].BlackKingLocj = j1;
                            break;
                    }
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i1, j1].PieceTypeColour *
                        this.MyGame.MyPosition[pPositionNumber].ColourToMove > 0)
                    {
                        switch (this.MyGame.MyPosition[pPositionNumber].MySquare[i1, j1].PieceTypeColour)
                        {
                            case 1: this.ListMoves_Guard(i1, j1, pPositionNumber); break;
                            case -1: this.ListMoves_Guard(i1, j1, pPositionNumber); break;
                            case 6: this.ListMoves_Guard(i1, j1, pPositionNumber); break;
                            case -6: this.ListMoves_Guard(i1, j1, pPositionNumber); break;
                            case 4: this.ListMoves_Knight(i1, j1, pPositionNumber); break;
                            case -4: this.ListMoves_Knight(i1, j1, pPositionNumber); break;
                            case 8: this.ListMoves_White_Pawn(i1, j1, pPositionNumber); break;
                            case -8: this.ListMoves_Black_Pawn(i1, j1, pPositionNumber); break;
                            case 3: this.ListMoves_Rook(i1, j1, pPositionNumber); break;
                            case -3: this.ListMoves_Rook(i1, j1, pPositionNumber); break;
                            case 5: this.ListMoves_Bishop(i1, j1, pPositionNumber); break;
                            case -5: this.ListMoves_Bishop(i1, j1, pPositionNumber); break;
                            case 7:
                                if (pPurpose == 0)
                                {
                                    this.ListMoves_Witch(i1, j1, pPositionNumber);
                                }
                                break;
                            case -7:
                                if (pPurpose == 0)
                                {
                                    this.ListMoves_Witch(i1, j1, pPositionNumber);
                                }
                                break;
                            case 2:
                                this.ListMoves_Rook(i1, j1, pPositionNumber);
                                this.ListMoves_Bishop(i1, j1, pPositionNumber);
                                break;
                            case -2:
                                this.ListMoves_Rook(i1, j1, pPositionNumber);
                                this.ListMoves_Bishop(i1, j1, pPositionNumber);
                                break;
                        }
                    }
                }
            }

            if (pPurpose == 2)
            {
                //UNDO swap ColourToMove
                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == -1)
                {
                    this.MyGame.MyPosition[pPositionNumber].ColourToMove = 1;
                }
                else
                {
                    this.MyGame.MyPosition[pPositionNumber].ColourToMove = -1;
                }
            }

            if (pPurpose == 0 | pPurpose == 1)
            {
                this.Enrich_IsInCheck(pPositionNumber);
            }

            if (pPurpose == 0)
            {
                this.EnrichIsCloseToKing(pPositionNumber);
                this.EnrichCloseToOtherKingScore(pPositionNumber);
                this.Enrich_CastleWhiteLeftBlockedTemp(pPositionNumber);
                this.Enrich_CastleWhiteRightBlockedTemp(pPositionNumber);
                this.Enrich_CastleBlackLeftBlockedTemp(pPositionNumber);
                this.Enrich_CastleBlackRightBlockedTemp(pPositionNumber);


                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove > 0)
                {
                    i1 = this.MyGame.MyPosition[pPositionNumber].WhiteKingLoci;
                    j1 = this.MyGame.MyPosition[pPositionNumber].WhiteKingLocj;
                    this.ListMoves_King(i1, j1, pPositionNumber);
                }
                else
                {
                    i1 = this.MyGame.MyPosition[pPositionNumber].BlackKingLoci;
                    j1 = this.MyGame.MyPosition[pPositionNumber].BlackKingLocj;
                    this.ListMoves_King(i1, j1, pPositionNumber);
                }

                //Result of evaluation of moves must explicitly be initialized, as it is only optionally updated
                this.Init_Move_Evaluation_Results(pPositionNumber);
            }

        }

        private void ListMoves_Guard(byte pi, byte pj, int pPositionNumber)
        {
            int i2;
            int j2;
            int nm;
            for (i2 = Math.Max(0, pi - 1); i2 < Math.Min(this.MyGame.NumberOfFiles, pi + 2); i2++)
            {
                for (j2 = Math.Max(0, pj - 1); j2 < Math.Min(this.MyGame.NumberOfRanks, pj + 2); j2++)
                {
                    if (i2 != pi | j2 != pj)
                    {

                        if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour > 0)
                        { this.MyGame.MyPosition[pPositionNumber].MySquare[i2, j2].IsAttackedByWhite = true; }
                        else
                        { this.MyGame.MyPosition[pPositionNumber].MySquare[i2, j2].IsAttackedByBlack = true; }

                        if (MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour *
                            MyGame.MyPosition[pPositionNumber].MySquare[i2, j2].PieceTypeColour <= 0)
                        {
                            this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                            nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                            this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                            this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                            this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i2;
                            this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j2;
                        }
                    }
                }
            }

        }
        private void ListMoves_King(byte pi, byte pj, int pPositionNumber)
        {
            int nm;

            //Normal king moves have already been listed by procedure ListMoves_Guard
            if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == 1)
            {
                if (this.MyGame.MyPosition[pPositionNumber].CastleWhiteLeftBlockedPerm == false &
                    this.MyGame.MyPosition[pPositionNumber].CastleWhiteLeftBlockedTemp == false)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)(pi - this.MyGame.CastleDistance);
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = pj;
                }
                if (this.MyGame.MyPosition[pPositionNumber].CastleWhiteRightBlockedPerm == false &
                    this.MyGame.MyPosition[pPositionNumber].CastleWhiteRightBlockedTemp == false)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)(pi + this.MyGame.CastleDistance);
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = pj;
                }
            }
            else
            {
                if (this.MyGame.MyPosition[pPositionNumber].CastleBlackLeftBlockedPerm == false &
                    this.MyGame.MyPosition[pPositionNumber].CastleBlackLeftBlockedTemp == false)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)(pi - this.MyGame.CastleDistance);
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = pj;
                }
                if (this.MyGame.MyPosition[pPositionNumber].CastleBlackRightBlockedPerm == false &
                    this.MyGame.MyPosition[pPositionNumber].CastleBlackRightBlockedTemp == false)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)(pi + this.MyGame.CastleDistance);
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = pj;
                }
            }
        }
        private void ListMoves_Knight(byte pi, byte pj, int pPositionNumber)
        {
            int nm;
            if (pi > 0 & pj > 1)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour > 0)
                { this.MyGame.MyPosition[pPositionNumber].MySquare[pi - 1, pj - 2].IsAttackedByWhite = true; }
                else
                { this.MyGame.MyPosition[pPositionNumber].MySquare[pi - 1, pj - 2].IsAttackedByBlack = true; }
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour *
                    this.MyGame.MyPosition[pPositionNumber].MySquare[pi - 1, pj - 2].PieceTypeColour <= 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)(pi - 1);
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)(pj - 2);
                }
            }
            if (pi > 0 & pj < MyGame.NumberOfRanks - 2)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour > 0)
                { this.MyGame.MyPosition[pPositionNumber].MySquare[pi - 1, pj + 2].IsAttackedByWhite = true; }
                else
                { this.MyGame.MyPosition[pPositionNumber].MySquare[pi - 1, pj + 2].IsAttackedByBlack = true; }
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour *
                    this.MyGame.MyPosition[pPositionNumber].MySquare[pi - 1, pj + 2].PieceTypeColour <= 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)(pi - 1);
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)(pj + 2);
                }
            }
            if (pi < this.MyGame.NumberOfFiles - 1 & pj > 1)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour > 0)
                { this.MyGame.MyPosition[pPositionNumber].MySquare[pi + 1, pj - 2].IsAttackedByWhite = true; }
                else
                { this.MyGame.MyPosition[pPositionNumber].MySquare[pi + 1, pj - 2].IsAttackedByBlack = true; }
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour *
                    this.MyGame.MyPosition[pPositionNumber].MySquare[pi + 1, pj - 2].PieceTypeColour <= 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)(pi + 1);
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)(pj - 2);
                }
            }
            if (pi < this.MyGame.NumberOfFiles - 1 & pj < MyGame.NumberOfRanks - 2)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour > 0)
                { this.MyGame.MyPosition[pPositionNumber].MySquare[pi + 1, pj + 2].IsAttackedByWhite = true; }
                else
                { this.MyGame.MyPosition[pPositionNumber].MySquare[pi + 1, pj + 2].IsAttackedByBlack = true; }
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour *
                    this.MyGame.MyPosition[pPositionNumber].MySquare[pi + 1, pj + 2].PieceTypeColour <= 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)(pi + 1);
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)(pj + 2);
                }
            }
            if (pi > 1 & pj > 0)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour > 0)
                { this.MyGame.MyPosition[pPositionNumber].MySquare[pi - 2, pj - 1].IsAttackedByWhite = true; }
                else
                { this.MyGame.MyPosition[pPositionNumber].MySquare[pi - 2, pj - 1].IsAttackedByBlack = true; }
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour *
                    this.MyGame.MyPosition[pPositionNumber].MySquare[pi - 2, pj - 1].PieceTypeColour <= 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)(pi - 2);
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)(pj - 1);
                }
            }
            if (pi > 1 & pj < MyGame.NumberOfRanks - 1)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour > 0)
                { this.MyGame.MyPosition[pPositionNumber].MySquare[pi - 2, pj + 1].IsAttackedByWhite = true; }
                else
                { this.MyGame.MyPosition[pPositionNumber].MySquare[pi - 2, pj + 1].IsAttackedByBlack = true; }
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour *
                    this.MyGame.MyPosition[pPositionNumber].MySquare[pi - 2, pj + 1].PieceTypeColour <= 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)(pi - 2);
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)(pj + 1);
                }
            }
            if (pi < this.MyGame.NumberOfFiles - 2 & pj > 0)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour > 0)
                { this.MyGame.MyPosition[pPositionNumber].MySquare[pi + 2, pj - 1].IsAttackedByWhite = true; }
                else
                { this.MyGame.MyPosition[pPositionNumber].MySquare[pi + 2, pj - 1].IsAttackedByBlack = true; }
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour *
                    this.MyGame.MyPosition[pPositionNumber].MySquare[pi + 2, pj - 1].PieceTypeColour <= 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)(pi + 2);
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)(pj - 1);
                }
            }
            if (pi < this.MyGame.NumberOfFiles - 2 & pj < MyGame.NumberOfRanks - 1)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour > 0)
                { this.MyGame.MyPosition[pPositionNumber].MySquare[pi + 2, pj + 1].IsAttackedByWhite = true; }
                else
                { this.MyGame.MyPosition[pPositionNumber].MySquare[pi + 2, pj + 1].IsAttackedByBlack = true; }
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour *
                    this.MyGame.MyPosition[pPositionNumber].MySquare[pi + 2, pj + 1].PieceTypeColour <= 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)(pi + 2);
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)(pj + 1);
                }
            }
        }


        private void ListMoves_White_Pawn(byte pi, byte pj, int pPositionNumber)
        {
            int nm;
            if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj + 1].PieceTypeColour == 0)
            {
                this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = pi;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)(pj + 1);
                this.Promote_White_Pawn(pPositionNumber, pi, pj);
            }
            if (pj == 1)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj + 2].PieceTypeColour == 0 &
                    (this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj + 1].PieceTypeColour == 0 |
                    this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj + 1].WhiteWitchInfluence == true))
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)(pj + 2);
                }
            }
            if (pi > 0)
            {
                this.MyGame.MyPosition[pPositionNumber].MySquare[pi - 1, pj + 1].IsAttackedByWhite = true;
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi - 1, pj + 1].PieceTypeColour < 0 |
                    this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].EnPassantLeftAllowed == true)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)(pi - 1);
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)(pj + 1);
                    this.Promote_White_Pawn(pPositionNumber, pi, pj);

                }
            }
            if (pi < this.MyGame.NumberOfFiles - 1)
            {
                this.MyGame.MyPosition[pPositionNumber].MySquare[pi + 1, pj + 1].IsAttackedByWhite = true;
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi + 1, pj + 1].PieceTypeColour < 0 |
                    this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].EnPassantRightAllowed == true)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)(pi + 1);
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)(pj + 1);
                    this.Promote_White_Pawn(pPositionNumber, pi, pj);
                }
            }
        }


        private void Promote_White_Pawn(int pPositionNumber, byte pfrom_i, byte pfrom_j)
        {
            int nm;

            nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;
            if (this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j == this.MyGame.NumberOfRanks - 2 &
                this.MyGame.MyPosition[pPositionNumber].MySquare[pfrom_i, pfrom_j].AdvancePawnPotentialCounted == false)
            {
                this.MyGame.MyPosition[pPositionNumber].AdvancePawnPotential =
                           this.MyGame.MyPosition[pPositionNumber].AdvancePawnPotential + 2;
                this.MyGame.MyPosition[pPositionNumber].MySquare[pfrom_i, pfrom_j].AdvancePawnPotentialCounted = true;
                return;
            }
            if (this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j == this.MyGame.NumberOfRanks - 1)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pfrom_i, pfrom_j].AdvancePawnPotentialCounted == false)
                {
                    this.MyGame.MyPosition[pPositionNumber].AdvancePawnPotential =
                               this.MyGame.MyPosition[pPositionNumber].AdvancePawnPotential + 7;
                    this.MyGame.MyPosition[pPositionNumber].MySquare[pfrom_i, pfrom_j].AdvancePawnPotentialCounted = true;
                }

                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].PromoteToPiece = 2;

                this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].from_i;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].from_j;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].to_i;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].to_j;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].PromoteToPiece = 3;

                this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].from_i;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].from_j;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].to_i;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].to_j;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].PromoteToPiece = 4;

                this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].from_i;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].from_j;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].to_i;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].to_j;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].PromoteToPiece = 5;

                this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].from_i;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].from_j;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].to_i;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].to_j;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].PromoteToPiece = 6;

                this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].from_i;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].from_j;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].to_i;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].to_j;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].PromoteToPiece = 7;
            }
        }


        private void ListMoves_Black_Pawn(byte pi, byte pj, int pPositionNumber)
        {
            int nm;
            if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj - 1].PieceTypeColour == 0)
            {
                this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = pi;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)(pj - 1);
                this.Promote_Black_Pawn(pPositionNumber, pi, pj);
            }
            if (pj == this.MyGame.NumberOfRanks - 2)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj - 2].PieceTypeColour == 0 &
                    (this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj - 1].PieceTypeColour == 0 |
                    this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj - 1].BlackWitchInfluence == true))
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)(pj - 2);
                }
            }
            if (pi > 0)
            {
                this.MyGame.MyPosition[pPositionNumber].MySquare[pi - 1, pj - 1].IsAttackedByBlack = true;
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi - 1, pj - 1].PieceTypeColour > 0 |
                    this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].EnPassantLeftAllowed == true)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)(pi - 1);
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)(pj - 1);
                    this.Promote_Black_Pawn(pPositionNumber, pi, pj);
                }
            }
            if (pi < this.MyGame.NumberOfFiles - 1)
            {
                this.MyGame.MyPosition[pPositionNumber].MySquare[pi + 1, pj - 1].IsAttackedByBlack = true;
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi + 1, pj - 1].PieceTypeColour > 0 |
                    this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].EnPassantRightAllowed == true)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)(pi + 1);
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)(pj - 1);
                    this.Promote_Black_Pawn(pPositionNumber, pi, pj);
                }
            }
        }


        private void Promote_Black_Pawn(int pPositionNumber, byte pfrom_i, byte pfrom_j)
        {
            int nm;

            nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;
            if (this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j == 1 &
                this.MyGame.MyPosition[pPositionNumber].MySquare[pfrom_i, pfrom_j].AdvancePawnPotentialCounted == false)
            {
                this.MyGame.MyPosition[pPositionNumber].AdvancePawnPotential =
                           this.MyGame.MyPosition[pPositionNumber].AdvancePawnPotential - 2;
                this.MyGame.MyPosition[pPositionNumber].MySquare[pfrom_i, pfrom_j].AdvancePawnPotentialCounted = true;
                return;
            }
            if (this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j == 0)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pfrom_i, pfrom_j].AdvancePawnPotentialCounted == false)
                {
                    this.MyGame.MyPosition[pPositionNumber].AdvancePawnPotential =
                               this.MyGame.MyPosition[pPositionNumber].AdvancePawnPotential - 7;
                    this.MyGame.MyPosition[pPositionNumber].MySquare[pfrom_i, pfrom_j].AdvancePawnPotentialCounted = true;
                }

                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].PromoteToPiece = -2;

                this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].from_i;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].from_j;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].to_i;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].to_j;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].PromoteToPiece = -3;

                this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].from_i;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].from_j;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].to_i;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].to_j;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].PromoteToPiece = -4;

                this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].from_i;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].from_j;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].to_i;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].to_j;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].PromoteToPiece = -5;

                this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].from_i;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].from_j;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].to_i;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].to_j;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].PromoteToPiece = -6;

                this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].from_i;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].from_j;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].to_i;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm - 1].to_j;
                this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].PromoteToPiece = -7;
            }
        }


        private void ListMoves_Rook(byte pi, byte pj, int pPositionNumber)
        {
            int i;
            int j;
            int nm;
            bool RangeBlocked;
            int ep;
            bool WitchInfluence;

            i = pi + 1;
            j = pj;
            RangeBlocked = false;
            while (i < this.MyGame.NumberOfFiles & RangeBlocked == false)
            {
                ep = this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour *
                      this.MyGame.MyPosition[pPositionNumber].ColourToMove;

                WitchInfluence = false;
                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == -1 &
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].BlackWitchInfluence == true)
                {
                    WitchInfluence = true;
                }
                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == 1 &
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].WhiteWitchInfluence == true)
                {
                    WitchInfluence = true;
                }

                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == 1)
                {
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].IsAttackedByWhite = true;
                } else
                {
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].IsAttackedByBlack = true;
                }

                if (ep <= 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                if (ep != 0 & WitchInfluence == false)
                {
                    RangeBlocked = true;
                }
                i++;
            }
            i = pi - 1;
            j = pj;
            RangeBlocked = false;
            while (i > -1 & RangeBlocked == false)
            {
                ep = this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour *
                      this.MyGame.MyPosition[pPositionNumber].ColourToMove;

                WitchInfluence = false;
                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == -1 &
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].BlackWitchInfluence == true)
                {
                    WitchInfluence = true;
                }
                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == 1 &
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].WhiteWitchInfluence == true)
                {
                    WitchInfluence = true;
                }


                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == 1)
                {
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].IsAttackedByWhite = true;
                }
                else
                {
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].IsAttackedByBlack = true;
                }
                if (ep <= 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                if (ep != 0 & WitchInfluence == false)
                {
                    RangeBlocked = true;
                }
                i--;
            }

            i = pi;
            j = pj + 1;
            RangeBlocked = false;
            while (j < this.MyGame.NumberOfRanks & RangeBlocked == false)
            {
                ep = this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour *
                      this.MyGame.MyPosition[pPositionNumber].ColourToMove;

                WitchInfluence = false;
                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == -1 &
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].BlackWitchInfluence == true)
                {
                    WitchInfluence = true;
                }
                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == 1 &
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].WhiteWitchInfluence == true)
                {
                    WitchInfluence = true;
                }

                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == 1)
                {
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].IsAttackedByWhite = true;
                }
                else
                {
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].IsAttackedByBlack = true;
                }

                if (ep <= 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                if (ep != 0 & WitchInfluence == false)
                {
                    RangeBlocked = true;
                }
                j++;
            }

            i = pi;
            j = pj - 1;
            RangeBlocked = false;
            while (j > -1 & RangeBlocked == false)
            {
                ep = this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour *
                      this.MyGame.MyPosition[pPositionNumber].ColourToMove;

                WitchInfluence = false;
                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == -1 &
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].BlackWitchInfluence == true)
                {
                    WitchInfluence = true;
                }
                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == 1 &
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].WhiteWitchInfluence == true)
                {
                    WitchInfluence = true;
                }

                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == 1)
                {
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].IsAttackedByWhite = true;
                }
                else
                {
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].IsAttackedByBlack = true;
                }

                if (ep <= 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                if (ep != 0 & WitchInfluence == false)
                {
                    RangeBlocked = true;
                }
                j--;
            }

        }
        private void ListMoves_Bishop(byte pi, byte pj, int pPositionNumber)
        {
            int i;
            int j;
            int nm;
            bool RangeBlocked;
            int ep;
            bool WitchInfluence;

            i = pi + 1;
            j = pj + 1;
            RangeBlocked = false;
            while (i < this.MyGame.NumberOfFiles & j < this.MyGame.NumberOfRanks & RangeBlocked == false)
            {
                ep = this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour *
                    this.MyGame.MyPosition[pPositionNumber].ColourToMove;

                WitchInfluence = false;
                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == -1 &
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].BlackWitchInfluence == true)
                {
                    WitchInfluence = true;
                }
                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == 1 &
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].WhiteWitchInfluence == true)
                {
                    WitchInfluence = true;
                }

                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == 1)
                {
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].IsAttackedByWhite = true;
                }
                else
                {
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].IsAttackedByBlack = true;
                }
                if (ep <= 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                if (ep != 0 & WitchInfluence == false)
                {
                    RangeBlocked = true;
                }

                i++;
                j++;
            }
            i = pi + 1;
            j = pj - 1;
            RangeBlocked = false;
            while (i < this.MyGame.NumberOfFiles & j > -1 & RangeBlocked == false)
            {
                ep = this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour *
                    this.MyGame.MyPosition[pPositionNumber].ColourToMove;

                WitchInfluence = false;
                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == -1 &
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].BlackWitchInfluence == true)
                {
                    WitchInfluence = true;
                }
                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == 1 &
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].WhiteWitchInfluence == true)
                {
                    WitchInfluence = true;
                }

                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == 1)
                {
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].IsAttackedByWhite = true;
                }
                else
                {
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].IsAttackedByBlack = true;
                }
                if (ep <= 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                if (ep != 0 & WitchInfluence == false)
                {
                    RangeBlocked = true;
                }

                i++;
                j--;
            }


            i = pi - 1;
            j = pj + 1;
            RangeBlocked = false;
            while (i > -1 & j < this.MyGame.NumberOfRanks & RangeBlocked == false)
            {
                ep = this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour *
                    this.MyGame.MyPosition[pPositionNumber].ColourToMove;

                WitchInfluence = false;
                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == -1 &
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].BlackWitchInfluence == true)
                {
                    WitchInfluence = true;
                }
                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == 1 &
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].WhiteWitchInfluence == true)
                {
                    WitchInfluence = true;
                }

                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == 1)
                {
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].IsAttackedByWhite = true;
                }
                else
                {
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].IsAttackedByBlack = true;
                }
                if (ep <= 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                if (ep != 0 & WitchInfluence == false)
                {
                    RangeBlocked = true;
                }

                i--;
                j++;
            }

            i = pi - 1;
            j = pj - 1;
            RangeBlocked = false;
            while (i > -1 & j > -1 & RangeBlocked == false)
            {
                ep = this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour *
                    this.MyGame.MyPosition[pPositionNumber].ColourToMove;

                WitchInfluence = false;
                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == -1 &
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].BlackWitchInfluence == true)
                {
                    WitchInfluence = true;
                }
                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == 1 &
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].WhiteWitchInfluence == true)
                {
                    WitchInfluence = true;
                }

                if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == 1)
                {
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].IsAttackedByWhite = true;
                }
                else
                {
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].IsAttackedByBlack = true;
                }
                if (ep <= 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                if (ep != 0 & WitchInfluence == false)
                {
                    RangeBlocked = true;
                }

                i--;
                j--;
            }
        }

        private void ListMoves_Witch(byte pi, byte pj, int pPositionNumber)
        {
            int i;
            int j;
            int nm;
            bool RangeBlocked;

            RangeBlocked = false;

            i = pi + 1;
            j = pj;
            if (i < this.MyGame.NumberOfFiles)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                else
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].ExtraWhiteWitchInfluence == false
                        & this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == 7)
                    {
                        RangeBlocked = true;
                    }
                    else
                    {
                        if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].ExtraBlackWitchInfluence == false
                            & this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == -7)
                        {
                            RangeBlocked = true;
                        }
                    }
                }
            }


            i = pi + 2;
            if (i < this.MyGame.NumberOfFiles)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                else
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].WhiteWitchInfluence == false &
                        this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == 7)
                    {
                        RangeBlocked = true;
                    }
                    else
                    {
                        if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].BlackWitchInfluence == false &
                            this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == -7)
                        {
                            RangeBlocked = true;
                        }
                    }
                }
            }

            i = pi + 3;
            while (i < this.MyGame.NumberOfFiles & RangeBlocked == false)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                else
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].WhiteWitchInfluence == false &
                        this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == 7)
                    {
                        RangeBlocked = true;
                    }
                    else
                    {
                        if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].BlackWitchInfluence == false &
                            this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == -7)
                        {
                            RangeBlocked = true;
                        }
                    }
                }
                i++;
            }
            RangeBlocked = false;
            i = pi - 1;
            j = pj;
            if (i > -1)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                else
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].ExtraWhiteWitchInfluence == false
                        & this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == 7)
                    {
                        RangeBlocked = true;
                    }
                    else
                    {
                        if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].ExtraBlackWitchInfluence == false
                            & this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == -7)
                        {
                            RangeBlocked = true;
                        }
                    }
                }
            }


            i = pi - 2;
            if (i > -1)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                else
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].WhiteWitchInfluence == false &
                        this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == 7)
                    {
                        RangeBlocked = true;
                    }
                    else
                    {
                        if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].BlackWitchInfluence == false &
                            this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == -7)
                        {
                            RangeBlocked = true;
                        }
                    }
                }
            }

            i = pi - 3;
            while (i > -1 & RangeBlocked == false)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                else
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].WhiteWitchInfluence == false &
                        this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == 7)
                    {
                        RangeBlocked = true;
                    }
                    else
                    {
                        if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].BlackWitchInfluence == false &
                            this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == -7)
                        {
                            RangeBlocked = true;
                        }
                    }
                }
                i--;
            }

            RangeBlocked = false;
            i = pi;
            j = pj + 1;
            if (j < this.MyGame.NumberOfRanks)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                else
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].ExtraWhiteWitchInfluence == false
                        & this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == 7)
                    {
                        RangeBlocked = true;
                    }
                    else
                    {
                        if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].ExtraBlackWitchInfluence == false
                            & this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == -7)
                        {
                            RangeBlocked = true;
                        }
                    }
                }
            }


            j = pj + 2;
            if (j < this.MyGame.NumberOfRanks)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                else
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].WhiteWitchInfluence == false &
                        this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == 7)
                    {
                        RangeBlocked = true;
                    }
                    else
                    {
                        if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].BlackWitchInfluence == false &
                            this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == -7)
                        {
                            RangeBlocked = true;
                        }
                    }
                }
            }

            j = pj + 3;
            while (j < this.MyGame.NumberOfRanks & RangeBlocked == false)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                else
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].WhiteWitchInfluence == false &
                        this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == 7)
                    {
                        RangeBlocked = true;
                    }
                    else
                    {
                        if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].BlackWitchInfluence == false &
                            this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == -7)
                        {
                            RangeBlocked = true;
                        }
                    }
                }
                j++;
            }

            RangeBlocked = false;
            i = pi;
            j = pj - 1;
            if (j > -1)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                else
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].ExtraWhiteWitchInfluence == false
                        & this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == 7)
                    {
                        RangeBlocked = true;
                    }
                    else
                    {
                        if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].ExtraBlackWitchInfluence == false
                            & this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == -7)
                        {
                            RangeBlocked = true;
                        }
                    }
                }
            }


            j = pj - 2;
            if (j > -1)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                else
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].WhiteWitchInfluence == false &
                        this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == 7)
                    {
                        RangeBlocked = true;
                    }
                    else
                    {
                        if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].BlackWitchInfluence == false &
                            this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == -7)
                        {
                            RangeBlocked = true;
                        }
                    }
                }
            }

            j = pj - 3;
            while (j > -1 & RangeBlocked == false)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                else
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].WhiteWitchInfluence == false &
                        this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == 7)
                    {
                        RangeBlocked = true;
                    }
                    else
                    {
                        if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].BlackWitchInfluence == false &
                            this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == -7)
                        {
                            RangeBlocked = true;
                        }
                    }
                }
                j--;
            }

            RangeBlocked = false;
            i = pi + 1;
            j = pj + 1;
            if (i < this.MyGame.NumberOfFiles & j < this.MyGame.NumberOfRanks)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                else
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].ExtraWhiteWitchInfluence == false
                        & this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == 7)
                    {
                        RangeBlocked = true;
                    }
                    else
                    {
                        if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].ExtraBlackWitchInfluence == false
                            & this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == -7)
                        {
                            RangeBlocked = true;
                        }
                    }
                }
            }


            i = pi + 2;
            j = pj + 2;
            if (i < this.MyGame.NumberOfFiles & j < this.MyGame.NumberOfRanks)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                else
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].WhiteWitchInfluence == false &
                        this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == 7)
                    {
                        RangeBlocked = true;
                    }
                    else
                    {
                        if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].BlackWitchInfluence == false &
                            this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == -7)
                        {
                            RangeBlocked = true;
                        }
                    }
                }
            }

            i = pi + 3;
            j = pj + 3;
            while (i < this.MyGame.NumberOfFiles & j < this.MyGame.NumberOfRanks & RangeBlocked == false)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                else
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].WhiteWitchInfluence == false &
                        this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == 7)
                    {
                        RangeBlocked = true;
                    }
                    else
                    {
                        if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].BlackWitchInfluence == false &
                            this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == -7)
                        {
                            RangeBlocked = true;
                        }
                    }
                }
                i++;
                j++;
            }

            RangeBlocked = false;
            i = pi + 1;
            j = pj - 1;
            if (i < this.MyGame.NumberOfFiles & j > -1)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                else
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].ExtraWhiteWitchInfluence == false
                        & this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == 7)
                    {
                        RangeBlocked = true;
                    }
                    else
                    {
                        if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].ExtraBlackWitchInfluence == false
                            & this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == -7)
                        {
                            RangeBlocked = true;
                        }
                    }
                }
            }


            i = pi + 2;
            j = pj - 2;
            if (i < this.MyGame.NumberOfFiles & j > -1)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                else
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].WhiteWitchInfluence == false &
                        this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == 7)
                    {
                        RangeBlocked = true;
                    }
                    else
                    {
                        if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].BlackWitchInfluence == false &
                            this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == -7)
                        {
                            RangeBlocked = true;
                        }
                    }
                }
            }

            i = pi + 3;
            j = pj - 3;
            while (i < this.MyGame.NumberOfFiles & j > -1 & RangeBlocked == false)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                else
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].WhiteWitchInfluence == false &
                        this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == 7)
                    {
                        RangeBlocked = true;
                    }
                    else
                    {
                        if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].BlackWitchInfluence == false &
                            this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == -7)
                        {
                            RangeBlocked = true;
                        }
                    }
                }
                i++;
                j--;
            }

            RangeBlocked = false;
            i = pi - 1;
            j = pj - 1;
            if (i > -1 & j > -1)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                else
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].ExtraWhiteWitchInfluence == false
                        & this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == 7)
                    {
                        RangeBlocked = true;
                    }
                    else
                    {
                        if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].ExtraBlackWitchInfluence == false
                            & this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == -7)
                        {
                            RangeBlocked = true;
                        }
                    }
                }
            }


            i = pi - 2;
            j = pj - 2;
            if (i > -1 & j > -1)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                else
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].WhiteWitchInfluence == false &
                        this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == 7)
                    {
                        RangeBlocked = true;
                    }
                    else
                    {
                        if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].BlackWitchInfluence == false &
                            this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == -7)
                        {
                            RangeBlocked = true;
                        }
                    }
                }
            }

            i = pi - 3;
            j = pj - 3;
            while (i > -1 & j > -1 & RangeBlocked == false)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                else
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].WhiteWitchInfluence == false &
                        this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == 7)
                    {
                        RangeBlocked = true;
                    }
                    else
                    {
                        if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].BlackWitchInfluence == false &
                            this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == -7)
                        {
                            RangeBlocked = true;
                        }
                    }
                }
                i--;
                j--;
            }

            RangeBlocked = false;
            i = pi - 1;
            j = pj + 1;
            if (i > -1 & j < this.MyGame.NumberOfRanks)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                else
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].ExtraWhiteWitchInfluence == false
                        & this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == 7)
                    {
                        RangeBlocked = true;
                    }
                    else
                    {
                        if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].ExtraBlackWitchInfluence == false
                            & this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == -7)
                        {
                            RangeBlocked = true;
                        }
                    }
                }
            }


            i = pi - 2;
            j = pj + 2;
            if (i > -1 & j < this.MyGame.NumberOfRanks)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                else
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].WhiteWitchInfluence == false &
                        this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == 7)
                    {
                        RangeBlocked = true;
                    }
                    else
                    {
                        if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].BlackWitchInfluence == false &
                            this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == -7)
                        {
                            RangeBlocked = true;
                        }
                    }
                }
            }

            i = pi - 3;
            j = pj + 3;
            while (i > -1 & j < this.MyGame.NumberOfRanks & RangeBlocked == false)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)i;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)j;
                }
                else
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].WhiteWitchInfluence == false &
                        this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == 7)
                    {
                        RangeBlocked = true;
                    }
                    else
                    {
                        if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].BlackWitchInfluence == false &
                            this.MyGame.MyPosition[pPositionNumber].MySquare[pi, pj].PieceTypeColour == -7)
                        {
                            RangeBlocked = true;
                        }
                    }
                }
                i--;
                j++;
            }

            if (pi > 0 & pj > 1)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi - 1, pj - 2].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)(pi - 1);
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)(pj - 2);
                }
            }
            if (pi > 0 & pj < MyGame.NumberOfRanks - 2)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi - 1, pj + 2].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)(pi - 1);
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)(pj + 2);
                }
            }
            if (pi < this.MyGame.NumberOfFiles - 1 & pj > 1)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi + 1, pj - 2].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)(pi + 1);
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)(pj - 2);
                }
            }
            if (pi < this.MyGame.NumberOfFiles - 1 & pj < MyGame.NumberOfRanks - 2)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi + 1, pj + 2].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)(pi + 1);
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)(pj + 2);
                }
            }
            if (pi > 1 & pj > 0)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi - 2, pj - 1].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)(pi - 2);
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)(pj - 1);
                }
            }
            if (pi > 1 & pj < MyGame.NumberOfRanks - 1)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi - 2, pj + 1].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)(pi - 2);
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)(pj + 1);
                }
            }
            if (pi < this.MyGame.NumberOfFiles - 2 & pj > 0)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi + 2, pj - 1].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)(pi + 2);
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)(pj - 1);
                }
            }
            if (pi < this.MyGame.NumberOfFiles - 2 & pj < MyGame.NumberOfRanks - 1)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[pi + 2, pj + 1].PieceTypeColour == 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves++;
                    nm = this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves - 1;

                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_i = pi;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].from_j = pj;
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_i = (byte)(pi + 2);
                    this.MyGame.MyPosition[pPositionNumber].MovesFromHere[nm].to_j = (byte)(pj + 1);
                }
            }

        }

        //Here the section ENDS that generates a list of moves from a given position

        //Start of the section that enriches the position


            private void Enrich_Initialize(int pPositionNumber)
        {
            int i1;
            int j1;

            this.MyGame.MyPosition[pPositionNumber].WhiteIsInCheck = false;
            this.MyGame.MyPosition[pPositionNumber].BlackIsInCheck = false;
            this.MyGame.MyPosition[pPositionNumber].AdvancePawnPotential = 0;
            this.MyGame.MyPosition[pPositionNumber].Distance_OpponentsKing_balance = 0;
            this.MyGame.MyPosition[pPositionNumber].BlackCloseToWhiteKingScore = 0;
            this.MyGame.MyPosition[pPositionNumber].WhiteCloseToBlackKingScore = 0;
            this.MyGame.MyPosition[pPositionNumber].CastleWhiteRightBlockedTemp = false;
            this.MyGame.MyPosition[pPositionNumber].CastleWhiteLeftBlockedTemp = false;
            this.MyGame.MyPosition[pPositionNumber].CastleBlackRightBlockedTemp = false;
            this.MyGame.MyPosition[pPositionNumber].CastleBlackLeftBlockedTemp = false;
            this.MyGame.MyPosition[pPositionNumber].WhiteHasWitch = false;
            this.MyGame.MyPosition[pPositionNumber].BlackHasWitch = false;
            this.MyGame.MyPosition[pPositionNumber].WhiteKingLoci = 0;
            this.MyGame.MyPosition[pPositionNumber].WhiteKingLocj = 0;
            this.MyGame.MyPosition[pPositionNumber].BlackKingLoci = 0;
            this.MyGame.MyPosition[pPositionNumber].BlackKingLocj = 0;

            for (i1 = 0; i1 < this.MyGame.NumberOfFiles; i1++)
            {
                for (j1 = 0; j1 < this.MyGame.NumberOfRanks; j1++)
                {
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i1, j1].WhiteWitchInfluence = false;
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i1, j1].BlackWitchInfluence = false;
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i1, j1].ExtraWhiteWitchInfluence = false;
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i1, j1].ExtraBlackWitchInfluence = false;
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i1, j1].IsAttackedByWhite = false;
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i1, j1].IsAttackedByBlack = false;
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i1, j1].IsCloseToWhiteKing = false;
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i1, j1].IsCloseToBlackKing = false;
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i1, j1].AdvancePawnPotentialCounted = false;
                }
            }
        }

        private void Enrich_CastleWhiteLeftBlockedTemp(int pPositionNumber)
        {
            int i;
            int j;

            j = 0;

            //If castling is already blocked, stop evaluating this
            if (this.MyGame.MyPosition[pPositionNumber].CastleWhiteLeftBlockedTemp == true |
                this.MyGame.MyPosition[pPositionNumber].CastleWhiteLeftBlockedPerm == true)
            {
                return;
            }

            //All squares between King and Rook vacant, or there is a transparent piece
            for (i = MyGame.MyPosition[pPositionNumber].WhiteKingLoci - 1; i > 0; i--)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].WhiteWitchInfluence != true &
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour != 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].CastleWhiteLeftBlockedTemp = true;
                    return;
                }
            }

            //The squares where the King and Rook end should be really vacant
            i = MyGame.MyPosition[pPositionNumber].WhiteKingLoci - this.MyGame.CastleDistance;
            if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour != 0 |
                this.MyGame.MyPosition[pPositionNumber].MySquare[i + 1, j].PieceTypeColour != 0)
            {
                this.MyGame.MyPosition[pPositionNumber].CastleWhiteLeftBlockedTemp = true;
                return;
            }

            //All squares from where the King starts to where the King ends should not be attacked
            for (i = MyGame.MyPosition[pPositionNumber].WhiteKingLoci;
                 i >= MyGame.MyPosition[pPositionNumber].WhiteKingLoci - this.MyGame.CastleDistance; i--)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].IsAttackedByBlack)
                {
                    this.MyGame.MyPosition[pPositionNumber].CastleWhiteLeftBlockedTemp = true;
                    return;
                }
            }

        }
        private void Enrich_CastleWhiteRightBlockedTemp(int pPositionNumber)
        {
            int i;
            int j;

            j = 0;

            //If castling is already blocked, stop evaluating this
            if (this.MyGame.MyPosition[pPositionNumber].CastleWhiteRightBlockedTemp == true |
                this.MyGame.MyPosition[pPositionNumber].CastleWhiteRightBlockedPerm == true)
            {
                return;
            }

            //All squares between King and Rook vacant, or there is a transparent piece
            for (i = MyGame.MyPosition[pPositionNumber].WhiteKingLoci + 1; i < this.MyGame.NumberOfFiles - 1; i++)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].WhiteWitchInfluence != true &
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour != 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].CastleWhiteRightBlockedTemp = true;
                    return;
                }
            }

            //The squares where the King and Rook end should be really vacant
            i = MyGame.MyPosition[pPositionNumber].WhiteKingLoci + this.MyGame.CastleDistance;
            if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour != 0 |
                this.MyGame.MyPosition[pPositionNumber].MySquare[i - 1, j].PieceTypeColour != 0)
            {
                this.MyGame.MyPosition[pPositionNumber].CastleWhiteRightBlockedTemp = true;
                return;
            }

            //All squares from where the King starts to where the King ends should not be attacked
            for (i = MyGame.MyPosition[pPositionNumber].WhiteKingLoci;
                 i <= MyGame.MyPosition[pPositionNumber].WhiteKingLoci + this.MyGame.CastleDistance; i++)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].IsAttackedByBlack)
                {
                    this.MyGame.MyPosition[pPositionNumber].CastleWhiteRightBlockedTemp = true;
                    return;
                }
            }

        }

        private void Enrich_CastleBlackLeftBlockedTemp(int pPositionNumber)
        {
            int i;
            int j;

            j = this.MyGame.NumberOfRanks - 1;

            //If castling is already blocked, stop evaluating this
            if (this.MyGame.MyPosition[pPositionNumber].CastleBlackLeftBlockedTemp == true |
                this.MyGame.MyPosition[pPositionNumber].CastleBlackLeftBlockedPerm == true)
            {
                return;
            }

            //All squares between King and Rook vacant, or there is a transparent piece
            for (i = MyGame.MyPosition[pPositionNumber].BlackKingLoci - 1; i > 0; i--)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].BlackWitchInfluence != true &
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour != 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].CastleBlackLeftBlockedTemp = true;
                    return;
                }
            }

            //The squares where the King and Rook end should be really vacant
            i = MyGame.MyPosition[pPositionNumber].BlackKingLoci - this.MyGame.CastleDistance;
            if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour != 0 |
                this.MyGame.MyPosition[pPositionNumber].MySquare[i + 1, j].PieceTypeColour != 0)
            {
                this.MyGame.MyPosition[pPositionNumber].CastleBlackLeftBlockedTemp = true;
                return;
            }

            //All squares from where the King starts to where the King ends should not be attacked
            for (i = MyGame.MyPosition[pPositionNumber].BlackKingLoci;
                 i >= MyGame.MyPosition[pPositionNumber].BlackKingLoci - this.MyGame.CastleDistance; i--)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].IsAttackedByWhite)
                {
                    this.MyGame.MyPosition[pPositionNumber].CastleBlackLeftBlockedTemp = true;
                    return;
                }
            }

        }
        private void Enrich_CastleBlackRightBlockedTemp(int pPositionNumber)
        {
            int i;
            int j;

            j = this.MyGame.NumberOfRanks - 1;

            //If castling is already blocked, stop evaluating this
            if (this.MyGame.MyPosition[pPositionNumber].CastleBlackRightBlockedTemp == true |
                this.MyGame.MyPosition[pPositionNumber].CastleBlackRightBlockedPerm == true)
            {
                return;
            }

            //All squares between King and Rook vacant, or there is a transparent piece
            for (i = MyGame.MyPosition[pPositionNumber].BlackKingLoci + 1; i < this.MyGame.NumberOfFiles - 1; i++)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].BlackWitchInfluence != true &
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour != 0)
                {
                    this.MyGame.MyPosition[pPositionNumber].CastleBlackRightBlockedTemp = true;
                    return;
                }
            }

            //The squares where the King and Rook end should be really vacant
            i = MyGame.MyPosition[pPositionNumber].BlackKingLoci + this.MyGame.CastleDistance;
            if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour != 0 |
                this.MyGame.MyPosition[pPositionNumber].MySquare[i - 1, j].PieceTypeColour != 0)
            {
                this.MyGame.MyPosition[pPositionNumber].CastleBlackRightBlockedTemp = true;
                return;
            }

            //All squares from where the King starts to where the King ends should not be attacked
            for (i = MyGame.MyPosition[pPositionNumber].BlackKingLoci;
                 i <= MyGame.MyPosition[pPositionNumber].BlackKingLoci + this.MyGame.CastleDistance; i++)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].IsAttackedByWhite)
                {
                    this.MyGame.MyPosition[pPositionNumber].CastleBlackRightBlockedTemp = true;
                    return;
                }
            }

        }
        private void Enrich_IsInCheck(int pPositionNumber)
        {
            byte i1;
            byte j1;

            i1 = this.MyGame.MyPosition[pPositionNumber].WhiteKingLoci;
            j1 = this.MyGame.MyPosition[pPositionNumber].WhiteKingLocj;

            if (this.MyGame.MyPosition[pPositionNumber].MySquare[i1, j1].IsAttackedByBlack == true)
            {
                this.MyGame.MyPosition[pPositionNumber].WhiteIsInCheck = true;
            }

            i1 = this.MyGame.MyPosition[pPositionNumber].BlackKingLoci;
            j1 = this.MyGame.MyPosition[pPositionNumber].BlackKingLocj;

            if (this.MyGame.MyPosition[pPositionNumber].MySquare[i1, j1].IsAttackedByWhite == true)
            {
                this.MyGame.MyPosition[pPositionNumber].BlackIsInCheck = true;
            }
        }

        private void EnrichIsCloseToKing(int pPositionNumber)
        {
            int i1;
            int j1;
            int i2;
            int j2;

            i1 = this.MyGame.MyPosition[pPositionNumber].WhiteKingLoci;
            j1 = this.MyGame.MyPosition[pPositionNumber].WhiteKingLocj;

            for (i2 = Math.Max(0, i1 - 1); i2 < Math.Min(this.MyGame.NumberOfFiles, i1 + 2); i2++)
            {
                for (j2 = Math.Max(0, j1 - 1); j2 < Math.Min(this.MyGame.NumberOfRanks, j1 + 2); j2++)
                {
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i2, j2].IsCloseToWhiteKing = true;
                }
            }
            i1 = this.MyGame.MyPosition[pPositionNumber].BlackKingLoci;
            j1 = this.MyGame.MyPosition[pPositionNumber].BlackKingLocj;

            for (i2 = Math.Max(0, i1 - 1); i2 < Math.Min(this.MyGame.NumberOfFiles, i1 + 2); i2++)
            {
                for (j2 = Math.Max(0, j1 - 1); j2 < Math.Min(this.MyGame.NumberOfRanks, j1 + 2); j2++)
                {
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i2, j2].IsCloseToBlackKing = true;
                }
            }
        }
        private void EnrichWitchInfluence(int pPositionNumber)
        {
            int i1;
            int j1;
            int i2;
            int j2;

            for (i1=0;i1 < this.MyGame.NumberOfFiles;i1++)
            {
                for (j1=0;j1 < this.MyGame.NumberOfRanks;j1++)
                {
                    if(this.MyGame.MyPosition[pPositionNumber].MySquare[i1, j1].PieceTypeColour == 7)
                    {
                        this.MyGame.MyPosition[pPositionNumber].WhiteHasWitch = true;
                        for (i2 = Math.Max(0, i1 - 1); i2 < Math.Min(this.MyGame.NumberOfFiles, i1 + 2); i2++)
                        {
                            for (j2 = Math.Max(0, j1 - 1); j2 < Math.Min(this.MyGame.NumberOfRanks, j1 + 2); j2++)
                            {
                                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i2, j2].WhiteWitchInfluence == true)
                                {
                                    this.MyGame.MyPosition[pPositionNumber].MySquare[i2, j2].ExtraWhiteWitchInfluence = true;
                                } else
                                {
                                    this.MyGame.MyPosition[pPositionNumber].MySquare[i2, j2].WhiteWitchInfluence = true;
                                }
                            }
                        }
                    }
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i1, j1].PieceTypeColour == -7)
                    {
                        this.MyGame.MyPosition[pPositionNumber].BlackHasWitch = true;
                        for (i2 = Math.Max(0, i1 - 1); i2 < Math.Min(this.MyGame.NumberOfFiles, i1 + 2); i2++)
                        {
                            for (j2 = Math.Max(0, j1 - 1); j2 < Math.Min(this.MyGame.NumberOfRanks, j1 + 2); j2++)
                            {
                                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i2, j2].BlackWitchInfluence == true)
                                {
                                    this.MyGame.MyPosition[pPositionNumber].MySquare[i2, j2].ExtraBlackWitchInfluence = true;
                                }
                                else
                                {
                                    this.MyGame.MyPosition[pPositionNumber].MySquare[i2, j2].BlackWitchInfluence = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        //End of the section that enriches the position


        public bool ValidateImportedGame()
        {
            int p;

            if (this.MyGame.NumberOfFiles < 8 | this.MyGame.NumberOfFiles > 12 |
                this.MyGame.NumberOfRanks < 8 | this.MyGame.NumberOfRanks > 12)
            {
                MessageBox.Show("Board size must be from 8 to 12");
                return false;
            }

            if (this.MyGame.CastleDistance < 2 | this.MyGame.CastleDistance > 3)
            {
                MessageBox.Show("Castle distance must be from 2 to 3");
                return false;
            }

            if (this.MyGame.NumberOfPositionsInGame > MaxNumberOfPositions - 10)
            {
                MessageBox.Show("Game length cannot be handled");
                return false;
            }

            for (p = 0; p < MyGame.NumberOfPositionsInGame; p++)
            {
                if (ValidateImportedPosition(p) == false)
                {
                    return false;
                }
            }

            return true;
        }

        private bool ValidateImportedPosition(int pPositionNumber)
        {
            int i;
            int j;
            bool eptarget;
            int eptargetcount;
            int WhiteKingcount;
            int BlackKingcount;

            if (this.MyGame.MyPosition[pPositionNumber].ColourToMove != -1 &
                this.MyGame.MyPosition[pPositionNumber].ColourToMove != 1)
            {
                MessageBox.Show("Valid colour to move must be specified");
                return false;
            }

            WhiteKingcount = 0;
            BlackKingcount = 0;

            for (i = 0; i < this.MyGame.NumberOfFiles; i++)
            {
                for (j = 0; j < this.MyGame.NumberOfRanks; j++)
                {
                    if (i == 0) 
                    {
                        this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].EnPassantLeftAllowed = false; 
                    }
                    if (i == this.MyGame.NumberOfFiles - 1) 
                    {
                        this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].EnPassantRightAllowed = false; 
                    }
                    if (i > 0)
                    {
                        if (this.MyGame.MyPosition[pPositionNumber].MySquare[i - 1, j].PieceTypeColour *
                                   this.MyGame.MyPosition[pPositionNumber].ColourToMove != -8)
                        {
                            this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].EnPassantLeftAllowed = false;
                        }
                    }
                    if (i < this.MyGame.NumberOfFiles - 1)
                    {
                        if (this.MyGame.MyPosition[pPositionNumber].MySquare[i + 1, j].PieceTypeColour *
                                   this.MyGame.MyPosition[pPositionNumber].ColourToMove != -8)
                        {
                            this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].EnPassantRightAllowed = false;
                        }
                    }
                    if (this.MyGame.MyPosition[pPositionNumber].ColourToMove == -1)
                    {
                        if (j != 3 | this.MyGame.MyPosition[pPositionNumber].MySquare[i,j].PieceTypeColour != -8) 
                        { 
                            this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].EnPassantLeftAllowed = false;
                            this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].EnPassantRightAllowed = false;
                        }
                    }
                    else
                    {
                        if (j != MyGame.NumberOfRanks - 4 | this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour != 8)
                        {
                            this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].EnPassantLeftAllowed = false;
                            this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].EnPassantRightAllowed = false;
                        }

                    }
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 1)
                    {
                        WhiteKingcount ++;
                        if (i * 2 < this.MyGame.NumberOfFiles - 2 | i * 2 > this.MyGame.NumberOfFiles | j > 0)
                        {
                            this.MyGame.MyPosition[pPositionNumber].CastleWhiteRightBlockedPerm = true;
                            this.MyGame.MyPosition[pPositionNumber].CastleWhiteLeftBlockedPerm = true;
                        }
                    }
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == -1)
                    {
                        BlackKingcount++;
                        if (i * 2 < this.MyGame.NumberOfFiles - 2 | i * 2 > this.MyGame.NumberOfFiles | j < this.MyGame.NumberOfRanks - 1)
                        {
                            this.MyGame.MyPosition[pPositionNumber].CastleBlackRightBlockedPerm = true;
                            this.MyGame.MyPosition[pPositionNumber].CastleBlackLeftBlockedPerm = true;
                        }
                    }
                }
            }

            if (WhiteKingcount != 1 | BlackKingcount != 1)
            {
                MessageBox.Show("Position does not have exactly one White and exactly one Black King");
                return false;
            }
            if (this.MyGame.MyPosition[pPositionNumber].MySquare[0, 0].PieceTypeColour != 3)
            {
                this.MyGame.MyPosition[pPositionNumber].CastleWhiteLeftBlockedPerm = true;
            }
            if (this.MyGame.MyPosition[pPositionNumber].MySquare[this.MyGame.NumberOfFiles - 1, 0].PieceTypeColour != 3)
            {
                this.MyGame.MyPosition[pPositionNumber].CastleWhiteRightBlockedPerm = true;
            }
            if (this.MyGame.MyPosition[pPositionNumber].MySquare[0, this.MyGame.NumberOfRanks - 1].PieceTypeColour != -3)
            {
                this.MyGame.MyPosition[pPositionNumber].CastleBlackLeftBlockedPerm = true;
            }
            if (this.MyGame.MyPosition[pPositionNumber].MySquare[this.MyGame.NumberOfFiles - 1, this.MyGame.NumberOfRanks - 1].PieceTypeColour != -3)
            {
                this.MyGame.MyPosition[pPositionNumber].CastleBlackRightBlockedPerm = true;
            }

            eptargetcount = 0;
            for (i = 0; i < this.MyGame.NumberOfFiles; i++)
            {
                eptarget = false;
                if (i > 0)
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i - 1, 3].EnPassantRightAllowed)
                    {
                        eptarget = true;
                    }
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i - 1, MyGame.NumberOfRanks - 4].EnPassantRightAllowed)
                    {
                        eptarget = true;
                    }
                }
                if (i < this.MyGame.NumberOfFiles - 1)
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i + 1, 3].EnPassantLeftAllowed)
                    {
                        eptarget = true;
                    }
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i + 1, MyGame.NumberOfRanks - 4].EnPassantLeftAllowed)
                    {
                        eptarget = true;
                    }
                }
                if (eptarget == true)
                {
                    eptargetcount++;
                }
            }
            if (eptargetcount > 1)
            {
                for (i = 0; i < this.MyGame.NumberOfFiles; i++)
                {
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, 3].EnPassantLeftAllowed = false;
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, 3].EnPassantRightAllowed = false;
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, MyGame.NumberOfRanks - 4].EnPassantLeftAllowed = false;
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, MyGame.NumberOfRanks - 4].EnPassantRightAllowed = false;
                }
                MessageBox.Show("Inconsistent en passant information detected and discarded");
            }

            for (i = 0; i < this.MyGame.NumberOfFiles; i++)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, 0].PieceTypeColour == 8 |
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, 0].PieceTypeColour == -8 |
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, this.MyGame.NumberOfRanks - 1].PieceTypeColour == 8 |
                    this.MyGame.MyPosition[pPositionNumber].MySquare[i, this.MyGame.NumberOfRanks - 1].PieceTypeColour == -8 )
                {
                    MessageBox.Show("Pawn illegaly placed on first or last rank");
                    return false;
                }
            }
            return true;
        }


        public bool ValidateEnteredMove(int pPositionNumber)
        {
            byte i;
            byte j;
            int mn;
            bool MatchFound;
            bool MustPromote;
            this.MyGame.EnteredMoveIdentifiedidx = -1;

            i = this.MyGame.EnteredMove.from_i;
            j = this.MyGame.EnteredMove.from_j;

            MustPromote = false;

            if (i >= this.MyGame.NumberOfFiles | j >= this.MyGame.NumberOfRanks)
            {
                MessageBox.Show("Move coordinates are exceeding the board size.");
                return false;
            }

            if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour *
                     this.MyGame.MyPosition[pPositionNumber].ColourToMove < 0)
            {
                MessageBox.Show("Piece of the wrong colour is being moved.");
                return false;
            }
            if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 0)
            {
                MessageBox.Show("The specified SquareFrom is vacant.");
                return false;
            }
            if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == -8 &
                       j == 1)
            {
                MustPromote = true;
                if (this.MyGame.EnteredMove.PromoteToPiece > 0)
                {
                    this.MyGame.EnteredMove.PromoteToPiece = (sbyte)(this.MyGame.EnteredMove.PromoteToPiece * (-1));
                    MessageBox.Show("Promotion piece of the correct colour has been substituted.");
                }
            }
            if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour == 8 &
                       j == this.MyGame.NumberOfRanks - 2)
            {
                MustPromote = true;
                if (this.MyGame.EnteredMove.PromoteToPiece < 0)
                {
                    this.MyGame.EnteredMove.PromoteToPiece = (sbyte)(this.MyGame.EnteredMove.PromoteToPiece * (-1));
                    MessageBox.Show("Promotion piece of the correct colour has been substituted.");
                }
            }
            if ((this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour != -8 |
                       j != 1) &
                       this.MyGame.MyPosition[pPositionNumber].ColourToMove == -1)
            {
                this.MyGame.EnteredMove.PromoteToPiece = 0;
            }

            if ((this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour != 8 |
                       j != this.MyGame.NumberOfRanks - 2) &
                       this.MyGame.MyPosition[pPositionNumber].ColourToMove == 1)
            {
                this.MyGame.EnteredMove.PromoteToPiece = 0;
            }



            this.ListMovesAndEnrich(pPositionNumber, 0);
            mn = 0;
            MatchFound = false;
            while (mn < this.MyGame.MyPosition[pPositionNumber].NumberOfFoundMoves & MatchFound == false)
            {
                if (this.MyGame.MyPosition[pPositionNumber].MovesFromHere[mn].from_i == this.MyGame.EnteredMove.from_i &
                      this.MyGame.MyPosition[pPositionNumber].MovesFromHere[mn].from_j == this.MyGame.EnteredMove.from_j &
                      this.MyGame.MyPosition[pPositionNumber].MovesFromHere[mn].to_i == this.MyGame.EnteredMove.to_i &
                      this.MyGame.MyPosition[pPositionNumber].MovesFromHere[mn].to_j == this.MyGame.EnteredMove.to_j &
                      (this.MyGame.MyPosition[pPositionNumber].MovesFromHere[mn].PromoteToPiece == 
                           this.MyGame.EnteredMove.PromoteToPiece | MustPromote == false))
                {
                    MatchFound = true;
                    this.MyGame.EnteredMoveIdentifiedidx = mn;
                }
                mn++;
            }
            if (MatchFound == false)
            {
                MessageBox.Show("Move does not match any of the generated legal moves.");
                return false;
            }

            this.MyGame.StatusMessage = EnteredMoveAsString(this.MyGame.EnteredMove) + " successfully validated";
            return true;
        }


        //Start of the section that deals with text and XML representation


        private string MoveAsString (Position pPosition, int mn)
        {
            string s;
            byte from_i;
            byte from_j;
            byte to_i;
            byte to_j;
            sbyte MovingPiece;

            from_i = pPosition.MovesFromHere[mn].from_i;
            from_j = pPosition.MovesFromHere[mn].from_j;
            to_i = pPosition.MovesFromHere[mn].to_i;
            to_j = pPosition.MovesFromHere[mn].to_j;
            MovingPiece = pPosition.MySquare[from_i, from_j].PieceTypeColour;

            s = PieceTypeColourAsString(MovingPiece) + Square_i_jAsText(from_i, from_j) + "-" + Square_i_jAsText(to_i, to_j);
            if (MovingPiece == 8 & to_j == this.MyGame.NumberOfRanks - 1)
            {
                s = s + PieceTypeColourAsString(pPosition.MovesFromHere[mn].PromoteToPiece);
            }
            if (MovingPiece == -8 & to_j == 0)
            {
                s = s + PieceTypeColourAsString(pPosition.MovesFromHere[mn].PromoteToPiece);
            }
            return s;
        }

        private string EnteredMoveAsString(Move pMove)
        {
            string s;
            byte from_i;
            byte from_j;
            byte to_i;
            byte to_j;
            sbyte MovingPiece;

            from_i = pMove.from_i;
            from_j = pMove.from_j;
            to_i = pMove.to_i;
            to_j = pMove.to_j;

            MovingPiece = this.MyGame.MyPosition[this.MyGame.ActualCurrentPositionidx].MySquare[from_i, from_j].PieceTypeColour;

            s = PieceTypeColourAsString(MovingPiece) + Square_i_jAsText(from_i, from_j) + "-" + Square_i_jAsText(to_i, to_j);
            if (MovingPiece == 8 & to_j == this.MyGame.NumberOfRanks - 1)
            {
                s = s + PieceTypeColourAsString(pMove.PromoteToPiece);
            }
            if (MovingPiece == -8 & to_j == 0)
            {
                s = s + PieceTypeColourAsString(pMove.PromoteToPiece);
            }
            return s;
        }

        private string PieceTypeColourAsString (sbyte PPieceTypeColour)
        {
            string s;
            //Numbers are hard-coded reserved for pieces
            //0 Vacant
            //1 King K
            //2 Queen Q
            //3 Rook R
            //4 Knight N
            //5 Bishop B
            //6 Guard G
            //7 Witch W
            //8 Pawn p
            //Positive number for White piece
            //Corresponding negative number for Black piece
            s = ".";
            switch (PPieceTypeColour)
            {
                case 0: s = ".";break;
                case 1: s = "K"; break;
                case 2: s = "Q"; break;
                case 3: s = "R"; break;
                case 4: s = "N"; break;
                case 5: s = "B"; break;
                case 6: s = "G"; break;
                case 7: s = "W"; break;
                case 8: s = "p"; break;
                case -1: s = "-K"; break;
                case -2: s = "-Q"; break;
                case -3: s = "-R"; break;
                case -4: s = "-N"; break;
                case -5: s = "-B"; break;
                case -6: s = "-G"; break;
                case -7: s = "-W"; break;
                case -8: s = "-p"; break;
            }

            return s;

        }

        private sbyte StringAsPieceTypeColour(string PString)
        {
            switch (PString.ToUpper())
            {
                case ".": return 0;
                case "K": return 1;
                case "Q": return 2;
                case "R": return 3;
                case "N": return 4;
                case "B": return 5;
                case "G": return 6;
                case "W": return 7;
                case "P": return 8;
                case "-K": return -1;
                case "-Q": return -2;
                case "-R": return -3;
                case "-N": return -4;
                case "-B": return -5;
                case "-G": return -6;
                case "-W": return -7;
                case "-P": return -8;
            }

            return 0;
        }


        private string PosEvaluationResultAsString(PosEvaluationResult pPosEvaluationResult)
        {
            string s;

            s = "";

            if (pPosEvaluationResult.MeInCheck == true)
            {
                s = s + "|Illegal - own King in check";
            }
            if (pPosEvaluationResult.IsStaleMate == true)
            {
                s = s + "|Stalemate";
            }
            if (pPosEvaluationResult.IsMate == true)
            {
                s = s + "|Mate";
            }
            if (pPosEvaluationResult.IsDrawByMaterial == true)
            {
                s = s + "|Draw by insufficient material";
            }

            s = s + "|" + pPosEvaluationResult.PositionAdvantage.ToString();

            if (pPosEvaluationResult.BestMoveidx > -1)
            {
                s = s + "|" + pPosEvaluationResult.BestMoveidx.ToString();
            }

            return s;
        }

        private XElement MovesFromHereAsXElement(Position pPosition)
        {
            int i;
            string s;
            XElement AllMovesFromHere;

            AllMovesFromHere = new XElement("AllMovesFromHere");

            for (i = 0; i < pPosition.NumberOfFoundMoves; i++)
            {
                s = this.MoveAsString(pPosition, i) + "|" +
                    this.PosEvaluationResultAsString(pPosition.MovesFromHere[i].MyResult)
                    ;

                AllMovesFromHere.Add(new XElement("Move", s));
            }
            return AllMovesFromHere;
        }

        private XElement EnrichedSquaresAsXElement(Square[,] pSquare)
        {
            int i;
            int j;
            string[] s;
            string MySquarexml;
            XElement AllRanks;

            AllRanks = new XElement("EnrichedSquares");

            s = new string[this.MyGame.NumberOfRanks];


            for (i = this.MyGame.NumberOfRanks - 1; i > -1; i--)
            {
                s[i] = "";
                for (j = 0; j < this.MyGame.NumberOfFiles; j++)
                {
                    MySquarexml = "";
                    if (pSquare[j, i].IsAttackedByWhite == true)
                    {
                        MySquarexml = MySquarexml + "w";
                    }
                    if (pSquare[j, i].IsAttackedByBlack == true)
                    {
                        MySquarexml = MySquarexml + "b";
                    }
                    if (pSquare[j, i].WhiteWitchInfluence == true)
                    {
                        MySquarexml = MySquarexml + "+";
                    }
                    if (pSquare[j, i].BlackWitchInfluence == true)
                    {
                        MySquarexml = MySquarexml + "-";
                    }
                    if (pSquare[j, i].ExtraWhiteWitchInfluence == true)
                    {
                        MySquarexml = MySquarexml + "+";
                    }
                    if (pSquare[j, i].ExtraBlackWitchInfluence == true)
                    {
                        MySquarexml = MySquarexml + "-";
                    }
                    if (pSquare[j, i].IsCloseToWhiteKing == true)
                    {
                        MySquarexml = MySquarexml + "(";
                    }
                    if (pSquare[j, i].IsCloseToBlackKing == true)
                    {
                        MySquarexml = MySquarexml + ")";
                    }

                    while (MySquarexml.Length < 2)
                    {
                        MySquarexml = " " + MySquarexml;
                    }
                    
                    
                    s[i] = s[i] + MySquarexml;
                    if (j < this.MyGame.NumberOfFiles - 1)
                    {
                        s[i] = s[i] + "|";
                    }
                }
                AllRanks.Add(new XElement("EnrichedRank", s[i]));
            }
            return AllRanks;
        }

        private XElement SquaresAsXElement(Square[,] pSquare)
        {
            //White's last rank is the first line in the xml, so a person can read the xml easier in an editor
            //A leading asterisk indicates en passant left (left/right = from White's perspective)
            //A trailing asterisk indicates en passant right
            int i;
            int j;
            string[] s;
            string MySquarexml;
            XElement AllRanks;

            AllRanks = new XElement("Squares");

            s = new string[this.MyGame.NumberOfRanks];


            for (i = this.MyGame.NumberOfRanks - 1; i > -1 ; i--)
                {
                s[i] = "";
                for (j = 0;j < this.MyGame.NumberOfFiles;j++)
                {
                    MySquarexml = PieceTypeColourAsString(pSquare[j, i].PieceTypeColour);
                    if (pSquare[j, i].EnPassantLeftAllowed == true) { MySquarexml = "*" + MySquarexml; }
                    if (pSquare[j, i].EnPassantRightAllowed == true) { MySquarexml = MySquarexml + '*'; }

                    if (MySquarexml.Length < 2)
                    {
                        MySquarexml = " " + MySquarexml;
                    }

                    s[i] = s[i] + MySquarexml;
                    if (j < this.MyGame.NumberOfFiles - 1)
                    {
                        s[i] = s[i] + "|";
                    }
                }
                AllRanks.Add(new XElement("Rank", s[i]));
            }
            return AllRanks;
        }

        private void GetSquaresFromXML(XElement parElement, int pPositionNumber)
        {
            //White's last rank is the first line in the xml, so a person can read the xml easier in an editor
            //A leading asterisk indicates en passant left (left/right = from White's perspective)
            //A trailing asterisk indicates en passant right
            IEnumerable<XElement> ElementsNum = parElement.Elements();
            string strMyValue;
            int i;
            int j;
            string s;

            j = this.MyGame.NumberOfRanks - 1;
            foreach (XElement elem in ElementsNum)
            {
                if (elem.Name == "Rank")
                {
                    strMyValue = elem.Value;

                    string[] MySquares = strMyValue.Split('|');
                    for (i = 0; i < MySquares.Length; i++)
                    {
                        s = MySquares[i].Trim(' ');

                        if (s == "")
                        {
                            s = ".";
                        }

                        if (s.Substring(0, 1) == "*")
                        {
                            this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].EnPassantLeftAllowed = true;
                            s = s.Substring(1);
                        }
                        if(s.Substring(s.Length - 1, 1) == "*")
                        {
                            this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].EnPassantRightAllowed = true;
                            s = s.Substring(0, s.Length - 1);
                        }
                        this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour = StringAsPieceTypeColour(s);
                    }
                    j = j - 1;

                }
            }

        }

        private XElement EnrichedPositionAsXElement(Position pPosition)
        {
            XElement XEPosition;

            XEPosition = new XElement("EnrichedPosition");

            if (pPosition.CastleWhiteRightBlockedTemp == true)
            { XEPosition.Add(new XElement("CastleWhiteRightBlockedTemp", "true")); }
            if (pPosition.CastleWhiteLeftBlockedTemp == true)
            { XEPosition.Add(new XElement("CastleWhiteLeftBlockedTemp", "true")); }
            if (pPosition.CastleBlackRightBlockedTemp == true)
            { XEPosition.Add(new XElement("CastleBlackRightBlockedTemp", "true")); }
            if (pPosition.CastleBlackLeftBlockedTemp == true)
            { XEPosition.Add(new XElement("CastleBlackLeftBlockedTemp", "true")); }
            if (pPosition.WhiteIsInCheck == true)
            { XEPosition.Add(new XElement("WhiteIsInCheck", "true")); }
            if (pPosition.BlackIsInCheck == true)
            { XEPosition.Add(new XElement("BlackIsInCheck", "true")); }
            XEPosition.Add(new XElement("AdvancePawnPotential", pPosition.AdvancePawnPotential.ToString()));
            XEPosition.Add(new XElement("Distance_OpponentsKing_balance", pPosition.Distance_OpponentsKing_balance.ToString()));
            XEPosition.Add(new XElement("WhiteCloseToBlackKingScore", pPosition.WhiteCloseToBlackKingScore.ToString()));
            XEPosition.Add(new XElement("BlackCloseToWhiteKingScore", pPosition.BlackCloseToWhiteKingScore.ToString()));
            if (pPosition.WhiteHasWitch == true)
            { XEPosition.Add(new XElement("WhiteHasWitch", "true")); }
            if (pPosition.BlackHasWitch == true)
            { XEPosition.Add(new XElement("BlackHasWitch", "true")); }

            return XEPosition;
        }
        private XElement PositionAsXElement(Position pPosition)
        {
            XElement XEPosition;

            XEPosition = new XElement("Position");
            XEPosition.Add(SquaresAsXElement(pPosition.MySquare));

            XEPosition.Add(EnrichedSquaresAsXElement(pPosition.MySquare));
            XEPosition.Add(MovesFromHereAsXElement(pPosition));

            if (pPosition.ColourToMove == 1)
            { XEPosition.Add(new XElement("ColourToMove", "w")); }
            if (pPosition.ColourToMove == -1)
            { XEPosition.Add(new XElement("ColourToMove", "b")); }
            if (pPosition.CastleWhiteRightBlockedPerm == true)
            { XEPosition.Add(new XElement("CastleWhiteRightBlockedPerm", "true")); }
            if (pPosition.CastleWhiteLeftBlockedPerm == true)
            { XEPosition.Add(new XElement("CastleWhiteLeftBlockedPerm", "true")); }
            if (pPosition.CastleBlackRightBlockedPerm == true)
            { XEPosition.Add(new XElement("CastleBlackRightBlockedPerm", "true")); }
            if (pPosition.CastleBlackLeftBlockedPerm == true)
            { XEPosition.Add(new XElement("CastleBlackLeftBlockedPerm", "true")); }

            return XEPosition;
        }
        private void GetPositionFromXML(XElement parElement, int pPositionNumber)
        {
            IEnumerable<XElement> ElementsNum = parElement.Elements();
         

            foreach (XElement elem in ElementsNum)
            {
                if (elem.Name == "Squares")
                {
                    this.GetSquaresFromXML(elem, pPositionNumber);
                }
                if (elem.Name == "ColourToMove")
                {
                    if (elem.Value.ToLower() == "w") { this.MyGame.MyPosition[pPositionNumber].ColourToMove = 1; }
                    else { this.MyGame.MyPosition[pPositionNumber].ColourToMove = -1;  }
                }
                if (elem.Name == "CastleWhiteRightBlockedPerm")
                {
                    if (elem.Value == "true") { this.MyGame.MyPosition[pPositionNumber].CastleWhiteRightBlockedPerm = true; }
                }
                if (elem.Name == "CastleWhiteLeftBlockedPerm")
                {
                    if (elem.Value == "true") { this.MyGame.MyPosition[pPositionNumber].CastleWhiteLeftBlockedPerm = true; }
                }
                if (elem.Name == "CastleBlackRightBlockedPerm")
                {
                    if (elem.Value == "true") { this.MyGame.MyPosition[pPositionNumber].CastleBlackRightBlockedPerm = true; }
                }
                if (elem.Name == "CastleBlackLeftBlockedPerm")
                {
                    if (elem.Value == "true") { this.MyGame.MyPosition[pPositionNumber].CastleBlackLeftBlockedPerm = true; }
                }
            }

        }
        public XElement GameAsXElement(Game pGame)
        {
            XElement XEGame;
            int i;
            XEGame = new XElement("Game");
            XEGame.Add(new XElement("NumberOfFiles", pGame.NumberOfFiles.ToString()));
            XEGame.Add(new XElement("NumberOfRanks", pGame.NumberOfRanks.ToString()));
            XEGame.Add(new XElement("NumberOfPositionsInGame", pGame.NumberOfPositionsInGame.ToString()));
            XEGame.Add(new XElement("CastleDistance", pGame.CastleDistance.ToString()));

            for (i=0;i < pGame.NumberOfPositionsInGame;i++)
            { 
                XEGame.Add(PositionAsXElement(pGame.MyPosition[i]));
                XEGame.Add(EnrichedPositionAsXElement(pGame.MyPosition[i]));
            }

            this.MyGame.StatusMessage = "Export XML finished";

            return XEGame;
        }


        public void GetGameFromXML(XElement parElement)
        {
            IEnumerable<XElement> ElementsNum = parElement.Elements();
            int eNumberOfFiles;
            int eNumberOfRanks;
            int eNumberOfPositionsInGame;
            bool NumberOfFilesSet;
            bool NumberOfRanksSet;
            bool NumberOfPositionsInGameSet;

            int PositionCount;
            bool ResetGameDone;

            eNumberOfFiles = this.MyGame.NumberOfFiles;
            eNumberOfRanks = this.MyGame.NumberOfRanks;
            eNumberOfPositionsInGame = this.MyGame.NumberOfPositionsInGame;
            NumberOfFilesSet = false;
            NumberOfRanksSet = false;
            NumberOfPositionsInGameSet = false;

            PositionCount = 0;
            ResetGameDone = false;

            foreach (XElement elem in ElementsNum)
            {
                if (elem.Name == "NumberOfFiles")
                {
                    eNumberOfFiles = byte.Parse(elem.Value);
                    NumberOfFilesSet = true;
                }
                if (elem.Name == "NumberOfRanks")
                {
                    eNumberOfRanks = byte.Parse(elem.Value);
                    NumberOfRanksSet = true;
                }
                if (elem.Name == "NumberOfPositionsInGame")
                {
                    eNumberOfPositionsInGame = int.Parse(elem.Value);
                    NumberOfPositionsInGameSet = true;
                }

                if (NumberOfFilesSet == true
                        & NumberOfRanksSet == true
                        & NumberOfPositionsInGameSet == true
                        & ResetGameDone == false)
                { 
                    this.ResetGame(eNumberOfFiles, eNumberOfRanks, eNumberOfPositionsInGame);
                    ResetGameDone = true;
                }

                if (elem.Name == "CastleDistance")
                {
                    this.MyGame.CastleDistance = int.Parse(elem.Value);
                }

                if (elem.Name == "Position")
                {
                    if (PositionCount > eNumberOfPositionsInGame - 1)
                    {
                        MessageBox.Show("XML contains more positions than specified number of positions");
                    }

                    this.GetPositionFromXML(elem, PositionCount);
                    PositionCount = PositionCount + 1;
                }
            }
            this.MyGame.StatusMessage = "Import XML finished";
        }

        private byte LetterAsi(string c)
        {
            switch (c.ToLower())
            {
                case "a": return 0;
                case "b": return 1;
                case "c": return 2;
                case "d": return 3;
                case "e": return 4;
                case "f": return 5;
                case "g": return 6;
                case "h": return 7;
                case "i": return 8;
                case "j": return 9;
                case "k": return 10;
                case "l": return 11;
            }
            return 0;
        }

        public string Square_i_jAsText(int i, int j)
        {
            string s;

            s = "";

            switch(i)
            {
                case 0: s = "a"; break;
                case 1: s = "b"; break;
                case 2: s = "c"; break;
                case 3: s = "d"; break;
                case 4: s = "e"; break;
                case 5: s = "f"; break;
                case 6: s = "g"; break;
                case 7: s = "h"; break;
                case 8: s = "i"; break;
                case 9: s = "j"; break;
                case 10: s = "k"; break;
                case 11: s = "l"; break;
            }
            s = s + (j + 1).ToString();

            return s;
        }

        private string[] PositionAsBoardToolText(int pPositionNumber)
        {
            int i;
            int j;
            int l;
            int NumberOfPieces;

            string[] output;

            NumberOfPieces = 0;
            for (i = 0; i < this.MyGame.NumberOfFiles; i++)
            {
                for (j = 0; j < this.MyGame.NumberOfRanks; j++)
                {
                    if (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour != 0)
                    {
                        NumberOfPieces++;
                    }
                }
            }

            l = NumberOfPieces + 14;

            output = new string[l];

            l = 0;

            output[0] = "[Rows " + this.MyGame.NumberOfRanks.ToString() + "]";
            output[1] = "[Columns " + this.MyGame.NumberOfFiles.ToString() + "]";
            output[2] = "[SquareWidth 57]";
            output[3] = "[SquareHeight 57]";
            output[4] = "[BoardCornerSize 12]";
            output[5] = "[SquareCornerSize 8]";
            output[6] = "[LeftOffset 19]";
            output[7] = "[RightOffset 12]";
            output[8] = "[TopOffset 9]";
            output[9] = "[BottomOffset 27]";
            output[10] = "[BoardColor 841886719]";
            output[11] = "[SquareColors 1989563903 4025406207]";
            output[12] = "[NotationSize 16]";
            output[13] = "[NotationColor 2610666495]";

            l = 14;

            for (i = 0; i < this.MyGame.NumberOfFiles; i++)
            {
                for (j = 0; j < this.MyGame.NumberOfRanks; j++)
                {
                    switch (this.MyGame.MyPosition[pPositionNumber].MySquare[i, j].PieceTypeColour)
                    {
                        case 1:
                            output[l] = "[Piece " + this.Square_i_jAsText(i, j) + " bulldog king white -]";
                            l++;
                            break;
                        case -1:
                            output[l] = "[Piece " + this.Square_i_jAsText(i, j) + " bulldog king black -]";
                            l++;
                            break;
                        case 2:
                            output[l] = "[Piece " + this.Square_i_jAsText(i, j) + " bulldog queen white -]";
                            l++;
                            break;
                        case -2:
                            output[l] = "[Piece " + this.Square_i_jAsText(i, j) + " bulldog queen black -]";
                            l++;
                            break;
                        case 3:
                            output[l] = "[Piece " + this.Square_i_jAsText(i, j) + " bulldog rook white -]";
                            l++;
                            break;
                        case -3:
                            output[l] = "[Piece " + this.Square_i_jAsText(i, j) + " bulldog rook black -]";
                            l++;
                            break;
                        case 4:
                            output[l] = "[Piece " + this.Square_i_jAsText(i, j) + " bulldog knight white -]";
                            l++;
                            break;
                        case -4:
                            output[l] = "[Piece " + this.Square_i_jAsText(i, j) + " bulldog knight black -]";
                            l++;
                            break;
                        case 5:
                            output[l] = "[Piece " + this.Square_i_jAsText(i, j) + " bulldog bishop white -]";
                            l++;
                            break;
                        case -5:
                            output[l] = "[Piece " + this.Square_i_jAsText(i, j) + " bulldog bishop black -]";
                            l++;
                            break;
                        case 6:
                            output[l] = "[Piece " + this.Square_i_jAsText(i, j) + " bulldog guard white -]";
                            l++;
                            break;
                        case -6:
                            output[l] = "[Piece " + this.Square_i_jAsText(i, j) + " bulldog guard black -]";
                            l++;
                            break;
                        case 7:
                            output[l] = "[Piece " + this.Square_i_jAsText(i, j) + " bulldog witch white -]";
                            l++;
                            break;
                        case -7:
                            output[l] = "[Piece " + this.Square_i_jAsText(i, j) + " bulldog witch black -]";
                            l++;
                            break;
                        case 8:
                            output[l] = "[Piece " + this.Square_i_jAsText(i, j) + " bulldog pawn white -]";
                            l++;
                            break;
                        case -8:
                            output[l] = "[Piece " + this.Square_i_jAsText(i, j) + " bulldog pawn black -]";
                            l++;
                            break;
                    }
                }
            }

            this.MyGame.StatusMessage = "Export to format Board painter tool created";
            return output;
        }

        public string[] LastPositionAsBoardToolText()
        {
            return this.PositionAsBoardToolText(this.MyGame.NumberOfPositionsInGame - 1);
        }


        public void GetEngineSettingsFromXML(XElement parElement)
        {
            IEnumerable<XElement> ElementsNum = parElement.Elements();

            foreach (XElement elem in ElementsNum)
            {
                if (elem.Name == "FindOnly1stMate_n_line")
                {
                    if (elem.Value == "true")
                    {
                        this.FindOnly1stMate_n_line = true;
                    }
                    if (elem.Value == "false")
                    {
                        this.FindOnly1stMate_n_line = false;
                    }
                }
                if (elem.Name == "BoardFromWhitePerspective")
                {
                    if (elem.Value == "true")
                    {
                        this.BoardFromWhitePerspective = true;
                    }
                    if (elem.Value == "false")
                    {
                        this.BoardFromWhitePerspective = false;
                    }
                }
                if (elem.Name == "NumberOfPliesToCalculate")
                {
                    this.NumberOfPliesToCalculate = byte.Parse(elem.Value);
                }
            }
            this.MyGame.StatusMessage = "Settings reloaded";
        }

        public Move GetMoveFromXML(XElement parElement)
        {
            Move outMove;
            string c1;
            string c2;

            IEnumerable<XElement> ElementsNum = parElement.Elements();

            outMove.from_i = 0;
            outMove.from_j = 0;
            outMove.to_i = 0;
            outMove.to_j = 0;
            outMove.PromoteToPiece = 0;
            outMove.MyResult.MeInCheck = false;
            outMove.MyResult.IsStaleMate = false;
            outMove.MyResult.IsMate = false;
            outMove.MyResult.IsDrawByMaterial = false;
            outMove.MyResult.PositionAdvantage = 0;
            outMove.MyResult.BestMoveidx = 0;

            foreach (XElement elem in ElementsNum)
            {
                if (elem.Name == "FromSquare")
                {
                    c1 = elem.Value.Substring(0, 1);
                    c2 = elem.Value.Substring(1);
                    outMove.from_i = LetterAsi(c1);
                    outMove.from_j = (byte)(int.Parse(c2) - 1);
                }
                if (elem.Name == "ToSquare")
                {
                    c1 = elem.Value.Substring(0, 1);
                    c2 = elem.Value.Substring(1);
                    outMove.to_i = LetterAsi(c1);
                    outMove.to_j = (byte)(int.Parse(c2) - 1);
                }
                if (elem.Name == "PromoteToPiece")
                {
                    outMove.PromoteToPiece = StringAsPieceTypeColour(elem.Value);
                }
                if (elem.Name == "PositionAdvantage")
                {
                    outMove.MyResult.PositionAdvantage = decimal.Parse(elem.Value);
                }
            }
            //MessageBox.Show(EnteredMoveAsString(outMove) + " successfully parsed");
            return outMove;
        }

        private XElement MoveAsXElement(Move pMove)
        {
            XElement MyMove;

            MyMove = new XElement("Move");
            MyMove.Add(new XElement("FromSquare", Square_i_jAsText(pMove.from_i, pMove.from_j)));
            MyMove.Add(new XElement("ToSquare", Square_i_jAsText(pMove.to_i, pMove.to_j)));
            MyMove.Add(new XElement("PromoteToPiece", PieceTypeColourAsString(pMove.PromoteToPiece)));
            MyMove.Add(new XElement("PositionAdvantage", pMove.MyResult.PositionAdvantage.ToString()));

            return MyMove;
        }

        //End of the section that deals with text and XML representation

    }
}
