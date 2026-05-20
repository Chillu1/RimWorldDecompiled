using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class GenWorld
{
	private static PlanetTile cachedTile_noSnap = PlanetTile.Invalid;

	private static int cachedFrame_noSnap = -1;

	private static PlanetTile cachedTile_snap = PlanetTile.Invalid;

	private static int cachedFrame_snap = -1;

	private static PlanetLayer cachedLayer;

	public const float MaxRayLength = 1500f;

	private static readonly List<WorldObject> tmpWorldObjectsUnderMouse = new List<WorldObject>();

	public static PlanetTile MouseTile(bool snapToExpandableWorldObjects = false)
	{
		PlanetLayer selected = PlanetLayer.Selected;
		if (snapToExpandableWorldObjects)
		{
			if (cachedFrame_snap == Time.frameCount && cachedLayer == selected)
			{
				return cachedTile_snap;
			}
			cachedTile_snap = TileAt(UI.MousePositionOnUI, snapToExpandableWorldObjects: true);
			cachedFrame_snap = Time.frameCount;
			cachedLayer = selected;
			return cachedTile_snap;
		}
		if (cachedFrame_noSnap == Time.frameCount && cachedLayer == selected)
		{
			return cachedTile_noSnap;
		}
		cachedTile_noSnap = TileAt(UI.MousePositionOnUI);
		cachedFrame_noSnap = Time.frameCount;
		cachedLayer = selected;
		return cachedTile_noSnap;
	}

	public static PlanetTile TileAt(Vector2 clickPos, bool snapToExpandableWorldObjects = false)
	{
		Camera worldCamera = Find.WorldCamera;
		if (!worldCamera.gameObject.activeInHierarchy)
		{
			return PlanetTile.Invalid;
		}
		if (snapToExpandableWorldObjects)
		{
			ExpandableWorldObjectsUtility.GetExpandedWorldObjectUnderMouse(UI.MousePositionOnUI, tmpWorldObjectsUnderMouse);
			if (tmpWorldObjectsUnderMouse.Any())
			{
				PlanetTile tile = tmpWorldObjectsUnderMouse[0].Tile;
				tmpWorldObjectsUnderMouse.Clear();
				return tile;
			}
		}
		Ray ray = worldCamera.ScreenPointToRay(clickPos * Prefs.UIScale);
		int worldLayerMask = WorldCameraManager.WorldLayerMask;
		WorldTerrainColliderManager.EnsureRaycastCollidersUpdated();
		if (Physics.Raycast(ray, out var hitInfo, 1500f, worldLayerMask))
		{
			return Find.World.renderer.GetTileFromRayHit(hitInfo);
		}
		return PlanetTile.Invalid;
	}

	public static string GetPollutionDescription(float pollution)
	{
		if (pollution <= 0f)
		{
			return "TilePollutionNone".Translate();
		}
		if (pollution <= 0.333f)
		{
			return "TilePollutionLight".Translate();
		}
		if (pollution <= 0.666f)
		{
			return "TilePollutionModerate".Translate();
		}
		return "TilePollutionExtreme".Translate();
	}
}
