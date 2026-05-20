using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompProperties_TreeConnection : CompProperties
	{
		public float spawnDays = 6f;

		public PawnKindDef pawnKind;

		public FloatRange initialConnectionStrengthRange;

		public float connectionStrengthLossPerDryadDeath = 0.1f;

		public float radiusToBuildingForConnectionStrengthLoss = 7.9f;

		public int maxDryadsWild;

		public SimpleCurve maxDryadsPerConnectionStrengthCurve;

		public SimpleCurve connectionLossPerLevelCurve;

		public SimpleCurve connectionLossDailyPerBuildingDistanceCurve;

		public SimpleCurve connectionStrengthGainPerPlantSkill;

		public float connectionStrengthGainPerHourPruningBase = 0.01f;

		public Vector3 spawningPodOffset;

		public FloatRange spawningPodSizeRange = FloatRange.One;

		public CompProperties_TreeConnection()
		{
			compClass = typeof(CompTreeConnection);
		}
	}
}
