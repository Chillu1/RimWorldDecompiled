using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class UndercaveMapComponent : CustomMapComponent
{
	private const int MinSpawnDistFromGateExit = 10;

	private const float FleshmassCollapseRateExponent = 5f;

	private static readonly IntRange SpawnGroupSize = new IntRange(2, 4);

	private const int SpawnGroupRadius = 3;

	private static readonly SimpleCurve ThreatPointsCurve = new SimpleCurve
	{
		new CurvePoint(100f, 250f),
		new CurvePoint(500f, 800f),
		new CurvePoint(1000f, 1500f),
		new CurvePoint(2000f, 2000f),
		new CurvePoint(8000f, 3000f)
	};

	private static readonly IntRange NumRockCollapsesRange = new IntRange(0, 2);

	private static readonly IntRange CollapsedRocksSizeRange = new IntRange(1, 5);

	private const float CollapsedRocksMinDistanceFromColonist = 10f;

	private const int InitialShakeDurationTicks = 120;

	private const float InitialShakeAmount = 0.2f;

	private const float StageOneShakeAmount = 0.1f;

	private const float StageTwoShakeAmount = 0.2f;

	private static readonly IntRange StageOneNumCollapseEffects = new IntRange(10, 15);

	private static readonly IntRange StageTwoNumCollapseEffects = new IntRange(15, 20);

	private static readonly IntRange FXTriggerDelay = new IntRange(10, 30);

	private static readonly SimpleCurve HoursToShakeMTBTicksCurve = new SimpleCurve
	{
		new CurvePoint(14f, 2500f),
		new CurvePoint(1f, 45f)
	};

	private const float AmbientFXMTB = 60f;

	public PitGate pitGate;

	public PocketMapExit exit;

	private Queue<QueuedCellEffecter> fxQueue = new Queue<QueuedCellEffecter>();

	private Sustainer collapsingSustainer;

	private static EffecterDef[] AmbientEffecters { get; } = new EffecterDef[2]
	{
		EffecterDefOf.UndercaveMapAmbience,
		EffecterDefOf.UndercaveCeilingDebris
	};

	public Map SourceMap => (map.Parent as PocketMapParent)?.sourceMap;

	public UndercaveMapComponent(Map map)
		: base(map)
	{
	}

	public override void MapGenerated()
	{
		pitGate = SourceMap?.listerThings?.ThingsOfDef(ThingDefOf.PitGate).FirstOrDefault() as PitGate;
		exit = map.listerThings.ThingsOfDef(ThingDefOf.CaveExit).FirstOrDefault() as PocketMapExit;
		if (pitGate == null)
		{
			Log.Warning("Pit gate was not found after generating undercave, if this map was created via dev tools you can ignore this");
			return;
		}
		if (exit == null)
		{
			Log.Error("Pit gate exit was not found after generating undercave");
			return;
		}
		float num = pitGate?.pointsMultiplier ?? 1f;
		List<Pawn> fleshbeastsForPoints = FleshbeastUtility.GetFleshbeastsForPoints(ThreatPointsCurve.Evaluate(StorytellerUtility.DefaultThreatPointsNow(SourceMap) * num), map);
		int num2 = 0;
		int randomInRange = SpawnGroupSize.RandomInRange;
		CellFinder.TryFindRandomCell(map, (IntVec3 c) => c.Standable(map) && !c.InHorDistOf(exit.Position, 10f), out var result);
		foreach (Pawn item in fleshbeastsForPoints)
		{
			CellFinder.TryFindRandomCellNear(result, map, 3, (IntVec3 c) => c.Standable(map), out var result2);
			if (result2.IsValid)
			{
				GenSpawn.Spawn(item, result2, map);
			}
			num2++;
			if (num2 >= randomInRange)
			{
				num2 = 0;
				randomInRange = SpawnGroupSize.RandomInRange;
				CellFinder.TryFindRandomCell(map, (IntVec3 c) => c.Standable(map) && !c.InHorDistOf(exit.Position, 10f), out result);
			}
		}
	}

	public override void MapComponentTick()
	{
		if (Find.CurrentMap != map)
		{
			collapsingSustainer?.End();
		}
		if (pitGate == null || Find.CurrentMap != map)
		{
			return;
		}
		if (pitGate.IsCollapsing)
		{
			float mtb = HoursToShakeMTBTicksCurve.Evaluate((float)pitGate.TicksUntilCollapse / 2500f);
			if (pitGate.CollapseStage == 1)
			{
				if (collapsingSustainer == null || collapsingSustainer.Ended)
				{
					collapsingSustainer = SoundDefOf.UndercaveCollapsingStage1.TrySpawnSustainer(SoundInfo.OnCamera(MaintenanceType.PerTick));
				}
				if (Find.CurrentMap == map && Rand.MTBEventOccurs(mtb, 1f, 1f))
				{
					TriggerCollapseFX(0.1f, StageOneNumCollapseEffects.RandomInRange);
				}
			}
			else
			{
				if (collapsingSustainer == null || collapsingSustainer.Ended)
				{
					collapsingSustainer = SoundDefOf.UndercaveCollapsingStage2.TrySpawnSustainer(SoundInfo.OnCamera(MaintenanceType.PerTick));
				}
				if (Find.CurrentMap == map && Rand.MTBEventOccurs(mtb, 1f, 1f))
				{
					TriggerCollapseFX(0.2f, StageTwoNumCollapseEffects.RandomInRange);
				}
			}
			collapsingSustainer.Maintain();
		}
		TriggerAmbientFX();
		ReadFxQueue();
	}

	private void TriggerAmbientFX()
	{
		if (!Rand.MTBEventOccurs(60f, 1f, 1f))
		{
			return;
		}
		if (Find.CameraDriver.ZoomRootSize < 20f && CellFinderLoose.TryGetRandomCellWith((IntVec3 c) => c.Standable(map) && Find.CameraDriver.CurrentViewRect.Contains(c), map, 100, out var result))
		{
			fxQueue.Enqueue(new QueuedCellEffecter(EffecterDefOf.UndercaveMapAmbienceWater, result, 0));
		}
		if (CellFinderLoose.TryGetRandomCellWith((IntVec3 c) => c.Standable(map), map, 100, out var result2) && !result2.Fogged(map) && Find.CameraDriver.CurrentViewRect.Contains(result2))
		{
			EffecterDef edef = AmbientEffecters.RandomElementByWeight((EffecterDef p) => p.randomWeight);
			fxQueue.Enqueue(new QueuedCellEffecter(edef, result2, 0));
		}
	}

	private void TriggerCollapseFX(float shakeAmt, int numDustEffecters)
	{
		Find.CameraDriver.shaker.DoShake(shakeAmt);
		SoundDefOf.UndercaveRumble.PlayOneShotOnCamera(map);
		int randomInRange = NumRockCollapsesRange.RandomInRange;
		for (int i = 0; i < randomInRange; i++)
		{
			if (!CellFinderLoose.TryGetRandomCellWith(delegate(IntVec3 c)
			{
				if (c.GetEdifice(map) != null)
				{
					return false;
				}
				foreach (Pawn item in map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer))
				{
					if (item.Position.InHorDistOf(c, 10f))
					{
						return false;
					}
				}
				return true;
			}, map, 100, out var result))
			{
				continue;
			}
			foreach (IntVec3 item2 in GridShapeMaker.IrregularLump(result, map, CollapsedRocksSizeRange.RandomInRange, (IntVec3 c) => c.GetEdifice(map) == null))
			{
				RoofCollapserImmediate.DropRoofInCells(item2, map);
			}
			EffecterDefOf.UndercaveCeilingDebris.SpawnMaintained(result, map);
		}
		int num = Find.TickManager.TicksGame;
		for (int num2 = 0; num2 < numDustEffecters; num2++)
		{
			if (CellFinderLoose.TryGetRandomCellWith((IntVec3 c) => c.GetEdifice(map) == null, map, 100, out var result2))
			{
				fxQueue.Enqueue(new QueuedCellEffecter(EffecterDefOf.UndercaveCeilingDebris, result2, num));
				num += FXTriggerDelay.RandomInRange;
			}
		}
	}

	public void ReadFxQueue()
	{
		while (fxQueue.Count > 0 && Find.TickManager.TicksGame >= fxQueue.Peek().tick)
		{
			QueuedCellEffecter queuedCellEffecter = fxQueue.Dequeue();
			queuedCellEffecter.effecterDef.SpawnMaintained(queuedCellEffecter.cell, map);
		}
	}

	public void Notify_BeginCollapsing(int collapseDurationTicks)
	{
		map.GetComponent<FleshmassMapComponent>()?.DestroyFleshmass(collapseDurationTicks, 5f, destroyInChunks: true);
		SoundDefOf.UndercaveRumble.PlayOneShotOnCamera(map);
		Find.CameraDriver.shaker.DoShake(0.2f, 120);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref pitGate, "pitGate");
		Scribe_References.Look(ref exit, "exit");
	}
}
