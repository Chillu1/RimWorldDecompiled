using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;

namespace Verse.AI;

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

	private const float ExtremeBreakMoodFraction = 1f / 7f;

	private static List<Thought> tmpThoughts = new List<Thought>();

	public float BreakThresholdExtreme => pawn.GetStatValue(StatDefOf.MentalBreakThreshold, applyPostProcess: true, 5) * (1f / 7f);

	public float BreakThresholdMajor => pawn.GetStatValue(StatDefOf.MentalBreakThreshold, applyPostProcess: true, 5) * 0.5714286f;

	public float BreakThresholdMinor => pawn.GetStatValue(StatDefOf.MentalBreakThreshold, applyPostProcess: true, 5);

	public bool CanDoRandomMentalBreaks
	{
		get
		{
			LifeStageDef curLifeStage = pawn.ageTracker.CurLifeStage;
			if (curLifeStage == null)
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
			return curLifeStage.canDoRandomMentalBreaks;
		}
	}

	public bool BreakExtremeIsImminent
	{
		get
		{
			if (pawn.MentalStateDef == null && CurMood < BreakThresholdExtreme)
			{
				return !Blocked;
			}
			return false;
		}
	}

	public bool BreakMajorIsImminent
	{
		get
		{
			if (pawn.MentalStateDef == null && !BreakExtremeIsImminent && CurMood < BreakThresholdMajor)
			{
				return !Blocked;
			}
			return false;
		}
	}

	public bool BreakMinorIsImminent
	{
		get
		{
			if (pawn.MentalStateDef == null && !BreakExtremeIsImminent && !BreakMajorIsImminent && CurMood < BreakThresholdMinor)
			{
				return !Blocked;
			}
			return false;
		}
	}

	public bool BreakExtremeIsApproaching
	{
		get
		{
			if (pawn.MentalStateDef == null && BreakExtremeIsImminent && CurMood < BreakThresholdExtreme + 0.1f)
			{
				return !Blocked;
			}
			return false;
		}
	}

	public bool Blocked => pawn.mindState.mentalStateHandler.MentalBreaksBlocked();

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
			foreach (MentalBreakDef item in GetBreaksForIntensity(CurrentDesiredMoodBreakIntensity, ChooseAnomalyBreak))
			{
				yield return item;
			}
		}
	}

	private bool ChooseAnomalyBreak
	{
		get
		{
			if (!ModsConfig.AnomalyActive)
			{
				return false;
			}
			if (pawn.IsCreepJoiner)
			{
				return true;
			}
			MonolithLevelDef levelDef = Find.Anomaly.LevelDef;
			if (levelDef != null)
			{
				return Rand.Chance(levelDef.anomalyMentalBreakChance);
			}
			return false;
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

	private IEnumerable<MentalBreakDef> GetBreaksForIntensity(MentalBreakIntensity intensity, bool anomalyBreak, bool tryNonAnomalyBreakIfNoneAvailable = true)
	{
		if (ModsConfig.IdeologyActive && pawn.Ideo != null)
		{
			bool flag = false;
			HashSet<MentalBreakDef> cachedPossibleMentalBreaks = pawn.Ideo.cachedPossibleMentalBreaks;
			foreach (MentalBreakDef item in cachedPossibleMentalBreaks)
			{
				if (item.Worker.BreakCanOccur(pawn))
				{
					yield return item;
					flag = true;
				}
			}
			if (flag)
			{
				yield break;
			}
		}
		while (intensity != MentalBreakIntensity.None)
		{
			MentalBreakIntensity intensityInner = intensity;
			IEnumerable<MentalBreakDef> enumerable = DefDatabase<MentalBreakDef>.AllDefsListForReading.Where((MentalBreakDef d) => d.intensity == intensityInner && d.Worker.BreakCanOccur(pawn) && d.anomalousBreak == anomalyBreak);
			if (anomalyBreak && enumerable.EnumerableNullOrEmpty() && tryNonAnomalyBreakIfNoneAvailable)
			{
				enumerable = DefDatabase<MentalBreakDef>.AllDefsListForReading.Where((MentalBreakDef d) => d.intensity == intensityInner && d.Worker.BreakCanOccur(pawn) && !d.anomalousBreak);
			}
			bool flag2 = false;
			foreach (MentalBreakDef item2 in enumerable)
			{
				yield return item2;
				flag2 = true;
			}
			if (!flag2)
			{
				intensity--;
				continue;
			}
			break;
		}
	}

	public void MentalBreakerTickInterval(int delta)
	{
		if (ticksUntilCanDoMentalBreak > 0 && pawn.Awake())
		{
			ticksUntilCanDoMentalBreak -= delta;
		}
		if (!CanDoRandomMentalBreaks || pawn.MentalStateDef != null || !pawn.IsHashIntervalTick(150, delta) || !DebugSettings.enableRandomMentalStates)
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
		if ((!TestMoodMentalBreak() || (!AlwaysBerserkUtility.TryTriggerBerserkBloodRage(pawn) && !AlwaysBerserkUtility.TryTriggerBerserkFrenzyInducer(pawn) && !TryDoRandomMoodCausedMentalBreak())) && pawn.story != null)
		{
			List<Trait> allTraits = pawn.story.traits.allTraits;
			for (int i = 0; i < allTraits.Count && (allTraits[i].Suppressed || !allTraits[i].CurrentData.MentalStateGiver.CheckGive(pawn, 150)); i++)
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
		if (!CanHaveMentalBreak())
		{
			return false;
		}
		if (!CurrentPossibleMoodBreaks.TryRandomElementByWeight((MentalBreakDef d) => d.Worker.CommonalityFor(pawn, moodCaused: true), out var result))
		{
			return false;
		}
		Thought thought = RandomFinalStraw();
		TaggedString taggedString = "MentalStateReason_Mood".Translate();
		if (thought != null)
		{
			taggedString += "\n\n" + "FinalStraw".Translate(thought.LabelCap);
		}
		return TryDoMentalBreak(taggedString, result);
	}

	public bool TryDoMentalBreak(string reason, MentalBreakDef breakDef)
	{
		if (!CanHaveMentalBreak())
		{
			return false;
		}
		return breakDef.Worker.TryStart(pawn, reason, causedByMood: true);
	}

	public bool TryGetRandomMentalBreak(MentalBreakIntensity intensity, out MentalBreakDef breakDef)
	{
		return GetBreaksForIntensity(intensity, ChooseAnomalyBreak).TryRandomElementByWeight((MentalBreakDef d) => d.Worker.CommonalityFor(pawn, moodCaused: true), out breakDef);
	}

	private bool CanHaveMentalBreak()
	{
		if (!CanDoRandomMentalBreaks || pawn.Downed || !pawn.Awake() || pawn.InMentalState)
		{
			return false;
		}
		if (pawn.IsMutant && pawn.mutant.Def.preventsMentalBreaks)
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
		if (Blocked)
		{
			return false;
		}
		return true;
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
		tmpThoughts.Where((Thought x) => x.MoodOffset() <= maxMoodOffset).TryRandomElementByWeight((Thought x) => 0f - x.MoodOffset(), out var result);
		tmpThoughts.Clear();
		return result;
	}

	public void Notify_RecoveredFromMentalState()
	{
		ticksUntilCanDoMentalBreak = 15000;
	}

	public float MentalBreakThresholdFor(MentalBreakIntensity intensity)
	{
		return intensity switch
		{
			MentalBreakIntensity.Extreme => BreakThresholdExtreme, 
			MentalBreakIntensity.Major => BreakThresholdMajor, 
			MentalBreakIntensity.Minor => BreakThresholdMinor, 
			_ => throw new NotImplementedException(), 
		};
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
		stringBuilder.AppendLine("Current desired mood break intensity: " + CurrentDesiredMoodBreakIntensity);
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Current possible mood breaks:");
		float num = CurrentPossibleMoodBreaks.Select((MentalBreakDef d) => d.Worker.CommonalityFor(pawn, moodCaused: true)).Sum();
		foreach (MentalBreakDef currentPossibleMoodBreak in CurrentPossibleMoodBreaks)
		{
			float num2 = currentPossibleMoodBreak.Worker.CommonalityFor(pawn, moodCaused: true);
			stringBuilder.AppendLine("   " + currentPossibleMoodBreak?.ToString() + "     " + (num2 / num).ToStringPercent());
		}
		return stringBuilder.ToString();
	}

	internal void LogPossibleMentalBreaks()
	{
		StringBuilder stringBuilder = new StringBuilder();
		MentalBreakIntensity currentDesiredMoodBreakIntensity = CurrentDesiredMoodBreakIntensity;
		stringBuilder.AppendLine(pawn?.ToString() + " current possible mood mental breaks:").AppendLine("CurrentDesiredMoodBreakIntensity: " + currentDesiredMoodBreakIntensity);
		foreach (MentalBreakDef item in GetBreaksForIntensity(currentDesiredMoodBreakIntensity, anomalyBreak: false))
		{
			stringBuilder.AppendLine("  " + item);
		}
		if (ModsConfig.AnomalyActive)
		{
			MonolithLevelDef levelDef = Find.Anomaly.LevelDef;
			if (levelDef != null && levelDef.anomalyMentalBreakChance > 0f)
			{
				foreach (MentalBreakDef item2 in GetBreaksForIntensity(currentDesiredMoodBreakIntensity, anomalyBreak: true, tryNonAnomalyBreakIfNoneAvailable: false))
				{
					stringBuilder.AppendLine("  " + item2);
				}
			}
		}
		Log.Message(stringBuilder.ToString());
	}
}
