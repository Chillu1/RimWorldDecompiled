using Verse;

namespace RimWorld
{
	public class CompProperties_Targetable : CompProperties_UseEffect
	{
		public bool psychicSensitiveTargetsOnly;

		public bool fleshCorpsesOnly;

		public bool nonDessicatedCorpsesOnly;

		public bool nonDownedPawnOnly;

		public bool ignoreQuestLodgerPawns;

		public bool ignorePlayerFactionPawns;

		public ThingDef moteOnTarget;

		public ThingDef moteConnecting;

		public CompProperties_Targetable()
		{
			compClass = typeof(CompTargetable);
		}
	}
}
