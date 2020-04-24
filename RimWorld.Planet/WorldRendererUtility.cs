using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class WorldRendererUtility
	{
		public static WorldRenderMode CurrentWorldRenderMode
		{
			get
			{
				if (Find.World == null)
				{
					return WorldRenderMode.None;
				}
				if (Current.ProgramState == ProgramState.Playing && Find.CurrentMap == null)
				{
					return WorldRenderMode.Planet;
				}
				return Find.World.renderer.wantedMode;
			}
		}

		public static bool WorldRenderedNow => CurrentWorldRenderMode != WorldRenderMode.None;

		public static void UpdateWorldShadersParams()
		{
			Vector3 v = -GenCelestial.CurSunPositionInWorldSpace();
			float value = Find.PlaySettings.usePlanetDayNightSystem ? 1f : 0f;
			Shader.SetGlobalVector(ShaderPropertyIDs.PlanetSunLightDirection, v);
			Shader.SetGlobalFloat(ShaderPropertyIDs.PlanetSunLightEnabled, value);
			WorldMaterials.PlanetGlow.SetFloat(ShaderPropertyIDs.PlanetRadius, 100f);
			WorldMaterials.PlanetGlow.SetFloat(ShaderPropertyIDs.GlowRadius, 8f);
		}

		public static void PrintQuadTangentialToPlanet(Vector3 pos, float size, float altOffset, LayerSubMesh subMesh, bool counterClockwise = false, bool randomizeRotation = false, bool printUVs = true)
		{
			PrintQuadTangentialToPlanet(pos, pos, size, altOffset, subMesh, counterClockwise, randomizeRotation, printUVs);
		}

		public static void PrintQuadTangentialToPlanet(Vector3 pos, Vector3 posForTangents, float size, float altOffset, LayerSubMesh subMesh, bool counterClockwise = false, bool randomizeRotation = false, bool printUVs = true)
		{
			GetTangentsToPlanet(posForTangents, out Vector3 first, out Vector3 second, randomizeRotation);
			Vector3 normalized = posForTangents.normalized;
			float d = size * 0.5f;
			Vector3 item = pos - first * d - second * d + normalized * altOffset;
			Vector3 item2 = pos - first * d + second * d + normalized * altOffset;
			Vector3 item3 = pos + first * d + second * d + normalized * altOffset;
			Vector3 item4 = pos + first * d - second * d + normalized * altOffset;
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

		public static void DrawQuadTangentialToPlanet(Vector3 pos, float size, float altOffset, Material material, bool counterClockwise = false, bool useSkyboxLayer = false, MaterialPropertyBlock propertyBlock = null)
		{
			if (material == null)
			{
				Log.Warning("Tried to draw quad with null material.");
				return;
			}
			Vector3 normalized = pos.normalized;
			Vector3 vector = (!counterClockwise) ? normalized : (-normalized);
			Quaternion q = Quaternion.LookRotation(Vector3.Cross(vector, Vector3.up), vector);
			Vector3 s = new Vector3(size, 1f, size);
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(pos + normalized * altOffset, q, s);
			int layer = useSkyboxLayer ? WorldCameraManager.WorldSkyboxLayer : WorldCameraManager.WorldLayer;
			if (propertyBlock != null)
			{
				Graphics.DrawMesh(MeshPool.plane10, matrix, material, layer, null, 0, propertyBlock);
			}
			else
			{
				Graphics.DrawMesh(MeshPool.plane10, matrix, material, layer);
			}
		}

		public static void GetTangentsToPlanet(Vector3 pos, out Vector3 first, out Vector3 second, bool randomizeRotation = false)
		{
			Quaternion rotation = Quaternion.LookRotation(upwards: (!randomizeRotation) ? Vector3.up : Rand.UnitVector3, forward: pos.normalized);
			first = rotation * Vector3.up;
			second = rotation * Vector3.right;
		}

		public static Vector3 ProjectOnQuadTangentialToPlanet(Vector3 center, Vector2 point)
		{
			GetTangentsToPlanet(center, out Vector3 first, out Vector3 second);
			return point.x * first + point.y * second;
		}

		public static void GetTangentialVectorFacing(Vector3 root, Vector3 pointToFace, out Vector3 forward, out Vector3 right)
		{
			Quaternion rotation = Quaternion.LookRotation(root, pointToFace);
			forward = rotation * Vector3.up;
			right = rotation * Vector3.left;
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
			Vector3 normalized = pos.normalized;
			Vector3 currentlyLookingAtPointOnSphere = Find.WorldCameraDriver.CurrentlyLookingAtPointOnSphere;
			return Vector3.Angle(normalized, currentlyLookingAtPointOnSphere) > 73f;
		}
	}
}
