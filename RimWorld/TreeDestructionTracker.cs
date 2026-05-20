using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class TreeDestructionTracker : IExposable
{
	private Map map;

	private List<int> playerTreeDestructionTicks = new List<int>();

	private const int DestructionsExpireInTicks = 900000;

	public int PlayerResponsibleTreeDestructionCount
	{
		get
		{
			DiscardOldEvents();
			return playerTreeDestructionTicks.Count;
		}
	}

	public TreeDestructionTracker(Map map)
	{
		this.map = map;
	}

	public void Notify_TreeDestroyed(DamageInfo dInfo)
	{
		if (dInfo.Instigator != null && dInfo.Instigator.Faction == Faction.OfPlayer)
		{
			playerTreeDestructionTicks.Add(Find.TickManager.TicksGame);
		}
	}

	public void Notify_TreeCut(Pawn by)
	{
		if (by.Faction == Faction.OfPlayer)
		{
			playerTreeDestructionTicks.Add(Find.TickManager.TicksGame);
		}
	}

	private void DiscardOldEvents()
	{
		int num = 0;
		while (num < playerTreeDestructionTicks.Count && Find.TickManager.TicksGame >= playerTreeDestructionTicks[num] + 900000)
		{
			playerTreeDestructionTicks.RemoveAt(num);
			num--;
			num++;
		}
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref playerTreeDestructionTicks, "playerTreeDestructionTicks", LookMode.Value);
	}
}
