using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ThoughtWorker_Precept_ResettledRecently : ThoughtWorker_Precept, IPreceptCompDescriptionArgs
{
	private static readonly List<(int day, int index)> Levels = new List<(int, int)>
	{
		(5, 0),
		(10, 1),
		(20, -1),
		(30, 2),
		(int.MaxValue, 3)
	};

	public int TicksSinceLastResettle => GenTicks.TicksGame - Find.IdeoManager.lastResettledTick;

	protected override ThoughtState ShouldHaveThought(Pawn p)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return false;
		}
		if (!p.IsColonist)
		{
			return false;
		}
		return Find.TickManager.TicksGame > Find.IdeoManager.lastResettledTick;
	}

	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (!base.CurrentStateInternal(p).Active)
		{
			return ThoughtState.Inactive;
		}
		float num = TicksSinceLastResettle.TicksToDays();
		foreach (var (num2, num3) in Levels)
		{
			if (num < (float)num2)
			{
				return (num3 < 0) ? ThoughtState.Inactive : ThoughtState.ActiveAtStage(num3);
			}
		}
		return ThoughtState.Inactive;
	}

	public IEnumerable<NamedArgument> GetDescriptionArgs()
	{
		yield return TicksSinceLastResettle.ToStringTicksToPeriod().Named("DURATION");
	}
}
