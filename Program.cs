using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Shax
{
    static class Program
    {
        /// <summary>
        /// The number of the turn, starting from 0.
        /// </summary>
        public static int turnNo;

        /// <summary>
        /// Pieces left for each player.
        /// </summary>
        public static int[] pieces = new int[] {0, 5, 5};

        /// <summary>
        /// Pieces left for each players.
        /// </summary>
        public static int[] piecesLeft = new int[] {0, 0, 0};           //ilosc ruchow kazdego gracza 

        /// <summary>
        /// The tag of the selected piece. A tag contains {x, y, z, owner}.
        /// </summary>
        public static int[] selectedTag = null;

        /// <summary>
        /// `true` if there is a piece scheduled to be removed, `false` otherwise.
        /// </summary>
        public static int hasToRemove;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
