using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse.AI.Group;

public class PsychicRitualToil_VoidProvocation : PsychicRitualToil
{
	private PsychicRitualRoleDef invokerRole;

	private SimpleCurve psychicShockChanceFromQualityCurve;

	protected PsychicRitualToil_VoidProvocation()
	{
	}

	public PsychicRitualToil_VoidProvocation(PsychicRitualRoleDef invokerRole, SimpleCurve psychicShockChanceFromQualityCurve)
	{
		this.invokerRole = invokerRole;
		this.psychicShockChanceFromQualityCurve = psychicShockChanceFromQualityCurve;
	}

	public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		Pawn pawn = psychicRitual.assignments.FirstAssignedPawn(invokerRole);
		if (pawn != null)
		{
			ApplyOutcome(psychicRitual, pawn);
		}
	}

	private void ApplyOutcome(PsychicRitual psychicRitual, Pawn invoker)
	{
		PsychicRitualDef_VoidProvocation psychicRitualDef_VoidProvocation = (PsychicRitualDef_VoidProvocation)psychicRitual.def;
		Map map = psychicRitual.Map;
		List<IncidentDef> list = new List<IncidentDef>();
		bool flag = false;
		foreach (EntityCategoryDef item in DefDatabase<EntityCategoryDef>.AllDefs.OrderBy((EntityCategoryDef x) => x.listOrder))
		{
			foreach (EntityCodexEntryDef allDef in DefDatabase<EntityCodexEntryDef>.AllDefs)
			{
				if (allDef.category != item || allDef.provocationIncidents.NullOrEmpty() || allDef.Discovered)
				{
					continue;
				}
				foreach (IncidentDef provocationIncident in allDef.provocationIncidents)
				{
					IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(provocationIncident.category, map);
					incidentParms.bypassStorytellerSettings = true;
					if (provocationIncident.Worker.CanFireNow(incidentParms))
					{
						list.Add(provocationIncident);
						flag = true;
					}
				}
			}
			if (flag)
			{
				break;
			}
		}
		if (!list.Any())
		{
			foreach (EntityCodexEntryDef allDef2 in DefDatabase<EntityCodexEntryDef>.AllDefs)
			{
				if (allDef2.provocationIncidents.NullOrEmpty())
				{
					continue;
				}
				foreach (IncidentDef provocationIncident2 in allDef2.provocationIncidents)
				{
					IncidentParms incidentParms2 = StorytellerUtility.DefaultParmsNow(provocationIncident2.category, map);
					incidentParms2.bypassStorytellerSettings = true;
					if (provocationIncident2.Worker.CanFireNow(incidentParms2))
					{
						list.Add(provocationIncident2);
					}
				}
			}
		}
		bool flag2;
		if (list.TryRandomElement(out var result))
		{
			flag2 = true;
			IncidentParms incidentParms3 = StorytellerUtility.DefaultParmsNow(result.category, map);
			incidentParms3.bypassStorytellerSettings = true;
			Find.Storyteller.incidentQueue.Add(result, Find.TickManager.TicksGame + Mathf.RoundToInt(psychicRitualDef_VoidProvocation.incidentDelayHoursRange.RandomInRange * 2500f), incidentParms3);
		}
		else
		{
			flag2 = false;
		}
		TaggedString text = "VoidProvocationCompletedText".Translate(invoker.Named("INVOKER"), psychicRitual.def.Named("RITUAL")) + "\n\n" + (flag2 ? "VoidProvocationSucceeded" : "VoidProvocationFailed").Translate();
		if (flag2 && Rand.Chance(psychicRitualDef_VoidProvocation.psychicShockChanceFromQualityCurve.Evaluate(psychicRitual.PowerPercent)))
		{
			text += "\n\n" + "VoidProvocationDarkPsychicShock".Translate(invoker.Named("INVOKER"));
			Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.DarkPsychicShock, invoker);
			int duration = Mathf.RoundToInt(psychicRitualDef_VoidProvocation.darkPsychicShockDurarionHoursRange.RandomInRange * 2500f);
			hediff.TryGetComp<HediffComp_Disappears>()?.SetDuration(duration);
			invoker.health.AddHediff(hediff);
		}
		Find.LetterStack.ReceiveLetter("PsychicRitualCompleteLabel".Translate(psychicRitual.def.label).CapitalizeFirst(), text, LetterDefOf.NeutralEvent);
		foreach (Pawn item2 in PawnsFinder.AllMaps_FreeColonistsSpawned)
		{
			if (item2.needs.mood.thoughts.memories.NumMemoriesOfDef(ThoughtDefOf.VoidCuriosity) > 0)
			{
				item2.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.VoidCuriosity);
				item2.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.VoidCuriositySatisfied);
			}
		}
		Find.Anomaly.hasPerformedVoidProvocation = true;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref invokerRole, "invokerRole");
		Scribe_Deep.Look(ref psychicShockChanceFromQualityCurve, "psychicShockChanceFromQualityCurve");
	}
}
