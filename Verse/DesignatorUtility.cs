using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class DesignatorUtility
	{
		public static readonly Material DragHighlightCellMat = MaterialPool.MatFrom("UI/Overlays/DragHighlightCell", ShaderDatabase.MetaOverlay);

		public static readonly Material DragHighlightThingMat = MaterialPool.MatFrom("UI/Overlays/DragHighlightThing", ShaderDatabase.MetaOverlay);

		private static Dictionary<Type, Designator> StandaloneDesignators = new Dictionary<Type, Designator>();

		private static HashSet<Thing> selectedThings = new HashSet<Thing>();

		public static Designator FindAllowedDesignator<T>() where T : Designator
		{
			List<DesignationCategoryDef> allDefsListForReading = DefDatabase<DesignationCategoryDef>.AllDefsListForReading;
			GameRules rules = Current.Game.Rules;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				List<Designator> allResolvedDesignators = allDefsListForReading[i].AllResolvedDesignators;
				for (int j = 0; j < allResolvedDesignators.Count; j++)
				{
					if (rules == null || rules.DesignatorAllowed(allResolvedDesignators[j]))
					{
						T val = allResolvedDesignators[j] as T;
						if (val != null)
						{
							return val;
						}
					}
				}
			}
			Designator designator = StandaloneDesignators.TryGetValue(typeof(T));
			if (designator == null)
			{
				designator = (Activator.CreateInstance(typeof(T)) as Designator);
				StandaloneDesignators[typeof(T)] = designator;
			}
			return designator;
		}

		public static void RenderHighlightOverSelectableCells(Designator designator, List<IntVec3> dragCells)
		{
			for (int i = 0; i < dragCells.Count; i++)
			{
				Vector3 position = dragCells[i].ToVector3Shifted();
				position.y = AltitudeLayer.MetaOverlays.AltitudeFor();
				Graphics.DrawMesh(MeshPool.plane10, position, Quaternion.identity, DragHighlightCellMat, 0);
			}
		}

		public static void RenderHighlightOverSelectableThings(Designator designator, List<IntVec3> dragCells)
		{
			selectedThings.Clear();
			for (int i = 0; i < dragCells.Count; i++)
			{
				List<Thing> thingList = dragCells[i].GetThingList(designator.Map);
				for (int j = 0; j < thingList.Count; j++)
				{
					if (designator.CanDesignateThing(thingList[j]).Accepted && !selectedThings.Contains(thingList[j]))
					{
						selectedThings.Add(thingList[j]);
						Vector3 drawPos = thingList[j].DrawPos;
						drawPos.y = AltitudeLayer.MetaOverlays.AltitudeFor();
						Graphics.DrawMesh(MeshPool.plane10, drawPos, Quaternion.identity, DragHighlightThingMat, 0);
					}
				}
			}
			selectedThings.Clear();
		}
	}
}
