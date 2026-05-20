using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class ThinkNode_ConditionalNPCCanSelfTendNow : ThinkNode_Conditional
{
	protected override bool Satisfied(Pawn pawn)
	{
		if (!pawn.health.hediffSet.hediffs.Any())
		{
			return false;
		}
		if (!pawn.RaceProps.Humanlike)
		{
			return false;
		}
		if (pawn.Faction == Faction.OfPlayer)
		{
			return false;
		}
		if (pawn.Faction != null && pawn.Faction.HostileTo(Faction.OfPlayer))
		{
			return false;
		}
		if (pawn.HostFaction == Faction.OfPlayer && pawn.guest.IsPrisoner)
		{
			return false;
		}
		if (pawn.InBed() && pawn.guest != null && pawn.Faction != pawn.guest.HostFaction && pawn.CurrentBed().Faction == pawn.guest.HostFaction)
		{
			return false;
		}
		if (Find.TickManager.TicksGame < pawn.mindState.lastHarmTick + 300)
		{
			return false;
		}
		if (Find.TickManager.TicksGame < pawn.mindState.lastEngageTargetTick + 300)
		{
			return false;
		}
		if (Find.TickManager.TicksGame < pawn.mindState.lastSelfTendTick + 300)
		{
			return false;
		}
		Lord lord = pawn.GetLord();
		if (lord != null && lord.CurLordToil != null && !lord.CurLordToil.AllowSelfTend)
		{
			return false;
		}
		if (!pawn.health.HasHediffsNeedingTend())
		{
			return false;
		}
		if (pawn.Faction != null)
		{
			bool foundActiveThreat = false;
			RegionTraverser.BreadthFirstTraverse(pawn.Position, pawn.Map, RegionTraverser.PassAll, delegate(Region x)
			{
				List<Thing> list = x.ListerThings.ThingsInGroup(ThingRequestGroup.AttackTarget);
				for (int i = 0; i < list.Count; i++)
				{
					if (GenHostility.IsActiveThreatTo((IAttackTarget)list[i], pawn.Faction, ignoreHives: false))
					{
						foundActiveThreat = true;
						break;
					}
				}
				return foundActiveThreat;
			}, 5);
			if (foundActiveThreat)
			{
				return false;
			}
		}
		return true;
	}
}
