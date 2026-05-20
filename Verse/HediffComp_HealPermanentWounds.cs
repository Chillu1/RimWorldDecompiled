using System.Linq;
using RimWorld;

namespace Verse;

public class HediffComp_HealPermanentWounds : HediffComp
{
	private int ticksToHeal;

	public HediffCompProperties_HealPermanentWounds Props => (HediffCompProperties_HealPermanentWounds)props;

	public override void CompPostMake()
	{
		base.CompPostMake();
		ResetTicksToHeal();
	}

	private void ResetTicksToHeal()
	{
		ticksToHeal = Rand.Range(15, 30) * 60000;
	}

	public override void CompPostTickInterval(ref float severityAdjustment, int delta)
	{
		ticksToHeal -= delta;
		if (ticksToHeal <= 0)
		{
			TryHealRandomPermanentWound(base.Pawn, parent.LabelCap);
			ResetTicksToHeal();
		}
	}

	public static void TryHealRandomPermanentWound(Pawn pawn, string cause)
	{
		if (pawn.health.hediffSet.hediffs.Where((Hediff hd) => hd.IsPermanent() || hd.def.chronic).TryRandomElement(out var result))
		{
			HealthUtility.Cure(result);
			if (PawnUtility.ShouldSendNotificationAbout(pawn))
			{
				Messages.Message("MessagePermanentWoundHealed".Translate(cause, pawn.LabelShort, result.Label, pawn.Named("PAWN")), pawn, MessageTypeDefOf.PositiveEvent);
			}
		}
	}

	public override void CompExposeData()
	{
		Scribe_Values.Look(ref ticksToHeal, "ticksToHeal", 0);
	}

	public override string CompDebugString()
	{
		return "ticksToHeal: " + ticksToHeal;
	}
}
