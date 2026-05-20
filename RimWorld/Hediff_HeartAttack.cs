using Verse;

namespace RimWorld;

public class Hediff_HeartAttack : HediffWithComps
{
	private float intervalFactor;

	private const int SeverityChangeInterval = 5000;

	private const float TendSuccessChanceFactor = 0.65f;

	private const float TendSeverityReduction = 0.3f;

	public override void PostMake()
	{
		base.PostMake();
		intervalFactor = Rand.Range(0.1f, 2f);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref intervalFactor, "intervalFactor", 0f);
	}

	public override void TickInterval(int delta)
	{
		base.TickInterval(delta);
		if (pawn.IsHashIntervalTick((int)(5000f * intervalFactor), delta))
		{
			Severity += Rand.Range(-0.4f, 0.6f);
		}
	}

	public override void Tended(float quality, float maxQuality, int batchPosition = 0)
	{
		base.Tended(quality, maxQuality, 0);
		float num = 0.65f * quality;
		if (Rand.Value < num)
		{
			if (batchPosition == 0 && pawn.Spawned)
			{
				MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "TextMote_TreatSuccess".Translate(num.ToStringPercent()), 6.5f);
			}
			Severity -= 0.3f;
		}
		else if (batchPosition == 0 && pawn.Spawned)
		{
			MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "TextMote_TreatFailed".Translate(num.ToStringPercent()), 6.5f);
		}
	}
}
