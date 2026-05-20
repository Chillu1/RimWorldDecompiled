using Verse;

namespace RimWorld
{
	public class ThoughtWorker_TreesDesired : ThoughtWorker_Precept
	{
		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			if (p.surroundings == null)
			{
				return ThoughtState.Inactive;
			}
			return ThoughtState.ActiveAtStage(ThoughtStageIndex(p));
		}

		private int ThoughtStageIndex(Pawn p)
		{
			int num = SightingsInRange(TreeCategory.Super, 2500);
			if (num > 1)
			{
				return 0;
			}
			if (num == 1)
			{
				return 1;
			}
			int num2 = SightingsInRange(TreeCategory.Super, 15000);
			int num3 = SightingsInRange(TreeCategory.Full, 15000);
			if (num2 >= 1 && num3 >= 3)
			{
				return 2;
			}
			if (num2 >= 1)
			{
				return 3;
			}
			if (num3 >= 5)
			{
				return 4;
			}
			if (num3 >= 3)
			{
				return 5;
			}
			int num4 = SightingsInRange(TreeCategory.Mini, 30000);
			if (num3 >= 1 && num4 >= 1)
			{
				return 6;
			}
			if (num3 == 1)
			{
				return 7;
			}
			if (num4 >= 5)
			{
				return 8;
			}
			if (SightingsInRange(TreeCategory.Mini, 60000) >= 1)
			{
				return 9;
			}
			if (AnySightingsInRange(120000))
			{
				return 10;
			}
			if (AnySightingsInRange(180000))
			{
				return 11;
			}
			if (AnySightingsInRange(240000))
			{
				return 12;
			}
			if (AnySightingsInRange(300000))
			{
				return 13;
			}
			return 14;
			bool AnySightingsInRange(int ticks)
			{
				if (SightingsInRange(TreeCategory.Super, ticks) <= 0 && SightingsInRange(TreeCategory.Full, ticks) <= 0)
				{
					return SightingsInRange(TreeCategory.Mini, ticks) > 0;
				}
				return true;
			}
			int SightingsInRange(TreeCategory category, int ticks)
			{
				return p.surroundings.NumSightingsInRange(category, ticks);
			}
		}
	}
}
