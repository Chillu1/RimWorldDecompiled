using UnityEngine;
using Verse;

namespace RimWorld
{
	public sealed class FertilityGrid
	{
		private Map map;

		private CellBoolDrawer drawerInt;

		private static readonly Color MediumFertilityColor = new Color(0.59f, 0.98f, 0.59f, 1f);

		private static readonly Color LowFertilityColor = Color.yellow;

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
			return map.terrainGrid.TerrainAt(loc).fertility;
		}

		public void FertilityGridUpdate()
		{
			if (Find.PlaySettings.showFertilityOverlay)
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
			return FertilityAt(intVec) > 0.69f;
		}

		private Color CellBoolDrawerGetExtraColorInt(int index)
		{
			float num = FertilityAt(CellIndicesUtility.IndexToCell(index, map.Size.x));
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
}
