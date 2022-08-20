namespace Mushikui_Puzzle_Workshop
{
	partial class frmMain
	{
		/// <summary>
		/// 設計工具所需的變數。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 清除任何使用中的資源。
		/// </summary>
		/// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form 設計工具產生的程式碼

		/// <summary>
		/// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改這個方法的內容。
		///
		/// </summary>
		private void InitializeComponent()
		{
			this.components=new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources=new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
			this.statusStrip=new System.Windows.Forms.StatusStrip();
			this.toolStripStatusLabel=new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStrip=new System.Windows.Forms.ToolStrip();
			this.tssbtFunction=new System.Windows.Forms.ToolStripSplitButton();
			this.tsmiSingleSearch=new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiMultipleSearch=new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1=new System.Windows.Forms.ToolStripSeparator();
			this.tsbtSearch=new System.Windows.Forms.ToolStripButton();
			this.toolStripLabel1=new System.Windows.Forms.ToolStripLabel();
			this.tstbFEN=new System.Windows.Forms.ToolStripTextBox();
			this.tsbtResetFEN=new System.Windows.Forms.ToolStripButton();
			this.tsbtInitStar=new System.Windows.Forms.ToolStripButton();
			this.tsbtAddStar=new System.Windows.Forms.ToolStripButton();
			this.tsbtListAll=new System.Windows.Forms.ToolStripButton();
			this.tsbtShowFEN=new System.Windows.Forms.ToolStripButton();
			this.imageList=new System.Windows.Forms.ImageList(this.components);
			this.tbInput=new System.Windows.Forms.TextBox();
			this.splitter1=new System.Windows.Forms.Splitter();
			this.tbOutput=new System.Windows.Forms.TextBox();
			this.notifyIcon=new System.Windows.Forms.NotifyIcon(this.components);
			this.statusStrip.SuspendLayout();
			this.toolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// statusStrip
			// 
			this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
			this.statusStrip.Location=new System.Drawing.Point(0, 390);
			this.statusStrip.Name="statusStrip";
			this.statusStrip.Size=new System.Drawing.Size(629, 22);
			this.statusStrip.TabIndex=0;
			this.statusStrip.Text="statusStrip1";
			// 
			// toolStripStatusLabel
			// 
			this.toolStripStatusLabel.Name="toolStripStatusLabel";
			this.toolStripStatusLabel.Overflow=System.Windows.Forms.ToolStripItemOverflow.Always;
			this.toolStripStatusLabel.Size=new System.Drawing.Size(614, 17);
			this.toolStripStatusLabel.Spring=true;
			this.toolStripStatusLabel.TextAlign=System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// toolStrip
			// 
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tssbtFunction,
            this.toolStripSeparator1,
            this.tsbtSearch,
            this.toolStripLabel1,
            this.tstbFEN,
            this.tsbtResetFEN,
            this.tsbtInitStar,
            this.tsbtAddStar,
            this.tsbtListAll,
            this.tsbtShowFEN});
			this.toolStrip.Location=new System.Drawing.Point(0, 0);
			this.toolStrip.Name="toolStrip";
			this.toolStrip.RenderMode=System.Windows.Forms.ToolStripRenderMode.System;
			this.toolStrip.Size=new System.Drawing.Size(629, 36);
			this.toolStrip.TabIndex=1;
			this.toolStrip.Text="toolStrip1";
			// 
			// tssbtFunction
			// 
			this.tssbtFunction.AutoSize=false;
			this.tssbtFunction.DisplayStyle=System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tssbtFunction.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiSingleSearch,
            this.tsmiMultipleSearch});
			this.tssbtFunction.Image=((System.Drawing.Image)(resources.GetObject("tssbtFunction.Image")));
			this.tssbtFunction.ImageScaling=System.Windows.Forms.ToolStripItemImageScaling.None;
			this.tssbtFunction.ImageTransparentColor=System.Drawing.Color.Magenta;
			this.tssbtFunction.Name="tssbtFunction";
			this.tssbtFunction.Size=new System.Drawing.Size(45, 33);
			this.tssbtFunction.Text="toolStripSplitButton1";
			this.tssbtFunction.ToolTipText="Choose running mode";
			this.tssbtFunction.ButtonClick+=new System.EventHandler(this.tssbtFunction_ButtonClick);
			// 
			// tsmiSingleSearch
			// 
			this.tsmiSingleSearch.Name="tsmiSingleSearch";
			this.tsmiSingleSearch.Size=new System.Drawing.Size(164, 22);
			this.tsmiSingleSearch.Text="Single Search";
			this.tsmiSingleSearch.Click+=new System.EventHandler(this.tsmiSingleSearch_Click);
			// 
			// tsmiMultipleSearch
			// 
			this.tsmiMultipleSearch.Name="tsmiMultipleSearch";
			this.tsmiMultipleSearch.Size=new System.Drawing.Size(164, 22);
			this.tsmiMultipleSearch.Text="Multiple Search";
			this.tsmiMultipleSearch.Click+=new System.EventHandler(this.tsmiMultipleSearch_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name="toolStripSeparator1";
			this.toolStripSeparator1.Size=new System.Drawing.Size(6, 36);
			// 
			// tsbtSearch
			// 
			this.tsbtSearch.AutoSize=false;
			this.tsbtSearch.CheckOnClick=true;
			this.tsbtSearch.DisplayStyle=System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbtSearch.Image=((System.Drawing.Image)(resources.GetObject("tsbtSearch.Image")));
			this.tsbtSearch.ImageScaling=System.Windows.Forms.ToolStripItemImageScaling.None;
			this.tsbtSearch.ImageTransparentColor=System.Drawing.Color.Magenta;
			this.tsbtSearch.Name="tsbtSearch";
			this.tsbtSearch.Size=new System.Drawing.Size(33, 33);
			this.tsbtSearch.ToolTipText="Search for the given pattern";
			this.tsbtSearch.CheckedChanged+=new System.EventHandler(this.tsbtSearch_CheckedChanged);
			this.tsbtSearch.Click+=new System.EventHandler(this.tsbtSearch_Click);
			// 
			// toolStripLabel1
			// 
			this.toolStripLabel1.Name="toolStripLabel1";
			this.toolStripLabel1.Size=new System.Drawing.Size(34, 33);
			this.toolStripLabel1.Text="FEN:";
			// 
			// tstbFEN
			// 
			this.tstbFEN.AutoSize=false;
			this.tstbFEN.Margin=new System.Windows.Forms.Padding(1, 5, 1, 0);
			this.tstbFEN.Name="tstbFEN";
			this.tstbFEN.Size=new System.Drawing.Size(400, 20);
			// 
			// tsbtResetFEN
			// 
			this.tsbtResetFEN.AutoSize=false;
			this.tsbtResetFEN.DisplayStyle=System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbtResetFEN.Image=((System.Drawing.Image)(resources.GetObject("tsbtResetFEN.Image")));
			this.tsbtResetFEN.ImageScaling=System.Windows.Forms.ToolStripItemImageScaling.None;
			this.tsbtResetFEN.ImageTransparentColor=System.Drawing.Color.Magenta;
			this.tsbtResetFEN.Name="tsbtResetFEN";
			this.tsbtResetFEN.Size=new System.Drawing.Size(33, 33);
			this.tsbtResetFEN.ToolTipText="Reset FEN to initial position";
			this.tsbtResetFEN.Click+=new System.EventHandler(this.tsbtResetFEN_Click);
			// 
			// tsbtInitStar
			// 
			this.tsbtInitStar.AutoSize=false;
			this.tsbtInitStar.DisplayStyle=System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbtInitStar.Image=((System.Drawing.Image)(resources.GetObject("tsbtInitStar.Image")));
			this.tsbtInitStar.ImageScaling=System.Windows.Forms.ToolStripItemImageScaling.None;
			this.tsbtInitStar.ImageTransparentColor=System.Drawing.Color.Magenta;
			this.tsbtInitStar.Name="tsbtInitStar";
			this.tsbtInitStar.Size=new System.Drawing.Size(33, 33);
			this.tsbtInitStar.ToolTipText="Use 2 through 7 stars";
			this.tsbtInitStar.Visible=false;
			this.tsbtInitStar.Click+=new System.EventHandler(this.tsbtInitStar_Click);
			// 
			// tsbtAddStar
			// 
			this.tsbtAddStar.AutoSize=false;
			this.tsbtAddStar.DisplayStyle=System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbtAddStar.Image=((System.Drawing.Image)(resources.GetObject("tsbtAddStar.Image")));
			this.tsbtAddStar.ImageScaling=System.Windows.Forms.ToolStripItemImageScaling.None;
			this.tsbtAddStar.ImageTransparentColor=System.Drawing.Color.Magenta;
			this.tsbtAddStar.Name="tsbtAddStar";
			this.tsbtAddStar.Size=new System.Drawing.Size(33, 33);
			this.tsbtAddStar.ToolTipText="Add stars to the patterns";
			this.tsbtAddStar.Visible=false;
			this.tsbtAddStar.Click+=new System.EventHandler(this.tsbtAddStar_Click);
			// 
			// tsbtListAll
			// 
			this.tsbtListAll.AutoSize=false;
			this.tsbtListAll.CheckOnClick=true;
			this.tsbtListAll.DisplayStyle=System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbtListAll.Image=((System.Drawing.Image)(resources.GetObject("tsbtListAll.Image")));
			this.tsbtListAll.ImageScaling=System.Windows.Forms.ToolStripItemImageScaling.None;
			this.tsbtListAll.ImageTransparentColor=System.Drawing.Color.Magenta;
			this.tsbtListAll.Name="tsbtListAll";
			this.tsbtListAll.Size=new System.Drawing.Size(33, 33);
			this.tsbtListAll.ToolTipText="List all solutions";
			this.tsbtListAll.CheckedChanged+=new System.EventHandler(this.tsbtListAll_CheckedChanged);
			// 
			// tsbtShowFEN
			// 
			this.tsbtShowFEN.AutoSize=false;
			this.tsbtShowFEN.CheckOnClick=true;
			this.tsbtShowFEN.DisplayStyle=System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbtShowFEN.Image=((System.Drawing.Image)(resources.GetObject("tsbtShowFEN.Image")));
			this.tsbtShowFEN.ImageScaling=System.Windows.Forms.ToolStripItemImageScaling.None;
			this.tsbtShowFEN.ImageTransparentColor=System.Drawing.Color.Magenta;
			this.tsbtShowFEN.Name="tsbtShowFEN";
			this.tsbtShowFEN.Size=new System.Drawing.Size(33, 33);
			this.tsbtShowFEN.Text="tsbtShowFEN";
			this.tsbtShowFEN.ToolTipText="Show final position\'s FEN for each solution";
			this.tsbtShowFEN.CheckedChanged+=new System.EventHandler(this.tsbtShowFEN_CheckedChanged);
			// 
			// imageList
			// 
			this.imageList.ImageStream=((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
			this.imageList.TransparentColor=System.Drawing.Color.Transparent;
			this.imageList.Images.SetKeyName(0, "search.png");
			this.imageList.Images.SetKeyName(1, "stop.png");
			this.imageList.Images.SetKeyName(2, "home.png");
			this.imageList.Images.SetKeyName(3, "all.png");
			this.imageList.Images.SetKeyName(4, "allc.png");
			this.imageList.Images.SetKeyName(5, "FEN.png");
			this.imageList.Images.SetKeyName(6, "FENc.png");
			this.imageList.Images.SetKeyName(7, "addstar.png");
			this.imageList.Images.SetKeyName(8, "singlesearch.png");
			this.imageList.Images.SetKeyName(9, "multiplesearch.png");
			this.imageList.Images.SetKeyName(10, "star.png");
			// 
			// tbInput
			// 
			this.tbInput.Dock=System.Windows.Forms.DockStyle.Top;
			this.tbInput.Location=new System.Drawing.Point(0, 36);
			this.tbInput.MaxLength=3276700;
			this.tbInput.Multiline=true;
			this.tbInput.Name="tbInput";
			this.tbInput.ScrollBars=System.Windows.Forms.ScrollBars.Vertical;
			this.tbInput.Size=new System.Drawing.Size(629, 128);
			this.tbInput.TabIndex=2;
			this.tbInput.KeyDown+=new System.Windows.Forms.KeyEventHandler(this.tbInput_KeyDown);
			this.tbInput.KeyPress+=new System.Windows.Forms.KeyPressEventHandler(this.tbInput_KeyPress);
			// 
			// splitter1
			// 
			this.splitter1.Dock=System.Windows.Forms.DockStyle.Top;
			this.splitter1.Location=new System.Drawing.Point(0, 164);
			this.splitter1.MinExtra=100;
			this.splitter1.MinSize=100;
			this.splitter1.Name="splitter1";
			this.splitter1.Size=new System.Drawing.Size(629, 3);
			this.splitter1.TabIndex=3;
			this.splitter1.TabStop=false;
			// 
			// tbOutput
			// 
			this.tbOutput.Dock=System.Windows.Forms.DockStyle.Fill;
			this.tbOutput.Location=new System.Drawing.Point(0, 167);
			this.tbOutput.Multiline=true;
			this.tbOutput.Name="tbOutput";
			this.tbOutput.ScrollBars=System.Windows.Forms.ScrollBars.Vertical;
			this.tbOutput.Size=new System.Drawing.Size(629, 223);
			this.tbOutput.TabIndex=4;
			this.tbOutput.KeyDown+=new System.Windows.Forms.KeyEventHandler(this.tbOutput_KeyDown);
			this.tbOutput.KeyPress+=new System.Windows.Forms.KeyPressEventHandler(this.tbOutput_KeyPress);
			// 
			// notifyIcon
			// 
			this.notifyIcon.MouseDoubleClick+=new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseDoubleClick);
			// 
			// frmMain
			// 
			this.AutoScaleDimensions=new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode=System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize=new System.Drawing.Size(629, 412);
			this.Controls.Add(this.tbOutput);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.tbInput);
			this.Controls.Add(this.toolStrip);
			this.Controls.Add(this.statusStrip);
			this.Icon=((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize=new System.Drawing.Size(645, 450);
			this.Name="frmMain";
			this.StartPosition=System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text="Mushikui Puzzle Workshop (ver 0.3.3)";
			this.Load+=new System.EventHandler(this.frmMain_Load);
			this.FormClosing+=new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
			this.Resize+=new System.EventHandler(this.frmMain_Resize);
			this.statusStrip.ResumeLayout(false);
			this.statusStrip.PerformLayout();
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.StatusStrip statusStrip;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
		private System.Windows.Forms.ToolStrip toolStrip;
		private System.Windows.Forms.ToolStripButton tsbtSearch;
		private System.Windows.Forms.ToolStripLabel toolStripLabel1;
		private System.Windows.Forms.ToolStripTextBox tstbFEN;
		private System.Windows.Forms.ToolStripButton tsbtResetFEN;
		private System.Windows.Forms.ToolStripButton tsbtListAll;
		private System.Windows.Forms.ToolStripButton tsbtShowFEN;
		private System.Windows.Forms.ImageList imageList;
		private System.Windows.Forms.TextBox tbInput;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.TextBox tbOutput;
		private System.Windows.Forms.ToolStripButton tsbtAddStar;
		private System.Windows.Forms.ToolStripSplitButton tssbtFunction;
		private System.Windows.Forms.ToolStripMenuItem tsmiSingleSearch;
		private System.Windows.Forms.ToolStripMenuItem tsmiMultipleSearch;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton tsbtInitStar;
		private System.Windows.Forms.NotifyIcon notifyIcon;
	}
}

