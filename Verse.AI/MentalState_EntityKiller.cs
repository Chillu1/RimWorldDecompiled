using RimWorld;

namespace Verse.AI;

public class MentalState_EntityKiller : MentalState_SlaughterThing
{
	private const float ChanceToRecoverAfterKill = 0.5f;

	protected override bool SlaughterTargetAvailable => AnomalyUtility.FindEntityOnPlatform(pawn.Map, EntityQueryType.ForSlaughter) != null;

	protected override int MinTicksBetweenSlaughter => 625;

	protected override int MaxThingsSlaughtered => 5;

	public override void Notify_SlaughteredTarget()
	{
		base.Notify_SlaughteredTarget();
		if (pawn.MentalState == this && Rand.Chance(0.5f))
		{
			RecoverFromState();
		}
	}
}
