using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld.Utility;

public static class VoidAwakeningUtility
{
	public enum WaveType
	{
		Fleshbeast,
		Twisted,
		NoctolSightstealer,
		DevourerGorehulk,
		ShamblerGorehulk
	}

	private static readonly IntRange MetalHellReturnDelaySeconds = new IntRange(5, 10);

	private const int DefenderSpawnRadius = 6;

	private static readonly SimpleCurve DefenderPointsByCombatPoints = new SimpleCurve
	{
		new CurvePoint(400f, 400f),
		new CurvePoint(1000f, 800f),
		new CurvePoint(2000f, 1500f),
		new CurvePoint(4000f, 2500f),
		new CurvePoint(10000f, 5000f)
	};

	public static string EncodeWaveType(string signal, WaveType waveType, float pointsFactor)
	{
		string[] obj = new string[5] { signal, ".", null, null, null };
		int num = (int)waveType;
		obj[2] = num.ToString();
		obj[3] = ".";
		obj[4] = pointsFactor.ToString("F2");
		return string.Concat(obj);
	}

	public static void DecodeWaveType(string signal, out WaveType waveType, out float pointsFactor)
	{
		string[] array = signal.Split('.');
		pointsFactor = float.Parse(array[^2] + "." + array[^1]);
		waveType = (WaveType)int.Parse(array[^3]);
	}

	public static void EmbraceTheVoid(Pawn pawn)
	{
		CloseMetalHell(pawn);
		Find.Anomaly.monolith.quest.End(QuestEndOutcome.Success, sendLetter: true, playSound: false);
		Find.Anomaly.SetLevel(MonolithLevelDefOf.Embraced);
		pawn.health.AddHediff(HediffDefOf.VoidTouched);
		pawn.health.AddHediff(HediffDefOf.Inhumanized);
		pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.EmbracedTheVoid);
		TaleRecorder.RecordTale(TaleDefOf.EmbracedTheVoid, pawn);
		if (pawn.health.hediffSet.TryGetHediff(HediffDefOf.DeathRefusal, out var hediff))
		{
			pawn.health.RemoveHediff(hediff);
		}
		(pawn.health.AddHediff(HediffDefOf.DeathRefusal) as Hediff_DeathRefusal).SetUseAmountDirect(4, ignoreLimit: true);
		GameVictoryUtility.ShowCredits("EmbraceTheVoidCredits".Translate(pawn.Named("PAWN")), null);
		Find.Anomaly.metalHellReturnLetterText = "EmbracedVoidText".Translate(pawn.Named("PAWN"));
	}

	public static void DisruptTheLink(Pawn pawn)
	{
		CloseMetalHell(pawn);
		Find.Anomaly.monolith.quest.End(QuestEndOutcome.Success, sendLetter: true, playSound: false);
		Find.Anomaly.SetLevel(MonolithLevelDefOf.Disrupted);
		pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.ClosedTheVoid);
		TaleRecorder.RecordTale(TaleDefOf.ClosedTheVoid, pawn);
		foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction)
		{
			if (item.Inhumanized())
			{
				item.Rehumanize();
			}
			item.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.VoidClosed);
			item.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.ClosedTheVoidOpinion, pawn);
		}
		Find.Anomaly.monolith.Collapse();
		GameVictoryUtility.ShowCredits("DisruptTheLinkCredits".Translate(pawn.Named("PAWN")), null);
		Find.Anomaly.metalHellReturnLetterText = "DisruptedLinkText".Translate(pawn.Named("PAWN"));
	}

	private static void CloseMetalHell(Pawn pawn)
	{
		Map mapHeld = pawn.MapHeld;
		if (mapHeld == null || !mapHeld.IsPocketMap)
		{
			return;
		}
		Find.Anomaly.voidNodeActivator = pawn;
		foreach (Pawn item in mapHeld.mapPawns.AllPawns.ToList())
		{
			item.DeSpawn();
			Find.Anomaly.metalHellPawns.Add(item);
		}
		Find.Anomaly.metalHellClosedTick = Find.TickManager.TicksGame;
		Find.Anomaly.metalHellReturnTick = Find.TickManager.TicksGame + MetalHellReturnDelaySeconds.RandomInRange * 60;
		PocketMapUtility.DestroyPocketMap(mapHeld);
	}

	public static void SpawnMetalhorrorDefenders(Thing structure)
	{
		float a = DefenderPointsByCombatPoints.Evaluate(StorytellerUtility.DefaultThreatPointsNow(structure.Map));
		a = Mathf.Max(a, Faction.OfEntities.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Metalhorrors) * 1.05f);
		List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
		{
			groupKind = PawnGroupKindDefOf.Metalhorrors,
			points = a,
			faction = Faction.OfEntities
		}).ToList();
		foreach (Pawn item in list)
		{
			if (CellFinder.TryFindRandomCellNear(structure.Position, structure.Map, 6, (IntVec3 c) => c.Walkable(structure.Map) && !c.Fogged(structure.Map), out var result))
			{
				GenSpawn.Spawn(item, result, structure.Map);
				item.GetLord()?.RemovePawn(item);
			}
		}
		list.RemoveAll((Pawn p) => !p.Spawned);
		LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_VoidAwakeningDefendStructure(structure.Position, 35f), structure.Map, list);
	}

	public static void KillAllFreeEntities(Map map)
	{
		foreach (Pawn item in map.mapPawns.PawnsInFaction(Faction.OfEntities))
		{
			if (!item.IsEntity)
			{
				continue;
			}
			CompHoldingPlatformTarget compHoldingPlatformTarget = item.TryGetComp<CompHoldingPlatformTarget>();
			if (compHoldingPlatformTarget == null || !compHoldingPlatformTarget.CurrentlyHeldOnPlatform)
			{
				BodyPartRecord brain = item.health.hediffSet.GetBrain();
				if (brain != null)
				{
					item.TakeDamage(new DamageInfo(DamageDefOf.Burn, 99999f, 999f, -1f, null, brain));
				}
				else
				{
					item.Kill(null, null);
				}
			}
		}
	}

	public static void SpawnVoidMetalAround(Thing thing, int size, int numMasses, bool withSkipEffects = false, CellRect? avoidRect = null)
	{
		List<IntVec3> list = GridShapeMaker.IrregularLump(thing.Position, thing.Map, size).ToList();
		list.Shuffle();
		int num = 0;
		foreach (IntVec3 item in list)
		{
			if (!item.InBounds(thing.Map))
			{
				continue;
			}
			thing.Map.terrainGrid.SetTerrain(item, TerrainDefOf.Voidmetal);
			thing.Map.snowGrid.SetDepth(item, 0f);
			thing.Map.sandGrid?.SetDepth(item, 0f);
			if (num != numMasses && (!avoidRect.HasValue || !avoidRect.GetValueOrDefault().Contains(item)) && item.Standable(thing.Map) && Rand.Chance(0.1f))
			{
				Thing thing2 = ThingMaker.MakeThing(ThingDefOf.VoidmetalMassSmall);
				if (thing2.Faction != thing.Faction)
				{
					thing2.SetFaction(thing.Faction);
				}
				GenSpawn.Spawn(thing2, item, thing.Map);
				if (withSkipEffects)
				{
					EffecterDefOf.Skip_EntryNoDelay.Spawn(thing2, thing.Map).Cleanup();
				}
				num++;
			}
		}
	}
}
