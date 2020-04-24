using Verse;

namespace RimWorld
{
	public abstract class RoadDefGenStep
	{
		public SimpleCurve chancePerPositionCurve;

		public float antialiasingMultiplier = 1f;

		public int periodicSpacing;

		public abstract void Place(Map map, IntVec3 position, TerrainDef rockDef, IntVec3 origin, GenStep_Roads.DistanceElement[,] distance);
	}
}
