
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;

namespace Mushikui_Puzzle_Workshop {
	partial class chessEngine2 {

		/////////////////////////////////
		// 長度為 2 的棋步
		/////////////////////////////////

		// 只需要搜尋兵的直走棋步就好
		// 特別注意到不會有上下釘住的情況
		// canAttackOppKing 只需要蒐集兵的部分就好

		private void computeLegalMoves2() {
			int i, l=0, pData;
			byte p;				
			byte so, ta;
			int data, p1, p2, p3;
			ulong result, pPos, d;

			// 設置代碼
			if(whoseMove==WT) {
				oP=wP; oR=wR; oN=wN; oB=wB; oQ=wQ; oK=wK; pP=bP; pR=bR; pN=bN; pB=bB; pQ=bQ; pK=bK;
			} else {
				oP=bP; oR=bR; oN=bN; oB=bB; oQ=bQ; oK=bK; pP=wP; pR=wR; pN=wN; pB=wB; pQ=wQ; pK=wK;
			}

			// 資料歸零
			canStopCheck=0;
			checkPieceCount=0;
			dblDis=dblPin=false;
			Array.Clear(canAttackOppKing, 0, 16);
			Array.Clear(pinByOpp, 0, 64);
			Array.Clear(pinBySelf, 0, 64);

			// 處理敵方國王
			p=kingPos[1-whoseMove];
			canAttackOppKing[oP]=(whoseMove==WT?pieceRangeBP[p]:pieceRangeWP[p]);			// 記得要用另一方的兵
			
			// 斜向
			result=slideRayRU[p, data=(int)(occuF>>occuShiftF[p]&0x3F)];
			pData=slideHitRU[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&(position[p1]>>3==whoseMove)) pinBySelf[p1]=4;
			result=slideRayLD[p, data];
			pData=slideHitLD[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&(position[p1]>>3==whoseMove)) pinBySelf[p1]=4;
			result=slideRayRD[p, data=(int)(occuB>>occuShiftB[p]&0x3F)];
			pData=slideHitRD[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&(position[p1]>>3==whoseMove)) pinBySelf[p1]=8;
			result=slideRayLU[p, data];
			pData=slideHitLU[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&(position[p1]>>3==whoseMove)) pinBySelf[p1]=8;

			// 右左
			result=slideRayR[p, data=(int)(occuH>>occuShiftH[p]&0x3F)];
			pData=slideHitR[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF; p3=pData>>16;
			if((position[p2]==oR||position[p2]==oQ)&&position[p1]>>3==whoseMove) pinBySelf[p1]=1;
			result=slideRayL[p, data];
			pData=slideHitL[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF; p3=pData>>16;
			if((position[p2]==oR||position[p2]==oQ)&&position[p1]>>3==whoseMove) pinBySelf[p1]=1;

			// 處理我方國王
			p=kingPos[whoseMove];
			if((result=((whoseMove==WT?pieceRangeWP[p]:pieceRangeBP[p])&piecePos[pP]))!=0)						// 由於合法性檢查已經排除了雙兵將跟雙騎士將
				{ checkPieceCount++; canStopCheck|=result;}
			if((result=(pieceRangeN[p]&piecePos[pN]))!=0) { checkPieceCount++; canStopCheck|=result;}			// 所以這邊這兩種都只要算一次即可

			// 斜向
			pPos=piecePos[pB]|piecePos[pQ];
			result=slideRayRU[p, data=(int)(occuF>>occuShiftF[p]&0x3F)];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitRU[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=4;
			}
			result=slideRayLD[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitLD[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=4;
			}
			result=slideRayRD[p, data=(int)(occuB>>occuShiftB[p]&0x3F)];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitRD[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=8;
			}
			result=slideRayLU[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitLU[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=8;
			}

			// 右左
			pPos=piecePos[pR]|piecePos[pQ];
			result=slideRayR[p, data=(int)(occuH>>occuShiftH[p]&0x3F)];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitR[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF; p3=pData>>16;
				if((position[p2]==pR||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=1;
			}
			result=slideRayL[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitL[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF; p3=pData>>16;
				if((position[p2]==pR||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=1;
			}

			// 上下，不做釘子檢查
			result=slideRayU[p, data=(int)(occuV>>occuShiftV[p]&0x3F)];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; }
			result=slideRayD[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; }
			
			// 資料生成完畢，生成合法棋步清單（生成的時候直接判斷完合法性，沒有殆合法棋步的生成階段）
			// 稍早因為已經排除了上下釘住的情況，所以底下只要檢查有沒有釘住就好了，不用管方向
			// 然後只要確定不會攻擊到對方的國王就好

			d=((ulong)oP<<otS)|((ulong)oP<<ntS)|len2;	// 通用尾端資料

			// 其他的棋步一律只有當不是雙將軍的時候才有可能下
			// 先考慮沒將軍的情況
			if(checkPieceCount==0) {

				// 小兵
				if(whoseMove==WT) {
					for(i=0;i<pieceCount[wP];i++) if(pinByOpp[so=pieceList[wP, i]]==0&&pinBySelf[so]==0) {
						if(position[ta=(byte)(so+8)]==b0) {
							if((so>>3)!=6) {
								if((canAttackOppKing[oP]&mask[ta])==0) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|d;
								if((so>>3)==1&&position[ta=(byte)(so+16)]==b0&&(canAttackOppKing[oP]&mask[ta])==0)
									moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|d;
							}
						}	
					}
				} else {
					for(i=0;i<pieceCount[bP];i++) if(pinByOpp[so=pieceList[bP, i]]==0&&pinBySelf[so]==0) {
						if(position[ta=(byte)(so-8)]==b0) {
							if((so>>3)!=1) {
								if((canAttackOppKing[oP]&mask[ta])==0) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|d;
								if((so>>3)==6&&position[ta=(byte)(so-16)]==b0&&(canAttackOppKing[oP]&mask[ta])==0)
									moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|d;
							}
						}
					}
				}
			}
			
			// 然後是單將軍的情況，差別只在於目的地必須在 canStopCheck 清單當中
			else if(checkPieceCount==1) {
				// 小兵
				if(whoseMove==WT) {
					for(i=0;i<pieceCount[wP];i++) if(pinByOpp[so=pieceList[wP, i]]==0&&pinBySelf[so]==0) {
						if(position[ta=(byte)(so+8)]==b0) {
							if((so>>3)!=6) {
								if((canStopCheck&mask[ta])!=0&&(canAttackOppKing[oP]&mask[ta])==0) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|d;
								if((so>>3)==1&&position[ta=(byte)(so+16)]==b0&&(canStopCheck&mask[ta])!=0&&(canAttackOppKing[oP]&mask[ta])==0)
									moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|d;
							}
						}
					}
				} else {
					for(i=0;i<pieceCount[bP];i++) if(pinByOpp[so=pieceList[bP, i]]==0&&pinBySelf[so]==0) {
						if(position[ta=(byte)(so-8)]==b0) {
							if((so>>3)!=1) {
								if((canStopCheck&mask[ta])!=0&&(canAttackOppKing[oP]&mask[ta])==0) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|d;
								if((so>>3)==6&&position[ta=(byte)(so-16)]==b0&&(canStopCheck&mask[ta])!=0&&(canAttackOppKing[oP]&mask[ta])==0)
									moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|d;
							}
						}
					}
				}
			}
			moveListLength[depth]=(byte)l;
#if DEBUG
			totalMoveCount[1]+=l;
			totalMoveCount[2]+=l;
#endif
		}

		/////////////////////////////////
		// 長度為 3 的棋步
		/////////////////////////////////

		// 兵的部分只要搜尋直走，入堡只要搜尋 O-O
		// 兵必須將軍，而其餘棋步都不能將軍

		private void computeLegalMoves3() {
			int i, j, k, l=0, pData;
			byte p, r;
			byte tag, so, ta, ot, nt, de, cp, mi;
			int data, p1, p2, p3;
			ulong result, pPos, m, n;
			int cx, cy;

			// 設置代碼
			if(whoseMove==WT) {
				oP=wP; oR=wR; oN=wN; oB=wB; oQ=wQ; oK=wK; pP=bP; pR=bR; pN=bN; pB=bB; pQ=bQ; pK=bK;
			} else {
				oP=bP; oR=bR; oN=bN; oB=bB; oQ=bQ; oK=bK; pP=wP; pR=wR; pN=wN; pB=wB; pQ=wQ; pK=wK;
			}

			// 資料歸零
			canStopCheck=0;
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
			result=slideRayRU[p, data=(int)(occuF>>occuShiftF[p]&0x3F)];
			canAttackOppKing[oB]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitRU[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&(position[p1]>>3==whoseMove)) pinBySelf[p1]=4;
			result=slideRayLD[p, data];
			canAttackOppKing[oB]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitLD[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&(position[p1]>>3==whoseMove)) pinBySelf[p1]=4;
			result=slideRayRD[p, data=(int)(occuB>>occuShiftB[p]&0x3F)];
			canAttackOppKing[oB]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitRD[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&(position[p1]>>3==whoseMove)) pinBySelf[p1]=8;
			result=slideRayLU[p, data];
			canAttackOppKing[oB]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitLU[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&(position[p1]>>3==whoseMove)) pinBySelf[p1]=8;

			// 右左
			result=slideRayR[p, data=(int)(occuH>>occuShiftH[p]&0x3F)];
			canAttackOppKing[oR]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitR[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF; p3=pData>>16;
			if((position[p2]==oR||position[p2]==oQ)&&position[p1]>>3==whoseMove) pinBySelf[p1]=1;
			result=slideRayL[p, data];
			canAttackOppKing[oR]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitL[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF; p3=pData>>16;
			if((position[p2]==oR||position[p2]==oQ)&&position[p1]>>3==whoseMove) pinBySelf[p1]=1;

			// 上下
			result=slideRayU[p, data=(int)(occuV>>occuShiftV[p]&0x3F)];
			canAttackOppKing[oR]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitU[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oR||position[p2]==oQ)&&position[p1]>>3==whoseMove) pinBySelf[p1]=2;
			result=slideRayD[p, data];
			canAttackOppKing[oR]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitD[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oR||position[p2]==oQ)&&position[p1]>>3==whoseMove) pinBySelf[p1]=2;

			// 處理我方國王
			p=kingPos[whoseMove];
			if((result=((whoseMove==WT?pieceRangeWP[p]:pieceRangeBP[p])&piecePos[pP]))!=0)						// 由於合法性檢查已經排除了雙兵將跟雙騎士將
				{ checkPieceCount++; canStopCheck|=result; }
			if((result=(pieceRangeN[p]&piecePos[pN]))!=0) { checkPieceCount++; canStopCheck|=result; }			// 所以這邊這兩種都只要算一次即可

			// 斜向
			pPos=piecePos[pB]|piecePos[pQ];
			result=slideRayRU[p, data=(int)(occuF>>occuShiftF[p]&0x3F)];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitRU[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=4;
			}
			result=slideRayLD[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitLD[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=4;
			}
			result=slideRayRD[p, data=(int)(occuB>>occuShiftB[p]&0x3F)];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitRD[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=8;
			}
			result=slideRayLU[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitLU[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=8;
			}

			// 右左
			pPos=piecePos[pR]|piecePos[pQ];
			result=slideRayR[p, data=(int)(occuH>>occuShiftH[p]&0x3F)];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitR[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF; p3=pData>>16;
				if((position[p2]==pR||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=1;
			}
			result=slideRayL[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitL[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF; p3=pData>>16;
				if((position[p2]==pR||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=1;
			}

			// 上下
			result=slideRayU[p, data=(int)(occuV>>occuShiftV[p]&0x3F)];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitU[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pR||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=2;
			}
			result=slideRayD[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitD[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pR||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=2;
			}

			// 生成 attackByOpp 資料
			attackByOpp=pieceRangeK[kingPos[1-whoseMove]];
			for(i=0;i<pieceCount[pP];i++) attackByOpp|=(whoseMove==BC?pieceRangeWP[pieceList[pP, i]]:pieceRangeBP[pieceList[pP, i]]);
			for(i=0;i<pieceCount[pN];i++) attackByOpp|=pieceRangeN[pieceList[pN, i]];
			for(i=0;i<pieceCount[pR];i++) attackByOpp|=slideRangeH[p=pieceList[pR, i], (occuH^mask[kingPos[whoseMove]])>>occuShiftH[p]&0x3F]|
				slideRangeV[p, (occuV^maskV[kingPos[whoseMove]])>>occuShiftV[p]&0x3F];
			for(i=0;i<pieceCount[pB];i++) attackByOpp|=slideRangeF[p=pieceList[pB, i], (occuF^maskF[kingPos[whoseMove]])>>occuShiftF[p]&0x3F]|
				slideRangeB[p, (occuB^maskB[kingPos[whoseMove]])>>occuShiftB[p]&0x3F];
			for(i=0;i<pieceCount[pQ];i++) attackByOpp|=slideRangeH[p=pieceList[pQ, i], (occuH^mask[kingPos[whoseMove]])>>occuShiftH[p]&0x3F]|
				slideRangeV[p, (occuV^maskV[kingPos[whoseMove]])>>occuShiftV[p]&0x3F]|slideRangeF[p, (occuF^maskF[kingPos[whoseMove]])>>occuShiftF[p]&0x3F]|
				slideRangeB[p, (occuB^maskB[kingPos[whoseMove]])>>occuShiftB[p]&0x3F];

			// 資料生成完畢，生成合法棋步清單（生成的時候直接判斷完合法性，沒有殆合法棋步的生成階段）

			// 國王棋步（普通的）
			for(j=0;(ta=pieceRuleK[so=kingPos[whoseMove], j])!=NS;j++)
				if((attackByOpp&mask[ta])==0&&(pinBySelf[so]&relDir[so, ta])==0) {	//目的地不能被攻擊，也不能閃擊
					if((cp=position[ta])==0) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oK<<otS)|((ulong)oK<<ntS)|len3;
					else if(cp>>3!=whoseMove) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oK<<otS)|((ulong)oK<<ntS)|((ulong)cp<<cpS)|len4;
				}

			// 其他的棋步一律只有當不是雙將軍的時候才有可能下
			// 先考慮沒將軍的情況
			if(checkPieceCount==0) {

				// 小兵
				if(whoseMove==WT) {
					for(i=0;i<pieceCount[wP];i++) {
						so=pieceList[wP, i];
						if(position[ta=(byte)(so+8)]==b0&&(pinByOpp[so]&relDirV)==0) {
							if((so>>3)!=6) {
								moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wP<<ntS)|len2;
								if((so>>3)==1&&position[ta=(byte)(so+16)]==b0)
									moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wP<<ntS)|len2;
							}
						}
					}
				} else {
					for(i=0;i<pieceCount[bP];i++) {
						so=pieceList[bP, i];
						if(position[ta=(byte)(so-8)]==b0&&(pinByOpp[so]&relDirV)==0) {
							if((so>>3)!=1) {
								moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bP<<ntS)|len2;
								if((so>>3)==6&&position[ta=(byte)(so-16)]==b0)
									moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bP<<ntS)|len2;
							}
						}
					}
				}

				// 騎士棋步
				for(i=0;i<pieceCount[oN];i++)
					if(pinByOpp[so=pieceList[oN, i]]==0)	// 騎士只要被釘住就不能動
						for(j=0;(ta=pieceRuleN[so, j])!=NS;j++)
							if((cp=position[ta])==0) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oN<<otS)|((ulong)oN<<ntS)|len3;
							else if(cp>>3!=whoseMove) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oN<<otS)|((ulong)oN<<ntS)|((ulong)cp<<cpS)|len4;

				// 城堡棋步
				for(i=0;i<pieceCount[oR];i++) {
					so=pieceList[oR, i];
					for(j=0;HVRuleDir[so, j]!=relDirNO;j++)
						if((pinByOpp[so]&HVRuleDir[so, j])==0)	// 確定這個射線沒有被釘住
							for(k=0;(ta=HVRule[so, j, k])!=NS;k++) {
								if((cp=position[ta])==0) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oR<<otS)|((ulong)oR<<ntS)|len3;
								else { if(cp>>3!=whoseMove) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oR<<otS)|((ulong)oR<<ntS)|((ulong)cp<<cpS)|len4; break; }
							}
				}

				// 主教棋步
				for(i=0;i<pieceCount[oB];i++) {
					so=pieceList[oB, i];
					for(j=0;DIRuleDir[so, j]!=relDirNO;j++)
						if((pinByOpp[so]&DIRuleDir[so, j])==0)
							for(k=0;(ta=DIRule[so, j, k])!=NS;k++) {
								if((cp=position[ta])==0) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oB<<otS)|((ulong)oB<<ntS)|len3;
								else { if(cp>>3!=whoseMove) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oB<<otS)|((ulong)oB<<ntS)|((ulong)cp<<cpS)|len4; break; }
							}
				}

				// 皇后棋步
				for(i=0;i<pieceCount[oQ];i++) {
					so=pieceList[oQ, i];
					for(j=0;HVRuleDir[so, j]!=relDirNO;j++)
						if((pinByOpp[so]&HVRuleDir[so, j])==0)
							for(k=0;(ta=HVRule[so, j, k])!=NS;k++) {
								if((cp=position[ta])==0) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|len3;
								else { if(cp>>3!=whoseMove) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4; break; }
							}
					for(j=0;DIRuleDir[so, j]!=relDirNO;j++)
						if((pinByOpp[so]&DIRuleDir[so, j])==0)
							for(k=0;(ta=DIRule[so, j, k])!=NS;k++) {
								if((cp=position[ta])==0) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|len3;
								else { if(cp>>3!=whoseMove) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4; break; }
							}
				}

				// 入堡棋步，只有沒有被將軍的時候可以做
				if(checkPieceCount==0) {
					if(whoseMove==WT) {
						if((castlingState[depth]&cwK)!=0&&position[5]==b0&&position[6]==b0&&(attackByOpp&(mask[5]|mask[6]))==0) moveList[depth, l++]=wOO;
					} else {
						if((castlingState[depth]&cbK)!=0&&position[61]==b0&&position[62]==b0&&(attackByOpp&(mask[61]|mask[62]))==0) moveList[depth, l++]=bOO;
					}
				}
			}

			// 然後是單將軍的情況，差別只在於目的地必須在 canStopCheck 清單當中
			else if(checkPieceCount==1) {
				// 小兵
				if(whoseMove==WT) {
					for(i=0;i<pieceCount[wP];i++) {
						so=pieceList[wP, i];
						if(position[ta=(byte)(so+8)]==b0&&(pinByOpp[so]&relDirV)==0) {
							if((so>>3)!=6) {
								if((canStopCheck&mask[ta])!=0) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wP<<ntS)|len2;
								if((so>>3)==1&&position[ta=(byte)(so+16)]==b0&&(canStopCheck&mask[ta])!=0)
									moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wP<<ntS)|len2;
							}
						}
					}
				} else {
					for(i=0;i<pieceCount[bP];i++) {
						so=pieceList[bP, i];
						if(position[ta=(byte)(so-8)]==b0&&(pinByOpp[so]&relDirV)==0) {
							if((so>>3)!=1) {
								if((canStopCheck&mask[ta])!=0) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bP<<ntS)|len2;
								if((so>>3)==6&&position[ta=(byte)(so-16)]==b0&&(canStopCheck&mask[ta])!=0)
									moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bP<<ntS)|len2;
							}
						}
					}
				}

				// 騎士棋步
				for(i=0;i<pieceCount[oN];i++)
					if(pinByOpp[so=pieceList[oN, i]]==0)	// 騎士只要被釘住就不能動
						for(j=0;(ta=pieceRuleN[so, j])!=NS;j++)
							if((canStopCheck&mask[ta])!=0)
								if((cp=position[ta])==0) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oN<<otS)|((ulong)oN<<ntS)|len3;
								else if(cp>>3!=whoseMove) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oN<<otS)|((ulong)oN<<ntS)|((ulong)cp<<cpS)|len4;

				// 城堡棋步
				for(i=0;i<pieceCount[oR];i++) {
					so=pieceList[oR, i]; m=0;
					if((pinByOpp[so]&relDirH)==0) m|=slideRangeH[so, (occuH>>occuShiftH[so])&0x3F];
					if((pinByOpp[so]&relDirV)==0) m|=slideRangeV[so, (occuV>>occuShiftV[so])&0x3F];
					m&=canStopCheck;
					while(m!=0) {
						n=m; ta=0;
						if((n&0xFFFFFFFF00000000)!=0) { n>>=32; ta|=32; } if((n&0xFFFF0000)!=0) { n>>=16; ta|=16; } if((n&0xFF00)!=0) { n>>=8; ta|=8; }
						if((n&0xF0)!=0) { n>>=4; ta|=4; } if((n&0xC)!=0) { n>>=2; ta|=2; } if(n==2) ta|=1; m^=mask[ta];
						if((cp=position[ta])==0) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oR<<otS)|((ulong)oR<<ntS)|len3;
						else moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oR<<otS)|((ulong)oR<<ntS)|((ulong)cp<<cpS)|len4;
					}
				}

				// 主教棋步
				for(i=0;i<pieceCount[oB];i++) {
					so=pieceList[oB, i]; m=0;
					if((pinByOpp[so]&relDirF)==0) m|=slideRangeF[so, (occuF>>occuShiftF[so])&0x3F];
					if((pinByOpp[so]&relDirB)==0) m|=slideRangeB[so, (occuB>>occuShiftB[so])&0x3F];
					m&=canStopCheck;
					while(m!=0) {
						n=m; ta=0;
						if((n&0xFFFFFFFF00000000)!=0) { n>>=32; ta|=32; } if((n&0xFFFF0000)!=0) { n>>=16; ta|=16; } if((n&0xFF00)!=0) { n>>=8; ta|=8; }
						if((n&0xF0)!=0) { n>>=4; ta|=4; } if((n&0xC)!=0) { n>>=2; ta|=2; } if(n==2) ta|=1; m^=mask[ta];
						if((cp=position[ta])==0) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oB<<otS)|((ulong)oB<<ntS)|len3;
						else moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oB<<otS)|((ulong)oB<<ntS)|((ulong)cp<<cpS)|len4;
					}
				}

				// 皇后棋步
				for(i=0;i<pieceCount[oQ];i++) {
					so=pieceList[oQ, i]; m=0;
					if((pinByOpp[so]&relDirH)==0) m|=slideRangeH[so, (occuH>>occuShiftH[so])&0x3F];
					if((pinByOpp[so]&relDirV)==0) m|=slideRangeV[so, (occuV>>occuShiftV[so])&0x3F];
					if((pinByOpp[so]&relDirF)==0) m|=slideRangeF[so, (occuF>>occuShiftF[so])&0x3F];
					if((pinByOpp[so]&relDirB)==0) m|=slideRangeB[so, (occuB>>occuShiftB[so])&0x3F];
					m&=canStopCheck;
					while(m!=0) {
						n=m; ta=0;
						if((n&0xFFFFFFFF00000000)!=0) { n>>=32; ta|=32; } if((n&0xFFFF0000)!=0) { n>>=16; ta|=16; } if((n&0xFF00)!=0) { n>>=8; ta|=8; }
						if((n&0xF0)!=0) { n>>=4; ta|=4; } if((n&0xC)!=0) { n>>=2; ta|=2; } if(n==2) ta|=1; m^=mask[ta];
						if((cp=position[ta])==0) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|len3;
						else moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4;
					}
				}
			}
			moveListLength[depth]=(byte)l;
#if DEBUG
			totalMoveCount[1]+=l;
			totalMoveCount[3]+=l;
#endif

			// 合法棋步生成完畢，檢驗將軍對方與否
			Array.Clear(DisambListLength, 0, 1024);
			for(i=0;i<l;i++) {
				m=moveList[depth, i];
				so=(byte)(m&0x3F);
				ta=(byte)((m>>taS)&0x3F);
				de=(byte)((m>>deS)&0x3F);
				ot=(byte)((m>>otS)&0xF);
				nt=(byte)((m>>ntS)&0xF);
				mi=(byte)((m>>miS)&0xF);
				tag=0;

				// 將軍判斷
				if(mi!=OOMove) {
					if((canAttackOppKing[nt]&mask[ta])!=0) tag=tgCheck;							// 直接走就進入攻擊位置（含升變）
					else if((pinBySelf[so]&relDir[so, ta])!=0) tag=tgCheck;						// 閃擊
				}
				// 注意到如果一個可以入堡的國王被自己釘住，
				// 那只有可能是對方的國王在另一側，所以如果做入堡動作一定會導致將軍
				else if(pinBySelf[so]==1) tag=tgCheck;											// 入堡的情況要多做一種「入堡閃擊」的判斷
				else if(mi==OOMove&&(canAttackOppKing[oR]&mask[so+1])!=0) tag=tgCheck;			// 一般的王側入堡將軍

				if(ot!=wP&&ot!=bP&&ot!=wK&&ot!=bK) DisambList[ta, ot, DisambListLength[ta, ot]++]=so;	// 登錄消歧義名單
				moveList[depth, i]|=(ulong)tag<<tgS;
			}

			// 全部的合法棋步都出來之後，進行消歧義標籤計算
			for(i=0;i<l;i++) {
				ta=(byte)((moveList[depth, i]>>taS)&0x3F);
				ot=(byte)((moveList[depth, i]>>otS)&0xF);
				if((j=DisambListLength[ta, ot])<=1) continue;
				so=(byte)(moveList[depth, i]&0x3F); cx=0; cy=0;
				for(k=0;k<j;k++) {
					if((r=relDir[DisambList[ta, ot, k], so])==13) cx++;
					if(r==14) cy++;
				}
				moveList[depth, i]|=((ulong)(cx==0?b1:(cy==0?b2:b3))<<dbS);
			}
		}

		/////////////////////////////////
		// 長度為 4 的棋步
		/////////////////////////////////

		// 兵只要搜尋直走升變跟單純吃子兩種，入堡只要搜尋 O-O

		private void computeLegalMoves4() {
			int i, j, k, l=0, pData;
			byte p, r;
			byte tag, so, ta, ot, nt, de, cp, mi;
			int data, p1, p2, p3;
			ulong result, pPos, m, n;
			int cx, cy;

			// 設置代碼
			if(whoseMove==WT) {
				oP=wP; oR=wR; oN=wN; oB=wB; oQ=wQ; oK=wK; pP=bP; pR=bR; pN=bN; pB=bB; pQ=bQ; pK=bK;
			} else {
				oP=bP; oR=bR; oN=bN; oB=bB; oQ=bQ; oK=bK; pP=wP; pR=wR; pN=wN; pB=wB; pQ=wQ; pK=wK;
			}
			ulong occuOpp=piecePos[pP]|piecePos[pR]|piecePos[pN]|piecePos[pB]|piecePos[pQ];
			ulong rangeLimit=~(piecePos[oP]|piecePos[oR]|piecePos[oN]|piecePos[oB]|piecePos[oQ]|piecePos[oK]);
			ulong disambRange;

			// 資料歸零
			canStopCheck=0;
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
			result=slideRayRU[p, data=(int)(occuF>>occuShiftF[p]&0x3F)];
			canAttackOppKing[oB]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitRU[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&(position[p1]>>3==whoseMove)) pinBySelf[p1]=4;
			result=slideRayLD[p, data];
			canAttackOppKing[oB]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitLD[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&(position[p1]>>3==whoseMove)) pinBySelf[p1]=4;
			result=slideRayRD[p, data=(int)(occuB>>occuShiftB[p]&0x3F)];
			canAttackOppKing[oB]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitRD[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&(position[p1]>>3==whoseMove)) pinBySelf[p1]=8;
			result=slideRayLU[p, data];
			canAttackOppKing[oB]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitLU[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&(position[p1]>>3==whoseMove)) pinBySelf[p1]=8;

			// 右左
			result=slideRayR[p, data=(int)(occuH>>occuShiftH[p]&0x3F)];
			canAttackOppKing[oR]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitR[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF; p3=pData>>16;
			if((position[p2]==oR||position[p2]==oQ)&&position[p1]>>3==whoseMove) pinBySelf[p1]=1;
			result=slideRayL[p, data];
			canAttackOppKing[oR]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitL[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF; p3=pData>>16;
			if((position[p2]==oR||position[p2]==oQ)&&position[p1]>>3==whoseMove) pinBySelf[p1]=1;

			// 上下
			result=slideRayU[p, data=(int)(occuV>>occuShiftV[p]&0x3F)];
			canAttackOppKing[oR]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitU[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oR||position[p2]==oQ)&&position[p1]>>3==whoseMove) pinBySelf[p1]=2;
			result=slideRayD[p, data];
			canAttackOppKing[oR]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitD[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oR||position[p2]==oQ)&&position[p1]>>3==whoseMove) pinBySelf[p1]=2;

			// 處理我方國王
			p=kingPos[whoseMove];
			if((result=((whoseMove==WT?pieceRangeWP[p]:pieceRangeBP[p])&piecePos[pP]))!=0)						// 由於合法性檢查已經排除了雙兵將跟雙騎士將
				{ checkPieceCount++; canStopCheck|=result; }
			if((result=(pieceRangeN[p]&piecePos[pN]))!=0) { checkPieceCount++; canStopCheck|=result; }			// 所以這邊這兩種都只要算一次即可

			// 斜向
			pPos=piecePos[pB]|piecePos[pQ];
			result=slideRayRU[p, data=(int)(occuF>>occuShiftF[p]&0x3F)];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitRU[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=4;
			}
			result=slideRayLD[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitLD[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=4;
			}
			result=slideRayRD[p, data=(int)(occuB>>occuShiftB[p]&0x3F)];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitRD[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=8;
			}
			result=slideRayLU[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitLU[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=8;
			}

			// 右左
			pPos=piecePos[pR]|piecePos[pQ];
			result=slideRayR[p, data=(int)(occuH>>occuShiftH[p]&0x3F)];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitR[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF; p3=pData>>16;
				if((position[p2]==pR||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=1;
			}
			result=slideRayL[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitL[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF; p3=pData>>16;
				if((position[p2]==pR||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=1;
			}

			// 上下
			result=slideRayU[p, data=(int)(occuV>>occuShiftV[p]&0x3F)];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitU[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pR||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=2;
			}
			result=slideRayD[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitD[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pR||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=2;
			}

			// 生成 attackByOpp 資料
			attackByOpp=pieceRangeK[kingPos[1-whoseMove]];
			for(i=0;i<pieceCount[pP];i++) attackByOpp|=(whoseMove==BC?pieceRangeWP[pieceList[pP, i]]:pieceRangeBP[pieceList[pP, i]]);
			for(i=0;i<pieceCount[pN];i++) attackByOpp|=pieceRangeN[pieceList[pN, i]];
			for(i=0;i<pieceCount[pR];i++) attackByOpp|=slideRangeH[p=pieceList[pR, i], (occuH^mask[kingPos[whoseMove]])>>occuShiftH[p]&0x3F]|
				slideRangeV[p, (occuV^maskV[kingPos[whoseMove]])>>occuShiftV[p]&0x3F];
			for(i=0;i<pieceCount[pB];i++) attackByOpp|=slideRangeF[p=pieceList[pB, i], (occuF^maskF[kingPos[whoseMove]])>>occuShiftF[p]&0x3F]|
				slideRangeB[p, (occuB^maskB[kingPos[whoseMove]])>>occuShiftB[p]&0x3F];
			for(i=0;i<pieceCount[pQ];i++) attackByOpp|=slideRangeH[p=pieceList[pQ, i], (occuH^mask[kingPos[whoseMove]])>>occuShiftH[p]&0x3F]|
				slideRangeV[p, (occuV^maskV[kingPos[whoseMove]])>>occuShiftV[p]&0x3F]|slideRangeF[p, (occuF^maskF[kingPos[whoseMove]])>>occuShiftF[p]&0x3F]|
				slideRangeB[p, (occuB^maskB[kingPos[whoseMove]])>>occuShiftB[p]&0x3F];

			// 資料生成完畢，生成合法棋步清單（生成的時候直接判斷完合法性，沒有殆合法棋步的生成階段）

			// 國王棋步（普通的）
			for(j=0;(ta=pieceRuleK[so=kingPos[whoseMove], j])!=NS;j++)
				if((attackByOpp&mask[ta])==0) {	// 唯一的條件是目的地不能被攻擊
					if((cp=position[ta])==0) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oK<<otS)|((ulong)oK<<ntS)|len3;
					else if(cp>>3!=whoseMove) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oK<<otS)|((ulong)oK<<ntS)|((ulong)cp<<cpS)|len4;
				}

			// 雙將軍的情況就不用繼續了
			if(checkPieceCount==2) {
#if DEBUG
				totalMoveCount[1]+=l;
				totalMoveCount[4]+=l;
#endif
				moveListLength[depth]=(byte)l; return;
			}

			if(canStopCheck!=0) rangeLimit&=canStopCheck;

			// 其他的棋步一律只有當不是雙將軍的時候才有可能下
			// 不管有沒有將軍情況幾乎都一樣，用同一組程式判斷

			// 小兵
			if(whoseMove==WT) {
				for(i=0;i<pieceCount[wP];i++) {
					so=pieceList[wP, i];
					if(position[ta=(byte)(so+8)]==b0&&(pinByOpp[so]&relDirV)==0) {
						if((so>>3)==6) {
							if((rangeLimit&mask[ta])!=0) {
								moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wN<<ntS)|len4;
								moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wR<<ntS)|len4;
								moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wB<<ntS)|len4;
								moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wQ<<ntS)|len4;
							}
						}
					}
					if((ta=pieceRuleWP[so, 0])!=NS) {
						if((pinByOpp[so]&relDir[so, ta])==0&&(rangeLimit&mask[ta])!=0) {
							if((cp=position[ta])>>3==BC) {
								if((so>>3)!=6) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wP<<ntS)|((ulong)cp<<cpS)|len4;
							}
						}
						if((ta=pieceRuleWP[so, 1])!=NS&&(pinByOpp[so]&relDir[so, ta])==0&&(rangeLimit&mask[ta])!=0) {
							if((cp=position[ta])>>3==BC) {
								if((so>>3)!=6) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wP<<ntS)|((ulong)cp<<cpS)|len4;
							}
						}
					}
				}
			} else {
				for(i=0;i<pieceCount[bP];i++) {
					so=pieceList[bP, i];
					if(position[ta=(byte)(so-8)]==b0&&(pinByOpp[so]&relDirV)==0) {
						if((so>>3)==1) {
							if((rangeLimit&mask[ta])!=0) {
								moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bN<<ntS)|len4;
								moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bR<<ntS)|len4;
								moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bB<<ntS)|len4;
								moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bQ<<ntS)|len4;
							}
						}
					}
					if((ta=pieceRuleBP[so, 0])!=NS) {
						if((pinByOpp[so]&relDir[so, ta])==0&&(rangeLimit&mask[ta])!=0) {
							if((cp=position[ta])>>3==WT&&cp!=0) {
								if((so>>3)!=1) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bP<<ntS)|((ulong)cp<<cpS)|len4;
							}
						}
						if((ta=pieceRuleBP[so, 1])!=NS&&(pinByOpp[so]&relDir[so, ta])==0&&(rangeLimit&mask[ta])!=0) {
							if((cp=position[ta])>>3==WT&&cp!=0) {
								if((so>>3)!=1) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bP<<ntS)|((ulong)cp<<cpS)|len4;
							}
						}
					}
				}
			}

			// 騎士棋步
			if(pieceCount[oN]==1) { moveRange[p=pieceList[oN, 0]]=(pinByOpp[p]==0?pieceRangeN[p]&rangeLimit:0); disambRange=0; } else if(pieceCount[oN]==2) {
				p1=pieceList[oN, 0]; p2=pieceList[oN, 1];
				moveRange[p1]=(pinByOpp[p1]==0?pieceRangeN[p1]&rangeLimit:0);
				moveRange[p2]=(pinByOpp[p2]==0?pieceRangeN[p2]&rangeLimit:0);
				disambRange=moveRange[p1]&moveRange[p2];
			} else {
				for(i=0;i<pieceCount[oN];i++) moveRange[p=pieceList[oN, i]]=(pinByOpp[p]==0?pieceRangeN[p]&rangeLimit:0);
				disambRange=0;
				for(i=0;i<pieceCount[oN];i++) for(j=i+1;j<pieceCount[oN];j++)
						disambRange|=moveRange[pieceList[oN, i]]&moveRange[pieceList[oN, j]];
			}
			for(i=0;i<pieceCount[oN];i++) {
				// 只搜尋（吃子 XOR 將軍）或消歧義。消歧義無論如何都得搜尋，因為需要把所有的消歧義棋步都正確註冊
				m=moveRange[so=pieceList[oN, i]]&(occuOpp^(pinBySelf[so]==0?canAttackOppKing[oN]:0xFFFFFFFFFFFFFFFF)|disambRange);
				while(m!=0) {
					n=m; ta=0;
					if((n&0xFFFFFFFF00000000)!=0) { n>>=32; ta|=32; } if((n&0xFFFF0000)!=0) { n>>=16; ta|=16; } if((n&0xFF00)!=0) { n>>=8; ta|=8; }
					if((n&0xF0)!=0) { n>>=4; ta|=4; } if((n&0xC)!=0) { n>>=2; ta|=2; } if(n==2) ta|=1; m^=mask[ta];
					if((cp=position[ta])==0) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oN<<otS)|((ulong)oN<<ntS)|len3;
					else moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oN<<otS)|((ulong)oN<<ntS)|((ulong)cp<<cpS)|len4;
				}
			}

			// 城堡棋步
			if(pieceCount[oR]==1) {
				moveRange[so=pieceList[oR, 0]]=(
						((pinByOpp[so]&relDirH)==0?slideRangeH[so, (occuH>>occuShiftH[so])&0x3F]:0)|
						((pinByOpp[so]&relDirV)==0?slideRangeV[so, (occuV>>occuShiftV[so])&0x3F]:0)
					)&rangeLimit;
				disambRange=0;
			} else {
				for(i=0;i<pieceCount[oR];i++) {
					moveRange[so=pieceList[oR, i]]=(
						((pinByOpp[so]&relDirH)==0?slideRangeH[so, (occuH>>occuShiftH[so])&0x3F]:0)|
						((pinByOpp[so]&relDirV)==0?slideRangeV[so, (occuV>>occuShiftV[so])&0x3F]:0)
					)&rangeLimit;
				}
				disambRange=0;
				for(i=0;i<pieceCount[oR];i++) for(j=i+1;j<pieceCount[oR];j++)
						disambRange|=moveRange[pieceList[oR, i]]&moveRange[pieceList[oR, j]];
			}
			for(i=0;i<pieceCount[oR];i++) {
				m=moveRange[so=pieceList[oR, i]]&(occuOpp^(canAttackOppKing[oR]|discovRange[pinBySelf[so], so])|disambRange);
				while(m!=0) {
					n=m; ta=0;
					if((n&0xFFFFFFFF00000000)!=0) { n>>=32; ta|=32; } if((n&0xFFFF0000)!=0) { n>>=16; ta|=16; } if((n&0xFF00)!=0) { n>>=8; ta|=8; }
					if((n&0xF0)!=0) { n>>=4; ta|=4; } if((n&0xC)!=0) { n>>=2; ta|=2; } if(n==2) ta|=1; m^=mask[ta];
					if((cp=position[ta])==0) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oR<<otS)|((ulong)oR<<ntS)|len3;
					else moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oR<<otS)|((ulong)oR<<ntS)|((ulong)cp<<cpS)|len4;
				}
			}

			// 主教棋步
			if(pieceCount[oB]==1) {
				moveRange[so=pieceList[oB, 0]]=(
						((pinByOpp[so]&relDirF)==0?slideRangeF[so, (occuF>>occuShiftF[so])&0x3F]:0)|
						((pinByOpp[so]&relDirB)==0?slideRangeB[so, (occuB>>occuShiftB[so])&0x3F]:0)
					)&rangeLimit;
				disambRange=0;
			} else {
				for(i=0;i<pieceCount[oB];i++) {
					moveRange[so=pieceList[oB, i]]=(
						((pinByOpp[so]&relDirF)==0?slideRangeF[so, (occuF>>occuShiftF[so])&0x3F]:0)|
						((pinByOpp[so]&relDirB)==0?slideRangeB[so, (occuB>>occuShiftB[so])&0x3F]:0)
					)&rangeLimit;
				}
				disambRange=0;
				if(pieceCount[oB]!=2||boardColor[pieceList[oB, 0]]==boardColor[pieceList[oB, 1]])	// 用棋盤顏色做快速排除
					for(i=0;i<pieceCount[oB];i++) for(j=i+1;j<pieceCount[oB];j++)
							disambRange|=moveRange[pieceList[oB, i]]&moveRange[pieceList[oB, j]];
			}
			for(i=0;i<pieceCount[oB];i++) {
				m=moveRange[so=pieceList[oB, i]]&(occuOpp^(canAttackOppKing[oB]|discovRange[pinBySelf[so], so])|disambRange);
				while(m!=0) {
					n=m; ta=0;
					if((n&0xFFFFFFFF00000000)!=0) { n>>=32; ta|=32; } if((n&0xFFFF0000)!=0) { n>>=16; ta|=16; } if((n&0xFF00)!=0) { n>>=8; ta|=8; }
					if((n&0xF0)!=0) { n>>=4; ta|=4; } if((n&0xC)!=0) { n>>=2; ta|=2; } if(n==2) ta|=1; m^=mask[ta];
					if((cp=position[ta])==0) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oB<<otS)|((ulong)oB<<ntS)|len3;
					else moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oB<<otS)|((ulong)oB<<ntS)|((ulong)cp<<cpS)|len4;
				}
			}

			// 皇后棋步
			if(pieceCount[oQ]==1) {
				moveRange[so=pieceList[oQ, 0]]=(
						((pinByOpp[so]&relDirH)==0?slideRangeH[so, (occuH>>occuShiftH[so])&0x3F]:0)|
						((pinByOpp[so]&relDirV)==0?slideRangeV[so, (occuV>>occuShiftV[so])&0x3F]:0)|
						((pinByOpp[so]&relDirF)==0?slideRangeF[so, (occuF>>occuShiftF[so])&0x3F]:0)|
						((pinByOpp[so]&relDirB)==0?slideRangeB[so, (occuB>>occuShiftB[so])&0x3F]:0)
					)&rangeLimit;
				disambRange=0;
			} else {
				for(i=0;i<pieceCount[oQ];i++) {
					moveRange[so=pieceList[oQ, i]]=(
						((pinByOpp[so]&relDirH)==0?slideRangeH[so, (occuH>>occuShiftH[so])&0x3F]:0)|
						((pinByOpp[so]&relDirV)==0?slideRangeV[so, (occuV>>occuShiftV[so])&0x3F]:0)|
						((pinByOpp[so]&relDirF)==0?slideRangeF[so, (occuF>>occuShiftF[so])&0x3F]:0)|
						((pinByOpp[so]&relDirB)==0?slideRangeB[so, (occuB>>occuShiftB[so])&0x3F]:0)
					)&rangeLimit;
				}
				disambRange=0;
				for(i=0;i<pieceCount[oQ];i++) for(j=i+1;j<pieceCount[oQ];j++)
						disambRange|=moveRange[pieceList[oQ, i]]&moveRange[pieceList[oQ, j]];
			}
			for(i=0;i<pieceCount[oQ];i++) {
				m=moveRange[so=pieceList[oQ, i]]&(occuOpp^(canAttackOppKing[oQ]|discovRange[pinBySelf[so], so])|disambRange);
				while(m!=0) {
					n=m; ta=0;
					if((n&0xFFFFFFFF00000000)!=0) { n>>=32; ta|=32; } if((n&0xFFFF0000)!=0) { n>>=16; ta|=16; } if((n&0xFF00)!=0) { n>>=8; ta|=8; }
					if((n&0xF0)!=0) { n>>=4; ta|=4; } if((n&0xC)!=0) { n>>=2; ta|=2; } if(n==2) ta|=1; m^=mask[ta];
					if((cp=position[ta])==0) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|len3;
					else moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4;
				}
			}

			// 入堡棋步，只有沒有被將軍的時候可以做
			if(checkPieceCount==0) {
				if(whoseMove==WT) {
					if((castlingState[depth]&cwK)!=0&&position[5]==b0&&position[6]==b0&&(attackByOpp&(mask[5]|mask[6]))==0) moveList[depth, l++]=wOO;
				} else {
					if((castlingState[depth]&cbK)!=0&&position[61]==b0&&position[62]==b0&&(attackByOpp&(mask[61]|mask[62]))==0) moveList[depth, l++]=bOO;
				}
			}

			moveListLength[depth]=(byte)l;
#if DEBUG
			totalMoveCount[1]+=l;
			totalMoveCount[4]+=l;
#endif

			// 合法棋步生成完畢，檢驗將軍對方與否
			Array.Clear(DisambListLength, 0, 1024);
			for(i=0;i<l;i++) {
				m=moveList[depth, i];
				so=(byte)(m&0x3F);
				ta=(byte)((m>>taS)&0x3F);
				de=(byte)((m>>deS)&0x3F);
				ot=(byte)((m>>otS)&0xF);
				nt=(byte)((m>>ntS)&0xF);
				mi=(byte)((m>>miS)&0xF);
				tag=0;

				// 將軍判斷
				if(mi!=OOMove) {
					if((canAttackOppKing[nt]&mask[ta])!=0) tag=tgCheck;							// 直接走就進入攻擊位置（含升變）
					else if((pinBySelf[so]&relDir[so, ta])!=0) tag=tgCheck;						// 閃擊
				}
				// 注意到如果一個可以入堡的國王被自己釘住，
				// 那只有可能是對方的國王在另一側，所以如果做入堡動作一定會導致將軍
				else if(pinBySelf[so]==1) tag=tgCheck;											// 入堡的情況要多做一種「入堡閃擊」的判斷
				else if(mi==OOMove&&(canAttackOppKing[oR]&mask[so+1])!=0) tag=tgCheck;			// 一般的王側入堡將軍

				if(ot!=wP&&ot!=bP&&ot!=wK&&ot!=bK) DisambList[ta, ot, DisambListLength[ta, ot]++]=so;	// 登錄消歧義名單
				moveList[depth, i]|=(ulong)tag<<tgS;
			}

			// 全部的合法棋步都出來之後，進行消歧義標籤計算
			for(i=0;i<l;i++) {
				ta=(byte)((moveList[depth, i]>>taS)&0x3F);
				ot=(byte)((moveList[depth, i]>>otS)&0xF);
				if((j=DisambListLength[ta, ot])<=1) continue;
				so=(byte)(moveList[depth, i]&0x3F); cx=0; cy=0;
				for(k=0;k<j;k++) {
					if((r=relDir[DisambList[ta, ot, k], so])==13) cx++;
					if(r==14) cy++;
				}
				moveList[depth, i]|=((ulong)(cx==0?b1:(cy==0?b2:b3))<<dbS);
			}
		}
	}
}
