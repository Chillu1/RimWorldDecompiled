using Verse;

namespace RimWorld
{
	public class CompAbility_RequiresCapacity : AbilityComp
	{
		public CompProperties_AbilityRequiresCapacity Props => (CompProperties_AbilityRequiresCapacity)props;

		public override bool GizmoDisabled(out string reason)
		{
			if (!parent.pawn.health.capacities.CapableOf(Props.capacity))
			{
				reason = "AbilityDisabledNoCapacity".Translate(parent.pawn, Props.capacity.label);
				return true;
			}
			reason = null;
			return false;
		}
	}
}
