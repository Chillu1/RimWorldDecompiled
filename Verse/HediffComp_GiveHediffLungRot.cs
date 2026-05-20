using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Verse;

public class HediffComp_GiveHediffLungRot : HediffComp
{
	private HediffCompProperties_GiveHediffLungRot Props => (HediffCompProperties_GiveHediffLungRot)props;

	public override void CompPostPostAdd(DamageInfo? dinfo)
	{
	}

	public override void CompPostTickInterval(ref float severityAdjustment, int delta)
	{
		Pawn pawn = parent.pawn;
		if (!pawn.Spawned || !(parent.Severity >= Props.minSeverity) || !pawn.Position.AnyGas(pawn.Map, GasType.RotStink) || pawn.health.hediffSet.HasHediff(HediffDefOf.LungRot) || !pawn.IsHashIntervalTick(Props.mtbCheckDuration, delta) || pawn.health.immunity.AnyGeneMakesFullyImmuneTo(HediffDefOf.LungRot) || !Rand.MTBEventOccurs(Props.mtbOverRotGasExposureCurve.Evaluate(parent.Severity), 2500f, Props.mtbCheckDuration))
		{
			return;
		}
		IEnumerable<BodyPartRecord> lungRotAffectedBodyParts = GasUtility.GetLungRotAffectedBodyParts(pawn);
		if (!lungRotAffectedBodyParts.Any())
		{
			return;
		}
		foreach (BodyPartRecord item in lungRotAffectedBodyParts)
		{
			pawn.health.AddHediff(HediffDefOf.LungRot, item);
		}
		if (PawnUtility.ShouldSendNotificationAbout(pawn))
		{
			TaggedString label = "LetterLabelNewDisease".Translate() + ": " + HediffDefOf.LungRot.LabelCap;
			TaggedString text = "LetterTextLungRot".Translate(pawn.Named("PAWN"), HediffDefOf.LungRot.label, pawn.LabelDefinite()).AdjustedFor(pawn).CapitalizeFirst();
			Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NegativeEvent, pawn);
		}
	}
}
