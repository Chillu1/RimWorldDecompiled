using Verse;

namespace RimWorld
{
	public class CompCauseHediff_Apparel : ThingComp
	{
		private CompProperties_CauseHediff_Apparel Props => (CompProperties_CauseHediff_Apparel)props;

		public override void Notify_Equipped(Pawn pawn)
		{
			if (pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff) == null)
			{
				HediffComp_RemoveIfApparelDropped hediffComp_RemoveIfApparelDropped = pawn.health.AddHediff(Props.hediff, pawn.health.hediffSet.GetNotMissingParts().FirstOrFallback((BodyPartRecord p) => p.def == Props.part)).TryGetComp<HediffComp_RemoveIfApparelDropped>();
				if (hediffComp_RemoveIfApparelDropped != null)
				{
					hediffComp_RemoveIfApparelDropped.wornApparel = (Apparel)parent;
				}
			}
		}
	}
}
