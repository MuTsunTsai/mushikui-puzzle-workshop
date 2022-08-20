
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mushikui_Puzzle_Workshop
{
	class chessEngine
	{
		/////////////////////////////////
		// 資料結構
		/////////////////////////////////
	
		private struct sq {
			public int x, y;
			public sq(int X, int Y) { x=X; y=Y;}
			public string col() { return ""+(char)(x+'a');}
			public string rnk() { return ""+(char)(y+'1'); }
			override public string ToString() {
				if(0<=x&&x<8&&0<=y&&y<8) return col()+rnk();
				else return "-";
			}
		}
		private struct pRule {
			public int type;
			public sq[] move;
			public pRule(int T, sq[] S) {
				type=T; move=S;
			}
		}
		private struct cState {
			public bool K;
			public bool Q;
			public bool k;
			public bool q;
			public cState(bool Kp, bool Qp, bool kp, bool qp) {
				K=Kp; Q=Qp; k=kp; q=qp;
			}
			override public string ToString() {
				if(K||Q||k||q) return (K?"K":"")+(Q?"Q":"")+(k?"k":"")+(q?"q":"");
				else return "-";
			}
		}
		private struct move {
			public int sx;
			public int sy;
			public int tx;
			public int ty;
			public int cp;
			public int mi;
			public move(int sourceX, int sourceY, int targetX, int targetY, int capturedPiece, int moveIndicator) {
				sx=sourceX; sy=sourceY; tx=targetX; ty=targetY; cp=capturedPiece; mi=moveIndicator;
			}
		}
		private struct stat {
			public cState	cs;
			public sq		ep;			
			public int		hm;
			public stat(cState castlingSt, sq enPassantSt, int halfmoveC) {
				ep=enPassantSt; cs=castlingSt; hm=halfmoveC;
			}
		}
	
		/////////////////////////////////
		// 行棋規則
		/////////////////////////////////
		
		private const byte wP=1;
		private const byte wR=2;
		private const byte wN=3;
		private const byte wB=4;
		private const byte wQ=5;
		private const byte wK=6;
		private const byte bP=7;
		private const byte bR=8;
		private const byte bN=9;
		private const byte bB=10;
		private const byte bQ=11;
		private const byte bK=12;
		
		private const byte OOMove=13;
		private const byte OOOMove=14;
		private const byte epMove=15;
		
		private pRule[] pieceRule={
			new pRule(0, new sq[] {}),
			new pRule(0, new sq[] { new sq(-1,1), new sq(1,1)}),																						//P
			new pRule(1, new sq[] { new sq(0,1), new sq(0,-1), new sq(1,0), new sq(-1,0)}),																//R
			new pRule(0, new sq[] {	new sq(-1,-2), new sq(1,-2), new sq(2,-1), new sq(2,1), new sq(1,2), new sq(-1,2), new sq(-2,1), new sq(-2,-1)}),	//N
			new pRule(1, new sq[] { new sq(-1,-1), new sq(1,1), new sq(-1,1), new sq(1,-1)}),															//B
			new pRule(1, new sq[] { new sq(0,-1), new sq(0,1), new sq(-1,0), new sq(1,0), new sq(-1,-1), new sq(1,1), new sq(-1,1), new sq(1,-1)}),		//Q
			new pRule(0, new sq[] { new sq(0,-1), new sq(0,1), new sq(-1,0), new sq(1,0), new sq(-1,-1), new sq(1,1), new sq(-1,1), new sq(1,-1)}),		//K
			new pRule(0, new sq[] { new sq(-1,-1), new sq(1,-1)}),																						//p
			new pRule(1, new sq[] { new sq(0,1), new sq(0,-1), new sq(1,0), new sq(-1,0)}),																//r
			new pRule(0, new sq[] { new sq(-1,-2), new sq(1,-2), new sq(2,-1), new sq(2,1), new sq(1,2), new sq(-1,2), new sq(-2,1), new sq(-2,-1)}),	//n
			new pRule(1, new sq[] { new sq(-1,-1), new sq(1,1), new sq(-1,1), new sq(1,-1)}),															//b
			new pRule(1, new sq[] { new sq(0,-1), new sq(0,1), new sq(-1,0), new sq(1,0), new sq(-1,-1), new sq(1,1), new sq(-1,1), new sq(1,-1)}),		//q
			new pRule(0, new sq[] { new sq(0,-1), new sq(0,1), new sq(-1,0), new sq(1,0), new sq(-1,-1), new sq(1,1), new sq(-1,1), new sq(1,-1)})		//k
		};
		
		private char[] pieceName={' ','P','R','N','B','Q','K','p','r','n','b','q','k'};
		private int pieceIndex(char c) {
			switch(c) {
				case 'P': return 1;
				case 'R': return 2;
				case 'N': return 3;
				case 'B': return 4;
				case 'Q': return 5;
				case 'K': return 6;
				case 'p': return 7;
				case 'r': return 8;
				case 'n': return 9;
				case 'b': return 10;
				case 'q': return 11;
				case 'k': return 12;
				default: return 0;
			}
		}
		private bool inBoard(int x, int y) { return 0<=x&&x<8&&0<=y&&y<8;}
		private int side(int k) { return k<1||k>12?-1:(0<k&&k<7?1:0);}

		/////////////////////////////////
		// 公開屬性（全部都是唯讀的）
		/////////////////////////////////

		public const string initFEN="rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
		public const int maxDepth=512;
		public const int maxVar=256;

		public string legalMoves(int i) {
			if(i<0||i>legalMovesLengthHis[depth]) return "";
			else return legalMovesHis[depth,i];
		}		
		public int legalMovesLength {
			get { return legalMovesLengthHis[depth];}
		}
		public bool positionError { get; private set;}
		
		public string PGN {
			get {
				int i, j=1;
				string s="";
				for(i=0;i<depth;i++) {
					if(i==0||i%2!=startSide) { s+=(j+startMove-1)+(startSide==0&&i==0?"...":"."); j++; }
					s+=moveHis[i]+" ";
				}
				return s;
			}
		}
		public string FEN {
			get {
				int i, j, k;
				string s="";
				for(j=7;j>=0;j--) {
					for(i=0;i<8;i++)
						if(position[i, j]>0) s+=pieceName[position[i, j]];
						else { for(k=0;i<8&&position[i, j]==0;k++,i++); s+=k; i--;}
					if(j>0) s+="/";
				}
				s+=	" "+(whoseMove==0?"b":"w")+
					" "+castlingState.ToString()+
					" "+enPassantState.ToString()+
					" "+halfmoveClock+
					" "+fullmoveClock;
				return s;
			}
		}

		/////////////////////////////////
		// 私有屬性
		/////////////////////////////////
		
		private int[]		legalMovesLengthHis;
		private string[,]	legalMovesHis;
		private move[,]		legalMovesSysHis;
		private stat[]		stateHis;
		private move[]		moveHisSys;
		private string[]	moveHis;
		
		private int		startSide;
		private int		startMove;
		private int		depth;
		
		private List<move>[,]	DisambList;
		
		private int[,]	position=new int[8,8];
		private int		whoseMove;
		private cState	castlingState;
		private sq		enPassantState;
		private int		halfmoveClock;
		private int		fullmoveClock;
		private sq[]	kingPos={new sq(0,0), new sq(0,0)};	// 紀錄國王位置，加速判斷是否有將軍
		
		/////////////////////////////////
		// 建構子
		/////////////////////////////////
		
		public chessEngine():this(initFEN) {}
		public chessEngine(string FEN) {
			int sx, sy;
			
			// 陣列初始化
			legalMovesLengthHis=new int[maxDepth];
			legalMovesHis=new string[maxDepth,maxVar];
			legalMovesSysHis=new move[maxDepth,maxVar];
			stateHis=new stat[maxDepth];
			moveHisSys=new move[maxDepth];
			moveHis=new string[maxDepth];
			depth=0;

			// 消歧義動態陣列

			DisambList=new List<move>[8, 8];
			for(sx=0;sx<8;sx++) for(sy=0;sy<8;sy++) DisambList[sx,sy]=new List<move>();
		
			// 配置局面
			fromFEN(FEN);
			positionError=!checkBasicLegality();
			if(!positionError) {
				stateHis[depth]=new stat(castlingState, enPassantState, halfmoveClock);
				computeLegalMoves(false);
			}
		}

		/////////////////////////////////
		// FEN 處理函數
		/////////////////////////////////

		private void fromFEN(string FENs) {
			int i, j, k;
			char c;
			char[] ca;
			string[] FEN=FENs.Split(' ');

			// 先設置變數預設值，以因應萬一語法錯誤而終止的情況

			kingPos[0]=new sq(-1,-1);
			kingPos[1]=new sq(-1,-1);
			castlingState=new cState(false,false,false,false);
			enPassantState=new sq(-1,-1);
			startSide=whoseMove=1;
			halfmoveClock=0;
			startMove=fullmoveClock=1;

			// 下面的語法當中，只要遇到無法解讀的情況就會終止處理，
			// 但並不會產生錯誤訊息，總之會一直試圖解讀到結束或者出現語法錯誤為止。

			if(FEN.Length<6) return;		// 至少得有六個部件，超過無所謂，後面的不管就是了

			FEN[0]=FEN[0].Replace("/", "");
			FEN[0]=FEN[0].Replace("1", " ");
			FEN[0]=FEN[0].Replace("2", "  ");
			FEN[0]=FEN[0].Replace("3", "   ");
			FEN[0]=FEN[0].Replace("4", "    ");
			FEN[0]=FEN[0].Replace("5", "     ");
			FEN[0]=FEN[0].Replace("6", "      ");
			FEN[0]=FEN[0].Replace("7", "       ");
			FEN[0]=FEN[0].Replace("8", "        ");
			if(FEN[0].Length!=64) return;	// 經過替換後，字數必須剛好吻合。

			ca=FEN[0].ToCharArray();
			for(i=0;i<8;i++) for(j=0;j<8;j++) {
				c=ca[(7-j)*8+i];
				position[i, j]=pieceIndex(c);
				if(c=='k') kingPos[0]=new sq(i, j);
				if(c=='K') kingPos[1]=new sq(i, j);
			}
						
			if(FEN[1]=="b") startSide=whoseMove=0;
			else if(FEN[1]!="w") return;

			if(FEN[2]!="-") {
				ca=FEN[2].ToCharArray();
				foreach(char C in ca) {
					if(C=='K') castlingState.K=true;
					else if(C=='Q') castlingState.Q=true;
					else if(C=='k') castlingState.k=true;
					else if(C=='q') castlingState.q=true;
				}
			}

			if(FEN[3]!="-") {
				ca=FEN[3].ToCharArray();
				if(ca.Length!=2) return;
				if(ca[0]>='a'&&ca[0]<='h'&&(ca[1]=='3'||ca[1]=='6')) enPassantState=new sq(ca[0]-'a', ca[1]-'1');
				else return;
			}

			if(Int32.TryParse(FEN[4], out k)) halfmoveClock=k; else return;
			if(Int32.TryParse(FEN[5], out k)) startMove=fullmoveClock=k;
		}
		
		// 檢查基本的局面錯誤：雙方必須恰一國王，兵不能在底線，雙方不能互將，被將一方得行棋。
		// 其餘的例如子力數量超過這種問題不予檢查（不影響行棋）。
		
		private bool checkBasicLegality() {
			int k=0, i;
			bool wC, bC;
			foreach(int p in position) {
				if(p==wK) k+=1;
				if(p==bK) k+=65;
			}
			if(k!=66) return false;
			for(i=0;i<8;i++)
				if(position[i,0]==wP||position[i,7]==wP||
					position[i,0]==bP||position[i,7]==bP) return false;
			bC=checkState(0);
			wC=checkState(1);
			if((bC&&wC)||(bC&&whoseMove==1)||(wC&&whoseMove==0)) return false;
			return true;			
		}
		private void loadState(stat st) {
			castlingState=st.cs; enPassantState=st.ep; halfmoveClock=st.hm;
		}
		
		/////////////////////////////////
		// 合法棋步計算
		/////////////////////////////////

		private bool computeLegalMoves(bool testMode) {
			move[] L=new move[maxVar];
			int[] tag=new int[maxVar];
			int sx, sy, i, j=0, k, l=0;
			int m, M, t;
			bool w;

			for(sy=7;sy>=0;sy--) for(sx=0;sx<8;sx++) {
					DisambList[sx, sy].Clear();
					k=position[sx, sy];
					if(side(k)==whoseMove) {

						// 小兵棋步

						if(k==wP||k==bP) {
							w=(whoseMove==1); m=w?2:8; M=m+4;
							if(position[sx, sy+(w?1:-1)]==0) {
								if(sy==(w?6:1)) for(j=m;j<M;j++) L[l++]=new move(sx, sy, sx, sy+(w?1:-1), 0, j);
								else L[l++]=new move(sx, sy, sx, sy+(w?1:-1), 0, 0);
								if(sy==(w?1:6)&&position[sx, sy+(w?2:-2)]==0) L[l++]=new move(sx, sy, sx, sy+(w?2:-2), 0, 0);
							}
							foreach(sq d in pieceRule[k].move) if(inBoard(sx+d.x, sy+d.y)) {
									if(side(position[sx+d.x, sy+d.y])==1-whoseMove) {
										if(sy==(w?6:1)) for(j=m;j<M;j++) L[l++]=new move(sx, sy, sx+d.x, sy+d.y, position[sx+d.x, sy+d.y], j);
										else L[l++]=new move(sx, sy, sx+d.x, sy+d.y, position[sx+d.x, sy+d.y], 0);
									}
									if(sx+d.x==enPassantState.x&&sy+d.y==enPassantState.y)
										L[l++]=new move(sx, sy, sx+d.x, sy+d.y, w?7:1, epMove);
								}
						}

						// 入堡棋步

						if(!testMode&&(k==bK||k==wK)) {
							if((k==wK?castlingState.K:castlingState.k)&&
								position[5, sy]==0&&position[6, sy]==0&&
								!checkState(whoseMove)&&testMove(new move(4, sy, 5, sy, 0, 0), true)>0)
								L[l++]=new move(4, sy, 6, sy, 0, OOMove);
							if((k==wK?castlingState.Q:castlingState.q)&&
								position[3, sy]==0&&position[2, sy]==0&&position[1, sy]==0&&
								!checkState(whoseMove)&&testMove(new move(4, sy, 3, sy, 0, 0), true)>0)
								L[l++]=new move(4, sy, 2, sy, 0, OOOMove);
						}

						// 普通棋步

						if(k!=wP&&k!=bP)
							if(pieceRule[k].type==0) {
								foreach(sq d in pieceRule[k].move)
									if(inBoard(sx+d.x, sy+d.y)&&side(position[sx+d.x, sy+d.y])!=whoseMove)
										L[l++]=new move(sx, sy, sx+d.x, sy+d.y, position[sx+d.x, sy+d.y], 0);
							} else {
								foreach(sq d in pieceRule[k].move)
									for(i=1;i<8&&inBoard(sx+i*d.x, sy+i*d.y);i++)
										if(position[sx+i*d.x, sy+i*d.y]==0)
											L[l++]=new move(sx, sy, sx+i*d.x, sy+i*d.y, 0, 0);
										else {
											if(side(position[sx+i*d.x, sy+i*d.y])!=whoseMove)
												L[l++]=new move(sx, sy, sx+i*d.x, sy+i*d.y, position[sx+i*d.x, sy+i*d.y], 0);
											break;
										}
							}
					}
				}

			legalMovesLengthHis[depth]=0;
			for(i=0, j=0;i<l;i++) {
				t=testMove(L[i], testMode);
				if(t>0) {
					if(testMode) { j=1; break; }
					tag[j]=t;
					DisambList[L[i].tx, L[i].ty].Add(L[i]);
					legalMovesSysHis[depth, j]=L[i];
					j++;
					legalMovesLengthHis[depth]=j;
				}
			}
			if(!testMode) for(i=0;i<j;i++) legalMovesHis[depth, i]=moveToPGN(legalMovesSysHis[depth, i], tag[i]);

			if(j==0&&depth>0&&moveHis[depth-1].EndsWith("+"))			// 如果沒有合法棋步可動，且上一步是將軍
				moveHis[depth-1]=moveHis[depth-1].Replace("+", "#");	// 換掉將軍符號為將死

			return j>0;
		}
		private bool checkState(int side) {
			int sx=kingPos[side].x, sy=kingPos[side].y, p;
			if(side==1) {
				foreach(sq d in pieceRule[bP].move) if(inBoard(sx-d.x, sy-d.y)&&position[sx-d.x, sy-d.y]==bP) return true;
				foreach(sq d in pieceRule[bN].move) if(inBoard(sx-d.x, sy-d.y)&&position[sx-d.x, sy-d.y]==bN) return true;
				foreach(sq d in pieceRule[bB].move)
					for(int i=1;i<8&&inBoard(sx-i*d.x, sy-i*d.y);i++) {
						p=position[sx-i*d.x, sy-i*d.y];
						if(p==0) continue;
						else { if(p==bB||p==bQ||(i==1&&p==bK)) return true; break; }
					}
				foreach(sq d in pieceRule[bR].move)
					for(int i=1;i<8&&inBoard(sx-i*d.x, sy-i*d.y);i++) {
						p=position[sx-i*d.x, sy-i*d.y];
						if(p==0) continue;
						else { if(p==bR||p==bQ||(i==1&&p==bK)) return true; break; }
					}
			} else {
				foreach(sq d in pieceRule[wP].move) if(inBoard(sx-d.x, sy-d.y)&&position[sx-d.x, sy-d.y]==wP) return true;
				foreach(sq d in pieceRule[wN].move) if(inBoard(sx-d.x, sy-d.y)&&position[sx-d.x, sy-d.y]==wN) return true;
				foreach(sq d in pieceRule[wB].move)
					for(int i=1;i<8&&inBoard(sx-i*d.x, sy-i*d.y);i++) {
						p=position[sx-i*d.x, sy-i*d.y];
						if(p==0) continue;
						else { if(p==wB||p==wQ||(i==1&&p==wK)) return true; break; }
					}
				foreach(sq d in pieceRule[wR].move)
					for(int i=1;i<8&&inBoard(sx-i*d.x, sy-i*d.y);i++) {
						p=position[sx-i*d.x, sy-i*d.y];
						if(p==0) continue;
						else { if(p==wR||p==wQ||(i==1&&p==wK)) return true; break; }
					}
			}
			return false;
		}
		private int testMove(move m, bool testMode) {
			int s=whoseMove, r;
			playMove(m);
			if(checkState(s)) r=0;
			else {
				if(testMode) r=1;
				else if(checkState(1-s)) r=2;		// 關於將死的處理，見合法棋步計算的最後一行
				else r=1;
			}
			retractMove(m);
			return r;
		}
		private string moveToPGN(move m, int tag) {
			string s="";
			int sx=m.sx, sy=m.sy, tx=m.tx, ty=m.ty;
			sq so=new sq(sx, sy), ta=new sq(tx, ty);
			int k=position[sx, sy], l=0, cx=0, cy=0;

			if(m.mi==OOMove) s="O-O";
			else if(m.mi==OOOMove) s="O-O-O";
			else {
				if(k==wP||k==bP) {
					if(m.cp==0) s=ta.ToString();
					else s=so.col()+"x"+ta.ToString()+(m.mi==epMove?"ep":"");
					if(m.mi>0&&m.mi<13) s+="="+pieceName[m.mi<7?m.mi:m.mi-6];
				} else {
					s=pieceName[k<7?k:k-6].ToString();
					if(k!=wK&&k!=bK) {
						foreach(move mo in DisambList[tx, ty])
							if(position[mo.sx, mo.sy]==k) { l++; if(mo.sx==so.x) cx++; if(mo.sy==so.y) cy++; }
						if(l>1) {
							if(cx==1) s+=so.col();
							else if(cy==1) s+=so.rnk();
							else s+=so.ToString();
						}
					}
					if(m.cp!=0) s+="x"; s+=ta.ToString();
				}
			}
			if(tag==2) s+="+";
			//if(tag==3) s+="#";	// 關於將死的處理，見合法棋步計算的最後一行
			return s;
		}
		
		/////////////////////////////////
		// 行棋函數
		/////////////////////////////////

		public void retract() {
			if(depth==0) return;
			retractMove(moveHisSys[depth-1]);
		}
		public void play(int i) {
			moveHis[depth]=legalMovesHis[depth,i];
			moveHisSys[depth]=legalMovesSysHis[depth,i];
			playMove(legalMovesSysHis[depth, i]);
			computeLegalMoves(false);
		}
		private void playMove(move m) {
			int sx=m.sx, sy=m.sy, tx=m.tx, ty=m.ty;
			int k=position[sx,sy];
			position[tx,ty]=(m.mi>0&&m.mi<13?m.mi:k);
			position[sx,sy]=0;
			if(m.mi==epMove) position[tx,ty+(k==wP?-1:1)]=0;
			if(m.mi==OOMove) { position[7,ty]=0; position[5,ty]=(k==wK?wR:bR);}
			if(m.mi==OOOMove) { position[0,ty]=0; position[3, ty]=(k==wK?wR:bR);}
			
			// 入堡狀態更新
			if(k==wK) { castlingState.K=castlingState.Q=false; kingPos[1]=new sq(tx,ty);}
			if(k==bK) { castlingState.k=castlingState.q=false; kingPos[0]=new sq(tx,ty);}
			if((k==wR&&sx==7&&sy==0)||(tx==7&&ty==0)) castlingState.K=false;
			if((k==wR&&sx==0&&sy==0)||(tx==0&&ty==0)) castlingState.Q=false;
			if((k==bR&&sx==7&&sy==7)||(tx==7&&ty==7)) castlingState.k=false;
			if((k==bR&&sx==0&&sy==7)||(tx==0&&ty==7)) castlingState.q=false;
						
			// 吃過路兵狀態更新
			if(k==wP&&ty-sy==2) enPassantState=new sq(tx,ty-1);
			else if(k==bP&&sy-ty==2) enPassantState=new sq(tx,ty+1);
			else enPassantState=new sq(-1, -1);
			
			depth++;
			if(k==wP||k==bP||m.cp!=0) halfmoveClock=0; else halfmoveClock++;
			stateHis[depth]=new stat(castlingState, enPassantState, halfmoveClock);
			whoseMove=1-whoseMove;
			if(whoseMove==1) fullmoveClock++;
		}
		private void retractMove(move m) {
			int sx=m.sx, sy=m.sy, tx=m.tx, ty=m.ty;
			int k=position[tx, ty];
			position[sx,sy]=(m.mi>0&&m.mi<7?wP:(m.mi>6&&m.mi<13?bP:k));
			position[tx,ty]=(m.cp>0&&m.mi!=15?m.cp:0);
			
			if(m.mi==epMove) position[tx, ty+(k==wP?-1:1)]=m.cp;
			if(m.mi==OOMove) { position[5, ty]=0; position[7, ty]=(k==wK?wR:bR);}
			if(m.mi==OOOMove) { position[3, ty]=0; position[0, ty]=(k==wK?wR:bR);}
			if(k==wK||k==bK) kingPos[side(k)]=new sq(sx, sy);
			
			depth--;
			loadState(stateHis[depth]);
			whoseMove=1-whoseMove;
			if(whoseMove==0) fullmoveClock--;
		}
	}
}
