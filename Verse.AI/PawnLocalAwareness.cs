namespace Verse.AI
{
	public static class PawnLocalAwareness
	{
		private const float SightRadius = 30f;

		public static bool AnimalAwareOf(this Pawn p, Thing t)
		{
			if (p.RaceProps.ToolUser || p.Faction != null)
			{
				return true;
			}
			if ((float)(p.Position - t.Position).LengthHorizontalSquared > 900f)
			{
				return false;
			}
			if (p.GetRoom() != t.GetRoom())
			{
				return false;
			}
			if (!GenSight.LineOfSight(p.Position, t.Position, p.Map))
			{
				return false;
			}
			return true;
		}
	}
}
