using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_NoRecentExecution : ThoughtWorker_Precept, IPreceptCompDescriptionArgs
	{
		private const int MinDaysSinceLastExecutionForThought = 30;

		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			if (!p.IsColonist || p.IsSlave)
			{
				return false;
			}
			int num = Mathf.Max(0, p.Faction.lastExecutionTick);
			return Find.TickManager.TicksGame - num > 1800000;
		}

		public IEnumerable<NamedArgument> GetDescriptionArgs()
		{
			yield return 30.Named("MINDAYSLASTEXECUTION");
		}
	}
}
