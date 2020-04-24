using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class GenStep_MechCluster : GenStep
	{
		public const int ExtraRangeToRectOfInterest = 20;

		public static readonly FloatRange DefaultPointsRange = new FloatRange(750f, 2500f);

		public override int SeedPart => 341176078;

		public override void Generate(Map map, GenStepParams parms)
		{
			MechClusterSketch sketch = MechClusterGenerator.GenerateClusterSketch(DefaultPointsRange.RandomInRange, map, startDormant: false);
			IntVec3 center = IntVec3.Invalid;
			if (MapGenerator.TryGetVar("RectOfInterest", out CellRect var))
			{
				center = var.ExpandedBy(20).MaxBy((IntVec3 x) => MechClusterUtility.GetClusterPositionScore(x, map, sketch));
			}
			if (!center.IsValid)
			{
				center = MechClusterUtility.FindClusterPosition(map, sketch);
			}
			List<Thing> list = MechClusterUtility.SpawnCluster(center, map, sketch, dropInPods: false);
			List<Pawn> list2 = new List<Pawn>();
			foreach (Thing item in list)
			{
				if (item is Pawn)
				{
					list2.Add((Pawn)item);
				}
			}
			if (list2.Any())
			{
				GenStep_SleepingMechanoids.SendMechanoidsToSleepImmediately(list2);
			}
		}
	}
}
