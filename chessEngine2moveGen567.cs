
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
			int i, j=0, l=0, pData;
			byte k, p, d, r;			// 一些變數，可能會有混著亂用的情況，請多包涵
			byte tag, so, ta, ot;
			int data, p1, p2;
			ulong result, pPos;
			int cx, cy;

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
			pData=slideHitR[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF;
			if((position[p2]==oR||position[p2]==oQ)&&position[p1]>>3==whoseMove) pinBySelf[p1]=true;
			result=slideRayL[p, data];
			canAttackOppKing[oR]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitL[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF;
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
				{ checkPieceCount++; canStopCheck|=result; }													// 由於合法性檢查已經排除了雙兵將跟雙騎士將
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
				pData=slideHitR[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF;
				if((position[p2]==pR||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=true;
			}
			result=slideRayL[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitL[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF;
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

			// 大部分的資料生成完畢，接著一邊生成殆合法棋步、一邊生成最後的 attackByOpp 資料

			for(p=0;p<64;p++) {
				k=position[p];
				if(k==b0) continue;
				if((k>>3)==whoseMove) {

					// 小兵棋步，只搜尋直走升變跟單純吃子兩種

					if(k==wP) {
						if(position[r=(byte)(p+8)]==b0&&(p>>3)==6) {
							TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)wN<<ntS)|len4;
							TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)wR<<ntS)|len4;
							TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)wB<<ntS)|len4;
							TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)wQ<<ntS)|len4;
						}
						if((r=pieceRuleWP[p, 0])!=NS) {
							if(side(d=position[r])==BC) {
								if((p>>3)!=6) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
							}
							if((r=pieceRuleWP[p, 1])!=NS&&side(d=position[r])==BC&&(p>>3)!=6)
								TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
						}
					} else if(k==bP) {
						if(position[r=(byte)(p-8)]==b0&&(p>>3)==1) {
							TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)bN<<ntS)|len4;
							TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)bR<<ntS)|len4;
							TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)bB<<ntS)|len4;
							TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)bQ<<ntS)|len4;
						}
						if((r=pieceRuleBP[p, 0])!=NS) {
							if(side(d=position[r])==WT) {
								if((p>>3)!=1) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
							}
							if((r=pieceRuleBP[p, 1])!=NS&&side(d=position[r])==WT&&(p>>3)!=1)
								TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
						}
					}


					// 普通棋步，如果沒有消歧義的話一定得吃子
					else if(pieceCount[k]==1) {
						if(k==oN) {
							for(i=0;(r=pieceRuleN[p, i])!=NS;i++)
								if((d=position[r])!=0&&d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
						} else if(k==oK) {
							for(i=0;(r=pieceRuleK[p, i])!=NS;i++)
								if((d=position[r])!=0&&d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
						} else {
							if((k&b1)==b1) {
								pData=slideHitR[p, data=(int)(occuH>>occuShiftH[p]&0x3F)]; r=(byte)(pData&0xFF); d=position[r];
								if(d!=0&&d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
								pData=slideHitL[p, data]; r=(byte)(pData&0xFF); d=position[r];
								if(d!=0&&d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
								pData=slideHitU[p, data=(int)(occuV>>occuShiftV[p]&0x3F)]; r=(byte)(pData&0xFF); d=position[r];
								if(d!=0&&d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
								pData=slideHitD[p, data]; r=(byte)(pData&0xFF); d=position[r];
								if(d!=0&&d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
							}
							if((k&b2)==b2) {
								pData=slideHitRU[p, data=(int)(occuFS>>occuShiftFS[p]&0x3F)]; r=(byte)(pData&0xFF); d=position[r];
								if(d!=0&&d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
								pData=slideHitLD[p, data]; r=(byte)(pData&0xFF); d=position[r];
								if(d!=0&&d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
								pData=slideHitRD[p, data=(int)(occuBS>>occuShiftBS[p]&0x3F)]; r=(byte)(pData&0xFF); d=position[r];
								if(d!=0&&d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
								pData=slideHitLU[p, data]; r=(byte)(pData&0xFF); d=position[r];
								if(d!=0&&d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
							}
						}
					}
					
					// 有消歧義可能的話就做一般的搜尋
					else {
						if(k==oN) {
							for(i=0;(r=pieceRuleN[p, i])!=NS;i++)
								if((d=position[r])==0) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|len3;
								else if(d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
						} else {
							if((k&b1)==b1)
								for(i=0;HVRule[p, i, 0]!=NS;i++) for(j=0;(r=HVRule[p, i, j])!=NS;j++) {
									if((d=position[r])==0) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|len3;
									else { if(d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4; break; }
								}
							if((k&b2)==b2)
								for(i=0;DIRule[p, i, 0]!=NS;i++) for(j=0;(r=DIRule[p, i, j])!=NS;j++) {
									if((d=position[r])==0) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|len3;
									else { if(d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4; break; }
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
			int i, j=0, l=0, pData;
			byte k, p, d, r;			// 一些變數，可能會有混著亂用的情況，請多包涵
			byte ep;					// 過路兵的位置
			byte tag, so, ta, ot;
			int data, p1, p2, p3;
			ulong result, pPos;
			int cx, cy;

			// 設置代碼
			if(whoseMove==WT) { oP=wP; oR=wR; oN=wN; oB=wB; oQ=wQ; oK=wK; pP=bP; pR=bR; pN=bN; pB=bB; pQ=bQ; pK=bK; }
			else { oP=bP; oR=bR; oN=bN; oB=bB; oQ=bQ; oK=bK; pP=wP; pR=wR; pN=wN; pB=wB; pQ=wQ; pK=wK; }
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
				{ checkPieceCount++; canStopCheck|=result; }													// 由於合法性檢查已經排除了雙兵將跟雙騎士將
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

			// 大部分的資料生成完畢，接著一邊生成殆合法棋步、一邊生成最後的 attackByOpp 資料

			for(p=0;p<64;p++) {
				k=position[p];
				if(k==b0) continue;
				if((k>>3)==whoseMove) {

					// 小兵棋步，只搜尋吃子升變跟吃過路兵

					if(k==wP) {
						if((r=pieceRuleWP[p, 0])!=NS) {
							if(side(d=position[r])==BC) {
								if((p>>3)==6) {
									TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)wN<<ntS)|((ulong)d<<cpS)|len6;
									TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)wR<<ntS)|((ulong)d<<cpS)|len6;
									TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)wB<<ntS)|((ulong)d<<cpS)|len6;
									TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)wQ<<ntS)|((ulong)d<<cpS)|len6;
								}
							} else if(r==enPassantState[depth])
								TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)ep<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)bP<<cpS)|((ulong)epMove<<miS)|len6;
							if((r=pieceRuleWP[p, 1])!=NS) {
								if(side(d=position[r])==BC) {
									if((p>>3)==6) {
										TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)wN<<ntS)|((ulong)d<<cpS)|len6;
										TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)wR<<ntS)|((ulong)d<<cpS)|len6;
										TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)wB<<ntS)|((ulong)d<<cpS)|len6;
										TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)wQ<<ntS)|((ulong)d<<cpS)|len6;
									}
								} else if(r==enPassantState[depth])
									TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)ep<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)bP<<cpS)|((ulong)epMove<<miS)|len6;
							}
						}
					} else if(k==bP) {
						if((r=pieceRuleBP[p, 0])!=NS) {
							if(side(d=position[r])==WT) {
								if((p>>3)==1) {
									TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)bN<<ntS)|((ulong)d<<cpS)|len6;
									TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)bR<<ntS)|((ulong)d<<cpS)|len6;
									TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)bB<<ntS)|((ulong)d<<cpS)|len6;
									TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)bQ<<ntS)|((ulong)d<<cpS)|len6;
								}
							} else if(r==enPassantState[depth])
								TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)ep<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)wP<<cpS)|((ulong)epMove<<miS)|len6;
							if((r=pieceRuleBP[p, 1])!=NS) {
								if(side(d=position[r])==WT) {
									if((p>>3)==1) {
										TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)bN<<ntS)|((ulong)d<<cpS)|len6;
										TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)bR<<ntS)|((ulong)d<<cpS)|len6;
										TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)bB<<ntS)|((ulong)d<<cpS)|len6;
										TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)bQ<<ntS)|((ulong)d<<cpS)|len6;
									}
								} else if(r==enPassantState[depth])
									TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)ep<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)wP<<cpS)|((ulong)epMove<<miS)|len6;
							}
						}
					}


					// 普通棋步的話一定要消歧義
					else if(pieceCount[k]>2) {
						if(k==oN) {
							for(i=0;(r=pieceRuleN[p, i])!=NS;i++)
								if((d=position[r])==0) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|len3;
								else if(d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
						} else {
							if((k&b1)==b1)
								for(i=0;HVRule[p, i, 0]!=NS;i++) for(j=0;(r=HVRule[p, i, j])!=NS;j++) {
									if((d=position[r])==0) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|len3;
									else { if(d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4; break; }
								}
							if((k&b2)==b2)
								for(i=0;DIRule[p, i, 0]!=NS;i++) for(j=0;(r=DIRule[p, i, j])!=NS;j++) {
									if((d=position[r])==0) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|len3;
									else { if(d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4; break; }
								}
						}
					}

					// 如果只有一碼消歧義的話必須吃子
					else if(pieceCount[k]==2) {
						if(k==oN) {
							for(i=0;(r=pieceRuleN[p, i])!=NS;i++)
								if((d=position[r])!=0&&d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
						} else {
							if((k&b1)==b1) {
								pData=slideHitR[p, data=(int)(occuH>>occuShiftH[p]&0x3F)]; r=(byte)(pData&0xFF); d=position[r];
								if(d!=0&&d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
								pData=slideHitL[p, data]; r=(byte)(pData&0xFF); d=position[r];
								if(d!=0&&d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
								pData=slideHitU[p, data=(int)(occuV>>occuShiftV[p]&0x3F)]; r=(byte)(pData&0xFF); d=position[r];
								if(d!=0&&d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
								pData=slideHitD[p, data]; r=(byte)(pData&0xFF); d=position[r];
								if(d!=0&&d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
							}
							if((k&b2)==b2) {
								pData=slideHitRU[p, data=(int)(occuFS>>occuShiftFS[p]&0x3F)]; r=(byte)(pData&0xFF); d=position[r];
								if(d!=0&&d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
								pData=slideHitLD[p, data]; r=(byte)(pData&0xFF); d=position[r];
								if(d!=0&&d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
								pData=slideHitRD[p, data=(int)(occuBS>>occuShiftBS[p]&0x3F)]; r=(byte)(pData&0xFF); d=position[r];
								if(d!=0&&d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
								pData=slideHitLU[p, data]; r=(byte)(pData&0xFF); d=position[r];
								if(d!=0&&d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
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
			int i, j=0, l=0, pData;
			byte k, p, d, r;			// 一些變數，可能會有混著亂用的情況，請多包涵
			byte ep;					// 過路兵的位置
			byte tag, so, ta, ot;
			int data, p1, p2, p3;
			ulong result, pPos;
			int cx, cy;

			// 設置代碼
			if(whoseMove==WT) { oP=wP; oR=wR; oN=wN; oB=wB; oQ=wQ; oK=wK; pP=bP; pR=bR; pN=bN; pB=bB; pQ=bQ; pK=bK; }
			else { oP=bP; oR=bR; oN=bN; oB=bB; oQ=bQ; oK=bK; pP=wP; pR=wR; pN=wN; pB=wB; pQ=wQ; pK=wK; }
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
				{ checkPieceCount++; canStopCheck|=result; }													// 由於合法性檢查已經排除了雙兵將跟雙騎士將
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

			// 大部分的資料生成完畢，接著生成殆合法棋步

			for(p=0;p<64;p++) {
				k=position[p];
				if(k==b0) continue;
				if((k>>3)==whoseMove) {

					// 小兵棋步，只搜尋吃子升變跟吃過路兵

					if(k==wP) {
						if((r=pieceRuleWP[p, 0])!=NS) {
							if(side(d=position[r])==BC) {
								if((p>>3)==6) {
									TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)wN<<ntS)|((ulong)d<<cpS)|len6;
									TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)wR<<ntS)|((ulong)d<<cpS)|len6;
									TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)wB<<ntS)|((ulong)d<<cpS)|len6;
									TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)wQ<<ntS)|((ulong)d<<cpS)|len6;
								}
							} else if(r==enPassantState[depth])
								TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)ep<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)bP<<cpS)|((ulong)epMove<<miS)|len6;
							if((r=pieceRuleWP[p, 1])!=NS) {
								if(side(d=position[r])==BC) {
									if((p>>3)==6) {
										TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)wN<<ntS)|((ulong)d<<cpS)|len6;
										TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)wR<<ntS)|((ulong)d<<cpS)|len6;
										TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)wB<<ntS)|((ulong)d<<cpS)|len6;
										TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)wQ<<ntS)|((ulong)d<<cpS)|len6;
									}
								} else if(r==enPassantState[depth])
									TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)ep<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)bP<<cpS)|((ulong)epMove<<miS)|len6;
							}
						}
					} else if(k==bP) {
						if((r=pieceRuleBP[p, 0])!=NS) {
							if(side(d=position[r])==WT) {
								if((p>>3)==1) {
									TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)bN<<ntS)|((ulong)d<<cpS)|len6;
									TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)bR<<ntS)|((ulong)d<<cpS)|len6;
									TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)bB<<ntS)|((ulong)d<<cpS)|len6;
									TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)bQ<<ntS)|((ulong)d<<cpS)|len6;
								}
							} else if(r==enPassantState[depth])
								TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)ep<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)wP<<cpS)|((ulong)epMove<<miS)|len6;
							if((r=pieceRuleBP[p, 1])!=NS) {
								if(side(d=position[r])==WT) {
									if((p>>3)==1) {
										TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)bN<<ntS)|((ulong)d<<cpS)|len6;
										TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)bR<<ntS)|((ulong)d<<cpS)|len6;
										TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)bB<<ntS)|((ulong)d<<cpS)|len6;
										TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)bQ<<ntS)|((ulong)d<<cpS)|len6;
									}
								} else if(r==enPassantState[depth])
									TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)ep<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)wP<<cpS)|((ulong)epMove<<miS)|len6;
							}
						}
					}

					// 普通棋步的話必須要兩碼消歧義外加吃子才行
					else if(pieceCount[k]>2) {					
						if(k==oN) {
							for(i=0;(r=pieceRuleN[p, i])!=NS;i++)
								if((d=position[r])!=0&&d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
						} else {
							if((k&b1)==b1) {
								pData=slideHitR[p, data=(int)(occuH>>occuShiftH[p]&0x3F)]; r=(byte)(pData&0xFF); d=position[r];
								if(d!=0&&d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
								pData=slideHitL[p, data]; r=(byte)(pData&0xFF); d=position[r];
								if(d!=0&&d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
								pData=slideHitU[p, data=(int)(occuV>>occuShiftV[p]&0x3F)]; r=(byte)(pData&0xFF); d=position[r];
								if(d!=0&&d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
								pData=slideHitD[p, data]; r=(byte)(pData&0xFF); d=position[r];
								if(d!=0&&d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
							}
							if((k&b2)==b2) {
								pData=slideHitRU[p, data=(int)(occuFS>>occuShiftFS[p]&0x3F)]; r=(byte)(pData&0xFF); d=position[r];
								if(d!=0&&d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
								pData=slideHitLD[p, data]; r=(byte)(pData&0xFF); d=position[r];
								if(d!=0&&d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
								pData=slideHitRD[p, data=(int)(occuBS>>occuShiftBS[p]&0x3F)]; r=(byte)(pData&0xFF); d=position[r];
								if(d!=0&&d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
								pData=slideHitLU[p, data]; r=(byte)(pData&0xFF); d=position[r];
								if(d!=0&&d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
							}
						}
					}
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