using RimWorld;

namespace Verse
{
	public class HediffComp_RemoveIfApparelDropped : HediffComp
	{
		public Apparel wornApparel;

		public HediffCompProperties_RemoveIfApparelDropped Props => (HediffCompProperties_RemoveIfApparelDropped)props;

		public override bool CompShouldRemove => !parent.pawn.apparel.Wearing(wornApparel);

		public override void CompExposeData()
		{
			base.CompExposeData();
			Scribe_References.Look(ref wornApparel, "wornApparel");
		}
	}
}
