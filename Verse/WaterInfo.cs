using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using RimWorld;
using UnityEngine;

namespace Verse;

public class WaterInfo : MapComponent
{
	public Texture2D riverFlowTexture;

	public IntVec3 lakeCenter = IntVec3.Invalid;

	public List<RiverNode> riverGraph = new List<RiverNode>();

	public List<float> riverFlowMap;

	[Obsolete]
	public CellRect riverFlowMapBounds;

	private Color[] flowMapPixels;

	private Texture2D skyTex;

	private const string ReflectionTexture = "Other/WaterReflection";

	public WaterInfo(Map map)
		: base(map)
	{
	}

	public override void MapRemoved()
	{
		LongEventHandler.ExecuteWhenFinished(delegate
		{
			UnityEngine.Object.Destroy(riverFlowTexture);
		});
	}

	public void SetTextures()
	{
		Camera subcamera = Current.SubcameraDriver.GetSubcamera(SubcameraDefOf.WaterDepth);
		if (subcamera != null)
		{
			Shader.SetGlobalTexture(ShaderPropertyIDs.WaterOutputTex, subcamera.targetTexture);
		}
		if (skyTex == null)
		{
			skyTex = ContentFinder<Texture2D>.Get("Other/WaterReflection");
		}
		Shader.SetGlobalTexture(ShaderPropertyIDs.WaterReflectionTex, skyTex);
		Camera camera = Find.Camera;
		Vector4 value = (camera.targetTexture ? new Vector4(camera.targetTexture.width, camera.targetTexture.height, 1f / (float)camera.targetTexture.width, 1f / (float)camera.targetTexture.height) : new Vector4(Screen.width, Screen.height, 1f / (float)Screen.width, 1f / (float)Screen.height));
		Shader.SetGlobalVector(ShaderPropertyIDs.MainCameraScreenParams, value);
		Matrix4x4 worldToCameraMatrix = camera.worldToCameraMatrix;
		Matrix4x4 value2 = GL.GetGPUProjectionMatrix(camera.projectionMatrix, renderIntoTexture: false) * worldToCameraMatrix;
		Shader.SetGlobalMatrix(ShaderPropertyIDs.MainCameraVP, value2);
		if (riverFlowTexture == null && riverFlowMap != null && riverFlowMap.Count > 0)
		{
			flowMapPixels = new Color[riverFlowMap.Count / 2];
			for (int i = 0; i < map.Size.x; i++)
			{
				for (int j = 0; j < map.Size.z; j++)
				{
					IntVec3 intVec = new IntVec3(i, 0, j);
					if (!map.terrainGrid.BaseTerrainAt(intVec).IsRiver)
					{
						continue;
					}
					int num = intVec.x * map.Size.z + intVec.z;
					Vector2 vector = new Vector2(riverFlowMap[num * 2], riverFlowMap[num * 2 + 1]);
					int num2 = 1;
					foreach (IntVec3 item in GenAdjFast.AdjacentCells8Way(intVec))
					{
						if (item.InBounds(map) && map.terrainGrid.BaseTerrainAt(intVec).IsRiver)
						{
							int num3 = item.x * map.Size.z + item.z;
							vector += new Vector2(riverFlowMap[num3 * 2], riverFlowMap[num3 * 2 + 1]);
							num2++;
						}
					}
					flowMapPixels[j * map.Size.x + i] = new Color(vector.x / (float)num2, vector.y / (float)num2, 0f);
				}
			}
			riverFlowTexture = new Texture2D(map.Size.x, map.Size.z, TextureFormat.RGFloat, mipChain: false);
			riverFlowTexture.SetPixels(flowMapPixels);
			riverFlowTexture.wrapMode = TextureWrapMode.Clamp;
			riverFlowTexture.Apply();
		}
		Shader.SetGlobalTexture(ShaderPropertyIDs.WaterOffsetTex, riverFlowTexture);
		Shader.SetGlobalVector(ShaderPropertyIDs.MapSize, new Vector4(map.Size.x, map.Size.z));
	}

	public Vector3 GetWaterMovement(Vector3 position)
	{
		if (riverFlowMap == null)
		{
			return Vector3.zero;
		}
		IntVec3 intVec = position.ToIntVec3();
		int num = intVec.x * map.Size.z + intVec.z;
		return new Vector3(riverFlowMap[num * 2], 0f, riverFlowMap[num * 2 + 1]);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			string value = null;
			if (riverFlowMap != null)
			{
				value = Convert.ToBase64String(CompressUtility.Compress(MemoryMarshal.AsBytes(riverFlowMap.AsSpan()))).AddLineBreaksToLongString();
			}
			Scribe_Values.Look(ref value, "riverFlowMapDeflate");
		}
		else if (Scribe.EnterNode("riverFlowMapDeflate"))
		{
			Scribe.ExitNode();
			string value2 = null;
			Scribe_Values.Look(ref value2, "riverFlowMapDeflate");
			if (value2 != null)
			{
				Span<float> span = MemoryMarshal.Cast<byte, float>(CompressUtility.Decompress(Convert.FromBase64String(value2)));
				riverFlowMap = new List<float>(span.Length);
				Span<float> destination = riverFlowMap.AsSpan(span.Length);
				span.CopyTo(destination);
				riverFlowMap.UnsafeSetCount(span.Length);
			}
		}
	}

	public void DebugDrawRiver()
	{
		if (riverGraph.NullOrEmpty())
		{
			return;
		}
		if (DebugViewSettings.drawRiverDebug)
		{
			foreach (RiverNode item in riverGraph)
			{
				GenDraw.DrawLineBetween(item.start, item.end, SimpleColor.Green, 5f);
				GenDraw.DrawArrowRotated(Vector3.Lerp(item.start, item.end, 0.5f), item.start.AngleToFlat(item.end) - 270f, ghost: false);
			}
		}
		if (!DebugViewSettings.drawRiverFlowDebug)
		{
			return;
		}
		foreach (IntVec3 allCell in map.AllCells)
		{
			int num = allCell.x * map.Size.z + allCell.z;
			Vector3 vector = new Vector3(riverFlowMap[num * 2], 0f, riverFlowMap[num * 2 + 1]);
			if (!(vector == Vector3.zero))
			{
				GenDraw.DrawArrowRotated(allCell.ToVector3Shifted(), vector.AngleFlat(), ghost: false);
			}
		}
	}
}
