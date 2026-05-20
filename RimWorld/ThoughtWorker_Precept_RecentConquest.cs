using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_RecentConquest : ThoughtWorker_Precept, IPreceptCompDescriptionArgs
	{
		private static readonly SimpleCurve MoodMultiplierCurve = new SimpleCurve
		{
			new CurvePoint(0f, 1f),
			new CurvePoint(25f, 0f),
			new CurvePoint(50f, 1f),
			new CurvePoint(75f, 1.5f)
		};

		private int DaysSinceLastRaidThreshold = 25;

		private float DaysSinceLastRaid => (float)(Find.TickManager.TicksGame - Find.History.lastTickPlayerRaidedSomeone) / 60000f;

		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			if (!p.IsColonist)
			{
				return false;
			}
			if (p.IsSlave)
			{
				return false;
			}
			int num = ((DaysSinceLastRaid > (float)DaysSinceLastRaidThreshold) ? 1 : 0);
			if (num == 1)
			{
				int num2 = Find.TickManager.SettleTick;
				if (p.Ideo.Fluid && p.Ideo.development.lastTickRaidingApproved > num2)
				{
					num2 = p.Ideo.development.lastTickRaidingApproved;
				}
				if ((float)(Find.TickManager.TicksGame - num2) < 1800000f)
				{
					return false;
				}
			}
			return ThoughtState.ActiveAtStage(num);
		}

		public override float MoodMultiplier(Pawn p)
		{
			return MoodMultiplierCurve.Evaluate(DaysSinceLastRaid);
		}

		public IEnumerable<NamedArgument> GetDescriptionArgs()
		{
			yield return DaysSinceLastRaidThreshold.Named("DAYSSINCELASTRAIDTHRESHOLD");
		}
	}
}
