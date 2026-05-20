using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class QuestPart_Notify_PlayerRaidedSomeone : QuestPart
	{
		public string inSignal;

		public Map getRaidersFromMap;

		public MapParent getRaidersFromMapParent;

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			if (!(signal.tag == inSignal))
			{
				return;
			}
			IEnumerable<Pawn> enumerable = null;
			if (getRaidersFromMap != null)
			{
				enumerable = getRaidersFromMap.mapPawns.FreeColonistsSpawned;
			}
			if (getRaidersFromMapParent != null)
			{
				Map map = getRaidersFromMapParent.Map;
				if (map == null)
				{
					Log.Error("Cannot resolve map for QuestPart_NotifyPlayerRaidedSomeone.getRaidersFromWorldObjectMap!");
					return;
				}
				enumerable = map.mapPawns.FreeColonistsSpawned;
			}
			if (enumerable == null)
			{
				Log.Error("No raiders could be determined to notify ideoligons!");
			}
			else
			{
				IdeoUtility.Notify_PlayerRaidedSomeone(enumerable);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_References.Look(ref getRaidersFromMap, "getRaidersFromMap");
			Scribe_References.Look(ref getRaidersFromMapParent, "getRaidersFromMapParent");
		}
	}
}
