using Verse;

namespace RimWorld;

public class HediffComp_DestroyOrgan : HediffComp
{
	public HediffCompProperties_DestroyOrgan Props => (HediffCompProperties_DestroyOrgan)props;

	public override void CompPostTickInterval(ref float severityAdjustment, int delta)
	{
		if (parent.Severity >= base.Def.maxSeverity)
		{
			DoDecayEffect();
		}
	}

	public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
	{
		if (culprit != null && culprit.def == HediffDefOf.OrganDecay && culprit.Part == parent.Part)
		{
			DoDecayEffect();
		}
	}

	private void DoDecayEffect()
	{
		base.Pawn.health.RemoveHediff(parent);
		if (base.Pawn.Dead)
		{
			base.Pawn.health.AddHediff(HediffDefOf.Decayed, parent.Part);
		}
		else
		{
			DamageInfo dinfo = new DamageInfo(Props.damageType ?? DamageDefOf.Rotting, 99999f, 999f, -1f, null, parent.Part);
			dinfo.SetAllowDamagePropagation(val: false);
			base.Pawn.TakeDamage(dinfo);
		}
		if (base.Pawn.Faction == Faction.OfPlayer && !string.IsNullOrEmpty(Props.messageText))
		{
			Messages.Message(Props.messageText.Formatted(base.Pawn.Named("PAWN"), parent.Part.Label), base.Pawn, MessageTypeDefOf.NegativeEvent);
		}
	}
}
