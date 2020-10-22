using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Need_Mood : Need_Seeker
	{
		public ThoughtHandler thoughts;

		public PawnObserver observer;

		public PawnRecentMemory recentMemory;

		public override float CurInstantLevel
		{
			get
			{
				float num = thoughts.TotalMoodOffset();
				if (pawn.IsColonist || pawn.IsPrisonerOfColony)
				{
					num += Find.Storyteller.difficultyValues.colonistMoodOffset;
				}
				return Mathf.Clamp01(def.baseLevel + num / 100f);
			}
		}

		public string MoodString
		{
			get
			{
				if (pawn.MentalStateDef != null)
				{
					return "Mood_MentalState".Translate();
				}
				float breakThresholdExtreme = pawn.mindState.mentalBreaker.BreakThresholdExtreme;
				if (CurLevel < breakThresholdExtreme)
				{
					return "Mood_AboutToBreak".Translate();
				}
				if (CurLevel < breakThresholdExtreme + 0.05f)
				{
					return "Mood_OnEdge".Translate();
				}
				if (CurLevel < pawn.mindState.mentalBreaker.BreakThresholdMinor)
				{
					return "Mood_Stressed".Translate();
				}
				if (CurLevel < 0.65f)
				{
					return "Mood_Neutral".Translate();
				}
				if (CurLevel < 0.9f)
				{
					return "Mood_Content".Translate();
				}
				return "Mood_Happy".Translate();
			}
		}

		public Need_Mood(Pawn pawn)
			: base(pawn)
		{
			thoughts = new ThoughtHandler(pawn);
			observer = new PawnObserver(pawn);
			recentMemory = new PawnRecentMemory(pawn);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref thoughts, "thoughts", pawn);
			Scribe_Deep.Look(ref recentMemory, "recentMemory", pawn);
		}

		public override void NeedInterval()
		{
			base.NeedInterval();
			recentMemory.RecentMemoryInterval();
			thoughts.ThoughtInterval();
			observer.ObserverInterval();
		}

		public override string GetTipString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(base.GetTipString());
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("MentalBreakThresholdExtreme".Translate() + ": " + pawn.mindState.mentalBreaker.BreakThresholdExtreme.ToStringPercent());
			stringBuilder.AppendLine("MentalBreakThresholdMajor".Translate() + ": " + pawn.mindState.mentalBreaker.BreakThresholdMajor.ToStringPercent());
			stringBuilder.AppendLine("MentalBreakThresholdMinor".Translate() + ": " + pawn.mindState.mentalBreaker.BreakThresholdMinor.ToStringPercent());
			return stringBuilder.ToString();
		}

		public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = int.MaxValue, float customMargin = -1f, bool drawArrows = true, bool doTooltip = true)
		{
			if (threshPercents == null)
			{
				threshPercents = new List<float>();
			}
			threshPercents.Clear();
			threshPercents.Add(pawn.mindState.mentalBreaker.BreakThresholdExtreme);
			threshPercents.Add(pawn.mindState.mentalBreaker.BreakThresholdMajor);
			threshPercents.Add(pawn.mindState.mentalBreaker.BreakThresholdMinor);
			base.DrawOnGUI(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip);
		}
	}
}
