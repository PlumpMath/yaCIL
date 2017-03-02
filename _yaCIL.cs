using System;
using System.Reflection;
using Verse;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading;
using RimWorld;
using System.Collections.Generic;
using System.Text;

namespace yacil
{
	[StaticConstructorOnStartup]
	public unsafe class _yaCIL
	{
		static _yaCIL()
		{
			Verse.LongEventHandler.QueueLongEvent (gate_Main, "gate_Main_indirect_call", false, null);
		}

		public class ConsoleToUnityLogWritter : System.IO.TextWriter
		{

			public override System.Text.Encoding Encoding
			{
				get { return null;}
			}

			public override void Write (string value)
			{
				Log.Warning (value);
			}

			public override void Write (object value)
			{
				Log.Warning (value.ToString ());
			}

			public override void WriteLine ()
			{
				return;
			}
		}

		static public void gate_Main()
		{
			Console.SetOut (new ConsoleToUnityLogWritter ());
			Main (null);
		}
			

		static public int ORIGINAL(int _test_int)
		{
			//this method do allocate it's own stack frame
			Console.WriteLine("ORIGINAL " + _test_int);
			return 84;
		}


		static _FuncPtr32 stored_ORIGINAL;
		static public int __GATE__ORIGINAL(int _test_int)
		{
			//just like this method
			//skipping this stackframe and passing control into original
			//actually should work
			_test_int+= 8;
			//Console.WriteLine("REPLACEMENT " + _test_int);
			stored_ORIGINAL.__HOST();
			//stored_ORIGINAL.__RETNOW();
			return 0;
		}
			
		private delegate Int32 MyAdd(Int32 x, Int32 y);

		static unsafe public void Main (string[] args)
		{
			Console.WriteLine ("Entry:start");

			stored_ORIGINAL = new _FuncPtr32(typeof(_yaCIL).GetMethod("ORIGINAL"));
			stored_ORIGINAL.Rebase();
			stored_ORIGINAL.Rebind();

			//this stored copy of ORIGINAL inside unmanaged memory, ready to be invoked
			//stored_ORIGINAL.__CALL();

			_FuncPtr32 jmp = new _FuncPtr32(typeof(_yaCIL).GetMethod("ORIGINAL"));
			_FuncPtr32 gate = new _FuncPtr32(typeof(_yaCIL).GetMethod("__GATE__ORIGINAL"));


			jmp.Stream2Image();
			jmp.Stream(__arglist((byte)0x68,(int)gate,(byte)0xC3));
			jmp.Image2Raw();

			//stored_ORIGINAL.__CALL();
			int tgtg = ORIGINAL(10);


			//Byte[] myAddNativeCodeBytes = new Byte[]

			//{

				//0x8B, 0x44, 0x24, 0x08, // mov eax,dword ptr [esp+8]

				//0x8B, 0x4C, 0x24, 0x04, // mov ecx,dword ptr [esp+4]

				//0x03, 0xC1,             // add eax,ecx

				//0xC2, 0x08, 0x00        // ret 8

			//};

			//IntPtr myAddNativeCodeBytesPtr =

				//Marshal.AllocHGlobal(myAddNativeCodeBytes.Length);

			//Marshal.Copy(myAddNativeCodeBytes, 0,

				//myAddNativeCodeBytesPtr, myAddNativeCodeBytes.Length);

			//MyAdd myAdd = (MyAdd)Marshal.GetDelegateForFunctionPointer(

				//myAddNativeCodeBytesPtr, typeof(MyAdd));

			//Int32 result = myAdd(4, 5);


			// Did it work?

			//Console.WriteLine("Result: {0}", result);

			//Console.WriteLine(tgtg);



			Console.WriteLine ("Entry:complete");
		}
	}
}