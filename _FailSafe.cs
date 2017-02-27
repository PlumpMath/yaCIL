using System;
using Verse;
using System.Threading;
using RimWorld;
using System.Reflection;

namespace rcrml
{
	[StaticConstructorOnStartup]
	public class _FailSafe
	{
		//this is special service class\type that does nothing by itself
		//instead, it reload "failed" assemblies after first loading cycle is completed
		//it's done by performing advanced (midfunction) code injection

		//codeinjection done via altering return pointer 


		//let all mods load and finalize, then jump into our own method
		//original return poin must be saved somehow (why somehow, just resetting ESP will work fine)



		//LocalDataStoreSlot tx = Thread.AllocateNamedDataSlot ("_FailSafeRegistry");

		//this class expected to run in singletron manner and support multiple instances
		//this done by allocating shared (global) data on main thread
		//there are some complications


		//this is one and only version and you are expected to never recompile it
		//to keep same hash

		//many mods may have this DLL inside, but game will run one one of them
		//rest will be loaded, but won't actually load, runtime will return reference to existing
		//assembly object

		//if assembly is recompiled, push it's version
		//if multiple different assemblies exists, one with higher version will run

		static private int __VERSION = 1;

		static public void GlobalMutexCreateOrRegister()
		{
			Console.WriteLine("before");
			//will create dataslot automatically
			LocalDataStoreSlot NDS = Thread.GetNamedDataSlot ("__FailSafeNDS");
			Console.WriteLine("after");

			object[] existing = (object[])Thread.GetData (NDS);

			if (existing == null) 
			{
				existing = new object[3];
				Thread.SetData (NDS, existing);
				existing [0] = __VERSION;
				existing [1] = "precalculated offset for frame injection";
				return;
			}
				
			//we are not first and already registered instance have greater version
			if ((Int32)existing [0] > __VERSION)
				return;

			existing [0] = __VERSION;
			existing [1] = "precalculated offset for frame injection";
		}

		static public void GATE()
		{
			//gate expected to never change, any gate from any failsafe instance should be the same
			//
			LocalDataStoreSlot NDS = Thread.GetNamedDataSlot ("__FailSafeNDS");
			object[] existing = (object[])Thread.GetData (NDS);
			((MethodInfo)existing [1]).Invoke (null, null);
		}

		static public void RESOLVER()
		{
			Console.WriteLine ("Do things in singular manner");

		}

		static _FailSafe ()
		{
			GlobalMutexCreateOrRegister ();
			Console.WriteLine (Environment.StackTrace);
		}
	}
}