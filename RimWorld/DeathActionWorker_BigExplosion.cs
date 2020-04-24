using Verse;

namespace RimWorld
{
	public class DeathActionWorker_BigExplosion : DeathActionWorker
	{
		public override RulePackDef DeathRules => RulePackDefOf.Transition_DiedExplosive;

		public override bool DangerousInMelee => true;

		public override void PawnDied(Corpse corpse)
		{
			GenExplosion.DoExplosion(radius: (corpse.InnerPawn.ageTracker.CurLifeStageIndex == 0) ? 1.9f : ((corpse.InnerPawn.ageTracker.CurLifeStageIndex != 1) ? 4.9f : 2.9f), center: corpse.Position, map: corpse.Map, damType: DamageDefOf.Flame, instigator: corpse.InnerPawn);
		}
	}
}
