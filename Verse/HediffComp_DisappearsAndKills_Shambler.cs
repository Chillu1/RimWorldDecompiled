using RimWorld;

namespace Verse;

public class HediffComp_DisappearsAndKills_Shambler : HediffComp_DisappearsAndKills
{
	private CompHoldingPlatformTarget compHoldingPlatformTarget => base.Pawn.TryGetComp<CompHoldingPlatformTarget>();

	protected override bool Paused
	{
		get
		{
			if (!disabled)
			{
				return compHoldingPlatformTarget?.CurrentlyHeldOnPlatform ?? false;
			}
			return true;
		}
	}

	public override void CompPostPostRemoved()
	{
		base.CompPostPostRemoved();
		if (!disabled && !base.Pawn.Dead)
		{
			base.Pawn.Kill(null, null);
		}
	}
}
