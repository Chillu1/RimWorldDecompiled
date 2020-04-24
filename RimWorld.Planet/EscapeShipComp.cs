using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class EscapeShipComp : WorldObjectComp
	{
		public override void CompTick()
		{
			MapParent mapParent = (MapParent)parent;
			if (!mapParent.HasMap)
			{
				return;
			}
			List<Pawn> allPawnsSpawned = mapParent.Map.mapPawns.AllPawnsSpawned;
			bool flag = mapParent.Map.mapPawns.FreeColonistsSpawnedOrInPlayerEjectablePodsCount != 0;
			bool flag2 = false;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				Pawn pawn = allPawnsSpawned[i];
				if (pawn.RaceProps.Humanlike && pawn.HostFaction == null && !pawn.Downed && pawn.Faction != null && pawn.Faction.HostileTo(Faction.OfPlayer))
				{
					flag2 = true;
				}
			}
			if (flag2 && !flag)
			{
				Find.LetterStack.ReceiveLetter("EscapeShipLostLabel".Translate(), "EscapeShipLost".Translate(), LetterDefOf.NegativeEvent);
				parent.Destroy();
			}
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
		{
			foreach (FloatMenuOption floatMenuOption in CaravanArrivalAction_VisitEscapeShip.GetFloatMenuOptions(caravan, (MapParent)parent))
			{
				yield return floatMenuOption;
			}
		}
	}
}
