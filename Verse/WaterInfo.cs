using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class WaterInfo : MapComponent
	{
		public byte[] riverOffsetMap;

		public Texture2D riverOffsetTexture;

		public List<Vector3> riverDebugData = new List<Vector3>();

		public float[] riverFlowMap;

		public CellRect riverFlowMapBounds;

		public const int RiverOffsetMapBorder = 2;

		public WaterInfo(Map map)
			: base(map)
		{
		}

		public override void MapRemoved()
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				UnityEngine.Object.Destroy(riverOffsetTexture);
			});
		}

		public void SetTextures()
		{
			Camera subcamera = Current.SubcameraDriver.GetSubcamera(SubcameraDefOf.WaterDepth);
			Shader.SetGlobalTexture(ShaderPropertyIDs.WaterOutputTex, subcamera.targetTexture);
			if (riverOffsetTexture == null && riverOffsetMap != null && riverOffsetMap.Length != 0)
			{
				riverOffsetTexture = new Texture2D(map.Size.x + 4, map.Size.z + 4, TextureFormat.RGFloat, mipChain: false);
				riverOffsetTexture.LoadRawTextureData(riverOffsetMap);
				riverOffsetTexture.wrapMode = TextureWrapMode.Clamp;
				riverOffsetTexture.Apply();
			}
			Shader.SetGlobalTexture(ShaderPropertyIDs.WaterOffsetTex, riverOffsetTexture);
		}

		public Vector3 GetWaterMovement(Vector3 position)
		{
			if (riverOffsetMap == null)
			{
				return Vector3.zero;
			}
			if (riverFlowMap == null)
			{
				GenerateRiverFlowMap();
			}
			IntVec3 intVec = new IntVec3(Mathf.FloorToInt(position.x), 0, Mathf.FloorToInt(position.z));
			IntVec3 c = new IntVec3(Mathf.FloorToInt(position.x) + 1, 0, Mathf.FloorToInt(position.z) + 1);
			if (!riverFlowMapBounds.Contains(intVec) || !riverFlowMapBounds.Contains(c))
			{
				return Vector3.zero;
			}
			int num = riverFlowMapBounds.IndexOf(intVec);
			int num2 = num + 1;
			int num3 = num + riverFlowMapBounds.Width;
			int num4 = num3 + 1;
			Vector3 a = Vector3.Lerp(new Vector3(riverFlowMap[num * 2], 0f, riverFlowMap[num * 2 + 1]), new Vector3(riverFlowMap[num2 * 2], 0f, riverFlowMap[num2 * 2 + 1]), position.x - Mathf.Floor(position.x));
			Vector3 b = Vector3.Lerp(new Vector3(riverFlowMap[num3 * 2], 0f, riverFlowMap[num3 * 2 + 1]), new Vector3(riverFlowMap[num4 * 2], 0f, riverFlowMap[num4 * 2 + 1]), position.x - Mathf.Floor(position.x));
			return Vector3.Lerp(a, b, position.z - (float)Mathf.FloorToInt(position.z));
		}

		public void GenerateRiverFlowMap()
		{
			if (riverOffsetMap == null)
			{
				return;
			}
			riverFlowMapBounds = new CellRect(-2, -2, map.Size.x + 4, map.Size.z + 4);
			riverFlowMap = new float[riverFlowMapBounds.Area * 2];
			float[] array = new float[riverFlowMapBounds.Area * 2];
			Buffer.BlockCopy(riverOffsetMap, 0, array, 0, array.Length * 4);
			for (int i = riverFlowMapBounds.minZ; i <= riverFlowMapBounds.maxZ; i++)
			{
				int newZ = (i == riverFlowMapBounds.minZ) ? i : (i - 1);
				int newZ2 = (i == riverFlowMapBounds.maxZ) ? i : (i + 1);
				float num = (i == riverFlowMapBounds.minZ || i == riverFlowMapBounds.maxZ) ? 1 : 2;
				for (int j = riverFlowMapBounds.minX; j <= riverFlowMapBounds.maxX; j++)
				{
					int newX = (j == riverFlowMapBounds.minX) ? j : (j - 1);
					int newX2 = (j == riverFlowMapBounds.maxX) ? j : (j + 1);
					float num2 = (j == riverFlowMapBounds.minX || j == riverFlowMapBounds.maxX) ? 1 : 2;
					float x = (array[riverFlowMapBounds.IndexOf(new IntVec3(newX2, 0, i)) * 2 + 1] - array[riverFlowMapBounds.IndexOf(new IntVec3(newX, 0, i)) * 2 + 1]) / num2;
					float z = (array[riverFlowMapBounds.IndexOf(new IntVec3(j, 0, newZ2)) * 2 + 1] - array[riverFlowMapBounds.IndexOf(new IntVec3(j, 0, newZ)) * 2 + 1]) / num;
					Vector3 vector = new Vector3(x, 0f, z);
					if (vector.magnitude > 0.0001f)
					{
						vector = vector.normalized / vector.magnitude;
						int num3 = riverFlowMapBounds.IndexOf(new IntVec3(j, 0, i)) * 2;
						riverFlowMap[num3] = vector.x;
						riverFlowMap[num3 + 1] = vector.z;
					}
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			DataExposeUtility.ByteArray(ref riverOffsetMap, "riverOffsetMap");
			GenerateRiverFlowMap();
		}

		public void DebugDrawRiver()
		{
			for (int i = 0; i < riverDebugData.Count; i += 2)
			{
				GenDraw.DrawLineBetween(riverDebugData[i], riverDebugData[i + 1], SimpleColor.Magenta);
			}
		}
	}
}
