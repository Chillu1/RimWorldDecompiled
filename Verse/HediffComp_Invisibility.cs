using RimWorld;

namespace Verse
{
	public class HediffComp_Invisibility : HediffComp
	{
		public override void CompPostPostAdd(DamageInfo? dinfo)
		{
			base.CompPostPostAdd(dinfo);
			UpdateTarget();
		}

		public override void CompPostPostRemoved()
		{
			base.CompPostPostRemoved();
			UpdateTarget();
		}

		private void UpdateTarget()
		{
			Pawn pawn = parent.pawn;
			if (pawn.Spawned)
			{
				pawn.Map.attackTargetsCache.UpdateTarget(pawn);
			}
			PortraitsCache.SetDirty(pawn);
		}
	}
}
