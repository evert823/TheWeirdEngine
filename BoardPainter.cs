using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace TheWeirdEngine
{
    public class BoardPainter
    {
        public const int SquareWidthHeight = 57;
        public const int BoardEdgeWidthHeight = 6;
        public const int BoardEdgeWidthHeightTop = 6;
        public const int BoardEdgeWidthHeightBottom = 24;
        public const int BoardEdgeWidthHeightLeft = 22;
        public const int BoardEdgeWidthHeightRight = 6;
        public string resourcefolder;
        
        public readonly WeirdEngineBackend MyWeirdEngineBackend;
        public readonly PictureBox PictureBox_Board;

        public BoardPainter(WeirdEngineBackend pWeirdEngineBackend, PictureBox pPictureBox_Board)
        {
            this.MyWeirdEngineBackend = pWeirdEngineBackend;
            this.PictureBox_Board = pPictureBox_Board;
        }

        public void Paint_PictureBox_Board(PaintEventArgs e)
        {
            Graphics g;
            g = e.Graphics;

            DrawBoard(g);
        }


        public void SaveBoardAsImage(string pFileName)
        {
            Bitmap MyBitmap;
            Image g;

            MyBitmap = new Bitmap(this.PictureBox_Board.Width, this.PictureBox_Board.Height);
            PictureBox_Board.DrawToBitmap(MyBitmap, PictureBox_Board.ClientRectangle);
            g = MyBitmap;
            g.Save(pFileName, System.Drawing.Imaging.ImageFormat.Png);
            this.MyWeirdEngineBackend.MyGame.StatusMessage = "png file created";
        }

        private string BmpFileNameFromPieceTypeColour(sbyte pPieceTypeColour)
        {
            switch (pPieceTypeColour)
            {
                case 0: return "vacant";
                case 1: return "whiteking";
                case 2: return "whitequeen";
                case 3: return "whiterook";
                case 4: return "whiteknight";
                case 5: return "whitebishop";
                case 6: return "whiteguard";
                case 7: return "whitewitch";
                case 8: return "whitepawn";
                case -1: return "blackking";
                case -2: return "blackqueen";
                case -3: return "blackrook";
                case -4: return "blackknight";
                case -5: return "blackbishop";
                case -6: return "blackguard";
                case -7: return "blackwitch";
                case -8: return "blackpawn";
            }
            return "";
        }
        private void DrawBoard(Graphics g)
        {
            Bitmap MyBitmap;
            string bmp_name;

            int i;
            int j;
            int i_b;
            int j_b;
            int draw_x;
            int draw_y;
            string colourpart;
            int p;
            string s;

            Font MyFont;
            Point MyPoint;

            i_b = 0;
            j_b = 0;

            p = this.MyWeirdEngineBackend.MyGame.ActualCurrentPositionidx;

            this.PictureBox_Board.Width = (SquareWidthHeight * this.MyWeirdEngineBackend.MyGame.NumberOfFiles)
                + BoardEdgeWidthHeightLeft + BoardEdgeWidthHeightRight;
            this.PictureBox_Board.Height = (SquareWidthHeight * this.MyWeirdEngineBackend.MyGame.NumberOfRanks)
                + BoardEdgeWidthHeightBottom + BoardEdgeWidthHeightTop;

            for (i = 0; i < this.MyWeirdEngineBackend.MyGame.NumberOfFiles; i++)
            {
                for (j = 0; j < this.MyWeirdEngineBackend.MyGame.NumberOfRanks; j++)
                {
                    if ((i + j) % 2 == 0)
                    {
                        colourpart = "onwhite";
                    }
                    else
                    {
                        colourpart = "onblack";
                    }
                    bmp_name = resourcefolder
                        + BmpFileNameFromPieceTypeColour(this.MyWeirdEngineBackend.MyGame.MyPosition[p].MySquare[i, j].PieceTypeColour)
                        + colourpart + ".jpg";

                    draw_x = BoardEdgeWidthHeightLeft + (i * SquareWidthHeight);
                    draw_y = (((this.MyWeirdEngineBackend.MyGame.NumberOfRanks - 1) - j) * SquareWidthHeight) + BoardEdgeWidthHeight;

                    if (this.MyWeirdEngineBackend.BoardFromWhitePerspective == false)
                    {

                        i_b = (this.MyWeirdEngineBackend.MyGame.NumberOfFiles - 1) - i;
                        j_b = (this.MyWeirdEngineBackend.MyGame.NumberOfRanks - 1) - j;


                        draw_x = BoardEdgeWidthHeightLeft + (i_b * SquareWidthHeight);
                        draw_y = (((this.MyWeirdEngineBackend.MyGame.NumberOfRanks - 1) - j_b) * SquareWidthHeight) + BoardEdgeWidthHeight;
                    }

                    try
                    {
                        MyBitmap = new Bitmap(bmp_name);
                        g.DrawImage(MyBitmap, new RectangleF(draw_x, draw_y, MyBitmap.Width, MyBitmap.Height));
                    }
                    catch (System.Exception)
                    {
                    }
                }
            }

            MyFont = new Font("Arial", 11);
            for (i = 0; i < this.MyWeirdEngineBackend.MyGame.NumberOfFiles; i++)
            {

                if (this.MyWeirdEngineBackend.BoardFromWhitePerspective == false)
                {
                    i_b = (this.MyWeirdEngineBackend.MyGame.NumberOfFiles - 1) - i;
                }
                else
                {
                    i_b = i;
                }

                draw_x = (int)(BoardEdgeWidthHeightLeft * 0.75) + (SquareWidthHeight / 2) + (i_b * SquareWidthHeight);
                draw_y = (this.MyWeirdEngineBackend.MyGame.NumberOfRanks * SquareWidthHeight) + BoardEdgeWidthHeightTop + 1;
                MyPoint = new Point(draw_x, draw_y);
                s = "";

                switch (i)
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
                g.DrawString(s, MyFont, Brushes.White, MyPoint);
            }

            for (j = 0; j < this.MyWeirdEngineBackend.MyGame.NumberOfRanks; j++)
            {

                if (this.MyWeirdEngineBackend.BoardFromWhitePerspective == false)
                {
                    j_b = (this.MyWeirdEngineBackend.MyGame.NumberOfRanks - 1) - j;
                }
                else
                {
                    j_b = j;
                }

                draw_x = (int)(BoardEdgeWidthHeightLeft * 0.25);
                if (j > 8)
                {
                    draw_x = (int)(BoardEdgeWidthHeightLeft * 0.1);
                }
                draw_y = BoardEdgeWidthHeightTop + (int)(SquareWidthHeight * 0.4)
                    + (((this.MyWeirdEngineBackend.MyGame.NumberOfRanks - 1) - j_b) * SquareWidthHeight);
                MyPoint = new Point(draw_x, draw_y);
                s = (j + 1).ToString();
                g.DrawString(s, MyFont, Brushes.White, MyPoint);
            }
        }

        public string DisplayInformation()
        {
            string s;
            int i;
            int j;
            int p;

            s = "";

            p = this.MyWeirdEngineBackend.MyGame.ActualCurrentPositionidx;

            if (this.MyWeirdEngineBackend.MyGame.MyPosition[p].ColourToMove == 1)
            {
                s = s + "White to move\n";
            }
            else
            {
                s = s + "Black to move\n";
            }


            for (i = 0; i < this.MyWeirdEngineBackend.MyGame.NumberOfFiles; i++)
            {
                j = this.MyWeirdEngineBackend.MyGame.NumberOfRanks - 4;
                if (this.MyWeirdEngineBackend.MyGame.MyPosition[p].MySquare[i, j].EnPassantLeftAllowed == true)
                {
                    s = s + this.MyWeirdEngineBackend.Square_i_jAsText(i, j) + " e.p. left possible\n";
                }
                if (this.MyWeirdEngineBackend.MyGame.MyPosition[p].MySquare[i, j].EnPassantRightAllowed == true)
                {
                    s = s + this.MyWeirdEngineBackend.Square_i_jAsText(i, j) + " e.p. right possible\n";
                }
                j = 3;
                if (this.MyWeirdEngineBackend.MyGame.MyPosition[p].MySquare[i, j].EnPassantLeftAllowed == true)
                {
                    s = s + this.MyWeirdEngineBackend.Square_i_jAsText(i, j) + " e.p. left possible\n";
                }
                if (this.MyWeirdEngineBackend.MyGame.MyPosition[p].MySquare[i, j].EnPassantRightAllowed == true)
                {
                    s = s + this.MyWeirdEngineBackend.Square_i_jAsText(i, j) + " e.p. right possible\n";
                }
            }
            if (this.MyWeirdEngineBackend.MyGame.MyPosition[p].CastleWhiteLeftBlockedPerm == false)
            {
                s = s + "White castle left possible\n";
            }
            if (this.MyWeirdEngineBackend.MyGame.MyPosition[p].CastleWhiteRightBlockedPerm == false)
            {
                s = s + "White castle right possible\n";
            }
            if (this.MyWeirdEngineBackend.MyGame.MyPosition[p].CastleBlackLeftBlockedPerm == false)
            {
                s = s + "Black castle left possible\n";
            }
            if (this.MyWeirdEngineBackend.MyGame.MyPosition[p].CastleBlackRightBlockedPerm == false)
            {
                s = s + "Black castle right possible\n";
            }

            s = s + "Fifty moves counter " +
                   this.MyWeirdEngineBackend.MyGame.MyPosition[p].FiftyMovesRulePlyCount.ToString() + "\n";
            s = s + "Repetition counter " +
                   this.MyWeirdEngineBackend.MyGame.MyPosition[p].RepetitionCount.ToString() + "\n";
            s = s + "Castle distance " +
                   this.MyWeirdEngineBackend.MyGame.CastleDistance.ToString() + "\n";

            s = s + "FindOnly1stMate_n_line " +
                   this.MyWeirdEngineBackend.FindOnly1stMate_n_line.ToString() + "\n";

            s = s + "NumberOfPliesToCalculate " +
                   this.MyWeirdEngineBackend.NumberOfPliesToCalculate.ToString();

            return s;
        }

    }
}
