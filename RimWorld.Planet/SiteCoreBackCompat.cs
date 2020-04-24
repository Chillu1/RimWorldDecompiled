using Verse;

namespace RimWorld.Planet
{
	public class SiteCoreBackCompat : IExposable
	{
		public SitePartDef def;

		public SitePartParams parms;

		public void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			Scribe_Deep.Look(ref parms, "parms");
		}
	}
}
