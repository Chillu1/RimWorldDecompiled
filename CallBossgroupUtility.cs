using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

public static class CallBossgroupUtility
{
	private static List<ThingDef> cachedBossCallers;

	public static void TryStartSummonBossgroupJob(BossgroupDef def, Pawn pawn, bool forced = true)
	{
		ThingDef bossgroupCaller = GetBossgroupCaller(def);
		List<Thing> list = pawn.Map.listerThings.ThingsOfDef(bossgroupCaller);
		list.SortBy((Thing t) => t.Position.DistanceToSquared(pawn.Position));
		for (int num = 0; num < list.Count; num++)
		{
			if (pawn.CanReserveAndReach(list[num], PathEndMode.Touch, Danger.Deadly, 1, -1, null, forced) && (bool)list[num].TryGetComp<CompUsable>().CanBeUsedBy(pawn))
			{
				list[num].TryGetComp<CompUsable>().TryStartUseJob(pawn, null, forced);
				break;
			}
		}
	}

	public static AcceptanceReport BossgroupEverCallable(Pawn pawn, BossgroupDef def, bool forced = true)
	{
		if (!pawn.Spawned)
		{
			return false;
		}
		ThingDef bossgroupCaller = GetBossgroupCaller(def);
		List<Thing> list = pawn.Map.listerThings.ThingsOfDef(bossgroupCaller);
		if (list.Count <= 0)
		{
			return "NoSubject".Translate(bossgroupCaller.label);
		}
		bool flag = false;
		for (int i = 0; i < list.Count; i++)
		{
			if (pawn.CanReach(list[i], PathEndMode.InteractionCell, Danger.Deadly))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return "NoReachableBossgroupCaller".Translate(bossgroupCaller.label);
		}
		bool flag2 = false;
		for (int j = 0; j < list.Count; j++)
		{
			if (pawn.CanReserve(list[j], 1, -1, null, forced))
			{
				flag2 = true;
				break;
			}
		}
		if (!flag2)
		{
			return "NoReservableBossgroupCaller".Translate(bossgroupCaller.label);
		}
		if (bossgroupCaller.HasComp(typeof(CompPowerTrader)))
		{
			bool flag3 = false;
			for (int k = 0; k < list.Count; k++)
			{
				if (list[k].TryGetComp<CompPowerTrader>().PowerOn)
				{
					flag3 = true;
					break;
				}
			}
			if (!flag3)
			{
				return "NoPoweredBossgroupCaller".Translate(bossgroupCaller.label);
			}
		}
		return true;
	}

	public static ThingDef GetBossgroupCaller(BossgroupDef def)
	{
		if (cachedBossCallers == null)
		{
			cachedBossCallers = DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.GetCompProperties<CompProperties_Useable_CallBossgroup>() != null).ToList();
		}
		foreach (ThingDef cachedBossCaller in cachedBossCallers)
		{
			CompProperties_Useable_CallBossgroup compProperties = cachedBossCaller.GetCompProperties<CompProperties_Useable_CallBossgroup>();
			if (compProperties != null && compProperties.bossgroupDef == def)
			{
				return cachedBossCaller;
			}
		}
		return null;
	}

	public static PawnKindDef GetPendingBossgroup()
	{
		List<Quest> questsListForReading = Find.QuestManager.QuestsListForReading;
		for (int i = 0; i < questsListForReading.Count; i++)
		{
			if (questsListForReading[i].State != QuestState.Ongoing)
			{
				continue;
			}
			List<QuestPart> partsListForReading = questsListForReading[i].PartsListForReading;
			for (int j = 0; j < partsListForReading.Count; j++)
			{
				if (partsListForReading[j] is QuestPart_Bossgroup questPart_Bossgroup && questPart_Bossgroup.bosses.Count > 0 && questPart_Bossgroup.bosses.All((Pawn p) => !p.Spawned && !p.Dead))
				{
					return questPart_Bossgroup.bosses[0].kindDef;
				}
			}
		}
		return null;
	}
}
