using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Verse.Profile;

public static class MemoryUtility
{
	public static void UnloadUnusedUnityAssets()
	{
		LongEventHandler.ExecuteWhenFinished(delegate
		{
			Resources.UnloadUnusedAssets();
		});
	}

	public static void ClearAllMapsAndWorld()
	{
		if (Current.Game != null && Current.Game.Maps != null)
		{
			List<Map> maps = Find.Maps;
			FieldInfo[] fields = typeof(Map).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (int i = 0; i < fields.Length; i++)
			{
				if (fields[i].FieldType.IsClass)
				{
					for (int j = 0; j < maps.Count; j++)
					{
						fields[i].SetValue(maps[j], null);
					}
				}
			}
			maps.Clear();
			Current.Game.currentMapIndex = -1;
		}
		if (Find.World != null)
		{
			World world = Find.World;
			FieldInfo[] fields2 = typeof(World).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (int k = 0; k < fields2.Length; k++)
			{
				if (fields2[k].FieldType.IsClass)
				{
					fields2[k].SetValue(world, null);
				}
			}
		}
		BillUtility.Clipboard = null;
		RegionTraverser.RecreateWorkers();
		SelectionDrawer.Clear();
		WorldSelectionDrawer.Clear();
		BreakRiskAlertUtility.Clear();
		Region.ClearStaticData();
		PawnsFinder.Clear();
		SectionLayer_GravshipMask.ResetStaticData();
		LongEventHandler.ExecuteWhenFinished(WorldTerrainColliderManager.ClearCache);
		List<MainButtonDef> allDefsListForReading = DefDatabase<MainButtonDef>.AllDefsListForReading;
		for (int l = 0; l < allDefsListForReading.Count; l++)
		{
			allDefsListForReading[l].Notify_ClearingAllMapsMemory();
		}
		List<ThingDef> allDefsListForReading2 = DefDatabase<ThingDef>.AllDefsListForReading;
		for (int m = 0; m < allDefsListForReading2.Count; m++)
		{
			if (allDefsListForReading2[m].inspectorTabsResolved != null)
			{
				for (int n = 0; n < allDefsListForReading2[m].inspectorTabsResolved.Count; n++)
				{
					allDefsListForReading2[m].inspectorTabsResolved[n].Notify_ClearingAllMapsMemory();
				}
			}
		}
		List<WorldObjectDef> allDefsListForReading3 = DefDatabase<WorldObjectDef>.AllDefsListForReading;
		for (int num = 0; num < allDefsListForReading3.Count; num++)
		{
			if (allDefsListForReading3[num].inspectorTabsResolved != null)
			{
				for (int num2 = 0; num2 < allDefsListForReading3[num].inspectorTabsResolved.Count; num2++)
				{
					allDefsListForReading3[num].inspectorTabsResolved[num2].Notify_ClearingAllMapsMemory();
				}
			}
		}
	}
}
