using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class Alert_CultistPsychicRitual : Alert_Critical
{
	private Lord ritualLord;

	private PsychicRitualToil_InvokeHorax invokeToil;

	private GlobalTargetInfo invoker;

	private PsychicRitual CurPsychicRitual
	{
		get
		{
			if (ritualLord.CurLordToil is LordToil_PsychicRitual lordToil_PsychicRitual)
			{
				return lordToil_PsychicRitual.RitualData.psychicRitual;
			}
			return null;
		}
	}

	protected override bool DoMessage => false;

	public Alert_CultistPsychicRitual()
	{
		defaultLabel = "Alert_CultistPsychicRitual".Translate();
		requireAnomaly = true;
	}

	public override string GetLabel()
	{
		return defaultLabel + ": " + invokeToil.TicksLeft.ToStringTicksToPeriodVerbose().CapitalizeFirst();
	}

	public override TaggedString GetExplanation()
	{
		Pawn pawn = invoker.Pawn;
		if (pawn == null)
		{
			return TaggedString.Empty;
		}
		return "Alert_CultistPsychicRitualDesc".Translate(CurPsychicRitual.def.Named("RITUAL"), pawn.Named("INVOKER")).CapitalizeFirst();
	}

	public override AlertReport GetReport()
	{
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			List<Lord> lords = maps[i].lordManager.lords;
			for (int j = 0; j < lords.Count; j++)
			{
				Lord lord = lords[j];
				if (lord.faction == Faction.OfHoraxCult && lord.CurLordToil is LordToil_PsychicRitual lordToil_PsychicRitual && !lordToil_PsychicRitual.RitualData.done && lordToil_PsychicRitual.RitualData.CurPsychicRitualToil is PsychicRitualToil_InvokeHorax { invokerRole: not null } psychicRitualToil_InvokeHorax)
				{
					Pawn pawn = lordToil_PsychicRitual.RitualData.psychicRitual.assignments?.FirstAssignedPawn(psychicRitualToil_InvokeHorax.invokerRole);
					if (pawn != null)
					{
						ritualLord = lord;
						invokeToil = psychicRitualToil_InvokeHorax;
						invoker = pawn;
						return AlertReport.CulpritIs(invoker);
					}
				}
			}
		}
		ritualLord = null;
		invokeToil = null;
		return false;
	}
}
