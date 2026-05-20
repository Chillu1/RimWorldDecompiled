using RimWorld;

namespace Verse;

public class Verb_LaunchProjectileStaticPsychic : Verb_LaunchProjectileStatic
{
	public override void OnGUI(LocalTargetInfo target)
	{
		base.OnGUI(target);
		if (!caster.Spawned || !target.IsValid || !CanHitTarget(target))
		{
			return;
		}
		bool needLOSToCenter;
		float num = HighlightFieldRadiusAroundTarget(out needLOSToCenter);
		if (!(num > 0.2f))
		{
			return;
		}
		foreach (IntVec3 item in DamageDefOf.Bomb.Worker.ExplosionCellsToHit(target.Cell, Find.CurrentMap, num))
		{
			if (!item.Fogged(Find.CurrentMap))
			{
				Pawn firstPawn = item.GetFirstPawn(Find.CurrentMap);
				if (firstPawn != null && firstPawn.GetStatValue(StatDefOf.PsychicSensitivity) < float.Epsilon && !firstPawn.IsHiddenFromPlayer())
				{
					Verb_CastPsycast.DrawIneffectiveWarningStatic(firstPawn);
				}
			}
		}
	}
}
