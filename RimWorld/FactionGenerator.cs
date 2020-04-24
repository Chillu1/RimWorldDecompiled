using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class FactionGenerator
	{
		private const int MinStartVisibleFactions = 5;

		private const int MaxPreferredFactionNameLength = 20;

		private static readonly FloatRange SettlementsPer100kTiles = new FloatRange(75f, 85f);

		public static void GenerateFactionsIntoWorld()
		{
			int i = 0;
			foreach (FactionDef allDef in DefDatabase<FactionDef>.AllDefs)
			{
				for (int j = 0; j < allDef.requiredCountAtGameStart; j++)
				{
					Faction faction = NewGeneratedFaction(allDef);
					Find.FactionManager.Add(faction);
					if (!allDef.hidden)
					{
						i++;
					}
				}
			}
			for (; i < 5; i++)
			{
				Faction faction2 = NewGeneratedFaction(DefDatabase<FactionDef>.AllDefs.Where((FactionDef fa) => fa.canMakeRandomly && Find.FactionManager.AllFactions.Count((Faction f) => f.def == fa) < fa.maxCountAtGameStart).RandomElement());
				Find.World.factionManager.Add(faction2);
			}
			int num = GenMath.RoundRandom((float)Find.WorldGrid.TilesCount / 100000f * SettlementsPer100kTiles.RandomInRange * Find.World.info.overallPopulation.GetScaleFactor());
			num -= Find.WorldObjects.Settlements.Count;
			for (int k = 0; k < num; k++)
			{
				Faction faction3 = Find.World.factionManager.AllFactionsListForReading.Where((Faction x) => !x.def.isPlayer && !x.def.hidden).RandomElementByWeight((Faction x) => x.def.settlementGenerationWeight);
				Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
				settlement.SetFaction(faction3);
				settlement.Tile = TileFinder.RandomSettlementTileFor(faction3);
				settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement);
				Find.WorldObjects.Add(settlement);
			}
		}

		public static void EnsureRequiredEnemies(Faction player)
		{
			foreach (FactionDef facDef in DefDatabase<FactionDef>.AllDefs)
			{
				if (facDef.mustStartOneEnemy && Find.World.factionManager.AllFactions.Any((Faction f) => f.def == facDef) && !Find.World.factionManager.AllFactions.Any((Faction f) => f.def == facDef && f.HostileTo(player)))
				{
					Faction faction = Find.World.factionManager.AllFactions.Where((Faction f) => f.def == facDef).RandomElement();
					int num = faction.GoodwillWith(player);
					faction.TryAffectGoodwillWith(goodwillChange: DiplomacyTuning.ForcedStartingEnemyGoodwillRange.RandomInRange - num, other: player, canSendMessage: false, canSendHostilityLetter: false);
					faction.TrySetRelationKind(player, FactionRelationKind.Hostile, canSendLetter: false);
				}
			}
		}

		public static Faction NewGeneratedFaction(FactionDef facDef)
		{
			Faction faction = new Faction();
			faction.def = facDef;
			faction.loadID = Find.UniqueIDsManager.GetNextFactionID();
			faction.colorFromSpectrum = NewRandomColorFromSpectrum(faction);
			if (!facDef.isPlayer)
			{
				if (facDef.fixedName != null)
				{
					faction.Name = facDef.fixedName;
				}
				else
				{
					string text = "";
					for (int i = 0; i < 10; i++)
					{
						string text2 = NameGenerator.GenerateName(facDef.factionNameMaker, Find.FactionManager.AllFactionsVisible.Select((Faction fac) => fac.Name));
						if (text2.Length <= 20)
						{
							text = text2;
						}
					}
					if (text.NullOrEmpty())
					{
						text = NameGenerator.GenerateName(facDef.factionNameMaker, Find.FactionManager.AllFactionsVisible.Select((Faction fac) => fac.Name));
					}
					faction.Name = text;
				}
			}
			faction.centralMelanin = Rand.Value;
			foreach (Faction item in Find.FactionManager.AllFactionsListForReading)
			{
				faction.TryMakeInitialRelationsWith(item);
			}
			if (!facDef.hidden && !facDef.isPlayer)
			{
				Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
				settlement.SetFaction(faction);
				settlement.Tile = TileFinder.RandomSettlementTileFor(faction);
				settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement);
				Find.WorldObjects.Add(settlement);
			}
			faction.GenerateNewLeader();
			return faction;
		}

		public static float NewRandomColorFromSpectrum(Faction faction)
		{
			float num = -1f;
			float result = 0f;
			for (int i = 0; i < 10; i++)
			{
				float value = Rand.Value;
				float num2 = 1f;
				List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
				for (int j = 0; j < allFactionsListForReading.Count; j++)
				{
					Faction faction2 = allFactionsListForReading[j];
					if (faction2.def == faction.def)
					{
						float num3 = Mathf.Abs(value - faction2.colorFromSpectrum);
						if (num3 < num2)
						{
							num2 = num3;
						}
					}
				}
				if (num2 > num)
				{
					num = num2;
					result = value;
				}
			}
			return result;
		}
	}
}
