using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Verse;

public class AreaManager : IExposable
{
	public Map map;

	private List<Area> areas = new List<Area>();

	public const int MaxAllowedAreas = 10;

	public List<Area> AllAreas => areas;

	public Area_Home Home => Get<Area_Home>();

	public Area_BuildRoof BuildRoof => Get<Area_BuildRoof>();

	public Area_NoRoof NoRoof => Get<Area_NoRoof>();

	public Area_SnowOrSandClear SnowOrSandClear => Get<Area_SnowOrSandClear>();

	public Area_PollutionClear PollutionClear => Get<Area_PollutionClear>();

	public AreaManager(Map map)
	{
		this.map = map;
	}

	public void AddStartingAreas()
	{
		areas.Add(new Area_Home(this));
		areas.Add(new Area_BuildRoof(this));
		areas.Add(new Area_NoRoof(this));
		areas.Add(new Area_SnowOrSandClear(this));
		if (ModsConfig.BiotechActive)
		{
			areas.Add(new Area_PollutionClear(this));
		}
		if (!map.wasSpawnedViaGravShipLanding)
		{
			TryMakeNewAllowed(out var _);
		}
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref areas, "areas", LookMode.Deep);
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			UpdateAllAreasLinks();
		}
		if (Scribe.mode == LoadSaveMode.PostLoadInit && ModsConfig.BiotechActive && PollutionClear == null)
		{
			Area_PollutionClear area_PollutionClear = new Area_PollutionClear(this);
			area_PollutionClear.areaManager = this;
			areas.Add(area_PollutionClear);
		}
	}

	public void AreaManagerUpdate()
	{
		for (int i = 0; i < areas.Count; i++)
		{
			areas[i].AreaUpdate();
		}
	}

	internal void Remove(Area area)
	{
		if (!area.Mutable)
		{
			Log.Error("Tried to delete non-Deletable area " + area);
			return;
		}
		areas.Remove(area);
		NotifyEveryoneAreaRemoved(area);
		if (Designator_AreaAllowed.SelectedArea == area)
		{
			Designator_AreaAllowed.ClearSelectedArea();
		}
	}

	public Area GetLabeled(string s)
	{
		for (int i = 0; i < areas.Count; i++)
		{
			if (areas[i].Label == s)
			{
				return areas[i];
			}
		}
		return null;
	}

	public T Get<T>() where T : Area
	{
		for (int i = 0; i < areas.Count; i++)
		{
			if (areas[i] is T result)
			{
				return result;
			}
		}
		return null;
	}

	private void SortAreas()
	{
		areas.InsertionSort((Area a, Area b) => b.ListPriority.CompareTo(a.ListPriority));
	}

	private void UpdateAllAreasLinks()
	{
		for (int i = 0; i < areas.Count; i++)
		{
			areas[i].areaManager = this;
		}
	}

	private void NotifyEveryoneAreaRemoved(Area area)
	{
		foreach (Pawn item in PawnsFinder.All_AliveOrDead)
		{
			if (item.playerSettings != null)
			{
				item.playerSettings.Notify_AreaRemoved(area);
			}
		}
	}

	public void Notify_MapRemoved()
	{
		for (int i = 0; i < areas.Count; i++)
		{
			NotifyEveryoneAreaRemoved(areas[i]);
		}
	}

	public bool CanMakeNewAllowed()
	{
		return areas.Where((Area a) => a is Area_Allowed).Count() < 10;
	}

	public bool TryMakeNewAllowed(out Area_Allowed area)
	{
		if (!CanMakeNewAllowed())
		{
			area = null;
			return false;
		}
		area = new Area_Allowed(this);
		areas.Add(area);
		SortAreas();
		return true;
	}
}
