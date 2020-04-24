using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verse.AI
{
	public class MentalBreaker : IExposable
	{
		private Pawn pawn;

		private int ticksUntilCanDoMentalBreak;

		private int ticksBelowExtreme;

		private int ticksBelowMajor;

		private int ticksBelowMinor;

		private const int CheckInterval = 150;

		private const float ExtremeBreakMTBDays = 0.5f;

		private const float MajorBreakMTBDays = 0.8f;

		private const float MinorBreakMTBDays = 4f;

		private const int MinTicksBelowToBreak = 2000;

		private const int MinTicksSinceRecoveryToBreak = 15000;

		private const float MajorBreakMoodFraction = 0.5714286f;

		private const float ExtremeBreakMoodFraction = 0.142857149f;

		private static List<Thought> tmpThoughts = new List<Thought>();

		public float BreakThresholdExtreme => pawn.GetStatValue(StatDefOf.MentalBreakThreshold) * 0.142857149f;

		public float BreakThresholdMajor => pawn.GetStatValue(StatDefOf.MentalBreakThreshold) * 0.5714286f;

		public float BreakThresholdMinor => pawn.GetStatValue(StatDefOf.MentalBreakThreshold);

		private bool CanDoRandomMentalBreaks
		{
			get
			{
				if (pawn.RaceProps.Humanlike)
				{
					if (!pawn.Spawned)
					{
						return pawn.IsCaravanMember();
					}
					return true;
				}
				return false;
			}
		}

		public bool BreakExtremeIsImminent
		{
			get
			{
				if (pawn.MentalStateDef == null)
				{
					return CurMood < BreakThresholdExtreme;
				}
				return false;
			}
		}

		public bool BreakMajorIsImminent
		{
			get
			{
				if (pawn.MentalStateDef == null && !BreakExtremeIsImminent)
				{
					return CurMood < BreakThresholdMajor;
				}
				return false;
			}
		}

		public bool BreakMinorIsImminent
		{
			get
			{
				if (pawn.MentalStateDef == null && !BreakExtremeIsImminent && !BreakMajorIsImminent)
				{
					return CurMood < BreakThresholdMinor;
				}
				return false;
			}
		}

		public bool BreakExtremeIsApproaching
		{
			get
			{
				if (pawn.MentalStateDef == null && !BreakExtremeIsImminent)
				{
					return CurMood < BreakThresholdExtreme + 0.1f;
				}
				return false;
			}
		}

		public float CurMood
		{
			get
			{
				if (pawn.needs.mood == null)
				{
					return 0.5f;
				}
				return pawn.needs.mood.CurLevel;
			}
		}

		private IEnumerable<MentalBreakDef> CurrentPossibleMoodBreaks
		{
			get
			{
				MentalBreakIntensity intensity;
				for (intensity = CurrentDesiredMoodBreakIntensity; intensity != 0; intensity--)
				{
					IEnumerable<MentalBreakDef> enumerable = DefDatabase<MentalBreakDef>.AllDefsListForReading.Where((MentalBreakDef d) => d.intensity == intensity && d.Worker.BreakCanOccur(pawn));
					bool flag = false;
					foreach (MentalBreakDef item in enumerable)
					{
						yield return item;
						flag = true;
					}
					if (flag)
					{
						break;
					}
				}
			}
		}

		private MentalBreakIntensity CurrentDesiredMoodBreakIntensity
		{
			get
			{
				if (ticksBelowExtreme >= 2000)
				{
					return MentalBreakIntensity.Extreme;
				}
				if (ticksBelowMajor >= 2000)
				{
					return MentalBreakIntensity.Major;
				}
				if (ticksBelowMinor >= 2000)
				{
					return MentalBreakIntensity.Minor;
				}
				return MentalBreakIntensity.None;
			}
		}

		public MentalBreaker()
		{
		}

		public MentalBreaker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		internal void Reset()
		{
			ticksBelowExtreme = 0;
			ticksBelowMajor = 0;
			ticksBelowMinor = 0;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref ticksUntilCanDoMentalBreak, "ticksUntilCanDoMentalBreak", 0);
			Scribe_Values.Look(ref ticksBelowExtreme, "ticksBelowExtreme", 0);
			Scribe_Values.Look(ref ticksBelowMajor, "ticksBelowMajor", 0);
			Scribe_Values.Look(ref ticksBelowMinor, "ticksBelowMinor", 0);
		}

		public void MentalBreakerTick()
		{
			if (ticksUntilCanDoMentalBreak > 0 && pawn.Awake())
			{
				ticksUntilCanDoMentalBreak--;
			}
			if (!CanDoRandomMentalBreaks || pawn.MentalStateDef != null || !pawn.IsHashIntervalTick(150) || !DebugSettings.enableRandomMentalStates)
			{
				return;
			}
			if (CurMood < BreakThresholdExtreme)
			{
				ticksBelowExtreme += 150;
			}
			else
			{
				ticksBelowExtreme = 0;
			}
			if (CurMood < BreakThresholdMajor)
			{
				ticksBelowMajor += 150;
			}
			else
			{
				ticksBelowMajor = 0;
			}
			if (CurMood < BreakThresholdMinor)
			{
				ticksBelowMinor += 150;
			}
			else
			{
				ticksBelowMinor = 0;
			}
			if ((!TestMoodMentalBreak() || !TryDoRandomMoodCausedMentalBreak()) && pawn.story != null)
			{
				List<Trait> allTraits = pawn.story.traits.allTraits;
				for (int i = 0; i < allTraits.Count && !allTraits[i].CurrentData.MentalStateGiver.CheckGive(pawn, 150); i++)
				{
				}
			}
		}

		private bool TestMoodMentalBreak()
		{
			if (ticksUntilCanDoMentalBreak > 0)
			{
				return false;
			}
			if (ticksBelowExtreme > 2000)
			{
				return Rand.MTBEventOccurs(0.5f, 60000f, 150f);
			}
			if (ticksBelowMajor > 2000)
			{
				return Rand.MTBEventOccurs(0.8f, 60000f, 150f);
			}
			if (ticksBelowMinor > 2000)
			{
				return Rand.MTBEventOccurs(4f, 60000f, 150f);
			}
			return false;
		}

		public bool TryDoRandomMoodCausedMentalBreak()
		{
			if (!CanDoRandomMentalBreaks || pawn.Downed || !pawn.Awake() || pawn.InMentalState)
			{
				return false;
			}
			if (pawn.Faction != Faction.OfPlayer && CurrentDesiredMoodBreakIntensity != MentalBreakIntensity.Extreme)
			{
				return false;
			}
			if (QuestUtility.AnyQuestDisablesRandomMoodCausedMentalBreaksFor(pawn))
			{
				return false;
			}
			if (!CurrentPossibleMoodBreaks.TryRandomElementByWeight((MentalBreakDef d) => d.Worker.CommonalityFor(pawn, moodCaused: true), out MentalBreakDef result))
			{
				return false;
			}
			Thought thought = RandomFinalStraw();
			TaggedString taggedString = "MentalStateReason_Mood".Translate();
			if (thought != null)
			{
				taggedString += "\n\n" + "FinalStraw".Translate(thought.LabelCap);
			}
			return result.Worker.TryStart(pawn, taggedString, causedByMood: true);
		}

		private Thought RandomFinalStraw()
		{
			pawn.needs.mood.thoughts.GetAllMoodThoughts(tmpThoughts);
			float num = 0f;
			for (int i = 0; i < tmpThoughts.Count; i++)
			{
				float num2 = tmpThoughts[i].MoodOffset();
				if (num2 < num)
				{
					num = num2;
				}
			}
			float maxMoodOffset = num * 0.5f;
			Thought result = null;
			tmpThoughts.Where((Thought x) => x.MoodOffset() <= maxMoodOffset).TryRandomElementByWeight((Thought x) => 0f - x.MoodOffset(), out result);
			tmpThoughts.Clear();
			return result;
		}

		public void Notify_RecoveredFromMentalState()
		{
			ticksUntilCanDoMentalBreak = 15000;
		}

		public float MentalBreakThresholdFor(MentalBreakIntensity intensity)
		{
			switch (intensity)
			{
			case MentalBreakIntensity.Extreme:
				return BreakThresholdExtreme;
			case MentalBreakIntensity.Major:
				return BreakThresholdMajor;
			case MentalBreakIntensity.Minor:
				return BreakThresholdMinor;
			default:
				throw new NotImplementedException();
			}
		}

		internal string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(pawn.ToString());
			stringBuilder.AppendLine("   ticksUntilCanDoMentalBreak=" + ticksUntilCanDoMentalBreak);
			stringBuilder.AppendLine("   ticksBelowExtreme=" + ticksBelowExtreme + "/" + 2000);
			stringBuilder.AppendLine("   ticksBelowSerious=" + ticksBelowMajor + "/" + 2000);
			stringBuilder.AppendLine("   ticksBelowMinor=" + ticksBelowMinor + "/" + 2000);
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Current desired mood break intensity: " + CurrentDesiredMoodBreakIntensity.ToString());
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Current possible mood breaks:");
			float num = CurrentPossibleMoodBreaks.Select((MentalBreakDef d) => d.Worker.CommonalityFor(pawn, moodCaused: true)).Sum();
			foreach (MentalBreakDef currentPossibleMoodBreak in CurrentPossibleMoodBreaks)
			{
				float num2 = currentPossibleMoodBreak.Worker.CommonalityFor(pawn, moodCaused: true);
				stringBuilder.AppendLine("   " + currentPossibleMoodBreak + "     " + (num2 / num).ToStringPercent());
			}
			return stringBuilder.ToString();
		}

		internal void LogPossibleMentalBreaks()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(pawn + " current possible mood mental breaks:");
			stringBuilder.AppendLine("CurrentDesiredMoodBreakIntensity: " + CurrentDesiredMoodBreakIntensity);
			foreach (MentalBreakDef currentPossibleMoodBreak in CurrentPossibleMoodBreaks)
			{
				stringBuilder.AppendLine("  " + currentPossibleMoodBreak);
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
