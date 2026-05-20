using System.Collections.Generic;

namespace Verse;

public class Gene_Healing : Gene
{
	private int ticksToHeal;

	private static readonly IntRange HealingIntervalTicksRange = new IntRange(900000, 1800000);

	public override void PostAdd()
	{
		base.PostAdd();
		ResetInterval();
	}

	public override void TickInterval(int delta)
	{
		base.TickInterval(delta);
		ticksToHeal -= delta;
		if (ticksToHeal <= 0)
		{
			HediffComp_HealPermanentWounds.TryHealRandomPermanentWound(pawn, LabelCap);
			ResetInterval();
		}
	}

	private void ResetInterval()
	{
		ticksToHeal = HealingIntervalTicksRange.RandomInRange;
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		if (DebugSettings.ShowDevGizmos)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Heal permanent wound",
				action = delegate
				{
					HediffComp_HealPermanentWounds.TryHealRandomPermanentWound(pawn, LabelCap);
					ResetInterval();
				}
			};
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref ticksToHeal, "ticksToHeal", 0);
	}
}
