
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
			int i, j=0, l=0, pData;
			byte k, p, r;			// 一些變數，可能會有混著亂用的情況，請多包涵
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
				pData=slideHitLD[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]==oP) pinByOpp[p1]=true;
			}
			result=slideRayLU[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitLD[p, data]; p1=pData&0xFF; p2=pData>>8;
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

			// 大部分的資料生成完畢，接著生成殆合法棋步

			if(checkPieceCount==2) { moveListLength[depth]=0; return;} // 雙將軍的情況就不用繼續了，不可能走兵
			
			d=((ulong)oP<<otS)|((ulong)oP<<ntS)|len2;			
			for(p=0;p<64;p++) {
				if((k=position[p])==oP) {

					// 只搜尋小兵的直走棋步就好
					if(k==wP) {
						if(position[r=(byte)(p+8)]==b0&&(p>>3)!=6) {
							TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|len2;
							if((p>>3)==1&&position[r=(byte)(p+16)]==b0) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|d;
						}
					} else if(k==bP) {
						if(position[r=(byte)(p-8)]==b0&&(p>>3)!=1) {
							TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|len2;
							if((p>>3)==6&&position[r=(byte)(p-16)]==b0) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|d;
						}
					}
				}
			}

			// 殆合法棋步以及所有需要的資料都生成完畢，檢驗棋步的合法性、以及將軍對方與否
			for(i=0, j=0;i<l;i++) if(checkMove2(TML[i])==1) moveList[depth, j++]=TML[i]|0x10000000000;	// 只有當沒有將軍的時候才收錄
			moveListLength[depth]=(byte)j;
#if DEBUG
			pseudoMoveCount[2]+=l;
			totalMoveCount[2]+=j;
#endif
		}

		// 二碼專用的合法性檢查函數
		
		private byte checkMove2(ulong m) {
			byte so=(byte)(m&0x3F);
			byte ta=(byte)((m>>taS)&0x3F);
			if(pinByOpp[so]||checkPieceCount==1&&(canStopCheck&mask[ta])==0) return 0;
			if((canAttackOppKing[oP]&mask[ta])!=0||pinBySelf[so]) return 2;
			return 1;
		}


		/////////////////////////////////
		// 長度為 3 的棋步
		/////////////////////////////////

		// 兵的部分只要搜尋直走，入堡只要搜尋 O-O
		// 不用管一次閃兩子的橫向閃擊問題

		private void computeLegalMoves3() {
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
				pData=slideHitLD[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=true;
			}
			result=slideRayLU[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitLD[p, data]; p1=pData&0xFF; p2=pData>>8;
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

					// 入堡棋步，這邊只檢查入堡權、當下的將軍以及中間的格子是否空的，攻擊檢查待會再做
					if(k==wK&&checkPieceCount==0) {
						if((castlingState[depth]&cwK)!=0&&position[5]==b0&&position[6]==b0)
							TML[l++]=((ulong)4)|((ulong)6<<taS)|((ulong)6<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)OOMove<<miS)|len3;
					} else if(k==bK&&checkPieceCount==0) {
						if((castlingState[depth]&cbK)!=0&&position[61]==b0&&position[62]==b0)
							TML[l++]=((ulong)60)|((ulong)62<<taS)|((ulong)62<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)OOMove<<miS)|len3;
					}

					// 只搜尋小兵的直走棋步就好

					if(k==wP) {
						if(position[r=(byte)(p+8)]==b0&&(p>>3)!=6) {
							TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|len2;
							if((p>>3)==1&&position[r=(byte)(p+16)]==b0) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|len2;
						}
					} else if(k==bP) {
						if(position[r=(byte)(p-8)]==b0&&(p>>3)!=1) {
							TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|len2;
							if((p>>3)==6&&position[r=(byte)(p-16)]==b0) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|len2;
						}
					}


					// 普通棋步（含國王的），不用管吃子
					else {
						if(k==oN) {
							for(i=0;(r=pieceRuleN[p, i])!=NS;i++)
								if((d=position[r])==0) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|len3;
						} else if(k==oK) {
							for(i=0;(r=pieceRuleK[p, i])!=NS;i++)
								if((d=position[r])==0) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|len3;
						} else {
							if((k&b1)==b1)
								for(i=0;HVRule[p, i, 0]!=NS;i++) for(j=0;(r=HVRule[p, i, j])!=NS;j++) {
									if((d=position[r])==0) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|len3;
									else break;
								}
							if((k&b2)==b2)
								for(i=0;DIRule[p, i, 0]!=NS;i++) for(j=0;(r=DIRule[p, i, j])!=NS;j++) {
									if((d=position[r])==0) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|len3;
									else break;
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
				if(DisambListLength[ta, ot]<=1) continue;
				so=(byte)(moveList[depth, i]&0x3F); cx=0; cy=0;
				for(l=0;l<DisambListLength[ta, ot];l++) {
					if((DisambList[ta, ot, l]&7)==(so&7)) cx++;
					if((DisambList[ta, ot, l]>>3)==(so>>3)) cy++;
				}
				moveList[depth, i]|=((ulong)(cx==1?b1:(cy==1?b2:b3))<<dbS);
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
					if(pinByOpp[so]&&(ot==oN||relDir[so, ta]!=relDir[so, kingPos[whoseMove]])) return 0;
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
						(ot==oN||relDir[so, ta]!=relDir[so, kingPos[whoseMove]])) return 0;			// 如果是一般棋子，那必須阻止對方的將軍，但那個棋子不能被釘住
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
			int i, j=0, l=0, pData;
			byte k, p, d, r;			// 一些變數，可能會有混著亂用的情況，請多包涵
			byte tag, so, ta, ot;
			int data, p1, p2;
			ulong result, pPos;
			int cx, cy;

			// 設置代碼
			if(whoseMove==WT) { oP=wP; oR=wR; oN=wN; oB=wB; oQ=wQ; oK=wK; pP=bP; pR=bR; pN=bN; pB=bB; pQ=bQ; pK=bK; }//
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
				pData=slideHitLD[p, data]; p1=pData&0xFF; p2=pData>>8;
				if((position[p2]==pB||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=true;
			}
			result=slideRayLU[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitLD[p, data]; p1=pData&0xFF; p2=pData>>8;
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

					// 入堡棋步，這邊只檢查入堡權、當下的將軍以及中間的格子是否空的，攻擊檢查待會再做
					if(k==wK&&checkPieceCount==0) {
						if((castlingState[depth]&cwK)!=0&&position[5]==b0&&position[6]==b0)
							TML[l++]=((ulong)4)|((ulong)6<<taS)|((ulong)6<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)OOMove<<miS)|len3;
					} else if(k==bK&&checkPieceCount==0) {
						if((castlingState[depth]&cbK)!=0&&position[61]==b0&&position[62]==b0)
							TML[l++]=((ulong)60)|((ulong)62<<taS)|((ulong)62<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)OOMove<<miS)|len3;
					}

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


					// 普通棋步（含國王的）
					else {
						if(k==oN) {
							for(i=0;(r=pieceRuleN[p, i])!=NS;i++)
								if((d=position[r])==0) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|len3;
								else if(d>>3!=whoseMove) TML[l++]=((ulong)p)|((ulong)r<<taS)|((ulong)r<<deS)|((ulong)k<<otS)|((ulong)k<<ntS)|((ulong)d<<cpS)|len4;
						} else if(k==oK) {
							for(i=0;(r=pieceRuleK[p, i])!=NS;i++)
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
				if(DisambListLength[ta, ot]<=1) continue;
				so=(byte)(moveList[depth, i]&0x3F); cx=0; cy=0;
				for(l=0;l<DisambListLength[ta, ot];l++) {
					if((DisambList[ta, ot, l]&7)==(so&7)) cx++;
					if((DisambList[ta, ot, l]>>3)==(so>>3)) cy++;
				}
				moveList[depth, i]|=((ulong)(cx==1?b1:(cy==1?b2:b3))<<dbS);
			}
		}

	}
}