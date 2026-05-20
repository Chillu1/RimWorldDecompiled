using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public abstract class Blueprint : ThingWithComps, IConstructible
{
	public override string Label => def.entityDefToBuild.label + "BlueprintLabelExtra".Translate();

	protected abstract float WorkTotal { get; }

	public override Graphic Graphic
	{
		get
		{
			ThingStyleDef styleDef = StyleDef;
			if (styleDef?.blueprintGraphicData != null)
			{
				if (styleGraphicInt == null)
				{
					styleGraphicInt = styleDef.blueprintGraphicData.Graphic;
				}
				return styleGraphicInt;
			}
			return base.Graphic;
		}
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		Gizmo selectMonumentMarkerGizmo = QuestUtility.GetSelectMonumentMarkerGizmo(this);
		if (selectMonumentMarkerGizmo != null)
		{
			yield return selectMonumentMarkerGizmo;
		}
	}

	public virtual bool TryReplaceWithSolidThing(Pawn workerPawn, out Thing createdThing, out bool jobEnded)
	{
		bool flag = Find.Selector.IsSelected(this);
		jobEnded = false;
		if (GenConstruct.FirstBlockingThing(this, workerPawn) != null)
		{
			workerPawn.jobs.EndCurrentJob(JobCondition.Incompletable);
			jobEnded = true;
			createdThing = null;
			return false;
		}
		createdThing = MakeSolidThing(out var shouldSelect);
		Map map = base.Map;
		if (Graphic is Graphic_Random)
		{
			createdThing.overrideGraphicIndex = thingIDNumber;
		}
		GenSpawn.WipeExistingThings(base.Position, base.Rotation, createdThing.def, map, DestroyMode.Deconstruct);
		if (createdThing is Plant)
		{
			base.Position.GetPlant(base.Map)?.Destroy();
		}
		if (!base.Destroyed)
		{
			Destroy();
		}
		if (createdThing.def.CanHaveFaction)
		{
			createdThing.SetFactionDirect(workerPawn.Faction);
		}
		Thing thing = GenSpawn.Spawn(createdThing, base.Position, map, base.Rotation);
		if ((shouldSelect || flag) && thing != null)
		{
			Find.Selector.Select(thing, playSound: false, forceDesignatorDeselect: false);
		}
		foreach (Pawn item in map.mapPawns.AllPawnsSpawned)
		{
			item.pather.NotifyThingTransformed(this, thing);
		}
		foreach (IntVec3 cell in thing.OccupiedRect().Cells)
		{
			if (map.planManager.TryGetPlan(cell, out var plan))
			{
				plan.RemoveCell(cell);
			}
		}
		return true;
	}

	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		if (!WorldComponent_GravshipController.CutsceneInProgress || WorldComponent_GravshipController.GravshipRenderInProgess)
		{
			base.DrawAt(drawLoc, flip);
		}
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		map.blueprintGrid.Register(this);
		base.SpawnSetup(map, respawningAfterLoad);
	}

	public void InheritStyle(Precept_ThingStyle styleSource, ThingStyleDef styleDef)
	{
		base.StyleSourcePrecept = styleSource;
		if (styleSource == null)
		{
			StyleDef = styleDef;
		}
	}

	public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
	{
		Map map = base.Map;
		base.DeSpawn(mode);
		map.blueprintGrid.DeRegister(this);
	}

	protected abstract Thing MakeSolidThing(out bool shouldSelect);

	public abstract List<ThingDefCountClass> TotalMaterialCost();

	public bool IsCompleted()
	{
		return false;
	}

	public int ThingCountNeeded(ThingDef stuff)
	{
		foreach (ThingDefCountClass item in TotalMaterialCost())
		{
			if (item.thingDef == stuff)
			{
				return item.count;
			}
		}
		return 0;
	}

	public int SpaceRemainingFor(ThingDef stuff)
	{
		return ThingCountNeeded(stuff);
	}

	public abstract BuildableDef EntityToBuild();

	public abstract ThingStyleDef EntityToBuildStyle();

	public abstract ThingDef EntityToBuildStuff();

	public Thing BlockingHaulableOnTop()
	{
		if (def.entityDefToBuild.passability == Traversability.Standable)
		{
			return null;
		}
		foreach (IntVec3 item in this.OccupiedRect())
		{
			List<Thing> thingList = item.GetThingList(base.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (thing.def.EverHaulable)
				{
					return thing;
				}
			}
		}
		return null;
	}

	public override string GetInspectString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(base.GetInspectString());
		stringBuilder.AppendLineIfNotEmpty();
		float workTotal = WorkTotal;
		if (!Mathf.Approximately(workTotal, -1f))
		{
			stringBuilder.Append("WorkLeft".Translate() + ": " + workTotal.ToStringWorkAmount());
		}
		return stringBuilder.ToString();
	}
}
