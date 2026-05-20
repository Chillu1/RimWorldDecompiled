using RimWorld;

namespace Verse;

public class DamageWorker_Extinguish : DamageWorker
{
	private const float DamageAmountToFireSizeRatio = 0.01f;

	public override DamageResult Apply(DamageInfo dinfo, Thing victim)
	{
		DamageResult result = new DamageResult();
		Fire fire = victim as Fire;
		if (fire == null || fire.Destroyed)
		{
			Thing thing = victim?.GetAttachment(ThingDefOf.Fire);
			if (thing != null)
			{
				fire = (Fire)thing;
			}
		}
		if (fire != null && !fire.Destroyed)
		{
			base.Apply(dinfo, victim);
			fire.fireSize -= dinfo.Amount * 0.01f;
			if (fire.fireSize < 0.1f)
			{
				fire.Destroy();
			}
		}
		if (victim is Pawn pawn)
		{
			Hediff hediff = HediffMaker.MakeHediff(dinfo.Def.hediff, pawn);
			hediff.Severity = dinfo.Amount;
			pawn.health.AddHediff(hediff, null, dinfo);
		}
		return result;
	}
}
