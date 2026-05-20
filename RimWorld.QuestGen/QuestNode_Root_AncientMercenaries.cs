using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_AncientMercenaries : QuestNode
{
	private const int TimeoutTicks = 1800000;

	protected override bool TestRunInt(Slate slate)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return false;
		}
		return true;
	}

	protected override void RunInt()
	{
		Quest quest = QuestGen.quest;
		Slate slate = QuestGen.slate;
		float num = slate.Get("points", 0f);
		PlanetTile tile = slate.Get<PlanetTile>("siteTile");
		ThingSetMakerParams parms = new ThingSetMakerParams
		{
			makingFaction = Faction.OfAncientsHostile,
			countRange = new IntRange(1, 1),
			totalMarketValueRange = new FloatRange(0.7f, 1.3f) * QuestTuning.PointsToRewardMarketValueCurve.Evaluate(num)
		};
		List<Thing> list = ThingSetMakerDefOf.Reward_UniqueWeapon.root.Generate(parms);
		if (list.Count != 1)
		{
			Log.Error("Expected 1 unique weapon, got " + list.Count);
		}
		ThingWithComps thingWithComps = list.First() as ThingWithComps;
		Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.AncientSoldier_Leader, Faction.OfAncientsHostile);
		if (thingWithComps != null)
		{
			pawn.equipment.DestroyAllEquipment();
			pawn.equipment.AddEquipment(thingWithComps);
		}
		IEnumerable<Pawn> collection = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
		{
			groupKind = PawnGroupKindDefOf.Combat,
			faction = Faction.OfAncientsHostile,
			points = Mathf.Max(num / 2f, Faction.OfAncientsHostile.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat) * 1.05f)
		});
		List<Pawn> list2 = new List<Pawn>();
		list2.Add(pawn);
		list2.AddRange(collection);
		List<TileMutatorDef> source = DefDatabase<TileMutatorDef>.AllDefs.Where((TileMutatorDef x) => x.categories.Contains("AncientStructure")).ToList();
		tile.Tile.AddMutator(source.RandomElement());
		Site site = QuestGen_Sites.GenerateSite(new SitePartDefWithParams[1]
		{
			new SitePartDefWithParams(SitePartDefOf.BanditGang, new SitePartParams())
		}, tile, Faction.OfAncientsHostile, hiddenSitePartsPossible: false, null, WorldObjectDefOf.ClaimableSite);
		quest.SpawnWorldObject(site);
		slate.Set("site", site);
		slate.Set("LEADER", pawn);
		slate.Set("WEAPON", thingWithComps);
		slate.Set("mercenaryList", PawnUtility.PawnKindsToLineList(list2.Select((Pawn p) => p.kindDef), "  - ", ColoredText.ThreatColor));
		slate.Set("atLandmark", site.Tile.Tile.Landmark != null);
		slate.Set("landmarkName", site.Tile.Tile.Landmark?.name ?? "");
		string inSignal = QuestGenUtility.HardcodedSignalWithQuestID("site.MapGenerated");
		string inSignal2 = QuestGenUtility.HardcodedSignalWithQuestID("site.AllEnemiesDefeated");
		string inSignal3 = QuestGenUtility.HardcodedSignalWithQuestID("site.MapRemoved");
		quest.WorldObjectTimeout(site, 1800000);
		quest.Delay(1800000, delegate
		{
			QuestGen_End.End(quest, QuestEndOutcome.Fail);
		});
		quest.AddPart(new QuestPart_SpawnPawnsInStructure(list2, inSignal));
		quest.End(QuestEndOutcome.Success, 0, null, inSignal2);
		quest.End(QuestEndOutcome.Unknown, 0, null, inSignal3);
	}
}
