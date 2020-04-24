using RimWorld;

namespace Verse
{
	public class Verb_Shoot : Verb_LaunchProjectile
	{
		protected override int ShotsPerBurst => verbProps.burstShotCount;

		public override void WarmupComplete()
		{
			base.WarmupComplete();
			Pawn pawn = currentTarget.Thing as Pawn;
			if (pawn != null && !pawn.Downed && CasterIsPawn && CasterPawn.skills != null)
			{
				float num = pawn.HostileTo(caster) ? 170f : 20f;
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
}
