using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Swed64;

namespace DDNetExternalClient
{
    internal class MovementRecord
    {

        private Swed mem;
        private IntPtr localPlayerPointer;
        private int frequency;

        private List<int> replayFire = new List<int>();
        private List<int> replayHook = new List<int>();
        private List<int> replayRight = new List<int>();
        private List<int> replayLeft = new List<int>();
        private List<int> replayJump = new List<int>();
        private List<Vector2> replayAim = new List<Vector2>();

        public Vector2 posStartRec;

        public Player localPlayer;

        public bool recorded = false;
        private bool record = false;
        private bool play = false;

        public MovementRecord(Swed mem, IntPtr localPlayerPointer, int frequency, Label recordText) {

            this.mem = mem;
            this.localPlayerPointer = localPlayerPointer;
            this.frequency = frequency;

            Record();

            new Thread(() =>
            {

                while (true)
                {

                    if (Overlay.GetAsyncKeyState(Keys.X) < 0 && !record) //START RECORD
                    {
                        replayFire.Clear();
                        replayHook.Clear();
                        replayRight.Clear();
                        replayLeft.Clear();
                        replayJump.Clear();
                        replayAim.Clear();

                        record = true;
                        posStartRec = localPlayer.pos;
                    }

                    if (Overlay.GetAsyncKeyState(Keys.Z) < 0 && record) //STOP RECORD
                    {
                        record = false;
                        recorded = true;
                    }

                    ////////////////////////////////////////////////////////////

                    recordText.Text = record.ToString() + "\n" + play.ToString() + "\n" + Overlay.GetAsyncKeyState(Keys.F).ToString();

                    Thread.Sleep(10);

                }

            }).Start();
        }

        public void Record()
        {

            new Thread(() =>
            {

                while (true)
                {

                    if (record)
                    {
                        replayRight.Add(mem.ReadInt(localPlayerPointer, Offsets.playerMoveD));
                        replayLeft.Add(mem.ReadInt(localPlayerPointer, Offsets.playerMoveS));
                        replayJump.Add(mem.ReadInt(localPlayerPointer, Offsets.playerJump));
                        replayFire.Add(mem.ReadInt(localPlayerPointer, Offsets.playerFire));
                        replayHook.Add(mem.ReadInt(localPlayerPointer, Offsets.playerHook));
                        replayAim.Add(new Vector2(mem.ReadInt(localPlayerPointer, Offsets.playerAimX), mem.ReadInt(localPlayerPointer, Offsets.playerAimY)));

                    }

                    Thread.Sleep(frequency);

                }

            }).Start();
        }

        public void Play()
        {

            if (!record)
            {
                play = true;

                for (int i = 0; i < replayFire.Count; i++)
                {
                    mem.WriteInt(localPlayerPointer, Offsets.playerFire, replayFire[i]);
                    mem.WriteInt(localPlayerPointer, Offsets.playerHook, replayHook[i]);
                    mem.WriteInt(localPlayerPointer, Offsets.playerMoveD, replayRight[i]);
                    mem.WriteInt(localPlayerPointer, Offsets.playerMoveS, replayLeft[i]);
                    mem.WriteInt(localPlayerPointer, Offsets.playerJump, replayJump[i]);
                    mem.WriteFloat(localPlayerPointer, Offsets.playerAimX, replayAim[i].X);
                    mem.WriteFloat(localPlayerPointer, Offsets.playerAimY, replayAim[i].Y);

                    Thread.Sleep(frequency);
                }

                play = false;
            }

        }

        public Rectangle rect(Form1 screen)
        {
            return new Rectangle
            {
                Location = new Point((screen.Width / 2 + (int)(posStartRec.X - localPlayer.pos.X)) - 32, (screen.Height / 2 + (int)(posStartRec.Y - localPlayer.pos.Y)) - 32),//(int)pos.X, (int)pos.Y),
                Size = new Size(65, 65)
            };
        }
    }
}
