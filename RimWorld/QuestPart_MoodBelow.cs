using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class QuestPart_MoodBelow : QuestPartActivable
{
	public List<Pawn> pawns = new List<Pawn>();

	public float threshold;

	public int minTicksBelowThreshold;

	public bool showAlert = true;

	private List<int> moodBelowThresholdTicks = new List<int>();

	private List<Pawn> culpritsResult = new List<Pawn>();

	public override AlertReport AlertReport
	{
		get
		{
			if (!showAlert || minTicksBelowThreshold < 60)
			{
				return AlertReport.Inactive;
			}
			culpritsResult.Clear();
			for (int i = 0; i < pawns.Count; i++)
			{
				if (MoodBelowThreshold(pawns[i]))
				{
					culpritsResult.Add(pawns[i]);
				}
			}
			return AlertReport.CulpritsAre(culpritsResult);
		}
	}

	public override bool AlertCritical => true;

	public override string AlertLabel => "QuestPartMoodBelowThreshold".Translate();

	public override string AlertExplanation => "QuestPartMoodBelowThresholdDesc".Translate(quest.name, GenLabel.ThingsLabel(pawns.Where(MoodBelowThreshold).Cast<Thing>()));

	public override void QuestPartTick()
	{
		base.QuestPartTick();
		while (moodBelowThresholdTicks.Count < pawns.Count)
		{
			moodBelowThresholdTicks.Add(0);
		}
		for (int i = 0; i < pawns.Count; i++)
		{
			if (MoodBelowThreshold(pawns[i]))
			{
				moodBelowThresholdTicks[i]++;
				if (moodBelowThresholdTicks[i] >= minTicksBelowThreshold)
				{
					Complete(pawns[i].Named("SUBJECT"));
					break;
				}
			}
			else
			{
				moodBelowThresholdTicks[i] = 0;
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
		Scribe_Values.Look(ref threshold, "threshold", 0f);
		Scribe_Values.Look(ref minTicksBelowThreshold, "minTicksBelowThreshold", 0);
		Scribe_Values.Look(ref showAlert, "showAlert", defaultValue: true);
		Scribe_Collections.Look(ref moodBelowThresholdTicks, "moodBelowThresholdTicks", LookMode.Value);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			pawns.RemoveAll((Pawn x) => x == null);
		}
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		if (Find.AnyPlayerHomeMap != null)
		{
			Map randomPlayerHomeMap = Find.RandomPlayerHomeMap;
			pawns.Add(randomPlayerHomeMap.mapPawns.FreeColonists.FirstOrDefault());
			threshold = 0.5f;
			minTicksBelowThreshold = 2500;
		}
	}

	private bool MoodBelowThreshold(Pawn pawn)
	{
		if (pawn.needs == null || pawn.needs.mood == null)
		{
			return false;
		}
		return pawn.needs.mood.CurLevelPercentage < threshold;
	}

	public override void ReplacePawnReferences(Pawn replace, Pawn with)
	{
		pawns.Replace(replace, with);
	}
}
