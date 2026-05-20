using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public static class WorldRendererUtility
{
	private const float WorldIntersectionFactor = 1f;

	public static WorldRenderMode CurrentWorldRenderMode
	{
		get
		{
			if (Find.World == null)
			{
				return WorldRenderMode.None;
			}
			if (WorldComponent_GravshipController.CutsceneInProgress)
			{
				if (Find.CurrentMap != null && Find.CurrentMap.generatorDef.renderWorld)
				{
					return WorldRenderMode.Background;
				}
				return WorldRenderMode.None;
			}
			if (Current.ProgramState == ProgramState.Playing && Find.CurrentMap == null)
			{
				return WorldRenderMode.Planet;
			}
			if (Find.World.renderer.wantedMode == WorldRenderMode.Planet)
			{
				return WorldRenderMode.Planet;
			}
			if (Find.CurrentMap != null && Find.CurrentMap.generatorDef.renderWorld)
			{
				return WorldRenderMode.Background;
			}
			return Find.World.renderer.wantedMode;
		}
	}

	public static bool WorldRendered => CurrentWorldRenderMode != WorldRenderMode.None;

	public static bool WorldBackgroundNow => CurrentWorldRenderMode == WorldRenderMode.Background;

	public static bool WorldSelected => CurrentWorldRenderMode == WorldRenderMode.Planet;

	public static bool DrawingMap => CurrentWorldRenderMode != WorldRenderMode.Planet;

	public static void UpdateGlobalShadersParams()
	{
		Vector3 vector = -GenCelestial.CurSunPositionInWorldSpace();
		float value = ((WorldBackgroundNow || !PlanetLayer.Selected.IsRootSurface || Find.PlaySettings.usePlanetDayNightSystem) ? 1f : 0f);
		Shader.SetGlobalVector(ShaderPropertyIDs.PlanetSunLightDirection, vector);
		Shader.SetGlobalFloat(ShaderPropertyIDs.PlanetSunLightEnabled, value);
		Shader.SetGlobalFloat(ShaderPropertyIDs.BackgroundModeEnabled, WorldBackgroundNow ? 1f : 0f);
	}

	public static void PrintQuadTangentialToPlanet(Vector3 pos, float size, float altOffset, LayerSubMesh subMesh, bool counterClockwise = false, float rotation = 0f, bool printUVs = true)
	{
		PrintQuadTangentialToPlanet(pos, pos, size, altOffset, subMesh, counterClockwise, rotation, printUVs);
	}

	public static void PrintQuadTangentialToPlanet(Vector3 pos, Vector3 posForTangents, float size, float altOffset, LayerSubMesh subMesh, bool counterClockwise = false, float rotation = 0f, bool printUVs = true)
	{
		GetTangentsToPlanet(posForTangents, out var first, out var second, rotation);
		Vector3 normalized = posForTangents.normalized;
		float num = size * 0.5f;
		Vector3 item = pos - first * num - second * num + normalized * altOffset;
		Vector3 item2 = pos - first * num + second * num + normalized * altOffset;
		Vector3 item3 = pos + first * num + second * num + normalized * altOffset;
		Vector3 item4 = pos + first * num - second * num + normalized * altOffset;
		int count = subMesh.verts.Count;
		subMesh.verts.Add(item);
		subMesh.verts.Add(item2);
		subMesh.verts.Add(item3);
		subMesh.verts.Add(item4);
		if (printUVs)
		{
			subMesh.uvs.Add(new Vector2(0f, 0f));
			subMesh.uvs.Add(new Vector2(0f, 1f));
			subMesh.uvs.Add(new Vector2(1f, 1f));
			subMesh.uvs.Add(new Vector2(1f, 0f));
		}
		if (counterClockwise)
		{
			subMesh.tris.Add(count + 2);
			subMesh.tris.Add(count + 1);
			subMesh.tris.Add(count);
			subMesh.tris.Add(count + 3);
			subMesh.tris.Add(count + 2);
			subMesh.tris.Add(count);
		}
		else
		{
			subMesh.tris.Add(count);
			subMesh.tris.Add(count + 1);
			subMesh.tris.Add(count + 2);
			subMesh.tris.Add(count);
			subMesh.tris.Add(count + 2);
			subMesh.tris.Add(count + 3);
		}
	}

	public static void DrawQuadTangentialToPlanet(Vector3 pos, float size, float altOffset, Material material, float rotationAngle = 0f, bool counterClockwise = false, bool useSkyboxLayer = false, MaterialPropertyBlock propertyBlock = null)
	{
		if (material == null)
		{
			Log.Warning("Tried to draw quad with null material.");
			return;
		}
		Vector3 normalized = pos.normalized;
		Vector3 vector = ((!counterClockwise) ? normalized : (-normalized));
		Quaternion quaternion = Quaternion.LookRotation(Vector3.Cross(vector, Vector3.up), vector);
		Quaternion q = Quaternion.AngleAxis(rotationAngle, normalized) * quaternion;
		Matrix4x4 matrix = Matrix4x4.TRS(s: new Vector3(size, 1f, size), pos: pos + normalized * altOffset, q: q);
		int layer = (useSkyboxLayer ? WorldCameraManager.WorldSkyboxLayer : WorldCameraManager.WorldLayer);
		if (propertyBlock != null)
		{
			Graphics.DrawMesh(MeshPool.plane10, matrix, material, layer, null, 0, propertyBlock);
		}
		else
		{
			Graphics.DrawMesh(MeshPool.plane10, matrix, material, layer);
		}
	}

	public static void GetTangentsToPlanet(Vector3 pos, out Vector3 first, out Vector3 second, float rotation = 0f)
	{
		Vector3 normalized = pos.normalized;
		Vector3 upwards = Vector3.up;
		if (Mathf.Abs(Vector3.Dot(normalized, Vector3.up)) > 0.999f)
		{
			upwards = Vector3.right;
		}
		Quaternion quaternion = Quaternion.LookRotation(normalized, upwards);
		Quaternion quaternion2 = Quaternion.AngleAxis(rotation, normalized) * quaternion;
		first = quaternion2 * Vector3.up;
		second = quaternion2 * Vector3.right;
	}

	public static Vector3 ProjectOnQuadTangentialToPlanet(Vector3 center, Vector2 point)
	{
		GetTangentsToPlanet(center, out var first, out var second);
		return point.x * first + point.y * second;
	}

	public static void GetTangentialVectorFacing(Vector3 root, Vector3 pointToFace, out Vector3 forward, out Vector3 right)
	{
		Quaternion quaternion = Quaternion.LookRotation(root, pointToFace);
		forward = quaternion * Vector3.up;
		right = quaternion * Vector3.left;
	}

	public static void PrintTextureAtlasUVs(int indexX, int indexY, int numX, int numY, LayerSubMesh subMesh)
	{
		float num = 1f / (float)numX;
		float num2 = 1f / (float)numY;
		float num3 = (float)indexX * num;
		float num4 = (float)indexY * num2;
		subMesh.uvs.Add(new Vector2(num3, num4));
		subMesh.uvs.Add(new Vector2(num3, num4 + num2));
		subMesh.uvs.Add(new Vector2(num3 + num, num4 + num2));
		subMesh.uvs.Add(new Vector2(num3 + num, num4));
	}

	public static bool HiddenBehindTerrainNow(Vector3 pos)
	{
		Dictionary<int, PlanetLayer> dictionary = (Dictionary<int, PlanetLayer>)Find.WorldGrid.PlanetLayers;
		foreach (int key in dictionary.Keys)
		{
			PlanetLayer planetLayer = dictionary[key];
			if (planetLayer.Visible && planetLayer.Def.obstructsExpandingIcons && planetLayer.LineIntersects(Find.WorldCameraDriver.CameraPosition, pos))
			{
				return true;
			}
		}
		return false;
	}
}
