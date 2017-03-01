using System;
using System.Runtime.CompilerServices;

namespace rcrml
{
	public unsafe class _ReJIT
	{
		//this is "magical" rejit class that allows to emit custom assembly into existing managed method
		//including operations that are not allowed by C# or default calling convention
		//like modification of stackframe, that belongs to other function call

		//there are absolutely no safety checks, you can crash entire runtime by single invalid opcode
		//also you may cause delayed crash if mess with GC or return pointers

		//method will be called like normal, hitting __REPLACE routine will terminate method
		//replace it's code with provided sequence
		//and RESTART method
		//if method can't be replaced, new method will be allocated inside unmanaged memory
		//relative calls like E8 and E9 will be patched automatically
		//everything else will be left as it is

		//you can use __REPLACE(0xC3) inside condition to "nullify" singlerun methods
		//also you may put __REPLACE(0xC3) to very end of method with same result
		//method will not be changed before actual call into __REPLACE

		//obviously, you can't use __REBASE to emic call into rebase, it will cause "problems"

		//static unsafe void __REJIT_


		static _ReJIT()
		{
			_FuncPtr32 RETIMM = new _FuncPtr32(typeof(_ReJIT).GetMethod("__TOPCALLER"));

			RETIMM.Array2Image
			(
				0x8B, 0x45, 0x04,	//mov    eax,DWORD PTR [esp+0x0]
				0xC3					//ret
			);
			RETIMM.Image2Raw();

			_FuncPtr32 DROPSPECIAL = new _FuncPtr32(typeof(_ReJIT).GetMethod("__DROPSPECIAL"));

			DROPSPECIAL.Array2Image
			(
				0x8B, 0x44, 0x24, 0x04, 
				0xC9, 
				0xC9, 
				0x50,
				0xC3 
			);
			DROPSPECIAL.Image2Raw();
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static public void* __TOPCALLER()
		{
			//this method returns immediate return adress
			//this is point where our method should normally return
			//unframed method (running inside frame of caller)
			//used to determinate caller
			//reflection is unsupported for now
			throw new Exception("__TOPCALLER FAILURE");
		}
		[MethodImpl(MethodImplOptions.NoInlining)]
		static public int __DROPSPECIAL(int _jump_abs)
		{
			//composite method that will drop two stackframes
			//current stackframe and next frame above
			//and jump into _jump_abs
			throw new Exception("__DROPSPECIAL FAILURE");
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static public void __REPLACE (params byte[] _opcodes)
		{
			_FuncPtr32 caller = new _FuncPtr32(__TOPCALLER());

			//caller.WriteLine();

			if (caller.Size < _opcodes.Length) //|| true)
			{
				//array cannot fit existing allocation
				//Console.WriteLine("test");
				_FuncPtr32 rebase = new _FuncPtr32(0,_opcodes.Length);
				rebase.Array2Image(_opcodes);
				rebase.Image2Raw();

				rebase.WriteLine();


				//Console.WriteLine("test2");
				caller.Stream2Image();
				caller.Stream((byte)0x68);
				caller.Stream((int)rebase);
				caller.Stream((byte)0xC3);
				//Console.WriteLine("test4");
				caller.Image2Raw();

				caller.WriteLine();
				//Console.WriteLine("test5");
			}
			else
			{
				//array can fit
				caller.Array2Image(_opcodes);
				caller.Image2Raw();
			}
			//Console.WriteLine("test6");
			__DROPSPECIAL((int)caller);//this value is adjusted
			//throw new Exception("__REPLACE FAILURE");
		}

		//this action is possible but somewhat complicated
		//relative jumps (jmpr8) used for loops and conditions must be updated for entire method
		//relative calls after site of injection (callr32) must be updated
		//site of injection is place of callr32 instruction into __INJECT
		//it cannot be before stackframe allocation or destruction
		//if you want to alter stackframe allocation use __HEAD or __TAIL methods
		[MethodImpl(MethodImplOptions.NoInlining)]
		static public void __INJECT (params byte[] _opcodes)
		{
			throw new Exception("not yet implemented");
		}
	}
}
