namespace PingGraph
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.m_contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.m_menuEnablePing = new System.Windows.Forms.ToolStripMenuItem();
            this.m_menuEnableTooltip = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.m_menuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.m_toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.m_drawTimer = new System.Windows.Forms.Timer(this.components);
            this.m_contextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_contextMenu
            // 
            this.m_contextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.m_contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuEnablePing,
            this.m_menuEnableTooltip,
            this.toolStripSeparator1,
            this.m_menuExit});
            this.m_contextMenu.Name = "contextMenuStrip1";
            this.m_contextMenu.Size = new System.Drawing.Size(188, 76);
            // 
            // m_menuEnablePing
            // 
            this.m_menuEnablePing.Name = "m_menuEnablePing";
            this.m_menuEnablePing.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.m_menuEnablePing.Size = new System.Drawing.Size(187, 22);
            this.m_menuEnablePing.Text = "Enable Ping";
            this.m_menuEnablePing.Click += new System.EventHandler(this.onMenuEnablePing_Click);
            // 
            // m_menuEnableTooltip
            // 
            this.m_menuEnableTooltip.Name = "m_menuEnableTooltip";
            this.m_menuEnableTooltip.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.m_menuEnableTooltip.Size = new System.Drawing.Size(187, 22);
            this.m_menuEnableTooltip.Text = "Enable tooltip";
            this.m_menuEnableTooltip.Click += new System.EventHandler(this.onMenuEnableTooltip_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(184, 6);
            // 
            // m_menuExit
            // 
            this.m_menuExit.Name = "m_menuExit";
            this.m_menuExit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.m_menuExit.Size = new System.Drawing.Size(187, 22);
            this.m_menuExit.Text = "Exit";
            this.m_menuExit.Click += new System.EventHandler(this.onMenuExit_Click);
            // 
            // m_drawTimer
            // 
            this.m_drawTimer.Enabled = true;
            this.m_drawTimer.Interval = 1000;
            this.m_drawTimer.Tick += new System.EventHandler(this.onDrawTimer);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(277, 156);
            this.ContextMenuStrip = this.m_contextMenu;
            this.ControlBox = false;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.onMouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.onMouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.onMouseUp);
            this.m_contextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip m_contextMenu;
        private System.Windows.Forms.ToolTip m_toolTip;
        private System.Windows.Forms.ToolStripMenuItem m_menuEnablePing;
        private System.Windows.Forms.ToolStripMenuItem m_menuEnableTooltip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem m_menuExit;
        private System.Windows.Forms.Timer m_drawTimer;
    }
}

