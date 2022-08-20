
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

		// 此時只需要搜尋兵的直走棋步就好，
		// 從而也不用管一次閃兩子的橫向閃擊問題，
		// 也不用生成 attackByOpp 資料，
		// canAttackOppKing 只要生成兵的部分即可，
		// 也不會有上下釘住的情況
		// 也不用處理消歧義清單

		private void computeLegalMoves2() {
			int i, j=0, pData;
			byte so, ta, p;
			int data, p1, p2;
			ulong result, pPos, d;

			// 設置代碼
			if(whoseMove==WT) { oP=wP; oR=wR; oN=wN; oB=wB; oQ=wQ; oK=wK; pP=bP; pR=bR; pN=bN; pB=bB; pQ=bQ; pK=bK; }
			else { oP=bP; oR=bR; oN=bN; oB=bB; oQ=bQ; oK=bK; pP=wP; pR=wR; pN=wN; pB=wB; pQ=wQ; pK=wK; }

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

			// 斜向
			result=slideRayRU[p, data=(int)(occuFS>>occuShiftFS[p]&0x3F)];
			pData=slideHitRU[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&position[p1]==oP) pinBySelf[p1]=true;
			result=slideRayLD[p, data];
			pData=slideHitLD[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&position[p1]==oP) pinBySelf[p1]=true;
			result=slideRayRD[p, data=(int)(occuBS>>occuShiftBS[p]&0x3F)];
			pData=slideHitRD[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&position[p1]==oP) pinBySelf[p1]=true;
			result=slideRayLU[p, data];
			pData=slideHitLU[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&position[p1]==oP) pinBySelf[p1]=true;

			// 右左
			result=slideRayR[p, data=(int)(occuH>>occuShiftH[p]&0x3F)];
			pData=slideHitR[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF;
			if((position[p2]==oR||position[p2]==oQ)&&position[p1]==oP) pinBySelf[p1]=true;
			result=slideRayL[p, data];
			pData=slideHitL[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF;
			if((position[p2]==oR||position[p2]==oQ)&&position[p1]==oP) pinBySelf[p1]=true;


			// 處理我方國王
			p=kingPos[whoseMove];
			if((result=((whoseMove==WT?pieceRangeWP[p]:pieceRangeBP[p])&piecePos[pP]))!=0)
				{ checkPieceCount++; canStopCheck|=result; }													// 由於合法性檢查已經排除了雙兵將跟雙騎士將
			if((result=(pieceRangeN[p]&piecePos[pN]))!=0) { checkPieceCount++; canStopCheck|=result; }			// 所以這邊這兩種都只要算一次即可

			// 斜向
			pPos=piecePos[pB]|piecePos[pQ];
			result=slideRayRU[p, data=(int)(occuFS>>occuShiftFS[p]&0x3F)];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitRU[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]==oP) pinByOpp[p1]=true;
			}
			result=slideRayLD[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitLD[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]==oP) pinByOpp[p1]=true;
			}
			result=slideRayRD[p, data=(int)(occuBS>>occuShiftBS[p]&0x3F)];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitRD[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]==oP) pinByOpp[p1]=true;
			}
			result=slideRayLU[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitLU[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]==oP) pinByOpp[p1]=true;
			}

			// 右左
			pPos=piecePos[pR]|piecePos[pQ];
			result=slideRayR[p, data=(int)(occuH>>occuShiftH[p]&0x3F)];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitR[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF;
				if((position[p2]==pR||position[p2]==pQ)&&position[p1]==oP) pinByOpp[p1]=true;
			}
			result=slideRayL[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitL[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF;
				if((position[p2]==pR||position[p2]==pQ)&&position[p1]==oP) pinByOpp[p1]=true;
			}

			// 上下
			result=slideRayU[p, data=(int)(occuV>>occuShiftV[p]&0x3F)];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; }
			result=slideRayD[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; }

			// 資料生成完畢，接著生成合法棋步（直接判斷完畢，沒有殆合法棋步的過程）

			if(checkPieceCount==2) { moveListLength[depth]=0; return; } // 雙將軍的情況就不用繼續了，不可能走兵

			d=((ulong)oP<<otS)|((ulong)oP<<ntS)|len2|0x10000000000;

			if(checkPieceCount==1) {	// 如果正面臨將軍，需要多判斷能不能阻擋將軍
				if(whoseMove==WT) {
					for(i=0;i<pieceCount[wP];i++) {
						so=pieceList[wP, i]; if(!pinByOpp[so]&&!pinBySelf[so]) {	// 不能被釘住、也不能做閃擊是兩碼的必要條件
							if(position[ta=(byte)(so+8)]==b0) {
								if((so>>3)!=6) {
									if((canStopCheck&mask[ta])!=0) moveList[depth, j++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|d;
									if((so>>3)==1&&position[ta=(byte)(so+16)]==b0&&(canStopCheck&mask[ta])!=0) moveList[depth, j++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|d;
								}
							}
						}
					}
				} else {
					for(i=0;i<pieceCount[bP];i++) {
						so=pieceList[bP, i]; if(!pinByOpp[so]&&!pinBySelf[so]) {
							if(position[ta=(byte)(so-8)]==b0) {
								if((so>>3)!=1) {
									if((canStopCheck&mask[ta])!=0) moveList[depth, j++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|d;
									if((so>>3)==6&&position[ta=(byte)(so-16)]==b0&&(canStopCheck&mask[ta])!=0) moveList[depth, j++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|d;
								}
							}
						}
					}
				}
			} else {
				if(whoseMove==WT) {
					for(i=0;i<pieceCount[wP];i++) {
						so=pieceList[wP, i]; if(!pinByOpp[so]&&!pinBySelf[so]) {
							if(position[ta=(byte)(so+8)]==b0) {
								if((so>>3)!=6) {
									moveList[depth, j++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|d;
									if((so>>3)==1&&position[ta=(byte)(so+16)]==b0) moveList[depth, j++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|d;
								}
							}
						}
					}
				} else {
					for(i=0;i<pieceCount[bP];i++) {
						so=pieceList[bP, i]; if(!pinByOpp[so]&&!pinBySelf[so]) {
							if(position[ta=(byte)(so-8)]==b0) {
								if((so>>3)!=1) {
									moveList[depth, j++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|d;
									if((so>>3)==6&&position[ta=(byte)(so-16)]==b0) moveList[depth, j++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|d;
								}
							}
						}
					}
				}
			}
			
			moveListLength[depth]=(byte)j;
#if DEBUG
			pseudoMoveCount[2]+=j;
			totalMoveCount[2]+=j;
#endif
		}

		/////////////////////////////////
		// 長度為 3 的棋步
		/////////////////////////////////

		// 兵的部分只要搜尋直走，入堡只要搜尋 O-O
		// 不用管一次閃兩子的橫向閃擊問題

		private void computeLegalMoves3() {
			int i, j, k, l=0, pData;
			byte p, r;
			byte tag, so, ta, ot, cp;
			int data, p1, p2, p3;
			ulong result, pPos;
			int cx, cy;

			// 設置代碼
			if(whoseMove==WT) { oP=wP; oR=wR; oN=wN; oB=wB; oQ=wQ; oK=wK; pP=bP; pR=bR; pN=bN; pB=bB; pQ=bQ; pK=bK;}
			else { oP=bP; oR=bR; oN=bN; oB=bB; oQ=bQ; oK=bK; pP=wP; pR=wR; pN=wN; pB=wB; pQ=wQ; pK=wK;}

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
			result=slideRayRU[p, data=(int)(occuFS>>occuShiftFS[p]&0x3F)];
			canAttackOppKing[oB]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitRU[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&(position[p1]>>3==whoseMove)) pinBySelf[p1]=true;
			result=slideRayLD[p, data];
			canAttackOppKing[oB]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitLD[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&(position[p1]>>3==whoseMove)) pinBySelf[p1]=true;
			result=slideRayRD[p, data=(int)(occuBS>>occuShiftBS[p]&0x3F)];
			canAttackOppKing[oB]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitRD[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&(position[p1]>>3==whoseMove)) pinBySelf[p1]=true;
			result=slideRayLU[p, data];
			canAttackOppKing[oB]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitLU[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&(position[p1]>>3==whoseMove)) pinBySelf[p1]=true;

			// 右左
			result=slideRayR[p, data=(int)(occuH>>occuShiftH[p]&0x3F)];
			canAttackOppKing[oR]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitR[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF; p3=pData>>16;
			if((position[p2]==oR||position[p2]==oQ)&&position[p1]>>3==whoseMove) pinBySelf[p1]=true;
			result=slideRayL[p, data];
			canAttackOppKing[oR]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitL[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF; p3=pData>>16;
			if((position[p2]==oR||position[p2]==oQ)&&position[p1]>>3==whoseMove) pinBySelf[p1]=true;

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
			if((result=(pieceRangeN[p]&piecePos[pN]))!=0) { checkPieceCount++; canStopCheck|=result; }			// 所以這邊這兩種都只要算一次即可

			// 斜向
			pPos=piecePos[pB]|piecePos[pQ];
			result=slideRayRU[p, data=(int)(occuFS>>occuShiftFS[p]&0x3F)];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitRU[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=true;
			}
			result=slideRayLD[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitLD[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=true;
			}
			result=slideRayRD[p, data=(int)(occuBS>>occuShiftBS[p]&0x3F)];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitRD[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=true;
			}
			result=slideRayLU[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitLU[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=true;
			}

			// 右左
			pPos=piecePos[pR]|piecePos[pQ];
			result=slideRayR[p, data=(int)(occuH>>occuShiftH[p]&0x3F)];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitR[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF; p3=pData>>16;
				if((position[p2]==pR||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=true;
			}
			result=slideRayL[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitL[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF; p3=pData>>16;
				if((position[p2]==pR||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=true;
			}

			// 上下
			result=slideRayU[p, data=(int)(occuV>>occuShiftV[p]&0x3F)];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitU[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pR||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=true;
			}
			result=slideRayD[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitD[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pR||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=true;
			}

			// 生成 attackByOpp 資料
			attackByOpp=pieceRangeK[kingPos[1-whoseMove]];
			for(i=0;i<pieceCount[pP];i++) attackByOpp|=(whoseMove==BC?pieceRangeWP[pieceList[pP, i]]:pieceRangeBP[pieceList[pP, i]]);
			for(i=0;i<pieceCount[pN];i++) attackByOpp|=pieceRangeN[pieceList[pN, i]];
			for(i=0;i<pieceCount[pR];i++) attackByOpp|=slideRangeH[p=pieceList[pR, i], (occuH^mask[kingPos[whoseMove]])>>occuShiftH[p]&0x3F]|
				slideRangeV[p, (occuV^maskV[kingPos[whoseMove]])>>occuShiftV[p]&0x3F];
			for(i=0;i<pieceCount[pB];i++) attackByOpp|=slideRangeFS[p=pieceList[pB, i], (occuFS^maskFS[kingPos[whoseMove]])>>occuShiftFS[p]&0x3F]|
				slideRangeBS[p, (occuBS^maskBS[kingPos[whoseMove]])>>occuShiftBS[p]&0x3F];
			for(i=0;i<pieceCount[pQ];i++) attackByOpp|=slideRangeH[p=pieceList[pQ, i], (occuH^mask[kingPos[whoseMove]])>>occuShiftH[p]&0x3F]|
				slideRangeV[p, (occuV^maskV[kingPos[whoseMove]])>>occuShiftV[p]&0x3F]|slideRangeFS[p, (occuFS^maskFS[kingPos[whoseMove]])>>occuShiftFS[p]&0x3F]|
				slideRangeBS[p, (occuBS^maskBS[kingPos[whoseMove]])>>occuShiftBS[p]&0x3F];

			// 資料生成完畢，生成殆合法棋步

			// 小兵，只搜尋直走
			if(whoseMove==WT) {
				for(i=0;i<pieceCount[wP];i++) {
					so=pieceList[wP, i];
					if(position[ta=(byte)(so+8)]==b0&&(so>>3)!=6) {
						TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wP<<ntS)|len2;
						if((so>>3)==1&&position[ta=(byte)(so+16)]==b0) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wP<<ntS)|len2;
					}
				}
			} else {
				for(i=0;i<pieceCount[bP];i++) {
					so=pieceList[bP, i];
					if(position[ta=(byte)(so-8)]==b0&&(so>>3)!=1) {
						TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bP<<ntS)|len2;
						if((so>>3)==6&&position[ta=(byte)(so-16)]==b0) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bP<<ntS)|len2;
					}					
				}
			}

			// 騎士棋步
			for(i=0;i<pieceCount[oN];i++)
				if(!pinByOpp[so=pieceList[oN, i]])	// 騎士只要被釘住就不能動
					for(j=0;(ta=pieceRuleN[so, j])!=NS;j++)
						if((cp=position[ta])==0) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oN<<otS)|((ulong)oN<<ntS)|len3;

			// 國王棋步（普通的）
			for(j=0;(ta=pieceRuleK[so=kingPos[whoseMove], j])!=NS;j++)
				if((cp=position[ta])==0) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oK<<otS)|((ulong)oK<<ntS)|len3;

			// 城堡棋步
			for(i=0;i<pieceCount[oR];i++)
				for(j=0;HVRule[so=pieceList[oR, i], j, 0]!=NS;j++)
					for(k=0;(ta=HVRule[so, j, k])!=NS;k++) {
						if((cp=position[ta])==0) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oR<<otS)|((ulong)oR<<ntS)|len3;
						else break;
					}

			// 主教棋步
			for(i=0;i<pieceCount[oB];i++)
				for(j=0;DIRule[so=pieceList[oB, i], j, 0]!=NS;j++)
					for(k=0;(ta=DIRule[so, j, k])!=NS;k++) {
						if((cp=position[ta])==0) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oB<<otS)|((ulong)oB<<ntS)|len3;
						else break;
					}

			// 皇后棋步
			for(i=0;i<pieceCount[oQ];i++) {
				for(j=0;HVRule[so=pieceList[oQ, i], j, 0]!=NS;j++)
					for(k=0;(ta=HVRule[so, j, k])!=NS;k++) {
						if((cp=position[ta])==0) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|len3;
						else break;
					}
				for(j=0;DIRule[so=pieceList[oQ, i], j, 0]!=NS;j++)
					for(k=0;(ta=DIRule[so, j, k])!=NS;k++) {
						if((cp=position[ta])==0) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|len3;
						else break;
					}
			}

			// 入堡棋步，這邊只檢查入堡權、當下的將軍以及中間的格子是否空的，攻擊檢查待會再做
			if(checkPieceCount==0) {
				if(whoseMove==WT) {
					if((castlingState[depth]&cwK)!=0&&position[5]==b0&&position[6]==b0) TML[l++]=wOO;
				} else {
					if((castlingState[depth]&cbK)!=0&&position[61]==b0&&position[62]==b0) TML[l++]=bOO;
				}
			}

			// 殆合法棋步以及所有需要的資料都生成完畢，檢驗棋步的合法性、以及將軍對方與否
			Array.Clear(DisambListLength, 0, 1024);
			for(i=0, j=0;i<l;i++) {
				tag=checkMoveNoEP(TML[i]);
				if(tag>0) {
					so=(byte)(TML[i]&0x3F);
					ta=(byte)((TML[i]>>taS)&0x3F);
					ot=(byte)((TML[i]>>otS)&0xF);
					if(ot!=wP&&ot!=bP&&ot!=wK&&ot!=bK)
						DisambList[ta, ot, DisambListLength[ta, ot]++]=so;	// 登錄消歧義名單
					moveList[depth, j++]=TML[i]|((ulong)tag<<tgS);
				}
			}
			moveListLength[depth]=(byte)j;
#if DEBUG
			pseudoMoveCount[3]+=l;
			totalMoveCount[3]+=j;
#endif
			// 全部的合法棋步都出來之後，進行消歧義標籤計算
			for(i=0;i<j;i++) {
				ta=(byte)((moveList[depth, i]>>taS)&0x3F);
				ot=(byte)((moveList[depth, i]>>otS)&0xF);
				if((l=DisambListLength[ta, ot])<=1) continue;
				so=(byte)(moveList[depth, i]&0x3F); cx=0; cy=0;
				for(k=0;k<l;k++) {
					if((r=relDir[DisambList[ta, ot, k], so])==1) cx++;
					if(r==0) cy++;
				}
				moveList[depth, i]|=((ulong)(cx==0?b1:(cy==0?b2:b3))<<dbS);
			}
		}

		// 不檢查吃過路兵的合法性檢查函數

		private byte checkMoveNoEP(ulong m) {
			byte so=(byte)(m&0x3F);
			byte ta=(byte)((m>>taS)&0x3F);
			byte de=(byte)((m>>deS)&0x3F);
			byte ot=(byte)((m>>otS)&0xF);
			byte nt=(byte)((m>>ntS)&0xF);
			byte cp=(byte)((m>>cpS)&0xF);
			byte mi=(byte)((m>>miS)&0xF);

			// 先排除不合法的情況

			// 沒有將軍的情況
			if(checkPieceCount==0) {
				// 如果不是國王（佔大多數），那只要檢查是否移動的棋子被釘住即可
				if(ot!=oK) {
					if(pinByOpp[so]&&relDir[so, ta]!=relDir[so, kingPos[whoseMove]]) return 0;
				}
					// 如果是國王
				else if(mi==OOMove&&(attackByOpp&mask[so+1])!=0) return 0;							// 檢查王側入堡的路上一格有沒有被攻擊
				else if(mi==OOOMove&&(attackByOpp&(mask[so-1]))!=0) return 0;					 	// 后側入堡的情況
				else if((attackByOpp&mask[ta])!=0) return 0;										// 檢查國王的目的地本身
			}

			// 單將軍的情況
			else if(checkPieceCount==1) {
				if(ot!=oK) {
					if((canStopCheck&mask[ta])==0||pinByOpp[so]&&
						relDir[so, ta]!=relDir[so, kingPos[whoseMove]]) return 0;					// 如果是一般棋子，那必須阻止對方的將軍，但那個棋子不能被釘住（騎士已經排除）
				} else if((attackByOpp&mask[ta])!=0) return 0;										// 如果動的是國王，必須閃到安全地帶（入堡在生成殆合法棋步時已經排除）
			}

			// 雙將軍的情況
			else if(ot!=oK||(attackByOpp&mask[ta])!=0) return 0;									// 移動的一定得是國王，而且目標不能被攻擊

			// 如果至此都沒有問題，那就表示棋步是合法的，進一步檢查這個棋步是否造成將軍對方
			if(mi!=OOMove&&mi!=OOOMove) {
				if((canAttackOppKing[nt]&mask[ta])!=0) return 2;									// 直接走就進入攻擊位置（含升變）
				else if(pinBySelf[so]&&
					(ot==oN||relDir[so, ta]!=relDir[so, kingPos[1-whoseMove]])) return 2;			// 閃擊
			} else {
				if(pinBySelf[so]) return 2;															// 入堡的情況要多做一種「入堡閃擊」的判斷
				// 注意到如果一個可以入堡的國王被自己釘住，那只有可能是對方的國王在另一側，所以如果做入堡動作一定會導致將軍
				else if(mi==OOMove&&(canAttackOppKing[oR]&mask[so+1])!=0) return 2;					// 一般的王側入堡將軍
				else if(mi==OOOMove&&(canAttackOppKing[oR]&mask[so-1])!=0) return 2;				// 一般的后側入堡將軍				
			}

			// 以上皆非的話，就表示沒有將軍對方
			return 1;
		}
		

		/////////////////////////////////
		// 長度為 4 的棋步
		/////////////////////////////////

		// 兵只要搜尋直走升變跟單純吃子兩種，入堡只要搜尋 O-O
		// 不用管一次閃兩子的橫向閃擊問題

		private void computeLegalMoves4() {
			int i, j, k, l=0, pData;
			byte p, r;
			byte tag, so, ta, ot, cp;
			int data, p1, p2, p3;
			ulong result, pPos;
			int cx, cy;

			// 設置代碼
			if(whoseMove==WT) { oP=wP; oR=wR; oN=wN; oB=wB; oQ=wQ; oK=wK; pP=bP; pR=bR; pN=bN; pB=bB; pQ=bQ; pK=bK;}
			else { oP=bP; oR=bR; oN=bN; oB=bB; oQ=bQ; oK=bK; pP=wP; pR=wR; pN=wN; pB=wB; pQ=wQ; pK=wK;}

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
			result=slideRayRU[p, data=(int)(occuFS>>occuShiftFS[p]&0x3F)];
			canAttackOppKing[oB]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitRU[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&(position[p1]>>3==whoseMove)) pinBySelf[p1]=true;
			result=slideRayLD[p, data];
			canAttackOppKing[oB]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitLD[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&(position[p1]>>3==whoseMove)) pinBySelf[p1]=true;
			result=slideRayRD[p, data=(int)(occuBS>>occuShiftBS[p]&0x3F)];
			canAttackOppKing[oB]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitRD[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&(position[p1]>>3==whoseMove)) pinBySelf[p1]=true;
			result=slideRayLU[p, data];
			canAttackOppKing[oB]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitLU[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&(position[p1]>>3==whoseMove)) pinBySelf[p1]=true;

			// 右左
			result=slideRayR[p, data=(int)(occuH>>occuShiftH[p]&0x3F)];
			canAttackOppKing[oR]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitR[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF; p3=pData>>16;
			if((position[p2]==oR||position[p2]==oQ)&&position[p1]>>3==whoseMove) pinBySelf[p1]=true;
			result=slideRayL[p, data];
			canAttackOppKing[oR]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitL[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF; p3=pData>>16;
			if((position[p2]==oR||position[p2]==oQ)&&position[p1]>>3==whoseMove) pinBySelf[p1]=true;

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
			if((result=(pieceRangeN[p]&piecePos[pN]))!=0) { checkPieceCount++; canStopCheck|=result; }			// 所以這邊這兩種都只要算一次即可

			// 斜向
			pPos=piecePos[pB]|piecePos[pQ];
			result=slideRayRU[p, data=(int)(occuFS>>occuShiftFS[p]&0x3F)];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitRU[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=true;
			}
			result=slideRayLD[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitLD[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=true;
			}
			result=slideRayRD[p, data=(int)(occuBS>>occuShiftBS[p]&0x3F)];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitRD[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=true;
			}
			result=slideRayLU[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitLU[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=true;
			}

			// 右左
			pPos=piecePos[pR]|piecePos[pQ];
			result=slideRayR[p, data=(int)(occuH>>occuShiftH[p]&0x3F)];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitR[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF; p3=pData>>16;
				if((position[p2]==pR||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=true;
			}
			result=slideRayL[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitL[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF; p3=pData>>16;
				if((position[p2]==pR||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=true;
			}

			// 上下
			result=slideRayU[p, data=(int)(occuV>>occuShiftV[p]&0x3F)];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitU[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pR||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=true;
			}
			result=slideRayD[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitD[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pR||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=true;
			}

			// 生成 attackByOpp 資料
			attackByOpp=pieceRangeK[kingPos[1-whoseMove]];
			for(i=0;i<pieceCount[pP];i++) attackByOpp|=(whoseMove==BC?pieceRangeWP[pieceList[pP, i]]:pieceRangeBP[pieceList[pP, i]]);
			for(i=0;i<pieceCount[pN];i++) attackByOpp|=pieceRangeN[pieceList[pN, i]];
			for(i=0;i<pieceCount[pR];i++) attackByOpp|=slideRangeH[p=pieceList[pR, i], (occuH^mask[kingPos[whoseMove]])>>occuShiftH[p]&0x3F]|
				slideRangeV[p, (occuV^maskV[kingPos[whoseMove]])>>occuShiftV[p]&0x3F];
			for(i=0;i<pieceCount[pB];i++) attackByOpp|=slideRangeFS[p=pieceList[pB, i], (occuFS^maskFS[kingPos[whoseMove]])>>occuShiftFS[p]&0x3F]|
				slideRangeBS[p, (occuBS^maskBS[kingPos[whoseMove]])>>occuShiftBS[p]&0x3F];
			for(i=0;i<pieceCount[pQ];i++) attackByOpp|=slideRangeH[p=pieceList[pQ, i], (occuH^mask[kingPos[whoseMove]])>>occuShiftH[p]&0x3F]|
				slideRangeV[p, (occuV^maskV[kingPos[whoseMove]])>>occuShiftV[p]&0x3F]|slideRangeFS[p, (occuFS^maskFS[kingPos[whoseMove]])>>occuShiftFS[p]&0x3F]|
				slideRangeBS[p, (occuBS^maskBS[kingPos[whoseMove]])>>occuShiftBS[p]&0x3F];

			// 資料生成完畢，生成殆合法棋步

			// 小兵
			if(whoseMove==WT) {
				for(i=0;i<pieceCount[wP];i++) {
					so=pieceList[wP, i];
					if(position[ta=(byte)(so+8)]==b0) {
						if((so>>3)==6) {
							TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wN<<ntS)|len4;
							TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wR<<ntS)|len4;
							TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wB<<ntS)|len4;
							TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wQ<<ntS)|len4;
						}
					}
					if((ta=pieceRuleWP[so, 0])!=NS) {
						if(side(cp=position[ta])==BC&&(so>>3)!=6) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wP<<ntS)|((ulong)cp<<cpS)|len4;
						if((ta=pieceRuleWP[so, 1])!=NS&&side(cp=position[ta])==BC&&(so>>3)!=6)
							TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wP<<ntS)|((ulong)cp<<cpS)|len4;
					}
				}
			} else {
				for(i=0;i<pieceCount[bP];i++) {
					so=pieceList[bP, i];
					if(position[ta=(byte)(so-8)]==b0) {
						if((so>>3)==1) {
							TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bN<<ntS)|len4;
							TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bR<<ntS)|len4;
							TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bB<<ntS)|len4;
							TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bQ<<ntS)|len4;
						}
					}
					if((ta=pieceRuleBP[so, 0])!=NS) {
						if(side(cp=position[ta])==WT&&(so>>3)!=1) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bP<<ntS)|((ulong)cp<<cpS)|len4;
						if((ta=pieceRuleBP[so, 1])!=NS&&side(cp=position[ta])==WT&&(so>>3)!=1)
							TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bP<<ntS)|((ulong)cp<<cpS)|len4;
					}
				}
			}

			// 騎士棋步
			for(i=0;i<pieceCount[oN];i++)
				if(!pinByOpp[so=pieceList[oN, i]])	// 騎士只要被釘住就不能動
					for(j=0;(ta=pieceRuleN[so, j])!=NS;j++)
						if((cp=position[ta])==0) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oN<<otS)|((ulong)oN<<ntS)|len3;
						else if(cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oN<<otS)|((ulong)oN<<ntS)|((ulong)cp<<cpS)|len4;

			// 國王棋步（普通的）
			for(j=0;(ta=pieceRuleK[so=kingPos[whoseMove], j])!=NS;j++)
				if((cp=position[ta])==0) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oK<<otS)|((ulong)oK<<ntS)|len3;
				else if(cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oK<<otS)|((ulong)oK<<ntS)|((ulong)cp<<cpS)|len4;

			// 城堡棋步
			for(i=0;i<pieceCount[oR];i++)
				for(j=0;HVRule[so=pieceList[oR, i], j, 0]!=NS;j++)
					for(k=0;(ta=HVRule[so, j, k])!=NS;k++) {
						if((cp=position[ta])==0) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oR<<otS)|((ulong)oR<<ntS)|len3;
						else { if(cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oR<<otS)|((ulong)oR<<ntS)|((ulong)cp<<cpS)|len4; break; }
					}

			// 主教棋步
			for(i=0;i<pieceCount[oB];i++)
				for(j=0;DIRule[so=pieceList[oB, i], j, 0]!=NS;j++)
					for(k=0;(ta=DIRule[so, j, k])!=NS;k++) {
						if((cp=position[ta])==0) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oB<<otS)|((ulong)oB<<ntS)|len3;
						else { if(cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oB<<otS)|((ulong)oB<<ntS)|((ulong)cp<<cpS)|len4; break; }
					}

			// 皇后棋步
			for(i=0;i<pieceCount[oQ];i++) {
				for(j=0;HVRule[so=pieceList[oQ, i], j, 0]!=NS;j++)
					for(k=0;(ta=HVRule[so, j, k])!=NS;k++) {
						if((cp=position[ta])==0) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|len3;
						else { if(cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4; break; }
					}
				for(j=0;DIRule[so=pieceList[oQ, i], j, 0]!=NS;j++)
					for(k=0;(ta=DIRule[so, j, k])!=NS;k++) {
						if((cp=position[ta])==0) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|len3;
						else { if(cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4; break; }
					}
			}

			// 入堡棋步，這邊只檢查入堡權、當下的將軍以及中間的格子是否空的，攻擊檢查待會再做
			if(checkPieceCount==0) {
				if(whoseMove==WT) {
					if((castlingState[depth]&cwK)!=0&&position[5]==b0&&position[6]==b0) TML[l++]=wOO;
				} else {
					if((castlingState[depth]&cbK)!=0&&position[61]==b0&&position[62]==b0) TML[l++]=bOO;
				}
			}

			// 殆合法棋步以及所有需要的資料都生成完畢，檢驗棋步的合法性、以及將軍對方與否
			Array.Clear(DisambListLength, 0, 1024);
			for(i=0, j=0;i<l;i++) {
				tag=checkMoveNoEP(TML[i]);
				if(tag>0) {
					so=(byte)(TML[i]&0x3F);
					ta=(byte)((TML[i]>>taS)&0x3F);
					ot=(byte)((TML[i]>>otS)&0xF);
					if(ot!=wP&&ot!=bP&&ot!=wK&&ot!=bK)
						DisambList[ta, ot, DisambListLength[ta, ot]++]=so;	// 登錄消歧義名單
					moveList[depth, j++]=TML[i]|((ulong)tag<<tgS);
				}
			}
			moveListLength[depth]=(byte)j;
#if DEBUG
			pseudoMoveCount[4]+=l;
			totalMoveCount[4]+=j;
#endif
			// 全部的合法棋步都出來之後，進行消歧義標籤計算
			for(i=0;i<j;i++) {
				ta=(byte)((moveList[depth, i]>>taS)&0x3F);
				ot=(byte)((moveList[depth, i]>>otS)&0xF);
				if((l=DisambListLength[ta, ot])<=1) continue;
				so=(byte)(moveList[depth, i]&0x3F); cx=0; cy=0;
				for(k=0;k<l;k++) {
					if((r=relDir[DisambList[ta, ot, k], so])==1) cx++;
					if(r==0) cy++;
				}
				moveList[depth, i]|=((ulong)(cx==0?b1:(cy==0?b2:b3))<<dbS);
			}
		}
	}
}