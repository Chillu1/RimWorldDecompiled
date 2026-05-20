namespace Verse;

public class HediffComp_DisappearsAndKills : HediffComp_DisappearsDisableable
{
	public override void CompPostPostRemoved()
	{
		base.CompPostPostRemoved();
		if (!disabled && ticksToDisappear <= 0 && !base.Pawn.Dead)
		{
			base.Pawn.Kill(null, null);
			parent.Notify_PawnDied(null);
		}
	}

	public override void CompExposeData()
	{
		base.CompExposeData();
		Scribe_Values.Look(ref disabled, "disabled", defaultValue: false);
	}
}
