using RimWorld;

namespace Verse;

public class DamageWorker_Nerve : DamageWorker_AddInjury
{
	private const float StunDamageFactor_Awful = 0.75f;

	private const float StunDamageFactor_Default = 1f;

	private const float StunDamageFactor_Masterwork = 1.25f;

	private const float StunDamageFactor_Legendary = 1.5f;

	private static readonly SimpleCurve NerveStunSecondsFromBodySizeCurve = new SimpleCurve
	{
		new CurvePoint(0.01f, 3f),
		new CurvePoint(1f, 2f),
		new CurvePoint(2f, 2f),
		new CurvePoint(4f, 1f)
	};

	protected override void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, DamageResult result)
	{
		base.ApplySpecialEffectsToPart(pawn, totalDamage, dinfo, result);
		if (!pawn.RaceProps.IsMechanoid && !pawn.Dead && result.totalDamageDealt > 0f)
		{
			DamageInfo dinfo2 = dinfo;
			dinfo2.Def = DamageDefOf.NerveStun;
			float amount = NerveStunSecondsFromBodySizeCurve.Evaluate(pawn.BodySize) * 2f * StunDamageFactorFromQuality(dinfo.WeaponQuality);
			dinfo2.SetAmount(amount);
			pawn.TakeDamage(dinfo2);
		}
	}

	private float StunDamageFactorFromQuality(QualityCategory category)
	{
		return category switch
		{
			QualityCategory.Awful => 0.75f, 
			QualityCategory.Masterwork => 1.25f, 
			QualityCategory.Legendary => 1.5f, 
			_ => 1f, 
		};
	}
}
