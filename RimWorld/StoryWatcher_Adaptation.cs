using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class StoryWatcher_Adaptation : IExposable
{
	private float adaptDays;

	private List<Pawn> pawnsJustDownedThisTick = new List<Pawn>();

	private const int UpdateInterval = 30000;

	public float TotalThreatPointsFactor => StorytellerDef.pointsFactorFromAdaptDays.Evaluate(adaptDays);

	public float AdaptDays => adaptDays;

	private int Population => PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists.Count();

	private StorytellerDef StorytellerDef => Find.Storyteller.def;

	public void Notify_PawnEvent(Pawn p, AdaptationEvent ev, DamageInfo? dinfo = null)
	{
		if (!p.RaceProps.Humanlike || !p.IsColonist || p.IsPrisoner)
		{
			return;
		}
		if (ev == AdaptationEvent.Downed)
		{
			if (dinfo.HasValue && dinfo.Value.Def.ExternalViolenceFor(p))
			{
				pawnsJustDownedThisTick.Add(p);
			}
		}
		else
		{
			ResolvePawnEvent(p, ev);
		}
	}

	private void ResolvePawnEvent(Pawn p, AdaptationEvent ev)
	{
		float num;
		if (ev == AdaptationEvent.Downed)
		{
			num = StorytellerDef.adaptDaysLossFromColonistViolentlyDownedByPopulation.Evaluate(Population);
		}
		else
		{
			if (pawnsJustDownedThisTick.Contains(p))
			{
				pawnsJustDownedThisTick.Remove(p);
			}
			int num2 = Population - 1;
			num = StorytellerDef.adaptDaysLossFromColonistLostByPostPopulation.Evaluate(num2);
		}
		if (DebugViewSettings.writeStoryteller)
		{
			Log.Message("Adaptation event: " + p?.ToString() + " " + ev.ToString() + ". Loss: " + num.ToString("F1") + " from " + adaptDays.ToString("F1"));
		}
		adaptDays = Mathf.Max(StorytellerDef.adaptDaysMin, adaptDays - num);
	}

	public void AdaptationWatcherTick()
	{
		for (int i = 0; i < pawnsJustDownedThisTick.Count; i++)
		{
			ResolvePawnEvent(pawnsJustDownedThisTick[i], AdaptationEvent.Downed);
		}
		pawnsJustDownedThisTick.Clear();
		if (Find.TickManager.TicksGame % 30000 == 0 && (!(adaptDays >= 0f) || !((float)GenDate.DaysPassedSinceSettle < StorytellerDef.adaptDaysGameStartGraceDays)))
		{
			float num = 0.5f * StorytellerDef.adaptDaysGrowthRateCurve.Evaluate(adaptDays);
			if (adaptDays > 0f)
			{
				num *= Find.Storyteller.difficulty.adaptationGrowthRateFactorOverZero;
			}
			adaptDays += num;
			adaptDays = Mathf.Min(adaptDays, StorytellerDef.adaptDaysMax);
		}
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref adaptDays, "adaptDays", 0f);
	}

	public void Debug_OffsetAdaptDays(float days)
	{
		adaptDays += days;
	}

	public void ResetAdaptDays()
	{
		adaptDays = 0f;
	}
}
