namespace Correct
{
    partial class FormWav
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
            this.chartGraph1 = new UltraChart.ChartGraph();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.chartGraph2 = new UltraChart.ChartGraph();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.chartGraph3 = new UltraChart.ChartGraph();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // chartGraph1
            // 
            this.chartGraph1.BackColor = System.Drawing.Color.Black;
            this.chartGraph1.Dock = System.Windows.Forms.DockStyle.Top;
            this.chartGraph1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(210)))), ((int)(((byte)(10)))));
            this.chartGraph1.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.chartGraph1.Location = new System.Drawing.Point(0, 25);
            this.chartGraph1.Name = "chartGraph1";
            this.chartGraph1.ShowInvalidPoint = true;
            this.chartGraph1.Size = new System.Drawing.Size(1105, 165);
            this.chartGraph1.TabIndex = 0;
            this.chartGraph1.UseFineScale = false;
            // 
            // splitter1
            // 
            this.splitter1.BackColor = System.Drawing.Color.DarkRed;
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter1.Location = new System.Drawing.Point(0, 190);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(1105, 10);
            this.splitter1.TabIndex = 1;
            this.splitter1.TabStop = false;
            // 
            // chartGraph2
            // 
            this.chartGraph2.BackColor = System.Drawing.Color.Black;
            this.chartGraph2.Dock = System.Windows.Forms.DockStyle.Top;
            this.chartGraph2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(210)))), ((int)(((byte)(10)))));
            this.chartGraph2.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.chartGraph2.Location = new System.Drawing.Point(0, 200);
            this.chartGraph2.Name = "chartGraph2";
            this.chartGraph2.ShowInvalidPoint = true;
            this.chartGraph2.Size = new System.Drawing.Size(1105, 205);
            this.chartGraph2.TabIndex = 2;
            this.chartGraph2.UseFineScale = false;
            // 
            // splitter2
            // 
            this.splitter2.BackColor = System.Drawing.Color.DarkRed;
            this.splitter2.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter2.Location = new System.Drawing.Point(0, 405);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(1105, 10);
            this.splitter2.TabIndex = 3;
            this.splitter2.TabStop = false;
            // 
            // chartGraph3
            // 
            this.chartGraph3.BackColor = System.Drawing.Color.Black;
            this.chartGraph3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartGraph3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(210)))), ((int)(((byte)(10)))));
            this.chartGraph3.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.chartGraph3.Location = new System.Drawing.Point(0, 415);
            this.chartGraph3.Name = "chartGraph3";
            this.chartGraph3.ShowInvalidPoint = true;
            this.chartGraph3.Size = new System.Drawing.Size(1105, 178);
            this.chartGraph3.TabIndex = 4;
            this.chartGraph3.UseFineScale = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1105, 25);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(44, 21);
            this.toolStripMenuItem1.Text = "操作";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(152, 22);
            this.toolStripMenuItem2.Text = "还没想起来";
            // 
            // FormWav
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1105, 593);
            this.Controls.Add(this.chartGraph3);
            this.Controls.Add(this.splitter2);
            this.Controls.Add(this.chartGraph2);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.chartGraph1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormWav";
            this.Text = "FormWav";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private UltraChart.ChartGraph chartGraph1;
        private System.Windows.Forms.Splitter splitter1;
        private UltraChart.ChartGraph chartGraph2;
        private System.Windows.Forms.Splitter splitter2;
        private UltraChart.ChartGraph chartGraph3;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
    }
}