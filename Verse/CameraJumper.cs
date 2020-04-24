using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse.Sound;

namespace Verse
{
	public static class CameraJumper
	{
		public static void TryJumpAndSelect(GlobalTargetInfo target)
		{
			if (target.IsValid)
			{
				TryJump(target);
				TrySelect(target);
			}
		}

		public static void TrySelect(GlobalTargetInfo target)
		{
			if (target.IsValid)
			{
				target = GetAdjustedTarget(target);
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
			if (flag | flag2)
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

		public static void TryJump(GlobalTargetInfo target)
		{
			if (target.IsValid)
			{
				target = GetAdjustedTarget(target);
				if (target.HasThing)
				{
					TryJumpInternal(target.Thing);
				}
				else if (target.HasWorldObject)
				{
					TryJumpInternal(target.WorldObject);
				}
				else if (target.Cell.IsValid)
				{
					TryJumpInternal(target.Cell, target.Map);
				}
				else
				{
					TryJumpInternal(target.Tile);
				}
			}
		}

		public static void TryJump(IntVec3 cell, Map map)
		{
			TryJump(new GlobalTargetInfo(cell, map));
		}

		public static void TryJump(int tile)
		{
			TryJump(new GlobalTargetInfo(tile));
		}

		private static void TryJumpInternal(Thing thing)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return;
			}
			Map mapHeld = thing.MapHeld;
			if (mapHeld == null || !Find.Maps.Contains(mapHeld) || !thing.PositionHeld.IsValid || !thing.PositionHeld.InBounds(mapHeld))
			{
				return;
			}
			bool flag = TryHideWorld();
			if (Find.CurrentMap != mapHeld)
			{
				Current.Game.CurrentMap = mapHeld;
				if (!flag)
				{
					SoundDefOf.MapSelected.PlayOneShotOnCamera();
				}
			}
			Find.CameraDriver.JumpToCurrentMapLoc(thing.PositionHeld);
		}

		private static void TryJumpInternal(IntVec3 cell, Map map)
		{
			if (Current.ProgramState != ProgramState.Playing || !cell.IsValid || map == null || !Find.Maps.Contains(map) || !cell.InBounds(map))
			{
				return;
			}
			bool flag = TryHideWorld();
			if (Find.CurrentMap != map)
			{
				Current.Game.CurrentMap = map;
				if (!flag)
				{
					SoundDefOf.MapSelected.PlayOneShotOnCamera();
				}
			}
			Find.CameraDriver.JumpToCurrentMapLoc(cell);
		}

		private static void TryJumpInternal(WorldObject worldObject)
		{
			if (Find.World != null && worldObject.Tile >= 0)
			{
				TryShowWorld();
				Find.WorldCameraDriver.JumpTo(worldObject.Tile);
			}
		}

		private static void TryJumpInternal(int tile)
		{
			if (Find.World != null && tile >= 0)
			{
				TryShowWorld();
				Find.WorldCameraDriver.JumpTo(tile);
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
			return target.Tile >= 0;
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
					Thing thing2 = parentHolder as Thing;
					if (thing2 != null && thing2.Spawned)
					{
						result = thing2;
						break;
					}
					ThingComp thingComp = parentHolder as ThingComp;
					if (thingComp != null && thingComp.parent.Spawned)
					{
						result = thingComp.parent;
						break;
					}
					WorldObject worldObject = parentHolder as WorldObject;
					if (worldObject != null && worldObject.Spawned)
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
				if (thing.Tile >= 0)
				{
					return new GlobalTargetInfo(thing.Tile);
				}
			}
			else if (target.Cell.IsValid && target.Tile >= 0 && target.Map != null && !Find.Maps.Contains(target.Map))
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
			if (!WorldRendererUtility.WorldRenderedNow)
			{
				return false;
			}
			if (Current.ProgramState != ProgramState.Playing)
			{
				return false;
			}
			if (Find.World.renderer.wantedMode != 0)
			{
				Find.World.renderer.wantedMode = WorldRenderMode.None;
				SoundDefOf.TabClose.PlayOneShotOnCamera();
				return true;
			}
			return false;
		}

		public static bool TryShowWorld()
		{
			if (WorldRendererUtility.WorldRenderedNow)
			{
				return true;
			}
			if (Current.ProgramState != ProgramState.Playing)
			{
				return false;
			}
			if (Find.World.renderer.wantedMode == WorldRenderMode.None)
			{
				Find.World.renderer.wantedMode = WorldRenderMode.Planet;
				SoundDefOf.TabOpen.PlayOneShotOnCamera();
				return true;
			}
			return false;
		}
	}
}
