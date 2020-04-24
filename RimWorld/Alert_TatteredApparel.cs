using Verse;

namespace RimWorld
{
	public class Alert_TatteredApparel : Alert_Thought
	{
		protected override ThoughtDef Thought => ThoughtDefOf.ApparelDamaged;

		public Alert_TatteredApparel()
		{
			defaultLabel = "AlertTatteredApparel".Translate();
			explanationKey = "AlertTatteredApparelDesc";
		}
	}
}
