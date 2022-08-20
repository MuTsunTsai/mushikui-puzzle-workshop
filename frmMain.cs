using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Mushikui_Puzzle_Workshop {
	public partial class frmMain:Form {
	
		public frmMain() {
			InitializeComponent();
		}
		private void frmMain_Load(object sender, EventArgs e) {
			tssbtFunction.Image=imageList.Images[8];
			tsmiSingleSearch.Image=imageList.Images[8];
			tsmiMultipleSearch.Image=imageList.Images[9];
			
			tsbtSearch.Image=imageList.Images[0];
			tsbtResetFEN.Image=imageList.Images[2];
			tsbtListAll.Image=imageList.Images[3];
			tsbtShowFEN.Image=imageList.Images[5];
			tsbtAddStar.Image=imageList.Images[7];
			tsbtInitStar.Image=imageList.Images[10];
			
			notifyIcon.Icon=Icon;
			
			tstbFEN.Text=chessEngine.initFEN;
		}

		/////////////////////////////////
		// 視窗功能
		/////////////////////////////////
		
		private void frmMain_Resize(object sender, EventArgs e) {
			tstbFEN.Width=ClientSize.Width-229;
			tstbFEN.Tag=tstbFEN.Text;
			tstbFEN.Text="";
			tstbFEN.Text=tstbFEN.Tag.ToString();
			if(FormWindowState.Minimized==WindowState) { Hide(); notifyIcon.Visible=true;}
		}
		private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e) {
			Show(); WindowState=FormWindowState.Normal;
			notifyIcon.Visible=false;
		}

		/////////////////////////////////
		// Toggle
		/////////////////////////////////

		private void tsbtShowFEN_CheckedChanged(object sender, EventArgs e) {
			tsbtShowFEN.Image=tsbtShowFEN.Checked?imageList.Images[6]:imageList.Images[5];
		}
		private void tsbtListAll_CheckedChanged(object sender, EventArgs e) {
			tsbtListAll.Image=tsbtListAll.Checked?imageList.Images[4]:imageList.Images[3];
		}
		private void tsbtSearch_CheckedChanged(object sender, EventArgs e) {
			if(tsbtSearch.Checked) {
				tsbtSearch.Image=imageList.Images[1];
				tsbtSearch.ToolTipText="Stop searching";
				tssbtFunction.Enabled=false;
				tstbFEN.Enabled=tsbtResetFEN.Enabled=tsbtListAll.Enabled=false;
				tsbtShowFEN.Enabled=tsbtAddStar.Enabled=tsbtInitStar.Enabled=false;
			} else {
				tsbtSearch.Image=imageList.Images[0];
				tsbtSearch.ToolTipText="Search for the given pattern";
				tssbtFunction.Enabled=true;
				tstbFEN.Enabled=tsbtResetFEN.Enabled=tsbtListAll.Enabled=true;
				tsbtShowFEN.Enabled=tsbtAddStar.Enabled=tsbtInitStar.Enabled=true;
			}
		}

		/////////////////////////////////
		// 按鈕功能
		/////////////////////////////////

		private void tsbtResetFEN_Click(object sender, EventArgs e) {
			tstbFEN.Text=chessEngine.initFEN;
		}
		private void tsmiMultipleSearch_Click(object sender, EventArgs e) {
			tssbtFunction.Image=imageList.Images[9];
			tsbtListAll.Visible=false;
			tsbtShowFEN.Visible=false;
			tsbtAddStar.Visible=true;
			tsbtInitStar.Visible=true;
			searchMode=2;
		}
		private void tsmiSingleSearch_Click(object sender, EventArgs e) {
			tssbtFunction.Image=imageList.Images[8];
			tsbtListAll.Visible=true;
			tsbtShowFEN.Visible=true;
			tsbtAddStar.Visible=false;
			tsbtInitStar.Visible=false;
			searchMode=1;
		}
		private void tssbtFunction_ButtonClick(object sender, EventArgs e) {
			if(searchMode==1) {
				tsmiMultipleSearch_Click(sender,e);
			} else {
				tsmiSingleSearch_Click(sender,e);
			}
		}
		private void tsbtSearch_Click(object sender, EventArgs e) {
			if(STOP) {
				if(searchMode==1) searchPattern(tstbFEN.Text, tbInput.Text);
				else searchMulPattern(tstbFEN.Text, tbInput.Text);
			} else stopSearch();
		}
		private void tsbtAddStar_Click(object sender, EventArgs e) {
			addStar();
		}
		private void tsbtInitStar_Click(object sender, EventArgs e) {
			tbInput.Text="**\r\n***\r\n****\r\n*****\r\n******\r\n*******";
		}
		
		private int searchMode=1;

		/////////////////////////////////
		// 處理 ctrl-A 的語法
		/////////////////////////////////

		private void tbOutput_KeyDown(object sender, KeyEventArgs e) { tb_SelectAll(sender, e); }
		private void tbOutput_KeyPress(object sender, KeyPressEventArgs e) { if(e.KeyChar==1) e.Handled=true; }
		private void tbInput_KeyDown(object sender, KeyEventArgs e) { tb_SelectAll(sender, e); }
		private void tbInput_KeyPress(object sender, KeyPressEventArgs e) { if(e.KeyChar==1) e.Handled=true; }
		private void tb_SelectAll(object sender, KeyEventArgs e) { if(e.Control&&e.KeyCode==Keys.A) ((TextBox)sender).SelectAll(); }

		/////////////////////////////////
		// 搜尋程式通用
		/////////////////////////////////

		private bool STOP=true;
		private chessEngine EN;
		private DateTime Time;
		private TimeSpan TimeUsed;

		private int[] goal=new int[chessEngine.maxDepth];
		private int goalLength;
		private int I, C, TOC;
		private int[] J=new int[chessEngine.maxDepth];

		private void startSearch() {
			STOP=false; tbOutput.Text="";
			Time=DateTime.Now; TOC=0;
		}
		private void stopSearch() {
			if(STOP) return;
			TimeUsed=DateTime.Now.Subtract(Time);
			tbOutput.Text+="\r\nTime used: "+(TimeUsed.Minutes*60+TimeUsed.Seconds)+"."+TimeUsed.Milliseconds;
			toolStripStatusLabel.Text="";
			tsbtSearch.Checked=false;
			STOP=true;
		}
		
		// 如果提早關閉視窗，終止搜尋，免得程式在幕後繼續跑
		private void frmMain_FormClosing(object sender, FormClosingEventArgs e) {
			stopSearch();
		}

		/////////////////////////////////
		// 單一搜尋程式
		/////////////////////////////////
		
		private void searchPattern(string FEN, string probs) {
			int i, j, l=0;
			char[] prob=probs.ToCharArray();
			
			startSearch();
			
			for(i=0;i<prob.Length&&l<500;i++) {
				if(prob[i]=='?') goal[l++]=1;
				else if(prob[i]=='*') {
					for(j=0;i<prob.Length&&prob[i]=='*';i++,j++);
					goal[l++]=j;
				}
			}
			if(l==500) {
				MessageBox.Show("Input problem exceed length 500 limit. The problem will not be processed.");
				stopSearch(); return;
			}
			goalLength=l;
			
			if(goalLength>0) {
				EN=new chessEngine(FEN);
				I=0; C=0; J[0]=0;
				while(!STOP&&runSearch()) {
					TOC++;
					if(TOC>30000&&I>1) {
						Application.DoEvents();
						toolStripStatusLabel.Text=EN.PGN;
						TOC=0;
					}
				}
			} else MessageBox.Show("Please enter a pattern.");
			stopSearch();
		}
		private bool runSearch() {
			if(I==goalLength) {
				tbOutput.Text+=EN.PGN+"\r\n"+(tsbtShowFEN.Checked?EN.FEN+"\r\n":"");
				EN.retract(); I--; C++; J[I]++;
				if(!tsbtListAll.Checked&&C==10) {
					tbOutput.Text+="\r\nToo many solutions. Forced stop. Use \"list all solutions\" option if needed.";
					return false;
				}				
			}
			while(goal[I]>1&&J[I]<EN.legalMovesLength&&EN.legalMoves(J[I]).Length!=goal[I]) J[I]++;
			if(J[I]==EN.legalMovesLength) {
				if(I==0) {
					if(C==0) tbOutput.Text="There's no solution to this pattern.";
					else tbOutput.Text+="\r\n"+C+" solution(s) exist to this pattern.";
					return false;
				} else { EN.retract(); I--; J[I]++;}
			} else {
				EN.play(J[I]); I++; J[I]=0;
			}
			return true;
		}

		/////////////////////////////////
		// 多重搜尋程式
		/////////////////////////////////

		private string tempSolution;

		private void searchMulPattern(string FEN, string probs) {
			int i, j, k, MC=0;
			List<int>[] goalList;
			string[] probList=probs.Split('\r');
			char[] prob;
			
			startSearch();
			tbInput.Text="";
			
			goalList=new List<int>[probList.Length];
			for(k=0;k<probList.Length;k++) {
				goalList[k]=new List<int>();
				prob=probList[k].ToCharArray();
				for(i=0;i<prob.Length;i++) {
					if(prob[i]=='?') goalList[k].Add(1);
					else if(prob[i]=='*') {
						for(j=0;i<prob.Length&&prob[i]=='*';i++, j++) ;
						goalList[k].Add(j);
					}
				}
			}			
			foreach(List<int> L in goalList) {
				if(STOP) break;
				Application.DoEvents();
				goalLength=L.Count;
				if(goalLength>0) {
					for(i=0;i<L.Count;i++) goal[i]=L[i];
					toolStripStatusLabel.Text=goalToStar();
					EN=new chessEngine(FEN);
					I=0; C=0; J[0]=0;
					while(!STOP&&runMulSearch()) {
						TOC++;
						if(TOC>30000) Application.DoEvents();
					}
					if(C>0) tbInput.Text+=goalToStar()+"\r\n";
					if(C==1) { tbOutput.Text+=goalToStar()+"\t"+tempSolution+"\r\n"; MC++;}
				}				
			}
			if(!STOP) {
				tbOutput.Text+=MC+" sequence(s) have unique solution.\r\n";
			}
			stopSearch();
		}
		private string goalToStar() {
			string s="";
			int i, j;
			for(i=0;i<goalLength;i++) {
				if(goal[i]==1) s+="? ";
				else if(goal[i]>1) {
					for(j=0;j<goal[i];j++) s+="*";
					s+=" ";
				}				
			}
			return s;
		}
		private bool runMulSearch() {
			if(I==goalLength) {
				tempSolution=EN.PGN;
				EN.retract(); I--; C++; J[I]++;
				if(C>1) return false;
			}
			while(goal[I]>1&&J[I]<EN.legalMovesLength&&EN.legalMoves(J[I]).Length!=goal[I]) J[I]++;
			if(J[I]==EN.legalMovesLength) {
				if(I==0) return false;
				else { EN.retract(); I--; J[I]++; }
			} else {
				EN.play(J[I]); I++; J[I]=0;
			}
			return true;
		}
		private void addStar() {
			int i, j, k;
			List<int>[] goalList;
			string[] probList=tbInput.Text.Split('\r');
			char[] prob;
			string s="";

			tbInput.Text="";

			goalList=new List<int>[probList.Length];
			for(k=0;k<probList.Length;k++) {
				goalList[k]=new List<int>();
				prob=probList[k].ToCharArray();
				for(i=0;i<prob.Length;i++) {
					if(prob[i]=='?') goalList[k].Add(1);
					else if(prob[i]=='*') {
						for(j=0;i<prob.Length&&prob[i]=='*';i++, j++) ;
						goalList[k].Add(j);
					}
				}
			}
			foreach(List<int> L in goalList) {
				goalLength=L.Count;
				if(goalLength>0) {
					for(i=0;i<L.Count;i++) goal[i]=L[i];
					goalLength++;
					for(i=2;i<=7;i++) {
						goal[L.Count]=i;
						s+=goalToStar()+"\r\n";
					}					
				}
			}
			tbInput.Text=s;
		}
	}
}
