using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class GenStep_MechCluster : GenStep
{
	public bool forceNoConditionCauser;

	public int extraRangeToRectOfInterest = 20;

	public override int SeedPart => 341176078;

	public override void Generate(Map map, GenStepParams parms)
	{
		MechClusterSketch sketch = MechClusterGenerator.GenerateClusterSketch(parms.sitePart.parms.threatPoints, map, startDormant: true, forceNoConditionCauser);
		IntVec3 center = IntVec3.Invalid;
		if (MapGenerator.TryGetVar<CellRect>("RectOfInterest", out var var))
		{
			center = var.ExpandedBy(extraRangeToRectOfInterest).MaxBy((IntVec3 x) => MechClusterUtility.GetClusterPositionScore(x, map, sketch));
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
