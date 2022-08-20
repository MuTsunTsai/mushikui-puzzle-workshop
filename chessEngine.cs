
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;	// 轉換十六進位字元

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
			public sq[] rule;
			
			public sq[,][]		move;
			public sq[,][][]	ray;
					
			public pRule(int T, sq[] S) {
				type=T; rule=S; move=new sq[8, 8][]; ray=new sq[8, 8][][];
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

		// 棋子純粹移動範圍 bitBoard
		private ulong[,,] pieceRange={
			{
				{0,0,0,0,0,0,0,0},{0,0,0,0,0,0,0,0},{0,0,0,0,0,0,0,0},{0,0,0,0,0,0,0,0},
				{0,0,0,0,0,0,0,0},{0,0,0,0,0,0,0,0},{0,0,0,0,0,0,0,0},{0,0,0,0,0,0,0,0}
			},
			//wP
			{
				{0x0,0x20000,0x2000000,0x200000000,0x20000000000,0x2000000000000,0x200000000000000,0x0},
				{0x0,0x50000,0x5000000,0x500000000,0x50000000000,0x5000000000000,0x500000000000000,0x0},
				{0x0,0xA0000,0xA000000,0xA00000000,0xA0000000000,0xA000000000000,0xA00000000000000,0x0},
				{0x0,0x140000,0x14000000,0x1400000000,0x140000000000,0x14000000000000,0x1400000000000000,0x0},
				{0x0,0x280000,0x28000000,0x2800000000,0x280000000000,0x28000000000000,0x2800000000000000,0x0},
				{0x0,0x500000,0x50000000,0x5000000000,0x500000000000,0x50000000000000,0x5000000000000000,0x0},
				{0x0,0xA00000,0xA0000000,0xA000000000,0xA00000000000,0xA0000000000000,0xA000000000000000,0x0},
				{0x0,0x400000,0x40000000,0x4000000000,0x400000000000,0x40000000000000,0x4000000000000000,0x0}
			},
			//wR
			{
				{0x1010101010101FE,0x10101010101FE01,0x101010101FE0101,0x1010101FE010101,0x10101FE01010101,0x101FE0101010101,0x1FE010101010101,0xFE01010101010101},
				{0x2020202020202FD,0x20202020202FD02,0x202020202FD0202,0x2020202FD020202,0x20202FD02020202,0x202FD0202020202,0x2FD020202020202,0xFD02020202020202},
				{0x4040404040404FB,0x40404040404FB04,0x404040404FB0404,0x4040404FB040404,0x40404FB04040404,0x404FB0404040404,0x4FB040404040404,0xFB04040404040404},
				{0x8080808080808F7,0x80808080808F708,0x808080808F70808,0x8080808F7080808,0x80808F708080808,0x808F70808080808,0x8F7080808080808,0xF708080808080808},
				{0x10101010101010EF,0x101010101010EF10,0x1010101010EF1010,0x10101010EF101010,0x101010EF10101010,0x1010EF1010101010,0x10EF101010101010,0xEF10101010101010},
				{0x20202020202020DF,0x202020202020DF20,0x2020202020DF2020,0x20202020DF202020,0x202020DF20202020,0x2020DF2020202020,0x20DF202020202020,0xDF20202020202020},
				{0x40404040404040BF,0x404040404040BF40,0x4040404040BF4040,0x40404040BF404040,0x404040BF40404040,0x4040BF4040404040,0x40BF404040404040,0xBF40404040404040},
				{0x808080808080807F,0x8080808080807F80,0x80808080807F8080,0x808080807F808080,0x8080807F80808080,0x80807F8080808080,0x807F808080808080,0x7F80808080808080}
			},
			//wN
			{
				{0x20400,0x2040004,0x204000402,0x20400040200,0x2040004020000,0x204000402000000,0x400040200000000,0x4020000000000},
				{0x50800,0x5080008,0x508000805,0x50800080500,0x5080008050000,0x508000805000000,0x800080500000000,0x8050000000000},
				{0xA1100,0xA110011,0xA1100110A,0xA1100110A00,0xA1100110A0000,0xA1100110A000000,0x1100110A00000000,0x110A0000000000},
				{0x142200,0x14220022,0x1422002214,0x142200221400,0x14220022140000,0x1422002214000000,0x2200221400000000,0x22140000000000},
				{0x284400,0x28440044,0x2844004428,0x284400442800,0x28440044280000,0x2844004428000000,0x4400442800000000,0x44280000000000},
				{0x508800,0x50880088,0x5088008850,0x508800885000,0x50880088500000,0x5088008850000000,0x8800885000000000,0x88500000000000},
				{0xA01000,0xA0100010,0xA0100010A0,0xA0100010A000,0xA0100010A00000,0xA0100010A0000000,0x100010A000000000,0x10A00000000000},
				{0x402000,0x40200020,0x4020002040,0x402000204000,0x40200020400000,0x4020002040000000,0x2000204000000000,0x20400000000000}
			},
			//wB
			{
				{0x8040201008040200,0x4020100804020002,0x2010080402000204,0x1008040200020408,0x804020002040810,0x402000204081020,0x200020408102040,0x2040810204080},
				{0x80402010080500,0x8040201008050005,0x4020100805000508,0x2010080500050810,0x1008050005081020,0x805000508102040,0x500050810204080,0x5081020408000},
				{0x804020110A00,0x804020110A000A,0x804020110A000A11,0x4020110A000A1120,0x20110A000A112040,0x110A000A11204080,0xA000A1120408000,0xA112040800000},
				{0x8041221400,0x804122140014,0x80412214001422,0x8041221400142241,0x4122140014224180,0x2214001422418000,0x1400142241800000,0x14224180000000},
				{0x182442800,0x18244280028,0x1824428002844,0x182442800284482,0x8244280028448201,0x4428002844820100,0x2800284482010000,0x28448201000000},
				{0x10204885000,0x1020488500050,0x102048850005088,0x204885000508804,0x488500050880402,0x8850005088040201,0x5000508804020100,0x50880402010000},
				{0x102040810A000,0x102040810A000A0,0x2040810A000A010,0x40810A000A01008,0x810A000A0100804,0x10A000A010080402,0xA000A01008040201,0xA0100804020100},
				{0x102040810204000,0x204081020400040,0x408102040004020,0x810204000402010,0x1020400040201008,0x2040004020100804,0x4000402010080402,0x40201008040201}
			},
			//wQ
			{
				{0x81412111090503FE,0x412111090503FE03,0x2111090503FE0305,0x11090503FE030509,0x90503FE03050911,0x503FE0305091121,0x3FE030509112141,0xFE03050911214181},
				{0x2824222120A07FD,0x824222120A07FD07,0x4222120A07FD070A,0x22120A07FD070A12,0x120A07FD070A1222,0xA07FD070A122242,0x7FD070A12224282,0xFD070A1222428202},
				{0x404844424150EFB,0x4844424150EFB0E,0x844424150EFB0E15,0x4424150EFB0E1524,0x24150EFB0E152444,0x150EFB0E15244484,0xEFB0E1524448404,0xFB0E152444840404},
				{0x8080888492A1CF7,0x80888492A1CF71C,0x888492A1CF71C2A,0x88492A1CF71C2A49,0x492A1CF71C2A4988,0x2A1CF71C2A498808,0x1CF71C2A49880808,0xF71C2A4988080808},
				{0x10101011925438EF,0x101011925438EF38,0x1011925438EF3854,0x11925438EF385492,0x925438EF38549211,0x5438EF3854921110,0x38EF385492111010,0xEF38549211101010},
				{0x2020212224A870DF,0x20212224A870DF70,0x212224A870DF70A8,0x2224A870DF70A824,0x24A870DF70A82422,0xA870DF70A8242221,0x70DF70A824222120,0xDF70A82422212020},
				{0x404142444850E0BF,0x4142444850E0BFE0,0x42444850E0BFE050,0x444850E0BFE05048,0x4850E0BFE0504844,0x50E0BFE050484442,0xE0BFE05048444241,0xBFE0504844424140},
				{0x8182848890A0C07F,0x82848890A0C07FC0,0x848890A0C07FC0A0,0x8890A0C07FC0A090,0x90A0C07FC0A09088,0xA0C07FC0A0908884,0xC07FC0A090888482,0x7FC0A09088848281}
			},
			//wK，包含自己的格子，以及如果在初始格子的時候往左右多追加一格
			{
				{0x303,0x30303,0x3030300,0x303030000,0x30303000000,0x3030300000000,0x303030000000000,0x303000000000000},
				{0x707,0x70707,0x7070700,0x707070000,0x70707000000,0x7070700000000,0x707070000000000,0x707000000000000},
				{0xE0E,0xE0E0E,0xE0E0E00,0xE0E0E0000,0xE0E0E000000,0xE0E0E00000000,0xE0E0E0000000000,0xE0E000000000000},
				{0x1C1C,0x1C1C1C,0x1C1C1C00,0x1C1C1C0000,0x1C1C1C000000,0x1C1C1C00000000,0x1C1C1C0000000000,0x1C1C000000000000},
				{0x387C,0x383838,0x38383800,0x3838380000,0x383838000000,0x38383800000000,0x3838380000000000,0x3838000000000000},
				{0x7070,0x707070,0x70707000,0x7070700000,0x707070000000,0x70707000000000,0x7070700000000000,0x7070000000000000},
				{0xE0E0,0xE0E0E0,0xE0E0E000,0xE0E0E00000,0xE0E0E0000000,0xE0E0E000000000,0xE0E0E00000000000,0xE0E0000000000000},
				{0xC0C0,0xC0C0C0,0xC0C0C000,0xC0C0C00000,0xC0C0C0000000,0xC0C0C000000000,0xC0C0C00000000000,0xC0C0000000000000}
			},
			//bP
			{
				{0x0,0x2,0x200,0x20000,0x2000000,0x200000000,0x20000000000,0x0},
				{0x0,0x5,0x500,0x50000,0x5000000,0x500000000,0x50000000000,0x0},
				{0x0,0xA,0xA00,0xA0000,0xA000000,0xA00000000,0xA0000000000,0x0},
				{0x0,0x14,0x1400,0x140000,0x14000000,0x1400000000,0x140000000000,0x0},
				{0x0,0x28,0x2800,0x280000,0x28000000,0x2800000000,0x280000000000,0x0},
				{0x0,0x50,0x5000,0x500000,0x50000000,0x5000000000,0x500000000000,0x0},
				{0x0,0xA0,0xA000,0xA00000,0xA0000000,0xA000000000,0xA00000000000,0x0},
				{0x0,0x40,0x4000,0x400000,0x40000000,0x4000000000,0x400000000000,0x0}
			},
			//bR
			{
				{0x1010101010101FE,0x10101010101FE01,0x101010101FE0101,0x1010101FE010101,0x10101FE01010101,0x101FE0101010101,0x1FE010101010101,0xFE01010101010101},
				{0x2020202020202FD,0x20202020202FD02,0x202020202FD0202,0x2020202FD020202,0x20202FD02020202,0x202FD0202020202,0x2FD020202020202,0xFD02020202020202},
				{0x4040404040404FB,0x40404040404FB04,0x404040404FB0404,0x4040404FB040404,0x40404FB04040404,0x404FB0404040404,0x4FB040404040404,0xFB04040404040404},
				{0x8080808080808F7,0x80808080808F708,0x808080808F70808,0x8080808F7080808,0x80808F708080808,0x808F70808080808,0x8F7080808080808,0xF708080808080808},
				{0x10101010101010EF,0x101010101010EF10,0x1010101010EF1010,0x10101010EF101010,0x101010EF10101010,0x1010EF1010101010,0x10EF101010101010,0xEF10101010101010},
				{0x20202020202020DF,0x202020202020DF20,0x2020202020DF2020,0x20202020DF202020,0x202020DF20202020,0x2020DF2020202020,0x20DF202020202020,0xDF20202020202020},
				{0x40404040404040BF,0x404040404040BF40,0x4040404040BF4040,0x40404040BF404040,0x404040BF40404040,0x4040BF4040404040,0x40BF404040404040,0xBF40404040404040},
				{0x808080808080807F,0x8080808080807F80,0x80808080807F8080,0x808080807F808080,0x8080807F80808080,0x80807F8080808080,0x807F808080808080,0x7F80808080808080}
			},
			//bN
			{
				{0x20400,0x2040004,0x204000402,0x20400040200,0x2040004020000,0x204000402000000,0x400040200000000,0x4020000000000},
				{0x50800,0x5080008,0x508000805,0x50800080500,0x5080008050000,0x508000805000000,0x800080500000000,0x8050000000000},
				{0xA1100,0xA110011,0xA1100110A,0xA1100110A00,0xA1100110A0000,0xA1100110A000000,0x1100110A00000000,0x110A0000000000},
				{0x142200,0x14220022,0x1422002214,0x142200221400,0x14220022140000,0x1422002214000000,0x2200221400000000,0x22140000000000},
				{0x284400,0x28440044,0x2844004428,0x284400442800,0x28440044280000,0x2844004428000000,0x4400442800000000,0x44280000000000},
				{0x508800,0x50880088,0x5088008850,0x508800885000,0x50880088500000,0x5088008850000000,0x8800885000000000,0x88500000000000},
				{0xA01000,0xA0100010,0xA0100010A0,0xA0100010A000,0xA0100010A00000,0xA0100010A0000000,0x100010A000000000,0x10A00000000000},
				{0x402000,0x40200020,0x4020002040,0x402000204000,0x40200020400000,0x4020002040000000,0x2000204000000000,0x20400000000000}
			},
			//bB
			{
				{0x8040201008040200,0x4020100804020002,0x2010080402000204,0x1008040200020408,0x804020002040810,0x402000204081020,0x200020408102040,0x2040810204080},
				{0x80402010080500,0x8040201008050005,0x4020100805000508,0x2010080500050810,0x1008050005081020,0x805000508102040,0x500050810204080,0x5081020408000},
				{0x804020110A00,0x804020110A000A,0x804020110A000A11,0x4020110A000A1120,0x20110A000A112040,0x110A000A11204080,0xA000A1120408000,0xA112040800000},
				{0x8041221400,0x804122140014,0x80412214001422,0x8041221400142241,0x4122140014224180,0x2214001422418000,0x1400142241800000,0x14224180000000},
				{0x182442800,0x18244280028,0x1824428002844,0x182442800284482,0x8244280028448201,0x4428002844820100,0x2800284482010000,0x28448201000000},
				{0x10204885000,0x1020488500050,0x102048850005088,0x204885000508804,0x488500050880402,0x8850005088040201,0x5000508804020100,0x50880402010000},
				{0x102040810A000,0x102040810A000A0,0x2040810A000A010,0x40810A000A01008,0x810A000A0100804,0x10A000A010080402,0xA000A01008040201,0xA0100804020100},
				{0x102040810204000,0x204081020400040,0x408102040004020,0x810204000402010,0x1020400040201008,0x2040004020100804,0x4000402010080402,0x40201008040201}
			},
			//bQ
			{
				{0x81412111090503FE,0x412111090503FE03,0x2111090503FE0305,0x11090503FE030509,0x90503FE03050911,0x503FE0305091121,0x3FE030509112141,0xFE03050911214181},
				{0x2824222120A07FD,0x824222120A07FD07,0x4222120A07FD070A,0x22120A07FD070A12,0x120A07FD070A1222,0xA07FD070A122242,0x7FD070A12224282,0xFD070A1222428202},
				{0x404844424150EFB,0x4844424150EFB0E,0x844424150EFB0E15,0x4424150EFB0E1524,0x24150EFB0E152444,0x150EFB0E15244484,0xEFB0E1524448404,0xFB0E152444840404},
				{0x8080888492A1CF7,0x80888492A1CF71C,0x888492A1CF71C2A,0x88492A1CF71C2A49,0x492A1CF71C2A4988,0x2A1CF71C2A498808,0x1CF71C2A49880808,0xF71C2A4988080808},
				{0x10101011925438EF,0x101011925438EF38,0x1011925438EF3854,0x11925438EF385492,0x925438EF38549211,0x5438EF3854921110,0x38EF385492111010,0xEF38549211101010},
				{0x2020212224A870DF,0x20212224A870DF70,0x212224A870DF70A8,0x2224A870DF70A824,0x24A870DF70A82422,0xA870DF70A8242221,0x70DF70A824222120,0xDF70A82422212020},
				{0x404142444850E0BF,0x4142444850E0BFE0,0x42444850E0BFE050,0x444850E0BFE05048,0x4850E0BFE0504844,0x50E0BFE050484442,0xE0BFE05048444241,0xBFE0504844424140},
				{0x8182848890A0C07F,0x82848890A0C07FC0,0x848890A0C07FC0A0,0x8890A0C07FC0A090,0x90A0C07FC0A09088,0xA0C07FC0A0908884,0xC07FC0A090888482,0x7FC0A09088848281}
			},
			//bK，包含自己的格子，以及如果在初始格子的時候往左右多追加一格
			{
				{0x303,0x30303,0x3030300,0x303030000,0x30303000000,0x3030300000000,0x303030000000000,0x303000000000000},
				{0x707,0x70707,0x7070700,0x707070000,0x70707000000,0x7070700000000,0x707070000000000,0x707000000000000},
				{0xE0E,0xE0E0E,0xE0E0E00,0xE0E0E0000,0xE0E0E000000,0xE0E0E00000000,0xE0E0E0000000000,0xE0E000000000000},
				{0x1C1C,0x1C1C1C,0x1C1C1C00,0x1C1C1C0000,0x1C1C1C000000,0x1C1C1C00000000,0x1C1C1C0000000000,0x1C1C000000000000},
				{0x3838,0x383838,0x38383800,0x3838380000,0x383838000000,0x38383800000000,0x3838380000000000,0x7C38000000000000},
				{0x7070,0x707070,0x70707000,0x7070700000,0x707070000000,0x70707000000000,0x7070700000000000,0x7070000000000000},
				{0xE0E0,0xE0E0E0,0xE0E0E000,0xE0E0E00000,0xE0E0E0000000,0xE0E0E000000000,0xE0E0E00000000000,0xE0E0000000000000},
				{0xC0C0,0xC0C0C0,0xC0C0C000,0xC0C0C00000,0xC0C0C0000000,0xC0C0C000000000,0xC0C0C00000000000,0xC0C0000000000000}
			}
		};


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

			generateRuleData();

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
		private void generateRuleData() {
			int x, y, k, i, c;
			for(k=wP;k<=bK;k++) for(x=0;x<8;x++) for(y=0;y<8;y++) {
				if(pieceRule[k].type==0) {
					c=0; foreach(sq d in pieceRule[k].rule) if(inBoard(x+d.x, y+d.y)) c++;
					pieceRule[k].move[x,y]=new sq[c];
					c=0; foreach(sq d in pieceRule[k].rule) if(inBoard(x+d.x, y+d.y)) pieceRule[k].move[x,y][c++].set(x+d.x, y+d.y);
				} else {
					c=0; foreach(sq d in pieceRule[k].rule) if(inBoard(x+d.x, y+d.y)) c++;
					pieceRule[k].ray[x, y]=new sq[c][];
					c=0; foreach(sq d in pieceRule[k].rule) if(inBoard(x+d.x, y+d.y)) {
						for(i=1;i<8&&inBoard(x+i*d.x, y+i*d.y);i++);
						pieceRule[k].ray[x, y][c]=new sq[i-1];
						for(i=1;i<8&&inBoard(x+i*d.x, y+i*d.y);i++) pieceRule[k].ray[x, y][c][i-1].set(x+i*d.x, y+i*d.y);
						c++;
					}
				}
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
		private int checkState(int side) {					// 將軍判斷，現在這個函數只有在載入的時候使用
			int sx=kingPos[side].x, sy=kingPos[side].y, p, i, c=0;
			if(side==1) {
				foreach(sq d in pieceRule[wP].move[sx, sy]) if(position[d.x, d.y]==bP) c++;		// 反方向的時候記得要採用另一方的兵的資料
				foreach(sq d in pieceRule[bN].move[sx, sy]) if(position[d.x, d.y]==bN) c++;
				foreach(sq d in pieceRule[bK].move[sx, sy]) if(position[d.x, d.y]==bK) c++;
				foreach(sq[] r in pieceRule[bB].ray[sx, sy])
					for(i=0;i<r.Length;i++) {
						p=position[r[i].x, r[i].y];
						if(p!=0) {
							if(p==bB||p==bQ||(i==0&&p==bK)) c++;
							if(p==bP&&enPassantState.x==r[i].x&&enPassantState.y==r[i].y+1) {	// 斜方向遇到對方的兵要多做一個檢查，
								for(i++;i<r.Length;i++) {										// 否則未來會遇到「吃過路兵導致被對方將軍」這種實際上不可能發生的狀況。
									p=position[r[i].x, r[i].y];
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
				foreach(sq[] r in pieceRule[bR].ray[sx,sy])
					foreach(sq d in r) {
						p=position[d.x, d.y];
						if(p==0) continue;
						else { if(p==bR||p==bQ) c++; break; }
					}
			} else {
				foreach(sq d in pieceRule[bP].move[sx, sy]) if(position[d.x, d.y]==wP) c++;		// 反方向的時候記得要採用另一方的兵的資料
				foreach(sq d in pieceRule[wN].move[sx, sy]) if(position[d.x, d.y]==wN) c++;
				foreach(sq d in pieceRule[wK].move[sx, sy]) if(position[d.x, d.y]==wK) c++;
				foreach(sq[] r in pieceRule[wB].ray[sx,sy])
					for(i=0;i<r.Length;i++) {
						p=position[r[i].x, r[i].y];
						if(p!=0) {
							if(p==wB||p==wQ||(i==0&&p==wK)) c++;
							if(p==wP&&enPassantState.x==r[i].x&&enPassantState.y==r[i].y-1) {	// 斜方向遇到對方的兵要多做一個檢查，
								for(i++;i<r.Length;i++) {										// 否則未來會遇到「吃過路兵導致被對方將軍」這種實際上不可能發生的狀況。
									p=position[r[i].x, r[i].y];
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
				foreach(sq[] r in pieceRule[wR].ray[sx,sy])
					foreach(sq d in r) {
						p=position[d.x, d.y];
						if(p==0) continue;
						else { if(p==wR||p==wQ) c++; break; }
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

#if DEBUG
		public int CT0=0, CT1=0, CT2=0;
#endif

		private bool computeLegalMoves() {
			move[] L=new move[maxVar];
			int[] tag=new int[maxVar];
			int sx, sy, i, j=0, k, l=0;
			bool w;
			
			generateBoardData();

			for(sy=7;sy>=0;sy--) for(sx=0;sx<8;sx++) {
				k=position[sx,sy];

				// 如果棋子是敵營的，生成 attackByOpp 資料
				if(side(k)==1-whoseMove&&(pieceRange[k,sx,sy]&
					pieceRange[oK,kingPos[whoseMove].x,kingPos[whoseMove].y])!=(ulong)0) {		// 如果其移動範圍根本打從一開始跟國王附近沒有交集，就不用理會
					if(pieceRule[k].type==0) {
						foreach(sq d in pieceRule[k].move[sx, sy]) {
							attackByOpp[d.x, d.y]=true;
#if DEBUG
							CT0++;
#endif
						}
					} else {
						foreach(sq[] r in pieceRule[k].ray[sx, sy])
							for(i=0;i<r.Length;i++) {
#if DEBUG
								CT0++;
#endif							
								attackByOpp[r[i].x, r[i].y]=true;
								if(position[r[i].x, r[i].y]==oK&&i<r.Length-1)		// 如果碰到我方國王，把下一格也列入攻擊範圍（以免國王往反方向跑）
									attackByOpp[r[i+1].x, r[i+1].y]=true;			// 很容易忽略的程式設計盲點！
								if(position[r[i].x, r[i].y]!=0) break;
							}
					}
				}
				
				// 如果棋子是自己這一方的
				if(side(k)==whoseMove) {
								
					// 小兵棋步
					if(k==oP) {
						w=(whoseMove==1);
						if(position[sx, sy+(w?1:-1)]==0) {
							if(sy==(w?6:1)) for(j=oR;j<oK;j++) L[l++]=new move(sx, sy, sx, sy+(w?1:-1), 0, j);
							else L[l++]=new move(sx, sy, sx, sy+(w?1:-1), 0, 0);
							if(sy==(w?1:6)&&position[sx, sy+(w?2:-2)]==0) L[l++]=new move(sx, sy, sx, sy+(w?2:-2), 0, 0);
#if DEBUG
							CT1++;
#endif
						}
						foreach(sq d in pieceRule[k].move[sx, sy]) {
							if(side(position[d.x, d.y])==1-whoseMove) {
								if(sy==(w?6:1)) for(j=oR;j<oK;j++) L[l++]=new move(sx, sy, d.x, d.y, position[d.x, d.y], j);
								else L[l++]=new move(sx, sy, d.x, d.y, position[d.x, d.y], 0);
							}
							if(d.x==enPassantState.x&&d.y==enPassantState.y)
								L[l++]=new move(sx, sy, d.x, d.y, w?7:1, epMove);
#if DEBUG
							CT1++;
#endif
						}
					}
					
					// 入堡棋步，這邊只檢查入堡權、當下的將軍以及中間的格子是否空的，攻擊檢查待會再做
					if(k==oK&&checkPieceCount==0) {
						if((k==wK?castlingState.K:castlingState.k)&&position[5, sy]==0&&position[6, sy]==0)
							L[l++]=new move(4, sy, 6, sy, 0, OOMove);
						if((k==wK?castlingState.Q:castlingState.q)&&position[3, sy]==0&&position[2, sy]==0&&position[1, sy]==0)
							L[l++]=new move(4, sy, 2, sy, 0, OOOMove);
					}
					
					// 普通棋步（含國王的）
					if(k!=oP) {
						if(pieceRule[k].type==0) {
							foreach(sq d in pieceRule[k].move[sx, sy]) {
								if(side(position[d.x, d.y])!=whoseMove)
									L[l++]=new move(sx, sy, d.x, d.y, position[d.x, d.y], 0);
#if DEBUG
								CT1++;
#endif
							}
						} else {
							foreach(sq[] r in pieceRule[k].ray[sx, sy])
								foreach(sq d in r) {
#if DEBUG
									CT1++;
#endif
									if(position[d.x, d.y]==0)
										L[l++]=new move(sx, sy, d.x, d.y, 0, 0);
									else {
										if(side(position[d.x, d.y])!=whoseMove)
											L[l++]=new move(sx, sy, d.x, d.y, position[d.x, d.y], 0);
										break;
									}
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
			foreach(sq d in pieceRule[pP].move[sx, sy]) {
				canAttackOppKing[d.x, d.y, oP]=true;						// 記得要用另一方的兵
#if DEBUG
				CT2++;
#endif
			}
			foreach(sq d in pieceRule[oN].move[sx, sy]) {
				canAttackOppKing[d.x, d.y, oN]=true;
#if DEBUG
				CT2++;
#endif
			}
			foreach(sq[] r in pieceRule[oB].ray[sx, sy])
				for(i=0;i<r.Length;i++) {
#if DEBUG
					CT2++;
#endif
					canAttackOppKing[r[i].x, r[i].y, oB]=true;
					canAttackOppKing[r[i].x, r[i].y, oQ]=true;
					if(position[r[i].x, r[i].y]!=0) {
						if(	side(position[r[i].x, r[i].y])==whoseMove||(position[r[i].x, r[i].y]==pP&&
							enPassantState.x==r[i].x&&enPassantState.y==r[i].y+(whoseMove==1?1:-1))) {				// 處理敵方國王的時候，斜向要考慮吃過路兵閃擊
							j=i;
							for(i++;i<r.Length;i++) {
#if DEBUG
								CT2++;
#endif
								if(position[r[i].x, r[i].y]==oB||position[r[i].x, r[i].y]==oQ) pinBySelf[r[j].x, r[j].y]=true;
								if(position[r[i].x, r[i].y]!=0) break;
							}
						}
						break;
					}
				}
			foreach(sq[] r in pieceRule[oR].ray[sx, sy])
				for(i=0;i<r.Length;i++) {
#if DEBUG
					CT2++;
#endif
					canAttackOppKing[r[i].x, r[i].y, oR]=true;
					canAttackOppKing[r[i].x, r[i].y, oQ]=true;
					if(position[r[i].x, r[i].y]!=0) {
						if(	side(position[r[i].x, r[i].y])==whoseMove||(r[0].y==sy&&position[r[i].x, r[i].y]==pP&&
							enPassantState.x==r[i].x&&enPassantState.y==r[i].y+(whoseMove==1?1:-1))) {
							j=i;
							if(r[0].y==sy&&i<r.Length-1) {											// 橫方向上需要再多做吃過路兵的一次閃兩子判別
								if(position[r[i].x, r[i].y]==oP&&position[r[i+1].x, r[i+1].y]==pP&&
									enPassantState.x==r[i+1].x&&enPassantState.y==r[i+1].y+(whoseMove==1?1:-1)) {
									for(i+=2;i<r.Length;i++) {
#if DEBUG
										CT2++;
#endif
										if(position[r[i].x, r[i].y]==oR||position[r[i].x, r[i].y]==oQ) pinBySelf[r[j+1].x, r[j+1].y]=true;
										if(position[r[i].x, r[i].y]!=0) break;
									}
								} else if(position[r[i].x, r[i].y]==pP&&position[r[i+1].x, r[i+1].y]==oP&&
									enPassantState.x==r[i].x&&enPassantState.y==r[i].y+(whoseMove==1?1:-1)) {
									for(i+=2;i<r.Length;i++) {
#if DEBUG
										CT2++;
#endif
										if(position[r[i].x, r[i].y]==oR||position[r[i].x, r[i].y]==oQ) pinBySelf[r[j].x, r[j].y]=true;
										if(position[r[i].x, r[i].y]!=0) break;
									}
								}
							} else {
								for(i++;i<r.Length;i++) {
#if DEBUG
									CT2++;
#endif
									if(position[r[i].x, r[i].y]==oR||position[r[i].x, r[i].y]==oQ) pinBySelf[r[j].x, r[j].y]=true;
									if(position[r[i].x, r[i].y]!=0) break;
								}
							}
						}
						break;
					}
				}

			// 處理我方國王
			sx=kingPos[whoseMove].x; sy=kingPos[whoseMove].y;
			foreach(sq d in pieceRule[oP].move[sx, sy]) if(position[d.x, d.y]==pP) {
#if DEBUG
				CT2++;
#endif
				checkPieceCount++; canStopCheck[d.x, d.y]=true;			// 記得反方向採用另一方的兵
			}
			foreach(sq d in pieceRule[pN].move[sx, sy]) if(position[d.x, d.y]==pN) {
#if DEBUG
				CT2++;
#endif
				checkPieceCount++; canStopCheck[d.x, d.y]=true;
			}
			foreach(sq[] r in pieceRule[pB].ray[sx, sy])
				for(i=0;i<r.Length;i++) {
					if(position[r[i].x, r[i].y]==pB||position[r[i].x, r[i].y]==pQ) {
#if DEBUG
						CT2++;
#endif
						checkPieceCount++;
						for(j=0;j<=i;j++) canStopCheck[r[j].x, r[j].y]=true;
					}
					if(position[r[i].x, r[i].y]!=0) {
						if(side(position[r[i].x, r[i].y])==whoseMove) {
							j=i;
							for(i++;i<r.Length;i++) {
#if DEBUG
								CT2++;
#endif
								if(position[r[i].x, r[i].y]==pB||position[r[i].x, r[i].y]==pQ) pinByOpp[r[j].x, r[j].y]=true;
								if(position[r[i].x, r[i].y]!=0) break;
							}
						}
						break;
					}
				}
			foreach(sq[] r in pieceRule[pR].ray[sx, sy])
				for(i=0;i<r.Length;i++) {
#if DEBUG
					CT2++;
#endif
					if(position[r[i].x, r[i].y]==pR||position[r[i].x, r[i].y]==pQ) {
						checkPieceCount++;
						for(j=0;j<=i;j++) canStopCheck[r[j].x, r[j].y]=true;
					}
					if(position[r[i].x, r[i].y]!=0) {
						if(side(position[r[i].x, r[i].y])==whoseMove||
							(r[0].y==sy&&position[r[i].x, r[i].y]==pP&&enPassantState.x==r[i].x&&enPassantState.y==r[i].y+(whoseMove==1?1:-1))) {
							j=i;
							if(r[0].y==sy&&i<r.Length-1) {											// 橫方向上需要再多做吃過路兵的一次閃兩子判別
								if(position[r[i].x, r[i].y]==oP&&position[r[i+1].x, r[i+1].y]==pP&&
									enPassantState.x==r[i+1].x&&enPassantState.y==r[i+1].y+(whoseMove==1?1:-1)) {
									for(i+=2;i<8&&inBoard(r[i].x, r[i].y);i++) {
#if DEBUG
										CT2++;
#endif
										if(position[r[i].x, r[i].y]==pR||position[r[i].x, r[i].y]==pQ) pinByOpp[r[j].x, r[j].y]=true;
										if(position[r[i].x, r[i].y]!=0) break;
									}
								} else if(position[r[i].x, r[i].y]==pP&&position[r[i+1].x, r[i+1].y]==oP&&
									enPassantState.x==r[i].x&&enPassantState.y==r[i].y+(whoseMove==1?1:-1)) {
									for(i+=2;i<8&&inBoard(r[i].x, r[i].y);i++) {
#if DEBUG
										CT2++;
#endif
										if(position[r[i].x, r[i].y]==pR||position[r[i].x, r[i].y]==pQ) pinByOpp[r[j+1].x, r[j+1].y]=true;
										if(position[r[i].x, r[i].y]!=0) break;
									}
								}
							} else {
								for(i++;i<r.Length;i++) {
#if DEBUG
									CT2++;
#endif
									if(position[r[i].x, r[i].y]==pR||position[r[i].x, r[i].y]==pQ) pinByOpp[r[j+1].x, r[j+1].y]=true;
									if(position[r[i].x, r[i].y]!=0) break;
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
				if(	position[m.sx, m.sy]!=oK||							// 駁回不是國王的移動
					attackByOpp[m.tx, m.ty])							// 如果目標格子被攻擊，駁回
					return 0;
				else return checkMoveCheck(m);							// 否則就表示合法

			
			// 單將軍的情況
			} else if(checkPieceCount==1) {
			
				if(position[m.sx, m.sy]==oK) {							// 如果動的是國王
					if(attackByOpp[m.tx, m.ty]) return 0;				// 閃到安全地帶就行了
					else return checkMoveCheck(m);
				} else if(canStopCheck[m.tx, m.ty]&&!checkPin(m)) {		// 如果阻止了對方的將軍也可以，但那個棋子不能被釘住
					return checkMoveCheck(m);
				} else return 0;										// 又不是動國王、又沒阻止將軍，一定不合法


			// 沒有將軍的情況
			} else {
				
				if(m.mi==OOMove) {										// 王側入堡的情況
					if(	attackByOpp[m.sx+1, m.sy]||
						attackByOpp[m.sx+2, m.sy]) return 0;			// 稍早已經檢查過其他要件了，這邊檢查國王路上會不會被攻擊就可以了
					else return checkMoveCheck(m);
				} else if(m.mi==OOOMove) {								// 后側入堡的情況，判斷方式一樣
					if(	attackByOpp[m.sx-1, m.sy]||
						attackByOpp[m.sx-2, m.sy]) return 0;	
					else return checkMoveCheck(m);
				} else if(position[m.sx, m.sy]==oK) {					// 如果移動的是國王
					if(attackByOpp[m.tx, m.ty])	return 0;				// 只要目的地不會被攻擊就好
					else return checkMoveCheck(m);
				} else {												// 如果以上狀況皆非，那只要檢查是否移動的棋子被釘住即可
					if(!checkPin(m)) return checkMoveCheck(m);
					else return 0;
				}				
			}
		}
		
		// 檢查移動棋子有沒有被對方釘住，傳回真表示有被釘住
		// 由於局面合法檢查的時候已經排除了「吃過路兵導致自己被將軍」情況，因此那個不用檢查
		// 吃過路兵的橫向一次兩子閃擊在稍早建立資料的時候已經涵蓋進去了
		private bool checkPin(move m) {
			return pinByOpp[m.sx, m.sy]&&(position[m.sx, m.sy]==oN||!checkParallel(m.tx-m.sx, m.ty-m.sy, m.sx-kingPos[whoseMove].x, m.sy-kingPos[whoseMove].y));
		}

		// 平行檢查，使用這個函數之前請先排除騎士的顯然不平行情況
		private bool checkParallel(int dx, int dy, int px, int py) {
			if((dx==0&&px==0)||(dy==0&&py==0)) return true;					// 縱橫向平行
			else if(dx==0||px==0||dy==0||py==0) return false;				// 至少一個為縱橫向但不平行
			else if((dy==dx&&py==px)||(dy==-dx&&py==-py)) return true;		// 斜向平行
			else return false;												// 不平行
		}
		
		// 已經通過合法檢查，進一步檢查這個棋步是否造成將軍對方
		private int checkMoveCheck(move m) {
			if((m.mi==OOMove||m.mi==OOOMove)&&canAttackOppKing[m.sx, m.sy, oR]) return 2;						// 入堡的情況要多做一種「入堡閃擊」的判斷
			else if(m.mi==OOMove&&canAttackOppKing[m.sx+1, m.sy, oR]) return 2;									// 一般的王側入堡將軍
			else if(m.mi==OOOMove&&canAttackOppKing[m.sx-1, m.sy, oR]) return 2;								// 一般的后側入堡將軍
			else if(pinBySelf[m.sx, m.sy]&&(position[m.sx, m.sy]==oN||!checkParallel(m.tx-m.sx, m.ty-m.sy,
					m.sx-kingPos[1-whoseMove].x, m.sy-kingPos[1-whoseMove].y))) return 2;						// 如果是閃擊（排除了一次閃兩子的橫向閃擊情況）					
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
