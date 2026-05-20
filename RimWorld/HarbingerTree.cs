using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

public class HarbingerTree : Plant
{
	private const float ConsumingGrowthRateFactor = 1.5f;

	private const int TwistedFleshCountAtMaxGrowth = 30;

	private static readonly FloatRange NutritionForNewTreeRange = new FloatRange(65f, 95f);

	private static readonly CachedTexture CreateCorpseStockpileIcon = new CachedTexture("UI/Icons/CorpseStockpileZone");

	private static readonly SimpleCurve ConsumeRadiusByGrowthCurve = new SimpleCurve
	{
		new CurvePoint(0.15f, 3.9f),
		new CurvePoint(0.85f, 5.9f)
	};

	private float nutrition;

	private float nutritionForNextTree;

	private Dictionary<ThingWithComps, Mote> roots;

	private List<ThingWithComps> rootTargets;

	private Queue<ThingWithComps> deferredDestroy = new Queue<ThingWithComps>();

	private List<Thing> tmpThings = new List<Thing>();

	private List<IntVec3> tmpRadialCells = new List<IntVec3>();

	private bool ConsumableNearby => rootTargets.Count > 0;

	private int BloomLevel => Mathf.Max(Mathf.FloorToInt(Growth / 0.25f - 0.001f), 0);

	public float ConsumeRadius => ConsumeRadiusByGrowthCurve.Evaluate(Growth);

	public override float GrowthRate => base.GrowthRate * GrowthRateFactor_Consuming;

	private float GrowthRateFactor_Consuming
	{
		get
		{
			if (!ConsumableNearby)
			{
				return 1f;
			}
			return 1.5f;
		}
	}

	private IEnumerable<IntVec3> RadialCells => GenRadial.RadialCellsAround(base.Position, ConsumeRadius, useCenter: true);

	public override string GrowthRateCalcDesc
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder(base.GrowthRateCalcDesc);
			if (GrowthRateFactor_Consuming != 1f)
			{
				stringBuilder.AppendInNewLine("StatsReport_MultiplierFor".Translate("FleshConsumption".Translate()) + ": " + GrowthRateFactor_Consuming.ToStringPercent());
			}
			return stringBuilder.ToString();
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref rootTargets, "rootTargets", LookMode.Reference);
		Scribe_Values.Look(ref nutrition, "nutrition", 0f);
		Scribe_Values.Look(ref nutritionForNextTree, "nutritionForNextTree", 0f);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			rootTargets.RemoveAll((ThingWithComps x) => x?.Destroyed ?? true);
		}
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		if (!ModLister.CheckAnomaly("Harbinger tree"))
		{
			Destroy();
			return;
		}
		base.SpawnSetup(map, respawningAfterLoad);
		LongEventHandler.ExecuteWhenFinished(UpdateRoots);
		UpdateBloomingStage();
		if (!respawningAfterLoad && !base.BeingTransportedOnGravship)
		{
			nutritionForNextTree = NutritionForNewTreeRange.RandomInRange;
		}
	}

	public override void TickLong()
	{
		base.TickLong();
		UpdateRoots();
		UpdateBloomingStage();
	}

	private void UpdateRoots()
	{
		if (roots == null)
		{
			roots = new Dictionary<ThingWithComps, Mote>();
		}
		if (rootTargets == null)
		{
			rootTargets = new List<ThingWithComps>();
		}
		tmpRadialCells.Clear();
		tmpRadialCells.AddRange(RadialCells.ToList());
		foreach (ThingWithComps rootTarget in rootTargets)
		{
			TryMakeRoot(rootTarget);
		}
		foreach (IntVec3 tmpRadialCell in tmpRadialCells)
		{
			if (!tmpRadialCell.InBounds(base.Map))
			{
				continue;
			}
			tmpThings.Clear();
			tmpThings.AddRange(tmpRadialCell.GetThingList(base.Map));
			foreach (Thing tmpThing in tmpThings)
			{
				if (tmpThing is ThingWithComps thing && thing.TryGetComp<CompHarbingerTreeConsumable>(out var comp) && comp.CanBeConsumed)
				{
					TryMakeRoot(thing);
				}
			}
		}
		foreach (ThingWithComps rootTarget2 in rootTargets)
		{
			if (rootTarget2.Destroyed || !tmpRadialCells.Contains(rootTarget2.PositionHeld))
			{
				deferredDestroy.Enqueue(rootTarget2);
			}
		}
		while (deferredDestroy.Count > 0)
		{
			ThingWithComps thingWithComps = deferredDestroy.Dequeue();
			if (roots.TryGetValue(thingWithComps, out var value))
			{
				value?.Destroy();
			}
			roots.Remove(thingWithComps);
			rootTargets.Remove(thingWithComps);
		}
	}

	private void TryMakeRoot(ThingWithComps thing)
	{
		if (!roots.ContainsKey(thing))
		{
			float exactRot = 0f;
			if (thing is Corpse corpse)
			{
				exactRot = corpse.InnerPawn.Drawer.renderer.BodyAngle(PawnRenderFlags.None);
			}
			roots[thing] = MoteMaker.MakeStaticMote(thing.Position.ToVector3Shifted(), base.Map, ThingDefOf.Mote_HarbingerTreeRoots, 1f, makeOffscreen: false, exactRot);
		}
		if (!rootTargets.Contains(thing))
		{
			rootTargets.Add(thing);
		}
	}

	public void AddNutrition(float amt)
	{
		if (!(amt <= 0f))
		{
			nutrition += amt;
			if (nutrition >= nutritionForNextTree)
			{
				nutrition = 0f;
				nutritionForNextTree = NutritionForNewTreeRange.RandomInRange;
				SpawnNewTree();
			}
		}
	}

	private void SpawnNewTree()
	{
		if (IncidentWorker_HarbingerTreeSpawn.TryGetHarbingerTreeSpawnCell(base.Map, out var cell, base.Position))
		{
			cell.GetPlant(base.Map)?.Destroy();
			Plant plant = (Plant)ThingMaker.MakeThing(ThingDefOf.Plant_TreeHarbinger);
			plant.Growth = IncidentDefOf.HarbingerTreeSpawn.treeGrowth;
			GenSpawn.Spawn(plant, cell, base.Map);
			Messages.Message("HarbingerTreeSprouted".Translate(), plant, MessageTypeDefOf.NeutralEvent);
			Find.Anomaly.Notify_HarbingerTreeSpawned();
		}
	}

	private void UpdateBloomingStage()
	{
		overrideGraphicIndex = BloomLevel;
		DirtyMapMesh(base.Map);
	}

	public override IEnumerable<ThingDefCountClass> GetAdditionalLeavings(DestroyMode mode)
	{
		foreach (ThingDefCountClass additionalLeaving in base.GetAdditionalLeavings(mode))
		{
			yield return additionalLeaving;
		}
		yield return new ThingDefCountClass(ThingDefOf.Meat_Twisted, Mathf.RoundToInt(30f * Growth));
	}

	public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
	{
		base.Destroy(mode);
		Find.Anomaly.Notify_HarbingerTreeDied();
	}

	private void CreateCorpseStockpile()
	{
		List<HarbingerTree> selectedTrees = Find.Selector.SelectedObjects.OfType<HarbingerTree>().ToList();
		if (base.Map.zoneManager.ZoneAt(base.Position) != null)
		{
			Zone zone = base.Map.zoneManager.ZoneAt(base.Position);
			Zone_Stockpile existing = zone as Zone_Stockpile;
			if (existing == null)
			{
				return;
			}
			base.Map.floodFiller.FloodFill(base.Position, (IntVec3 c) => selectedTrees.Any((HarbingerTree tree) => tree.RadialCells.Contains(c)) && (base.Map.zoneManager.ZoneAt(c) == null || base.Map.zoneManager.ZoneAt(c) == existing) && (bool)Designator_ZoneAdd.IsZoneableCell(c, base.Map), delegate(IntVec3 c)
			{
				if (!existing.ContainsCell(c))
				{
					existing.AddCell(c);
				}
			});
			return;
		}
		Zone_Stockpile stockpile = new Zone_Stockpile(StorageSettingsPreset.CorpseStockpile, base.Map.zoneManager);
		stockpile.settings.filter.SetAllow(ThingCategoryDefOf.CorpsesMechanoid, allow: false);
		stockpile.settings.filter.SetAllow(SpecialThingFilterDefOf.AllowCorpsesUnnatural, allow: false);
		base.Map.zoneManager.RegisterZone(stockpile);
		Zone_Stockpile existingStockpile = null;
		base.Map.floodFiller.FloodFill(base.Position, delegate(IntVec3 c)
		{
			if (base.Map.zoneManager.ZoneAt(c) is Zone_Stockpile zone_Stockpile)
			{
				existingStockpile = zone_Stockpile;
			}
			return selectedTrees.Any((HarbingerTree tree) => tree.RadialCells.Contains(c)) && base.Map.zoneManager.ZoneAt(c) == null && (bool)Designator_ZoneAdd.IsZoneableCell(c, base.Map);
		}, delegate(IntVec3 c)
		{
			stockpile.AddCell(c);
		});
		if (existingStockpile == null)
		{
			return;
		}
		List<IntVec3> list = stockpile.Cells.ToList();
		stockpile.Delete();
		foreach (IntVec3 item in list)
		{
			existingStockpile.AddCell(item);
		}
	}

	public override void DrawExtraSelectionOverlays()
	{
		base.DrawExtraSelectionOverlays();
		GenDraw.DrawRadiusRing(base.Position, ConsumeRadius);
	}

	public override string GetInspectString()
	{
		StringBuilder stringBuilder = new StringBuilder(base.GetInspectString());
		stringBuilder.AppendLineIfNotEmpty();
		if (DebugSettings.godMode)
		{
			stringBuilder.AppendLine("DEV: Nutrition: " + nutrition.ToString("F2"));
		}
		if (ConsumableNearby)
		{
			stringBuilder.AppendLine("HarbingerTreeConsuming".Translate());
		}
		else
		{
			stringBuilder.AppendLine("HarbingerTreeNotConsuming".Translate());
		}
		return stringBuilder.ToString().TrimEndNewlines();
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (!(base.Map.zoneManager.ZoneAt(base.Position) is Zone_Stockpile) || base.Map.zoneManager.ZoneAt(base.Position) != null)
		{
			yield return new Command_Action
			{
				defaultLabel = "CreateCorpseStockpile".Translate(),
				defaultDesc = "CreateCorpseStockpileDesc".Translate(),
				icon = CreateCorpseStockpileIcon.Texture,
				action = CreateCorpseStockpile
			};
		}
		if (!DebugSettings.ShowDevGizmos)
		{
			yield break;
		}
		yield return new Command_Action
		{
			defaultLabel = "DEV: Add 10 nutrition",
			action = delegate
			{
				AddNutrition(10f);
			}
		};
		yield return new Command_Action
		{
			defaultLabel = "DEV: Spawn new tree",
			action = SpawnNewTree
		};
		yield return new Command_Action
		{
			defaultLabel = "DEV: Update Roots",
			action = UpdateRoots
		};
		yield return new Command_Action
		{
			defaultLabel = "DEV: Blood Spatters (Delay)",
			action = delegate
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				for (int i = 1; i < 10; i++)
				{
					int delay = i * 60;
					list.Add(new FloatMenuOption(delay.TicksToSeconds() + "s", delegate
					{
						DelayedSplatter(delay);
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
		};
		void DelayedSplatter(int ticks)
		{
			EffecterDef effecterDef = new EffecterDef
			{
				maintainTicks = ticks + 5
			};
			SubEffecterDef item = new SubEffecterDef
			{
				subEffecterClass = typeof(SubEffecter_GroupedChance),
				chancePerTick = 1f,
				children = new List<SubEffecterDef>(EffecterDefOf.HarbingerTreeConsume.children),
				initialDelayTicks = ticks,
				subTriggerOnSpawn = false
			};
			effecterDef.children = new List<SubEffecterDef> { item };
			foreach (ThingWithComps rootTarget in rootTargets)
			{
				effecterDef.SpawnMaintained(rootTarget, rootTarget);
			}
		}
	}
}
