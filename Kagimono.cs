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

namespace rcrml
{
	[StaticConstructorOnStartup]
	public unsafe class Kagimono
	{
		static Kagimono()
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
			

		static public void ORIGINAL(int _test_int)
		{
			//this method do allocate it's own stack frame
			Console.WriteLine("ORIGINAL " + _test_int);
		}


		static _FuncPtr32 stored_ORIGINAL;
		static public void __GATE__ORIGINAL(int _test_int)
		{
			//just like this method
			//skipping this stackframe and passing control into original
			//actually should work
			_test_int+= 8;
			//Console.WriteLine("REPLACEMENT " + _test_int);
			stored_ORIGINAL.__HOST();
		}


		static unsafe public void Main (string[] args)
		{
			Console.WriteLine ("Entry:start");

			stored_ORIGINAL = new _FuncPtr32(typeof(Kagimono).GetMethod("ORIGINAL"));
			stored_ORIGINAL.Rebase();
			stored_ORIGINAL.Rebind();

			//this stored copy of ORIGINAL inside unmanaged memory, ready to be invoked
			//stored_ORIGINAL.__CALL();

			_FuncPtr32 jmp = new _FuncPtr32(typeof(Kagimono).GetMethod("ORIGINAL"));
			_FuncPtr32 gate = new _FuncPtr32(typeof(Kagimono).GetMethod("__GATE__ORIGINAL"));


			jmp.Stream2Image();
			jmp.Stream(__arglist((byte)0x68,(int)gate,(byte)0xC3));
			jmp.Image2Raw();

			ORIGINAL(10);


			Console.WriteLine ("Entry:complete");
		}
	}
}