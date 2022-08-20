
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;

namespace Mushikui_Puzzle_Workshop {
	partial class chessEngine2 {

		/////////////////////////////////
		// 合法棋步計算
		/////////////////////////////////

		private ulong		attackByOpp;						// 一個格子是否正被對方攻擊（保護）著
		private ulong		canStopCheck;						// 假如把一個棋子移動到這邊，就可以解除將軍（擋住長程子力、或者吃掉將軍子）
		private ulong[]		canAttackOppKing=new ulong[16];		// 如果將一個特定種類的棋子移動到該格子上，是否可以攻擊到對方的國王
		private bool[]		pinByOpp=new bool[64];				// 一個格上（上的棋子）是否正在被對方釘著
		private bool[]		pinBySelf=new bool[64];				// 一個格子（上的棋子）是否正在被我方釘著（亦即移開該棋子可造成閃擊）

		private int			checkPieceCount;
		private bool		dblDis;								// 特殊的吃過路兵一次閃兩子閃擊
		private bool		dblPin;								// 特殊的吃過路兵一次閃兩子釘

		private byte[,,]	DisambList=new byte[64, 16, 16];
		private int[,]		DisambListLength=new int[64, 16];

		private ulong[]		TML=new ulong[maxVar];				// tempMoveList

		private byte oP, oR, oN, oB, oQ, oK;					// 我方棋子的代碼
		private byte pP, pR, pN, pB, pQ, pK;					// 敵方棋子的代碼

		private byte side(byte k) {
			if(k==b0) return 3;
			else return (byte)(k>>3);
		}
		private void computeLegalMoves() {
			int i, j=0, l=0, pData;
			byte k, p, d, r;			// 一些變數，可能會有混著亂用的情況，請多包涵
			byte ep;					// 過路兵的位置
			byte tag, so, ta, ot;
			int data, p1, p2, p3;
			ulong result;
			int cx, cy;

			// 設置代碼
			if(whoseMove==WT) { oP=wP; oR=wR; oN=wN; oB=wB; oQ=wQ; oK=wK; pP=bP; pR=bR; pN=bN; pB=bB; pQ=bQ; pK=bK;}
			else { oP=bP; oR=bR; oN=bN; oB=bB; oQ=bQ; oK=bK; pP=wP; pR=wR; pN=wN; pB=wB; pQ=wQ; pK=wK;}
			ep=(byte)(enPassantState[depth]==NS?NS:enPassantState[depth]+(whoseMove==WT?-8:8));

			// 資料歸零
			attackByOpp=canStopCheck=0;
			checkPieceCount=0;
			dblDis=dblPin=false;
			Array.Clear(canAttackOppKing, 0, 16);
			Array.Clear(pinByOpp, 0, 64);
			Array.Clear(pinBySelf, 0, 64);

			// 處理敵方國王
			p=kingPos[1-whoseMove];
			canAttackOppKing[oP]=(whoseMove==WT?pieceRangeBP[p]:pieceRangeWP[p]);			// 記得要用另一方的兵
			canAttackOppKing[oN]=pieceRangeN[p];
			
			// 斜向
			result=slideRayRU[p, data=(int)(occuFS>>occuShiftFS[p]&0x3F)];
			canAttackOppKing[oB]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitRU[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&(position[p1]>>3==whoseMove||p1==ep)) pinBySelf[p1]=true;
			result=slideRayLD[p, data];
			canAttackOppKing[oB]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitLD[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&(position[p1]>>3==whoseMove||p1==ep)) pinBySelf[p1]=true;
			result=slideRayRD[p, data=(int)(occuBS>>occuShiftBS[p]&0x3F)];
			canAttackOppKing[oB]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitRD[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&(position[p1]>>3==whoseMove||p1==ep)) pinBySelf[p1]=true;
			result=slideRayLU[p, data];
			canAttackOppKing[oB]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitLU[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&(position[p1]>>3==whoseMove||p1==ep)) pinBySelf[p1]=true;

			// 右左
			result=slideRayR[p, data=(int)(occuH>>occuShiftH[p]&0x3F)];
			canAttackOppKing[oR]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitR[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF; p3=pData>>16;
			if((position[p2]==oR||position[p2]==oQ)&&position[p1]>>3==whoseMove) pinBySelf[p1]=true;
			else if(p3!=NS&&ep!=NS&&(position[p3]==oR||position[p3]==oQ)&&(position[p1]==oP&&p2==ep||position[p2]==oP&&p1==ep)) dblDis=true;
			result=slideRayL[p, data];
			canAttackOppKing[oR]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitL[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF; p3=pData>>16;
			if((position[p2]==oR||position[p2]==oQ)&&position[p1]>>3==whoseMove) pinBySelf[p1]=true;
			else if(p3!=NS&&ep!=NS&&(position[p3]==oR||position[p3]==oQ)&&(position[p1]==oP&&p2==ep||position[p2]==oP&&p1==ep)) dblDis=true;

			// 上下
			result=slideRayU[p, data=(int)(occuV>>occuShiftV[p]&0x3F)];
			canAttackOppKing[oR]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitU[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oR||position[p2]==oQ)&&position[p1]>>3==whoseMove) pinBySelf[p1]=true;
			result=slideRayD[p, data];
			canAttackOppKing[oR]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitD[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oR||position[p2]==oQ)&&position[p1]>>3==whoseMove) pinBySelf[p1]=true;

			// 處理我方國王
			p=kingPos[whoseMove];
			if((result=((whoseMove==WT?pieceRangeWP[p]:pieceRangeBP[p])&piecePos[pP]))!=0)
				{ checkPieceCount++; canStopCheck|=result;}														// 由於合法性檢查已經排除了雙兵將跟雙騎士將
			if((result=(pieceRangeN[p]&piecePos[pN]))!=0) { checkPieceCount++; canStopCheck|=result;}			// 所以這邊這兩種都只要算一次即可

			// 斜向
			result=slideRayRU[p, data=(int)(occuFS>>occuShiftFS[p]&0x3F)];
			if((result&(piecePos[pB]|piecePos[pQ]))!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitRU[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=true;
			}
			result=slideRayLD[p, data];
			if((result&(piecePos[pB]|piecePos[pQ]))!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitLD[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=true;
			}
			result=slideRayRD[p, data=(int)(occuBS>>occuShiftBS[p]&0x3F)];
			if((result&(piecePos[pB]|piecePos[pQ]))!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitLD[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=true;
			}
			result=slideRayLU[p, data];
			if((result&(piecePos[pB]|piecePos[pQ]))!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitLD[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=true;
			}

			// 右左
			result=slideRayR[p, data=(int)(occuH>>occuShiftH[p]&0x3F)];
			if((result&(piecePos[pR]|piecePos[pQ]))!=0) { checkPieceCount++; canStopCheck|=result;}
			else {
				pData=slideHitR[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF; p3=pData>>16;
				if((position[p2]==pR||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=true;
				else if(p3!=NS&&ep!=NS&&(position[p3]==pR||position[p3]==pQ)&&(position[p1]==oP&&p2==ep||position[p2]==oP&&p1==ep)) dblPin=true;
			}
			result=slideRayL[p, data];
			if((result&(piecePos[pR]|piecePos[pQ]))!=0) { checkPieceCount++; canStopCheck|=result;}
			else {
				pData=slideHitL[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF; p3=pData>>16;
				if((position[p2]==pR||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=true;
				else if(p3!=NS&&ep!=NS&&(position[p3]==pR||position[p3]==pQ)&&(position[p1]==oP&&p2==ep||position[p2]==oP&&p1==ep)) dblPin=true;
			}

			// 上下
			result=slideRayU[p, data=(int)(occuV>>occuShiftV[p]&0x3F)];
			if((result&(piecePos[pR]|piecePos[pQ]))!=0) { checkPieceCount++; canStopCheck|=result;}
			else {
				pData=slideHitU[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pR||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=true;
			}
			result=slideRayD[p, data];
			if((result&(piecePos[pR]|piecePos[pQ]))!=0) { checkPieceCount++; canStopCheck|=result;}
			else {
				pData=slideHitD[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pR||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=true;
			}

			// 大部分的資料生成完畢，接著一邊生成殆合法棋步、一邊生成最後的 attackByOpp 資料

			for(p=0;p<64;p++) {
				k=position[p];
				if(k==b0) continue;
				if((k>>3)==whoseMove) {

					// 入堡棋步，這邊只檢查入堡權、當下的將軍以及中間的格子是否空的，攻擊檢查待會再做
					if(k==wK&&checkPieceCount==0) {
						if((castlingState[depth]&cwK)!=0&&position[5]==b0&&position[6]==b0)
							TML[l++]=((ulong)4)|((ulong)6<<8)|((ulong)6<<16)|((ulong)k<<24)|((ulong)k<<28)|((ulong)OOMove<<36)|((ulong)3<<48);
						if((castlingState[depth]&cwQ)!=0&&position[1]==b0&&position[2]==b0&&position[3]==b0)
							TML[l++]=((ulong)4)|((ulong)2<<8)|((ulong)2<<16)|((ulong)k<<24)|((ulong)k<<28)|((ulong)OOOMove<<36)|((ulong)5<<48);
					} else if(k==bK&&checkPieceCount==0) {
						if((castlingState[depth]&cbK)!=0&&position[61]==b0&&position[62]==b0)
							TML[l++]=((ulong)60)|((ulong)62<<8)|((ulong)62<<16)|((ulong)k<<24)|((ulong)k<<28)|((ulong)OOMove<<36)|((ulong)3<<48);
						if((castlingState[depth]&cbQ)!=0&&position[57]==b0&&position[58]==b0&&position[59]==b0)
							TML[l++]=((ulong)60)|((ulong)58<<8)|((ulong)58<<16)|((ulong)k<<24)|((ulong)k<<28)|((ulong)OOOMove<<36)|((ulong)5<<48);
					}

					// 小兵棋步

					if(k==wP) {
						if(position[r=(byte)(p+8)]==b0) {
							if((p>>3)==6) {
								TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)wN<<28)|((ulong)4<<48);
								TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)wR<<28)|((ulong)4<<48);
								TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)wB<<28)|((ulong)4<<48);
								TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)wQ<<28)|((ulong)4<<48);
							} else {
								TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)k<<28)|((ulong)2<<48);
								if((p>>3)==1&&position[r=(byte)(p+16)]==b0) TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)k<<28)|((ulong)2<<48);
							}
						}
						if((r=pieceRuleWP[p, 0])!=NS) {
							if(side(d=position[r])==BC) {
								if((p>>3)==6) {
									TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)wN<<28)|((ulong)d<<32)|((ulong)6<<48);
									TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)wR<<28)|((ulong)d<<32)|((ulong)6<<48);
									TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)wB<<28)|((ulong)d<<32)|((ulong)6<<48);
									TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)wQ<<28)|((ulong)d<<32)|((ulong)6<<48);
								} else TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)k<<28)|((ulong)d<<32)|((ulong)4<<48);
							} else if(r==enPassantState[depth])
								TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)ep<<16)|((ulong)k<<24)|((ulong)k<<28)|((ulong)bP<<32)|((ulong)epMove<<36)|((ulong)6<<48);
							if((r=pieceRuleWP[p, 1])!=NS) {
								if(side(d=position[r])==BC) {
									if((p>>3)==6) {
										TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)wN<<28)|((ulong)d<<32)|((ulong)6<<48);
										TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)wR<<28)|((ulong)d<<32)|((ulong)6<<48);
										TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)wB<<28)|((ulong)d<<32)|((ulong)6<<48);
										TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)wQ<<28)|((ulong)d<<32)|((ulong)6<<48);
									} else TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)k<<28)|((ulong)d<<32)|((ulong)4<<48);
								} else if(r==enPassantState[depth])
									TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)ep<<16)|((ulong)k<<24)|((ulong)k<<28)|((ulong)bP<<32)|((ulong)epMove<<36)|((ulong)6<<48);
							}
						}
					} else if(k==bP) {
						if(position[r=(byte)(p-8)]==b0) {
							if((p>>3)==1) {
								TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)bN<<28)|((ulong)4<<48);
								TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)bR<<28)|((ulong)4<<48);
								TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)bB<<28)|((ulong)4<<48);
								TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)bQ<<28)|((ulong)4<<48);
							} else {
								TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)k<<28)|((ulong)2<<48);
								if((p>>3)==6&&position[r=(byte)(p-16)]==b0) TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)k<<28)|((ulong)2<<48);
							}
						}
						if((r=pieceRuleBP[p, 0])!=NS) {
							if(side(d=position[r])==WT) {
								if((p>>3)==1) {
									TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)bN<<28)|((ulong)d<<32)|((ulong)6<<48);
									TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)bR<<28)|((ulong)d<<32)|((ulong)6<<48);
									TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)bB<<28)|((ulong)d<<32)|((ulong)6<<48);
									TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)bQ<<28)|((ulong)d<<32)|((ulong)6<<48);
								} else TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)k<<28)|((ulong)d<<32)|((ulong)4<<48);
							} else if(r==enPassantState[depth])
								TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)ep<<16)|((ulong)k<<24)|((ulong)k<<28)|((ulong)wP<<32)|((ulong)epMove<<36)|((ulong)6<<48);
							if((r=pieceRuleBP[p, 1])!=NS) {
								if(side(d=position[r])==WT) {
									if((p>>3)==1) {
										TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)bN<<28)|((ulong)d<<32)|((ulong)6<<48);
										TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)bR<<28)|((ulong)d<<32)|((ulong)6<<48);
										TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)bB<<28)|((ulong)d<<32)|((ulong)6<<48);
										TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)bQ<<28)|((ulong)d<<32)|((ulong)6<<48);
									} else TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)k<<28)|((ulong)d<<32)|((ulong)4<<48);
								} else if(r==enPassantState[depth])
									TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)ep<<16)|((ulong)k<<24)|((ulong)k<<28)|((ulong)wP<<32)|((ulong)epMove<<36)|((ulong)6<<48);
							}
						}
					}


					// 普通棋步（含國王的）
					else {
						if(k==oN) {
							for(i=0;(r=pieceRuleN[p, i])!=NS;i++)
								if((d=position[r])==0) TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)k<<28)|((ulong)3<<48);
								else if(d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)k<<28)|((ulong)d<<32)|((ulong)4<<48);
						} else if(k==oK) {
							for(i=0;(r=pieceRuleK[p, i])!=NS;i++)
								if((d=position[r])==0) TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)k<<28)|((ulong)3<<48);
								else if(d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)k<<28)|((ulong)d<<32)|((ulong)4<<48);
						} else {
							if((k&b1)==b1)
								for(i=0;HVRule[p, i, 0]!=NS;i++) for(j=0;(r=HVRule[p, i, j])!=NS;j++) {
										if((d=position[r])==0) TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)k<<28)|((ulong)3<<48);
										else { if(d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)k<<28)|((ulong)d<<32)|((ulong)4<<48); break; }
									}
							if((k&b2)==b2)
								for(i=0;DIRule[p, i, 0]!=NS;i++) for(j=0;(r=DIRule[p, i, j])!=NS;j++) {
										if((d=position[r])==0) TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)k<<28)|((ulong)3<<48);
										else { if(d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<8)|((ulong)r<<16)|((ulong)k<<24)|((ulong)k<<28)|((ulong)d<<32)|((ulong)4<<48); break; }
									}
						}
					}
				}
				
				// 如果是敵方的棋子，生成 attackByOpp 資料
				else {
					if(k==pP) attackByOpp|=(k==wP?pieceRangeWP[p]:pieceRangeBP[p]);
					else if(k==pN) attackByOpp|=pieceRangeN[p];
					else if(k==pK) attackByOpp|=pieceRangeK[p];
					else {
						// 這邊記得要想像我方的國王不存在，免得待會國王往反方向閃
						if((k&b1)==b1) attackByOpp|=slideRangeH[p, (occuH^mask[kingPos[whoseMove]])>>occuShiftH[p]&0x3F]|
							slideRangeV[p, (occuV^maskV[kingPos[whoseMove]])>>occuShiftV[p]&0x3F];
						if((k&b2)==b2) attackByOpp|=slideRangeFS[p, (occuFS^maskFS[kingPos[whoseMove]])>>occuShiftFS[p]&0x3F]|
							slideRangeBS[p, (occuBS^maskBS[kingPos[whoseMove]])>>occuShiftBS[p]&0x3F];
					}
				}
			}

			// 殆合法棋步以及所有需要的資料都生成完畢，檢驗棋步的合法性、以及將軍對方與否
			Array.Clear(DisambListLength, 0, 1024);
			for(i=0, j=0;i<l;i++) {
				tag=checkMove(TML[i]);
				if(tag>0) {
					so=(byte)(TML[i]&0x3F);
					ta=(byte)((TML[i]>>8)&0x3F);
					ot=(byte)((TML[i]>>24)&0xF);	
					if(ot!=wP&&ot!=bP&&ot!=wK&&ot!=bK)
						DisambList[ta, ot, DisambListLength[ta, ot]++]=so;	// 登錄消歧義名單
					moveList[depth, j++]=TML[i]|((ulong)tag<<40);
				}
			}
			moveListLength[depth]=(byte)j;
			
			// 全部的合法棋步都出來之後，進行消歧義標籤計算
			for(i=0;i<j;i++) {
				ta=(byte)((moveList[depth, i]>>8)&0x3F);
				ot=(byte)((moveList[depth, i]>>24)&0xF);
				if(DisambListLength[ta, ot]<=1) continue;
				so=(byte)(moveList[depth, i]&0x3F); cx=0; cy=0;
				for(l=0;l<DisambListLength[ta, ot];l++) {
					if((DisambList[ta, ot, l]&7)==(so&7)) cx++;
					if((DisambList[ta, ot, l]>>3)==(so>>3)) cy++;
				}
				moveList[depth, i]|=((ulong)(cx==1?b1:(cy==1?b2:b3))<<44);
			}

			if(j==0&&depth>0&&(moveHis[depth-1]>>40)==2) moveHis[depth-1]|=(1<<40);		// 如果沒有合法棋步可動，且上一步是將軍，換掉將軍符號為將死
		}

		/////////////////////////////////
		// 合法棋步判斷
		/////////////////////////////////

		private byte checkMove(ulong m) {
			byte so=(byte)(m&0x3F);
			byte ta=(byte)((m>>8)&0x3F);
			byte de=(byte)((m>>16)&0x3F);
			byte ot=(byte)((m>>24)&0xF);
			byte nt=(byte)((m>>28)&0xF);
			byte cp=(byte)((m>>32)&0xF);
			byte mi=(byte)((m>>36)&0xF);

			// 先排除不合法的情況
		
			// 沒有將軍的情況
			if(checkPieceCount==0) {
				// 如果不是國王（佔大多數），那只要檢查是否移動的棋子被釘住即可
				if(ot!=oK) {
					if(dblPin&&mi==epMove||pinByOpp[so]&&
						(ot==oN||relDir[so, ta]!=relDir[so, kingPos[whoseMove]])) return 0;
				}
				// 如果是國王
				else if(mi==OOMove&&(attackByOpp&mask[so+1])!=0) return 0;							// 檢查王側入堡的路上一格有沒有被攻擊
				else if(mi==OOOMove&&(attackByOpp&(mask[so-1]))!=0) return 0;					 	// 后側入堡的情況
				else if((attackByOpp&mask[ta])!=0) return 0;										// 檢查國王的目的地本身
			}

			// 單將軍的情況
			else if(checkPieceCount==1) {
				if(ot!=oK) {
					if(mi==epMove) {
						if((canStopCheck&mask[de])==0) return 0;									// 如果是吃過路兵，而且那個過路兵正在將軍，就可以
					}
					// 特別注意到如果前一步是動過路兵而且造成了將軍，且下一手可以藉由吃過路兵解除，那一定是那個過路兵本身正在將軍國王
					else if((canStopCheck&mask[ta])==0||(dblPin&&mi==epMove||pinByOpp[so]&&
						(ot==oN||relDir[so, ta]!=relDir[so, kingPos[whoseMove]]))) return 0;		// 如果是一般棋子，那必須阻止對方的將軍，但那個棋子不能被釘住
				} else if((attackByOpp&mask[ta])!=0) return 0;										// 如果動的是國王，必須閃到安全地帶（入堡在生成殆合法棋步時已經排除）
			}

			// 雙將軍的情況
			else if(ot!=oK||(attackByOpp&mask[ta])!=0) return 0;									// 移動的一定得是國王，而且目標不能被攻擊

			// 如果至此都沒有問題，那就表示棋步是合法的，進一步檢查這個棋步是否造成將軍對方
			if(mi!=OOMove&&mi!=OOOMove) {
				if((canAttackOppKing[nt]&mask[ta])!=0) return 2;									// 直接走就進入攻擊位置（含升變）
				else if(mi==epMove&&(dblDis||pinBySelf[de])||pinBySelf[so]&&
					(ot==oN||relDir[so, ta]!=relDir[so, kingPos[1-whoseMove]])) return 2;			// 閃擊（涵蓋了一次閃兩子的橫向閃擊情況、以及斜向的吃過路兵閃擊）
			} else {
				if(pinBySelf[so]) return 2;															// 入堡的情況要多做一種「入堡閃擊」的判斷
				// 注意到如果一個可以入堡的國王被自己釘住，那只有可能是對方的國王在另一側，所以如果做入堡動作一定會導致將軍
				else if(mi==OOMove&&(canAttackOppKing[oR]&mask[so+1])!=0) return 2;					// 一般的王側入堡將軍
				else if(mi==OOOMove&&(canAttackOppKing[oR]&mask[so-1])!=0) return 2;				// 一般的后側入堡將軍				
			}

			// 以上皆非的話，就表示沒有將軍對方
			return 1;
		}

	}
}