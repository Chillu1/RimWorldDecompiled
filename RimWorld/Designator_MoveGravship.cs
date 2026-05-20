using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_MoveGravship : Designator
	{
		public GravshipLandingMarker marker;

		public Map map;

		private Rot4 deselectedRotation;

		private AcceptanceReport lastAcceptanceReport;

		private static readonly Color diagonalsColorValid = ColorLibrary.White.WithAlpha(0.5f);

		private static readonly Color diagonalsColorInvalid = ColorLibrary.Red.WithAlpha(0.5f);

		private readonly List<IntVec3> tmpValidGravshipCells = new List<IntVec3>();

		private readonly List<IntVec3> tmpInvalidGravshipCells = new List<IntVec3>();

		private readonly List<IntVec3> tmpThrusterCells = new List<IntVec3>();

		private static float middleMouseDownTime;

		public override string Label => "DesignatorMoveGravship".Translate();

		public override string Desc => "DesignatorMoveGravshipDesc".Translate();

		public override float Order => -100f;

		public override bool AlwaysDoGuiControls => true;

		public float PaneTopY => UI.screenHeight - 35;

		public IntVec3 AdjustedMouseCell => GetSizeRotAdjustedCell(UI.MouseCell());

		public Designator_MoveGravship(Map map, GravshipLandingMarker marker)
		{
			this.marker = marker;
			this.map = map ?? marker.Map;
		}

		private IntVec3 GetSizeRotAdjustedCell(IntVec3 cell)
		{
			IntVec3 intVec = new IntVec3(marker.gravship.Bounds.Size.x / 2, 0, 0);
			IntVec3 intVec2 = new IntVec3(0, 0, marker.gravship.Bounds.Size.z / 2);
			if (marker.GravshipRotation == Rot4.North)
			{
				cell += intVec + intVec2;
			}
			else if (marker.GravshipRotation == Rot4.East)
			{
				cell += intVec - intVec2;
			}
			else if (marker.GravshipRotation == Rot4.South)
			{
				cell += -intVec - intVec2;
			}
			else if (marker.GravshipRotation == Rot4.West)
			{
				cell += -intVec + intVec2;
			}
			return cell;
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 loc)
		{
			return ValidGravshipLocation(loc);
		}

		private AcceptanceReport ValidGravshipLocation(IntVec3 loc, List<IntVec3> validCells = null, List<IntVec3> invalidCells = null)
		{
			Map currentMap = Find.CurrentMap;
			IntVec3 root = PrefabUtility.GetRoot(GetSizeRotAdjustedCell(loc), marker.gravship.Bounds.Size, marker.GravshipRotation);
			bool flag = false;
			AcceptanceReport result = true;
			foreach (IntVec3 gravshipCell in marker.GravshipCells)
			{
				IntVec3 intVec = root + gravshipCell;
				if (!flag && intVec.InBounds(currentMap) && intVec.GetTerrain(currentMap) != TerrainDefOf.Space)
				{
					flag = true;
				}
				AcceptanceReport acceptanceReport = IsValidCell(intVec, currentMap);
				if (acceptanceReport.Accepted)
				{
					validCells?.Add(intVec);
					continue;
				}
				result = acceptanceReport;
				invalidCells?.Add(intVec);
				if (validCells != null || invalidCells != null)
				{
					continue;
				}
				return acceptanceReport;
			}
			if (!flag)
			{
				invalidCells?.AddRange(validCells);
				validCells?.Clear();
				return "GravshipMustBeConnectedToLand".Translate();
			}
			return result;
		}

		private static AcceptanceReport IsValidCell(IntVec3 cell, Map map)
		{
			if (!cell.InBounds(map))
			{
				return "GravshipOutOfBounds".Translate();
			}
			if (!cell.InBounds(map, 1) || cell.InNoBuildEdgeArea(map))
			{
				return "GravshipInNoBuildArea".Translate();
			}
			if (map.landingBlockers != null)
			{
				foreach (CellRect landingBlocker in map.landingBlockers)
				{
					if (landingBlocker.Contains(cell))
					{
						return "GravshipInBlockedArea".Translate();
					}
				}
			}
			if (cell.Roofed(map))
			{
				return "GravshipBlockedByRoof".Translate();
			}
			if (cell.Fogged(map))
			{
				return "GravshipBlockedByFog".Translate();
			}
			foreach (Thing thing in cell.GetThingList(map))
			{
				if (!thing.def.preventGravshipLandingOn)
				{
					BuildingProperties building = thing.def.building;
					if (building == null || building.canLandGravshipOn)
					{
						if (thing is Pawn pawn && (pawn.RaceProps.Humanlike || pawn.HostileTo(Faction.OfPlayer)))
						{
							return "GravshipBlockedBy".Translate(pawn);
						}
						continue;
					}
				}
				return "GravshipBlockedBy".Translate(thing);
			}
			if (!GenConstruct.CanBuildOnTerrain(TerrainDefOf.Substructure, cell, map, Rot4.North))
			{
				return "GravshipBlockedByTerrain".Translate(cell.GetTerrain(map));
			}
			return true;
		}

		public override void Selected()
		{
			if ((marker.Spawned && Find.CurrentMap != marker.Map) || Find.World.renderer.wantedMode == WorldRenderMode.Planet)
			{
				CameraJumper.TryJump(marker);
			}
			deselectedRotation = marker.GravshipRotation;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			marker.Position = PrefabUtility.GetRoot(GetSizeRotAdjustedCell(c), marker.gravship.Bounds.Size, marker.GravshipRotation);
			if (!marker.Spawned)
			{
				GenSpawn.Spawn(marker, marker.Position, map);
			}
			marker.Notify_Moved();
			deselectedRotation = marker.GravshipRotation;
			Find.DesignatorManager.Deselect();
		}

		public override void Deselected()
		{
			marker.GravshipRotation = deselectedRotation;
		}

		public override void DrawMouseAttachments()
		{
			if (!lastAcceptanceReport.Accepted)
			{
				string reason = lastAcceptanceReport.Reason;
				Color? textColor = ColorLibrary.RedReadable;
				Color textBgColor = new Color(0f, 0f, 0f, 0.5f);
				GenUI.DrawMouseAttachment(null, reason, 0f, default(Vector2), null, textColor, drawTextBackground: true, textBgColor);
			}
		}

		public override void SelectedUpdate()
		{
			tmpValidGravshipCells.Clear();
			tmpInvalidGravshipCells.Clear();
			lastAcceptanceReport = ValidGravshipLocation(UI.MouseCell(), tmpValidGravshipCells, tmpInvalidGravshipCells);
			tmpThrusterCells.Clear();
			tmpThrusterCells.AddRange(marker.ThrusterCells);
			IntVec3 root = PrefabUtility.GetRoot(GetSizeRotAdjustedCell(UI.MouseCell()), marker.gravship.Bounds.Size, marker.GravshipRotation);
			for (int i = 0; i < tmpThrusterCells.Count; i++)
			{
				tmpThrusterCells[i] += root;
			}
			GenDraw.DrawFieldEdges(tmpValidGravshipCells, ColorLibrary.White);
			GenDraw.DrawFieldEdges(tmpInvalidGravshipCells, ColorLibrary.Red);
			GenDraw.DrawDiagonalStripes(tmpValidGravshipCells, diagonalsColorValid);
			GenDraw.DrawDiagonalStripes(tmpInvalidGravshipCells, diagonalsColorInvalid);
			GenDraw.DrawFieldEdges(tmpThrusterCells, ColorLibrary.Orange);
			foreach (var (thing2, data2) in marker.gravship.ExteriorDoorPlacements)
			{
				GhostUtility.GhostGraphicFor(thing2.Graphic, thing2.def, Color.white).DrawFromDef(GenThing.TrueCenter(root + data2.local, data2.rotation, thing2.def.Size, AltitudeLayer.MetaOverlays.AltitudeFor()), data2.rotation, thing2.def);
			}
		}

		public override void DoExtraGuiControls(float leftX, float bottomY)
		{
			DesignatorUtility.GUIDoRotationControls(leftX, bottomY, marker.GravshipRotation, delegate(Rot4 rot)
			{
				marker.GravshipRotation = rot;
			});
		}

		public override void SelectedProcessInput(Event ev)
		{
			RotationDirection rotationDirection = RotationDirection.None;
			if (Event.current.button == 2)
			{
				if (Event.current.type == EventType.MouseDown)
				{
					Event.current.Use();
					middleMouseDownTime = Time.realtimeSinceStartup;
				}
				if (Event.current.type == EventType.MouseUp && Time.realtimeSinceStartup - middleMouseDownTime < 0.15f)
				{
					rotationDirection = RotationDirection.Clockwise;
				}
			}
			if (KeyBindingDefOf.Designator_RotateRight.KeyDownEvent)
			{
				rotationDirection = RotationDirection.Clockwise;
			}
			else if (KeyBindingDefOf.Designator_RotateLeft.KeyDownEvent)
			{
				rotationDirection = RotationDirection.Counterclockwise;
			}
			if (rotationDirection != RotationDirection.None)
			{
				marker.GravshipRotation = marker.GravshipRotation.Rotated(rotationDirection);
			}
			DesignatorUtility.GUIDoRotationControls(0f, PaneTopY, marker.GravshipRotation, delegate(Rot4 rot)
			{
				marker.GravshipRotation = rot;
			});
		}
	}
}
