using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompRitualEffect_IntervalSpawnCircle : CompRitualEffect_IntervalSpawnBurst
	{
		protected new CompProperties_RitualEffectIntervalSpawnCircle Props => (CompProperties_RitualEffectIntervalSpawnCircle)props;

		protected override Vector3 SpawnPos(LordJob_Ritual ritual)
		{
			return CellRect.CenteredOn(ritual.selectedTarget.Cell, Props.area.x / 2, Props.area.z / 2).ClipInsideMap(ritual.Map).Cells.RandomElementByWeight(delegate(IntVec3 c)
			{
				float f = Mathf.Max(Mathf.Abs((c - ritual.selectedTarget.Cell).LengthHorizontal - Props.radius), 1f);
				return 1f / Mathf.Pow(f, Props.concentration);
			}).ToVector3Shifted() + Rand.UnitVector3 * 0.5f;
		}
	}
}
