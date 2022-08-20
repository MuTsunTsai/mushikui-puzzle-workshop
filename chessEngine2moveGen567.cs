
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;

namespace Mushikui_Puzzle_Workshop {
	partial class chessEngine2 {

		/////////////////////////////////
		// 長度為 5 的棋步
		/////////////////////////////////

		// 跟長度 4 的處理幾乎一樣，除了入堡只搜尋 O-O-O 之外

		private void computeLegalMoves5() {
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

			// 底下的棋步當中，沒有消歧義可能的話就一定要吃子

			// 騎士棋步
			if(pieceCount[oN]==1) {
				for(i=0;i<pieceCount[oN];i++)
					if(!pinByOpp[so=pieceList[oN, i]])	// 騎士只要被釘住就不能動
						for(j=0;(ta=pieceRuleN[so, j])!=NS;j++)
							if((cp=position[ta])!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oN<<otS)|((ulong)oN<<ntS)|((ulong)cp<<cpS)|len4;
			} else {
				for(i=0;i<pieceCount[oN];i++)
					if(!pinByOpp[so=pieceList[oN, i]])	// 騎士只要被釘住就不能動
						for(j=0;(ta=pieceRuleN[so, j])!=NS;j++)
							if((cp=position[ta])==0) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oN<<otS)|((ulong)oN<<ntS)|len3;
							else if(cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oN<<otS)|((ulong)oN<<ntS)|((ulong)cp<<cpS)|len4;
			}

			// 國王棋步（普通的）
			for(j=0;(ta=pieceRuleK[so=kingPos[whoseMove], j])!=NS;j++)
				if((cp=position[ta])!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oK<<otS)|((ulong)oK<<ntS)|((ulong)cp<<cpS)|len4;

			// 城堡棋步
			if(pieceCount[oR]==1)
				for(i=0;i<pieceCount[oR];i++) {
					pData=slideHitR[so=pieceList[oR, i], data=(int)(occuH>>occuShiftH[so]&0x3F)]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oR<<otS)|((ulong)oR<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitL[so, data]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oR<<otS)|((ulong)oR<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitU[so, data=(int)(occuV>>occuShiftV[so]&0x3F)]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oR<<otS)|((ulong)oR<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitD[so, data]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oR<<otS)|((ulong)oR<<ntS)|((ulong)cp<<cpS)|len4;
				}
			else
				for(i=0;i<pieceCount[oR];i++)
					for(j=0;HVRule[so=pieceList[oR, i], j, 0]!=NS;j++)
						for(k=0;(ta=HVRule[so, j, k])!=NS;k++) {
							if((cp=position[ta])==0) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oR<<otS)|((ulong)oR<<ntS)|len3;
							else { if(cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oR<<otS)|((ulong)oR<<ntS)|((ulong)cp<<cpS)|len4; break; }
						}

			// 主教棋步
			if(pieceCount[oB]==1)
				for(i=0;i<pieceCount[oB];i++) {
					pData=slideHitRU[so=pieceList[oB, i], data=(int)(occuFS>>occuShiftFS[so]&0x3F)]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oB<<otS)|((ulong)oB<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitLD[so, data]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oB<<otS)|((ulong)oB<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitRD[so, data=(int)(occuBS>>occuShiftBS[so]&0x3F)]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oB<<otS)|((ulong)oB<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitLU[so, data]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oB<<otS)|((ulong)oB<<ntS)|((ulong)cp<<cpS)|len4;
				}
			else
				for(i=0;i<pieceCount[oB];i++)
					for(j=0;DIRule[so=pieceList[oB, i], j, 0]!=NS;j++)
						for(k=0;(ta=DIRule[so, j, k])!=NS;k++) {
							if((cp=position[ta])==0) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oB<<otS)|((ulong)oB<<ntS)|len3;
							else { if(cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oB<<otS)|((ulong)oB<<ntS)|((ulong)cp<<cpS)|len4; break; }
						}

			// 皇后棋步
			if(pieceCount[oQ]==1)
				for(i=0;i<pieceCount[oQ];i++) {
					pData=slideHitR[so=pieceList[oQ, i], data=(int)(occuH>>occuShiftH[so]&0x3F)]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitL[so, data]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitU[so, data=(int)(occuV>>occuShiftV[so]&0x3F)]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitD[so, data]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitRU[so, data=(int)(occuFS>>occuShiftFS[so]&0x3F)]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitLD[so, data]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitRD[so, data=(int)(occuBS>>occuShiftBS[so]&0x3F)]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitLU[so, data]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4;
				}
			else
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
					if((castlingState[depth]&cwQ)!=0&&position[1]==b0&&position[2]==b0&&position[3]==b0) TML[l++]=wOOO;
				} else {
					if((castlingState[depth]&cbQ)!=0&&position[57]==b0&&position[58]==b0&&position[59]==b0) TML[l++]=bOOO;
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
			pseudoMoveCount[5]+=l;
			totalMoveCount[5]+=j;
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

		/////////////////////////////////
		// 長度為 6 的棋步
		/////////////////////////////////

		// 兵的動作只搜尋吃過路兵和吃子升變兩種
		// 入堡只搜尋 O-O-O
		// 不用搜尋國王的通常移動

		private void computeLegalMoves6() {
			int i, j, k, l=0, pData;
			byte p, r;
			byte ep;				// 過路兵的位置
			byte tag, so, ta, ot, cp;
			int data, p1, p2, p3;
			ulong result, pPos;
			int cx, cy;

			// 設置代碼
			if(whoseMove==WT) { oP=wP; oR=wR; oN=wN; oB=wB; oQ=wQ; oK=wK; pP=bP; pR=bR; pN=bN; pB=bB; pQ=bQ; pK=bK;}
			else { oP=bP; oR=bR; oN=bN; oB=bB; oQ=bQ; oK=bK; pP=wP; pR=wR; pN=wN; pB=wB; pQ=wQ; pK=wK;}
			ep=(byte)(enPassantState[depth]==NS?NS:enPassantState[depth]+(whoseMove==WT?-8:8));

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
				else if(p3!=NS&&ep!=NS&&(position[p3]==pR||position[p3]==pQ)&&(position[p1]==oP&&p2==ep||position[p2]==oP&&p1==ep)) dblPin=true;
			}
			result=slideRayL[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitL[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF; p3=pData>>16;
				if((position[p2]==pR||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=true;
				else if(p3!=NS&&ep!=NS&&(position[p3]==pR||position[p3]==pQ)&&(position[p1]==oP&&p2==ep||position[p2]==oP&&p1==ep)) dblPin=true;
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
					if((ta=pieceRuleWP[so, 0])!=NS) {
						if(side(cp=position[ta])==BC) {
							if((so>>3)==6) {
								TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wN<<ntS)|((ulong)cp<<cpS)|len6;
								TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wR<<ntS)|((ulong)cp<<cpS)|len6;
								TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wB<<ntS)|((ulong)cp<<cpS)|len6;
								TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wQ<<ntS)|((ulong)cp<<cpS)|len6;
							}
						} else if(ta==enPassantState[depth])
							TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ep<<deS)|((ulong)wP<<otS)|((ulong)wP<<ntS)|((ulong)bP<<cpS)|((ulong)epMove<<miS)|len6;
						if((ta=pieceRuleWP[so, 1])!=NS) {
							if(side(cp=position[ta])==BC) {
								if((so>>3)==6) {
									TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wN<<ntS)|((ulong)cp<<cpS)|len6;
									TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wR<<ntS)|((ulong)cp<<cpS)|len6;
									TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wB<<ntS)|((ulong)cp<<cpS)|len6;
									TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wQ<<ntS)|((ulong)cp<<cpS)|len6;
								}
							} else if(ta==enPassantState[depth])
								TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ep<<deS)|((ulong)wP<<otS)|((ulong)wP<<ntS)|((ulong)bP<<cpS)|((ulong)epMove<<miS)|len6;
						}
					}
				}
			} else {
				for(i=0;i<pieceCount[bP];i++) {
					so=pieceList[bP, i];
					if((ta=pieceRuleBP[so, 0])!=NS) {
						if(side(cp=position[ta])==WT) {
							if((so>>3)==1) {
								TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bN<<ntS)|((ulong)cp<<cpS)|len6;
								TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bR<<ntS)|((ulong)cp<<cpS)|len6;
								TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bB<<ntS)|((ulong)cp<<cpS)|len6;
								TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bQ<<ntS)|((ulong)cp<<cpS)|len6;
							}
						} else if(ta==enPassantState[depth])
							TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ep<<deS)|((ulong)bP<<otS)|((ulong)bP<<ntS)|((ulong)wP<<cpS)|((ulong)epMove<<miS)|len6;
						if((ta=pieceRuleBP[so, 1])!=NS) {
							if(side(cp=position[ta])==WT) {
								if((so>>3)==1) {
									TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bN<<ntS)|((ulong)cp<<cpS)|len6;
									TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bR<<ntS)|((ulong)cp<<cpS)|len6;
									TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bB<<ntS)|((ulong)cp<<cpS)|len6;
									TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bQ<<ntS)|((ulong)cp<<cpS)|len6;
								}
							} else if(ta==enPassantState[depth])
								TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ep<<deS)|((ulong)bP<<otS)|((ulong)bP<<ntS)|((ulong)wP<<cpS)|((ulong)epMove<<miS)|len6;
						}
					}
				}
			}

			// 底下的棋步當中，一定要消歧義，而且如果是一碼消歧義的話一定要吃子

			// 騎士棋步
			if(pieceCount[oN]==2) {
				for(i=0;i<pieceCount[oN];i++)
					if(!pinByOpp[so=pieceList[oN, i]])	// 騎士只要被釘住就不能動
						for(j=0;(ta=pieceRuleN[so, j])!=NS;j++)
							if((cp=position[ta])!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oN<<otS)|((ulong)oN<<ntS)|((ulong)cp<<cpS)|len4;
			} else if(pieceCount[oN]>2) {
				for(i=0;i<pieceCount[oN];i++)
					if(!pinByOpp[so=pieceList[oN, i]])	// 騎士只要被釘住就不能動
						for(j=0;(ta=pieceRuleN[so, j])!=NS;j++)
							if((cp=position[ta])==0) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oN<<otS)|((ulong)oN<<ntS)|len3;
							else if(cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oN<<otS)|((ulong)oN<<ntS)|((ulong)cp<<cpS)|len4;
			}

			// 城堡棋步
			if(pieceCount[oR]==2)
				for(i=0;i<pieceCount[oR];i++) {
					pData=slideHitR[so=pieceList[oR, i], data=(int)(occuH>>occuShiftH[so]&0x3F)]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oR<<otS)|((ulong)oR<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitL[so, data]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oR<<otS)|((ulong)oR<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitU[so, data=(int)(occuV>>occuShiftV[so]&0x3F)]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oR<<otS)|((ulong)oR<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitD[so, data]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oR<<otS)|((ulong)oR<<ntS)|((ulong)cp<<cpS)|len4;
				}
			else if(pieceCount[oR]>2)
				for(i=0;i<pieceCount[oR];i++)
					for(j=0;HVRule[so=pieceList[oR, i], j, 0]!=NS;j++)
						for(k=0;(ta=HVRule[so, j, k])!=NS;k++) {
							if((cp=position[ta])==0) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oR<<otS)|((ulong)oR<<ntS)|len3;
							else { if(cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oR<<otS)|((ulong)oR<<ntS)|((ulong)cp<<cpS)|len4; break; }
						}

			// 主教棋步
			if(pieceCount[oB]==2)
				for(i=0;i<pieceCount[oB];i++) {
					pData=slideHitRU[so=pieceList[oB, i], data=(int)(occuFS>>occuShiftFS[so]&0x3F)]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oB<<otS)|((ulong)oB<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitLD[so, data]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oB<<otS)|((ulong)oB<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitRD[so, data=(int)(occuBS>>occuShiftBS[so]&0x3F)]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oB<<otS)|((ulong)oB<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitLU[so, data]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oB<<otS)|((ulong)oB<<ntS)|((ulong)cp<<cpS)|len4;
				}
			else if(pieceCount[oB]>2)
				for(i=0;i<pieceCount[oB];i++)
					for(j=0;DIRule[so=pieceList[oB, i], j, 0]!=NS;j++)
						for(k=0;(ta=DIRule[so, j, k])!=NS;k++) {
							if((cp=position[ta])==0) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oB<<otS)|((ulong)oB<<ntS)|len3;
							else { if(cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oB<<otS)|((ulong)oB<<ntS)|((ulong)cp<<cpS)|len4; break; }
						}

			// 皇后棋步
			if(pieceCount[oQ]==2)
				for(i=0;i<pieceCount[oQ];i++) {
					pData=slideHitR[so=pieceList[oQ, i], data=(int)(occuH>>occuShiftH[so]&0x3F)]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitL[so, data]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitU[so, data=(int)(occuV>>occuShiftV[so]&0x3F)]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitD[so, data]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitRU[so, data=(int)(occuFS>>occuShiftFS[so]&0x3F)]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitLD[so, data]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitRD[so, data=(int)(occuBS>>occuShiftBS[so]&0x3F)]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitLU[so, data]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4;
				}
			else if(pieceCount[oQ]>2)
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
					if((castlingState[depth]&cwQ)!=0&&position[1]==b0&&position[2]==b0&&position[3]==b0) TML[l++]=wOOO;
				} else {
					if((castlingState[depth]&cbQ)!=0&&position[57]==b0&&position[58]==b0&&position[59]==b0) TML[l++]=bOOO;
				}
			}

			// 殆合法棋步以及所有需要的資料都生成完畢，檢驗棋步的合法性、以及將軍對方與否
			Array.Clear(DisambListLength, 0, 1024);
			for(i=0, j=0;i<l;i++) {
				tag=checkMove(TML[i]);
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
			pseudoMoveCount[6]+=l;
			totalMoveCount[6]+=j;
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

		/////////////////////////////////
		// 長度為 7 的棋步
		/////////////////////////////////

		// 跟 6 很類似，除了只要搜尋吃子的棋步即可，而且不搜尋入堡，所以完全沒有國王的動作，因此不用建立 attackByOpp 資料

		private void computeLegalMoves7() {
			int i, j, k, l=0, pData;
			byte p, r;
			byte ep;				// 過路兵的位置
			byte tag, so, ta, ot, cp;
			int data, p1, p2, p3;
			ulong result, pPos;
			int cx, cy;

			// 設置代碼
			if(whoseMove==WT) { oP=wP; oR=wR; oN=wN; oB=wB; oQ=wQ; oK=wK; pP=bP; pR=bR; pN=bN; pB=bB; pQ=bQ; pK=bK;}
			else { oP=bP; oR=bR; oN=bN; oB=bB; oQ=bQ; oK=bK; pP=wP; pR=wR; pN=wN; pB=wB; pQ=wQ; pK=wK;}
			ep=(byte)(enPassantState[depth]==NS?NS:enPassantState[depth]+(whoseMove==WT?-8:8));

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
				else if(p3!=NS&&ep!=NS&&(position[p3]==pR||position[p3]==pQ)&&(position[p1]==oP&&p2==ep||position[p2]==oP&&p1==ep)) dblPin=true;
			}
			result=slideRayL[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitL[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF; p3=pData>>16;
				if((position[p2]==pR||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=true;
				else if(p3!=NS&&ep!=NS&&(position[p3]==pR||position[p3]==pQ)&&(position[p1]==oP&&p2==ep||position[p2]==oP&&p1==ep)) dblPin=true;
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

			// 資料生成完畢，生成殆合法棋步

			// 小兵
			if(whoseMove==WT) {
				for(i=0;i<pieceCount[wP];i++) {
					so=pieceList[wP, i];
					if((ta=pieceRuleWP[so, 0])!=NS) {
						if(side(cp=position[ta])==BC) {
							if((so>>3)==6) {
								TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wN<<ntS)|((ulong)cp<<cpS)|len6;
								TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wR<<ntS)|((ulong)cp<<cpS)|len6;
								TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wB<<ntS)|((ulong)cp<<cpS)|len6;
								TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wQ<<ntS)|((ulong)cp<<cpS)|len6;
							}
						} else if(ta==enPassantState[depth])
							TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ep<<deS)|((ulong)wP<<otS)|((ulong)wP<<ntS)|((ulong)bP<<cpS)|((ulong)epMove<<miS)|len6;
						if((ta=pieceRuleWP[so, 1])!=NS) {
							if(side(cp=position[ta])==BC) {
								if((so>>3)==6) {
									TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wN<<ntS)|((ulong)cp<<cpS)|len6;
									TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wR<<ntS)|((ulong)cp<<cpS)|len6;
									TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wB<<ntS)|((ulong)cp<<cpS)|len6;
									TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wQ<<ntS)|((ulong)cp<<cpS)|len6;
								}
							} else if(ta==enPassantState[depth])
								TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ep<<deS)|((ulong)wP<<otS)|((ulong)wP<<ntS)|((ulong)bP<<cpS)|((ulong)epMove<<miS)|len6;
						}
					}
				}
			} else {
				for(i=0;i<pieceCount[bP];i++) {
					so=pieceList[bP, i];
					if((ta=pieceRuleBP[so, 0])!=NS) {
						if(side(cp=position[ta])==WT) {
							if((so>>3)==1) {
								TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bN<<ntS)|((ulong)cp<<cpS)|len6;
								TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bR<<ntS)|((ulong)cp<<cpS)|len6;
								TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bB<<ntS)|((ulong)cp<<cpS)|len6;
								TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bQ<<ntS)|((ulong)cp<<cpS)|len6;
							}
						} else if(ta==enPassantState[depth])
							TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ep<<deS)|((ulong)bP<<otS)|((ulong)bP<<ntS)|((ulong)wP<<cpS)|((ulong)epMove<<miS)|len6;
						if((ta=pieceRuleBP[so, 1])!=NS) {
							if(side(cp=position[ta])==WT) {
								if((so>>3)==1) {
									TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bN<<ntS)|((ulong)cp<<cpS)|len6;
									TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bR<<ntS)|((ulong)cp<<cpS)|len6;
									TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bB<<ntS)|((ulong)cp<<cpS)|len6;
									TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bQ<<ntS)|((ulong)cp<<cpS)|len6;
								}
							} else if(ta==enPassantState[depth])
								TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ep<<deS)|((ulong)bP<<otS)|((ulong)bP<<ntS)|((ulong)wP<<cpS)|((ulong)epMove<<miS)|len6;
						}
					}
				}
			}

			// 騎士棋步
			if(pieceCount[oN]>2)
				for(i=0;i<pieceCount[oN];i++)
					if(!pinByOpp[so=pieceList[oN, i]])	// 騎士只要被釘住就不能動
						for(j=0;(ta=pieceRuleN[so, j])!=NS;j++)
							if((cp=position[ta])!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oN<<otS)|((ulong)oN<<ntS)|((ulong)cp<<cpS)|len4;

			// 城堡棋步
			if(pieceCount[oR]>2)
				for(i=0;i<pieceCount[oR];i++) {
					pData=slideHitR[so=pieceList[oR, i], data=(int)(occuH>>occuShiftH[so]&0x3F)]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oR<<otS)|((ulong)oR<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitL[so, data]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oR<<otS)|((ulong)oR<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitU[so, data=(int)(occuV>>occuShiftV[so]&0x3F)]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oR<<otS)|((ulong)oR<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitD[so, data]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oR<<otS)|((ulong)oR<<ntS)|((ulong)cp<<cpS)|len4;
				}

			// 主教棋步
			if(pieceCount[oB]>2)
				for(i=0;i<pieceCount[oB];i++) {
					pData=slideHitRU[so=pieceList[oB, i], data=(int)(occuFS>>occuShiftFS[so]&0x3F)]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oB<<otS)|((ulong)oB<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitLD[so, data]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oB<<otS)|((ulong)oB<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitRD[so, data=(int)(occuBS>>occuShiftBS[so]&0x3F)]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oB<<otS)|((ulong)oB<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitLU[so, data]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oB<<otS)|((ulong)oB<<ntS)|((ulong)cp<<cpS)|len4;
				}
				

			// 皇后棋步
			if(pieceCount[oQ]>2)
				for(i=0;i<pieceCount[oQ];i++) {
					pData=slideHitR[so=pieceList[oQ, i], data=(int)(occuH>>occuShiftH[so]&0x3F)]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitL[so, data]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitU[so, data=(int)(occuV>>occuShiftV[so]&0x3F)]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitD[so, data]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitRU[so, data=(int)(occuFS>>occuShiftFS[so]&0x3F)]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitLD[so, data]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitRD[so, data=(int)(occuBS>>occuShiftBS[so]&0x3F)]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4;
					pData=slideHitLU[so, data]; ta=(byte)(pData&0xFF); cp=position[ta];
					if(cp!=0&&cp>>3!=whoseMove) TML[l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4;
				}

			// 殆合法棋步以及所有需要的資料都生成完畢，檢驗棋步的合法性、以及將軍對方與否
			Array.Clear(DisambListLength, 0, 1024);
			for(i=0, j=0;i<l;i++) {
				tag=checkMove(TML[i]);
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
			pseudoMoveCount[7]+=l;
			totalMoveCount[7]+=j;
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