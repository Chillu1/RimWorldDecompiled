using System;
using System.Collections.Generic;
using System.Text;
using LudeonTK;
using RimWorld;
using UnityEngine;

namespace Verse;

public static class DebugActionsPrefabs
{
	private static Rot4 Rotation = Rot4.North;

	private static PrefabDef buffer;

	[DebugAction("Generation", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void RotatePrefabSpawn()
	{
		Rotation.Rotate(RotationDirection.Clockwise);
		Messages.Message("Prefab rotation: " + Rotation.ToStringHuman(), MessageTypeDefOf.NeutralEvent, historical: false);
	}

	[DebugAction("Generation", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void CreatePrefab()
	{
		DebugToolsGeneral.GenericRectTool("Create", delegate(CellRect rect)
		{
			PrefabDef prefabDef = PrefabUtility.CreatePrefab(rect, DebugGenerationSettings.prefabCopyAllThings, DebugGenerationSettings.prefabCopyTerrain);
			StringBuilder stringBuilder = new StringBuilder();
			string text = "  ";
			stringBuilder.AppendLine("\n<PrefabDef>");
			stringBuilder.AppendLine(text + "<defName>NewPrefab</defName> <!-- rename -->");
			stringBuilder.AppendLine($"{text}<size>({rect.Size.x},{rect.Size.z})</size>");
			if (prefabDef.things.CountAllowNull() > 0)
			{
				stringBuilder.AppendLine(text + "<things>");
				for (int i = 0; i < prefabDef.things.Count; i++)
				{
					PrefabThingData prefabThingData = prefabDef.things[i];
					stringBuilder.AppendLine(text + text + "<" + prefabThingData.def.defName + ">");
					if (prefabThingData.rects != null)
					{
						stringBuilder.AppendLine(text + text + text + "<rects>");
						foreach (CellRect rect in prefabThingData.rects)
						{
							stringBuilder.AppendLine($"{text}{text}{text}{text}<li>{rect}</li>");
						}
						stringBuilder.AppendLine(text + text + text + "</rects>");
					}
					else if (prefabThingData.positions != null)
					{
						stringBuilder.AppendLine(text + text + text + "<positions>");
						foreach (IntVec3 position in prefabThingData.positions)
						{
							stringBuilder.AppendLine($"{text}{text}{text}{text}<li>{position}</li>");
						}
						stringBuilder.AppendLine(text + text + text + "</positions>");
					}
					else
					{
						stringBuilder.AppendLine($"{text}{text}{text}<position>{prefabThingData.position}</position>");
					}
					if (prefabThingData.relativeRotation != RotationDirection.None)
					{
						stringBuilder.AppendLine(text + text + text + "<relativeRotation>" + Enum.GetName(typeof(RotationDirection), prefabThingData.relativeRotation) + "</relativeRotation>");
					}
					if (prefabThingData.stuff != null)
					{
						stringBuilder.AppendLine(text + text + text + "<stuff>" + prefabThingData.stuff.defName + "</stuff>");
					}
					if (prefabThingData.quality.HasValue)
					{
						stringBuilder.AppendLine($"{text}{text}{text}<quality>{prefabThingData.quality}</quality>");
					}
					if (prefabThingData.hp != 0)
					{
						stringBuilder.AppendLine($"{text}{text}{text}<hp>{prefabThingData.hp}</hp>");
					}
					if (prefabThingData.stackCountRange != IntRange.One)
					{
						stringBuilder.AppendLine($"{text}{text}{text}<stackCountRange>{prefabThingData.stackCountRange.min}~{prefabThingData.stackCountRange.max}</stackCountRange>");
					}
					if (prefabThingData.colorDef != null)
					{
						stringBuilder.AppendLine($"{text}{text}{text}<colorDef>{prefabThingData.colorDef}</colorDef>");
					}
					if (prefabThingData.color != default(Color))
					{
						stringBuilder.AppendLine($"{text}{text}{text}<color>{prefabThingData.color}</color>");
					}
					stringBuilder.AppendLine(text + text + "</" + prefabThingData.def.defName + ">");
				}
				stringBuilder.AppendLine(text + "</things>");
			}
			if (prefabDef.terrain.CountAllowNull() > 0)
			{
				stringBuilder.AppendLine(text + "<terrain>");
				foreach (PrefabTerrainData item in prefabDef.terrain)
				{
					stringBuilder.AppendLine(text + text + "<" + item.def.defName + ">");
					if (item.color != null)
					{
						stringBuilder.AppendLine($"{text}{text}{text}<color>{item.color}</color>");
					}
					stringBuilder.AppendLine(text + text + text + "<rects>");
					foreach (CellRect rect2 in item.rects)
					{
						stringBuilder.AppendLine($"{text}{text}{text}{text}<li>{rect2}</li>");
					}
					stringBuilder.AppendLine(text + text + text + "</rects>");
					stringBuilder.AppendLine(text + text + "</" + item.def.defName + ">");
				}
				stringBuilder.AppendLine(text + "</terrain>");
			}
			stringBuilder.AppendLine("</PrefabDef>");
			buffer = prefabDef;
			GUIUtility.systemCopyBuffer = stringBuilder.ToString();
			Messages.Message("Copied to clipboard", MessageTypeDefOf.NeutralEvent, historical: false);
		}, closeOnComplete: true);
	}

	[DebugAction("Generation", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static List<DebugActionNode> SpawnPlayerPrefab()
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		list.Add(new DebugActionNode("Buffer", DebugActionType.ToolMap)
		{
			action = delegate
			{
				if (buffer != null)
				{
					SpawnAtMouseCell(buffer, blueprint: false, Faction.OfPlayer);
				}
			}
		});
		foreach (PrefabDef def in DefDatabase<PrefabDef>.AllDefsListForReading)
		{
			list.Add(new DebugActionNode(def.defName ?? "", DebugActionType.ToolMap)
			{
				action = delegate
				{
					SpawnAtMouseCell(def, blueprint: false, Faction.OfPlayer);
				}
			});
		}
		return list;
	}

	[DebugAction("Generation", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static List<DebugActionNode> SpawnPrefab()
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		list.Add(new DebugActionNode("Buffer", DebugActionType.ToolMap)
		{
			action = delegate
			{
				if (buffer != null)
				{
					SpawnAtMouseCell(buffer);
				}
			}
		});
		foreach (PrefabDef def in DefDatabase<PrefabDef>.AllDefsListForReading)
		{
			list.Add(new DebugActionNode(def.defName ?? "", DebugActionType.ToolMap)
			{
				action = delegate
				{
					SpawnAtMouseCell(def);
				}
			});
		}
		return list;
	}

	[DebugAction("Generation", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static List<DebugActionNode> SpawnPrefabBlueprint()
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		list.Add(new DebugActionNode("Buffer", DebugActionType.ToolMap)
		{
			action = delegate
			{
				if (buffer != null)
				{
					SpawnAtMouseCell(buffer, blueprint: true);
				}
			}
		});
		foreach (PrefabDef def in DefDatabase<PrefabDef>.AllDefsListForReading)
		{
			list.Add(new DebugActionNode(def.defName ?? "", DebugActionType.ToolMap)
			{
				action = delegate
				{
					SpawnAtMouseCell(def, blueprint: true);
				}
			});
		}
		return list;
	}

	private static void SpawnAtMouseCell(PrefabDef def, bool blueprint = false, Faction faction = null)
	{
		IntVec3 intVec = UI.MouseCell();
		CellRect cellRect = GenAdj.OccupiedRect(intVec, Rotation, def.size);
		if (!intVec.InBounds(Find.CurrentMap))
		{
			return;
		}
		foreach (IntVec3 cell in cellRect.Cells)
		{
			if (cell.InBounds(Find.CurrentMap))
			{
				Find.CurrentMap.terrainGrid.SetTerrain(cell, TerrainDefOf.MetalTile);
			}
		}
		Map currentMap = Find.CurrentMap;
		Rot4 rotation = Rotation;
		bool blueprint2 = blueprint;
		PrefabUtility.SpawnPrefab(def, currentMap, intVec, rotation, faction, null, null, null, blueprint2);
	}
}
