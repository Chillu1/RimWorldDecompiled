using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class PsychicRitualUtility
{
	private static List<SkillRecord> tmpCandidateSkills = new List<SkillRecord>();

	public static IEnumerable<Thing> GetPsychicRitualSpotsAffectedByThing(Map map, CellRect thingRect)
	{
		foreach (Thing item in map.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.PsychicRitualSpot)))
		{
			CompPsychicRitualSpot compPsychicRitualSpot = item.TryGetComp<CompPsychicRitualSpot>();
			if (compPsychicRitualSpot != null && compPsychicRitualSpot.OccupiedCells.Overlaps(thingRect.Cells))
			{
				yield return item;
			}
		}
	}

	public static void DrawPsychicRitualSpotsAffectedByThingOverlay(Map map, ThingDef def, IntVec3 pos, Rot4 rotation)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return;
		}
		int num = 0;
		CellRect thingRect = GenAdj.OccupiedRect(pos, rotation, def.size);
		foreach (Thing item in GetPsychicRitualSpotsAffectedByThing(map, thingRect))
		{
			if (num++ > 10)
			{
				break;
			}
			GenDraw.DrawLineBetween(GenThing.TrueCenter(pos, rotation, def.size, def.Altitude), item.TrueCenter(), SimpleColor.Red);
		}
	}

	public static SkillDef GetPhilophagySkillAndXpTransfer(Pawn invoker, Pawn target, float xpTransferPercent, out float xpTransfer)
	{
		IOrderedEnumerable<SkillRecord> orderedEnumerable = from s in target.skills.skills
			where !s.TotallyDisabled && !invoker.skills.GetSkill(s.def).TotallyDisabled
			orderby s.XpTotalEarned descending
			select s;
		if (!orderedEnumerable.Any())
		{
			xpTransfer = 0f;
			return null;
		}
		tmpCandidateSkills.Clear();
		SkillRecord skillRecord = orderedEnumerable.First();
		foreach (SkillRecord item in orderedEnumerable)
		{
			if (item == skillRecord)
			{
				tmpCandidateSkills.Add(item);
			}
			else if (Mathf.Abs(item.XpTotalEarned - skillRecord.XpTotalEarned) < 0.01f)
			{
				tmpCandidateSkills.Add(item);
			}
		}
		SkillDef def = tmpCandidateSkills[0].def;
		foreach (SkillRecord tmpCandidateSkill in tmpCandidateSkills)
		{
			if (invoker.skills.GetSkill(def).XpTotalEarned < invoker.skills.GetSkill(tmpCandidateSkill.def).XpTotalEarned)
			{
				def = tmpCandidateSkill.def;
			}
		}
		float xpTotalEarned = target.skills.GetSkill(def).XpTotalEarned;
		xpTransfer = xpTotalEarned * xpTransferPercent;
		return def;
	}

	public static void RegisterAsExecutionIfPrisoner(Pawn target, Pawn invoker)
	{
		if (target.IsPrisonerOfColony)
		{
			ThoughtUtility.GiveThoughtsForPawnExecuted(target, invoker, PawnExecutionKind.GenericBrutal);
			TaleRecorder.RecordTale(TaleDefOf.ExecutedPrisoner, invoker, target);
		}
	}

	public static void AddPsychicRitualGuiltToPawns(PsychicRitualDef ritualDef, IEnumerable<Pawn> pawns)
	{
		foreach (Pawn pawn in pawns)
		{
			Thought_PsychicRitualGuilt thought_PsychicRitualGuilt = (Thought_PsychicRitualGuilt)ThoughtMaker.MakeThought(ThoughtDefOf.PsychicRitualGuilt);
			thought_PsychicRitualGuilt.ritualDef = ritualDef;
			pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(thought_PsychicRitualGuilt);
		}
	}
}
