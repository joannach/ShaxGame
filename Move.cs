using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shax
{
    public class Move
    {
        public int turnNo = 0;

        public int player = 0;

        public int x, y, z;

        public enum Type { Zbicie, Przesuniecie, Polozenie};

        public Type type;

        public int x_docelowy, y_docelowy, z_docelowy;

        public Move() { }

        public Move(Type t, int x, int y, int z, int player)            //polozenie i zbcie 
        {
            type = t;
            this.x = x;
            this.y = y;
            this.z = z;
            this.player = player;
        }

        public Move(Type t, int x, int y, int z, int player, int x_docelowy, int y_docelowy, int z_docelowy)
        {
            type = t;
            this.x = x;
            this.y = y;
            this.z = z;
            this.player = player;
            this.x_docelowy = x_docelowy;
            this.y_docelowy = y_docelowy;
            this.z_docelowy = z_docelowy;

        }
    }
}
