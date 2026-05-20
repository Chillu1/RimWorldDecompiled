using System.Collections.Generic;
using RimWorld.QuestGen;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class BossgroupWorker
{
	public const int TimeBetweenAllBossgroups = 120000;

	public BossgroupDef def;

	public virtual AcceptanceReport CanResolve(Pawn caller)
	{
		int lastBossgroupCalled = Find.BossgroupManager.lastBossgroupCalled;
		int num = Find.TickManager.TicksGame - lastBossgroupCalled;
		if (num < 120000)
		{
			return "BossgroupAvailableIn".Translate((120000 - num).ToStringTicksToPeriod());
		}
		PawnKindDef pendingBossgroup = CallBossgroupUtility.GetPendingBossgroup();
		if (pendingBossgroup != null)
		{
			return "BossgroupIncoming".Translate(pendingBossgroup.label);
		}
		return true;
	}

	public virtual AcceptanceReport ShouldSummonNow(Map map)
	{
		List<Lord> lords = map.lordManager.lords;
		for (int i = 0; i < lords.Count; i++)
		{
			LordJob lordJob = lords[i].LordJob;
			if (lordJob is LordJob_TradeWithColony || lordJob is LordJob_BestowingCeremony || lordJob is LordJob_VisitColony)
			{
				return false;
			}
		}
		return true;
	}

	public virtual void Resolve(Map map, int wave)
	{
		Find.BossgroupManager.Notify_BossgroupCalled(def);
		Slate slate = new Slate();
		slate.Set("bossgroup", def);
		slate.Set("map", map);
		slate.Set("reward", def.rewardDef);
		slate.Set("wave", wave);
		QuestUtility.GenerateQuestAndMakeAvailable(def.quest, slate);
	}
}
