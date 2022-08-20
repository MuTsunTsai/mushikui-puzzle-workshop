
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.VisualBasic;

namespace Mushikui_Puzzle_Workshop {
	partial class chessEngine2 {

		/////////////////////////////////
		// 合法棋步計算用資料
		/////////////////////////////////

		private ulong		attackByOpp;						// 一個格子是否正被對方攻擊（保護）著
		private ulong		canStopCheck;						// 假如把一個棋子移動到這邊，就可以解除將軍（擋住長程子力、或者吃掉將軍子）
		private ulong[]		canAttackOppKing=new ulong[16];		// 如果將一個特定種類的棋子移動到該格子上，是否可以攻擊到對方的國王
		private int[]		pinByOpp=new int[64];				// 一個格上（上的棋子）被對方釘住的方向，0=沒有 1=橫 2=縱 4=正斜 8=反斜
		private int[]		pinBySelf=new int[64];				// 一個格子（上的棋子）被我方釘著的方向（亦即移開該棋子可造成閃擊）

		private int			checkPieceCount;
		private bool		dblDis;								// 特殊的吃過路兵一次閃兩子閃擊
		private bool		dblPin;								// 特殊的吃過路兵一次閃兩子釘

		private byte[,,]	DisambList=new byte[64, 16, 16];
		private int[,]		DisambListLength=new int[64, 16];
		
		private ulong[]		moveRange=new ulong[64];			// 暫存棋子的移動範圍

		private byte oP, oR, oN, oB, oQ, oK;					// 我方棋子的代碼
		private byte pP, pR, pN, pB, pQ, pK;					// 敵方棋子的代碼

		/////////////////////////////////
		// 合法棋步計算函數（全部生成）
		/////////////////////////////////
				
		private void computeLegalMoves() {
			int i, j, k, l=0, pData;
			byte p, r;				
			byte ep;				// 過路兵的位置
			byte tag, so, ta, ot, nt, de, cp, mi;
			int data, p1, p2, p3;
			ulong result, pPos, m, n;
			//ulong NOToccuSelf;
			int cx, cy;

			// 設置代碼
			if(whoseMove==WT) {
				oP=wP; oR=wR; oN=wN; oB=wB; oQ=wQ; oK=wK; pP=bP; pR=bR; pN=bN; pB=bB; pQ=bQ; pK=bK;
			} else {
				oP=bP; oR=bR; oN=bN; oB=bB; oQ=bQ; oK=bK; pP=wP; pR=wR; pN=wN; pB=wB; pQ=wQ; pK=wK;
			}
			ep=(byte)(enPassantState[depth]==NS?NS:enPassantState[depth]+(whoseMove==WT?-8:8));
			//NOToccuSelf=~(piecePos[oP]|piecePos[oR]|piecePos[oN]|piecePos[oB]|piecePos[oQ]|piecePos[oK]);

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
			if((position[p2]==oB||position[p2]==oQ)&&(position[p1]>>3==whoseMove||p1==ep)) pinBySelf[p1]=4;
			result=slideRayLD[p, data];
			canAttackOppKing[oB]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitLD[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&(position[p1]>>3==whoseMove||p1==ep)) pinBySelf[p1]=4;
			result=slideRayRD[p, data=(int)(occuB>>occuShiftB[p]&0x3F)];
			canAttackOppKing[oB]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitRD[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&(position[p1]>>3==whoseMove||p1==ep)) pinBySelf[p1]=8;
			result=slideRayLU[p, data];
			canAttackOppKing[oB]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitLU[p, data]; p1=pData&0xFF; p2=pData>>8;
			if((position[p2]==oB||position[p2]==oQ)&&(position[p1]>>3==whoseMove||p1==ep)) pinBySelf[p1]=8;

			// 右左
			result=slideRayR[p, data=(int)(occuH>>occuShiftH[p]&0x3F)];
			canAttackOppKing[oR]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitR[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF; p3=pData>>16;
			if((position[p2]==oR||position[p2]==oQ)&&position[p1]>>3==whoseMove) pinBySelf[p1]=1;
			else if(p3!=NS&&ep!=NS&&(position[p3]==oR||position[p3]==oQ)&&(position[p1]==oP&&p2==ep||position[p2]==oP&&p1==ep)) dblDis=true;
			result=slideRayL[p, data];
			canAttackOppKing[oR]|=result; canAttackOppKing[oQ]|=result;
			pData=slideHitL[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF; p3=pData>>16;
			if((position[p2]==oR||position[p2]==oQ)&&position[p1]>>3==whoseMove) pinBySelf[p1]=1;
			else if(p3!=NS&&ep!=NS&&(position[p3]==oR||position[p3]==oQ)&&(position[p1]==oP&&p2==ep||position[p2]==oP&&p1==ep)) dblDis=true;

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
				else if(p3!=NS&&ep!=NS&&(position[p3]==pR||position[p3]==pQ)&&(position[p1]==oP&&p2==ep||position[p2]==oP&&p1==ep)) dblPin=true;
			}
			result=slideRayL[p, data];
			if((result&pPos)!=0) { checkPieceCount++; canStopCheck|=result; } else {
				pData=slideHitL[p, data]; p1=pData&0xFF; p2=(pData>>8)&0xFF; p3=pData>>16;
				if((position[p2]==pR||position[p2]==pQ)&&position[p1]>>3==whoseMove) pinByOpp[p1]=1;
				else if(p3!=NS&&ep!=NS&&(position[p3]==pR||position[p3]==pQ)&&(position[p1]==oP&&p2==ep||position[p2]==oP&&p1==ep)) dblPin=true;
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

			// 其他的棋步一律只有當不是雙將軍的時候才有可能下
			// 先考慮沒將軍的情況
			if(checkPieceCount==0) {

				// 小兵
				if(whoseMove==WT) {
					for(i=0;i<pieceCount[wP];i++) {
						so=pieceList[wP, i];
						if(position[ta=(byte)(so+8)]==b0&&(pinByOpp[so]&relDirV)==0) {
							if((so>>3)==6) {
								moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wN<<ntS)|len4;
								moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wR<<ntS)|len4;
								moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wB<<ntS)|len4;
								moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wQ<<ntS)|len4;
							} else {
								moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wP<<ntS)|len2;
								if((so>>3)==1&&position[ta=(byte)(so+16)]==b0)
									moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wP<<ntS)|len2;
							}
						}
						if((ta=pieceRuleWP[so, 0])!=NS) {
							if((pinByOpp[so]&relDir[so, ta])==0) {
								if((cp=position[ta])>>3==BC) {
									if((so>>3)==6) {
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wN<<ntS)|((ulong)cp<<cpS)|len6;
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wR<<ntS)|((ulong)cp<<cpS)|len6;
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wB<<ntS)|((ulong)cp<<cpS)|len6;
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wQ<<ntS)|((ulong)cp<<cpS)|len6;
									} else moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wP<<ntS)|((ulong)cp<<cpS)|len4;
								} else if(ta==enPassantState[depth]&&!dblPin)
									moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ep<<deS)|((ulong)wP<<otS)|((ulong)wP<<ntS)|((ulong)bP<<cpS)|((ulong)epMove<<miS)|len6;
							}
							if((ta=pieceRuleWP[so, 1])!=NS&&(pinByOpp[so]&relDir[so, ta])==0) {
								if((cp=position[ta])>>3==BC) {
									if((so>>3)==6) {
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wN<<ntS)|((ulong)cp<<cpS)|len6;
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wR<<ntS)|((ulong)cp<<cpS)|len6;
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wB<<ntS)|((ulong)cp<<cpS)|len6;
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wQ<<ntS)|((ulong)cp<<cpS)|len6;
									} else moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wP<<ntS)|((ulong)cp<<cpS)|len4;
								} else if(ta==enPassantState[depth]&&!dblPin)
									moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ep<<deS)|((ulong)wP<<otS)|((ulong)wP<<ntS)|((ulong)bP<<cpS)|((ulong)epMove<<miS)|len6;
							}
						}
					}
				} else {
					for(i=0;i<pieceCount[bP];i++) {
						so=pieceList[bP, i];
						if(position[ta=(byte)(so-8)]==b0&&(pinByOpp[so]&relDirV)==0) {
							if((so>>3)==1) {
								moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bN<<ntS)|len4;
								moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bR<<ntS)|len4;
								moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bB<<ntS)|len4;
								moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bQ<<ntS)|len4;
							} else {
								moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bP<<ntS)|len2;
								if((so>>3)==6&&position[ta=(byte)(so-16)]==b0)
									moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bP<<ntS)|len2;
							}
						}
						if((ta=pieceRuleBP[so, 0])!=NS) {
							if((pinByOpp[so]&relDir[so, ta])==0) {
								if((cp=position[ta])>>3==WT&&cp!=0) {
									if((so>>3)==1) {
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bN<<ntS)|((ulong)cp<<cpS)|len6;
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bR<<ntS)|((ulong)cp<<cpS)|len6;
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bB<<ntS)|((ulong)cp<<cpS)|len6;
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bQ<<ntS)|((ulong)cp<<cpS)|len6;
									} else moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bP<<ntS)|((ulong)cp<<cpS)|len4;
								} else if(ta==enPassantState[depth]&&!dblPin)
									moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ep<<deS)|((ulong)bP<<otS)|((ulong)bP<<ntS)|((ulong)wP<<cpS)|((ulong)epMove<<miS)|len6;
							}
							if((ta=pieceRuleBP[so, 1])!=NS&&(pinByOpp[so]&relDir[so, ta])==0) {
								if((cp=position[ta])>>3==WT&&cp!=0) {
									if((so>>3)==1) {
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bN<<ntS)|((ulong)cp<<cpS)|len6;
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bR<<ntS)|((ulong)cp<<cpS)|len6;
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bB<<ntS)|((ulong)cp<<cpS)|len6;
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bQ<<ntS)|((ulong)cp<<cpS)|len6;
									} else moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bP<<ntS)|((ulong)cp<<cpS)|len4;
								} else if(ta==enPassantState[depth]&&!dblPin)
									moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ep<<deS)|((ulong)bP<<otS)|((ulong)bP<<ntS)|((ulong)wP<<cpS)|((ulong)epMove<<miS)|len6;
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
								else { if(cp>>3!=whoseMove) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oR<<otS)|((ulong)oR<<ntS)|((ulong)cp<<cpS)|len4; break;}
							}
				}
				
				// 主教棋步
				for(i=0;i<pieceCount[oB];i++) {
					so=pieceList[oB, i];
					for(j=0;DIRuleDir[so, j]!=relDirNO;j++)
						if((pinByOpp[so]&DIRuleDir[so, j])==0)
							for(k=0;(ta=DIRule[so, j, k])!=NS;k++) {
								if((cp=position[ta])==0) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oB<<otS)|((ulong)oB<<ntS)|len3;
								else { if(cp>>3!=whoseMove) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oB<<otS)|((ulong)oB<<ntS)|((ulong)cp<<cpS)|len4; break;}
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
				if(whoseMove==WT) {
					if((castlingState[depth]&cwK)!=0&&position[5]==b0&&position[6]==b0&&(attackByOpp&(mask[5]|mask[6]))==0) moveList[depth, l++]=wOO;
					if((castlingState[depth]&cwQ)!=0&&position[1]==b0&&position[2]==b0&&position[3]==b0&&(attackByOpp&(mask[2]|mask[3]))==0) moveList[depth, l++]=wOOO;
				} else {
					if((castlingState[depth]&cbK)!=0&&position[61]==b0&&position[62]==b0&&(attackByOpp&(mask[61]|mask[62]))==0) moveList[depth, l++]=bOO;
					if((castlingState[depth]&cbQ)!=0&&position[57]==b0&&position[58]==b0&&position[59]==b0&&(attackByOpp&(mask[58]|mask[59]))==0) moveList[depth, l++]=bOOO;
				}
			}
			
			// 然後是單將軍的情況，差別只在於目的地必須在 canStopCheck 清單當中
			else if(checkPieceCount==1) {
				// 小兵
				if(whoseMove==WT) {
					for(i=0;i<pieceCount[wP];i++) {
						so=pieceList[wP, i];
						if(position[ta=(byte)(so+8)]==b0&&(pinByOpp[so]&relDirV)==0) {
							if((so>>3)==6) {
								if((canStopCheck&mask[ta])!=0) {
									moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wN<<ntS)|len4;
									moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wR<<ntS)|len4;
									moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wB<<ntS)|len4;
									moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wQ<<ntS)|len4;
								}
							} else {
								if((canStopCheck&mask[ta])!=0) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wP<<ntS)|len2;
								if((so>>3)==1&&position[ta=(byte)(so+16)]==b0&&(canStopCheck&mask[ta])!=0)
									moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wP<<ntS)|len2;
							}
						}
						if((ta=pieceRuleWP[so, 0])!=NS) {
							if((pinByOpp[so]&relDir[so, ta])==0&&(canStopCheck&mask[ta])!=0) {
								if((cp=position[ta])>>3==BC) {
									if((so>>3)==6) {
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wN<<ntS)|((ulong)cp<<cpS)|len6;
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wR<<ntS)|((ulong)cp<<cpS)|len6;
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wB<<ntS)|((ulong)cp<<cpS)|len6;
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wQ<<ntS)|((ulong)cp<<cpS)|len6;
									} else moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wP<<ntS)|((ulong)cp<<cpS)|len4;
								} else if(ta==enPassantState[depth]&&!dblPin)
									moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ep<<deS)|((ulong)wP<<otS)|((ulong)wP<<ntS)|((ulong)bP<<cpS)|((ulong)epMove<<miS)|len6;
							}
							if((ta=pieceRuleWP[so, 1])!=NS&&(pinByOpp[so]&relDir[so, ta])==0&&(canStopCheck&mask[ta])!=0) {
								if((cp=position[ta])>>3==BC) {
									if((so>>3)==6) {
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wN<<ntS)|((ulong)cp<<cpS)|len6;
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wR<<ntS)|((ulong)cp<<cpS)|len6;
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wB<<ntS)|((ulong)cp<<cpS)|len6;
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wQ<<ntS)|((ulong)cp<<cpS)|len6;
									} else moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)wP<<otS)|((ulong)wP<<ntS)|((ulong)cp<<cpS)|len4;
								} else if(ta==enPassantState[depth]&&!dblPin)
									moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ep<<deS)|((ulong)wP<<otS)|((ulong)wP<<ntS)|((ulong)bP<<cpS)|((ulong)epMove<<miS)|len6;
							}
						}
					}
				} else {
					for(i=0;i<pieceCount[bP];i++) {
						so=pieceList[bP, i];		
						if(position[ta=(byte)(so-8)]==b0&&(pinByOpp[so]&relDirV)==0) {
							if((so>>3)==1) {
								if((canStopCheck&mask[ta])!=0) {
									moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bN<<ntS)|len4;
									moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bR<<ntS)|len4;
									moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bB<<ntS)|len4;
									moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bQ<<ntS)|len4;
								}
							} else {
								if((canStopCheck&mask[ta])!=0) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bP<<ntS)|len2;
								if((so>>3)==6&&position[ta=(byte)(so-16)]==b0&&(canStopCheck&mask[ta])!=0)
									moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bP<<ntS)|len2;
							}
						}
						if((ta=pieceRuleBP[so, 0])!=NS) {
							if((pinByOpp[so]&relDir[so, ta])==0&&(canStopCheck&mask[ta])!=0) {
								if((cp=position[ta])>>3==WT&&cp!=0) {
									if((so>>3)==1) {
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bN<<ntS)|((ulong)cp<<cpS)|len6;
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bR<<ntS)|((ulong)cp<<cpS)|len6;
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bB<<ntS)|((ulong)cp<<cpS)|len6;
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bQ<<ntS)|((ulong)cp<<cpS)|len6;
									} else moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bP<<ntS)|((ulong)cp<<cpS)|len4;
								} else if(ta==enPassantState[depth]&&!dblPin)
									moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ep<<deS)|((ulong)bP<<otS)|((ulong)bP<<ntS)|((ulong)wP<<cpS)|((ulong)epMove<<miS)|len6;
							}
							if((ta=pieceRuleBP[so, 1])!=NS&&(pinByOpp[so]&relDir[so, ta])==0&&(canStopCheck&mask[ta])!=0) {
								if((cp=position[ta])>>3==WT&&cp!=0) {
									if((so>>3)==1) {
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bN<<ntS)|((ulong)cp<<cpS)|len6;
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bR<<ntS)|((ulong)cp<<cpS)|len6;
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bB<<ntS)|((ulong)cp<<cpS)|len6;
										moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bQ<<ntS)|((ulong)cp<<cpS)|len6;
									} else moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)bP<<otS)|((ulong)bP<<ntS)|((ulong)cp<<cpS)|len4;
								} else if(ta==enPassantState[depth]&&!dblPin)
									moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ep<<deS)|((ulong)bP<<otS)|((ulong)bP<<ntS)|((ulong)wP<<cpS)|((ulong)epMove<<miS)|len6;
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
				if(mi!=OOMove&&mi!=OOOMove) {
					if((canAttackOppKing[nt]&mask[ta])!=0) tag=tgCheck;							// 直接走就進入攻擊位置（含升變）
					else if(mi==epMove&&(dblDis||pinBySelf[de]!=0)||
						(pinBySelf[so]&relDir[so, ta])!=0) tag=tgCheck;							// 閃擊（涵蓋了一次閃兩子的橫向閃擊情況、以及斜向的吃過路兵閃擊）
				}
				// 注意到如果一個可以入堡的國王被自己釘住，
				// 那只有可能是對方的國王在另一側，所以如果做入堡動作一定會導致將軍
				else if(pinBySelf[so]==1) tag=tgCheck;											// 入堡的情況要多做一種「入堡閃擊」的判斷
				else if(mi==OOMove&&(canAttackOppKing[oR]&mask[so+1])!=0) tag=tgCheck;			// 一般的王側入堡將軍
				else if(mi==OOOMove&&(canAttackOppKing[oR]&mask[so-1])!=0) tag=tgCheck;			// 一般的后側入堡將軍
				
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

#if DEBUG
			//for(i=0;i<l;i++) totalMoveCount[moveToLength(moveList[depth, i])]++;
#endif
			if(l==0&&depth>0&&(moveHis[depth-1]>>tgS&0xF)==tgCheck) moveHis[depth-1]|=(ulong)tgCheckmate<<tgS;		// 如果沒有合法棋步可動，且上一步是將軍，換掉將軍符號為將死
		}

		// 騎士的新程式碼（因為騎士範圍太小，用 MSB 反而會浪費時間，所以不採用）

		//if(pinByOpp[so=pieceList[oN, i]]==0&&(m=pieceRangeN[so]&canStopCheck)!=0)
		//    while(m!=0) {
		//        ta=MSB(m); m^=mask[ta];
		//        if((cp=position[ta])==0) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oN<<otS)|((ulong)oN<<ntS)|len3;
		//        else moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oN<<otS)|((ulong)oN<<ntS)|((ulong)cp<<cpS)|len4;
		//    }

		// 城堡的舊程式碼

		//for(j=0;HVRuleDir[so, j]!=relDirNO;j++)
		//    if((pinByOpp[so]&HVRuleDir[so, j])==0)
		//        for(k=0;(ta=HVRule[so, j, k])!=NS;k++) {
		//            cp=position[ta];
		//            if((canStopCheck&mask[ta])!=0)
		//                if(cp==0) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oR<<otS)|((ulong)oR<<ntS)|len3;
		//                else if(cp>>3!=whoseMove) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oR<<otS)|((ulong)oR<<ntS)|((ulong)cp<<cpS)|len4;
		//            if(cp!=0) break;
		//        }

		// 主教的舊程式碼

		//for(j=0;DIRuleDir[so, j]!=relDirNO;j++)
		//    if((pinByOpp[so]&DIRuleDir[so, j])==0)
		//        for(k=0;(ta=DIRule[so, j, k])!=NS;k++) {
		//            cp=position[ta];
		//            if((canStopCheck&mask[ta])!=0)
		//                if(cp==0) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oB<<otS)|((ulong)oB<<ntS)|len3;
		//                else if(cp>>3!=whoseMove) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oB<<otS)|((ulong)oB<<ntS)|((ulong)cp<<cpS)|len4;
		//            if(cp!=0) break;
		//        }

		// 皇后的舊程式碼

		//for(j=0;HVRuleDir[so, j]!=relDirNO;j++)
		//    if((pinByOpp[so]&HVRuleDir[so, j])==0)
		//        for(k=0;(ta=HVRule[so, j, k])!=NS;k++) {
		//            cp=position[ta];
		//            if((canStopCheck&mask[ta])!=0)
		//                if(cp==0) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|len3;
		//                else if(cp>>3!=whoseMove) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4;
		//            if(cp!=0) break;
		//        }
		//for(j=0;DIRuleDir[so, j]!=relDirNO;j++)
		//    if((pinByOpp[so]&DIRuleDir[so, j])==0)
		//        for(k=0;(ta=DIRule[so, j, k])!=NS;k++) {
		//            cp=position[ta];
		//            if((canStopCheck&mask[ta])!=0)
		//                if(cp==0) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|len3;
		//                else if(cp>>3!=whoseMove) moveList[depth, l++]=((ulong)so)|((ulong)ta<<taS)|((ulong)ta<<deS)|((ulong)oQ<<otS)|((ulong)oQ<<ntS)|((ulong)cp<<cpS)|len4;
		//            if(cp!=0) break;
		//        }

	}
}