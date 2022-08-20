
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mushikui_Puzzle_Workshop {

	/////////////////////////////////
	// 雜湊表
	/////////////////////////////////

	public class HashTable<T> {

		private int hashBits;
		private int tableSize;
		private int keySize;

		private byte[,] tabKey;
		private byte[]	tempKey;
		private bool[]	tabOcup;
		private T[]		tabData;

		/////////////////////////////////
		// 建構子
		/////////////////////////////////

		public HashTable(int HashBits, int KeySize) {
			hashBits=HashBits;
			tableSize=1<<hashBits;
			keySize=KeySize;
			tempKey=new byte[keySize];
		}

		/////////////////////////////////
		// 配置方法
		/////////////////////////////////
		
		public void Initialize() {
			tabKey=new byte[tableSize, keySize];
			tabKey.Initialize();
			tabOcup=new bool[tableSize];
			tabOcup.Initialize();
			tabData=new T[tableSize];
			tabData.Initialize();
		}
		public void Clear() {		// 在不去干涉鍵表和值表的情況下，單純清除佔據表
			tabOcup=null;
			GC.Collect();
			tabOcup=new bool[tableSize];
			tabOcup.Initialize();
		}
		public void Delete() {		// 完全釋放記憶體
			tabKey=null;
			tabOcup=null;
			tabData=null;
			GC.Collect();
		}

		/////////////////////////////////
		// 資料使用方法
		/////////////////////////////////

		public T Value {
			get; private set;
		}
		public bool LookUp(byte[] key) {			// 查表的時候，先呼叫這個函數，傳回真偽值表示該鍵是否已經存在，然後再呼叫 Value 屬性取得值
			int hash=MH2(key);
			if(tabOcup[hash]) {
				Buffer.BlockCopy(tabKey, hash*keySize, tempKey, 0, keySize);
				if(tempKey.SequenceEqual(key)) {
					Value=tabData[hash];
					return true;
				}
			}
			return false;
		}
		public bool Insert(byte[] key, T data) {	// 目前這個雜湊表並不進行碰撞迴避，只有當 bucket 為空的時候才會填入。傳回真偽值表示填入是否成功
			int hash=MH2(key);
			if(!tabOcup[hash]) {
				Buffer.BlockCopy(key, 0, tabKey, hash*keySize, keySize);
				tabData[hash]=data;
				tabOcup[hash]=true;
				return true;
			}
			return false;
		}

		/////////////////////////////////
		// 雜湊處理
		/////////////////////////////////
		
		public int MH2(byte[] key)  {
			return (int)(MH2U.Hash(key)>>(32-hashBits));
		}
		private MurmurHash2Unsafe MH2U=new MurmurHash2Unsafe();		// 使用的是我能找到最快的 MurmurHash2 函數的最快 Unsafe C# 實作

	}
	
	/////////////////////////////////
	// MurmurHash2
	/////////////////////////////////

	public interface IHashAlgorithm {
		UInt32 Hash(Byte[] data);
	}
	public interface ISeededHashAlgorithm:IHashAlgorithm {
		UInt32 Hash(Byte[] data, UInt32 seed);
	}
	public class MurmurHash2Unsafe:ISeededHashAlgorithm {
		public UInt32 Hash(Byte[] data) {
			return Hash(data, 0xc58f1a7b);
		}
		const UInt32 m=0x5bd1e995;
		const Int32 r=24;

		public unsafe UInt32 Hash(Byte[] data, UInt32 seed) {
			Int32 length=data.Length;
			if(length==0)
				return 0;
			UInt32 h=seed^(UInt32)length;
			Int32 remainingBytes=length&3; // mod 4
			Int32 numberOfLoops=length>>2; // div 4
			fixed(byte* firstByte=&(data[0])) {
				UInt32* realData=(UInt32*)firstByte;
				while(numberOfLoops!=0) {
					UInt32 k=*realData;
					k*=m;
					k^=k>>r;
					k*=m;

					h*=m;
					h^=k;
					numberOfLoops--;
					realData++;
				}
				switch(remainingBytes) {
					case 3:
						h^=(UInt16)(*realData);
						h^=((UInt32)(*(((Byte*)(realData))+2)))<<16;
						h*=m;
						break;
					case 2:
						h^=(UInt16)(*realData);
						h*=m;
						break;
					case 1:
						h^=*((Byte*)realData);
						h*=m;
						break;
					default:
						break;
				}
			}
			h^=h>>13;
			h*=m;
			h^=h>>15;

			return h;
		}
	}
}
