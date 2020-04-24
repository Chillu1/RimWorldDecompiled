using System;
using Verse;

namespace RimWorld.SketchGen
{
	public static class SketchGen
	{
		private static bool working;

		public static Sketch Generate(SketchResolverDef root, ResolveParams parms)
		{
			if (working)
			{
				Log.Error("Cannot call Generate() while already generating. Nested calls are not allowed.");
				return parms.sketch;
			}
			working = true;
			try
			{
				root.Resolve(parms);
				return parms.sketch;
			}
			catch (Exception arg)
			{
				Log.Error("Error in SketchGen: " + arg);
				return parms.sketch;
			}
			finally
			{
				working = false;
			}
		}
	}
}
