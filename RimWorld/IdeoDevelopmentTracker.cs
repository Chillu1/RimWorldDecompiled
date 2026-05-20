using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class IdeoDevelopmentTracker : IExposable
{
	private const int DevelopmentPointsPerConversion = 1;

	private static readonly SimpleCurve DevelopmentPointsOverReformCountCurve = new SimpleCurve
	{
		new CurvePoint(0f, 10f),
		new CurvePoint(1f, 12f),
		new CurvePoint(2f, 14f),
		new CurvePoint(3f, 16f),
		new CurvePoint(4f, 18f),
		new CurvePoint(5f, 20f)
	};

	public Ideo ideo;

	public int points;

	public int reformCount;

	public int lastTickRaidingApproved = -9999999;

	public int Points => points;

	public bool CanReformNow => Points >= NextReformationDevelopmentPoints;

	public int NextReformationDevelopmentPoints => Mathf.FloorToInt(DevelopmentPointsOverReformCountCurve.Evaluate(reformCount));

	public bool CanBeDevelopedNow => ideo.ColonistBelieverCountCached > 0;

	public IdeoDevelopmentTracker()
	{
	}

	public IdeoDevelopmentTracker(Ideo ideo)
	{
		this.ideo = ideo;
	}

	public void Notify_MemberGainedByConversion()
	{
		TryAddDevelopmentPoints(1);
	}

	public void Notify_Reformed()
	{
		ResetDevelopmentPoints();
		reformCount++;
		foreach (Precept item in ideo.PreceptsListForReading)
		{
			item.Notify_IdeoReformed();
		}
		foreach (Pawn item2 in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoCryptosleep)
		{
			if (item2.Ideo == ideo && !item2.IsSlave && !item2.IsQuestLodger())
			{
				item2.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
			}
		}
	}

	public void Notify_PreReform(Ideo newIdeo)
	{
		if (!ApprovesOfRaiding(ideo) && ApprovesOfRaiding(newIdeo))
		{
			lastTickRaidingApproved = Find.TickManager.TicksGame;
		}
		static bool ApprovesOfRaiding(Ideo ideo)
		{
			List<Precept> preceptsListForReading = ideo.PreceptsListForReading;
			for (int i = 0; i < preceptsListForReading.Count; i++)
			{
				if (preceptsListForReading[i].def.approvesOfRaiding)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool TryGainDevelopmentPointsForRitualOutcome(Precept_Ritual ritual, int outcomeIndex, out int developmentPoints)
	{
		developmentPoints = 0;
		if (!CanBeDevelopedNow)
		{
			return false;
		}
		SimpleCurve developmentPointsOverOutcomeIndexCurveForRitual = IdeoDevelopmentUtility.GetDevelopmentPointsOverOutcomeIndexCurveForRitual(ideo, ritual);
		if (developmentPointsOverOutcomeIndexCurveForRitual != null)
		{
			developmentPoints = Mathf.FloorToInt(developmentPointsOverOutcomeIndexCurveForRitual.Evaluate(outcomeIndex));
			int num = Points;
			if (developmentPoints > 0 && TryAddDevelopmentPoints(developmentPoints))
			{
				Messages.Message("MessageDevelopmentPointsEarned".Translate(num, Points, ritual.Label), MessageTypeDefOf.PositiveEvent);
			}
			return true;
		}
		return false;
	}

	public bool TryAddDevelopmentPoints(int pointsToAdd)
	{
		if (pointsToAdd <= 0 || CanReformNow)
		{
			return false;
		}
		bool canReformNow = CanReformNow;
		points = Mathf.Min(points + pointsToAdd, NextReformationDevelopmentPoints);
		if (!canReformNow && CanReformNow)
		{
			Find.LetterStack.ReceiveLetter("LetterLabelReformIdeo".Translate(), "LetterTextReformIdeo".Translate(ideo), LetterDefOf.PositiveEvent);
		}
		return true;
	}

	public void ResetDevelopmentPoints()
	{
		points = 0;
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref points, "points", 0);
		Scribe_Values.Look(ref reformCount, "reformCount", 0);
		Scribe_Values.Look(ref lastTickRaidingApproved, "lastTickRaidingApproved", -9999999);
	}

	public void CopyTo(IdeoDevelopmentTracker other)
	{
		other.points = points;
		other.reformCount = reformCount;
		other.lastTickRaidingApproved = lastTickRaidingApproved;
	}
}
