using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Thought_Situational_Precept_HighLife : Thought_Situational
	{
		protected override float BaseMoodOffset
		{
			get
			{
				if (ThoughtUtility.ThoughtNullified(pawn, def))
				{
					return 0f;
				}
				float x = (float)(Find.TickManager.TicksGame - pawn.mindState.lastTakeRecreationalDrugTick) / 60000f;
				return Mathf.RoundToInt(ThoughtWorker_Precept_HighLife.MoodOffsetFromDaysSinceLastDrugCurve.Evaluate(x));
			}
		}
	}
}
