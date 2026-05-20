using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class Blueprint_Install : Blueprint
{
	private MinifiedThing miniToInstall;

	private Building buildingToReinstall;

	public Thing MiniToInstallOrBuildingToReinstall
	{
		get
		{
			if (miniToInstall != null)
			{
				return miniToInstall;
			}
			if (buildingToReinstall != null)
			{
				return buildingToReinstall;
			}
			throw new InvalidOperationException("Nothing to install.");
		}
	}

	public Thing ThingToInstall => MiniToInstallOrBuildingToReinstall.GetInnerIfMinified();

	public override ThingStyleDef StyleDef
	{
		get
		{
			return ThingToInstall?.StyleDef;
		}
		set
		{
			ThingToInstall.StyleDef = value;
		}
	}

	protected override float WorkTotal => (ThingToInstall.def.plant != null) ? (-1) : 150;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref miniToInstall, "miniToInstall");
		Scribe_References.Look(ref buildingToReinstall, "buildingToReinstall");
	}

	public override BuildableDef EntityToBuild()
	{
		return ThingToInstall?.def;
	}

	public override ThingStyleDef EntityToBuildStyle()
	{
		return ThingToInstall?.StyleDef;
	}

	public override ThingDef EntityToBuildStuff()
	{
		return ThingToInstall?.Stuff;
	}

	public override List<ThingDefCountClass> TotalMaterialCost()
	{
		Log.Error("Called MaterialsNeededTotal on a Blueprint_Install.");
		return new List<ThingDefCountClass>();
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		map.listerBuildings.RegisterInstallBlueprint(this);
		if (buildingToReinstall != null)
		{
			overrideGraphicIndex = buildingToReinstall.overrideGraphicIndex ?? buildingToReinstall.thingIDNumber;
		}
		else if (miniToInstall != null)
		{
			overrideGraphicIndex = miniToInstall.InnerThing.overrideGraphicIndex ?? miniToInstall.InnerThing.thingIDNumber;
		}
		if (!respawningAfterLoad && !MiniToInstallOrBuildingToReinstall.BeingTransportedOnGravship && (MiniToInstallOrBuildingToReinstall.Destroyed || !MiniToInstallOrBuildingToReinstall.SpawnedOrAnyParentSpawned))
		{
			Destroy();
		}
	}

	public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
	{
		base.Map.listerBuildings.DeregisterInstallBlueprint(this);
		base.DeSpawn(mode);
	}

	protected override Thing MakeSolidThing(out bool shouldSelect)
	{
		Thing thingToInstall = ThingToInstall;
		if (miniToInstall != null)
		{
			foreach (Designation item in base.Map.designationManager.AllDesignationsOn(miniToInstall))
			{
				Designation newDes = new Designation(thingToInstall, item.def, item.colorDef);
				base.Map.designationManager.AddDesignation(newDes);
			}
			shouldSelect = Find.Selector.IsSelected(miniToInstall);
			miniToInstall.InnerThing = null;
			miniToInstall.Destroy();
		}
		else
		{
			shouldSelect = false;
		}
		return thingToInstall;
	}

	public override bool TryReplaceWithSolidThing(Pawn workerPawn, out Thing createdThing, out bool jobEnded)
	{
		Map map = base.Map;
		bool num = base.TryReplaceWithSolidThing(workerPawn, out createdThing, out jobEnded);
		if (num)
		{
			if (overrideGraphicIndex.HasValue)
			{
				createdThing.overrideGraphicIndex = overrideGraphicIndex;
			}
			if (createdThing is Plant)
			{
				SoundDefOf.Replant_Complete.PlayOneShot(new TargetInfo(base.Position, map));
			}
			else
			{
				SoundDefOf.Building_Complete.PlayOneShot(new TargetInfo(base.Position, map));
			}
			workerPawn.records.Increment(RecordDefOf.ThingsInstalled);
		}
		return num;
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		Command command = BuildCopyCommandUtility.BuildCopyCommand(ThingToInstall.def, ThingToInstall.Stuff, ThingToInstall.StyleSourcePrecept as Precept_Building, StyleDef, styleOverridden: true);
		if (command != null)
		{
			yield return command;
		}
		if (base.Faction != Faction.OfPlayer)
		{
			yield break;
		}
		foreach (Command item in BuildRelatedCommandUtility.RelatedBuildCommands(ThingToInstall.def))
		{
			yield return item;
		}
	}

	public override void DrawExtraSelectionOverlays()
	{
		base.DrawExtraSelectionOverlays();
		if (buildingToReinstall != null)
		{
			GenDraw.DrawLineBetween(buildingToReinstall.TrueCenter(), this.TrueCenter());
		}
		if (ThingToInstall.def.drawPlaceWorkersWhileInstallBlueprintSelected && ThingToInstall.def.PlaceWorkers != null)
		{
			List<PlaceWorker> placeWorkers = ThingToInstall.def.PlaceWorkers;
			for (int i = 0; i < placeWorkers.Count; i++)
			{
				placeWorkers[i].DrawGhost(ThingToInstall.def, base.Position, base.Rotation, Color.white, ThingToInstall);
			}
		}
	}

	internal void SetThingToInstallFromMinified(MinifiedThing itemToInstall)
	{
		miniToInstall = itemToInstall;
		buildingToReinstall = null;
	}

	internal void SetBuildingToReinstall(Building buildingToReinstall)
	{
		if (!buildingToReinstall.def.Minifiable)
		{
			Log.Error("Tried to reinstall non-minifiable building.");
			return;
		}
		miniToInstall = null;
		this.buildingToReinstall = buildingToReinstall;
	}
}
