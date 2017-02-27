using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Diagnostics;
using System.Reflection;

namespace rcrml
{
	unsafe public class CodeManipulationUtils
	{
		/**
		 * 
		 * NATIVE BINDING SECTION
		 * 
		 **/

		//this method will allocate new codemanager
		//you now allowed to access global codemanager with mono.dll shipped with game*
		//*it's possible to access it anyway, but there are no reasons to do it
		[DllImport("__Internal")]
		static extern private unsafe void* mono_code_manager_new();

		//this method will allocate memory with RWX permissions
		//since you have exclusive access to that memory, no reasons to follow commit procedure
		[DllImport("__Internal")]
		static extern private unsafe void* mono_code_manager_reserve(void* MonoCodeManager, int size);
			
		/**
			*/
		/**
		 * 
		 * ASM METHODS SECTION
		 * 
		 **/

		//ASM methods are fun!
		//1) no safety checks here, make sure that your code is valid
		//2) make sure that your code will fit, allocation from inside of method is suffice
		//3) if you allocate by reference, add few throw declarations
		//4) "noinline" declaration is mandatory
		//5) do not try to call __rejit by any type of gate, such feat will result in runtime crash


		[MethodImpl(MethodImplOptions.NoInlining)]
		static public unsafe void* __rejit(params byte[] code)
		{
			StackTrace st = new StackTrace ();
			MethodBase calle = st.GetFrames () [1].GetMethod ();


			return (void*) 0;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static public unsafe void __dropframe()
		{

		}
			

		//get pointer from managed type
		[MethodImpl(MethodImplOptions.NoInlining)]
		static public unsafe void* __ptr(object o)
		{
			return __rejit ( 0x8B, 0x44, 0x24, 0x04, 0xC3 );
		}
			/**
		static public byte[] MethodWrite(Type t, string s,params int[] code)
		{
			MethodInfo mi = t.GetMethod (s, (BindingFlags)60);
			if (mi == null)
				return null;
			void* fpraw = mi.MethodHandle.GetFunctionPointer().ToPointer();
			void* jitinforaw = mono_jit_info_table_find (mono_domain_get (), fpraw);
			u_MonoJitInfo jitinfo = *(u_MonoJitInfo*)jitinforaw;
			int size = jitinfo.code_size;

			int i = 0;
			byte* walk = (byte*)fpraw;
			while (i < code.Length && i < size) 
			{
				if (code [i] != -1)
					walk [i] = (byte)code [i];
				i++;
			}
			return null;
		}
*/





		//this struct is used to read current position of object on stack
		//mono GC expected to move objects around without any kind warning
		//for this reason, DO NOT store instances of this struct on heap by any means
		//mono GC expected to crash entire runtime on hitting invalid object reference on GC round
		//results of position read expected to turn into junk on method return andor GC rounds
		[StructLayout(LayoutKind.Explicit,Size=32)]
		public unsafe struct reinterpret_cast_struct_32
		{
			[FieldOffset(0)] public object target;
			[FieldOffset(0)] public void* pointer;
			//you may add here any kind of object with same offset
			//in case of valuetypes, make sure that struct size can fit your valuetype, 
			//in other case you will corrupt stack (and if you try to reference it, you will damage heap)
		}
			
		static private void* u_MonoCodeManager;
		static private void* u_RWXM;

		static CodeManipulationUtils()
		{
			u_MonoCodeManager = mono_code_manager_new ();
			u_RWXM = mono_code_manager_reserve (u_MonoCodeManager, sizeof(void*)*256);


			//int[] dataxx = new int[]{ 0x8B, 0x44, 0x24, 0x06, 0xC3, 0xE0 };
			//int[] dataxx = new int[]{ 0x8B, 0x44, 0x24, 0x04, 0x90, 0xFF, 0xE0 };
			//int[] jmp32payload = new int[]{ 0x8B, 0x44, 0x24, 0x04, 0x90, 0xFF, 0xE0 };
			//push ret combination
			//pop ret combination to skip frame

			//try self modification via two methods and stack hijack
			//need info about return and call operators
			//need info about original code layout

			Console.WriteLine ("RWX memory allocation " + (int)u_RWXM);
		}


		//ease of use bits, it's possible to allocate delegate object by other means
		delegate int __jmp_delegate();

		//method to allocate delegate, never actually called
		[MethodImpl(MethodImplOptions.NoInlining)]
		static public int __jmp_dummy_method()
		{
			throw new Exception ("Jump failure");
			return 0;
		}

		//this method explained in forum post, line by line
		[MethodImpl(MethodImplOptions.NoInlining)]
		static public int __jmp31(void* _dest)
		{
			__jmp_delegate callsite = __jmp_dummy_method;
			reinterpret_cast_struct_32 rcs = new reinterpret_cast_struct_32 ();
			rcs.target = callsite;
			((int*)rcs.pointer) [3] = (int)_dest;
			return callsite ();
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static public void __jmp32_init()
		{

			int[] payload = new int[]{ 0x8B, 0x44, 0x24, 0x04, 0x90, 0xFF, 0xE0 };

		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static public int __jmp32(void* _dest)
		{
			//modification of method that execute now is complicated task
			//for this reason, we will use "double jit" technique
			//first we will modify target method __jmp32_init()


			__jmp32_init ();
			throw new Exception ("__jmp32 init failure");
			return (int)_dest;
		}

		//load bytearray into RWX memory and call into it
		[MethodImpl(MethodImplOptions.NoInlining)]
		static public int __asm32(params byte[] x8632)
		{
			Marshal.Copy (x8632, 0, new IntPtr (u_RWXM), x8632.Length);
			return __jmp31 (u_RWXM);
		}
	}
}