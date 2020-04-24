using Verse;

namespace RimWorld
{
	public class Verb_MechCluster : Verb
	{
		public const float Points = 2500f;

		protected override bool TryCastShot()
		{
			if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
			{
				return false;
			}
			MechClusterUtility.SpawnCluster(currentTarget.Cell, caster.Map, MechClusterGenerator.GenerateClusterSketch(2500f, caster.Map));
			if (base.EquipmentSource != null && !base.EquipmentSource.Destroyed)
			{
				base.EquipmentSource.Destroy();
			}
			return true;
		}

		public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
		{
			needLOSToCenter = false;
			return 23f;
		}
	}
}
