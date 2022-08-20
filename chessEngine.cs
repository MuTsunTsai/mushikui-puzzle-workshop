
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
			public void set(int X, int Y) { x=X; y=Y;}
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
			public int sx;		// 起點 x
			public int sy;		// 起點 y
			public int tx;		// 終點 x
			public int ty;		// 終點 y
			public int cp;		// 吃子
			public int mi;		// 0=普通 1~12=升變 13=王側入堡 14=后側入堡 15=吃過路兵
			public int tag;		// 0=未知 1=普通 2=將軍 3=雙將軍
			public move(int sourceX, int sourceY, int targetX, int targetY, int capturedPiece, int moveIndicator) {
				sx=sourceX; sy=sourceY; tx=targetX; ty=targetY; cp=capturedPiece; mi=moveIndicator; tag=0;
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
		
		public bool		positionError { get; private set;}
		public string	positionErrorText { get; private set; }
		
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
		private move[]		moveSysHis;
		private string[]	moveHis;
		
		private int		startSide;
		private int		startMove;
		private int		depth;
		
		private int		oP, oR, oN, oB, oQ, oK;		// 我方棋子的代碼
		private int		pP, pR, pN, pB, pQ;			// 敵方棋子的代碼
		
		private int[,]	position=new int[8,8];
		private int		whoseMove;
		private cState	castlingState;
		private sq		enPassantState=new sq(-1,-1);
		private int		halfmoveClock;
		private int		fullmoveClock;
		private sq[]	kingPos={new sq(0,0), new sq(0,0)};
		
		/////////////////////////////////
		// 建構子
		/////////////////////////////////
		
		public chessEngine():this(initFEN) {}
		public chessEngine(string FEN) {
			
			// 陣列初始化
			legalMovesLengthHis=new int[maxDepth];
			legalMovesHis=new string[maxDepth,maxVar];
			legalMovesSysHis=new move[maxDepth,maxVar];
			stateHis=new stat[maxDepth];
			moveSysHis=new move[maxDepth];
			moveHis=new string[maxDepth];
			depth=0;

			// 配置局面
			fromFEN(FEN);
			setPieceCode();
			positionError=!checkBasicLegality();
			if(!positionError) {
				stateHis[depth]=new stat(castlingState, enPassantState, halfmoveClock);
				computeLegalMoves();
			} else {
				legalMovesLengthHis[depth]=0;	// 局面出現錯誤的話，直接設定合法棋步為空，此物件從此無法使用
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

			kingPos[0].set(-1,-1);
			kingPos[1].set(-1,-1);
			castlingState=new cState(false,false,false,false);
			enPassantState.set(-1,-1);
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
				if(c=='k') kingPos[0].set(i, j);
				if(c=='K') kingPos[1].set(i, j);
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
				if(ca[0]>='a'&&ca[0]<='h'&&(ca[1]=='3'||ca[1]=='6')) enPassantState.set(ca[0]-'a', ca[1]-'1');
				else return;
			}

			if(Int32.TryParse(FEN[4], out k)) halfmoveClock=k; else return;
			if(Int32.TryParse(FEN[5], out k)) startMove=fullmoveClock=k;
		}
		
		// 檢查基本的局面錯誤：雙方必須恰一國王，雙方各至多 16 子，兵不能在底線，雙方不能互將，多重將軍不可能，被將一方得行棋，
		// 如果有入堡權的話城堡國王必須在原位，如果有吃過路兵權的話對應位置必須要合理
		// 這些錯誤都會導致之後檢查合法棋步以及行棋的程式出現異常反應
		
		private bool checkBasicLegality() {
			int k=0, i, wC=0, bC=0;
			
			// 棋子數檢查
			foreach(int p in position) {
				if(p==wK) k+=1;
				else if(p==bK) k+=65;
				else if(side(p)==1) wC++;
				else if(side(p)==0) bC++;
			}
			if(k!=66||wC>15||bC>15) { positionErrorText="Piece number error"; return false;}
			
			// 底線兵檢查
			for(i=0;i<8;i++)
				if(position[i,0]==wP||position[i,7]==wP||
					position[i,0]==bP||position[i,7]==bP)
						{ positionErrorText="Pawn in 1st or 8th rank error"; return false;}
			
			// 入堡權檢查
			if(castlingState.K&&(position[4, 0]!=wK||position[7, 0]!=wR)) { positionErrorText="Castling state K error"; return false;}
			if(castlingState.Q&&(position[4, 0]!=wK||position[0, 0]!=wR)) { positionErrorText="Castling state Q error"; return false;}
			if(castlingState.k&&(position[4, 7]!=bK||position[7, 7]!=bR)) { positionErrorText="Castling state k error"; return false;}
			if(castlingState.q&&(position[4, 7]!=bK||position[0, 7]!=bR)) { positionErrorText="Castling state q error"; return false;}
			
			// 吃過路兵權檢查
			if(	enPassantState.y==2&&(whoseMove!=0||
				position[enPassantState.x, enPassantState.y+1]!=wP||
				position[enPassantState.x, enPassantState.y]!=0||
				position[enPassantState.x, enPassantState.y-1]!=0)) { positionErrorText="En passant status error"; return false;}
			if(enPassantState.y==5&&(whoseMove!=1||
				position[enPassantState.x, enPassantState.y-1]!=bP||
				position[enPassantState.x, enPassantState.y]!=0||
				position[enPassantState.x, enPassantState.y+1]!=0)) { positionErrorText="En passant status error"; return false;}
			
			// 將軍檢查
			bC=checkState(0);
			wC=checkState(1);
			if(bC==-1||wC==-1) { positionErrorText="Check before pawn move error"; return false;}	// 發生過路兵的特殊不可能狀況
			if(bC>2||wC>2) { positionErrorText="Multiple check"; return false; }					// 多重將軍
			if((bC>0&&wC>0)||(bC>0&&whoseMove==1)||(wC>0&&whoseMove==0)) { positionErrorText="Checking status error"; return false;}
			return true;	
		}
		private void loadState(stat st) {
			castlingState=st.cs; enPassantState=st.ep; halfmoveClock=st.hm;
		}
		private int checkState(int side) {					// 將軍判斷，現在這個函數只有在載入的時候使用，計算合法棋步的時候若還用這個檢查就太慢了
			int sx=kingPos[side].x, sy=kingPos[side].y, p, i, c=0;
			if(side==1) {
				foreach(sq d in pieceRule[bP].move) if(inBoard(sx-d.x, sy-d.y)&&position[sx-d.x, sy-d.y]==bP) c++;
				foreach(sq d in pieceRule[bN].move) if(inBoard(sx-d.x, sy-d.y)&&position[sx-d.x, sy-d.y]==bN) c++;
				foreach(sq d in pieceRule[bB].move)
					for(i=1;i<8&&inBoard(sx-i*d.x, sy-i*d.y);i++) {
						p=position[sx-i*d.x, sy-i*d.y];
						if(p==0) continue;
						else {
							if(p==bB||p==bQ||(i==1&&p==bK)) c++;
							if(p==bP&&enPassantState.x==sx-i*d.x&&enPassantState.y==sy-i*d.y+1) {	// 斜方向遇到對方的兵要多做一個檢查，
								for(i++;i<8&&inBoard(sx-i*d.x, sy-i*d.y);i++) {						// 否則未來會遇到「吃過路兵導致被對方將軍」這種實際上不可能發生的狀況。
									p=position[sx-i*d.x, sy-i*d.y];
									if(p==0) continue;
									else {
										if(p==bB||p==bQ) return -1;
										break;
									}
								}
							}
							break;
						}
					}
				foreach(sq d in pieceRule[bR].move)
					for(i=1;i<8&&inBoard(sx-i*d.x, sy-i*d.y);i++) {
						p=position[sx-i*d.x, sy-i*d.y];
						if(p==0) continue;
						else { if(p==bR||p==bQ||(i==1&&p==bK)) c++; break; }
					}
			} else {
				foreach(sq d in pieceRule[wP].move) if(inBoard(sx-d.x, sy-d.y)&&position[sx-d.x, sy-d.y]==wP) c++;
				foreach(sq d in pieceRule[wN].move) if(inBoard(sx-d.x, sy-d.y)&&position[sx-d.x, sy-d.y]==wN) c++;
				foreach(sq d in pieceRule[wB].move)
					for(i=1;i<8&&inBoard(sx-i*d.x, sy-i*d.y);i++) {
						p=position[sx-i*d.x, sy-i*d.y];
						if(p==0) continue;
						else {
							if(p==wB||p==wQ||(i==1&&p==wK)) c++;
							if(p==wP&&enPassantState.x==sx-i*d.x&&enPassantState.y==sy-i*d.y-1) {	// 斜方向遇到對方的兵要多做一個檢查，
								for(i++;i<8&&inBoard(sx-i*d.x, sy-i*d.y);i++) {						// 否則未來會遇到「吃過路兵導致被對方將軍」這種實際上不可能發生的狀況。
									p=position[sx-i*d.x, sy-i*d.y];
									if(p==0) continue;
									else {
										if(p==wB||p==wQ) return -1;
										break;
									}
								}
							}
							break;
						}
					}
				foreach(sq d in pieceRule[wR].move)
					for(i=1;i<8&&inBoard(sx-i*d.x, sy-i*d.y);i++) {
						p=position[sx-i*d.x, sy-i*d.y];
						if(p==0) continue;
						else { if(p==wR||p==wQ||(i==1&&p==wK)) c++; break; }
					}
			}
			return c;
		}
		
		/////////////////////////////////
		// 合法棋步計算
		/////////////////////////////////
		
		private bool[,]		attackByOpp			=new bool[8,8];			// 一個格子是否正被對方攻擊（保護）著
		private bool[,]		pinByOpp			=new bool[8,8];			// 一個格上（上的棋子）是否正在被對方釘著
		private bool[,,]	canAttackOppKing	=new bool[8,8,13];		// 如果將一個特定種類的棋子移動到該格子上，是否可以攻擊到對方的國王
		private bool[,]		pinBySelf			=new bool[8,8];			// 一個格子（上的棋子）是否正在被我方釘著（亦即移開該棋子可造成閃擊）
		private bool[,]		canStopCheck		=new bool[8,8];			// 假如把一個棋子移動到這邊，就可以解除將軍（擋住長程子力、或者吃掉將軍子）
		
		private int			checkPieceCount;
		
		private move[,,]	DisambList			=new move[8,8,16];
		private int[,]		DisambListLength	=new int[8,8];
		
		private bool computeLegalMoves() {
			move[] L=new move[maxVar];
			int[] tag=new int[maxVar];
			int sx, sy, i, j=0, k, l=0;
			bool w;
			
			generateBoardData();

			for(sy=7;sy>=0;sy--) for(sx=0;sx<8;sx++) {
				k=position[sx,sy];

				if(side(k)==1-whoseMove) {	// 如果棋子是敵營的，生成 attackByOpp 資料
					if(pieceRule[k].type==0) {
						foreach(sq d in pieceRule[k].move)
							if(inBoard(sx+d.x, sy+d.y)) attackByOpp[sx+d.x, sy+d.y]=true;
								
					} else {
						foreach(sq d in pieceRule[k].move)
							for(i=1;i<8&&inBoard(sx+i*d.x, sy+i*d.y);i++) {
								attackByOpp[sx+i*d.x, sy+i*d.y]=true;
								if(position[sx+i*d.x, sy+i*d.y]==oK&&inBoard(sx+(i+1)*d.x, sy+(i+1)*d.y))	// 如果碰到我方國王，把下一格也列入攻擊範圍（以免國王往反方向跑）
									attackByOpp[sx+(i+1)*d.x, sy+(i+1)*d.y]=true;							// 很容易忽略的程式設計盲點！
								if(position[sx+i*d.x, sy+i*d.y]!=0) break;
							}
					}
				}
				if(side(k)==whoseMove) {	// 如果棋子是自己這一方的
								
					// 小兵棋步
					if(k==oP) {
						w=(whoseMove==1);
						if(position[sx, sy+(w?1:-1)]==0) {
							if(sy==(w?6:1)) for(j=oR;j<oK;j++) L[l++]=new move(sx, sy, sx, sy+(w?1:-1), 0, j);
							else L[l++]=new move(sx, sy, sx, sy+(w?1:-1), 0, 0);
							if(sy==(w?1:6)&&position[sx, sy+(w?2:-2)]==0) L[l++]=new move(sx, sy, sx, sy+(w?2:-2), 0, 0);
						}
						foreach(sq d in pieceRule[k].move) if(inBoard(sx+d.x, sy+d.y)) {
							if(side(position[sx+d.x, sy+d.y])==1-whoseMove) {
								if(sy==(w?6:1)) for(j=oR;j<oK;j++) L[l++]=new move(sx, sy, sx+d.x, sy+d.y, position[sx+d.x, sy+d.y], j);
								else L[l++]=new move(sx, sy, sx+d.x, sy+d.y, position[sx+d.x, sy+d.y], 0);
							}
							if(sx+d.x==enPassantState.x&&sy+d.y==enPassantState.y)
								L[l++]=new move(sx, sy, sx+d.x, sy+d.y, w?7:1, epMove);
						}
					}
					
					// 入堡棋步，這邊只檢查入堡權以及中間的格子是否空的，攻擊檢查待會再做
					if(k==oK) {
						if(	(k==wK?castlingState.K:castlingState.k)
							&&position[5, sy]==0&&position[6, sy]==0)
								L[l++]=new move(4, sy, 6, sy, 0, OOMove);
						if(	(k==wK?castlingState.Q:castlingState.q)&&
							position[3, sy]==0&&position[2, sy]==0&&position[1, sy]==0)
								L[l++]=new move(4, sy, 2, sy, 0, OOOMove);
					}
					
					// 普通棋步（含國王的）
					if(k!=oP) {
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
			}

			Array.Clear(DisambListLength, 0, 64);
			for(i=0,j=0;i<l;i++) {
				L[i].tag=checkMove(L[i]);
				if(L[i].tag>0) {
					DisambList[L[i].tx, L[i].ty, DisambListLength[L[i].tx, L[i].ty]++]=L[i];	// 登錄消歧義名單
					legalMovesSysHis[depth, j++]=L[i];
				}
			}
			legalMovesLengthHis[depth]=j;
			for(i=0;i<j;i++) legalMovesHis[depth, i]=moveToPGN(legalMovesSysHis[depth, i]);
			
			if(j==0&&depth>0&&moveHis[depth-1].EndsWith("+"))			// 如果沒有合法棋步可動，且上一步是將軍
				moveHis[depth-1]=moveHis[depth-1].Replace("+", "#");	// 換掉將軍符號為將死
			
			return j>0;
		}

		// 生成大部分的棋盤資料（除了 attackByOpp 資料之外）
		// 這些資料都只要以國王為中心看一次就可以曉得了，所以無須看過整個棋盤
		private void generateBoardData() {
			int sx, sy, i, j;
			
			Array.Clear(attackByOpp, 0, 64);
			Array.Clear(pinByOpp, 0, 64);
			Array.Clear(canAttackOppKing, 0, 832);
			Array.Clear(pinBySelf, 0, 64);
			Array.Clear(canStopCheck, 0, 64);

			checkPieceCount=0;
			
			// 處理敵方國王
			sx=kingPos[1-whoseMove].x; sy=kingPos[1-whoseMove].y;
			foreach(sq d in pieceRule[oP].move) if(inBoard(sx-d.x, sy-d.y)) canAttackOppKing[sx-d.x, sy-d.y, oP]=true;
			foreach(sq d in pieceRule[oN].move) if(inBoard(sx-d.x, sy-d.y)) canAttackOppKing[sx-d.x, sy-d.y, oN]=true;
			foreach(sq d in pieceRule[oB].move)
				for(i=1;i<8&&inBoard(sx-i*d.x, sy-i*d.y);i++) {
					canAttackOppKing[sx-i*d.x, sy-i*d.y, oB]=true;
					canAttackOppKing[sx-i*d.x, sy-i*d.y, oQ]=true;
					if(position[sx-i*d.x, sy-i*d.y]!=0) {
						if(	side(position[sx-i*d.x, sy-i*d.y])==whoseMove||(position[sx-i*d.x, sy-i*d.y]==pP&&
							enPassantState.x==sx-i*d.x&&enPassantState.y==sy-i*d.y+(whoseMove==1?1:-1))) {				// 處理敵方國王的時候，斜向要考慮吃過路兵閃擊
							j=i;
							for(i++;i<8&&inBoard(sx-i*d.x, sy-i*d.y);i++) {
								if(position[sx-i*d.x, sy-i*d.y]==oB||position[sx-i*d.x, sy-i*d.y]==oQ) pinBySelf[sx-j*d.x, sy-j*d.y]=true;
								if(position[sx-i*d.x, sy-i*d.y]!=0) break;
							}
						}
						break;
					}
				}
			foreach(sq d in pieceRule[oR].move)
				for(i=1;i<8&&inBoard(sx-i*d.x, sy-i*d.y);i++) {
					canAttackOppKing[sx-i*d.x, sy-i*d.y, oR]=true;
					canAttackOppKing[sx-i*d.x, sy-i*d.y, oQ]=true;
					if(position[sx-i*d.x, sy-i*d.y]!=0) {
						if(	side(position[sx-i*d.x, sy-i*d.y])==whoseMove||(d.y==0&&position[sx-i*d.x, sy-i*d.y]==pP&&
							enPassantState.x==sx-i*d.x&&enPassantState.y==sy-i*d.y+(whoseMove==1?1:-1))) {
							j=i;
							if(d.y==0&&inBoard(sx-(i+1)*d.x, sy-(i+1)*d.y)) {											// 橫方向上需要再多做吃過路兵的一次閃兩子判別
								if(position[sx-i*d.x, sy-i*d.y]==oP&&position[sx-(i+1)*d.x, sy-(i+1)*d.y]==pP&&
									enPassantState.x==sx-(i+1)*d.x&&enPassantState.y==sy-(i+1)*d.y+(whoseMove==1?1:-1)) {
									for(i+=2;i<8&&inBoard(sx-i*d.x, sy-i*d.y);i++) {
										if(position[sx-i*d.x, sy-i*d.y]==oR||position[sx-i*d.x, sy-i*d.y]==oQ) pinBySelf[sx-(j+1)*d.x, sy-(j+1)*d.y]=true;
										if(position[sx-i*d.x, sy-i*d.y]!=0) break;
									}
								} else if(position[sx-i*d.x, sy-i*d.y]==pP&&position[sx-(i+1)*d.x, sy-(i+1)*d.y]==oP&&
									enPassantState.x==sx-i*d.x&&enPassantState.y==sy-i*d.y+(whoseMove==1?1:-1)) {
									for(i+=2;i<8&&inBoard(sx-i*d.x, sy-i*d.y);i++) {
										if(position[sx-i*d.x, sy-i*d.y]==oR||position[sx-i*d.x, sy-i*d.y]==oQ) pinBySelf[sx-j*d.x, sy-j*d.y]=true;
										if(position[sx-i*d.x, sy-i*d.y]!=0) break;
									}
								}
							} else {
								for(i++;i<8&&inBoard(sx-i*d.x, sy-i*d.y);i++) {
									if(position[sx-i*d.x, sy-i*d.y]==oR||position[sx-i*d.x, sy-i*d.y]==oQ) pinBySelf[sx-j*d.x, sy-j*d.y]=true;
									if(position[sx-i*d.x, sy-i*d.y]!=0) break;
								}
							}
						}
						break;
					}
				}

			// 處理我方國王
			sx=kingPos[whoseMove].x; sy=kingPos[whoseMove].y;
			foreach(sq d in pieceRule[pP].move) if(inBoard(sx-d.x, sy-d.y)&&position[sx-d.x, sy-d.y]==pP) { checkPieceCount++; canStopCheck[sx-d.x, sy-d.y]=true;}
			foreach(sq d in pieceRule[pN].move) if(inBoard(sx-d.x, sy-d.y)&&position[sx-d.x, sy-d.y]==pN) { checkPieceCount++; canStopCheck[sx-d.x, sy-d.y]=true;}
			foreach(sq d in pieceRule[pB].move)
				for(i=1;i<8&&inBoard(sx-i*d.x, sy-i*d.y);i++) {
					if(position[sx-i*d.x, sy-i*d.y]==pB||position[sx-i*d.x, sy-i*d.y]==pQ) {
						checkPieceCount++;
						for(j=1;j<=i;j++) canStopCheck[sx-j*d.x, sy-j*d.y]=true;
					}
					if(position[sx-i*d.x, sy-i*d.y]!=0) {
						if(side(position[sx-i*d.x, sy-i*d.y])==whoseMove) {
							j=i;
							for(i++;i<8&&inBoard(sx-i*d.x, sy-i*d.y);i++) {
								if(position[sx-i*d.x, sy-i*d.y]==pB||position[sx-i*d.x, sy-i*d.y]==pQ) pinByOpp[sx-j*d.x, sy-j*d.y]=true;
								if(position[sx-i*d.x, sy-i*d.y]!=0) break;
							}
						}
						break;
					}
				}
			foreach(sq d in pieceRule[pR].move)
				for(i=1;i<8&&inBoard(sx-i*d.x, sy-i*d.y);i++) {
					if(position[sx-i*d.x, sy-i*d.y]==pR||position[sx-i*d.x, sy-i*d.y]==pQ) {
						checkPieceCount++;
						for(j=1;j<=i;j++) canStopCheck[sx-j*d.x, sy-j*d.y]=true;
					}
					if(position[sx-i*d.x, sy-i*d.y]!=0) {
						if(side(position[sx-i*d.x, sy-i*d.y])==whoseMove||
							(d.y==0&&position[sx-i*d.x, sy-i*d.y]==pP&&enPassantState.x==sx-i*d.x&&enPassantState.y==sy-i*d.y+(whoseMove==1?1:-1))) {
							j=i;
							if(d.y==0&&inBoard(sx-(i+1)*d.x, sy-(i+1)*d.y)) {											// 橫方向上需要再多做吃過路兵的一次閃兩子判別
								if(position[sx-i*d.x, sy-i*d.y]==oP&&position[sx-(i+1)*d.x, sy-(i+1)*d.y]==pP&&
									enPassantState.x==sx-(i+1)*d.x&&enPassantState.y==sy-(i+1)*d.y+(whoseMove==1?1:-1)) {
									for(i+=2;i<8&&inBoard(sx-i*d.x, sy-i*d.y);i++) {
										if(position[sx-i*d.x, sy-i*d.y]==pR||position[sx-i*d.x, sy-i*d.y]==pQ) pinByOpp[sx-j*d.x, sy-j*d.y]=true;
										if(position[sx-i*d.x, sy-i*d.y]!=0) break;
									}
								} else if(position[sx-i*d.x, sy-i*d.y]==pP&&position[sx-(i+1)*d.x, sy-(i+1)*d.y]==oP&&
									enPassantState.x==sx-i*d.x&&enPassantState.y==sy-i*d.y+(whoseMove==1?1:-1)) {
									for(i+=2;i<8&&inBoard(sx-i*d.x, sy-i*d.y);i++) {
										if(position[sx-i*d.x, sy-i*d.y]==pR||position[sx-i*d.x, sy-i*d.y]==pQ) pinByOpp[sx-(j+1)*d.x, sy-(j+1)*d.y]=true;
										if(position[sx-i*d.x, sy-i*d.y]!=0) break;
									}
								}
							} else {
								for(i++;i<8&&inBoard(sx-i*d.x, sy-i*d.y);i++) {
									if(position[sx-i*d.x, sy-i*d.y]==pR||position[sx-i*d.x, sy-i*d.y]==pQ) pinByOpp[sx-j*d.x, sy-j*d.y]=true;
									if(position[sx-i*d.x, sy-i*d.y]!=0) break;
								}
							}
						}
						break;
					}
				}
		}

		// 快速合法性判斷，必須先建立棋盤資料才能使用
		private int checkMove(move m) {
			
			// 雙將軍的情況
			if(checkPieceCount>1) {
				if( position[m.sx, m.sy]!=oK||					// 雙將軍的話，動國王是唯一選擇，其他棋步一律駁回
					attackByOpp[m.tx, m.ty]||					// 如果目標格子被攻擊也駁回
					m.mi==OOMove||m.mi==OOOMove) return 0;		// 被將軍還試圖入堡、駁回
				else return checkMoveCheck(m);					// 如果上述情況都沒發生那就表示合法

			
			// 單將軍的情況
			} else if(checkPieceCount==1) {
			
				if(position[m.sx, m.sy]==oK) {									// 如果動的是國王
					if(attackByOpp[m.tx, m.ty]||m.mi==OOMove||m.mi==OOOMove)	// 以不是入堡的方式閃到安全地帶就行了
						return 0;
					else return checkMoveCheck(m);
				} else if(canStopCheck[m.tx, m.ty]&&!checkPin(m)) {				// 如果阻止了對方的將軍也可以，但那個棋子不能被釘住
					return checkMoveCheck(m);
				} else return 0;												// 又不是動國王、又沒阻止將軍，一定不合法


			// 沒有將軍的情況
			} else {
				
				if(m.mi==OOMove) {												// 王側入堡的情況
					if(	attackByOpp[m.sx+1, m.sy]||
						attackByOpp[m.sx+2, m.sy]) return 0;					// 稍早已經檢查過其他要件了，這邊檢查國王路上會不會被攻擊就可以了
					else return checkMoveCheck(m);
				} else if(m.mi==OOOMove) {										// 后側入堡的情況，判斷方式一樣
					if(	attackByOpp[m.sx-1, m.sy]||
						attackByOpp[m.sx-2, m.sy]) return 0;	
					else return checkMoveCheck(m);
				} else if(position[m.sx, m.sy]==oK) {							// 如果移動的是國王
					if(attackByOpp[m.tx, m.ty])	return 0;						// 只要目的地不會被攻擊就好
					else return checkMoveCheck(m);
				} else {														// 如果以上狀況皆非，那只要檢查是否移動的棋子被釘住即可
					if(!checkPin(m)) return checkMoveCheck(m);
					else return 0;
				}				
			}
		}
		
		// 檢查移動棋子有沒有被對方釘住，傳回真表示有被釘住
		// 由於局面合法檢查的時候已經排除了「吃過路兵導致自己被將軍」情況，因此那個不用檢查
		// 吃過路兵的橫向一次兩子閃擊在稍早建立資料的時候已經涵蓋進去了
		private bool checkPin(move m) {
			if(!pinByOpp[m.sx, m.sy]) return false;
			else return !checkParallel(m.tx-m.sx, (float)(m.ty-m.sy), m.sx-kingPos[whoseMove].x, (float)(m.sy-kingPos[whoseMove].y));
		}

		// 平行檢查
		private bool checkParallel(int dx, float dy, int px, float py) {
			if(dx==0&&px==0) return true;
			else if((dx==0&&px!=0)||(dx!=0&&px==0)) return false;
			else if(dy/dx==py/px) return true;
			else return false;
		}
		
		// 已經通過合法檢查，進一步檢查這個棋步是否造成將軍對方
		private int checkMoveCheck(move m) {
			if((m.mi==OOMove||m.mi==OOOMove)&&canAttackOppKing[m.sx, m.sy, oR]) return 2;						// 入堡的情況要多做一種「入堡閃擊」的判斷
			else if(m.mi==OOMove&&canAttackOppKing[m.sx+1, m.sy, oR]) return 2;									// 一般的王側入堡將軍
			else if(m.mi==OOOMove&&canAttackOppKing[m.sx-1, m.sy, oR]) return 2;								// 一般的后側入堡將軍
			else if(pinBySelf[m.sx, m.sy]&&!checkParallel(m.tx-m.sx, (float)(m.ty-m.sy),
					m.sx-kingPos[1-whoseMove].x, (float)(m.sy-kingPos[1-whoseMove].y))) {						// 如果是閃擊（排除了一次閃兩子的橫向閃擊情況），有雙將軍的可能
				//    if((m.mi==0||m.mi==epMove)&&canAttackOppKing[m.tx, m.ty, position[m.sx, m.sy]]) return 3;	// 直接走就進入攻擊位置，雙將軍
				//    else if(m.mi>0&&m.mi<13&&canAttackOppKing[m.tx, m.ty, m.mi]) return 3;						// 升變進入攻擊位置，雙將軍
				//    else if(m.mi==epMove&&pinBySelf[m.tx, m.ty+(whoseMove==1?-1:1)]) return 3;					// 吃過路兵閃擊，雙將軍
				//    else return 2;																				// 以上皆非的話就是單將軍
				//}
				return 2;}
			else if((m.mi==0||m.mi==epMove)&&canAttackOppKing[m.tx, m.ty, position[m.sx, m.sy]]) return 2;		// 直接走就進入攻擊位置
			else if(m.mi>0&&m.mi<13&&canAttackOppKing[m.tx, m.ty, m.mi]) return 2;								// 升變進入攻擊位置
			else if(m.mi==epMove&&pinBySelf[m.tx, m.ty+(whoseMove==1?-1:1)]) return 2;							// 吃過路兵閃擊（涵蓋了一次閃兩子的橫向閃擊情況）
			else return 1;																						// 以上皆非的話，就表示沒有將軍對方		
		}
		private string moveToPGN(move m) {
			string s="";
			int sx=m.sx, sy=m.sy, tx=m.tx, ty=m.ty;
			sq so=new sq(sx,sy), ta=new sq(tx,ty);
			int k=position[sx,sy], l=0, cx=0, cy=0;
			
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
						for(int i=0;i<DisambListLength[tx,ty];i++)
							if(position[DisambList[tx, ty, i].sx, DisambList[tx, ty, i].sy]==k)
								{ l++; if(DisambList[tx, ty, i].sx==so.x) cx++; if(DisambList[tx, ty, i].sy==so.y) cy++;}
						if(l>1) {
							if(cx==1) s+=so.col();
							else if(cy==1) s+=so.rnk();
							else s+=so.ToString();
						}						
					}
					if(m.cp!=0) s+="x"; s+=ta.ToString();
				}				
			}
			if(m.tag==2||m.tag==3) s+="+";			// 關於將死的處理，見合法棋步計算的最後一行
			return s;
		}
		
		/////////////////////////////////
		// 行棋函數
		/////////////////////////////////

		public void retract() {
			if(depth==0) return;
			retractMove(moveSysHis[depth-1]);
		}
		public void play(int i) {
			moveHis[depth]=legalMovesHis[depth,i];
			moveSysHis[depth]=legalMovesSysHis[depth,i];
			playMove(legalMovesSysHis[depth, i]);
			computeLegalMoves();
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
			if(k==wK) { castlingState.K=castlingState.Q=false; kingPos[1].set(tx,ty);}
			if(k==bK) { castlingState.k=castlingState.q=false; kingPos[0].set(tx,ty);}
			if((k==wR&&sx==7&&sy==0)||(tx==7&&ty==0)) castlingState.K=false;
			if((k==wR&&sx==0&&sy==0)||(tx==0&&ty==0)) castlingState.Q=false;
			if((k==bR&&sx==7&&sy==7)||(tx==7&&ty==7)) castlingState.k=false;
			if((k==bR&&sx==0&&sy==7)||(tx==0&&ty==7)) castlingState.q=false;
						
			// 吃過路兵狀態更新
			if(k==wP&&ty-sy==2) enPassantState.set(tx,ty-1);
			else if(k==bP&&sy-ty==2) enPassantState.set(tx,ty+1);
			else enPassantState.set(-1, -1);
			
			depth++;
			if(k==wP||k==bP||m.cp!=0) halfmoveClock=0; else halfmoveClock++;
			stateHis[depth]=new stat(castlingState, enPassantState, halfmoveClock);
			whoseMove=1-whoseMove;
			if(whoseMove==1) fullmoveClock++;
			setPieceCode();
			
			_positionData=null;
		}
		private void retractMove(move m) {
			int sx=m.sx, sy=m.sy, tx=m.tx, ty=m.ty;
			int k=position[tx, ty];
			position[sx,sy]=(m.mi>0&&m.mi<7?wP:(m.mi>6&&m.mi<13?bP:k));
			position[tx,ty]=(m.cp>0&&m.mi!=15?m.cp:0);
			
			if(m.mi==epMove) position[tx, ty+(k==wP?-1:1)]=m.cp;
			if(m.mi==OOMove) { position[5, ty]=0; position[7, ty]=(k==wK?wR:bR);}
			if(m.mi==OOOMove) { position[3, ty]=0; position[0, ty]=(k==wK?wR:bR);}
			if(k==wK||k==bK) kingPos[side(k)].set(sx, sy);
			
			depth--;
			loadState(stateHis[depth]);
			whoseMove=1-whoseMove;
			if(whoseMove==0) fullmoveClock--;

			// 棋子代碼只有判斷合法棋步的時候需要使用，所以倒退的時候不用重設棋子代碼

			_positionData=null;
		}
		private void setPieceCode() {
			if(whoseMove==1) {
				oP=wP; oR=wR; oN=wN; oB=wB; oQ=wQ; oK=wK;
				pP=bP; pR=bR; pN=bN; pB=bB; pQ=bQ;
			} else {
				oP=bP; oR=bR; oN=bN; oB=bB; oQ=bQ; oK=bK;
				pP=wP; pR=wR; pN=wN; pB=wB; pQ=wQ;
			}
		}

		/////////////////////////////////
		// hash 處理
		/////////////////////////////////

		public const int hashBits=23;		// 此數值決定要開多大的調換表，2^23 是很理想的大小
		public const int posDataSize=35;	// 局面資料的大小

		private byte[] _positionData;
		public byte[] positionData {
			get {
				int i, j;
				if(_positionData==null) {
					_positionData=new byte[posDataSize];
					for(i=0;i<4;i++) for(j=0;j<8;j++) _positionData[j*4+i]=(byte)(position[i*2, j]|(position[i*2+1, j]<<4));
					_positionData[32]=(byte)((castlingState.K?8:0)|(castlingState.Q?4:0)|(castlingState.k?2:0)|(castlingState.q?1:0));
					_positionData[33]=(byte)((enPassantState.x==-1?0:(32|(enPassantState.x<<2)|(enPassantState.y==2?2:0)))|(depth>>8));
					_positionData[34]=(byte)(depth&0xFF);
				}
				return _positionData;
			}
		}
		public uint hash {
			get { return MH2.Hash(positionData)>>(32-hashBits); }
		}
		private MurmurHash2Unsafe MH2=new MurmurHash2Unsafe();
	
	}
}
