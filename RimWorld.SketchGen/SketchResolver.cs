using System;
using Verse;

namespace RimWorld.SketchGen
{
	public abstract class SketchResolver
	{
		public void Resolve(ResolveParams parms)
		{
			try
			{
				ResolveInt(parms);
			}
			catch (Exception ex)
			{
				Log.Error("Exception resolving " + GetType().Name + ": " + ex + "\n\nParms:\n" + parms.ToString());
			}
		}

		public bool CanResolve(ResolveParams parms)
		{
			try
			{
				return CanResolveInt(parms);
			}
			catch (Exception ex)
			{
				Log.Error("Exception test running " + GetType().Name + ": " + ex + "\n\nParms:\n" + parms.ToString());
				return false;
			}
		}

		protected abstract void ResolveInt(ResolveParams parms);

		protected abstract bool CanResolveInt(ResolveParams parms);
	}
}
