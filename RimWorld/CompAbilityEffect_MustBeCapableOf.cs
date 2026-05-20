using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_MustBeCapableOf : CompAbilityEffect
	{
		public new CompProperties_AbilityMustBeCapableOf Props => (CompProperties_AbilityMustBeCapableOf)props;

		public override bool GizmoDisabled(out string reason)
		{
			foreach (WorkTags allSelectedItem in Props.workTags.GetAllSelectedItems<WorkTags>())
			{
				if (parent.pawn.WorkTagIsDisabled(allSelectedItem))
				{
					reason = "AbilityDisabled_IncapableOfWorkTag".Translate(parent.pawn.Named("PAWN"), allSelectedItem.LabelTranslated());
					return true;
				}
			}
			reason = null;
			return false;
		}
	}
}
