using Verse;

namespace RimWorld;

public class CompAncientToxVent : CompAncientVent
{
	protected override bool AppliesEffectsToPawns => true;

	protected override void ToggleIndividualVent(bool on)
	{
		parent.GetComp<CompFleckEmitterLongTerm>().Enabled = on;
	}

	protected override void ApplyPawnEffect(Pawn pawn, int delta)
	{
		ToxicUtility.DoPawnToxicDamage(pawn, 0.0028977108f * (float)delta);
	}
}
