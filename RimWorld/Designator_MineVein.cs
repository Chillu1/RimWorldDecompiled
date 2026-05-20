using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_MineVein : Designator_Mine
{
	public override bool DragDrawMeasurements => true;

	protected override DesignationDef Designation => DesignationDefOf.MineVein;

	public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.FilledRectangle;

	public Designator_MineVein()
	{
		defaultLabel = "DesignatorMineVein".Translate();
		icon = ContentFinder<Texture2D>.Get("UI/Designators/MineVein");
		defaultDesc = "DesignatorMineVeinDesc".Translate();
		useMouseIcon = true;
		hotKey = KeyBindingDefOf.Misc11;
		soundDragSustain = SoundDefOf.Designate_DragStandard;
		soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
		soundSucceeded = SoundDefOf.Designate_Mine;
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 c)
	{
		if (!c.InBounds(base.Map))
		{
			return false;
		}
		if (base.Map.designationManager.DesignationAt(c, Designation) != null)
		{
			return AcceptanceReport.WasRejected;
		}
		if (c.Fogged(base.Map))
		{
			return true;
		}
		Mineable firstMineable = c.GetFirstMineable(base.Map);
		if (firstMineable == null)
		{
			return "MessageMustDesignateMineable".Translate();
		}
		AcceptanceReport result = CanDesignateThing(firstMineable);
		if (!result.Accepted)
		{
			return result;
		}
		return AcceptanceReport.WasAccepted;
	}

	public override AcceptanceReport CanDesignateThing(Thing t)
	{
		if (!t.def.mineable || !t.def.building.veinMineable)
		{
			return false;
		}
		if (base.Map.designationManager.DesignationAt(t.Position, Designation) != null)
		{
			return AcceptanceReport.WasRejected;
		}
		return true;
	}

	public override void DesignateSingleCell(IntVec3 loc)
	{
		FloodFillDesignations(loc, base.Map, loc.GetEdifice(base.Map).def);
		Designator_Mine.PossiblyWarnPlayerOnDesignatingMining();
	}

	public static void FloodFillDesignations(IntVec3 loc, Map map, ThingDef def)
	{
		map.floodFiller.FloodFill(loc, Validator, delegate(IntVec3 c)
		{
			map.designationManager.AddDesignation(new Designation(c, DesignationDefOf.MineVein));
			map.designationManager.TryRemoveDesignation(c, DesignationDefOf.Mine);
		});
		bool Validator(IntVec3 c)
		{
			if (c.Fogged(map))
			{
				return false;
			}
			if (c.GetEdifice(map)?.def != def)
			{
				return false;
			}
			if (map.designationManager.DesignationAt(c, DesignationDefOf.MineVein) != null)
			{
				return false;
			}
			return true;
		}
	}

	public static void RemoveContiguousDesignations(IntVec3 loc, Map map, ThingDef def)
	{
		map.floodFiller.FloodFill(loc, (IntVec3 c) => map.designationManager.DesignationAt(c, DesignationDefOf.MineVein) != null && c.GetEdifice(map).def == def, delegate(IntVec3 c)
		{
			map.designationManager.RemoveDesignation(map.designationManager.DesignationAt(c, DesignationDefOf.MineVein));
		});
	}
}
