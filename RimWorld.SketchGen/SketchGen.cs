using System;
using Verse;

namespace RimWorld.SketchGen;

public static class SketchGen
{
	private static bool working;

	public static Sketch Generate(SketchResolverDef root, SketchResolveParams parms)
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
		catch (Exception ex)
		{
			Log.Error("Error in SketchGen: " + ex);
			return parms.sketch;
		}
		finally
		{
			working = false;
		}
	}
}
