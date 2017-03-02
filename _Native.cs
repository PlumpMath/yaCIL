using System;
using System.Runtime.InteropServices;

namespace yacil
{
	public unsafe class _Native
	{
		static public PLATFORM CURRENT_PLATFORM;
		public enum PLATFORM
		{
			EXT,W32,M64,L64
		}
			

		//mono_domain_get

		[DllImport("__Internal",
			EntryPoint="mono_domain_get")]
		extern static private void* mono_domain_get__EXT ();
		[DllImport("mono.dll",
			EntryPoint="mono_domain_get")]
		extern static private void* mono_domain_get__W32 ();
		[DllImport("libmono.so",
			EntryPoint="mono_domain_get")]
		extern static private void* mono_domain_get__M64 ();
		[DllImport("libmono.so",
			EntryPoint="mono_domain_get")]
		extern static private void* mono_domain_get__L64 ();

		//mono_jit_info_table_find

		//void* equal to "native integer", it follows word size based on platform used automatically
		public struct _MonoJitInfo
		{
			//should work on x64 due to variable size of void*
			public RuntimeMethodHandle method; //should work properly because IntPtr host void* intenrnally
			public void* 	next_jit_code_hash;
			public IntPtr 	code_start; //same void* under wrappers
			public uint  	unwind_info;
			public int   	code_size;
			public void* 	__rest_is_omitted;
			//struct is longer actually, but rest of fields does not matter
		}

		[DllImport("__Internal",
			EntryPoint="mono_jit_info_table_find")]
		extern static private _MonoJitInfo* mono_jit_info_table_find__EXT (void* domain,void* function);
		[DllImport("mono.dll",
			EntryPoint="mono_jit_info_table_find")]
		extern static private _MonoJitInfo* mono_jit_info_table_find__W32 (void* domain,void* function);
		[DllImport("libmono.so",
			EntryPoint="mono_jit_info_table_find")]
		extern static private _MonoJitInfo* mono_jit_info_table_find__M64 (void* domain,void* function);
		[DllImport("libmono.so",
			EntryPoint="mono_jit_info_table_find")]
		extern static private _MonoJitInfo* mono_jit_info_table_find__L64 (void* domain,void* function);

		//mono_code_manager_new

		[DllImport("__Internal",
			EntryPoint="mono_code_manager_new")]
		extern static private void* mono_code_manager_new__EXT ();
		[DllImport("mono.dll",
			EntryPoint="mono_code_manager_new")]
		extern static private void* mono_code_manager_new__W32 ();
		[DllImport("libmono.so",
			EntryPoint="mono_code_manager_new")]
		extern static private void* mono_code_manager_new__M64 ();
		[DllImport("libmono.so",
			EntryPoint="mono_code_manager_new")]
		extern static private void* mono_code_manager_new__L64 ();

		//mono_code_manager_reserve

		[DllImport("__Internal",
			EntryPoint="mono_code_manager_reserve")]
		extern static private void* mono_code_manager_reserve__EXT (void* MonoCodeManager, int size);
		[DllImport("mono.dll",
			EntryPoint="mono_code_manager_reserve")]
		extern static private void* mono_code_manager_reserve__W32 (void* MonoCodeManager, int size);
		[DllImport("libmono.so",
			EntryPoint="mono_code_manager_reserve")]
		extern static private void* mono_code_manager_reserve__M64 (void* MonoCodeManager, int size);
		[DllImport("libmono.so",
			EntryPoint="mono_code_manager_reserve")]
		extern static private void* mono_code_manager_reserve__L64 (void* MonoCodeManager, int size);


		static public void* mono_code_manager_reserve (void* MonoCodeManager, int size)
		{
			if (CURRENT_PLATFORM == PLATFORM.EXT)
				return mono_code_manager_reserve__EXT(MonoCodeManager, size);

			return mono_code_manager_reserve__W32(MonoCodeManager, size);
		}

		static public void* mono_code_manager_new ()
		{
			if (CURRENT_PLATFORM == PLATFORM.EXT)
				return mono_code_manager_new__EXT();

			return mono_code_manager_new__W32();
		}

		static public void* mono_domain_get ()
		{
			if (CURRENT_PLATFORM == PLATFORM.EXT)
				return mono_domain_get__EXT();
			
			return mono_domain_get__W32();
		}

		static public _MonoJitInfo* mono_jit_info_table_find (void* domain,void* function)
		{
			if (CURRENT_PLATFORM == PLATFORM.EXT)
				return mono_jit_info_table_find__EXT(domain,function);

			return mono_jit_info_table_find__W32(domain,function);
		}

		static _Native()
		{
			string s = Environment.CommandLine;
			//last char of command line string
			s = s.Substring (s.Length - 1);

			switch (s) 
			{
			case "e": //exE
				CURRENT_PLATFORM = PLATFORM.W32;
				return;
			case "l": //dlL
				CURRENT_PLATFORM = PLATFORM.EXT;
				return;

				//C for mac
				//4 for linux
			}
		}
	}
}