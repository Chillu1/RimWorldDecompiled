using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse.Sound;

namespace Verse;

public static class CameraJumper
{
	public enum MovementMode
	{
		Pan,
		Cut
	}

	public static void TryJumpAndSelect(GlobalTargetInfo target, MovementMode mode = MovementMode.Pan)
	{
		if (target.IsValid)
		{
			TryJump(target, mode);
			TrySelect(target);
		}
	}

	public static void TrySelect(GlobalTargetInfo target, bool skipTargetAdjustment = false)
	{
		if (target.IsValid)
		{
			if (!skipTargetAdjustment)
			{
				target = GetAdjustedTarget(target);
			}
			if (target.HasThing)
			{
				TrySelectInternal(target.Thing);
			}
			else if (target.HasWorldObject)
			{
				TrySelectInternal(target.WorldObject);
			}
		}
	}

	private static void TrySelectInternal(Thing thing)
	{
		if (Current.ProgramState != ProgramState.Playing || !thing.Spawned || !thing.def.selectable)
		{
			return;
		}
		bool flag = TryHideWorld();
		bool flag2 = false;
		if (thing.Map != Find.CurrentMap)
		{
			Current.Game.CurrentMap = thing.Map;
			flag2 = true;
			if (!flag)
			{
				SoundDefOf.MapSelected.PlayOneShotOnCamera();
			}
		}
		if (flag || flag2)
		{
			Find.CameraDriver.JumpToCurrentMapLoc(thing.Position);
		}
		Find.Selector.ClearSelection();
		Find.Selector.Select(thing);
	}

	private static void TrySelectInternal(WorldObject worldObject)
	{
		if (Find.World != null && worldObject.Spawned && worldObject.SelectableNow)
		{
			TryShowWorld();
			Find.WorldSelector.ClearSelection();
			Find.WorldSelector.Select(worldObject);
		}
	}

	public static void TryJump(GlobalTargetInfo target, MovementMode mode = MovementMode.Pan)
	{
		if (target.IsValid)
		{
			target = GetAdjustedTarget(target);
			if (target.HasThing)
			{
				TryJumpInternal(target.Thing, mode);
			}
			else if (target.HasWorldObject)
			{
				TryJumpInternal(target.WorldObject);
			}
			else if (target.Cell.IsValid)
			{
				TryJumpInternal(target.Cell, target.Map, mode);
			}
			else
			{
				TryJumpInternal(target.Tile);
			}
		}
	}

	public static void TryJump(IntVec3 cell, Map map, MovementMode mode = MovementMode.Pan)
	{
		TryJump(new GlobalTargetInfo(cell, map), mode);
	}

	public static void TryJump(PlanetTile tile, MovementMode mode = MovementMode.Pan)
	{
		TryJump(new GlobalTargetInfo(tile), mode);
	}

	private static void TryJumpInternal(Thing thing, MovementMode mode)
	{
		TryJumpInternal(thing.PositionHeld, thing.MapHeld, mode);
	}

	private static void TryJumpInternal(IntVec3 cell, Map map, MovementMode mode)
	{
		if (Current.ProgramState != ProgramState.Playing || !cell.IsValid || map == null || !Find.Maps.Contains(map) || !cell.InBounds(map))
		{
			return;
		}
		bool flag = TryHideWorld();
		bool flag2 = false;
		if (Find.CurrentMap != map)
		{
			Current.Game.CurrentMap = map;
			flag2 = true;
			if (!flag)
			{
				SoundDefOf.MapSelected.PlayOneShotOnCamera();
			}
		}
		JumpLocalInternal(cell, (!Prefs.SmoothCameraJumps || flag || flag2) ? MovementMode.Cut : mode);
	}

	private static void TryJumpInternal(WorldObject worldObject)
	{
		TryJumpInternal(worldObject.Tile);
	}

	private static void TryJumpInternal(PlanetTile tile)
	{
		if (Find.World != null && tile.Valid)
		{
			TryShowWorld();
			Find.WorldCameraDriver.JumpTo(tile);
		}
	}

	private static void JumpLocalInternal(IntVec3 localCell, MovementMode mode)
	{
		switch (mode)
		{
		case MovementMode.Pan:
			Find.CameraDriver.PanToMapLoc(localCell);
			break;
		default:
			Find.CameraDriver.JumpToCurrentMapLoc(localCell);
			break;
		}
	}

	public static bool CanJump(GlobalTargetInfo target)
	{
		if (!target.IsValid)
		{
			return false;
		}
		target = GetAdjustedTarget(target);
		if (target.HasThing)
		{
			if (target.Thing.MapHeld != null && Find.Maps.Contains(target.Thing.MapHeld) && target.Thing.PositionHeld.IsValid)
			{
				return target.Thing.PositionHeld.InBounds(target.Thing.MapHeld);
			}
			return false;
		}
		if (target.HasWorldObject)
		{
			return target.WorldObject.Spawned;
		}
		if (target.Cell.IsValid)
		{
			if (target.Map != null && Find.Maps.Contains(target.Map) && target.Cell.IsValid)
			{
				return target.Cell.InBounds(target.Map);
			}
			return false;
		}
		return target.Tile.Valid;
	}

	public static GlobalTargetInfo GetAdjustedTarget(GlobalTargetInfo target)
	{
		if (target.HasThing)
		{
			Thing thing = target.Thing;
			if (thing.Spawned)
			{
				return thing;
			}
			GlobalTargetInfo result = GlobalTargetInfo.Invalid;
			for (IThingHolder parentHolder = thing.ParentHolder; parentHolder != null; parentHolder = parentHolder.ParentHolder)
			{
				if (parentHolder is Thing { Spawned: not false } thing2)
				{
					result = thing2;
					break;
				}
				if (parentHolder is ThingComp thingComp && thingComp.parent.Spawned)
				{
					result = thingComp.parent;
					break;
				}
				if (parentHolder is WorldObject { Spawned: not false } worldObject)
				{
					result = worldObject;
					break;
				}
			}
			if (result.IsValid)
			{
				return result;
			}
			if (target.Thing.TryGetComp<CompCauseGameCondition>() != null)
			{
				List<Site> sites = Find.WorldObjects.Sites;
				for (int i = 0; i < sites.Count; i++)
				{
					for (int j = 0; j < sites[i].parts.Count; j++)
					{
						if (sites[i].parts[j].conditionCauser == target.Thing)
						{
							return sites[i];
						}
					}
				}
			}
			if (thing.Tile.Valid)
			{
				return new GlobalTargetInfo(thing.Tile);
			}
		}
		else if (target.Cell.IsValid && target.Tile.Valid && target.Map != null && !Find.Maps.Contains(target.Map))
		{
			MapParent parent = target.Map.Parent;
			if (parent != null && parent.Spawned)
			{
				return parent;
			}
			return GlobalTargetInfo.Invalid;
		}
		return target;
	}

	public static GlobalTargetInfo GetWorldTarget(GlobalTargetInfo target)
	{
		GlobalTargetInfo adjustedTarget = GetAdjustedTarget(target);
		if (adjustedTarget.IsValid)
		{
			if (adjustedTarget.IsWorldTarget)
			{
				return adjustedTarget;
			}
			return GetWorldTargetOfMap(adjustedTarget.Map);
		}
		return GlobalTargetInfo.Invalid;
	}

	public static GlobalTargetInfo GetWorldTargetOfMap(Map map)
	{
		if (map == null)
		{
			return GlobalTargetInfo.Invalid;
		}
		if (map.Parent != null && map.Parent.Spawned)
		{
			return map.Parent;
		}
		return GlobalTargetInfo.Invalid;
	}

	public static bool TryHideWorld()
	{
		if (!WorldRendererUtility.WorldSelected)
		{
			return false;
		}
		if (Current.ProgramState != ProgramState.Playing)
		{
			return false;
		}
		if (Find.World.renderer.wantedMode != WorldRenderMode.None)
		{
			Find.World.renderer.wantedMode = WorldRenderMode.None;
			SoundDefOf.TabClose.PlayOneShotOnCamera();
			Find.MainTabsRoot.EscapeCurrentTab(playSound: false);
			return true;
		}
		return false;
	}

	public static bool TryShowWorld()
	{
		if (WorldRendererUtility.WorldSelected)
		{
			return true;
		}
		if (Current.ProgramState != ProgramState.Playing)
		{
			return false;
		}
		if (Find.World.renderer.wantedMode == WorldRenderMode.None)
		{
			AmbientSoundManager.EnsureWorldAmbientSoundCreated();
			Find.World.renderer.wantedMode = WorldRenderMode.Planet;
			SoundDefOf.TabOpen.PlayOneShotOnCamera();
			Find.MainTabsRoot.EscapeCurrentTab(playSound: false);
			return true;
		}
		return false;
	}
}
