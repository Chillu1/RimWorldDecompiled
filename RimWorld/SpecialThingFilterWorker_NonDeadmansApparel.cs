using Verse;

namespace RimWorld
{
	public class SpecialThingFilterWorker_NonDeadmansApparel : SpecialThingFilterWorker
	{
		public override bool Matches(Thing t)
		{
			Apparel apparel = t as Apparel;
			if (apparel != null)
			{
				return !apparel.WornByCorpse;
			}
			return false;
		}

		public override bool CanEverMatch(ThingDef def)
		{
			if (def.IsApparel)
			{
				return def.apparel.careIfWornByCorpse;
			}
			return false;
		}
	}
}
