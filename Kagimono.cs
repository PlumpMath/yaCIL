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
			
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static int __rejit_grab_caller()
		{
			Console.WriteLine ("VOID");
			Console.WriteLine ("VOID");
			return 0;
		}


		[MethodImpl(MethodImplOptions.NoInlining)]
		public static int CALLGATE()
		{
			//callgate is invoked instead of hooked routine
			//for this reason, callgate have access to all arguments
			//and able to set "return"
			//to make things simple and easy, no arguments are passed directly
			//access of arguments done via seminative
			//return also done via seminative
			//calling other method from inside of same stackframe should be
			//implemented via special jitter

			//invoking other method from middle of stack is not possible
			//probably different kind of memory should be used, or perform stack area copy
			//to allow method to access arguments without possible corruption of stack structure
			//yes, i need to perform signature fetch and params copy
			//number of arguments is extracted from data stored on hooking


			//original routine is extracted by this.

			//int xx = __rejit_grab_caller ();

			//byte* dxdx = (byte*)xx;
			//dxdx -= 5;
			//Console.WriteLine ("{0:X2}",dxdx[0]);
			//dxdx += 1;
			//Console.WriteLine ("{0:X2}",((int*)dxdx)[0]);


			//int vex = ((int*)dxdx) [0];

			//int offset = vex + xx;

			//Console.WriteLine (offset);
			//Console.WriteLine (new _FunctionHandle (offset));
			return 20;
		}


		//need this for RMSIB byte calculation
		static string PadBold(byte b)
		{
			string bin = Convert.ToString(b, 2);
			return new string('0', 8 - bin.Length) + bin;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void TEST_HOOK()
		{
			Console.WriteLine ("na");
		}


		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void TEST_NEW()
		{
			TEST_HOOK ();
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void TEST_THROW()
		{
			throw null;
		}

		class SampleCollection<T>
		{
			// Declare an array to store the data elements.
			private T[] arr = new T[100];

			// Define the indexer, which will allow client code
			// to use [] notation on the class instance itself.
			// (See line 2 of code in Main below.)        
			public T this[int i]
			{
				get
				{
					// This indexer is very simple, and just returns or sets
					// the corresponding element from the internal array.
					return arr[i];
				}
				set
				{
					arr[i] = value;
				}
			}
		}
			
		public static Thing MakeThing(ThingDef def, ThingDef stuff)
		{
			if (stuff != null && !stuff.IsStuff)
			{
				Log.Error(string.Concat(new object[]
					{
						"MakeThing error: Tried to make ",
						def,
						" from ",
						stuff,
						" which is not a stuff. Assigning default."
					}));
				stuff = GenStuff.DefaultStuffFor(def);
			}
			if (def.MadeFromStuff && stuff == null)
			{
				Log.Error("MakeThing error: " + def + " is madeFromStuff but stuff=null. Assigning default.");
				stuff = GenStuff.DefaultStuffFor(def);
			}
			if (!def.MadeFromStuff && stuff != null)
			{
				Log.Error(string.Concat(new object[]
					{
						"MakeThing error: ",
						def,
						" is not madeFromStuff but stuff=",
						stuff,
						". Setting to null."
					}));
				stuff = null;
			}
				

			if (def.ingestible != null)
				if (def.ingestible.foodType == FoodTypeFlags.Meat)
					def = DefDatabase<ThingDef>.GetNamed ("Chicken_Meat",false);

			Thing thing;

			try
			{
				thing = (Thing)Activator.CreateInstance(def.thingClass);
			}catch 
			{
				Dictionary<Def,ModContentPack> test = new Dictionary<Def,ModContentPack> ();
				foreach (ModContentPack tz in LoadedModManager.RunningMods) 
				{
					foreach (Def gh in tz.AllDefs) 
					{
						test [gh] = tz;
					}
				}
					
				throw new Exception (def.defName + " have invalid thingclass owner is " + test[def].Name);
			};

			thing.def = def;
			thing.SetStuffDirect(stuff);
			thing.PostMake();
			return thing;
		}


		static public _FuncPtr32 __GATE_GetFunctionPointer;
		static public IntPtr __REJIT_GATE_GetFunctionPointer(RuntimeMethodHandle _handle)
		{
			Console.WriteLine (Environment.StackTrace);
			return __GATE_GetFunctionPointer.__CALL ();
		}

			

		[MethodImpl(MethodImplOptions.NoInlining)]
		static public void TESTCALL1()
		{
			Console.WriteLine ("THIS IS TESTCALL1 prev");
			_FuncPtr32 tx = new _FuncPtr32(typeof(Kagimono).GetMethod("TESTCALL1"));
			tx.Array2Image(0xC3);
			tx.Image2Raw();
			_ReJIT.__DROPSPECIAL((int)tx);
			Console.WriteLine ("THIS IS TESTCALL1");
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static public void TESTCALL2()
		{
			TESTCALL1();
			Console.WriteLine ("THIS IS TESTCALL2");
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static public void TESTCALL3()
		{
			TESTCALL2();
			Console.WriteLine ("THIS IS TESTCALL3");
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static public void TESTCALL4()
		{
			TESTCALL3();
			Console.WriteLine ("THIS IS TESTCALL4");
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static public void TESTCALL5()
		{
			Console.WriteLine ("THIS IS TESTCALL5 UNCHAINED");
		}



		[MethodImpl(MethodImplOptions.NoInlining)]
		static public int RETURN_CALLER(int argx)
		{
			Console.WriteLine ("THIS IS RETURN CALLER PRE");
			_ReJIT.__REPLACE
			(
				0x8B, 0x44, 0x24, 0x04, 0xD1, 0xE0 ,0xC3
			);
			Console.WriteLine ("THIS IS RETURN CALLER POST");
			return 0;
		}

		static unsafe public void Main (string[] args)
		{
			Console.WriteLine ("Entry:start");

			Console.WriteLine("ENTRY");
			int test = RETURN_CALLER(100);
			//test = RETURN_CALLER(8080);
			Console.WriteLine(test);


			//Console.WriteLine("ENTRY");
			//TEST_ARGS1(1);
			//Console.WriteLine("EXIT");

			//new _FuncPtr32(typeof(Kagimono).GetMethod("TEST_ARGS1")).WriteLine();



			//TEST_ARGS1(0);


			//ffc.WriteLine();
			//ffc.WriteLine();
			//Console.WriteLine("BEFORE");
			//TEST_ARGS1((int)ffc);
			//Console.WriteLine("AFTER");



			//ffc.WriteLine();



			if (true)
				return;

		//	ffc.__REBASE();


			//Console.WriteLine (_X68.GetOpcodeLenght(0,0x83,0xEC,0x0C,0xFF,0x75,0x08));

			//should be 3  25: 83 ec 0c                sub    esp,0xc
			//EC is top 3 return type

			//if (true)
				//return;

			byte[] data = null; //ffc.Image;

			int offset = 0;
			int tmp = 0;

			int index = 0;
			int bound = 0;

			int iter = 0;

			while(true)
			{
				if (offset >= data.Length)
					break;
				tmp = _X68.GetOpcodeLenght(offset,data);

				index = offset;
				bound = offset+tmp;

				while(index < bound)
				{
					Console.Write(string.Format("{0:X2}",data[index]));
					Console.Write(" ");
					index++;
				}
				Console.WriteLine();
				offset+=tmp;

			}
				


			//true_function_pointer.__REBASE ();


			//Console.WriteLine (_X68.CalculateModRMPayload(0x04,0x05));

			//8B		r						MOV	r16/32	    r/m16/32 //register to memory
			//89		r						MOV	r/m16/32	r16/32   //memory to register



			//_FunctionHandle rebased_function_pointer = true_function_pointer.__REBASE ();

			//true_function_pointer.WriteLine ();
			//rebased_function_pointer.WriteLine ();

			//_FuncPtr32 fx = new _FuncPtr32 (typeof(Kagimono).GetMethod ("TESTCALL"));
			//
			//_FuncPtr32 ff = new _FuncPtr32 (typeof(_FuncPtr32).GetMethod ("__CALL"));

			//mov eax,[esp+4] //we pick reference from stack
			//mov eax,[eax+8] //we modify reference by 8 and read value stored inside
			//call eax
			//ret


			//ff.Array2Image ( 0x8B, 0x44, 0x24, 0x04, 0x8B, 0x40, 0x08, 0xFF, 0xD0, 0xC3 );
			//ff.Image2Raw ();

			//fx.__CALL ();


			//_FuncPtr32 ggh = new _FuncPtr32 (typeof(Kagimono).GetMethod ("TESTCALL"));

			//_FuncPtr32 pvx = ggh.__REBASE ();

			//ggh.__CALL ();

			//ggh.Array2Image (0xc3);

			//ggh.Image2Raw ();

			//ggh.__CALL ();
			//ggh.WriteLine ();
			//pvx.WriteLine ();

			//pvx.Image2Raw ();
			//
			//pvx.__CALL ();




			//fy.WriteLine ();


			//_FunctionHandle ss = new _FunctionHandle ().GetGetMethod());

			//_FunctionHandle ss = new _FunctionHandle (typeof(WildSpawner).GetProperty 
				//("CurrentTotalAnimalWeight",(BindingFlags)60).GetGetMethod (true));

			//ss.SyncArray2Image (0x31, 0xC0, 0xC3);
			//ss.SyncImage2Raw ();


			//TEST_NEW ();
			//TEST_THROW ();

			//_FunctionHandle f1 = new _FunctionHandle (typeof(Kagimono).GetMethod ("TEST_THROW"));
			//_FunctionHandle f2 = new _FunctionHandle (typeof(Kagimono).GetMethod ("TEST_HOOK"));

			//Console.WriteLine ((int)f1);
			//Console.WriteLine ((int)f2);

			//Console.WriteLine ((int)f2-f1);

			//f1.WriteLine ();
			//f2.WriteLine ();

			//byte* fx = f1;
			//fx += 7;

			//select first byte of offset

			//Console.WriteLine ("{0:X2}", fx[0]);

			//int offset = ((int*)fx) [0];
			//int offsetbase = (int)fx;
			//offsetbase += 4;



			//Console.WriteLine (offsetbase + offset);

			//new _FunctionHandle (offset + offsetbase).WriteLine (290);

			//Console.WriteLine ((int)f2);

			//Console.WriteLine ("{0:X2}", foffset);

			//tx.WriteLine ();

			//38 vs 53

			Console.WriteLine ("Entry:complete");
		}
	}
}