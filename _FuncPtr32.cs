using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Text;

namespace rcrml
{

	//this is function pointer class (not struct) for 32 bit platform

	[StructLayout(LayoutKind.Sequential)]
	unsafe public class _FuncPtr32
	{
		//system rely on "first" word in class only
		//zero word is vtable
		//first word is syncblock
		//this is third word actually, mod 8\16
		private void* funcptr_alligned;
		private void* funcptr_provided;

		private int 		malloc_size;
		private int 		stream_offset;
		private byte[] 		function_image;	   
		private byte[][]    image_backup_stack;

		private MethodBase 	function_vm_object;

		//void* can fit 64 pointer on 64bit systems
		static private void* RWX_MEM;
		static private int   RWX_TOP;

		static _FuncPtr32()
		{
			RWX_MEM = _Native.mono_code_manager_reserve (_Native.mono_code_manager_new (), 1024 * 1024);
			RWX_TOP = 0;
		}

		//used to construct fully unmanaged functions
		//called when constructor provides non zero size and zero pointer
		public void Rebase(int _size)
		{
			long ret = (long)RWX_MEM + RWX_TOP;
			RWX_TOP += _size;

			funcptr_alligned = (void*)((long)RWX_MEM + RWX_TOP);
			funcptr_provided = (void*)((long)RWX_MEM + RWX_TOP);
			malloc_size = _size;
		}

		//semi constructors:

		//basic constructor
		public _FuncPtr32(void* _pointer)
		{
			CTOR (_pointer,0);
		}

		//basic constructor with unmanaged support
		public _FuncPtr32(void* _pointer,int _size)
		{
			CTOR (_pointer,_size);
		}

		public _FuncPtr32(MethodInfo _method_info)
		{
			CTOR ((void*)_method_info.MethodHandle.GetFunctionPointer (),0);
		}

		public _FuncPtr32(RuntimeMethodHandle _method_info_native)
		{
			CTOR ((void*)_method_info_native.GetFunctionPointer (),0);
		}
			
		//no platform word size checks here
		public _FuncPtr32(long _unsafe_long)
		{
			CTOR ((void*)_unsafe_long,0);
		}

		//unsafe with unmanaged support
		public _FuncPtr32(int _unsafe_long,int _size)
		{
			CTOR ((void*)_unsafe_long,_size);
		}

		public _FuncPtr32(int _unsafe_int)
		{
			CTOR ((void*)_unsafe_int,0);
		}
			

		//actual constructor that handle actual logic
		private void CTOR(void* _pointer, int _size)
		{
			if (_pointer == (void*)0 && _size == 0)
				return; //both value are zero, nothing to do here

			if (_pointer == (void*)0 && _size != 0)
			{
				//rebase case
				Rebase(_size); //rebase reset all pointers, does not reset existing image if any
				//right after construction rebased methods contains trash and unusable
				return;
			}

			funcptr_provided = _pointer;

			if (_size != 0) //pointer nonzero, still size is provided, unamanged case
			{
				//both pointers set to same value, we can't calculate method start
				funcptr_alligned = _pointer;
				malloc_size = _size;
				return;
			}
				
			//managed case

			_Native._MonoJitInfo* mji = 
				_Native.mono_jit_info_table_find (_Native.mono_domain_get (),_pointer);
			
			if (mji == null) //managed case failure
				return; //pointer does not belong to managed method

			malloc_size = Math.Max (mji->code_size, sizeof(void*) * 2);
			funcptr_alligned = (void*)mji->code_start;
			function_vm_object = MethodBase.GetMethodFromHandle (mji->method);
		}
			
		//read and write methods are follow

		private void Image2Backup()
		{
			if (image_backup_stack == null)
				image_backup_stack = new byte[8][];

			Array.Copy (image_backup_stack, 0, image_backup_stack, 1, 7);
			image_backup_stack [0] = function_image;
		}

		//restore image from backup, used to restore original method code
		public byte[] Backup2Image()
		{
			if (image_backup_stack == null)
				return null;
			if (image_backup_stack[0] == null)
				return null;

			byte[] shadow = function_image;

			function_image = image_backup_stack [0];
			Array.Copy (image_backup_stack, 1, image_backup_stack, 0, 7);
			return shadow;
		}
			
		//read unmanaged memory into fresh byte[]
		//always push value of function_image to backup stack, ever on fresh objects
		[MethodImpl(MethodImplOptions.NoInlining)]
		public void Raw2Image(int _size_override)
		{
			if (malloc_size <= 0 || funcptr_alligned <= (void*)0) //it is not possible to read zero bytes or from zero adress
				return;
			//no more exceptions currently, just keep null inside function_image
			//throw new Exception ("Pointer " + string.Format("{0:X4}",(long)funcptr_provided) + " does not belong to managed method");

			int size = _size_override==0 ? malloc_size :_size_override;

			byte[]  newarray = new byte[size];
			Marshal.Copy ((IntPtr)funcptr_alligned, newarray, 0, size);
			Image2Backup ();
			function_image = newarray;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public void Raw2Image()
		{
			Raw2Image(0);
		}

		//Emit data stored inside image into unmanaged memory
		//minor safety checks
		[MethodImpl(MethodImplOptions.NoInlining)]
		public void Image2Raw()
		{
			if (function_image == null) //no operation, no warning
				return;
			if (funcptr_alligned == (void*)0 || malloc_size == 0)
				throw new Exception ("Pointer " + string.Format("{0:X4}",(long)funcptr_provided) + " does not belong to managed method");
			if (function_image.Length > malloc_size)
				throw new Exception ("Provided array cannot fit, probably you want to rebase.");
			Marshal.Copy (function_image, 0, (IntPtr)funcptr_alligned, function_image.Length);
		}

			
		//replace existing array with new one, params syntax sugar version, no safety
		[MethodImpl(MethodImplOptions.NoInlining)]
		public void Array2Image(params byte[] _array)
		{
			//system do not expose references to function_image
			//and do not accept references from outside
			byte[] tmp = new byte[_array.Length];
			Array.Copy(_array,tmp,_array.Length);
			Image2Backup ();
			function_image = tmp;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public void Stream2Image()
		{
			if (malloc_size == 0)
				throw new Exception ("Zero stream");
			Stream2Image(malloc_size);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public void Stream2Image(int _size)
		{
			stream_offset = 0;
			byte[] newarray = new byte[_size];
			Image2Backup ();
			function_image = newarray;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public void Stream(__arglist)
		{
			ArgIterator iter = new ArgIterator (__arglist);
			TypedReference curref;
			while (iter.GetRemainingCount () != 0) 
			{
				curref = iter.GetNextArg ();
				Type t = __reftype(curref);


				if (t == typeof(Byte)) 
				{
					byte _data = __refvalue(curref,Byte);
					function_image [stream_offset++] = _data;
				}

				if (t == typeof(Int16)) 
				{
					short _data = __refvalue(curref,Int16);
					byte* iab = (byte*)&_data;
					function_image [stream_offset++] = iab [0];
					function_image [stream_offset++] = iab [1];
				}

				if (t == typeof(Int32)) 
				{
					int _data = __refvalue(curref,Int32);
					byte* iab = (byte*)&_data;
					function_image [stream_offset++] = iab [0];
					function_image [stream_offset++] = iab [1];
					function_image [stream_offset++] = iab [2];
					function_image [stream_offset++] = iab [3];
				}

				if (t == typeof(Int64)) 
				{
					long _data = __refvalue(curref,Int64);
					byte* iab = (byte*)&_data;
					function_image [stream_offset++] = iab [0];
					function_image [stream_offset++] = iab [1];
					function_image [stream_offset++] = iab [2];
					function_image [stream_offset++] = iab [3];
					function_image [stream_offset++] = iab [4];
					function_image [stream_offset++] = iab [5];
					function_image [stream_offset++] = iab [6];
					function_image [stream_offset++] = iab [7];
				}
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public void Stream(byte _data)
		{
			function_image [stream_offset++] = _data;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public void Stream(int _data)
		{
			byte* iab = (byte*)&_data;
			function_image [stream_offset++] = iab [0];
			function_image [stream_offset++] = iab [1];
			function_image [stream_offset++] = iab [2];
			function_image [stream_offset++] = iab [3];
		}
			

		public void WriteLine()
		{
			Console.WriteLine (OPSTRING (false,0));
		}

		public void WriteLine(bool b)
		{
			Console.WriteLine (OPSTRING (true,0));
		}

		public void WriteLine(int _size_override)
		{
			Console.WriteLine (OPSTRING (false,_size_override));
		}

		public string OPSTRING(bool brief,int _size_override)
		{
			if (function_image == null) //probably this is fresh function
				Raw2Image (_size_override);
			if (function_image == null)
				return "FuncPtr32 " + string.Format("{0:X4}",(long)funcptr_provided) + " is invalid.";
			
			StringBuilder sb = new StringBuilder ();

			//name and signature of method or unmanaged+adress
			string mb = ReflectionHandle!=null ? ReflectionHandle.ToString () : "unmanaged:" + 
				string.Format("{0:X4}",(long)funcptr_alligned);

			//for stackwalking
			long delta = (long)funcptr_provided - (long)funcptr_alligned;

			sb.Append ("IP BEGIN " + mb + "+" + string.Format("{0:X4}",delta) + 
				" ( "+String.Format("{0:X4}", (long)funcptr_alligned)+" )");

			if (brief)
				return sb.ToString();

			int i = 0;

			foreach (byte b in function_image) 
			{
				if (i++ % 4 == 0)
					sb.Append ("\n" + String.Format("{0:X2}", b));
				else
					sb.Append (" " + String.Format("{0:X2}", b));
			}

			sb.Append ("\nLenght " + malloc_size);
			sb.Append ("\nIP END " + mb +" ( "+String.Format("{0:X2}", (long)funcptr_alligned)+" )");

			return sb.ToString();
		}

		public override string ToString()
		{
			return OPSTRING(false,0);
		}

		public static implicit operator void * (_FuncPtr32 _self)
		{
			return _self.funcptr_alligned;
		}

		public static implicit operator byte * (_FuncPtr32 _self)
		{
			return (byte*)_self.funcptr_alligned;
		}

		public static implicit operator int (_FuncPtr32 _self)
		{
			return (int)_self.funcptr_alligned;
		}

		public static implicit operator long (_FuncPtr32 _self)
		{
			return (long)_self.funcptr_alligned;
		}
			
		public byte[] Image
		{
			get 
			{
				byte[] shadow = new byte[function_image.Length];
				Array.Copy (function_image, shadow, function_image.Length);
				return shadow;
			}
		}

		public byte this[int _offset]
		{
			get
			{
				return function_image[_offset];
			}
		}

		public MethodBase ReflectionHandle
		{
			get 
			{
				return function_vm_object;
			}
		}

		public int Size
		{
			get 
			{
				return malloc_size;
			}
		}
			
	//	public _FuncPtr32 __REBASE()
	//	{
			//Raw2Image ();
			//_FuncPtr32 rebased = new _FuncPtr32 (ReserveRWX (malloc_size),malloc_size);

			//int delta = (int)funcptr_alligned - (int)rebased;

			//Console.WriteLine ("delta is " + delta);

			//rebased.function_image = function_image;

			//Console.WriteLine ("array lenght is " + rebased.function_image.Length);

			//_X68.Update__CALLR (delta, rebased.function_image);

			//return rebased;
	//	}

		public IntPtr __CALL()
		{
			//new _ReJIT (0x8B, 0x44, 0x24, 0x04, 0x8B, 0x40, 0x08, 0xFF, 0xD0, 0xC3);
			return new IntPtr (0);
		}
			
	}
}