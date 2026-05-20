using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

[StaticConstructorOnStartup]
public static class WorldGizmoUtility
{
	private static readonly Texture2D FormCaravanCommand = ContentFinder<Texture2D>.Get("UI/Commands/FormCaravan");

	private static readonly Texture2D JumpToCommand = ContentFinder<Texture2D>.Get("UI/Commands/JumpTo");

	private static Gizmo mouseoverGizmo;

	private static Gizmo lastMouseOverGizmo;

	private static readonly List<object> tmpObjectsList = new List<object>();

	private static readonly List<MapParent> settlements = new List<MapParent>();

	private static HashSet<WorldObject> tmpJumpTargets = new HashSet<WorldObject>();

	private static readonly List<MapParent> possible = new List<MapParent>();

	public static Gizmo LastMouseOverGizmo => lastMouseOverGizmo;

	public static void WorldUIOnGUI()
	{
		tmpObjectsList.Clear();
		foreach (Gizmo gizmo2 in Find.WorldGrid.GetGizmos())
		{
			tmpObjectsList.Add(gizmo2);
		}
		if (!Find.TilePicker.Active)
		{
			WorldRoutePlanner worldRoutePlanner = Find.WorldRoutePlanner;
			List<WorldObject> selectedObjects = Find.WorldSelector.SelectedObjects;
			if (TryGetCaravanGizmo(out var gizmo))
			{
				tmpObjectsList.Add(gizmo);
			}
			for (int i = 0; i < selectedObjects.Count; i++)
			{
				if (!worldRoutePlanner.Active || selectedObjects[i] is RoutePlannerWaypoint)
				{
					tmpObjectsList.Add(selectedObjects[i]);
				}
			}
			if (Find.WorldSelector.SelectedTile != PlanetTile.Invalid)
			{
				tmpObjectsList.AddRange(Find.WorldSelector.SelectedTile.Tile.GetGizmos());
			}
		}
		GizmoGridDrawer.DrawGizmoGridFor(tmpObjectsList, out mouseoverGizmo);
		tmpObjectsList.Clear();
	}

	public static void WorldUIUpdate()
	{
		lastMouseOverGizmo = mouseoverGizmo;
		if (mouseoverGizmo != null)
		{
			mouseoverGizmo.GizmoUpdateOnMouseover();
		}
	}

	public static Gizmo GetJumpToGizmo()
	{
		return new Command_Action
		{
			defaultLabel = "WorldJumpTo".Translate() + "...",
			defaultDesc = "WorldJumpToDesc".Translate(),
			icon = JumpToCommand,
			action = delegate
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				tmpJumpTargets.Clear();
				foreach (Map m in Find.Maps)
				{
					if (!tmpJumpTargets.Contains(m.Parent) && !m.IsPocketMap)
					{
						string text = m.Parent.LabelCap;
						if (GravshipUtility.TryGetNameOfGravshipOnMap(m, out var name))
						{
							text = text + " (" + name + ")";
						}
						list.Add(new FloatMenuOption(text, delegate
						{
							CameraJumper.TryJumpAndSelect(m.Parent);
						}, m.Parent.ExpandingIcon, m.Parent.ExpandingIconColor));
						tmpJumpTargets.Add(m.Parent);
					}
				}
				foreach (Quest item in Find.QuestManager.questsInDisplayOrder)
				{
					foreach (QuestPart item2 in item.PartsListForReading)
					{
						QuestPart_SpawnWorldObject spawnPart = item2 as QuestPart_SpawnWorldObject;
						if (spawnPart != null && spawnPart.worldObject != null && !spawnPart.worldObject.Destroyed && spawnPart.worldObject.Spawned && !tmpJumpTargets.Contains(spawnPart.worldObject))
						{
							list.Add(new FloatMenuOption(spawnPart.worldObject.LabelCap, delegate
							{
								CameraJumper.TryJumpAndSelect(spawnPart.worldObject);
							}, spawnPart.worldObject.ExpandingIcon, spawnPart.worldObject.ExpandingIconColor));
							tmpJumpTargets.Add(spawnPart.worldObject);
						}
					}
				}
				foreach (Caravan caravan in Find.WorldObjects.Caravans)
				{
					if (!tmpJumpTargets.Contains(caravan))
					{
						list.Add(new FloatMenuOption(caravan.LabelCap, delegate
						{
							CameraJumper.TryJumpAndSelect(caravan);
						}, caravan.ExpandingIcon, caravan.ExpandingIconColor));
						tmpJumpTargets.Add(caravan);
					}
				}
				if (!list.Any())
				{
					list.Add(new FloatMenuOption("NothingToJumpTo".Translate(), null));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
		};
	}

	public static bool TryGetCaravanGizmo(out Gizmo gizmo)
	{
		gizmo = null;
		if (Find.WorldRoutePlanner.Active)
		{
			return false;
		}
		if (!PlanetLayer.Selected.Def.canFormCaravans)
		{
			return false;
		}
		List<WorldObject> selectedObjects = Find.WorldSelector.SelectedObjects;
		PlanetTile tile = Find.WorldSelector.SelectedTile;
		Settlement settlement = null;
		foreach (WorldObject item in selectedObjects)
		{
			if (item is Caravan || item is CaravansBattlefield || item is TravellingTransporters)
			{
				return false;
			}
			if (item is Settlement settlement2)
			{
				settlement = settlement2;
			}
			FormCaravanComp component = item.GetComponent<FormCaravanComp>();
			if (component != null && component.CanReformNow())
			{
				return false;
			}
			if (!tile.Valid)
			{
				tile = item.Tile;
			}
		}
		RefreshSettlements();
		if (settlements.Count == 0 || Find.TilePicker.ForGravship)
		{
			return false;
		}
		settlement = settlement ?? Find.WorldObjects.SettlementAt(tile);
		if (settlement != null)
		{
			if (settlement.Faction != Faction.OfPlayer)
			{
				gizmo = SendToTileGizmo(settlement.Tile);
			}
			else if (settlement.Map != null)
			{
				gizmo = GetFormCaravanAction("CommandFormCaravan".Translate(), "CommandFormCaravanDesc".Translate(), delegate
				{
					Find.WindowStack.Add(new Dialog_FormCaravan(settlement.Map));
				});
			}
			return true;
		}
		MapParent parent = Find.WorldObjects.MapParentAt(tile);
		FormCaravanComp formCaravanComp = parent?.GetComponent<FormCaravanComp>();
		if (formCaravanComp != null && parent.HasMap && parent.Map.mapPawns.ColonistCount > 0 && !formCaravanComp.Reform)
		{
			gizmo = GetFormCaravanAction("CommandFormCaravan".Translate(), "CommandFormCaravanDesc".Translate(), delegate
			{
				Dialog_FormCaravan window = new Dialog_FormCaravan(parent.Map);
				Find.WindowStack.Add(window);
			});
			return true;
		}
		if (tile.Valid)
		{
			gizmo = SendToTileGizmo(tile);
			return true;
		}
		gizmo = GetFormCaravanAction("CommandFormCaravan".Translate(), "CommandFormCaravanDesc".Translate(), delegate
		{
			Dialog_FormCaravan window = new Dialog_FormCaravan(settlements[0].Map);
			Find.WindowStack.Add(window);
		});
		return true;
	}

	private static Gizmo SendToTileGizmo(PlanetTile tile)
	{
		possible.Clear();
		if (!PlanetLayer.Selected.Def.canFormCaravans)
		{
			return null;
		}
		if (!Find.World.Impassable(tile) || Find.WorldObjects.AnySettlementAt(tile))
		{
			for (int i = 0; i < settlements.Count; i++)
			{
				if (settlements[i].Tile.LayerDef.canFormCaravans && Find.WorldReachability.CanReach(settlements[i].Tile, tile))
				{
					possible.Add(settlements[i]);
				}
			}
		}
		TaggedString taggedString = "CommandSendCaravanDesc".Translate();
		Gizmo gizmo = ((possible.Count <= 1) ? GetFormCaravanAction("CommandSendCaravan".Translate(), taggedString, delegate
		{
			DialogFromToSettlement(possible[0].Map, tile);
		}) : GetFormCaravanAction("CommandSendCaravanMultiple".Translate(), taggedString, delegate
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			for (int j = 0; j < possible.Count; j++)
			{
				MapParent settlement = possible[j];
				list.Add(new FloatMenuOption(settlement.LabelCap, delegate
				{
					DialogFromToSettlement(settlement.Map, tile);
				}));
			}
			FloatMenu window = new FloatMenu(list)
			{
				absorbInputAroundWindow = true
			};
			Find.WindowStack.Add(window);
		}));
		if (!possible.Any())
		{
			gizmo.Disable("CommandSendCaravanCantReach".Translate());
		}
		return gizmo;
	}

	private static void DialogFromToSettlement(Map origin, PlanetTile tile)
	{
		Dialog_FormCaravan dialog_FormCaravan = new Dialog_FormCaravan(origin);
		Find.WindowStack.Add(dialog_FormCaravan);
		WorldRoutePlanner worldRoutePlanner = Find.WorldRoutePlanner;
		worldRoutePlanner.Start(dialog_FormCaravan);
		worldRoutePlanner.TryAddWaypoint(tile);
	}

	private static Command_Action GetFormCaravanAction(string label, string desc, Action action)
	{
		return new Command_Action
		{
			tutorTag = "FormCaravan",
			defaultLabel = label,
			defaultDesc = desc,
			icon = FormCaravanCommand,
			hotKey = KeyBindingDefOf.Misc2,
			action = action
		};
	}

	private static void RefreshSettlements()
	{
		for (int num = settlements.Count - 1; num >= 0; num--)
		{
			if (settlements[num] == null)
			{
				settlements.RemoveAt(num);
			}
			else if (!Current.Game.Maps.Contains(settlements[num].Map))
			{
				settlements.RemoveAt(num);
			}
		}
		for (int i = 0; i < Current.Game.Maps.Count; i++)
		{
			Map map = Current.Game.Maps[i];
			if (map.IsPlayerHome && !settlements.Contains(map.Parent))
			{
				settlements.Add(map.Parent);
			}
		}
	}
}
