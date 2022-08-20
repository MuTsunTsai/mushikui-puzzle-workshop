
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;

namespace Mushikui_Puzzle_Workshop {
	partial class chessEngine2 {

		/////////////////////////////////
		// 測速程式
		/////////////////////////////////

		public void test() {
			ulong a;
			for(int i=0;i<1<<30;i++) a=(ulong)1<<(i%64);
		}

		public void test2() {
			ulong a;
			for(int i=0;i<1<<30;i++) a=mask[i%64];
		}

		/////////////////////////////////
		// 棋步資料結構
		/////////////////////////////////

		// 位元配置：
		// 8 位元 so 起點
		// 8 位元 ta 終點
		// 8 位元 de 吃子點（幾乎都會等於終點，除了吃過路兵的情況外）
		// 4 位元 ot 棋子原始類型
		// 4 位元 nt 棋子新類型（幾乎都等於原始類型，除了升變之外）
		// 4 位元 cp 吃子類型
		// 4 位元 mi 棋步 0=普通 1=O-O 2=O-O-O 3=ep
		// 4 位元 tag 標籤 0=未知 1=普通 2=將軍 3=將死
		// 4 位元 disamb 消歧義 0=不用 1=行 2=列 3=行列
		// 4 位元 le 基底長度（除了消歧義跟將軍記號之外的長度）

		private const int taS=8;
		private const int deS=16;
		private const int otS=24;
		private const int ntS=28;
		private const int cpS=32;
		private const int miS=36;
		private const int tgS=40;
		private const int dbS=44;
		private const int leS=48;
		
		private const byte OOMove=1;
		private const byte OOOMove=2;
		private const byte epMove=3;

		private const ulong len2=(ulong)2<<leS;
		private const ulong len3=(ulong)3<<leS;
		private const ulong len4=(ulong)4<<leS;
		private const ulong len5=(ulong)5<<leS;
		private const ulong len6=(ulong)6<<leS;

		private int moveToLength(ulong m) {
			byte tag=(byte)((m>>tgS)&0xF);
			byte disamb=(byte)((m>>dbS)&0xF);
			int le=(int)(m>>leS);

			if(disamb!=b0) { le++; if(disamb==b3) le++; }
			if(tag==b2) le++;
			return le;
		}
		private string moveToString(ulong m) {
			byte so=(byte)(m&0x3F);
			byte ta=(byte)((m>>taS)&0x3F);
			byte ot=(byte)((m>>otS)&0xF);
			byte nt=(byte)((m>>ntS)&0xF);
			byte cp=(byte)((m>>cpS)&0xF);
			byte mi=(byte)((m>>miS)&0xF);
			byte tag=(byte)((m>>tgS)&0xF);
			byte disamb=(byte)((m>>dbS)&0xF);
		
			string s="";
			if(mi==OOMove) s="O-O";
			else if(mi==OOOMove) s="O-O-O";
			else {
				if((ot&lMask)==wP) {
					if(cp==b0) s=cor(ta);
					else s=col(so)+"x"+cor(ta)+(mi==epMove?"ep":"");
					if(ot!=nt) s+="="+pieceName[nt&lMask];
				} else {
					s+=pieceName[ot&lMask];
					if(disamb==b1) s+=col(so);
					else if(disamb==b2) s+=row(so);
					else if(disamb==b3) s+=cor(so);
					if(cp!=0) s+="x"; s+=cor(ta);
				}
			}
			if(tag==b2) s+="+";
			if(tag==b3) s+="#";
			return s;
		}

		/////////////////////////////////
		// 常數
		/////////////////////////////////

		public const string initFEN="rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

		public const int maxDepth=256;	// 最大深度。實際上會被這個程式搜尋的問題長度往往遠小於這個界線
		public const int maxVar=256;	// 最大棋步清單。暫時無法從理論上保證這個上界一定夠用，但等到不夠的時候再說吧

		// 棋子代碼
		private const byte wP=1;		// 位元結構：
		private const byte wN=2;		// 8=顏色（0 白 1 黑）
		private const byte wK=3;		// 4=長程子力
		private const byte wR=5;		//		2=斜向移動
		private const byte wB=6;		//		1=縱橫移動
		private const byte wQ=7;		// 若非長程子力，低位元區分 P, N, K 三種類型
		private const byte bP=9;		// 下三位元可以區分是哪一種棋子類別（不分顏色）
		private const byte bN=10;
		private const byte bK=11;
		private const byte bR=13;
		private const byte bB=14;
		private const byte bQ=15;
		
		private const byte lMask=7;
		
		private const byte WT=0;		// 白，直觀上好像白應該是 1 才對，但其實把白當成 0 有若干方便之處
		private const byte BC=1;		// 黑，之所以叫 BC 而不叫 BK 是為了跟 bK 區隔
		
		private const byte NS=64;		// 一個不存在的格子
		
		// 入堡常數
		private const byte cwK=8;
		private const byte cwQ=4;
		private const byte cbK=2;
		private const byte cbQ=1;
		private const byte cK=10;
		private const byte cQ=5;

		// 各種型態的數值簡寫
		private const byte	b0=0;
		private const byte	b1=1;
		private const byte	b2=2;
		private const byte	b3=3;
		private const byte	b4=4;
		private const byte  b5=5;
		private const byte  b6=7;
		private const byte  b7=7;
		private const byte	b8=8;
		private const byte	b10=10;
		private const byte	b11=11;
		private const byte	b12=12;
		private const byte	b13=13;
		private const byte	b14=14;
		private const byte	b16=16;
		private const byte	b56=56;
		private const byte	b59=59;
		private const byte	b61=61;
		private const byte	b63=63;
		private const ulong u0=0;

		/////////////////////////////////
		// 棋盤資料
		/////////////////////////////////

		// 棋子佔據情形的位元棋盤，有四種旋轉
		private ulong occuH;
		private ulong occuV;
		private ulong occuFS;
		private ulong occuBS;

		private byte[]		position	=new byte[65];		// 每一個格子的內容，使用於陣列查找，其中最後一格是永久空格		
		private ulong[]		piecePos	=new ulong[16];		// 每一種棋子的分佈情況（有若干空欄位，但管它的）
		private byte[]		kingPos		=new byte[2];		// 國王位置
		
		private int[]		pieceCount	=new int[16];		// 各種棋子的計數器

		private byte		whoseMove=WT;
		private byte[]		castlingState	=new byte[maxDepth];	// 四個位元分別為 KQkq
		private byte[]		enPassantState	=new byte[maxDepth];	// 可以吃過路兵的格子，若為 0 表示沒有
		private byte[]		halfmoveClock	=new byte[maxDepth];
		private int			fullmoveClock=0;
		
		private byte		depth=0;						// 目前的深度
		
		private byte		startSide=WT;
		private int			startMove=0;
		
#if DEBUG
		public int[] pseudoMoveCount=new int[8];
		public int[] totalMoveCount=new int[8];
#endif

		/////////////////////////////////
		// 棋步資料
		/////////////////////////////////

		public int moveLength(int i) { return moveToLength(moveList[depth, i]);}
		public byte moveCount { get { return moveListLength[depth];}}

		private ulong[,]	moveList		=new ulong[maxDepth,maxVar];
		private byte[]		moveListLength	=new byte[maxDepth];
		
		private ulong[]		moveHis			=new ulong[maxDepth];

		private static char[] pieceName= { ' ', 'P', 'N', 'K', ' ', 'R', 'B', 'Q', ' ', 'p', 'n', 'k', ' ', 'r', 'b', 'q'};
		private static char col(byte p) { return (char)((p&lMask)+'a'); }
		private static char row(byte p) { return (char)((p>>3)+'1'); }
		private static string cor(byte p) { return ""+col(p)+row(p); }
		
		private static string castlingString(byte c) {
			if(c==b0) return "-";
			else return ((c&cwK)==cwK?"K":"")+((c&cwQ)==cwQ?"Q":"")+((c&cbK)==cbK?"k":"")+((c&cbQ)==cbQ?"q":"");
		}
		public string PGN {
			get {
				int i, j=1;
				string s="";
				for(i=0;i<depth;i++) {
					if(i==0||i%2==startSide) { s+=(j+startMove-1)+(startSide==BC&&i==0?"...":"."); j++; }
					s+=moveToString(moveHis[i])+" ";
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
						if(position[i+j*8]>0) s+=pieceName[position[i+j*8]];
						else { for(k=0;i<8&&position[i+j*8]==0;k++, i++); s+=k; i--; }
					if(j>0) s+="/";
				}
				s+=" "+(whoseMove==BC?"b":"w")+
					" "+castlingString(castlingState[depth])+
					" "+(enPassantState[depth]==NS?"-":cor(enPassantState[depth]))+
					" "+halfmoveClock[depth]+
					" "+fullmoveClock;
				return s;
			}
		}

		/////////////////////////////////
		// 建構子
		/////////////////////////////////

		public chessEngine2() {
		}

		/////////////////////////////////
		// 局面載入
		/////////////////////////////////
		
		public string errorText { get; private set;}
		
		public bool load(string FEN) {		// 載入指定的 FEN，傳回真偽值表示成功與否
			depth=0;
			fromFEN(FEN);
#if DEBUG
			Array.Clear(pseudoMoveCount,0,8);
			Array.Clear(totalMoveCount,0,8);
#endif
			if(checkBasicLegality()) {
				computeLegalMoves();
				return true;
			} else {
				moveListLength[depth]=0;
				return false;
			}
		}
		private void fromFEN(string FENs) {
			int i, j, k;
			char c;
			char[] ca;
			string[] FEN=FENs.Split(' ');

			// 先設置變數預設值，以因應萬一語法錯誤而終止的情況

			kingPos[0]=NS; kingPos[1]=NS;
			castlingState[depth]=0;
			enPassantState[depth]=NS;
			startSide=whoseMove=WT;
			halfmoveClock[depth]=0;
			fullmoveClock=startMove=1;
			Array.Clear(position, 0, 65);
			Array.Clear(piecePos, 0, 16);
			Array.Clear(pieceCount, 0, 16);
			occuH=occuV=occuFS=occuBS=0;

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
					if(c!=' ') {
						position[i+j*8]=(byte)pieceIndex(c);
						piecePos[pieceIndex(c)]|=mask[i+j*8];
						pieceCount[pieceIndex(c)]++;
						occuH|=mask[i+j*8];
						occuV|=maskV[i+j*8];
						occuFS|=maskFS[i+j*8];
						occuBS|=maskBS[i+j*8];
						if(c=='k') kingPos[BC]=(byte)(i+j*8);
						if(c=='K') kingPos[WT]=(byte)(i+j*8);
					}
				}

			if(FEN[1]=="b") startSide=whoseMove=BC;
			else if(FEN[1]!="w") return;

			if(FEN[2]!="-") {
				ca=FEN[2].ToCharArray();
				foreach(char C in ca) {
					if(C=='K') castlingState[depth]|=cwK;
					else if(C=='Q') castlingState[depth]|=cwQ;
					else if(C=='k') castlingState[depth]|=cbK;
					else if(C=='q') castlingState[depth]|=cbQ;
				}
			}

			if(FEN[3]!="-") {
				ca=FEN[3].ToCharArray();
				if(ca.Length!=2) return;
				if(ca[0]>='a'&&ca[0]<='h'&&(ca[1]=='3'||ca[1]=='6')) enPassantState[depth]=(byte)((ca[0]-'a')+(ca[1]-'1')*8);
				else return;
			}

			if(Int32.TryParse(FEN[4], out k)) halfmoveClock[depth]=(byte)k; else return;
			if(Int32.TryParse(FEN[5], out k)) startMove=fullmoveClock=k;
		}

		// 檢查基本的局面錯誤：雙方必須恰一國王、至多 8 兵且至多 16 子，兵不能在底線，雙方不能互將，被將一方得行棋，
		// 如果有入堡權的話城堡國王必須在原位，如果有吃過路兵權的話對應位置必須要合理
		// 這些錯誤都會導致之後檢查合法棋步以及行棋的程式出現異常反應

		private bool checkBasicLegality() {
			int wC=0, bC=0;
			ulong result;

			// 棋子數檢查
			wC=pieceCount[wP]+pieceCount[wR]+pieceCount[wN]+pieceCount[wB]+pieceCount[wQ];
			bC=pieceCount[bP]+pieceCount[bR]+pieceCount[bN]+pieceCount[bB]+pieceCount[bQ];
			if(	pieceCount[wK]!=1||pieceCount[bK]!=1||					// 雙方恰一國王
				pieceCount[wP]>8||pieceCount[bP]>8||					// 雙方至多八兵
				wC>15||bC>15)											// 雙方不得超過 16 子
				{ errorText="Piece number error"; return false; }

			// 底線兵檢查
			if(((piecePos[wP]|piecePos[bP])&0xFF000000000000FF)!=0) { errorText="Pawn in 1st or 8th rank error"; return false; }

			// 入堡權檢查
			if((castlingState[depth]&cwK)!=0&&(position[4]!=wK||position[7]!=wR)) { errorText="Castling state K error"; return false; }
			if((castlingState[depth]&cwQ)!=0&&(position[4]!=wK||position[0]!=wR)) { errorText="Castling state Q error"; return false; }
			if((castlingState[depth]&cbK)!=0&&(position[60]!=bK||position[63]!=bR)) { errorText="Castling state k error"; return false; }
			if((castlingState[depth]&cbQ)!=0&&(position[60]!=bK||position[56]!=bR)) { errorText="Castling state q error"; return false; }

			// 吃過路兵權檢查
			if(enPassantState[depth]>>3==b2&&(whoseMove!=BC||position[enPassantState[depth]+8]!=wP||
					position[enPassantState[depth]]!=0||position[enPassantState[depth]-8]!=0))
				{ errorText="En passant status error"; return false;}
			if(enPassantState[depth]>>3==b5&&(whoseMove!=WT||position[enPassantState[depth]-8]!=bP||
					position[enPassantState[depth]]!=0||position[enPassantState[depth]+8]!=0))
				{ errorText="En passant status error"; return false;}
				
			// 吃過路兵的不可能將軍檢查，避免之後發生「可以用吃過路兵的方式擋住長程子力將軍」這種不可能狀況
			if(enPassantState[depth]!=NS) {
				if(whoseMove==WT) {
					if((pieceRangeN[kingPos[WT]]&piecePos[bN])!=0) { errorText="Impossible check"; return false; }
					result=slideRangeH[kingPos[WT], occuH>>occuShiftH[kingPos[WT]]&0x3F];
					if((result&(piecePos[bR]|piecePos[bQ]))!=0&&(result&mask[enPassantState[depth]+8])==0) { errorText="Impossible check"; return false; }
					result=slideRangeFS[kingPos[WT], occuFS>>occuShiftFS[kingPos[WT]]&0x3F]|slideRangeBS[kingPos[WT], occuBS>>occuShiftBS[kingPos[WT]]&0x3F];
					if((result&(piecePos[bB]|piecePos[bQ]))!=0&&(result&mask[enPassantState[depth]+8])==0) { errorText="Impossible check"; return false; }
				} else {
					if((pieceRangeN[kingPos[BC]]&piecePos[wN])!=0) { errorText="Impossible check"; return false; }
					result=slideRangeH[kingPos[BC], occuH>>occuShiftH[kingPos[BC]]&0x3F];
					if((result&(piecePos[wR]|piecePos[wQ]))!=0&&(result&mask[enPassantState[depth]-8])==0) { errorText="Impossible check"; return false; }
					result=slideRangeFS[kingPos[BC], occuFS>>occuShiftFS[kingPos[BC]]&0x3F]|slideRangeBS[kingPos[BC], occuBS>>occuShiftBS[kingPos[BC]]&0x3F];
					if((result&(piecePos[wB]|piecePos[wQ]))!=0&&(result&mask[enPassantState[depth]-8])==0) { errorText="Impossible check"; return false; }
				}
			}				

			// 將軍檢查
			bC=checkCount(BC);
			wC=checkCount(WT);
			if(bC==-1||wC==-1) { errorText="Special error"; return false; }		// 王緊貼、兵或騎士雙將、過路兵的特殊不可能狀況
			if(bC>2||wC>2) { errorText="Multiple check"; return false; }		// 多重將軍
			if((bC>0&&wC>0)||(bC>0&&whoseMove==WT)||(wC>0&&whoseMove==BC)) { errorText="Checking status error"; return false; }
			return true;
		}

		// 計算將軍次數，傳回 -1 表示發生錯誤
		// 這個函數是初始的時候使用，主要用途是避免使用者亂輸入 FEN
		private int checkCount(byte side) {
			int p=kingPos[side], c=0, i, data, result;
			ulong A, B;
			byte p1, p2;
			if(side==WT) {
				if((pieceRangeK[p]&piecePos[bK])!=0) return -1;								// 如果發現國王，直接傳回錯誤	
				A=piecePos[bR]|piecePos[bQ]; B=piecePos[bB]|piecePos[bQ];
				for(i=0;(p1=pieceRuleWP[p, i])!=NS;i++) if(position[p1]==bP) c++;			// 小兵跟騎士的將軍因為要計算數目，不能用位元棋盤
				for(i=0;(p1=pieceRuleN[p, i])!=NS;i++) if(position[p1]==bN) c++;
				if(c>=2) return -1;															// 兵或騎士的雙將，這是不可能的
				
				// 縱橫檢查
				if((slideRayR[p, data=(int)(occuH>>occuShiftH[p]&0x3F)]&A)!=0) c++;
				if((slideRayL[p, data]&A)!=0) c++;
				if((slideRayU[p, data=(int)(occuV>>occuShiftV[p]&0x3F)]&A)!=0) c++;
				if((slideRayD[p, data]&A)!=0) c++;

				// 斜向檢查，其中有額外的檢查避免日後出現不可能的「吃過路兵導致被將」
				if((slideRayRU[p, data=(int)(occuFS>>occuShiftFS[p]&0x3F)]&B)!=0) c++;
				if(enPassantState[depth]!=NS) {
					result=slideHitRU[p, data]; p1=(byte)(result&0xFF); p2=(byte)(result>>taS);
					if((position[p2]==bB||position[p2]==bQ)&&p1==enPassantState[depth]-8) return -1;
				}
				if((slideRayLD[p, data]&B)!=0) c++;
				if(enPassantState[depth]!=NS) {
					result=slideHitRU[p, data]; p1=(byte)(result&0xFF); p2=(byte)(result>>taS);
					if((position[p2]==bB||position[p2]==bQ)&&p1==enPassantState[depth]-8) return -1;
				}
				if((slideRayRD[p, data=(int)(occuBS>>occuShiftBS[p]&0x3F)]&B)!=0) c++;
				if(enPassantState[depth]!=NS) {
					result=slideHitRU[p, data]; p1=(byte)(result&0xFF); p2=(byte)(result>>taS);
					if((position[p2]==bB||position[p2]==bQ)&&p1==enPassantState[depth]-8) return -1;
				}
				if((slideRayLU[p, data]&B)!=0) c++;
				if(enPassantState[depth]!=NS) {
					result=slideHitLU[p, data]; p1=(byte)(result&0xFF); p2=(byte)(result>>taS);
					if((position[p2]==bB||position[p2]==bQ)&&p1==enPassantState[depth]-8) return -1;
				}
				
			} else {
				if((pieceRangeK[p]&piecePos[wK])!=0) return -1;								// 如果發現國王，直接傳回錯誤	
				A=piecePos[wR]|piecePos[wQ]; B=piecePos[wB]|piecePos[wQ];
				for(i=0;(p1=pieceRuleBP[p, i])!=NS;i++) if(position[p1]==wP) c++;			// 小兵跟騎士的將軍因為要計算數目，不能用位元棋盤
				for(i=0;(p1=pieceRuleN[p, i])!=NS;i++) if(position[p1]==wN) c++;
				if(c>=2) return -1;															// 兵或騎士的雙將，這是不可能的

				// 縱橫檢查
				if((slideRayR[p, data=(int)(occuH>>occuShiftH[p]&0x3F)]&A)!=0) c++;
				if((slideRayL[p, data]&A)!=0) c++;
				if((slideRayU[p, data=(int)(occuV>>occuShiftV[p]&0x3F)]&A)!=0) c++;
				if((slideRayD[p, data]&A)!=0) c++;

				// 斜向檢查，其中有額外的檢查避免日後出現不可能的「吃過路兵導致被將」				
				if((slideRayRU[p, data=(int)(occuFS>>occuShiftFS[p]&0x3F)]&B)!=0) c++;
				if(enPassantState[depth]!=NS) {
					result=slideHitRU[p, data]; p1=(byte)(result&0xFF); p2=(byte)(result>>taS);
					if((position[p2]==wB||position[p2]==wQ)&&p1==enPassantState[depth]+8) return -1;
				}
				if((slideRayLD[p, data]&B)!=0) c++;
				if(enPassantState[depth]!=NS) {
					result=slideHitLD[p, data]; p1=(byte)(result&0xFF); p2=(byte)(result>>taS);
					if((position[p2]==wB||position[p2]==wQ)&&p1==enPassantState[depth]+8) return -1;
				}
				if((slideRayRD[p, data=(int)(occuBS>>occuShiftBS[p]&0x3F)]&B)!=0) c++;
				if(enPassantState[depth]!=NS) {
					result=slideHitRD[p, data]; p1=(byte)(result&0xFF); p2=(byte)(result>>taS);
					if((position[p2]==wB||position[p2]==wQ)&&p1==enPassantState[depth]+8) return -1;
				}
				if((slideRayLU[p, data]&B)!=0) c++;
				if(enPassantState[depth]!=NS) {
					result=slideHitLU[p, data]; p1=(byte)(result&0xFF); p2=(byte)(result>>taS);
					if((position[p2]==wB||position[p2]==wQ)&&p1==enPassantState[depth]+8) return -1;
				}
			}
			return c;
		}

		/////////////////////////////////
		// 行棋函數
		/////////////////////////////////

		public void play(int i) { play(i, 0);}		
		public void play(int i, int len) {
			ulong m=moveList[depth, i];
			moveHis[depth]=m;
			byte so=(byte)(m&0x3F);
			byte ta=(byte)((m>>taS)&0x3F);
			byte de=(byte)((m>>deS)&0x3F);
			byte ot=(byte)((m>>otS)&0xF);
			byte nt=(byte)((m>>ntS)&0xF);
			byte cp=(byte)((m>>cpS)&0xF);
			byte mi=(byte)((m>>miS)&0xF);
			
			if(cp!=0) {
				piecePos[cp]^=mask[de]; pieceCount[cp]--;
				if(mi==epMove) {
					position[de]=b0;
					occuH^=mask[de]; occuV^=maskV[de];
					occuFS^=maskFS[de]; occuBS^=maskBS[de];
				}
			}
			if(cp==0||mi==epMove) {
				occuH|=mask[ta]; occuV|=maskV[ta];
				occuFS|=maskFS[ta]; occuBS|=maskBS[ta];
			}
			if(ot!=nt) { pieceCount[ot]--; pieceCount[nt]++;}
			position[so]=b0; position[ta]=nt;
			piecePos[ot]^=mask[so]; piecePos[nt]|=mask[ta];
			occuH^=mask[so]; occuV^=maskV[so];
			occuFS^=maskFS[so]; occuBS^=maskBS[so];			
			
			if(ot==wK||ot==bK) {
				kingPos[ot>>3]=ta;
				if(mi==OOMove) {
					if(ot==wK) {
						position[7]=0; position[5]=wR; piecePos[wR]^=0xA0;
						occuH^=0xA0; occuV^=0x100010000000000; occuFS^=0x10001; occuBS^=0x100010000000000;
					} else {
						position[63]=0; position[61]=bR; piecePos[bR]^=0xA000000000000000;
						occuH^=0xA000000000000000; occuV^=0x8000800000000000; occuFS^=0x8000000000008000; occuBS^=0x80008000000000;
					}
				} else if(mi==OOOMove) {
					if(ot==wK) {
						position[0]=0; position[3]=wR; piecePos[wR]^=0x9;
						occuH^=0x9; occuV^=0x1000001; occuFS^=0x100000100000000; occuBS^=0x1000001;
					} else {
						position[56]=0; position[59]=bR; piecePos[bR]^=0x900000000000000;
						occuH^=0x900000000000000; occuV^=0x80000080; occuFS^=0x80000080000000; occuBS^=0x8000000000800000;
					}
				}
			}

			depth++;
			castlingState[depth]=castlingState[depth-1];

			// 入堡狀態更新
			if(ot==wK) castlingState[depth]&=b3;
			if(ot==bK) castlingState[depth]&=b12;
			if((ot==wR&&so==b7)||(cp==wR&&de==b7)) castlingState[depth]&=b7;
			if((ot==wR&&so==b0)||(cp==wR&&de==b0)) castlingState[depth]&=b11;
			if((ot==bR&&so==b63)||(cp==bR&&de==b63)) castlingState[depth]&=b13;
			if((ot==bR&&so==b56)||(cp==bR&&de==b56)) castlingState[depth]&=b14;

			// 吃過路兵狀態更新
			if(ot==wP&&ta-so==b16) enPassantState[depth]=(byte)(so+8);
			else if(ot==bP&&so-ta==b16) enPassantState[depth]=(byte)(so-8);
			else enPassantState[depth]=NS;

			// 其餘狀態更新
			if(ot==wP||ot==bP||cp!=0) halfmoveClock[depth]=b0; else halfmoveClock[depth]=(byte)(halfmoveClock[depth-1]+1);
			whoseMove=(byte)(1-whoseMove);
			if(whoseMove==WT) fullmoveClock++;
			
			if(len==2) computeLegalMoves2();
			else if(len==3) computeLegalMoves3();
			else if(len==4) computeLegalMoves4();
			else if(len==5) computeLegalMoves5();
			else if(len==6) computeLegalMoves6();
			else if(len==7) computeLegalMoves7();
			else computeLegalMoves();
			
			positionDataReady=false;
		}
		public void retract() {
			if(depth==0) return;
			ulong m=moveHis[depth-1];
			byte so=(byte)(m&0x3F);
			byte ta=(byte)((m>>taS)&0x3F);
			byte de=(byte)((m>>deS)&0x3F);
			byte ot=(byte)((m>>otS)&0xF);
			byte nt=(byte)((m>>ntS)&0xF);
			byte cp=(byte)((m>>cpS)&0xF);
			byte mi=(byte)((m>>miS)&0xF);

			position[so]=ot;
			piecePos[ot]|=mask[so]; piecePos[nt]^=mask[ta];
			occuH|=mask[so]; occuV|=maskV[so];
			occuFS|=maskFS[so]; occuBS|=maskBS[so];

			if(cp!=0) { position[de]=cp; piecePos[cp]|=mask[de]; pieceCount[cp]++;}
			if(cp==0||mi==epMove) {
				position[ta]=b0;
				occuH^=mask[ta]; occuV^=maskV[ta];
				occuFS^=maskFS[ta]; occuBS^=maskBS[ta];
			}
			if(ot!=nt) { pieceCount[nt]--; pieceCount[ot]++;}
			
			if(ot==wK||ot==bK) {
				kingPos[ot>>3]=so;
				if(mi==OOMove) {
					if(ot==wK) {
						position[7]=wR; position[5]=0; piecePos[wR]^=0xA0;
						occuH^=0xA0; occuV^=0x100010000000000; occuFS^=0x10001; occuBS^=0x100010000000000;
					} else {
						position[63]=bR; position[61]=0; piecePos[bR]^=0xA000000000000000;
						occuH^=0xA000000000000000; occuV^=0x8000800000000000; occuFS^=0x8000000000008000; occuBS^=0x80008000000000;
					}
				} else if(mi==OOOMove) {
					if(ot==wK) {
						position[0]=wR; position[3]=0; piecePos[wR]^=0x9;
						occuH^=0x9; occuV^=0x1000001; occuFS^=0x100000100000000; occuBS^=0x1000001;
					} else {
						position[56]=bR; position[59]=0; piecePos[bR]^=0x900000000000000;
						occuH^=0x900000000000000; occuV^=0x80000080; occuFS^=0x80000080000000; occuBS^=0x8000000000800000;
					}
				}
			}

			depth--;
			whoseMove=(byte)(1-whoseMove);
			if(whoseMove==BC) fullmoveClock--;
			positionDataReady=false;
		}

		/////////////////////////////////
		// 調換表
		/////////////////////////////////

		public const int posDataSize=35;	// 局面資料的大小

		private byte[] _positionData=new byte[posDataSize];
		private bool positionDataReady=false;

		public byte[] positionData {
			get {
				int i;
				if(!positionDataReady) {
					for(i=0;i<32;i++) _positionData[i]=(byte)(position[i<<1]|(position[(i<<1)|1]<<4));
					// 因為 C# 沒有只使用 4bit 的資料型態，所以這邊怎樣都不可能用 Buffer.BlockCopy，只能跑迴圈
					// 嚴格來說記錄 13^64 種狀態只需要 30 byte，但那樣節省記憶體沒有意義，徒增運算量
					_positionData[32]=castlingState[depth];
					_positionData[33]=enPassantState[depth];
					_positionData[34]=depth;
					positionDataReady=true;
				}
				return _positionData;
			}
		}
	}
}
