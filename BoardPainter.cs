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
        public bool BoardFromWhitePerspective;
        public string resourcefolder;
        
        public readonly WeirdEngineMoveFinder MyWeirdEngineMoveFinder;
        public readonly PictureBox PictureBox_Board;

        public BoardPainter(WeirdEngineMoveFinder pWeirdEngineMoveFinder, PictureBox pPictureBox_Board)
        {
            this.BoardFromWhitePerspective = true;
            this.MyWeirdEngineMoveFinder = pWeirdEngineMoveFinder;
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
        }

        private string BmpFileNameFromPieceTypeColour(int psquare)
        {
            if (psquare == 0)
            {
                return "vacant";
            }
            int pti = MyWeirdEngineMoveFinder.pieceTypeIndex(psquare);
            string myname = MyWeirdEngineMoveFinder.piecetypes[pti].name;

            if (psquare < 0)
            {
                return "black" + myname.ToLower();
            }
            if (psquare > 0)
            {
                return "white" + myname.ToLower();
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
            string s;

            Font MyFont;
            Point MyPoint;

            i_b = 0;
            j_b = 0;

            this.PictureBox_Board.Width = (SquareWidthHeight * this.MyWeirdEngineMoveFinder.positionstack[0].boardwidth)
                + BoardEdgeWidthHeightLeft + BoardEdgeWidthHeightRight;
            this.PictureBox_Board.Height = (SquareWidthHeight * this.MyWeirdEngineMoveFinder.positionstack[0].boardheight)
                + BoardEdgeWidthHeightBottom + BoardEdgeWidthHeightTop;

            for (i = 0; i < this.MyWeirdEngineMoveFinder.positionstack[0].boardwidth; i++)
            {
                for (j = 0; j < this.MyWeirdEngineMoveFinder.positionstack[0].boardheight; j++)
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
                        + BmpFileNameFromPieceTypeColour(this.MyWeirdEngineMoveFinder.positionstack[0].squares[i, j])
                        + colourpart + ".jpg";

                    draw_x = BoardEdgeWidthHeightLeft + (i * SquareWidthHeight);
                    draw_y = (((this.MyWeirdEngineMoveFinder.positionstack[0].boardheight - 1) - j) * SquareWidthHeight) + BoardEdgeWidthHeight;

                    if (BoardFromWhitePerspective == false)
                    {

                        i_b = (this.MyWeirdEngineMoveFinder.positionstack[0].boardwidth - 1) - i;
                        j_b = (this.MyWeirdEngineMoveFinder.positionstack[0].boardheight - 1) - j;


                        draw_x = BoardEdgeWidthHeightLeft + (i_b * SquareWidthHeight);
                        draw_y = (((this.MyWeirdEngineMoveFinder.positionstack[0].boardheight - 1) - j_b) * SquareWidthHeight) + BoardEdgeWidthHeight;
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
            for (i = 0; i < this.MyWeirdEngineMoveFinder.positionstack[0].boardwidth; i++)
            {

                if (BoardFromWhitePerspective == false)
                {
                    i_b = (this.MyWeirdEngineMoveFinder.positionstack[0].boardwidth - 1) - i;
                }
                else
                {
                    i_b = i;
                }

                draw_x = (int)(BoardEdgeWidthHeightLeft * 0.75) + (SquareWidthHeight / 2) + (i_b * SquareWidthHeight);
                draw_y = (this.MyWeirdEngineMoveFinder.positionstack[0].boardheight * SquareWidthHeight) + BoardEdgeWidthHeightTop + 1;
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

            for (j = 0; j < this.MyWeirdEngineMoveFinder.positionstack[0].boardheight; j++)
            {

                if (BoardFromWhitePerspective == false)
                {
                    j_b = (this.MyWeirdEngineMoveFinder.positionstack[0].boardheight - 1) - j;
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
                    + (((this.MyWeirdEngineMoveFinder.positionstack[0].boardheight - 1) - j_b) * SquareWidthHeight);
                MyPoint = new Point(draw_x, draw_y);
                s = (j + 1).ToString();
                g.DrawString(s, MyFont, Brushes.White, MyPoint);
            }
        }

        public string DisplayInformation()
        {
            string s;

            s = "";

            if (this.MyWeirdEngineMoveFinder.positionstack[0].colourtomove == 1)
            {
                s = s + "White to move\n";
            }
            else
            {
                s = s + "Black to move\n";
            }

            s = s + "presort_when_n_plies_gt " + MyWeirdEngineMoveFinder.presort_when_n_plies_gt.ToString() + "\n";
            s = s + "SearchForFastestMate " + MyWeirdEngineMoveFinder.setting_SearchForFastestMate.ToString() + "\n";
            s = s + "presort_using_n_plies " + MyWeirdEngineMoveFinder.presort_using_n_plies.ToString() + "\n";
            s = s + "display_when_n_plies_gt " + MyWeirdEngineMoveFinder.display_when_n_plies_gt.ToString() + "\n";
            s = s + "jsonsourcepath " + MyWeirdEngineMoveFinder.MyWeirdEngineJson.jsonsourcepath + "\n";
            s = s + "jsonworkpath " + MyWeirdEngineMoveFinder.MyWeirdEngineJson.jsonworkpath + "\n";

            return s;
        }

    }
}
