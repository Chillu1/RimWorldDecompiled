using System;
using Verse;

namespace RimWorld.SketchGen;

public abstract class SketchResolver
{
	public void Resolve(SketchResolveParams parms)
	{
		try
		{
			ResolveInt(parms);
		}
		catch (Exception ex)
		{
			Log.Error("Exception resolving " + GetType().Name + ": " + ex?.ToString() + "\n\nParms:\n" + parms.ToString());
		}
	}

	public bool CanResolve(SketchResolveParams parms)
	{
		try
		{
			return CanResolveInt(parms);
		}
		catch (Exception ex)
		{
			Log.Error("Exception test running " + GetType().Name + ": " + ex?.ToString() + "\n\nParms:\n" + parms.ToString());
			return false;
		}
	}

	protected abstract void ResolveInt(SketchResolveParams parms);

	protected abstract bool CanResolveInt(SketchResolveParams parms);
}
