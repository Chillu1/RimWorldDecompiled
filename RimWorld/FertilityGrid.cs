using UnityEngine;
using Verse;

namespace RimWorld;

public sealed class FertilityGrid
{
	public const float MaxPollutedFertility = 0.5f;

	private Map map;

	private CellBoolDrawer drawerInt;

	private static readonly Color ExtraLowFertilityColor = Color.red;

	private static readonly Color LowFertilityColor = Color.yellow;

	private static readonly Color MediumFertilityColor = new Color(0.59f, 0.98f, 0.59f, 1f);

	private static readonly Color HighFertilityColor = Color.green;

	public CellBoolDrawer Drawer
	{
		get
		{
			if (drawerInt == null)
			{
				drawerInt = new CellBoolDrawer(CellBoolDrawerGetBoolInt, CellBoolDrawerColorInt, CellBoolDrawerGetExtraColorInt, map.Size.x, map.Size.z, 3610);
			}
			return drawerInt;
		}
	}

	public FertilityGrid(Map map)
	{
		this.map = map;
	}

	public float FertilityAt(IntVec3 loc)
	{
		return CalculateFertilityAt(loc);
	}

	private float CalculateFertilityAt(IntVec3 loc)
	{
		Thing edifice = loc.GetEdifice(map);
		if (edifice != null && edifice.def.AffectsFertility)
		{
			return edifice.def.fertility;
		}
		float num = map.terrainGrid.TerrainAt(loc).fertility;
		if (ModsConfig.BiotechActive && map.pollutionGrid.IsPolluted(loc))
		{
			num = Mathf.Min(num, 0.5f);
		}
		return num;
	}

	public void FertilityGridUpdate()
	{
		if (Find.PlaySettings.showFertilityOverlay && !Find.ScreenshotModeHandler.Active)
		{
			Drawer.MarkForDraw();
		}
		Drawer.CellBoolDrawerUpdate();
	}

	private Color CellBoolDrawerColorInt()
	{
		return Color.white;
	}

	private bool CellBoolDrawerGetBoolInt(int index)
	{
		IntVec3 intVec = CellIndicesUtility.IndexToCell(index, map.Size.x);
		if (intVec.Filled(map) || intVec.Fogged(map))
		{
			return false;
		}
		float num = FertilityAt(intVec);
		if (ModsConfig.BiotechActive)
		{
			return num >= 0.5f;
		}
		return num > 0.69f;
	}

	private Color CellBoolDrawerGetExtraColorInt(int index)
	{
		float num = FertilityAt(CellIndicesUtility.IndexToCell(index, map.Size.x));
		if (ModsConfig.BiotechActive && num <= 0.5f)
		{
			return ExtraLowFertilityColor;
		}
		if (num <= 0.95f)
		{
			return LowFertilityColor;
		}
		if (num <= 1.1f)
		{
			return MediumFertilityColor;
		}
		if (num >= 1.1f)
		{
			return HighFertilityColor;
		}
		return Color.white;
	}
}
