using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PingGraph
{
    public partial class Form1 : Form
    {
        bool pingEnabled = true;
        bool tooltipEnabled = true;

        public Form1()
        {
            InitializeComponent();

            //-- DIABLED BY NF: see how delegate is used
            /*
            tooltip = new ToolTip { ReshowDelay = 10 };
            ContextMenuStrip mnu = new ContextMenuStrip { };
            this.ContextMenuStrip = mnu;
            ToolStripMenuItem mnuPause = new ToolStripMenuItem { Text = "Enable disable ping", ToolTipText = "Enable or disable pinging drawing", Checked = true };
            mnuPause.Click += delegate
            {
                pingEnabled = !pingEnabled;
                mnuPause.Checked = pingEnabled;
            };

            ToolStripMenuItem mnuToolTip = new ToolStripMenuItem { Text = "Enable Disable Tooltip", ToolTipText = "Enable or disable the tooltip over line", Checked = true };
            mnuToolTip.Click += delegate
            {
                tooltipEnabled = !tooltipEnabled;
                mnuToolTip.Checked = tooltipEnabled;

                if (!tooltipEnabled)
                {
                    //tooltip.Hide();
                    tooltip.SetToolTip(this, string.Empty);
                }
            };

            
            mnu.Items.AddRange(new ToolStripItem[] { mnuPause, mnuToolTip });

            */

            this.FormBorderStyle = FormBorderStyle.None;
            toolTip1.SetToolTip(this, "Auby zuby");//-- NF: set something in the toolip
                                                  //this.StartPosition = FormStartPosition.CenterScreen;

            this.Text = "";
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.ControlBox = false;

            this.DoubleBuffered = true;

            readConfig();

            m_blackPen = new Pen(Color.Black);
            m_grayPen = new Pen(Color.LightGray);

            m_timer = new Timer();
            m_timer.Tick += onTimer;
            m_timer.Interval = 1000;
            m_timer.Enabled = true;

            MouseDown += onMouseDown;
            MouseMove += onMouseMove;
            MouseUp += onMouseUp;
        }
        

        class Clock
        {
            private static double m_tickPerNano = 1e6 / TimeSpan.TicksPerMillisecond;

            public static double now()
            {
                return DateTime.Now.Ticks * m_tickPerNano;
            }
        };

        class Pinger
        {
            public static uint capacity = 1024;

            public Pen m_pen;
            public IPAddress m_ip;
            public string m_addr;

            // Round trip time = recvTime - sendTime
            public double[] m_rtt = new double[capacity];
            public uint m_recv_pos = 0;

            //private bool m_sending = false;
            private Ping m_sender = new Ping();

            public Pinger()
            {
                m_sender.PingCompleted += (sender, e) => this.onPingComplete(sender, e);
            }

            public void ping(uint pos, double now)
            {
                m_rtt[pos] = -now; // negative means no response yet
                m_sender.SendAsync(m_ip, 20000);
            }

            public void onPingComplete(object sender, PingCompletedEventArgs e)
            {
                uint pos = m_recv_pos % capacity;
                m_recv_pos++;
                double rtt = m_rtt[pos] + Clock.now();
                m_rtt[pos] = Math.Log10(Math.Max(1, rtt)); // positive, or 0, means we got a response
                                                           //System.Diagnostics.Debug.WriteLine($"ping return - {m_addr} {rtt}");
            }
        };


       
        System.Drawing.Pen m_blackPen;
        System.Drawing.Pen m_grayPen;

        System.Drawing.SolidBrush m_bgBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White);

        Timer m_timer;

        List<Pinger> m_pingers = new List<Pinger>();

        uint m_send_pos = 0;

        static double y_min = 1e6; // 1ms
        static double y_max = 20e9; // 20s
        static double log_y_min = Math.Log10(y_min);
        static double log_y_max = Math.Log10(y_max);

        private bool m_draging = false;
        private Point m_dragStart;

        enum Styles : int
        {
            WS_BORDER = 0x00800000,
            WS_CAPTION = 0x00C00000,
            WS_CHILD = 0x40000000,
            WS_CHILDWINDOW = 0x40000000,
            WS_CLIPCHILDREN = 0x02000000,
            WS_CLIPSIBLINGS = 0x04000000,
            WS_DISABLED = 0x08000000,
            WS_DLGFRAME = 0x00400000,
            WS_GROUP = 0x00020000,
            WS_HSCROLL = 0x00100000,
            WS_ICONIC = 0x20000000,
            WS_MAXIMIZE = 0x01000000,
            WS_MAXIMIZEBOX = 0x00010000,
            WS_MINIMIZE = 0x20000000,
            WS_MINIMIZEBOX = 0x00020000,
            WS_OVERLAPPED = 0x00000000,
            WS_OVERLAPPEDWINDOW = (WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX),
            WS_POPUP = -2147483648,
            WS_POPUPWINDOW = (WS_POPUP | WS_BORDER | WS_SYSMENU),
            WS_SIZEBOX = 0x00040000,
            WS_SYSMENU = 0x00080000,
            WS_TABSTOP = 0x00010000,
            WS_THICKFRAME = 0x00040000,
            WS_TILED = 0x00000000,
            WS_TILEDWINDOW = (WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX),
            WS_VISIBLE = 0x10000000,
            WS_VSCROLL = 0x00200000,
        };

        enum ExStyles : int
        {
            WS_EX_ACCEPTFILES = 0x00000010,
            WS_EX_APPWINDOW = 0x00040000,
            WS_EX_CLIENTEDGE = 0x00000200,
            WS_EX_COMPOSITED = 0x02000000,
            WS_EX_CONTEXTHELP = 0x00000400,
            WS_EX_CONTROLPARENT = 0x00010000,
            WS_EX_DLGMODALFRAME = 0x00000001,
            WS_EX_LAYERED = 0x00080000,
            WS_EX_LAYOUTRTL = 0x00400000,
            WS_EX_LEFT = 0x00000000,
            WS_EX_LEFTSCROLLBAR = 0x00004000,
            WS_EX_LTRREADING = 0x00000000,
            WS_EX_MDICHILD = 0x00000040,
            WS_EX_NOACTIVATE = 0x08000000,
            WS_EX_NOINHERITLAYOUT = 0x00100000,
            WS_EX_NOPARENTNOTIFY = 0x00000004,
            WS_EX_NOREDIRECTIONBITMAP = 0x00200000,
            WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE),
            WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST),
            WS_EX_RIGHT = 0x00001000,
            WS_EX_RIGHTSCROLLBAR = 0x00000000,
            WS_EX_RTLREADING = 0x00002000,
            WS_EX_STATICEDGE = 0x00020000,
            WS_EX_TOOLWINDOW = 0x00000080,
            WS_EX_TOPMOST = 0x00000008,
            WS_EX_TRANSPARENT = 0x00000020,
            WS_EX_WINDOWEDGE = 0x00000100,
        };

        protected override CreateParams CreateParams
        {
            get
            {
                //new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();

                // Extend the CreateParams property of the Button class.
                CreateParams cp = base.CreateParams;
                // Update the button Style.
                cp.Style = (int)(Styles.WS_VISIBLE | Styles.WS_SIZEBOX | Styles.WS_POPUP);
                cp.ExStyle = (int)(ExStyles.WS_EX_WINDOWEDGE);
                cp.Caption = "";

                return cp;
            }
        }

        //ToolTip tooltip;


    
        void errorBox(string msg)
        {
            MessageBox.Show(msg, "Pingraph - Error", MessageBoxButtons.OK);
        }

        void readConfig()
        {
           // string[] args = Environment.GetCommandLineArgs();
            //if (args.Length != 2)
            //{
            //    errorBox("Expected config file in command line");
            //    Application.Exit();
            //}

            int lineNum = 0;
            string lineStr;
            string cfgPath= Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "pingraph.cfg");
            //if (!System.IO.File.Exists(args[1])) args[1] = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "pingraph.cfg");
            // Read the file and display it line by line.  

            string[] lines = File.ReadAllLines(cfgPath);
            //System.IO.StreamReader file = new System.IO.StreamReader(cfgPath);
            foreach(string sLine in lines)//((lineStr = file.ReadLine()) != null)
            {
                lineNum++;
                lineStr = sLine.Trim();
                if (lineStr.Length == 0 || lineStr[0] == '#')
                    continue;

                string[] lineArgs = lineStr.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                if (lineArgs[0] == "ping")
                {
                    initPinger(lineNum, lineArgs);
                }
                //else if (lineArgs[0] == "log")
            }
           // file.Close();
        }

        private void initPinger(int lineNum, string[] lineArgs)
        {
            if (lineArgs.Length != 3)
            {
                errorBox($"Config error in line {lineNum} - expected addr and color");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"Creating pinger {lineArgs[1]} {lineArgs[2]}");

                Pinger pinger = new Pinger();
                pinger.m_addr = lineArgs[1];
                pinger.m_ip = Dns.GetHostAddresses(lineArgs[1])[0];

                Color color = System.Drawing.ColorTranslator.FromHtml("#ff" + lineArgs[2]);
                pinger.m_pen = new System.Drawing.Pen(color);
                pinger.m_pen.Width = (float)1.75;
                pinger.m_pen.LineJoin = LineJoin.Round;

                m_pingers.Add(pinger);
            }
            catch (Exception e)
            {
                errorBox($"Config error in line {lineNum} - {e.Message}");
            }
        }

        private void onTimer(Object source, EventArgs e)
        {
            uint pos = m_send_pos % Pinger.capacity;
            m_send_pos++;

            double now = Clock.now();

            //System.Diagnostics.Debug.WriteLine($"sending pings {now} {pos}");

            foreach (Pinger pinger in m_pingers)
            {
                pinger.ping(pos, now);
            }

            Refresh();
        }
        protected override void OnResize(EventArgs e)
        {
            Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!pingEnabled) return;

            Graphics g = e.Graphics;
            g.CompositingMode = CompositingMode.SourceOver;
            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            int w = this.ClientSize.Width;
            int h = this.ClientSize.Height;
            //double now = Clock.now();
            //System.Diagnostics.Debug.WriteLine($"on paint {now} {w} x {h} --- {e.ClipRectangle}");

            double drawSeconds = w * 30.0 / 200.0;

            double yf = h / (log_y_max - log_y_min);
            double xf = w / drawSeconds;

            double get_y(double log_y) { return h - (yf * log_y - yf * log_y_min); }
            double get_x(double x) { return x * xf; }

            g.FillRectangle(m_bgBrush, e.ClipRectangle);

            double y;
            for (double i = log_y_min; i <= log_y_max; i++)
            {
                y = get_y(i);
                g.DrawLine(m_grayPen, 0, (float)y, w, (float)y);
            }

            double[] y_bars = { Math.Log10(1e6), Math.Log10(1e7), Math.Log10(1e8), Math.Log10(1e9), Math.Log10(1e10) };
            foreach (double y_bar in y_bars)
            {
                y = get_y(y_bar);
                g.DrawLine(m_blackPen, 0, (float)y, 5, (float)y);
            }

            uint start = (m_send_pos - (uint)drawSeconds + Pinger.capacity) % Pinger.capacity;
            if (m_send_pos < (uint)drawSeconds)
            {
                start = 0;
                drawSeconds = m_send_pos;
            }

            foreach (Pinger pinger in m_pingers)
            {
                double t = pinger.m_rtt[start % Pinger.capacity];
                if (t < 0)
                    continue;


                double lastY = get_y(t);
                double lastX = w - drawSeconds * xf;

                for (int i = 1; i <= (int)drawSeconds; i++)
                {
                    t = pinger.m_rtt[(start + i) % Pinger.capacity];
                    if (t < 0)
                        break;
                    double x = lastX + xf;
                    y = get_y(t);
                    g.DrawLine(pinger.m_pen, (float)lastX, (float)lastY, (float)x, (float)y);
                    lastX = x;
                    lastY = y;
                }
            }
        }

        private void onMouseDown(Object sender, MouseEventArgs e)
        {
            m_draging = true;
            m_dragStart = e.Location;
        }

        private void onMouseMove(Object sender, MouseEventArgs e)
        {
            if (tooltipEnabled) toolTip1.SetToolTip(this, $"{e.X} say something {e.Y}");

            if (m_draging)
            {
                this.Location = new Point(Location.X - m_dragStart.X + e.X, Location.Y - m_dragStart.Y + e.Y);
                this.Update();
            }
        }
        private void onMouseUp(Object sender, MouseEventArgs e)
        {
            m_draging = false;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void enablePingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pingEnabled = !pingEnabled;
            enablePingToolStripMenuItem.Checked = pingEnabled;
        }

        private void enableTooltipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tooltipEnabled = !tooltipEnabled;
            enableTooltipToolStripMenuItem.Checked = tooltipEnabled;

            if (!tooltipEnabled)
            {
                //tooltip.Hide();
                toolTip1.SetToolTip(this, string.Empty);
            }
        }
    }
}
