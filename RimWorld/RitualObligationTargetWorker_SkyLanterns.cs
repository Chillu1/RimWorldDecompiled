using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld;

public class RitualObligationTargetWorker_SkyLanterns : RitualObligationTargetWorker_ThingDef
{
	private static readonly HashSet<Pawn> blocked = new HashSet<Pawn>();

	public RitualObligationTargetWorker_SkyLanterns()
	{
	}

	public RitualObligationTargetWorker_SkyLanterns(RitualObligationTargetFilterDef def)
		: base(def)
	{
	}

	public override IEnumerable<TargetInfo> GetTargets(RitualObligation obligation, Map map)
	{
		if (ModLister.CheckIdeology("Skylantern target"))
		{
			List<Thing> ritualSpot = map.listerThings.ThingsOfDef(ThingDefOf.RitualSpot);
			for (int j = 0; j < ritualSpot.Count; j++)
			{
				yield return ritualSpot[j];
			}
			List<Thing> campfire = map.listerThings.ThingsOfDef(ThingDefOf.Campfire);
			for (int j = 0; j < campfire.Count; j++)
			{
				yield return campfire[j];
			}
		}
	}

	protected override RitualTargetUseReport CanUseTargetInternal(TargetInfo target, RitualObligation obligation)
	{
		Thing thing = target.Thing;
		if (thing.def != ThingDefOf.RitualSpot && thing.def != ThingDefOf.Campfire)
		{
			return false;
		}
		Room room = thing.GetRoom();
		if (def.colonistThingsOnly && (thing.Faction == null || !thing.Faction.IsPlayer))
		{
			return false;
		}
		int num = 0;
		foreach (IntVec3 item in GenRadial.RadialCellsAround(target.Cell, def.unroofedCellSearchRadius, useCenter: false))
		{
			if (item.InBounds(target.Map) && !item.Roofed(target.Map) && item.GetRoom(thing.Map) == room)
			{
				num++;
				if (num >= def.minUnroofedCells)
				{
					break;
				}
			}
		}
		if (num < def.minUnroofedCells)
		{
			return "RitualTargetNeedUnroofedCells".Translate(def.minUnroofedCells);
		}
		if (thing.def == ThingDefOf.RitualSpot)
		{
			return true;
		}
		if (thing.def == ThingDefOf.Campfire)
		{
			if (target.Cell.Roofed(target.Map))
			{
				return "RitualTargetCampfireMustBeUnroofed".Translate();
			}
			CompRefuelable compRefuelable = thing.TryGetComp<CompRefuelable>();
			if (compRefuelable != null)
			{
				if (compRefuelable.HasFuel)
				{
					return true;
				}
				return "RitualTargetCampfireNoFuel".Translate();
			}
		}
		return false;
	}

	public override IEnumerable<string> GetTargetInfos(RitualObligation obligation)
	{
		yield return "RitualTargetCampfirePartyInfo".Translate();
		yield return ThingDefOf.RitualSpot.label;
	}

	public override bool ShouldGrayOut(Pawn pawn, ILordJobAssignmentsManager<RitualRole> assignments, out TaggedString reason)
	{
		if (blocked.Contains(pawn) && assignments.PawnParticipating(pawn))
		{
			reason = "RitualTargetWoodInfoWarning".Translate(pawn.Named("PAWN"));
			return true;
		}
		reason = TaggedString.Empty;
		return false;
	}

	public override IEnumerable<string> GetBlockingIssues(TargetInfo target, RitualRoleAssignments assignments)
	{
		Dictionary<Thing, int> dictionary = new Dictionary<Thing, int>();
		List<Thing> list = target.Map.listerThings.ThingsOfDef(ThingDefOf.WoodLog);
		bool flag = true;
		int num = 0;
		blocked.Clear();
		List<Pawn> participants = assignments.Participants;
		foreach (Pawn item in participants)
		{
			int num2 = Math.Max(def.woodPerParticipant - item.inventory.Count(ThingDefOf.WoodLog), 0);
			bool flag2 = false;
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing = list[i];
				if (thing.IsForbidden(item) || !item.CanReserveAndReach(thing, PathEndMode.Touch, item.NormalMaxDanger()))
				{
					continue;
				}
				int num3 = thing.stackCount;
				if (dictionary.TryGetValue(thing, out var value))
				{
					num3 = Math.Max(num3 - value, 0);
				}
				if (num3 >= num2)
				{
					if (dictionary.ContainsKey(thing))
					{
						dictionary[thing] += num2;
					}
					else
					{
						dictionary[thing] = num2;
					}
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				blocked.Add(item);
				flag = false;
				num++;
			}
		}
		if (!flag)
		{
			TaggedString taggedString = "RitualTargetWoodInfo".Translate(def.woodPerParticipant, def.woodPerParticipant * participants.Count());
			if (num == 1)
			{
				taggedString += string.Format(" {0}", "RitualTargetWoodInfoAppend".Translate(num));
			}
			else
			{
				taggedString += string.Format(" {0}", "RitualTargetWoodInfoAppendMultiple".Translate(num));
			}
			yield return taggedString;
		}
	}
}
