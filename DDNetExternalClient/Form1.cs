using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Numerics;
using System.Windows.Forms;
using Swed64;

namespace DDNetExternalClient
{
    public partial class Form1 : Form
    {

        private Overlay overlay;

        private int r = 0, g = 255, b = 0;
        private List<Player> players = new List<Player>();
        private Swed mem;
        private IntPtr moduleBase;
        private IntPtr playersPointer;
        private IntPtr localPlayerPointer;
        private Player localPlayer;
        private Panel rainbowPanel;

        private bool stabilize = false;

        private bool _dragging;
        private Point _startPos;

        MovementRecord recorder;

        private double testA;
        private double testB;

        private void Aim()
        {

            //Player? nearest = (Player?)GetNearestToPlayer();
            //Player? nearest = (Player?)GetNearestToPlayer();//(100);
            //Player? nearest = (Player?)GetNearestToMouse(50);//(100);
            Player? nearest = (Player?)GetNearestToPlayer();//(100);

            if (nearest is not null)
            {
                
                mem.WriteFloat(localPlayerPointer, Offsets.playerAimX, nearest.pos.X - localPlayer.pos.X);
                mem.WriteFloat(localPlayerPointer, Offsets.playerAimY, (nearest.pos.Y - localPlayer.pos.Y));
                

            }


        }

        private void BetterHook()
        {

            //Player? nearest = (Player?)GetNearestToPlayer();
            //Player? nearest = (Player?)GetNearestToMouse(50);
            Player? nearest = (Player?)GetNearestInSector(45, 400);

            if (nearest is not null)
            {
                mem.WriteFloat(localPlayerPointer, Offsets.playerAimX, nearest.pos.X - localPlayer.pos.X);
                mem.WriteFloat(localPlayerPointer, Offsets.playerAimY, nearest.pos.Y - localPlayer.pos.Y);
                mem.WriteInt(localPlayerPointer, Offsets.playerHook, 1);

            }

        }

        private Player? GetNearestInSector(double a, int r)
        {
            a = a * (Math.PI / 180);

            int myId = mem.ReadInt(playersPointer, Offsets.localplayerId);

            double closestDist = double.MaxValue;
            Player closestPlayer = null;

            var aAim = Math.Atan2(mem.ReadFloat(localPlayerPointer, Offsets.playerAimY), mem.ReadFloat(localPlayerPointer, Offsets.playerAimX));

            for (int i = 0; i < players.Count; i++)
            {
                Player player = players[i];

                if (player.id != myId && player.gametick != 0 && player.dist < r)
                {
                    var aPlayer = Math.Atan2(player.pos.Y - localPlayer.pos.Y, player.pos.X - localPlayer.pos.X);

                    if (aPlayer > aAim - (a / 2) && aPlayer < aAim + (a / 2))
                    {
                       
                        if (player.dist < closestDist)
                        {
                            closestDist = player.dist;
                            closestPlayer = player;
                        }
                    }
                }
            }

            return closestPlayer;
        }


        private Player? GetNearestToMouse(float minDist)
        {
            int myId = mem.ReadInt(playersPointer, Offsets.localplayerId);

            double closestDist = double.MaxValue;
            Player closestPlayer = null;

            for (int i = 0; i < players.Count; i++)
            {
                Player player = players[i];

                if (player.id != myId && player.gametick != 0)
                {

                    if (player.dist > 0 && player.dist < closestDist)
                    {
                        closestDist = player.dist;
                        closestPlayer = player;
                    }
                }
            }
            
            return closestPlayer;
        }


        private Player? GetNearestToPlayer()
        {
            int myId = mem.ReadInt(playersPointer, Offsets.localplayerId);

            double closestDist = double.MaxValue;
            Player closestPlayer = null;

            for (int i = 0; i < players.Count; i++)
            {
                Player player = players[i];

                if (player.id != myId && player.gametick != 0)
                {

                    float dist = Vector2.Distance(localPlayer.pos, player.pos);

                    if (player.dist > 0 && player.dist < closestDist)
                    {
                        closestDist = player.dist;
                        closestPlayer = player;
                    }
                }
            }

            return closestPlayer;
        }

        private void UpdateLocalPlayer()
        {
            int myId = mem.ReadInt(playersPointer, Offsets.localplayerId);

            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].id == myId)
                {
                    localPlayer = players[i];
                    break;
                }
            }

        }

        private void UpdatePlayers()
        {
            players.Clear();

            int playerOnlineN = mem.ReadInt(playersPointer, Offsets.onlinePlayersNum);
            int myId = mem.ReadInt(playersPointer, Offsets.localplayerId);

            if (playerOnlineN > 1)
            {
                for (int i = 0; i < 63; i++)
                {
                    int gametick = mem.ReadInt(playersPointer, Offsets.players + i * 0xF8);

                    float playerX = mem.ReadFloat(playersPointer, Offsets.playerX + i * 0xF8);
                    float playerY = mem.ReadFloat(playersPointer, Offsets.playerY + i * 0xF8);
                    int playerHp = mem.ReadInt(playersPointer, Offsets.localPlayerHp + i * 0xF8);
                    float dist = -1;
                    if (localPlayer != null)
                        dist = Vector2.Distance(new Vector2(playerX, playerY), localPlayer.pos);

                    players.Add(new Player(gametick, myId == i ? myId : i, new Vector2(playerX, playerY), playerHp, dist));

                }
            }

        }

        private void Stabileze()
        {

            Player? nearest = (Player?)GetNearestToPlayer();

            if (nearest is not null)
            {
                if (localPlayer.pos.X < nearest.pos.X)
                {
                    mem.WriteInt(localPlayerPointer, Offsets.playerMoveS, 0);
                    mem.WriteInt(localPlayerPointer, Offsets.playerMoveD, 1);
                }
                else if(localPlayer.pos.X > nearest.pos.X)
                {
                    mem.WriteInt(localPlayerPointer, Offsets.playerMoveD, 0);
                    mem.WriteInt(localPlayerPointer, Offsets.playerMoveS, 1);
                }

            }
        }

        private bool IsHooked()
        {
            return mem.ReadInt(localPlayerPointer, Offsets.playerHook) == 0 ? false : true;
        }

        private void Hook()
        {
            mem.WriteInt(localPlayerPointer, Offsets.playerHook, 1);
        }

        private void UnHook()
        {
            mem.WriteInt(localPlayerPointer, Offsets.playerHook, 0);
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            CheckForIllegalCrossThreadCalls = false;
            this.BackColor = Color.Wheat;
            this.TransparencyKey = Color.Wheat;
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;

            string WindowName = "DDNet Client";

            Overlay overlay = new Overlay();

            if (!overlay.ProcessIsFullScreen(WindowName))
            {
                overlay.SetInvi(this);
                overlay.StartLoop(6, WindowName, this);
            }

            backgroundWorker1.RunWorkerAsync();

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            /*
            try
            {
                foreach( var pl in players)
                {
                    if (pl.gametick != 0)
                    {
                        g.DrawRectangle(new Pen(Color.Red, 3), pl.rect(localPlayer, this));
                        g.DrawLine(new Pen(Color.Blue, 2), new Point(this.Size.Width/2,this.Size.Height/2), new Point(this.Width/2 + (int)(pl.pos.X - localPlayer.pos.X), this.Height/2 + (int)(pl.pos.Y - localPlayer.pos.Y)));//posizione player

                    }
                }
            }
            catch { }*/

            int centerX = this.Width / 2;
            int centerY = this.Height / 2;

            int radius = 400;

            double startAngle = (testA - (testA - (0.706858 / 2) / 2));
            double endAngle = (testA + (testA - (0.706858 / 2) / 2));

            g.DrawPie(
                Pens.Red,
                centerX - radius,
                centerY - radius,
                radius * 2,
                radius * 2,
                (float)(startAngle * (180 / Math.PI)),
                (float)((endAngle - startAngle) * (180 / Math.PI))
                );


            if (recorder != null && recorder.recorded)
                g.DrawRectangle(new Pen(Color.Red, 3), recorder.rect(this));

        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {

            mem = new Swed("DDNet");
            moduleBase = mem.GetModuleBase(".exe");

            playersPointer = mem.ReadPointer(moduleBase, Offsets.playersBase);
            localPlayerPointer = mem.ReadPointer(moduleBase, Offsets.playerBase);

            rainbowPanel = new Panel();
            rainbowPanel.BackColor = Color.Green;
            rainbowPanel.Size = new Size(panelMain1.Size.Width + 5, panelMain1.Size.Height + 5);

            /*new Thread(() =>
            {

                while (true)
                {

                    Aim();

                    if (!IsHooked())
                    {
                        Hook();
                        Thread.Sleep(1500);
                        UnHook();
                    }

                    Thread.Sleep(1);
                }

            }).Start();*/

            while (true)
            {

                UpdatePlayers();
                UpdateLocalPlayer();

                panel1.Size = new Size(this.Size.Width, this.Size.Height);
                panel1.Refresh();

                if (localPlayer is not null)
                {
                    if(recorder == null)
                    {
                        //recorder = new MovementRecord(mem, localPlayerPointer, 1, label2, localPlayer);
                    }

                    if (Overlay.GetAsyncKeyState(Keys.R) < 0) //START PLAYING
                    {
                        //recorder.Play();
                    }

                    /*if (Overlay.GetAsyncKeyState(Keys.E) < 0) //STOP PLAYING
                    {
                        
                    }*/

                    if (Overlay.GetAsyncKeyState(Keys.LButton) != 0)
                    {
                        //Aim();
                    }

                    if (Overlay.GetAsyncKeyState(Keys.RButton) != 0)
                    {
                        BetterHook();
                    }

                    mem.WriteInt(playersPointer, mem.ReadInt(playersPointer, Offsets.players + (localPlayer.id * 0xF8)) - 999 * 2 / 40);
                    mem.WriteInt(playersPointer, mem.ReadInt(playersPointer, Offsets.players + (localPlayer.id * 0xF8)) + (mem.ReadInt(playersPointer, Offsets.players + (localPlayer.id * 0xF8)) % 2));

                    if (Overlay.GetAsyncKeyState(Keys.MButton) != 0)
                    {
                        Stabileze();

                        stabilize = false;

                    }else if (!stabilize)
                    {
                        stabilize = true;
                        mem.WriteInt(localPlayerPointer, Offsets.playerMoveD, 0);
                        mem.WriteInt(localPlayerPointer, Offsets.playerMoveS, 0);
                    }

                    var stringTest = "";

                    for (int i = 0; i < players.Count; i++)
                    {
                        Player player = players[i];

                        if(player.gametick != 0)
                            stringTest += "id: " + i + ") " + player.gametick + " - X, Y: (" + Convert.ToInt32(player.pos.X) + ", " + Convert.ToInt32(player.pos.Y) + ")" + (i != localPlayer.id ? " - Distance: " + player.dist : "") + (i == localPlayer.id ? " - Hp: " + player.hp + "<-- me"  : "") + "\n";
                    }

                    label1.Text = "My id: " + localPlayer.id + "\n\nX Velocity: "+ mem.ReadFloat(playersPointer, Offsets.playerSpeedX) + "\n\nPlayers: \n" + stringTest + "\n\n" + mem.ReadFloat(localPlayerPointer, Offsets.playerAimXWorld) + ", " + mem.ReadFloat(localPlayerPointer, Offsets.playerAimYWorld) + "\n\n" + testA + " - " + testB ;

                }


                rainbowPanel.Location = panelMain1.Location;

                Thread.Sleep(10);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(rainbowPanel is not null)
            {
                rainbowPanel.BackColor = Color.FromArgb(r, g, b);

                if (r > 0 && b == 0)
                {
                    r--;
                    g++;
                }

                if (g > 0 && r == 0)
                {
                    g--;
                    b++;
                }

                if (b > 0 && g == 0)
                {
                    b--;
                    r++;
                }
            }

        }
    }
}