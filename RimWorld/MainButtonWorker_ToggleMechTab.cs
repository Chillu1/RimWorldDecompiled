using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class MainButtonWorker_ToggleMechTab : MainButtonWorker_ToggleTab
{
	private const long DisabledCheckInterval = 180L;

	private long lastDisabledCheckTick = -10000000L;

	private int lastDisabledCheckMapId = int.MinValue;

	private bool lastDisabled;

	public override bool Disabled
	{
		get
		{
			if (base.Disabled)
			{
				return true;
			}
			Map currentMap = Find.CurrentMap;
			int num = currentMap?.uniqueID ?? int.MinValue;
			if (GenTicks.TicksGame - lastDisabledCheckTick < 180 && lastDisabledCheckMapId == num)
			{
				return lastDisabled;
			}
			lastDisabledCheckMapId = num;
			lastDisabledCheckTick = GenTicks.TicksGame;
			if (currentMap != null)
			{
				List<Pawn> list = currentMap.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].RaceProps.IsMechanoid)
					{
						lastDisabled = false;
						return false;
					}
				}
				List<Pawn> list2 = currentMap.mapPawns.PawnsInFaction(Faction.OfPlayer);
				for (int j = 0; j < list2.Count; j++)
				{
					if (list2[j].RaceProps.IsMechanoid)
					{
						lastDisabled = false;
						return false;
					}
				}
			}
			lastDisabled = true;
			return true;
		}
	}

	public override bool Visible => !Disabled;
}
