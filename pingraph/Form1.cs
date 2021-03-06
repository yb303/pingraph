﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

namespace PingGraph
{
    static class Extension
    {
        public static T[] Slice<T>(this T[] a, int start, int end)
        {
            return a.Take(end).Skip(start).ToArray();
        }
    }

    public partial class Form1 : Form
    {
        class CannotStartException : Exception {}

        class Clock
        {
            private static double m_tickPerNano = 1e9 / Stopwatch.Frequency;

            public static double now()
            {
                return Stopwatch.GetTimestamp() * m_tickPerNano;
            }
        };

        class Pinger
        {
            public static uint capacity = 1024;

            private Form1 m_form;
            public Pen m_pen;
            private IPAddress m_ip;
            private string m_addr;

            // Round trip time = recvTime - sendTime
            private double[] m_rtt = new double[capacity];
            private uint m_recv_pos = 0;

            private Ping m_sender;
            private int m_timeout;

            public Pinger(Form1 form, string addr, string cfg_color)
            {
                m_form = form;

                m_addr = addr;
                try
                {
                    m_ip = Dns.GetHostAddresses(addr)[0];
                }
                catch (Exception)
                {
                }
                m_timeout = (int)(Form1.y_max * 1e-6 * 2);

                Color color = System.Drawing.ColorTranslator.FromHtml("#ff" + cfg_color);
                m_pen = new System.Drawing.Pen(color);
                m_pen.Width = (float)1.75;
                m_pen.LineJoin = LineJoin.Round;
            }

            public void prep()
            {
                try
                {
                    if (m_ip.Equals(IPAddress.None))
                        m_ip = Dns.GetHostAddresses(m_addr)[0];
                }
                catch(Exception)
                {
                }

                m_sender = new Ping();
                m_sender.PingCompleted += onPingComplete;
            }

            public void send(uint pos, double now)
            {
                m_rtt[pos] = -now; // negative means no response yet
                try
                {
                    m_sender.SendAsync(m_ip, m_timeout);
                }
                catch(Exception)
                {
                    m_recv_pos++;
                }
            }

            public void onPingComplete(object sender, PingCompletedEventArgs e)
            {
                uint pos = m_recv_pos++ % capacity;
                if (e.Reply.Status != IPStatus.Success)
                    return;
                double rtt = m_rtt[pos] + Clock.now();
                m_rtt[pos] = Math.Log10(Math.Max(1, rtt)); // positive, or 0, means we got a response
                m_form.Invalidate();
            }

            public double getRtt(uint pos)
            {
                return m_rtt[pos % Pinger.capacity];
            }
        };

        System.Threading.Timer m_pingTimer;
        List<Pinger> m_pingers = new List<Pinger>();
        bool m_pingEnabled = true;

        uint m_send_pos = 0;

        System.Drawing.Pen m_blackPen;
        System.Drawing.Pen m_grayPen;
        System.Drawing.SolidBrush m_bgBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White);

        bool m_tooltipEnabled = false;

        static public double y_min = 1e6; // 1ms
        static public double y_max = 20e9; // 20s
        static double log_y_min = Math.Log10(y_min);
        static double log_y_max = Math.Log10(y_max);

        private bool m_draging = false;
        private Point m_dragStart;

        public Form1()
        {
            InitializeComponent();

            this.ControlBox = false;
            this.Text = "";
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            //this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;

            this.DoubleBuffered = true;

            m_toolTip.SetToolTip(this, "");
            m_menuEnablePing.Checked = m_pingEnabled;
            m_menuEnableTooltip.Checked = m_tooltipEnabled;

            readConfig();

            m_blackPen = new Pen(Color.DarkGray);
            m_grayPen = new Pen(Color.LightGray);

            m_pingTimer = new System.Threading.Timer(onPingTimer, null, 1000, 1000);
        }
 
        void errorBox(string msg)
        {
            MessageBox.Show(msg, "Pingraph - Error", MessageBoxButtons.OK);
        }

        void readConfig()
        {
            string defaultCfgPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "pingraph.cfg"); ;
            string cfgPath = defaultCfgPath;
            try
            {
                string[] args = Environment.GetCommandLineArgs();
                if (args.Length == 2)
                    cfgPath = args[1];

                int lineNum = 0;
                string lineStr;
                string[] lines = File.ReadAllLines(cfgPath);
                foreach (string sLine in lines)
                {
                    lineNum++;
                    lineStr = sLine.Trim();
                    if (lineStr.Length == 0 || lineStr[0] == '#')
                        continue;

                    string[] lineArgs = lineStr.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    if (lineArgs[0] == "ping")
                        initPinger(lineNum, lineArgs);

                    //else if (lineArgs[0] == "log")
                }
                return;
            }
            catch (Exception) {}

            if (cfgPath != defaultCfgPath || File.Exists(cfgPath))
            {
                errorBox($"Cannot read config file - {cfgPath}\n");
                throw new CannotStartException();
            }

            bool ok = true;
            try
            {
                File.WriteAllText(defaultCfgPath, "ping localhost ff0000\n");
                ok = true;
            }
            catch (Exception) {}

            if (ok)
            {
                errorBox($"Expected 'pingraph.cfg' in exe directory, or given in command line\n" +
                         $"Created minimal config - {defaultCfgPath}");
                readConfig();
            }
            else
            {
                errorBox($"Expected 'pingraph.cfg' in exe directory, or given in command line\n" +
                         $"Failed to create minimal config - {defaultCfgPath}");
                throw new CannotStartException();
            }
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

                Pinger pinger = new Pinger(this, lineArgs[1], lineArgs[2]);
                m_pingers.Add(pinger);
            }
            catch (Exception e)
            {
                errorBox($"Config error in line {lineNum} - {e.Message}");
            }
        }

        private void onPingTimer(object state)
        {
            if (!m_pingEnabled) return;

            uint pos = m_send_pos % Pinger.capacity;
            m_send_pos++;

            //System.Diagnostics.Debug.WriteLine($"sending pings {now} {pos}");

            foreach (Pinger pinger in m_pingers)
                pinger.prep();
            
            foreach (Pinger pinger in m_pingers)
                pinger.send(pos, Clock.now());
        }

        private void onDrawTimer(Object source, EventArgs e)
        {
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            Invalidate();
        }

        private string timeString(double nanos, int decimals)
        {
            nanos += 0.5;
            double exp = Math.Log10(nanos);
            uint ts = (uint)exp / 3;
            double num = Math.Round(nanos / Math.Pow(10, ts * 3), decimals);

            string[] suffixes = { "ns", "us", "ms", "s" };

            return $"{num}{suffixes[ts]}";
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            double now = Clock.now();
            //System.Diagnostics.Debug.WriteLine($"on paint {now} {w} x {h} --- {e.ClipRectangle}");

            Graphics g = e.Graphics;
            g.CompositingMode = CompositingMode.SourceOver;
            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            int w = this.ClientSize.Width;
            int h = this.ClientSize.Height;

            // Set the units translation
            double drawSeconds = w * 30.0 / 200.0;
            double yf = h / (log_y_max - log_y_min);
            double xf = w / drawSeconds;

            uint seconds = (uint)drawSeconds;
            seconds += drawSeconds > (uint)drawSeconds ? 1u : 0u;

            float get_y(double log_y) { return (float)(h - yf * (log_y - log_y_min)); }
            //float get_x(double x) { return x * xf; }

            // Draw last point only if we got any pings back
            uint send_pos = m_send_pos;
            bool send_pos_ok = false;
            foreach (Pinger pinger in m_pingers)
                send_pos_ok = pinger.getRtt(send_pos) >= 0;
            if (send_pos > 0 && !send_pos_ok)
                send_pos--;

            // Get left start position
            uint start = (send_pos - seconds + Pinger.capacity) % Pinger.capacity;
            if (send_pos < seconds)
            {
                start = 0;
                seconds = send_pos;
            }

            // Background
            g.FillRectangle(m_bgBrush, e.ClipRectangle);

            // Bars
            for (uint y_bar = (uint)Math.Round(log_y_min); y_bar <= (uint)Math.Round(log_y_max); y_bar++)
            {
                float y = get_y(y_bar);
                g.DrawLine(m_grayPen, 0, y, w, y);
            }

            // first point. Cannot draw lines with array of one
            if (seconds == 0)
                return;

            // Recycle these?
            PointF[] points = new PointF[seconds + 1];

            foreach (Pinger pinger in m_pingers)
            {
                int valid = -1;
                float x = (float)(w - seconds * xf);
                for (int i = 0; i <= seconds; i++)
                {
                    x += (float)xf;
                    double t = pinger.getRtt(start + (uint)i);
                    if (t >= 0)
                    {
                        points[i].X = x;
                        points[i].Y = get_y(t);
                        if (valid == -1) // switch to valid
                            valid = i;
                    }
                    else if (valid > -1) // was valid - draw line
                    {
                        if (i - valid > 1)
                            g.DrawLines(pinger.m_pen, points.Slice(valid, i));
                        valid = -1;
                    }
                }
                if (valid > -1) // was valid - draw line
                {
                    if (seconds - valid > 1)
                        g.DrawLines(pinger.m_pen, points.Slice(valid, (int)seconds));
                }
            }

            // Bar labels
            for (uint y_bar = (uint)Math.Round(log_y_min); y_bar <= (uint)Math.Round(log_y_max); y_bar++)
            {
                float y = get_y(y_bar);
                g.DrawString(timeString(Math.Pow(10, y_bar), 0), this.Font, m_blackPen.Brush, 0, y);
            }

            // Min max labels
            //g.DrawString(timeString(y_min), this.Font, m_blackPen.Brush, 0, h - g.MeasureString("0", this.Font).Height);
            //g.DrawString(timeString(y_max), this.Font, m_blackPen.Brush, 0, 0);
        }

        private void onMouseDown(Object sender, MouseEventArgs e)
        {
            m_draging = true;
            m_dragStart = e.Location;
        }
        private void onMouseMove(Object sender, MouseEventArgs e)
        {
            if (m_draging)
            {
                this.Location = new Point(Location.X - m_dragStart.X + e.X, Location.Y - m_dragStart.Y + e.Y);
                this.Update();
            }
            else if (m_tooltipEnabled)
            {
                int h = this.ClientRectangle.Height;
                double yf = h / (log_y_max - log_y_min);
                double log_y = Math.Pow(10, (h - e.Y) / yf + log_y_min);
                m_toolTip.SetToolTip(this, timeString(log_y, 2));
            }
        }
        private void onMouseUp(Object sender, MouseEventArgs e)
        {
            m_draging = false;
        }

        private void onMenuExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void onMenuEnablePing_Click(object sender, EventArgs e)
        {
            m_pingEnabled = !m_pingEnabled;
            m_menuEnablePing.Checked = m_pingEnabled;
        }

        private void onMenuEnableTooltip_Click(object sender, EventArgs e)
        {
            m_tooltipEnabled = !m_tooltipEnabled;
            m_menuEnableTooltip.Checked = m_tooltipEnabled;

            if (!m_tooltipEnabled)
            {
                m_toolTip.SetToolTip(this, string.Empty);
            }
        }
    }
}
