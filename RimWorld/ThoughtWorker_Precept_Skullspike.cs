using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_Skullspike : ThoughtWorker_Precept
	{
		public static readonly IntRange LowRange = new IntRange(1, 3);

		public static readonly IntRange MediumRange = new IntRange(4, 8);

		public static readonly IntRange MaxRange = new IntRange(9, int.MaxValue);

		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			if (p.surroundings == null || p.IsSlave)
			{
				return false;
			}
			int val = p.surroundings.NumSkullspikeSightings();
			if (LowRange.Includes(val))
			{
				return ThoughtState.ActiveAtStage(0);
			}
			if (MediumRange.Includes(val))
			{
				return ThoughtState.ActiveAtStage(1);
			}
			if (MaxRange.Includes(val))
			{
				return ThoughtState.ActiveAtStage(2);
			}
			return false;
		}
	}
}
