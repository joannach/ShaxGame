using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Shax
{
    public partial class MainForm : Form
    {

        //picturebox dla kazdego pionka
        public PictureBox[,,] pieces = new PictureBox[3, 3, 3];     

        //pozycja kazdego pionka (piksele na planszy)
        public Point[,,] locations = new Point[3, 3, 3];

        public List<Move> moves = new List<Move>();     // do undo

        public List<PictureBox> pictureboxes_not_null = new List<PictureBox>();

        public MainForm()
        {
            InitializeComponent();
            UpdateToolStripStatus();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Pozycje pionkow
            // pierwszy kwadrat (brzegowy)
            locations[0, 0, 0] = new Point(12, 12);
            locations[0, 0, 1] = new Point(210, 12);
            locations[0, 0, 2] = new Point(410, 12);
            locations[0, 1, 0] = new Point(12, 210);
            locations[0, 1, 2] = new Point(410, 210);
            locations[0, 2, 0] = new Point(12, 410);
            locations[0, 2, 1] = new Point(210, 410);
            locations[0, 2, 2] = new Point(410, 410);
            // drugi kwadrat (srodkowy)
            locations[1, 0, 0] = new Point(75, 75);
            locations[1, 0, 1] = new Point(210, 75);
            locations[1, 0, 2] = new Point(340, 75);
            locations[1, 1, 0] = new Point(75, 210);
            locations[1, 1, 2] = new Point(340, 210);
            locations[1, 2, 0] = new Point(75, 340);
            locations[1, 2, 1] = new Point(210, 340);
            locations[1, 2, 2] = new Point(340, 340);
            // trzeci kwadrat (najmniejszy)
            locations[2, 0, 0] = new Point(140, 140);
            locations[2, 0, 1] = new Point(210, 140);
            locations[2, 0, 2] = new Point(275, 140);
            locations[2, 1, 0] = new Point(140, 210);
            locations[2, 1, 2] = new Point(275, 210);
            locations[2, 2, 0] = new Point(140, 275);
            locations[2, 2, 1] = new Point(210, 275);
            locations[2, 2, 2] = new Point(275, 275);

            // generowanie
            for (int i = 0; i != 3; ++i)
            {
                for (int j = 0; j != 3; ++j)
                {
                    for (int k = 0; k != 3; ++k)
                    {
                        if ((j == 1) && (k == 1))
                        {
                            continue;
                        }
                        pieces[i, j, k] = new PictureBox();
                        pieces[i, j, k].Parent = this;
                        pieces[i, j, k].BackColor = Color.Transparent;
                        pieces[i, j, k].Location = locations[i, j, k];
                        pieces[i, j, k].Size = new Size(32, 32);
                        pieces[i, j, k].Tag = new int[] { i, j, k, 0 };
                        pieces[i, j, k].Click += new EventHandler(OnPieceClick);
                    }
                }
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        if(pieces[i, j, k]!=null)
                            pictureboxes_not_null.Add(pieces[i, j, k]);
                    }
                }
            }

        }

        public void AddMove(Move move)
        {
            move.turnNo = Program.turnNo;
            moves.Add(move);
        }


        private void OnPieceClick(object o, EventArgs e)
        {
            PictureBox sender = (PictureBox)o;
            int[] tag = (int[])sender.Tag;
            int playerToMove = GetPlayerToMove();

            //if (CheckForMills(tag[0], tag[1], tag[2]) == true )
            //    Program.hasToRemove = 1;

            bool gameStateChanged = false;
            // jeden z graczy musi usunac pionek przeciwnika
            if (Program.hasToRemove != 0)
            {
                if ((GetPieceOwner(sender) != 0) && (GetPieceOwner(sender) != Program.hasToRemove))
                {
                    removePiece(tag[0], tag[1], tag[2]);
                    gameStateChanged = true;
                }
            }
            // stan - gracze umieszczaja pionki
            else if ((Program.pieces[1] > 0) || (Program.pieces[2] > 0))
            {
                if (tag[3] == 0)
                {
                    putPiece(tag[0], tag[1], tag[2]);
                    gameStateChanged = true;
                }
            }
            // stan - przesuwanie
            else if ((Program.piecesLeft[1] > 2) && (Program.piecesLeft[2] > 2))
            {
                // gracz wybiera pionek
                if (Program.selectedTag == null)
                {
                    if (tag[3] == playerToMove)
                    {
                        SetPieceStatus(sender, playerToMove, true);
                    }
                }
                // gracz wybiera punkt docelowy
                else
                {
                    PictureBox from = pieces[Program.selectedTag[0], Program.selectedTag[1], Program.selectedTag[2]];
                    int delta =
                        Math.Abs(tag[0] - Program.selectedTag[0]) +
                        Math.Abs(tag[1] - Program.selectedTag[1]) +
                        Math.Abs(tag[2] - Program.selectedTag[2]);
                    // sprawdza czy docelowy jest pusty i czy jest w zasiegu
                    if ((tag[3] == 0) && (delta == 1))
                    {
                        movePiece(Program.selectedTag[0], Program.selectedTag[1], Program.selectedTag[2], tag[0], tag[1], tag[2]);
                        gameStateChanged = true;
                    }
                    else
                    {
                        SetPieceStatus(from, Program.selectedTag[3], false);
                    }

                    // usuwa zaznaczony
                    Program.selectedTag = null;
                }
            }
            UpdateToolStripStatus();
            DoVictoryCheck();

            if (gameStateChanged && GetPlayerToMove() == 2)
            {
                Komputer();
            }

        }

        public void movePiece(int x, int y, int z, int x_docelowy, int y_docelowy, int z_docelowy)
        {
            int[] tag = (int[])pieces[x, y, z].Tag;
            SetPieceStatus(pieces[x_docelowy, y_docelowy, z_docelowy], tag[3], false);     //Program.selectedTag[3] - gracz
            SetPieceStatus(pieces[x, y, z], 0, false);

            AddMove(new Move(Shax.Move.Type.Przesuniecie, x, y, z, GetPlayerToMove(), x_docelowy, y_docelowy, z_docelowy));

            if (CheckForMills(x_docelowy, y_docelowy, z_docelowy))
            {
                Program.hasToRemove = GetPlayerToMove();
            }
            else
                ++Program.turnNo;
        }

        public void putPiece(int x, int y, int z)
        {
            SetPieceStatus(pieces[x, y, z], GetPlayerToMove(), false);
            --Program.pieces[GetPlayerToMove()];
            ++Program.piecesLeft[GetPlayerToMove()];

            AddMove(new Move(Shax.Move.Type.Polozenie, x, y, z, GetPlayerToMove()));

            if (CheckForMills(x, y, z))
            {
                Program.hasToRemove = GetPlayerToMove();
            }
            else
                ++Program.turnNo;
        }

        public void removePiece(int x, int y, int z)
        {
            SetPieceStatus(pieces[x, y, z], 0, false);
            --Program.piecesLeft[GetOpponentPlayerID(Program.hasToRemove)];
            AddMove(new Move(Shax.Move.Type.Zbicie, x, y, z, Program.hasToRemove));
            Program.hasToRemove = 0;
            ++Program.turnNo;
        }

        public void undo()
        {
            if(moves.Count>0)
            {
                Move last = moves.Last();

                if(last.type == Shax.Move.Type.Polozenie)
                {
                    SetPieceStatus(pieces[last.x, last.y, last.z], 0, false);
                    ++Program.pieces[last.player];
                    --Program.piecesLeft[last.player];
                    Program.hasToRemove = 0;
                }
                else if(last.type == Shax.Move.Type.Przesuniecie)
                {
                    SetPieceStatus(pieces[last.x_docelowy, last.y_docelowy, last.z_docelowy], 0, false);     //Program.selectedTag[3] - gracz
                    SetPieceStatus(pieces[last.x, last.y, last.z], last.player, false);
                    Program.hasToRemove = 0;
                }
                else if(last.type == Shax.Move.Type.Zbicie)
                {
                    SetPieceStatus(pieces[last.x, last.y, last.z], GetOpponentPlayerID(last.player), false);
                    ++Program.piecesLeft[GetOpponentPlayerID(last.player)];
                    Program.hasToRemove = last.player;
                }
                moves.RemoveAt(moves.Count - 1);
                Program.turnNo = last.turnNo;
            }
        }

        public void Komputer()
        {

            if(CheckWinner() == 0)
            {
                Move move = new Move();
                MinMax(2, move);
                if(move.type == Shax.Move.Type.Polozenie)
                {
                    putPiece(move.x, move.y, move.z);
                }
                else if(move.type == Shax.Move.Type.Przesuniecie)
                {
                    movePiece(move.x, move.y, move.z, move.x_docelowy, move.y_docelowy, move.z_docelowy);

                }
                else if(move.type == Shax.Move.Type.Zbicie)
                {
                    removePiece(move.x, move.y, move.z);
                }
                if (Program.hasToRemove > 0)
                {
                    Komputer();
                }
            }
            MessageBox.Show("Komputer wykonał ruch");          
        }

        public int MinMax(int glebokosc, Move move)
        {
            int najlepsza_ocena = 0;            // w forach spr czy == najlepsza_ocena - lista
            bool pierwszy_ruch = true;
            int winner = CheckWinner();



            List<Move> best_moves = null;

            if (move != null)
                best_moves = new List<Move>();

            if(glebokosc==0 || winner!=0)
            {
                if(winner == 1)
                    return -1000;
                if (winner == 2)
                    return 1000;
                return Program.piecesLeft[2] - Program.piecesLeft[1];
            }
            else
            {
                if(Program.hasToRemove !=0  )
                {
                    int opponent = GetOpponentPlayerID(Program.hasToRemove);

                    foreach(PictureBox p in pictureboxes_not_null)
                    {
                        int[] tag = (int[])p.Tag;
                        if (tag[3] == opponent)
                        {
                            removePiece(tag[0], tag[1], tag[2]);
                            int ocena = MinMax(glebokosc - 1, move);
                            undo();
                            if (pierwszy_ruch || (Program.hasToRemove == 1 && ocena < najlepsza_ocena) || (Program.hasToRemove == 2 && ocena > najlepsza_ocena))
                            {
                                najlepsza_ocena = ocena;

                                if (move != null)
                                {
                                    //move.type = Shax.Move.Type.Zbicie;
                                    //move.x = tag[0];
                                    //move.y = tag[1];
                                    //move.z = tag[2];

                                    best_moves.Clear();
                                    best_moves.Add(new Shax.Move(Shax.Move.Type.Zbicie, tag[0], tag[1], tag[2], 0));
                                }

                            }
                            else if(ocena == najlepsza_ocena)
                            {
                                move.type = Shax.Move.Type.Zbicie;
                                move.x = tag[0];
                                move.y = tag[1];
                                move.z = tag[2];
                                best_moves.Add(move);
                            }

                        }                                
                    }
                }
                else
                {
                    if((Program.pieces[1] > 0) || (Program.pieces[2] > 0))
                    {
                        foreach (PictureBox p in pictureboxes_not_null)
                        {
                            int[] tag = (int[])p.Tag;
                            if (tag[3] == 0)
                            {
                                putPiece(tag[0], tag[1], tag[2]);
                                int ocena = MinMax(glebokosc - 1, move);
                                undo();
                                if (pierwszy_ruch || (GetPlayerToMove() == 1 && ocena < najlepsza_ocena) || (GetPlayerToMove() == 2 && ocena > najlepsza_ocena))
                                {
                                    najlepsza_ocena = ocena;

                                    if (move != null)
                                    {
                                        //move.type = Shax.Move.Type.Polozenie;
                                        //move.x = tag[0];
                                        //move.y = tag[1];
                                        //move.z = tag[2];

                                        best_moves.Clear();
                                        best_moves.Add(new Shax.Move(Shax.Move.Type.Polozenie, tag[0], tag[1], tag[2], 0));

                                    }
                                    else if (ocena == najlepsza_ocena)
                                    {
                                        move.type = Shax.Move.Type.Polozenie;
                                        move.x = tag[0];
                                        move.y = tag[1];
                                        move.z = tag[2];
                                        best_moves.Add(move);
                                    }
                                }

                            }
                        }
                    }
                    else if((Program.piecesLeft[1] > 2) && (Program.piecesLeft[2] > 2))          //przesuniecie
                    {
                        foreach(PictureBox p in pictureboxes_not_null)
                        {
                            int[] tag = (int[])p.Tag;

                            if (tag[3] == GetPlayerToMove())
                            {
                                foreach(PictureBox p2 in pictureboxes_not_null)
                                {
                                    int[] tag2 = (int[])p2.Tag;
                                    if (tag2[3] == 0)
                                    {
                                        int delta =
                                            Math.Abs(tag[0] - tag2[0]) +
                                            Math.Abs(tag[1] - tag2[1]) +
                                            Math.Abs(tag[2] - tag2[2]);

                                        if (delta == 1)
                                        {
                                            movePiece(tag[0], tag[1], tag[2], tag2[0], tag2[1], tag2[2]);
                                            int ocena = MinMax(glebokosc - 1, move);
                                            undo();
                                            if (pierwszy_ruch || (GetPlayerToMove() == 1 && ocena < najlepsza_ocena) || (GetPlayerToMove() == 2 && ocena > najlepsza_ocena))
                                            {
                                                najlepsza_ocena = ocena;

                                                if (move != null)
                                                {
                                                    //move.type = Shax.Move.Type.Przesuniecie;
                                                    //move.x = tag[0];
                                                    //move.y = tag[1];
                                                    //move.z = tag[2];
                                                    //move.x_docelowy = tag2[0];
                                                    //move.y_docelowy = tag2[1];
                                                    //move.z_docelowy = tag2[2];

                                                    best_moves.Clear();
                                                    best_moves.Add(new Shax.Move(Shax.Move.Type.Przesuniecie, tag[0], tag[1], tag[2], 0, tag2[0], tag2[1], tag2[2]));

                                                }
                                                else if (ocena == najlepsza_ocena)
                                                {
                                                    move.type = Shax.Move.Type.Przesuniecie;
                                                    move.x = tag[0];
                                                    move.y = tag[1];
                                                    move.z = tag[2];
                                                    move.x_docelowy = tag2[0];
                                                    move.y_docelowy = tag2[1];
                                                    move.z_docelowy = tag2[2];
                                                    best_moves.Add(move);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (move != null)
            {
                Random r = new Random();

                Move m = best_moves[r.Next(best_moves.Count)];

                move.type = m.type;
                move.x = m.x;
                move.y = m.y;
                move.z = m.z;
                move.x_docelowy = m.x_docelowy;
                move.y_docelowy = m.y_docelowy;
                move.z_docelowy = m.z_docelowy;
            }
            return najlepsza_ocena;
        }

        private void SetPieceStatus(PictureBox p, int player, bool isSelected)      // DODAJE PIONEK NA PLANSZE 
        {
            int[] tag = (int[])p.Tag;
            if (player == 0)
            {
                p.Image = null;
                tag[3] = 0;
            }
            else
            {
                // aktualizacja obrazka
                if (isSelected)
                {
                    if (player == 1)
                    {
                        p.Image = global::Shax.Properties.Resources.selected1;
                    }
                    else
                    {
                        p.Image = global::Shax.Properties.Resources.selected2;
                    }
                    Program.selectedTag = tag;
                }
                else
                {
                    if (player == 1)
                    {
                        p.Image = global::Shax.Properties.Resources.player1;
                    }
                    else
                    {
                        p.Image = global::Shax.Properties.Resources.player2;
                    }
                    Program.selectedTag = null;
                }
                // aktualizacja gracza
                tag[3] = player;
            }
        }

        private bool CheckForMills(int x, int y, int z)
        {
            int player = GetPieceOwner(pieces[x, y, z]);
            if (player == 0)
            {
                throw new ArgumentException();
            }
            bool ok1 = true, ok2 = false, ok3 = false;
            // Between squares.
            for (int i = 0; i != 3; ++i)
            {
                if (player != GetPieceOwner(pieces[i, y, z]))
                {
                    ok1 = false;
                    break;
                }
            }
            // Horizontal.
            if (y != 1)
            {
                ok2 = true;
                for (int k = 0; k != 3; ++k)
                {
                    if (player != GetPieceOwner(pieces[x, y, k]))
                    {
                        ok2 = false;
                        break;
                    }
                }
            }
            // Vertical.
            if (z != 1)
            {
                ok3 = true;
                for (int j = 0; j != 3; ++j)
                {
                    if (player != GetPieceOwner(pieces[x, j, z]))
                    {
                        ok3 = false;
                        break;
                    }
                }
            }
            return ok1 || ok2 || ok3;
        }

        private int GetPieceOwner(PictureBox pb)
        {
            int[] tag = (int[])pb.Tag;
            return tag[3];
        }

        private void DoVictoryCheck()       //zmienic
        {
            // jeszcze nie koniec
            if ((Program.pieces[1] > 0) || (Program.pieces[2] > 0))
            {
                return;
            }
            int winner = 0;
            if (Program.piecesLeft[1] < 3)
            {
                winner = 2;
            }
            else if (Program.piecesLeft[2] < 3)
            {
                winner = 1;
            }
            if (winner != 0)
            {
                toolStripStatusLabel1.Text = GetPlayerNameByID(winner) + " wygrywa!";
                toolStripStatusLabel2.Text = "";
                MessageBox.Show(GetPlayerNameByID(winner) + " wygrywa!");
            }
        }

        private int CheckWinner()       //zmienic
        {
            // jeszcze nie zakonczona
            if ((Program.pieces[1] > 0) || (Program.pieces[2] > 0))
            {
                return 0;
            }
            if (Program.piecesLeft[1] < 3)
            {
                return 2;
            }
            else if (Program.piecesLeft[2] < 3)
            {
                return 1;
            }
            return 0;
        }

        private int GetPlayerToMove()
        {
            return (Program.turnNo % 2) + 1;
        }

        private int GetOpponentPlayerID(int playerId)
        {
            if (playerId == 1)
            {
                return 2;
            }
            else if (playerId == 2)
            {
                return 1;
            }
            throw new ArgumentException();
        }

        private string GetPlayerNameByID(int playerId)
        {
            if (playerId == 1)
            {
                return "Red";
            }
            else if (playerId == 2)
            {
                return "Blue";
            }
            throw new ArgumentException();
        }


        private void UpdateToolStripStatus()
        {
            // Label 1.
            if (Program.hasToRemove != 0)
            {
                toolStripStatusLabel1.Text = "Gracz " + Program.hasToRemove + " musi usunąć pionek przeciwnika.";
            }
            else
            {
                int playerToMove = GetPlayerToMove();
                if (Program.pieces[playerToMove] > 0)
                {
                    toolStripStatusLabel1.Text = GetPlayerNameByID(playerToMove) + " musi postawić pionek.";
                }
                else
                {
                    toolStripStatusLabel1.Text = GetPlayerNameByID(playerToMove) + " musi przesunąć pionek.";
                }
            }
            // Label 2.
            toolStripStatusLabel2.Text =
                GetPlayerNameByID(1) + ": " + Program.piecesLeft[1].ToString() + " (" + Program.pieces[1].ToString() + ")" +
                " // " +
                GetPlayerNameByID(2) + ": " + Program.piecesLeft[2].ToString() + " (" + Program.pieces[2].ToString() + ")";
        }

        private void button_undo_Click(object sender, EventArgs e)
        {
            undo();
        }
    }
}



//wrzucic pictureboxy roznw od null do tablicy i przechodzic po niej w minmax ------------- zrobione
//losowac najlepszy ruch w przyp istnienia kilku najlepszych tablica Move[] o tych samych ocenach 
//sprawdzenie zablokowanie gracza(czy moze wykonac ruch)
//SetPiece Status - jesli aktualnie komputer mysli to nie zmieniac stanu wizualnego gry 


    //dodac w kazdtm ifie w minmax obsluge listy ruchow
    //liczbe pionkow zwiekszyc
