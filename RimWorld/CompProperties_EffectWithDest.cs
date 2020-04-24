using Verse;

namespace RimWorld
{
	public class CompProperties_EffectWithDest : CompProperties_AbilityEffect
	{
		public AbilityEffectDestination destination;

		public bool requiresLineOfSight;

		public float range;

		public FloatRange randomRange;

		public ClamorDef destClamorType;

		public int destClamorRadius;
	}
}
