using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld.QuestGen;

public class QuestNode_Root_WorkSite : QuestNode
{
	private struct SiteSpawnCandidate
	{
		public PlanetTile tile;

		public SitePartDef sitePart;
	}

	private enum FactionType
	{
		Temporary,
		Enemy,
		AllyOrNeutral
	}

	private const int SpawnRange = 9;

	private const int MinSpawnDist = 3;

	private const float MinPointsForSurpriseReinforcements = 400f;

	private const float SurpriseReinforcementChance = 0.35f;

	private static readonly SimpleCurve ExistingCampsAppearanceFrequencyMultiplier = new SimpleCurve
	{
		new CurvePoint(0f, 1f),
		new CurvePoint(2f, 1f),
		new CurvePoint(3f, 0.3f),
		new CurvePoint(4f, 0.1f)
	};

	private const float MinPoints = 40f;

	private const string SitePartTag = "WorkSite";

	private const float TemporaryFactionChance = 0.6f;

	private const float EnemyFactionChance = 0.25f;

	private const float AllyFactionChance = 0.15f;

	private static readonly Tuple<float, FactionType>[] FactionChances = new Tuple<float, FactionType>[3]
	{
		new Tuple<float, FactionType>(0.6f, FactionType.Temporary),
		new Tuple<float, FactionType>(0.25f, FactionType.Enemy),
		new Tuple<float, FactionType>(0.15f, FactionType.AllyOrNeutral)
	};

	private static bool AnySpawnCandidate(PlanetTile aroundTile)
	{
		foreach (SiteSpawnCandidate candidate in GetCandidates(aroundTile))
		{
			if (Find.WorldGrid[candidate.tile].PrimaryBiome.campSelectionWeight > 0f)
			{
				return true;
			}
		}
		return false;
	}

	private static SiteSpawnCandidate GetSpawnCandidate(PlanetTile aroundTile)
	{
		List<SiteSpawnCandidate> list = GetCandidates(aroundTile).ToList();
		if (list.Count == 0)
		{
			return default(SiteSpawnCandidate);
		}
		SitePartDef campTypeToGenerate = list.Select((SiteSpawnCandidate tct) => tct.sitePart).Distinct().RandomElement();
		return list.Where((SiteSpawnCandidate tct) => tct.sitePart == campTypeToGenerate).RandomElementByWeightWithFallback((SiteSpawnCandidate t) => Find.WorldGrid[t.tile].PrimaryBiome.campSelectionWeight);
	}

	private static IEnumerable<SiteSpawnCandidate> GetCandidates(PlanetTile aroundTile)
	{
		IEnumerable<SitePartDef> source = DefDatabase<SitePartDef>.AllDefs.Where((SitePartDef def) => def.tags != null && def.tags.Contains("WorkSite") && typeof(SitePartWorker_WorkSite).IsAssignableFrom(def.workerClass));
		List<PlanetTile> potentialTiles = PotentialSiteTiles(aroundTile);
		return source.SelectMany(delegate(SitePartDef sitePart)
		{
			SitePartWorker_WorkSite worker = (SitePartWorker_WorkSite)sitePart.Worker;
			return from t in potentialTiles
				where worker.CanSpawnOn(t)
				select new SiteSpawnCandidate
				{
					tile = t,
					sitePart = sitePart
				};
		});
	}

	public static Site GenerateSite(float points, PlanetTile aroundTile)
	{
		SiteSpawnCandidate spawnCandidate = GetSpawnCandidate(aroundTile);
		SitePartWorker_WorkSite sitePartWorker = (SitePartWorker_WorkSite)spawnCandidate.sitePart.Worker;
		Faction faction = null;
		FactionType item = FactionChances.RandomElementByWeight((Tuple<float, FactionType> e) => e.Item1).Item2;
		QuestPart_Hyperlinks questPart_Hyperlinks = new QuestPart_Hyperlinks();
		QuestGen.quest.AddPart(questPart_Hyperlinks);
		Predicate<Faction> factionValidator = null;
		switch (item)
		{
		case FactionType.Enemy:
			factionValidator = (Faction f) => f.HostileTo(Faction.OfPlayer);
			break;
		case FactionType.AllyOrNeutral:
			factionValidator = (Faction f) => f.AllyOrNeutralTo(Faction.OfPlayer);
			break;
		}
		if (factionValidator != null)
		{
			faction = Find.FactionManager.AllFactionsListForReading.Where((Faction f) => FactionUseable(f) && factionValidator(f)).RandomElementWithFallback();
		}
		if (faction != null)
		{
			questPart_Hyperlinks.factions.Add(faction);
		}
		else
		{
			FactionDef factionDef = DefDatabase<FactionDef>.AllDefsListForReading.Where(FactionDefUseable).RandomElement();
			bool? hidden = true;
			FactionGeneratorParms parms = new FactionGeneratorParms(factionDef, default(IdeoGenerationParms), hidden);
			parms.ideoGenerationParms = new IdeoGenerationParms(parms.factionDef, forceNoExpansionIdeo: false, sitePartWorker.DisallowedPrecepts.ToList());
			List<FactionRelation> list = new List<FactionRelation>();
			foreach (Faction item2 in Find.FactionManager.AllFactionsListForReading)
			{
				if (!item2.def.PermanentlyHostileTo(parms.factionDef))
				{
					if (item2 == Faction.OfPlayer)
					{
						list.Add(new FactionRelation
						{
							other = item2,
							kind = FactionRelationKind.Hostile
						});
					}
					else
					{
						list.Add(new FactionRelation
						{
							other = item2,
							kind = FactionRelationKind.Neutral
						});
					}
				}
			}
			faction = FactionGenerator.NewGeneratedFactionWithRelations(parms, list);
			faction.temporary = true;
			Find.FactionManager.Add(faction);
		}
		Site site = QuestGen_Sites.GenerateSite(new SitePartDefWithParams[1]
		{
			new SitePartDefWithParams(spawnCandidate.sitePart, new SitePartParams
			{
				threatPoints = points
			})
		}, spawnCandidate.tile, faction);
		if (site.parts.Any() && site.parts[0].lootThings.Any())
		{
			questPart_Hyperlinks.thingDefs.Add(site.parts[0].lootThings[0].ThingDef);
		}
		site.desiredThreatPoints = site.ActualThreatPoints;
		return site;
		bool FactionDefUseable(FactionDef def)
		{
			if (def.humanlikeFaction && !def.pawnGroupMakers.NullOrEmpty() && def.pawnGroupMakers.Any((PawnGroupMaker gm) => gm.kindDef == PawnGroupKindDefOf.Settlement))
			{
				return def.pawnGroupMakers.Any((PawnGroupMaker gm) => gm.kindDef == sitePartWorker.WorkerGroupKind);
			}
			return false;
		}
		bool FactionUseable(Faction f)
		{
			if (f.ideos?.PrimaryIdeo != null && sitePartWorker.DisallowedPrecepts.Any((PreceptDef p) => f.ideos.PrimaryIdeo.PreceptsListForReading.Any((Precept precept) => precept.def == p)))
			{
				return false;
			}
			if (!f.def.canGenerateQuestSites)
			{
				return false;
			}
			return FactionDefUseable(f.def);
		}
	}

	public static List<PlanetTile> PotentialSiteTiles(PlanetTile root)
	{
		List<PlanetTile> tiles = new List<PlanetTile>();
		root.Layer.Filler.FloodFill(root, (PlanetTile p) => !Find.World.Impassable(p) && Find.WorldGrid.ApproxDistanceInTiles(p, root) <= 9f, delegate(PlanetTile p)
		{
			if (Find.WorldGrid.ApproxDistanceInTiles(p, root) >= 3f && Find.World.landmarks?[p] == null)
			{
				tiles.Add(p);
			}
		});
		return tiles;
	}

	public static float AppearanceFrequency(Map map)
	{
		float num = 1f;
		float num2 = 0f;
		List<PlanetTile> list = PotentialSiteTiles(map.Tile);
		if (list.Count == 0)
		{
			return 0f;
		}
		if (!AnySpawnCandidate(map.Tile))
		{
			return 0f;
		}
		foreach (PlanetTile item in list)
		{
			num2 += Find.WorldGrid[item].PrimaryBiome.campSelectionWeight;
		}
		num2 /= (float)list.Count;
		num *= num2;
		int num3 = 0;
		foreach (Site site in Find.WorldObjects.Sites)
		{
			if (site.MainSitePartDef.tags != null && site.MainSitePartDef.tags.Contains("WorkSite"))
			{
				num3++;
			}
		}
		num *= ExistingCampsAppearanceFrequencyMultiplier.Evaluate(num3);
		int num4 = map.mapPawns.FreeColonists.Count();
		if (num4 <= 1)
		{
			return 0f;
		}
		if (num4 == 2)
		{
			return num / 2f;
		}
		return num;
	}

	public static float BestAppearanceFrequency()
	{
		float num = 0f;
		foreach (Map map in Find.Maps)
		{
			if (map.IsPlayerHome)
			{
				num = Mathf.Max(num, AppearanceFrequency(map));
			}
		}
		return num;
	}

	protected override void RunInt()
	{
		if (!ModLister.CheckIdeology("Work site"))
		{
			return;
		}
		Quest quest = QuestGen.quest;
		Slate slate = QuestGen.slate;
		QuestGenUtility.RunAdjustPointsForDistantFight();
		float num = slate.Get("points", 0f);
		if (num < 40f)
		{
			num = 40f;
		}
		Map map = Find.Maps.Where((Map m) => m.IsPlayerHome && GetCandidates(m.Tile).Any()).RandomElementByWeight((Map m) => AppearanceFrequency(m));
		slate.Set("map", map);
		Site site = GenerateSite(num, map.Tile);
		quest.SpawnWorldObject(site);
		quest.ReserveFaction(site.Faction);
		int num2 = 1800000;
		quest.WorldObjectTimeout(site, num2);
		quest.Delay(num2, delegate
		{
			QuestGen_End.End(quest, QuestEndOutcome.Fail);
		});
		quest.Message("MessageCampDetected".Translate(site.Named("CAMP"), site.Faction.Named("FACTION")), MessageTypeDefOf.NeutralEvent, getLookTargetsFromSignal: false, null, new LookTargets(site));
		SitePart sitePart = site.parts[0];
		if (!sitePart.things.NullOrEmpty())
		{
			ThingDef def = sitePart.things.First().def;
			int num3 = 0;
			foreach (Thing item2 in (IEnumerable<Thing>)sitePart.things)
			{
				if (item2.def == def)
				{
					num3 += item2.stackCount;
				}
			}
			QuestGen.AddQuestDescriptionRules(new List<Rule>
			{
				new Rule_String("loot", def.label + " x" + num3)
			});
		}
		slate.Set("campSite", site);
		slate.Set("faction", site.Faction);
		slate.Set("timeout", num2);
		string inSignal = QuestGenUtility.HardcodedSignalWithQuestID("campSite.AllEnemiesDefeated");
		string inSignalEnabled = QuestGenUtility.HardcodedSignalWithQuestID("campSite.MapGenerated");
		string inSignal2 = QuestGenUtility.HardcodedSignalWithQuestID("campSite.MapRemoved");
		string inSignal3 = QuestGenUtility.HardcodedSignalWithQuestID("campSite.NoActiveThreats");
		QuestPart_Choice questPart_Choice = quest.RewardChoice();
		QuestPart_Choice.Choice item = new QuestPart_Choice.Choice
		{
			rewards = { (Reward)new Reward_CampLoot() }
		};
		questPart_Choice.choices.Add(item);
		if (num >= 400f)
		{
			quest.SurpriseReinforcements(inSignalEnabled, site, site.Faction, 0.35f);
		}
		quest.Notify_PlayerRaidedSomeone(null, site, inSignal);
		quest.End(QuestEndOutcome.Success, 0, null, inSignal3);
		quest.End(QuestEndOutcome.Fail, 0, null, inSignal2);
		QuestGen.AddQuestDescriptionRules(new List<Rule>
		{
			new Rule_String("siteLabel", site.Label)
		});
	}

	protected override bool TestRunInt(Slate slate)
	{
		if (!Find.Storyteller.difficulty.allowViolentQuests)
		{
			return false;
		}
		QuestGenUtility.TestRunAdjustPointsForDistantFight(slate);
		if (slate.Get("points", 0f) < 40f)
		{
			return false;
		}
		foreach (Map map in Find.Maps)
		{
			if (map.IsPlayerHome && AppearanceFrequency(map) > 0f && GetCandidates(map.Tile).Any())
			{
				return true;
			}
		}
		return false;
	}
}
