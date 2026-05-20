using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public class CompPlantable : ThingComp
{
	private List<IntVec3> plantCells = new List<IntVec3>();

	private static readonly Texture2D CancelCommandTex = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

	private static TargetingParameters TargetingParams => new TargetingParameters
	{
		canTargetPawns = false,
		canTargetLocations = true
	};

	public CompProperties_Plantable Props => (CompProperties_Plantable)props;

	public List<IntVec3> PlantCells => plantCells;

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (!ModsConfig.IdeologyActive && !ModsConfig.BiotechActive && !ModsConfig.AnomalyActive && !ModsConfig.OdysseyActive)
		{
			yield break;
		}
		if (PlantCells.Count > 0)
		{
			yield return new Command_Action
			{
				defaultLabel = "CancelPlantThing".Translate(Props.plantDefToSpawn),
				defaultDesc = "CancelPlantThingDesc".Translate(Props.plantDefToSpawn),
				icon = CancelCommandTex,
				hotKey = KeyBindingDefOf.Designator_Cancel,
				action = delegate
				{
					plantCells.Clear();
				}
			};
		}
		if (PlantCells.Count < parent.stackCount)
		{
			yield return new Command_Action
			{
				defaultLabel = "PlantThing".Translate(Props.plantDefToSpawn),
				defaultDesc = "PlantThingDesc".Translate(Props.plantDefToSpawn),
				icon = Props.plantDefToSpawn.uiIcon,
				action = BeginTargeting
			};
		}
	}

	private void BeginTargeting()
	{
		Find.Targeter.BeginTargeting(TargetingParams, delegate(LocalTargetInfo t)
		{
			if (ValidateTarget(t))
			{
				if (ConnectionStrengthReducedByNearbyBuilding(t.Cell, out var _))
				{
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("TreeBlockedByNearbyBuildingsDesc".Translate(NamedArgumentUtility.Named(Props.plantDefToSpawn, "PLANT")), delegate
					{
						plantCells.Add(t.Cell);
					}));
				}
				else
				{
					plantCells.Add(t.Cell);
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
				}
			}
			else
			{
				BeginTargeting();
			}
		}, delegate(LocalTargetInfo t)
		{
			if (CanPlantAt(t.Cell, parent.MapHeld).Accepted)
			{
				GenDraw.DrawTargetHighlight(t);
				DrawSurroundingsInfo(t.Cell);
			}
		}, (LocalTargetInfo t) => true, null, null, Props.plantDefToSpawn.uiIcon, playSoundOnAction: false);
	}

	private bool ConnectionStrengthReducedByNearbyBuilding(IntVec3 cell, out List<Thing> buildings)
	{
		CompProperties_TreeConnection compProperties = Props.plantDefToSpawn.GetCompProperties<CompProperties_TreeConnection>();
		if (compProperties != null)
		{
			buildings = parent.MapHeld.listerArtificialBuildingsForMeditation.GetForCell(cell, compProperties.radiusToBuildingForConnectionStrengthLoss);
			if (buildings.Any())
			{
				return true;
			}
		}
		buildings = null;
		return false;
	}

	private void DrawSurroundingsInfo(IntVec3 cell)
	{
		CompProperties_TreeConnection compProperties = Props.plantDefToSpawn.GetCompProperties<CompProperties_TreeConnection>();
		if (compProperties != null)
		{
			Color color = Color.white;
			if (ConnectionStrengthReducedByNearbyBuilding(cell, out var buildings))
			{
				color = Color.red;
				foreach (Thing item in buildings)
				{
					GenDraw.DrawLineBetween(cell.ToVector3ShiftedWithAltitude(AltitudeLayer.Blueprint), item.TrueCenter(), SimpleColor.Red);
				}
			}
			color.a = 0.5f;
			GenDraw.DrawRadiusRing(cell, compProperties.radiusToBuildingForConnectionStrengthLoss, color);
		}
		for (int i = 0; i < Props.plantDefToSpawn.comps.Count; i++)
		{
			if (Props.plantDefToSpawn.comps[i] is IPlantEffectRadius plantEffectRadius)
			{
				Color white = Color.white;
				white.a = 0.5f;
				GenDraw.DrawRadiusRing(cell, plantEffectRadius.PlantEffectRadius, white);
				break;
			}
		}
	}

	public override void PostDrawExtraSelectionOverlays()
	{
		for (int i = 0; i < plantCells.Count; i++)
		{
			if (plantCells[i].IsValid)
			{
				IntVec3 intVec = plantCells[i];
				GenDraw.DrawLineBetween(parent.PositionHeld.ToVector3Shifted(), intVec.ToVector3Shifted(), AltitudeLayer.Blueprint.AltitudeFor());
				GhostDrawer.DrawGhostThing(intVec, Rot4.North, Props.plantDefToSpawn, Props.plantDefToSpawn.graphic, Color.white, AltitudeLayer.Blueprint);
				DrawSurroundingsInfo(intVec);
			}
		}
	}

	public void DoPlant(Pawn planter, IntVec3 cell, Map map)
	{
		Plant plant = (Plant)ThingMaker.MakeThing(Props.plantDefToSpawn);
		if (GenPlace.TryPlaceThing(plant, cell, map, ThingPlaceMode.Direct))
		{
			planter.records.Increment(RecordDefOf.PlantsSown);
			Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.SowedPlant, planter.Named(HistoryEventArgsNames.Doer)));
			if (Props.plantDefToSpawn.plant.humanFoodPlant)
			{
				Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.SowedHumanFoodPlant, planter.Named(HistoryEventArgsNames.Doer)));
			}
			if (ModsConfig.IdeologyActive && plant.def == ThingDefOf.Plant_TreeGauranlen)
			{
				EffecterDefOf.GauranlenLeavesBatch.Spawn(cell, map).Cleanup();
			}
			plant.Growth = 0.0001f;
			plant.sown = true;
			map.mapDrawer.MapMeshDirty(cell, MapMeshFlagDefOf.Things);
			parent.Destroy();
			if (ModsConfig.IdeologyActive && plant.def == ThingDefOf.Plant_TreeGauranlen)
			{
				Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.PlantedGauranlenTree, planter.Named(HistoryEventArgsNames.Doer)));
			}
		}
	}

	public void Notify_SeedRemovedFromStackForPlantingAt(IntVec3 cell)
	{
		if (plantCells.Contains(cell))
		{
			plantCells.Remove(cell);
		}
	}

	public void Notify_RemovedFromStackForPlantingAt(IntVec3 newPlantCell)
	{
		plantCells.Clear();
		if (newPlantCell.IsValid)
		{
			plantCells.Add(newPlantCell);
		}
	}

	public override void PreAbsorbStack(Thing otherStack, int count)
	{
		CompPlantable compPlantable = otherStack.TryGetComp<CompPlantable>();
		if (compPlantable == null)
		{
			return;
		}
		List<IntVec3> list = compPlantable.PlantCells;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].IsValid && !plantCells.Contains(list[i]))
			{
				plantCells.Add(list[i]);
			}
		}
	}

	public override void PostSplitOff(Thing piece)
	{
		CompPlantable compPlantable = piece.TryGetComp<CompPlantable>();
		if (compPlantable == null)
		{
			return;
		}
		List<IntVec3> list = plantCells;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].IsValid && !compPlantable.plantCells.Contains(list[i]))
			{
				compPlantable.plantCells.Add(list[i]);
			}
		}
	}

	public AcceptanceReport CanPlantAt(IntVec3 cell, Map map)
	{
		if (!cell.IsValid || cell.Fogged(map))
		{
			return false;
		}
		Thing blockingThing;
		AcceptanceReport acceptanceReport = Props.plantDefToSpawn.CanEverPlantAt(cell, map, out blockingThing, canWipePlantsExceptTree: true, checkMapTemperature: false);
		if (!acceptanceReport.Accepted)
		{
			return "CannotPlantThing".Translate(parent) + ": " + acceptanceReport.Reason.CapitalizeFirst();
		}
		TerrainDef terrain = cell.GetTerrain(map);
		if (terrain.IsFloor && terrain.fertility <= 0f)
		{
			return "CannotPlantMissingTerrainTag".Translate();
		}
		if (Props.plantDefToSpawn.plant.interferesWithRoof && cell.Roofed(map))
		{
			return "CannotPlantThing".Translate(parent) + ": " + "BlockedByRoof".Translate().CapitalizeFirst();
		}
		Thing thing = PlantUtility.AdjacentSowBlocker(Props.plantDefToSpawn, cell, map);
		if (thing != null)
		{
			return "CannotPlantThing".Translate(parent) + ": " + "AdjacentSowBlocker".Translate(thing);
		}
		if (Props.plantDefToSpawn.plant.minSpacingBetweenSamePlant > 0f)
		{
			foreach (Thing item in map.listerThings.ThingsOfDef(Props.plantDefToSpawn))
			{
				if (item.Position.InHorDistOf(cell, Props.plantDefToSpawn.plant.minSpacingBetweenSamePlant))
				{
					return "CannotPlantThing".Translate(parent) + ": " + "TooCloseToOtherPlant".Translate(item);
				}
			}
			foreach (Thing item2 in map.listerThings.ThingsOfDef(parent.def))
			{
				CompPlantable compPlantable = item2.TryGetComp<CompPlantable>();
				if (compPlantable == null || compPlantable.PlantCells.NullOrEmpty())
				{
					continue;
				}
				for (int i = 0; i < compPlantable.PlantCells.Count; i++)
				{
					if (compPlantable.PlantCells[i].InHorDistOf(cell, Props.plantDefToSpawn.plant.minSpacingBetweenSamePlant))
					{
						return "CannotPlantThing".Translate(parent) + ": " + "TooCloseToOtherSeedPlantCell".Translate(item2.GetCustomLabelNoCount(includeHp: false));
					}
				}
			}
		}
		List<Thing> list = map.thingGrid.ThingsListAt(cell);
		for (int j = 0; j < list.Count; j++)
		{
			if (list[j] is Building_PlantGrower)
			{
				return "CannotPlantThing".Translate(parent) + ": " + "BlockedBy".Translate(list[j]).CapitalizeFirst();
			}
		}
		return true;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Collections.Look(ref plantCells, "plantCells", LookMode.Value);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && plantCells == null)
		{
			plantCells = new List<IntVec3>();
		}
	}

	public bool ValidateTarget(LocalTargetInfo target)
	{
		AcceptanceReport acceptanceReport = CanPlantAt(target.Cell, parent.MapHeld);
		if (!acceptanceReport.Accepted)
		{
			if (!acceptanceReport.Reason.NullOrEmpty())
			{
				Messages.Message(acceptanceReport.Reason, parent, MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		return true;
	}
}
