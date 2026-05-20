using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.Sound;
using Verse.Steam;

namespace Verse;

[StaticConstructorOnStartup]
public static class DesignatorUtility
{
	public static readonly Material DragHighlightCellMat = MaterialPool.MatFrom("UI/Overlays/DragHighlightCell", ShaderDatabase.MetaOverlay);

	public static readonly Material DragHighlightThingMat = MaterialPool.MatFrom("UI/Overlays/DragHighlightThing", ShaderDatabase.MetaOverlay);

	private static Dictionary<Type, Designator> StandaloneDesignators = new Dictionary<Type, Designator>();

	private static HashSet<Thing> selectedThings = new HashSet<Thing>();

	private const float RotButSize = 64f;

	private const float RotButSpacing = 10f;

	public static T FindAllowedDesignator<T>() where T : Designator
	{
		List<DesignationCategoryDef> allDefsListForReading = DefDatabase<DesignationCategoryDef>.AllDefsListForReading;
		GameRules rules = Current.Game.Rules;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			List<Designator> allResolvedDesignators = allDefsListForReading[i].AllResolvedDesignators;
			for (int j = 0; j < allResolvedDesignators.Count; j++)
			{
				if ((rules == null || rules.DesignatorAllowed(allResolvedDesignators[j])) && allResolvedDesignators[j] is T result)
				{
					return result;
				}
			}
		}
		Designator designator = StandaloneDesignators.TryGetValue(typeof(T));
		if (designator == null)
		{
			designator = Activator.CreateInstance(typeof(T)) as Designator;
			StandaloneDesignators[typeof(T)] = designator;
		}
		return (T)designator;
	}

	public static void RenderHighlightOverSelectableCells(Designator designator, List<IntVec3> dragCells)
	{
		foreach (IntVec3 dragCell in dragCells)
		{
			Vector3 position = dragCell.ToVector3Shifted();
			position.y = AltitudeLayer.MetaOverlays.AltitudeFor();
			Graphics.DrawMesh(MeshPool.plane10, position, Quaternion.identity, DragHighlightCellMat, 0);
		}
	}

	public static void RenderHighlightOverSelectableThings(Designator designator, List<IntVec3> dragCells)
	{
		selectedThings.Clear();
		foreach (IntVec3 dragCell in dragCells)
		{
			List<Thing> thingList = dragCell.GetThingList(designator.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (designator.CanDesignateThing(thingList[i]).Accepted && !selectedThings.Contains(thingList[i]))
				{
					selectedThings.Add(thingList[i]);
					Vector3 drawPos = thingList[i].DrawPos;
					drawPos.y = AltitudeLayer.MetaOverlays.AltitudeFor();
					Graphics.DrawMesh(MeshPool.plane10, drawPos, Quaternion.identity, DragHighlightThingMat, 0);
				}
			}
		}
		selectedThings.Clear();
	}

	public static void GUIDoRotationControls(float leftX, float bottomY, Rot4 rot, Action<Rot4> rotSetter)
	{
		Rect winRect = new Rect(leftX, bottomY - 90f, 200f, 90f);
		Find.WindowStack.ImmediateWindow(73095, winRect, WindowLayer.GameUI, delegate
		{
			RotationDirection rotationDirection = RotationDirection.None;
			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Medium;
			Rect rect = new Rect(winRect.width / 2f - 64f - 5f, 15f, 64f, 64f);
			if (Widgets.ButtonImage(rect, TexUI.RotLeftTex))
			{
				SoundDefOf.DragSlider.PlayOneShotOnCamera();
				rotationDirection = RotationDirection.Counterclockwise;
				Event.current.Use();
			}
			if (!SteamDeck.IsSteamDeck)
			{
				Widgets.Label(rect, KeyBindingDefOf.Designator_RotateLeft.MainKeyLabel);
			}
			Rect rect2 = new Rect(winRect.width / 2f + 5f, 15f, 64f, 64f);
			if (Widgets.ButtonImage(rect2, TexUI.RotRightTex))
			{
				SoundDefOf.DragSlider.PlayOneShotOnCamera();
				rotationDirection = RotationDirection.Clockwise;
				Event.current.Use();
			}
			if (!SteamDeck.IsSteamDeck)
			{
				Widgets.Label(rect2, KeyBindingDefOf.Designator_RotateRight.MainKeyLabel);
			}
			if (rotationDirection != RotationDirection.None)
			{
				rot.Rotate(rotationDirection);
				rotSetter(rot);
			}
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
		});
	}
}
