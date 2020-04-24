using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public static class SettleUtility
	{
		public static readonly Texture2D SettleCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/Settle");

		public static bool PlayerSettlementsCountLimitReached
		{
			get
			{
				int num = 0;
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					if (maps[i].IsPlayerHome && maps[i].Parent is Settlement)
					{
						num++;
					}
				}
				return num >= Prefs.MaxNumberOfPlayerSettlements;
			}
		}

		public static Settlement AddNewHome(int tile, Faction faction)
		{
			Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
			settlement.Tile = tile;
			settlement.SetFaction(faction);
			settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement);
			Find.WorldObjects.Add(settlement);
			return settlement;
		}
	}
}
