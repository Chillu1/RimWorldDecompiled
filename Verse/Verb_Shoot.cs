using RimWorld;

namespace Verse;

public class Verb_Shoot : Verb_LaunchProjectile
{
	protected override int ShotsPerBurst => base.BurstShotCount;

	public override void WarmupComplete()
	{
		base.WarmupComplete();
		if (currentTarget.Thing is Pawn { Downed: false, IsColonyMech: false } pawn && CasterIsPawn && CasterPawn.skills != null)
		{
			float num = (pawn.HostileTo(caster) ? 170f : 20f);
			float num2 = verbProps.AdjustedFullCycleTime(this, CasterPawn);
			CasterPawn.skills.Learn(SkillDefOf.Shooting, num * num2);
		}
	}

	protected override bool TryCastShot()
	{
		bool num = base.TryCastShot();
		if (num && CasterIsPawn)
		{
			CasterPawn.records.Increment(RecordDefOf.ShotsFired);
		}
		return num;
	}
}
