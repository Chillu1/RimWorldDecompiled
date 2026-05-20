using Verse;

namespace RimWorld;

public class CompAbilityEffect_DeactivateMechanoid : CompAbilityEffect
{
	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		Pawn pawn = target.Pawn;
		if (pawn != null)
		{
			pawn.GetComp<CompMechanoid>()?.Deactivate();
			Effecter effecter = EffecterDefOf.MechDisabled.Spawn();
			effecter.Trigger(new TargetInfo(target.Cell, parent.pawn.MapHeld), new TargetInfo(target.Cell, parent.pawn.MapHeld));
			effecter.Cleanup();
		}
	}

	public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
	{
		return Valid(target);
	}

	public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
	{
		Pawn pawn = target.Pawn;
		if (pawn == null)
		{
			return false;
		}
		if (!pawn.RaceProps.IsMechanoid)
		{
			return false;
		}
		if (!pawn.HostileTo(Faction.OfPlayer))
		{
			return false;
		}
		return base.Valid(target, throwMessages);
	}
}
