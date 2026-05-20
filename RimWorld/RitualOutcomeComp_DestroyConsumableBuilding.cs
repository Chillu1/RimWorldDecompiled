using Verse;

namespace RimWorld
{
	public class RitualOutcomeComp_DestroyConsumableBuilding : RitualOutcomeComp
	{
		protected virtual string LabelForDesc => label.CapitalizeFirst();

		public override bool Applies(LordJob_Ritual ritual)
		{
			return ritual.selectedTarget.HasThing;
		}
	}
}
