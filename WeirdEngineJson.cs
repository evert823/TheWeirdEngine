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
    //In WeirdEngineJson we implement code related to WeirdEngineMoveFinder, for
    //parsing from - and storing into Json format
    public struct jsonpiecenamelist
    {
        public string[] piecetypes;
    }
    public struct jsoncastlinginfo
    {
        public bool whitekinghasmoved;
        public bool whitekingsiderookhasmoved;
        public bool whitequeensiderookhasmoved;
        public bool blackkinghasmoved;
        public bool blackkingsiderookhasmoved;
        public bool blackqueensiderookhasmoved;
    }
    public struct jsonmovecoord
    {
        public int x_from;
        public int y_from;
        public int x_to;
        public int y_to;
    }
    public struct jsonchessposition
    {
        public int boardwidth;
        public int boardheight;
        public int colourtomove;
        public jsonmovecoord precedingmove;
        public jsoncastlinginfo castlinginfo;
        public string[] squares;
    }
    public class WeirdEngineJson
    {
        public WeirdEngineMoveFinder MyWeirdEngineMoveFinder;
        public string jsonsourcepath;
        public string jsonworkpath;
        public string logfilename;
        public WeirdEngineJson(WeirdEngineMoveFinder pWeirdEngineMoveFinder)
        {
            this.MyWeirdEngineMoveFinder = pWeirdEngineMoveFinder;
            this.SetLogfilename();
        }
        public void SetLogfilename()
        {
            string myts;
            DateTime localDate;
            localDate = DateTime.Now;
            myts = localDate.ToString("yyyy-MM-dd_HH_mm");
            this.logfilename = "WeirdEngineMoveFinder_log_" + myts + ".log";
        }
        public void writelog(string themessage)
        {
            string myts;
            DateTime localDate;
            localDate = DateTime.Now;
            myts = localDate.ToString("yyyy-MM-ddTHH:mm:ss");

            using (StreamWriter writer = new StreamWriter(this.jsonworkpath + "log\\" + this.logfilename, append: true))
            {
                writer.WriteLine(myts + " " + themessage);
                writer.Close();
            }
        }
        public string DisplayMovelist(ref chessposition pposition)
        {
            string s = "";
            for (int movei = 0;movei < pposition.movelist_totalfound;movei++)
            {
                string mvstr = ShortNotation(pposition.movelist[movei]);
                if (s == "")
                {
                    s += mvstr;
                }
                else
                {
                    s += "|" + mvstr;
                }
            }
            return s;
        }
        public int Str2PieceType(string psymbol)
        {
            int n = this.MyWeirdEngineMoveFinder.piecetypes.Length;
            for (int i = 0; i < n; i++)
            {
                if (this.MyWeirdEngineMoveFinder.piecetypes[i].symbol == psymbol)
                {
                    return i + 1;
                }
                if ("-" + this.MyWeirdEngineMoveFinder.piecetypes[i].symbol == psymbol)
                {
                    return (i + 1) * -1;
                }
            }
            return 0;
        }
        public int Str2PieceType4FEN(string psymbol)
        {
            int n = this.MyWeirdEngineMoveFinder.piecetypes.Length;
            for (int i = 0; i < n; i++)
            {
                if (this.MyWeirdEngineMoveFinder.piecetypes[i].symbol.ToUpper() == psymbol)
                {
                    return i + 1;
                }
                if (this.MyWeirdEngineMoveFinder.piecetypes[i].symbol.ToLower() == psymbol)
                {
                    return (i + 1) * -1;
                }
            }
            return 0;
        }
        public string PieceType2Str(int ptypenr)
        {
            int i;
            if (ptypenr > 0)
            {
                i = ptypenr - 1;
                return this.MyWeirdEngineMoveFinder.piecetypes[i].symbol;
            }
            if (ptypenr < 0)
            {
                i = (ptypenr * -1) - 1;
                return "-" + this.MyWeirdEngineMoveFinder.piecetypes[i].symbol;
            }
            return ".";
        }
        public string PieceType2Str4FEN(int ptypenr)
        {
            int i;
            if (ptypenr > 0)
            {
                i = ptypenr - 1;
                return this.MyWeirdEngineMoveFinder.piecetypes[i].symbol.ToUpper();
            }
            if (ptypenr < 0)
            {
                i = (ptypenr * -1) - 1;
                return this.MyWeirdEngineMoveFinder.piecetypes[i].symbol.ToLower();
            }
            return ".";
        }
        public void LoadPieceFromJson(string pFileName, int seq)
        {
            string json;
            using (StreamReader r = new StreamReader(this.jsonsourcepath + "piecedefinitions\\" + pFileName + ".json"))
            {
                json = r.ReadToEnd();
            }
            chesspiecetype a = JsonConvert.DeserializeObject<chesspiecetype>(json);

            this.MyWeirdEngineMoveFinder.piecetypes[seq].symbol = a.symbol;
            this.MyWeirdEngineMoveFinder.piecetypes[seq].name = a.name;
            this.MyWeirdEngineMoveFinder.piecetypes[seq].IsRoyal = a.IsRoyal;
            this.MyWeirdEngineMoveFinder.piecetypes[seq].IsDivergent = a.IsDivergent;
            if (a.stepleapmovevectors != null)
            {
                this.MyWeirdEngineMoveFinder.piecetypes[seq].stepleapmovevectors = new vector[a.stepleapmovevectors.Length];
                for (int i = 0; i < a.stepleapmovevectors.Length; i++)
                {
                    this.MyWeirdEngineMoveFinder.piecetypes[seq].stepleapmovevectors[i] = a.stepleapmovevectors[i];
                }
            }
            if (a.slidemovevectors != null)
            {
                this.MyWeirdEngineMoveFinder.piecetypes[seq].slidemovevectors = new vector[a.slidemovevectors.Length];
                for (int i = 0; i < a.slidemovevectors.Length; i++)
                {
                    this.MyWeirdEngineMoveFinder.piecetypes[seq].slidemovevectors[i] = a.slidemovevectors[i];
                }
            }
            if (a.stepleapcapturevectors != null)
            {
                this.MyWeirdEngineMoveFinder.piecetypes[seq].stepleapcapturevectors = new vector[a.stepleapcapturevectors.Length];
                for (int i = 0; i < a.stepleapcapturevectors.Length; i++)
                {
                    this.MyWeirdEngineMoveFinder.piecetypes[seq].stepleapcapturevectors[i] = a.stepleapcapturevectors[i];
                }
            }
            if (a.slidecapturevectors != null)
            {
                this.MyWeirdEngineMoveFinder.piecetypes[seq].slidecapturevectors = new vector[a.slidecapturevectors.Length];
                for (int i = 0; i < a.slidecapturevectors.Length; i++)
                {
                    this.MyWeirdEngineMoveFinder.piecetypes[seq].slidecapturevectors[i] = a.slidecapturevectors[i];
                }
            }
        }
        public void SavePieceAsJson(chesspiecetype pchesspiecetype, string pFileName)
        {
            string jsonString = JsonConvert.SerializeObject(pchesspiecetype, Formatting.Indented);
            using (StreamWriter writer = new StreamWriter(this.jsonworkpath + "piecedefinitions_verify\\" + pFileName + ".json"))
            {
                writer.WriteLine(jsonString);
                writer.Close();
            }
        }
        public void LoadPieceTypesFromJson(string pFileName)
        {
            string json;
            using (StreamReader r = new StreamReader(this.jsonsourcepath + "games\\" + pFileName + ".json"))
            {
                json = r.ReadToEnd();
            }
            jsonpiecenamelist piecenames = JsonConvert.DeserializeObject<jsonpiecenamelist>(json);
            int n = piecenames.piecetypes.Length;
            this.MyWeirdEngineMoveFinder.piecetypes = new chesspiecetype[n];
            for (int i = 0; i < n; i++)
            {
                this.LoadPieceFromJson(piecenames.piecetypes[i], i);
            }
        }
        public void SavePieceTypesAsJson(string pFileName)
        {
            jsonpiecenamelist mynamelist = new jsonpiecenamelist();
            int n = this.MyWeirdEngineMoveFinder.piecetypes.Length;
            mynamelist.piecetypes = new string[n];
            for (int i = 0; i < n; i++)
            {
                mynamelist.piecetypes[i] = this.MyWeirdEngineMoveFinder.piecetypes[i].name;
                this.SavePieceAsJson(this.MyWeirdEngineMoveFinder.piecetypes[i], mynamelist.piecetypes[i]);
            }
            string jsonString = JsonConvert.SerializeObject(mynamelist, Formatting.Indented);

            using (StreamWriter writer = new StreamWriter(this.jsonworkpath + "games_verify\\"  + pFileName + ".json"))
            {
                writer.WriteLine(jsonString);
                writer.Close();
            }
        }
        public void LoadPositionJson(string ppath, string pFileName)
        {
            string json;
            using (StreamReader r = new StreamReader(ppath + "\\" + pFileName + ".json"))
            {
                json = r.ReadToEnd();
            }
            jsonchessposition loadedpos = JsonConvert.DeserializeObject<jsonchessposition>(json);
            this.MyWeirdEngineMoveFinder.ResetBoardsize(ref this.MyWeirdEngineMoveFinder.mainposition,
                                                        loadedpos.boardwidth, loadedpos.boardheight);
            this.MyWeirdEngineMoveFinder.mainposition.colourtomove = loadedpos.colourtomove;
            this.MyWeirdEngineMoveFinder.mainposition.precedingmove = new int[4]
                { loadedpos.precedingmove.x_from, loadedpos.precedingmove.y_from,
                    loadedpos.precedingmove.x_to, loadedpos.precedingmove.y_to };
            this.MyWeirdEngineMoveFinder.mainposition.whitekinghasmoved = loadedpos.castlinginfo.whitekinghasmoved;
            this.MyWeirdEngineMoveFinder.mainposition.whitekingsiderookhasmoved = loadedpos.castlinginfo.whitekingsiderookhasmoved;
            this.MyWeirdEngineMoveFinder.mainposition.whitequeensiderookhasmoved = loadedpos.castlinginfo.whitequeensiderookhasmoved;
            this.MyWeirdEngineMoveFinder.mainposition.blackkinghasmoved = loadedpos.castlinginfo.blackkinghasmoved;
            this.MyWeirdEngineMoveFinder.mainposition.blackkingsiderookhasmoved = loadedpos.castlinginfo.blackkingsiderookhasmoved;
            this.MyWeirdEngineMoveFinder.mainposition.blackqueensiderookhasmoved = loadedpos.castlinginfo.blackqueensiderookhasmoved;
            for (int j = 0; j < loadedpos.boardheight; j++)
            {
                int rj = (loadedpos.boardheight - 1) - j;
                string[] mysymbol = loadedpos.squares[rj].Split('|');
                for (int i = 0; i < loadedpos.boardwidth; i++)
                {
                    string s = mysymbol[i].TrimStart(' ');
                    this.MyWeirdEngineMoveFinder.mainposition.squares[i, j] = this.Str2PieceType(s);
                }
            }
        }
        public void SavePositionAsJson(string pFileName)
        {
            jsonchessposition mypos = new jsonchessposition();
            mypos.boardwidth = this.MyWeirdEngineMoveFinder.mainposition.boardwidth;
            mypos.boardheight = this.MyWeirdEngineMoveFinder.mainposition.boardheight;
            mypos.squares = new string[mypos.boardheight];
            mypos.colourtomove = this.MyWeirdEngineMoveFinder.mainposition.colourtomove;
            mypos.precedingmove.x_from = this.MyWeirdEngineMoveFinder.mainposition.precedingmove[0];
            mypos.precedingmove.y_from = this.MyWeirdEngineMoveFinder.mainposition.precedingmove[1];
            mypos.precedingmove.x_to = this.MyWeirdEngineMoveFinder.mainposition.precedingmove[2];
            mypos.precedingmove.y_to = this.MyWeirdEngineMoveFinder.mainposition.precedingmove[3];
            mypos.castlinginfo.whitekinghasmoved = this.MyWeirdEngineMoveFinder.mainposition.whitekinghasmoved;
            mypos.castlinginfo.whitekingsiderookhasmoved = this.MyWeirdEngineMoveFinder.mainposition.whitekingsiderookhasmoved;
            mypos.castlinginfo.whitequeensiderookhasmoved = this.MyWeirdEngineMoveFinder.mainposition.whitequeensiderookhasmoved;
            mypos.castlinginfo.blackkinghasmoved = this.MyWeirdEngineMoveFinder.mainposition.blackkinghasmoved;
            mypos.castlinginfo.blackkingsiderookhasmoved = this.MyWeirdEngineMoveFinder.mainposition.blackkingsiderookhasmoved;
            mypos.castlinginfo.blackqueensiderookhasmoved = this.MyWeirdEngineMoveFinder.mainposition.blackqueensiderookhasmoved;

            for (int j = 0; j < mypos.boardheight; j++)
            {
                int rj = (mypos.boardheight - 1) - j;
                string myvisualrank = "";
                for (int i = 0; i < mypos.boardwidth; i++)
                {
                    string mysymbol = this.PieceType2Str(this.MyWeirdEngineMoveFinder.mainposition.squares[i, rj]);
                    while (mysymbol.Length < 2)
                    {
                        mysymbol = " " + mysymbol;
                    }
                    myvisualrank += mysymbol;
                    if (i < mypos.boardwidth - 1)
                    {
                        myvisualrank += "|";
                    }
                }
                mypos.squares[j] = myvisualrank;
            }

            string jsonString = JsonConvert.SerializeObject(mypos, Formatting.Indented);

            using (StreamWriter writer = new StreamWriter(this.jsonworkpath + "positions_verify\\" + pFileName + ".json"))
            {
                writer.WriteLine(jsonString);
                writer.Close();
            }
        }
        public void LoadPositionFromFEN(string pfen)
        {
            string[] fenparts0 = pfen.Split(' ');
            string[] fenparts = fenparts0[0].Split('/');
            this.MyWeirdEngineMoveFinder.ResetBoardsize(ref this.MyWeirdEngineMoveFinder.mainposition, 8, 8);

            if (fenparts0[1].ToLower() == "w")
            {
                this.MyWeirdEngineMoveFinder.mainposition.colourtomove = 1;
            }
            else
            {
                this.MyWeirdEngineMoveFinder.mainposition.colourtomove = -1;
            }
            this.MyWeirdEngineMoveFinder.DisableCastling(ref this.MyWeirdEngineMoveFinder.mainposition);
            this.MyWeirdEngineMoveFinder.mainposition.precedingmove = null;
            this.MyWeirdEngineMoveFinder.mainposition.precedingmove = new int[4] { -1, -1, -1, -1 };

            int vacantsquares;

            for (int j = 0; j < fenparts.Length; j++)
            {
                int rj = (this.MyWeirdEngineMoveFinder.mainposition.boardheight - 1) - j;
                string fp = fenparts[j];
                int csqi = 0;
                for (int ci = 0; ci < fp.Length; ci++)
                {
                    if (int.TryParse(fp[ci].ToString(), out vacantsquares) == true)
                    {
                        csqi += vacantsquares;
                    }
                    else
                    {
                        this.MyWeirdEngineMoveFinder.mainposition.squares[csqi, rj] = this.Str2PieceType4FEN(fp[ci].ToString());
                        csqi += 1;
                    }
                }
            }
        }
        public string PositionAsFEN()
        {
            string[] fenparts = new string[this.MyWeirdEngineMoveFinder.mainposition.boardheight];
            for (int j = 0; j < fenparts.Length; j++)
            {
                int rj = (this.MyWeirdEngineMoveFinder.mainposition.boardheight - 1) - j;
                int vacantcount = 0;
                string fenpart = "";
                for (int i = 0; i < this.MyWeirdEngineMoveFinder.mainposition.boardwidth; i++)
                {
                    if (this.MyWeirdEngineMoveFinder.mainposition.squares[i, rj] != 0)
                    {
                        if (vacantcount != 0)
                        {
                            fenpart += vacantcount.ToString();
                            vacantcount = 0;
                        }
                        string mysymbol = this.PieceType2Str4FEN(this.MyWeirdEngineMoveFinder.mainposition.squares[i, rj]);
                        fenpart += mysymbol;
                    }
                    else
                    {
                        vacantcount += 1;
                    }
                }
                if (vacantcount != 0)
                {
                    fenpart += vacantcount.ToString();
                }
                fenparts[j] = fenpart;
            }
            string fen = "";
            for (int j = 0; j < fenparts.Length; j++)
            {
                fen += fenparts[j];
                if (j < fenparts.Length - 1)
                {
                    fen += "/";
                }
            }
            if (this.MyWeirdEngineMoveFinder.mainposition.colourtomove == 1)
            {
                fen += " w";
            }
            else
            {
                fen += " b";
            }
            using (StreamWriter writer = new StreamWriter(this.jsonworkpath + "fen\\somefen.txt"))
            {
                writer.WriteLine(fen);
                writer.Close();
            }
            return fen;
        }
        public string DisplayAttacks(ref chessposition pposition)
        {
            string AttackedByPMstr = "";
            string AttackedByPOstr = "";
            for (int i = 0; i < pposition.boardwidth; i++)
                for (int j = 0; j < pposition.boardheight; j++)
                {
                    if (pposition.squareInfo[i, j].AttackedByPM == true)
                    {
                        AttackedByPMstr += "(" + i.ToString() + "," + j.ToString() + ")";
                    }
                    if (pposition.squareInfo[i, j].AttackedByPO == true)
                    {
                        AttackedByPOstr += "(" + i.ToString() + "," + j.ToString() + ")";
                    }
                }
            string myresult = "Attacked by PM : " + AttackedByPMstr + " Attacked by PO : " + AttackedByPOstr;
            return myresult;
        }
        public string ShortNotation(chessmove pmove)
        {
            if (pmove.IsCastling == true)
            {
                if (pmove.coordinates[2] == 2)
                {
                    return "0-0-0";
                }
                else
                {
                    return "0-0";
                }
            }
            string s = this.PieceType2Str(pmove.MovingPiece).Replace("-", "");
            s += this.Coord2Squarename(pmove.coordinates[0], pmove.coordinates[1]);
            if (pmove.IsCapture == true)
            {
                s += "x";
            }
            else
            {
                s += "-";
            }
            s += this.Coord2Squarename(pmove.coordinates[2], pmove.coordinates[3]);
            if (pmove.PromoteToPiece != 0)
            {
                s += this.PieceType2Str(pmove.PromoteToPiece).Replace("-", "");
            }
            if (pmove.IsEnPassant == true)
            {
                s += " e.p.";
            }
            return s;
        }
        public string Coord2Squarename(int pi, int pj)
        {
            string myalphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (pi >= 26)
            {
                return "INVALID file number";
            }
            string s = myalphabet.ToLower()[pi].ToString();
            s += (pj + 1).ToString();
            return s;
        }
    }
}
