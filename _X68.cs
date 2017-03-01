using System;

namespace rcrml
{
	public unsafe class _X68
	{
		static public byte AX = 0;
		static public byte CX = 1;
		static public byte DX = 2;
		static public byte BX = 3;
		static public byte SP = 4;
		static public byte BP = 5;
		static public byte SI = 6;
		static public byte DI = 7;

		static public byte EX  = 0x0F;
		static public byte IM1 = 0x40;
		static public byte IM4 = 0x80;

		static public byte CALLR = 0xE8;

		static private byte T2 = (1<<7) + (1<<6);
		//static private byte M3 = (1<<5) + (1<<4) + (1<<3);
		static private byte B3 = (1<<2) + (1<<1) + (1<<0);

		static public byte[] B2L = new byte[512];

		static private void O(params int[] _data)
		{
			if (_data[0] == EX)
			{
				B2L[256+_data[1]] = (byte)_data[2];
				return;
			}
			B2L[_data[0]] = (byte)_data[1];
		}

		//group opcodes
		static private void G(int _base,int _range, int _size)
		{
			int i = 0;
			while(true)
			{
				if (i > _range)
					return;
				O(_base + i++,_size);
			}
		}

		//bool is ignored, if used, always emit extension byte
		static private void G(int _base,int _range, int _size, bool ex)
		{
			int i = 0;
			while(true)
			{
				if (i > _range)
					return;
				O(EX,_base + i++,_size);
			}
		}

		static _X68()
		{
			//opcodes that does not accept modRMSIB

			//opcode groups
			G(0x40,7,1); //push
			G(0x48,7,1); //pop
			G(0x50,7,1); //inc
			G(0x58,7,1); //dec

			G(0xB0,7,2); //mov8

			G(0xB8,7,5); //mov32

			//yes all group is rel32
			//this is extension opcode 0F80 not just 80
			//it's mov opcodes, all
			G(0x80,15,6,true);

			O (0xC3, 1); //RET
			O (0xC9, 1); //LEAVE
			O (0x90, 1); //NOP

			O (0x99, 1); //Convert Word to Doubleword

			O (0xEB, 2); //jump rel8

			O (0x6A, 2); //push8

			O (0x75, 2);

			O (0x68, 5); //push word

			O (CALLR, 5); //call relative word
			O (0xE9, 5); //jump relative word

			O (0x25, 5);

			O(0x83,IM1); //rm + imm8
			O(0xC7,IM4); //mov + rm + imm32
		}

		static public int GetOpcodeLenght (int _base, params byte[] _data)
		{
			//Console.WriteLine ("OPERATION IS: " + string.Format ("{0:X2}", _data [_base]));

			int ret = 1;//size of base is atleast single byte
			int key = 0;

			if (_data [_base] == EX) //this is two byte opcode
			{
				key = B2L[256 + _data [ ++ _base]]; //read data from database about next opcode instead
				ret++; //add size of 0F to output
			}
			else //single byte opcode
			{
				key = B2L[_data [_base]]; // just load data
			}


			//base offset also pushed forward
				

			//Console.WriteLine ("KEY IS: " + key);
			//Console.WriteLine ("RET BEFORE KEY IS: " + ret);
			if (key != 0) //something is stored for opcode
			{
				if ((key & (IM4+IM1)) != 0) //have IMM addition
				{
					if ((key & IM1) != 0) //have single IMM
					{
						//Console.WriteLine ("HIT MULTIPLE IM1");
						ret++; 
					} 

					if ((key & IM4) != 0)
					{
						//Console.WriteLine ("HIT MULTIPLE IM4");
						ret+=4;
					}
				}
				else //stored value is absolute and forced one, size of twobyte defined direcly in database
				{
					return key;
				}
			}
			//Console.WriteLine ("RET AFTER KEY IS: " + ret);

			// read next byte that expected to be RM
			int top = (_data [_base+1] & T2) >> 6;
			int bot = (_data [_base+1] & B3);

			//Console.WriteLine ("TOP is " + top);
			//Console.WriteLine ("BOT is " + bot);

			ret++;


			//Console.WriteLine ("RET BEFORE RMPARSE IS: " + ret);

			if (top == 0) 
			{
				//this one is tricky
				if (bot == 4) 
				{
					int sbot = (_data [_base+2] & B3);

					//Console.WriteLine ("SBOT is " + sbot);

					if (sbot == 5)
						ret+=4;
					ret++;
				}

				if (bot == 5) 
				{
					ret+=4;
				}
			}

			if (top == 1) 
			{
				//always displacement 8
				ret++;
				//size of sib
				if (bot == 4)
					ret++;
			}

			if (top == 2) 
			{
				//always displacement 32
				ret+=4;
				//size of sib
				if (bot == 4)
					ret++;
			}

			//top 3 never sib and always same default displacement
				
			return ret;
		}



		static public void Update__CALLR(int delta,params byte[] _source)
		{
			int walk = 0;

			int tmp = 0;

			int composite = 0;
			byte* compositeBREF = (byte*)&composite;

			while (true) 
			{
				if (_source [walk] == CALLR)
				{
					compositeBREF [0] = _source [walk + 1];
					compositeBREF [1] = _source [walk + 2];
					compositeBREF [2] = _source [walk + 3];
					compositeBREF [3] = _source [walk + 4];
					composite += delta;
					//composite = 0;
					_source [walk + 1] = compositeBREF [0];
					_source [walk + 2] = compositeBREF [1];
					_source [walk + 3] = compositeBREF [2];
					_source [walk + 4] = compositeBREF [3];
				}


				tmp = GetOpcodeLenght (walk, _source);

				//Console.WriteLine ("OPCODE " + string.Format ("{0:X2}",_source [walk]) + " size " + tmp);


				//will never trigger due to current implementation
				if (tmp == -1) 
				{
					Console.WriteLine ("FAILED AT " + walk + "@" + string.Format ("{0:X2}", walk));
					return;
				}

				walk += tmp;

				if (walk >= _source.Length)
					return;
			}
		}



	}
}