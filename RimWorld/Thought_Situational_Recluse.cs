using Verse;

namespace RimWorld
{
	public class Thought_Situational_Recluse : Thought_Situational
	{
		public const int NumPawns_Alone = 1;

		public const int NumPawns_NotCrowded = 4;

		public const int NumPawns_Inactive = 10;

		public const int NumPawns_Crowded = 15;

		private static readonly SimpleCurve MoodOffsetCurveFromPopulation = new SimpleCurve
		{
			new CurvePoint(1f, 12f),
			new CurvePoint(2f, 8f),
			new CurvePoint(3f, 6f),
			new CurvePoint(4f, 4f),
			new CurvePoint(5f, 0f),
			new CurvePoint(10f, 0f),
			new CurvePoint(11f, -2f),
			new CurvePoint(12f, -3f),
			new CurvePoint(13f, -4f),
			new CurvePoint(14f, -5f),
			new CurvePoint(15f, -6f),
			new CurvePoint(16f, -8f)
		};

		public override float MoodOffset()
		{
			int freeColonistsAndPrisonersSpawnedCount = pawn.Map.mapPawns.FreeColonistsAndPrisonersSpawnedCount;
			return MoodOffsetCurveFromPopulation.Evaluate(freeColonistsAndPrisonersSpawnedCount);
		}
	}
}
