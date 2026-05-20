using Verse;

namespace RimWorld;

public class CompCerebexCore_Destroyed : ThingComp
{
	private int hiveDestroyedTick = -1;

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		if (!respawningAfterLoad)
		{
			hiveDestroyedTick = Find.TickManager.TicksGame;
		}
	}

	public override void CompTick()
	{
		if (hiveDestroyedTick >= 0 && Find.TickManager.TicksGame - hiveDestroyedTick == 60)
		{
			DoGoodwillForDestroyingCore();
			hiveDestroyedTick = -1;
		}
	}

	private void DoGoodwillForDestroyingCore()
	{
		foreach (Faction item in Find.FactionManager.AllFactionsVisible)
		{
			if (item != Faction.OfPlayer && item.def.humanlikeFaction && !item.def.permanentEnemy)
			{
				item.TryAffectGoodwillWith(Faction.OfPlayer, 50, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.DestroyedMechhive);
			}
		}
	}

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref hiveDestroyedTick, "hiveDestroyedTick", -1);
	}
}
