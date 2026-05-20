using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ThoughtWorker_Precept_NoRecentHumanMeat : ThoughtWorker_Precept, IPreceptCompDescriptionArgs
{
	public const int MinDaysSinceLastHumanMeatForThought = 8;

	protected override ThoughtState ShouldHaveThought(Pawn p)
	{
		if (p.needs?.food == null)
		{
			return false;
		}
		int num = Mathf.Max(0, p.mindState.lastHumanMeatIngestedTick);
		return Find.TickManager.TicksGame - num > 480000;
	}

	public IEnumerable<NamedArgument> GetDescriptionArgs()
	{
		yield return 8.Named("HUMANMEATREQUIREDINTERVAL");
	}
}
