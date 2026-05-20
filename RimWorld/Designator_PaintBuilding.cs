using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Designator_PaintBuilding : Designator_Paint
{
	private static readonly HashSet<Thing> tmpPaintThings = new HashSet<Thing>();

	protected override Texture2D IconTopTex => ContentFinder<Texture2D>.Get("UI/Designators/Paint_Top");

	protected override DesignationDef Designation => DesignationDefOf.PaintBuilding;

	public Designator_PaintBuilding()
	{
		defaultLabel = "DesignatorPaintBuilding".Translate();
		defaultDesc = "DesignatorPaintBuildingDesc".Translate();
		icon = ContentFinder<Texture2D>.Get("UI/Designators/Paint_Bottom");
		tutorTag = "PaintBuilding";
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 c)
	{
		if (eyedropMode)
		{
			return eyedropper.CanDesignateCell(c);
		}
		if (!c.InBounds(base.Map) || c.Fogged(base.Map))
		{
			return false;
		}
		List<Thing> thingList = c.GetThingList(base.Map);
		for (int i = 0; i < thingList.Count; i++)
		{
			if ((bool)CanDesignateThing(thingList[i]))
			{
				return true;
			}
		}
		return "MessageMustDesignatePaintableBuildings".Translate(colorDef);
	}

	public override void DesignateSingleCell(IntVec3 c)
	{
		if (eyedropMode)
		{
			eyedropper.DesignateSingleCell(c);
			return;
		}
		List<Thing> thingList = c.GetThingList(base.Map);
		for (int i = 0; i < thingList.Count; i++)
		{
			if (CanDesignateThing(thingList[i]).Accepted)
			{
				DesignateThing(thingList[i]);
			}
		}
	}

	public override AcceptanceReport CanDesignateThing(Thing t)
	{
		if (t.def.building == null || !t.def.building.paintable)
		{
			return false;
		}
		if (t.Faction != Faction.OfPlayer)
		{
			return false;
		}
		if (t is Building building && building.PaintColorDef == colorDef)
		{
			return false;
		}
		if (base.Map.designationManager.DesignationOn(t, DesignationDefOf.Uninstall) != null)
		{
			return false;
		}
		return true;
	}

	public override void DesignateThing(Thing t)
	{
		base.Map.designationManager.TryRemoveDesignationOn(t, Designation);
		base.Map.designationManager.TryRemoveDesignationOn(t, DesignationDefOf.RemovePaintBuilding);
		if (DebugSettings.godMode)
		{
			((Building)t).ChangePaint(colorDef);
		}
		else
		{
			base.Map.designationManager.AddDesignation(new Designation(t, Designation, colorDef));
		}
	}

	protected override int NumHighlightedCells()
	{
		tmpPaintThings.Clear();
		Find.DesignatorManager.Dragger.UpdateCellBuffer();
		foreach (IntVec3 item in Find.DesignatorManager.Dragger.CellBuffer)
		{
			if (!item.InBounds(base.Map) || item.Fogged(base.Map))
			{
				continue;
			}
			List<Thing> thingList = item.GetThingList(base.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (!tmpPaintThings.Contains(thingList[i]) && (bool)CanDesignateThing(thingList[i]))
				{
					tmpPaintThings.Add(thingList[i]);
				}
			}
		}
		return tmpPaintThings.Count;
	}
}
