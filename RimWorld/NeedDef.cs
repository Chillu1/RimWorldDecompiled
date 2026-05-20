using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class NeedDef : Def
{
	public Type needClass;

	public Intelligence minIntelligence;

	public bool colonistAndPrisonersOnly;

	public bool playerMechsOnly;

	public bool colonistsOnly;

	public bool slavesOnly;

	public bool onlyIfCausedByHediff;

	public bool onlyIfCausedByGene;

	public bool onlyIfCausedByTrait;

	public bool onlyIfCausedByIdeo;

	public bool neverOnPrisoner;

	public bool neverOnSlave;

	public List<RoyalTitleDef> titleRequiredAny;

	public List<HediffDef> hediffRequiredAny;

	public List<PreceptDef> nullifyingPrecepts;

	public List<CompProperties> requiredComps;

	public DevelopmentalStage developmentalStageFilter = DevelopmentalStage.Child | DevelopmentalStage.Adult;

	public bool showOnNeedList = true;

	public float baseLevel = 0.5f;

	public bool major;

	public int listPriority;

	[NoTranslate]
	public string tutorHighlightTag;

	public bool showForCaravanMembers;

	public bool scaleBar;

	public bool showUnitTicks;

	public float fallPerDay = 0.5f;

	public float seekerRisePerHour;

	public float seekerFallPerHour;

	public bool freezeWhileSleeping;

	public bool freezeInMentalState;

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (description.NullOrEmpty() && showOnNeedList)
		{
			yield return "no description";
		}
		if (needClass == null)
		{
			yield return "needClass is null";
		}
		if (needClass == typeof(Need_Seeker) && (seekerRisePerHour == 0f || seekerFallPerHour == 0f))
		{
			yield return "seeker rise/fall rates not set";
		}
	}
}
