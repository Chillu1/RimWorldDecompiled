using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public abstract class Area : IExposable, ILoadReferenceable, ICellBoolGiver, IRenameable
{
	public AreaManager areaManager;

	public int ID = -1;

	private BoolGrid innerGrid;

	private CellBoolDrawer drawer;

	private Texture2D colorTextureInt;

	public virtual string RenamableLabel { get; set; }

	public virtual string BaseLabel { get; set; }

	public virtual string InspectLabel => RenamableLabel;

	public Map Map => areaManager.map;

	public BoolGrid InnerGrid => innerGrid;

	public int TrueCount => innerGrid.TrueCount;

	public abstract string Label { get; }

	public abstract Color Color { get; }

	public abstract int ListPriority { get; }

	public Texture2D ColorTexture
	{
		get
		{
			if (colorTextureInt == null)
			{
				colorTextureInt = SolidColorMaterials.NewSolidColorTexture(Color);
			}
			return colorTextureInt;
		}
	}

	public bool this[int index]
	{
		get
		{
			return innerGrid[index];
		}
		set
		{
			Set(Map.cellIndices.IndexToCell(index), value);
		}
	}

	public bool this[IntVec3 c]
	{
		get
		{
			return innerGrid[Map.cellIndices.CellToIndex(c)];
		}
		set
		{
			Set(c, value);
		}
	}

	private CellBoolDrawer Drawer
	{
		get
		{
			if (drawer == null)
			{
				drawer = new CellBoolDrawer(this, Map.Size.x, Map.Size.z, 3650);
			}
			return drawer;
		}
	}

	public IEnumerable<IntVec3> ActiveCells => innerGrid.ActiveCells;

	public virtual bool Mutable => false;

	public Area()
	{
	}

	public Area(AreaManager areaManager)
	{
		this.areaManager = areaManager;
		innerGrid = new BoolGrid(areaManager.map);
		ID = Find.UniqueIDsManager.GetNextAreaID();
	}

	public virtual void ExposeData()
	{
		Scribe_Values.Look(ref ID, "ID", -1);
		Scribe_Deep.Look(ref innerGrid, "innerGrid");
	}

	public bool GetCellBool(int index)
	{
		return innerGrid[index];
	}

	public Color GetCellExtraColor(int index)
	{
		return Color.white;
	}

	public virtual bool AssignableAsAllowed()
	{
		return false;
	}

	protected virtual void Set(IntVec3 c, bool val)
	{
		int index = Map.cellIndices.CellToIndex(c);
		if (innerGrid[index] != val)
		{
			innerGrid[index] = val;
			MarkDirty(c);
		}
	}

	private void MarkDirty(IntVec3 c)
	{
		Drawer.SetDirty();
		Map.pathFinder.MapData.Notify_AreaDelta(this, c);
		c.GetRegion(Map, RegionType.Set_All)?.Notify_AreaChanged(this);
	}

	public void Delete()
	{
		areaManager.Remove(this);
	}

	public void MarkForDraw()
	{
		if (Map == Find.CurrentMap && !Find.ScreenshotModeHandler.Active)
		{
			Drawer.MarkForDraw();
		}
	}

	public void AreaUpdate()
	{
		Drawer.CellBoolDrawerUpdate();
	}

	public void Invert()
	{
		innerGrid.Invert();
		Drawer.SetDirty();
	}

	public void Clear()
	{
		innerGrid.Clear();
		Drawer.SetDirty();
	}

	protected void InvalidateColorTexture()
	{
		Object.Destroy(colorTextureInt);
		colorTextureInt = null;
		drawer?.Notify_ColorChanged();
	}

	public abstract string GetUniqueLoadID();
}
