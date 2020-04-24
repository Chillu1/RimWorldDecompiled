using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_Stun : CompAbilityEffect_WithDuration
	{
		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			if (target.HasThing)
			{
				base.Apply(target, dest);
				Pawn pawn = target.Thing as Pawn;
				pawn?.stances.stunner.StunFor(GetDurationSeconds(pawn).SecondsToTicks(), parent.pawn, addBattleLog: false);
			}
		}
	}
}
