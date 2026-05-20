using System.Collections.Generic;
using Verse;

namespace RimWorld;

public abstract class CompAutoCut : ThingComp
{
	public bool autoCut;

	private int? lastCheckedAutoCutTick;

	private ThingFilter autoCutFilter;

	public ThingFilter AutoCutFilter => autoCutFilter;

	public virtual bool CanDesignatePlants => true;

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		if (autoCutFilter == null)
		{
			autoCutFilter = new ThingFilter();
			autoCutFilter.CopyAllowancesFrom(GetDefaultAutoCutFilter());
		}
	}

	public abstract ThingFilter GetDefaultAutoCutFilter();

	public abstract ThingFilter GetFixedAutoCutFilter();

	public override void CompTickLong()
	{
		base.CompTickLong();
		if (autoCut)
		{
			DesignatePlantsToCut();
		}
	}

	public abstract IEnumerable<IntVec3> GetAutoCutCells();

	public void DesignatePlantsToCut()
	{
		if (!CanDesignatePlants)
		{
			return;
		}
		Map map = parent.Map;
		foreach (IntVec3 autoCutCell in GetAutoCutCells())
		{
			List<Thing> thingList = autoCutCell.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i] is Plant plant && CanDesignatePlant(plant))
				{
					map.designationManager.AddDesignation(new Designation(plant, DesignationDefOf.CutPlant));
				}
			}
		}
	}

	protected virtual bool CanDesignatePlant(Plant plant)
	{
		if (!plant.def.plant.allowAutoCut)
		{
			return false;
		}
		if (plant.DeliberatelyCultivated())
		{
			return false;
		}
		if (!autoCutFilter.Allows(plant))
		{
			return false;
		}
		if (plant.Map.designationManager.HasMapDesignationOn(plant))
		{
			return false;
		}
		return true;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref autoCut, "autoCut", defaultValue: false);
		Scribe_Values.Look(ref lastCheckedAutoCutTick, "lastCheckedAutoCutTick");
		Scribe_Deep.Look(ref autoCutFilter, "autoCutFilter");
	}
}
