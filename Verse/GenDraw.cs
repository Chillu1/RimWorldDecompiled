using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class GenDraw
	{
		public struct FillableBarRequest
		{
			public Vector3 center;

			public Vector2 size;

			public float fillPercent;

			public Material filledMat;

			public Material unfilledMat;

			public float margin;

			public Rot4 rotation;

			public Vector2 preRotationOffset;
		}

		private static readonly Material TargetSquareMatSingle = MaterialPool.MatFrom("UI/Overlays/TargetHighlight_Square", ShaderDatabase.Transparent);

		private const float TargetPulseFrequency = 8f;

		public static readonly string LineTexPath = "UI/Overlays/ThingLine";

		public static readonly string OneSidedLineTexPath = "UI/Overlays/OneSidedLine";

		private static readonly Material LineMatWhite = MaterialPool.MatFrom(LineTexPath, ShaderDatabase.Transparent, Color.white);

		private static readonly Material LineMatRed = MaterialPool.MatFrom(LineTexPath, ShaderDatabase.Transparent, Color.red);

		private static readonly Material LineMatGreen = MaterialPool.MatFrom(LineTexPath, ShaderDatabase.Transparent, Color.green);

		private static readonly Material LineMatBlue = MaterialPool.MatFrom(LineTexPath, ShaderDatabase.Transparent, Color.blue);

		private static readonly Material LineMatMagenta = MaterialPool.MatFrom(LineTexPath, ShaderDatabase.Transparent, Color.magenta);

		private static readonly Material LineMatYellow = MaterialPool.MatFrom(LineTexPath, ShaderDatabase.Transparent, Color.yellow);

		private static readonly Material LineMatCyan = MaterialPool.MatFrom(LineTexPath, ShaderDatabase.Transparent, Color.cyan);

		private static readonly Material LineMatMetaOverlay = MaterialPool.MatFrom(LineTexPath, ShaderDatabase.MetaOverlay);

		private static readonly Material WorldLineMatWhite = MaterialPool.MatFrom(LineTexPath, ShaderDatabase.WorldOverlayTransparent, Color.white, WorldMaterials.WorldLineRenderQueue);

		private static readonly Material OneSidedWorldLineMatWhite = MaterialPool.MatFrom(OneSidedLineTexPath, ShaderDatabase.WorldOverlayTransparent, Color.white, WorldMaterials.WorldLineRenderQueue);

		private const float LineWidth = 0.2f;

		private const float BaseWorldLineWidth = 0.2f;

		public static readonly Material InteractionCellMaterial = MaterialPool.MatFrom("UI/Overlays/InteractionCell", ShaderDatabase.Transparent);

		private static readonly Color InteractionCellIntensity = new Color(1f, 1f, 1f, 0.3f);

		private static List<int> cachedEdgeTiles = new List<int>();

		private static int cachedEdgeTilesForCenter = -1;

		private static int cachedEdgeTilesForRadius = -1;

		private static int cachedEdgeTilesForWorldSeed = -1;

		private static List<IntVec3> ringDrawCells = new List<IntVec3>();

		private static bool maxRadiusMessaged = false;

		private static BoolGrid fieldGrid;

		private static bool[] rotNeeded = new bool[4];

		private static readonly Material AimPieMaterial = SolidColorMaterials.SimpleSolidColorMaterial(new Color(1f, 1f, 1f, 0.3f));

		private static readonly Material ArrowMatWhite = MaterialPool.MatFrom("UI/Overlays/Arrow", ShaderDatabase.CutoutFlying, Color.white);

		public static Material CurTargetingMat
		{
			get
			{
				TargetSquareMatSingle.color = CurTargetingColor;
				return TargetSquareMatSingle;
			}
		}

		public static Color CurTargetingColor
		{
			get
			{
				float num = (float)Math.Sin(Time.time * 8f);
				num *= 0.2f;
				num += 0.8f;
				return new Color(1f, num, num);
			}
		}

		public static void DrawNoBuildEdgeLines()
		{
			DrawMapEdgeLines(10);
		}

		public static void DrawNoZoneEdgeLines()
		{
			DrawMapEdgeLines(5);
		}

		private static void DrawMapEdgeLines(int edgeDist)
		{
			float y = AltitudeLayer.MetaOverlays.AltitudeFor();
			IntVec3 size = Find.CurrentMap.Size;
			Vector3 vector = new Vector3(edgeDist, y, edgeDist);
			Vector3 vector2 = new Vector3(edgeDist, y, size.z - edgeDist);
			Vector3 vector3 = new Vector3(size.x - edgeDist, y, size.z - edgeDist);
			Vector3 vector4 = new Vector3(size.x - edgeDist, y, edgeDist);
			DrawLineBetween(vector, vector2, LineMatMetaOverlay);
			DrawLineBetween(vector2, vector3, LineMatMetaOverlay);
			DrawLineBetween(vector3, vector4, LineMatMetaOverlay);
			DrawLineBetween(vector4, vector, LineMatMetaOverlay);
		}

		public static void DrawLineBetween(Vector3 A, Vector3 B)
		{
			DrawLineBetween(A, B, LineMatWhite);
		}

		public static void DrawLineBetween(Vector3 A, Vector3 B, float layer)
		{
			DrawLineBetween(A + Vector3.up * layer, B + Vector3.up * layer, LineMatWhite);
		}

		public static void DrawLineBetween(Vector3 A, Vector3 B, SimpleColor color)
		{
			DrawLineBetween(A, B, GetLineMat(color));
		}

		public static void DrawLineBetween(Vector3 A, Vector3 B, Material mat)
		{
			if (!(Mathf.Abs(A.x - B.x) < 0.01f) || !(Mathf.Abs(A.z - B.z) < 0.01f))
			{
				Vector3 pos = (A + B) / 2f;
				if (!(A == B))
				{
					A.y = B.y;
					float z = (A - B).MagnitudeHorizontal();
					Quaternion q = Quaternion.LookRotation(A - B);
					Vector3 s = new Vector3(0.2f, 1f, z);
					Matrix4x4 matrix = default(Matrix4x4);
					matrix.SetTRS(pos, q, s);
					Graphics.DrawMesh(MeshPool.plane10, matrix, mat, 0);
				}
			}
		}

		public static void DrawCircleOutline(Vector3 center, float radius)
		{
			DrawCircleOutline(center, radius, LineMatWhite);
		}

		public static void DrawCircleOutline(Vector3 center, float radius, SimpleColor color)
		{
			DrawCircleOutline(center, radius, GetLineMat(color));
		}

		public static void DrawCircleOutline(Vector3 center, float radius, Material material)
		{
			int num = Mathf.Clamp(Mathf.RoundToInt(24f * radius), 12, 48);
			float num2 = 0f;
			float num3 = (float)Math.PI * 2f / (float)num;
			Vector3 vector = center;
			Vector3 a = center;
			for (int i = 0; i < num + 2; i++)
			{
				if (i >= 2)
				{
					DrawLineBetween(a, vector, material);
				}
				a = vector;
				vector = center;
				vector.x += Mathf.Cos(num2) * radius;
				vector.z += Mathf.Sin(num2) * radius;
				num2 += num3;
			}
		}

		private static Material GetLineMat(SimpleColor color)
		{
			switch (color)
			{
			case SimpleColor.White:
				return LineMatWhite;
			case SimpleColor.Red:
				return LineMatRed;
			case SimpleColor.Green:
				return LineMatGreen;
			case SimpleColor.Blue:
				return LineMatBlue;
			case SimpleColor.Magenta:
				return LineMatMagenta;
			case SimpleColor.Yellow:
				return LineMatYellow;
			case SimpleColor.Cyan:
				return LineMatCyan;
			default:
				return LineMatWhite;
			}
		}

		public static void DrawWorldLineBetween(Vector3 A, Vector3 B)
		{
			DrawWorldLineBetween(A, B, WorldLineMatWhite);
		}

		public static void DrawWorldLineBetween(Vector3 A, Vector3 B, Material material, float widthFactor = 1f)
		{
			if (!(Mathf.Abs(A.x - B.x) < 0.005f) || !(Mathf.Abs(A.y - B.y) < 0.005f) || !(Mathf.Abs(A.z - B.z) < 0.005f))
			{
				Vector3 pos = (A + B) / 2f;
				float magnitude = (A - B).magnitude;
				Quaternion q = Quaternion.LookRotation(A - B, pos.normalized);
				Vector3 s = new Vector3(0.2f * Find.WorldGrid.averageTileSize * widthFactor, 1f, magnitude);
				Matrix4x4 matrix = default(Matrix4x4);
				matrix.SetTRS(pos, q, s);
				Graphics.DrawMesh(MeshPool.plane10, matrix, material, WorldCameraManager.WorldLayer);
			}
		}

		public static void DrawWorldRadiusRing(int center, int radius)
		{
			if (radius >= 0)
			{
				if (cachedEdgeTilesForCenter != center || cachedEdgeTilesForRadius != radius || cachedEdgeTilesForWorldSeed != Find.World.info.Seed)
				{
					cachedEdgeTilesForCenter = center;
					cachedEdgeTilesForRadius = radius;
					cachedEdgeTilesForWorldSeed = Find.World.info.Seed;
					cachedEdgeTiles.Clear();
					Find.WorldFloodFiller.FloodFill(center, (int tile) => true, delegate(int tile, int dist)
					{
						if (dist > radius + 1)
						{
							return true;
						}
						if (dist == radius + 1)
						{
							cachedEdgeTiles.Add(tile);
						}
						return false;
					});
					WorldGrid worldGrid = Find.WorldGrid;
					Vector3 c = worldGrid.GetTileCenter(center);
					Vector3 i = c.normalized;
					cachedEdgeTiles.Sort(delegate(int a, int b)
					{
						float num = Vector3.Dot(i, Vector3.Cross(worldGrid.GetTileCenter(a) - c, worldGrid.GetTileCenter(b) - c));
						if (Mathf.Abs(num) < 0.0001f)
						{
							return 0;
						}
						return (!(num < 0f)) ? 1 : (-1);
					});
				}
				DrawWorldLineStrip(cachedEdgeTiles, OneSidedWorldLineMatWhite, 5f);
			}
		}

		public static void DrawWorldLineStrip(List<int> edgeTiles, Material material, float widthFactor)
		{
			if (edgeTiles.Count < 3)
			{
				return;
			}
			WorldGrid worldGrid = Find.WorldGrid;
			float d = 0.05f;
			for (int i = 0; i < edgeTiles.Count; i++)
			{
				int index = (i == 0) ? (edgeTiles.Count - 1) : (i - 1);
				int num = edgeTiles[index];
				int num2 = edgeTiles[i];
				if (worldGrid.IsNeighbor(num, num2))
				{
					Vector3 tileCenter = worldGrid.GetTileCenter(num);
					Vector3 tileCenter2 = worldGrid.GetTileCenter(num2);
					tileCenter += tileCenter.normalized * d;
					tileCenter2 += tileCenter2.normalized * d;
					DrawWorldLineBetween(tileCenter, tileCenter2, material, widthFactor);
				}
			}
		}

		public static void DrawTargetHighlight(LocalTargetInfo targ)
		{
			if (targ.Thing != null)
			{
				DrawTargetingHighlight_Thing(targ.Thing);
			}
			else
			{
				DrawTargetingHighlight_Cell(targ.Cell);
			}
		}

		private static void DrawTargetingHighlight_Cell(IntVec3 c)
		{
			Vector3 position = c.ToVector3ShiftedWithAltitude(AltitudeLayer.Building);
			Graphics.DrawMesh(MeshPool.plane10, position, Quaternion.identity, CurTargetingMat, 0);
		}

		private static void DrawTargetingHighlight_Thing(Thing t)
		{
			Graphics.DrawMesh(MeshPool.plane10, t.TrueCenter() + Altitudes.AltIncVect, t.Rotation.AsQuat, CurTargetingMat, 0);
		}

		public static void DrawTargetingHightlight_Explosion(IntVec3 c, float Radius)
		{
			DrawRadiusRing(c, Radius);
		}

		public static void DrawInteractionCell(ThingDef tDef, IntVec3 center, Rot4 placingRot)
		{
			if (!tDef.hasInteractionCell)
			{
				return;
			}
			IntVec3 c = ThingUtility.InteractionCellWhenAt(tDef, center, placingRot, Find.CurrentMap);
			Vector3 vector = c.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
			if (c.InBounds(Find.CurrentMap))
			{
				Building edifice = c.GetEdifice(Find.CurrentMap);
				if (edifice != null && edifice.def.building != null && edifice.def.building.isSittable)
				{
					return;
				}
			}
			if (tDef.interactionCellGraphic == null && tDef.interactionCellIcon != null)
			{
				ThingDef thingDef = tDef.interactionCellIcon;
				if (thingDef.blueprintDef != null)
				{
					thingDef = thingDef.blueprintDef;
				}
				tDef.interactionCellGraphic = thingDef.graphic.GetColoredVersion(ShaderTypeDefOf.EdgeDetect.Shader, InteractionCellIntensity, Color.white);
			}
			if (tDef.interactionCellGraphic != null)
			{
				Rot4 rot = tDef.interactionCellIconReverse ? placingRot.Opposite : placingRot;
				tDef.interactionCellGraphic.DrawFromDef(vector, rot, tDef.interactionCellIcon);
			}
			else
			{
				Graphics.DrawMesh(MeshPool.plane10, vector, Quaternion.identity, InteractionCellMaterial, 0);
			}
		}

		public static void DrawRadiusRing(IntVec3 center, float radius, Color color, Func<IntVec3, bool> predicate = null)
		{
			if (radius > GenRadial.MaxRadialPatternRadius)
			{
				if (!maxRadiusMessaged)
				{
					Log.Error("Cannot draw radius ring of radius " + radius + ": not enough squares in the precalculated list.");
					maxRadiusMessaged = true;
				}
				return;
			}
			ringDrawCells.Clear();
			int num = GenRadial.NumCellsInRadius(radius);
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec = center + GenRadial.RadialPattern[i];
				if (predicate == null || predicate(intVec))
				{
					ringDrawCells.Add(intVec);
				}
			}
			DrawFieldEdges(ringDrawCells, color);
		}

		public static void DrawRadiusRing(IntVec3 center, float radius)
		{
			DrawRadiusRing(center, radius, Color.white);
		}

		public static void DrawFieldEdges(List<IntVec3> cells)
		{
			DrawFieldEdges(cells, Color.white);
		}

		public static void DrawFieldEdges(List<IntVec3> cells, Color color)
		{
			Map currentMap = Find.CurrentMap;
			MaterialRequest req = default(MaterialRequest);
			req.shader = ShaderDatabase.Transparent;
			req.color = color;
			req.BaseTexPath = "UI/Overlays/TargetHighlight_Side";
			Material material = MaterialPool.MatFrom(req);
			material.GetTexture("_MainTex").wrapMode = TextureWrapMode.Clamp;
			if (fieldGrid == null)
			{
				fieldGrid = new BoolGrid(currentMap);
			}
			else
			{
				fieldGrid.ClearAndResizeTo(currentMap);
			}
			int x = currentMap.Size.x;
			int z = currentMap.Size.z;
			int count = cells.Count;
			for (int i = 0; i < count; i++)
			{
				if (cells[i].InBounds(currentMap))
				{
					fieldGrid[cells[i].x, cells[i].z] = true;
				}
			}
			for (int j = 0; j < count; j++)
			{
				IntVec3 c = cells[j];
				if (!c.InBounds(currentMap))
				{
					continue;
				}
				rotNeeded[0] = (c.z < z - 1 && !fieldGrid[c.x, c.z + 1]);
				rotNeeded[1] = (c.x < x - 1 && !fieldGrid[c.x + 1, c.z]);
				rotNeeded[2] = (c.z > 0 && !fieldGrid[c.x, c.z - 1]);
				rotNeeded[3] = (c.x > 0 && !fieldGrid[c.x - 1, c.z]);
				for (int k = 0; k < 4; k++)
				{
					if (rotNeeded[k])
					{
						Graphics.DrawMesh(MeshPool.plane10, c.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays), new Rot4(k).AsQuat, material, 0);
					}
				}
			}
		}

		public static void DrawAimPie(Thing shooter, LocalTargetInfo target, int degreesWide, float offsetDist)
		{
			float facing = 0f;
			if (target.Cell != shooter.Position)
			{
				facing = ((target.Thing == null) ? (target.Cell - shooter.Position).AngleFlat : (target.Thing.DrawPos - shooter.Position.ToVector3Shifted()).AngleFlat());
			}
			DrawAimPieRaw(shooter.DrawPos + new Vector3(0f, offsetDist, 0f), facing, degreesWide);
		}

		public static void DrawAimPieRaw(Vector3 center, float facing, int degreesWide)
		{
			if (degreesWide > 0)
			{
				if (degreesWide > 360)
				{
					degreesWide = 360;
				}
				center += Quaternion.AngleAxis(facing, Vector3.up) * Vector3.forward * 0.8f;
				Graphics.DrawMesh(MeshPool.pies[degreesWide], center, Quaternion.AngleAxis(facing + (float)(degreesWide / 2) - 90f, Vector3.up), AimPieMaterial, 0);
			}
		}

		public static void DrawCooldownCircle(Vector3 center, float radius)
		{
			Vector3 s = new Vector3(radius, 1f, radius);
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(center, Quaternion.identity, s);
			Graphics.DrawMesh(MeshPool.circle, matrix, AimPieMaterial, 0);
		}

		public static void DrawFillableBar(FillableBarRequest r)
		{
			Vector2 vector = r.preRotationOffset.RotatedBy(r.rotation.AsAngle);
			r.center += new Vector3(vector.x, 0f, vector.y);
			if (r.rotation == Rot4.South)
			{
				r.rotation = Rot4.North;
			}
			if (r.rotation == Rot4.West)
			{
				r.rotation = Rot4.East;
			}
			Vector3 s = new Vector3(r.size.x + r.margin, 1f, r.size.y + r.margin);
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(r.center, r.rotation.AsQuat, s);
			Graphics.DrawMesh(MeshPool.plane10, matrix, r.unfilledMat, 0);
			if (r.fillPercent > 0.001f)
			{
				s = new Vector3(r.size.x * r.fillPercent, 1f, r.size.y);
				matrix = default(Matrix4x4);
				Vector3 pos = r.center + Vector3.up * 0.01f;
				if (!r.rotation.IsHorizontal)
				{
					pos.x -= r.size.x * 0.5f;
					pos.x += 0.5f * r.size.x * r.fillPercent;
				}
				else
				{
					pos.z -= r.size.x * 0.5f;
					pos.z += 0.5f * r.size.x * r.fillPercent;
				}
				matrix.SetTRS(pos, r.rotation.AsQuat, s);
				Graphics.DrawMesh(MeshPool.plane10, matrix, r.filledMat, 0);
			}
		}

		public static void DrawMeshNowOrLater(Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow)
		{
			if (drawNow)
			{
				mat.SetPass(0);
				Graphics.DrawMeshNow(mesh, loc, quat);
			}
			else
			{
				Graphics.DrawMesh(mesh, loc, quat, mat, 0);
			}
		}

		public static void DrawArrowPointingAt(Vector3 mapTarget, bool offscreenOnly = false)
		{
			Vector3 vector = UI.UIToMapPosition(UI.screenWidth / 2, UI.screenHeight / 2);
			if ((vector - mapTarget).MagnitudeHorizontalSquared() < 81f)
			{
				if (!offscreenOnly)
				{
					Vector3 position = mapTarget;
					position.y = AltitudeLayer.MetaOverlays.AltitudeFor();
					position.z -= 1.5f;
					Graphics.DrawMesh(MeshPool.plane20, position, Quaternion.identity, ArrowMatWhite, 0);
				}
			}
			else
			{
				Vector3 normalized = (mapTarget - vector).Yto0().normalized;
				Vector3 position2 = vector + normalized * 7f;
				position2.y = AltitudeLayer.MetaOverlays.AltitudeFor();
				Quaternion rotation = Quaternion.LookRotation(normalized);
				Graphics.DrawMesh(MeshPool.plane20, position2, rotation, ArrowMatWhite, 0);
			}
		}
	}
}
