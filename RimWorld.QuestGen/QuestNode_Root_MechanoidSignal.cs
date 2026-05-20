using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_MechanoidSignal : QuestNode
{
	private const int SkyfallerDelayTicks = 300;

	private static readonly IntRange ChunkCountRange = new IntRange(4, 6);

	private static readonly IntRange DeadMechCountPerChunk = new IntRange(0, 2);

	private static readonly IntRange GravlitePanelCountTotal = new IntRange(350, 375);

	protected override void RunInt()
	{
		Quest quest = QuestGen.quest;
		Map map = QuestGen_Get.GetMap();
		string dropPodsSpawnedSignal = QuestGenUtility.HardcodedSignalWithQuestID("dropPodsSpawned");
		Thing gravEngine = ThingMaker.MakeThing(ThingDefOf.GravEngine);
		string inSignal = QuestGenUtility.HardcodedSignalWithQuestID("gravEngine.Inspected");
		QuestUtility.AddQuestTag(gravEngine, QuestGenUtility.HardcodedTargetQuestTagWithQuestID("gravEngine"));
		quest.Delay(300, delegate
		{
			List<IntVec3> spots = new List<IntVec3>();
			List<PawnKindDef> mechTypes = new List<PawnKindDef>
			{
				PawnKindDefOf.Mech_Pikeman,
				PawnKindDefOf.Mech_Scyther
			};
			int num = ChunkCountRange.RandomInRange + 1;
			int gravlitePanelCount = GravlitePanelCountTotal.RandomInRange / (num - 1);
			CellFinder.TryFindRandomCell(map, (IntVec3 c) => DropCellFinder.IsGoodDropSpot(c, map, allowFogged: false, canRoofPunch: false) && CanLandHere(c, map, engine: true, spots), out var result);
			int i;
			for (i = 0; i < num; i++)
			{
				Skyfaller skyfaller = SkyfallerMaker.MakeSkyfaller(ThingDefOf.ShipChunkIncoming_SmallExplosion, ChunkContents(quest, gravEngine, i, mechTypes, gravlitePanelCount));
				skyfaller.contentsCanOverlap = false;
				skyfaller.moveAside = true;
				QuestPart_SpawnThing questPart_SpawnThing = new QuestPart_SpawnThing
				{
					thing = skyfaller,
					mapParentOfPawn = map.mapPawns.FreeColonistsSpawned.RandomElement(),
					inSignal = QuestGen.slate.Get<string>("inSignal"),
					cell = CellFinder.RandomClosewalkCellNear(result, map, 15, (IntVec3 x) => CanLandHere(x, map, i == 0, spots)),
					questLookTarget = false
				};
				spots.Add(questPart_SpawnThing.cell);
				quest.AddPart(questPart_SpawnThing);
			}
			quest.Letter(LetterDefOf.PositiveEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, Gen.YieldSingle(gravEngine), filterDeadPawnsFromLookTargets: false, "[gravEngineSpawnedLetterText]", null, "[gravEngineSpawnedLetterLabel]");
			quest.SignalPass(null, null, dropPodsSpawnedSignal);
		});
		quest.LookTargets(Gen.YieldSingle(new GlobalTargetInfo(gravEngine)));
		QuestPart_Choice questPart_Choice = quest.RewardChoice();
		QuestPart_Choice.Choice item = new QuestPart_Choice.Choice
		{
			rewards = { (Reward)new Reward_DefinedThingDef
			{
				thingDef = ThingDefOf.GravEngine
			} }
		};
		questPart_Choice.choices.Add(item);
		quest.End(QuestEndOutcome.Success, 0, null, inSignal);
	}

	private bool CanLandHere(IntVec3 cell, Map map, bool engine, List<IntVec3> spots)
	{
		for (int i = 0; i < spots.Count; i++)
		{
			if (cell.InHorDistOf(spots[i], 3f))
			{
				return false;
			}
		}
		if (engine)
		{
			IntVec2 size = ThingDefOf.GravEngine.Size;
			foreach (IntVec3 item in new CellRect(cell.x, cell.z, size.x, size.z))
			{
				if (!item.InBounds(map))
				{
					return false;
				}
				if (!DropCellFinder.CanPhysicallyDropInto(item, map, canRoofPunch: false, allowedIndoors: false))
				{
					return false;
				}
				if (!GenConstruct.CanBuildOnTerrain(ThingDefOf.GravEngine, item, map, ThingDefOf.GravEngine.defaultPlacingRot))
				{
					return false;
				}
			}
		}
		return true;
	}

	private List<Thing> ChunkContents(Quest quest, Thing gravEngine, int index, List<PawnKindDef> mechTypes, int gravlitePanelCount)
	{
		List<Thing> list = new List<Thing>();
		if (index == 0)
		{
			list.Add(gravEngine);
			return list;
		}
		list.Add(ThingMaker.MakeThing(ThingDefOf.ShipChunk_Mech));
		int randomInRange = DeadMechCountPerChunk.RandomInRange;
		for (int i = 0; i < randomInRange; i++)
		{
			Corpse corpse = quest.GeneratePawn(new PawnGenerationRequest(mechTypes.RandomElement(), Faction.OfMechanoids, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: true))?.Corpse;
			if (corpse != null)
			{
				corpse.SetForbidden(value: true, warnOnFail: false);
				list.Add(corpse);
			}
		}
		Thing thing = ThingMaker.MakeThing(ThingDefOf.GravlitePanel);
		thing.stackCount = gravlitePanelCount;
		thing.SetForbidden(value: true, warnOnFail: false);
		list.Add(thing);
		return list;
	}

	protected override bool TestRunInt(Slate slate)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return false;
		}
		if (QuestGen_Get.GetMap() == null)
		{
			return false;
		}
		if (GravshipUtility.PlayerHasGravEngine())
		{
			return false;
		}
		return true;
	}
}
