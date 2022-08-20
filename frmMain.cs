
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;				// StopWatch 類別用的
using Microsoft.VisualBasic.Devices;	// 取得記憶體資訊，需要加入參考 Microsoft.VisualBais



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
			notifyIcon.Text=Text;

			EN.load(chessEngine2.initFEN);

			//SW.Start();
			//EN.test();
			//SW.Stop();
			//MessageBox.Show(SW.Elapsed.TotalSeconds.ToString());
			//SW.Reset();

			//SW.Start();
			//EN.test2();
			//SW.Stop();
			//MessageBox.Show(SW.Elapsed.TotalSeconds.ToString());
			//SW.Reset();

			//chessEngine E=new chessEngine("rn6/p4k1p/6p1/1pP1p3/8/7P/P4P1P/2K4R w - b6 0 21");
			//SW.Start();
			//for(i=0;i<(1<<19);i++) E.test();
			//SW.Stop();
			//MessageBox.Show(SW.Elapsed.TotalSeconds.ToString());
			//SW.Reset();

			tstbFEN.Text=chessEngine2.initFEN;
		}

		/////////////////////////////////
		// 視窗功能
		/////////////////////////////////

		private void frmMain_Resize(object sender, EventArgs e) {
			tstbFEN.Width=ClientSize.Width-229;
			tstbFEN.Tag=tstbFEN.Text;
			tstbFEN.Text="";
			tstbFEN.Text=tstbFEN.Tag.ToString();
			if(FormWindowState.Minimized==WindowState) { Hide(); notifyIcon.Visible=true; }
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
			tstbFEN.Text=chessEngine2.initFEN;
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
				tsmiMultipleSearch_Click(sender, e);
			} else {
				tsmiSingleSearch_Click(sender, e);
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
		private chessEngine2 EN=new chessEngine2();
		private Stopwatch SW=new Stopwatch();
		private const int TOL=4000000;

		private int[] goal=new int[chessEngine2.maxDepth];
		private int goalLength;
		private int I, C, TOC;
		private int[] J=new int[chessEngine2.maxDepth];

		private void startSearch() {
			STOP=false; tbOutput.Text="";
			SW.Reset(); SW.Start();
		}
		private void stopSearch() {
			delTransTable();
			if(STOP) return;
			SW.Stop();
			tbOutput.Text+="\r\nTime used: "+SW.Elapsed.TotalSeconds+"\r\nTicks: "+SW.ElapsedTicks;
			toolStripStatusLabel.Text="";
			tsbtSearch.Checked=false;
			STOP=true;
		}
		
		// 如果提早關閉視窗，終止搜尋，免得程式在幕後繼續跑
		private void frmMain_FormClosing(object sender, FormClosingEventArgs e) {
			stopSearch();
		}

		/////////////////////////////////
		// 調換表（transposition table）
		/////////////////////////////////
		
		private const int hashBits=23;				// 此數值決定要開多大的調換表，2^23 是很理想的大小
		private const int branchSizeLowerBound=10;	// 設置下限，防止後續分歧太小的局面被加入調換表
		private const int lengthLimit=200;			// 設定搜尋題目的最長上界

		private ComputerInfo CInfo=new ComputerInfo();

		private HashTable<bool> transTable=new HashTable<bool>(hashBits, chessEngine2.posDataSize);		// false=無解 true=有解

		private int[]	branchSize;
		private bool[]	hasSolution;	// 目前的 DFS 當中各層是否有解

		private int posCount, transCount, collCount;
		
		private string outputString;
		
		private void initTransTable() {
			transTable.Initialize();
						
			// 下列兩個陣列不用初始化，因為程式執行過程中會自動配值
			branchSize=new int[chessEngine2.maxDepth];
			hasSolution=new bool[chessEngine2.maxDepth];
			
			posCount=0; transCount=0; collCount=0;
		}
		private void clearTransTable() {	// 將調換表狀態全部設為未初化，但不重新配置調換表本身的記憶體
			transTable.Clear();
			posCount=0; transCount=0; collCount=0;
		}
		private void delTransTable() {	// 釋放記憶體
			transTable.Delete();
		}
		private bool checkMemoryFail() {
			if(CInfo.AvailablePhysicalMemory<367001600) {
				stopSearch();
				MessageBox.Show("Not enough memory. Need at least 350MB memory to run search.");
				return true;
			} else return false;
		}

		/////////////////////////////////
		// 單一搜尋程式
		/////////////////////////////////
		
		private void searchPattern(string FEN, string probs) {
			int i, j, l=0;
			char[] prob=probs.ToCharArray();
			
			startSearch();
			if(checkMemoryFail()) return;
					
			for(i=0;i<prob.Length&&l<lengthLimit;i++) {
				if(prob[i]=='?') goal[l++]=1;
				else if(prob[i]=='*') {
					for(j=0;i<prob.Length&&prob[i]=='*';i++,j++);
					goal[l++]=j;
				}
			}
			if(l==lengthLimit) MessageBox.Show("Input problem exceed length "+lengthLimit+" limit. The problem will not be processed.");
			else {
				initTransTable();	
				goalLength=l;
				if(goalLength>0) {
					if(!EN.load(FEN)) {
						MessageBox.Show("Inputed FEN is not a legal position: "+EN.errorText);
						stopSearch();
					}
					I=0; C=0; J[0]=0; branchSize[0]=1; hasSolution[0]=false;
					outputString="";
					while(!STOP&&runSearch()) {
						TOC+=EN.moveCount;
						if(TOC>TOL&&I>1) {
							Application.DoEvents();
							toolStripStatusLabel.Text=EN.PGN;
							tbOutput.Text+=outputString;
							outputString="";
							TOC=0;
						}
					}
					tbOutput.Text+=outputString;
				} else MessageBox.Show("Please enter a pattern.");
				tbOutput.Text+="\r\nPosition:"+posCount+", Transposition:"+transCount+", Collision:"+collCount;
				delTransTable();
			}
			stopSearch();
		}
		private bool runSearch() {
			if(I==goalLength&&!foundSolution(false)) return false;
			while(goal[I]>1&&J[I]<EN.moveCount&&EN.moveLength(J[I])!=goal[I]) J[I]++;
			if(J[I]==EN.moveCount) {
				if(I==0) {
					if(C==0) outputString="There's no solution to this pattern.";
					else outputString+="\r\n"+C+" solution(s) exist to this pattern.";
					return false;
				} else {
					if(I>2) {
						if(branchSize[I]>branchSizeLowerBound) {			// 後續分歧太小的話就不要管，減少無謂局面的記錄，從根本減少碰撞發生率
							if(transTable.Insert(EN.positionData, hasSolution[I])) posCount++;
							else collCount++;
						}
					}
					runRetract();
				}
			} else {
				EN.play(J[I]); I++; J[I]=0; branchSize[I]=1; hasSolution[I]=false;
				if(I>2) {
					if(transTable.LookUp(EN.positionData)) {
						transCount++;
						if(transTable.Value&&!foundSolution(true)) return false;
						if(!transTable.Value) runRetract();
					}
				}
			}
			return true;
		}
		private void runRetract() { branchSize[I-1]+=branchSize[I]; EN.retract(); I--; J[I]++;}		// 這個部分跟多重搜尋是共用的
		private bool foundSolution(bool trans) {
			int i;
			outputString+=EN.PGN+(trans?"transposition":"")+"\r\n"+(tsbtShowFEN.Checked?EN.FEN+"\r\n":"");
			for(i=I;i>=0&&!hasSolution[i];i--) hasSolution[i]=true;
			runRetract(); C++;
			if(!tsbtListAll.Checked&&C==10) {
				outputString+="\r\nToo many solutions. Forced stop. Use \"list all solutions\" option if needed.";
				return false;
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
			if(checkMemoryFail()) return;
			tbInput.Text="";
			
			goalList=new List<int>[probList.Length];
			for(k=0;k<probList.Length;k++) {
				goalList[k]=new List<int>();
				prob=probList[k].ToCharArray();
				for(i=0;i<prob.Length&&goalList[k].Count<lengthLimit;i++) {
					if(prob[i]=='?') goalList[k].Add(1);
					else if(prob[i]=='*') {
						for(j=0;i<prob.Length&&prob[i]=='*';i++, j++) ;
						goalList[k].Add(j);
					}
				}
			}
			initTransTable();
			if(!EN.load(FEN)) {
				MessageBox.Show("Inputed FEN is not a legal position: "+EN.errorText);
				stopSearch();
			}
			foreach(List<int> L in goalList) {
				if(STOP) break;
				Application.DoEvents();
				goalLength=L.Count;
				if(goalLength>0) {
					clearTransTable();
					for(i=0;i<L.Count;i++) goal[i]=L[i];
					toolStripStatusLabel.Text=goalToStar();
					EN.load(FEN);
					I=0; C=0; J[0]=0; branchSize[0]=1; hasSolution[0]=false;
					while(!STOP&&runMulSearch()) {
						TOC+=EN.moveCount;
						if(TOC>TOL) { Application.DoEvents(); TOC=0;}
					}
					if(C>0) tbInput.Text+=goalToStar()+"\r\n";
					if(C==1) { tbOutput.Text+=goalToStar()+"\t"+tempSolution+"\r\n"; MC++;}
				}
			}
			if(!STOP) {
				tbOutput.Text+=MC+" sequence(s) have unique solution.\r\n";
			}
			stopSearch();
			delTransTable();
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
				for(int i=I;i>=0&&!hasSolution[i];i--) hasSolution[i]=true;
				runRetract(); C++;
				if(C>1) return false;
			}
			while(goal[I]>1&&J[I]<EN.moveCount&&EN.moveLength(J[I])!=goal[I]) J[I]++;
			if(J[I]==EN.moveCount) {
				if(I==0) return false;
				else {
					if(I>2) {
						if(branchSize[I]>branchSizeLowerBound)			// 後續分歧太小的話就不要管，減少無謂局面的記錄，從根本減少碰撞發生率
							transTable.Insert(EN.positionData, hasSolution[I]);
					}
					runRetract();
				}
			} else {
				EN.play(J[I]); I++; J[I]=0; branchSize[I]=1; hasSolution[I]=false;
				if(I>2) {
					if(transTable.LookUp(EN.positionData)) {
						if(transTable.Value) { C++; return false; }	// 找到調換解，可以結束了。
						if(!transTable.Value) runRetract();
					}
				}
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
