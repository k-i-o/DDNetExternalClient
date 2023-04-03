using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DDNetExternalClient
{
    internal class Player
    {
        public Vector2 pos;
        public int gametick;
        public int id;
        public float dist;
        public int hp;

        public Player(int gametick, int id, Vector2 pos, int hp, float dist = -1)
        {
            this.gametick = gametick;
            this.id = id;
            this.pos = pos;
            this.hp = hp;
            this.dist = dist;
        }

        public Rectangle rect(Player localPlayer, Form1 screen)
        {

            return new Rectangle
            {
                Location = new Point((screen.Width/2 + (int)(pos.X - localPlayer.pos.X))-32, (screen.Height/2 + (int)(pos.Y - localPlayer.pos.Y))-32),//(int)pos.X, (int)pos.Y),
                Size = new Size(65, 65)
            };
        }
    }
}
