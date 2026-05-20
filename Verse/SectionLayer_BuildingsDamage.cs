using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public class SectionLayer_BuildingsDamage : SectionLayer
{
	private static List<Vector2> scratches = new List<Vector2>();

	public SectionLayer_BuildingsDamage(Section section)
		: base(section)
	{
		relevantChangeTypes = (ulong)MapMeshFlagDefOf.BuildingsDamage | (ulong)MapMeshFlagDefOf.Buildings;
	}

	public override void Regenerate()
	{
		ClearSubMeshes(MeshParts.All);
		foreach (IntVec3 item in section.CellRect)
		{
			List<Thing> list = base.Map.thingGrid.ThingsListAt(item);
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				if (list[i] is Building building && building.def.useHitPoints && building.HitPoints < building.MaxHitPoints && building.def.drawDamagedOverlay && building.Position.x == item.x && building.Position.z == item.z)
				{
					PrintDamageVisualsFrom(building);
				}
			}
		}
		FinalizeMesh(MeshParts.All);
	}

	private void PrintDamageVisualsFrom(Building b)
	{
		if (b.def.graphicData == null || b.def.graphicData.damageData == null || b.def.graphicData.damageData.enabled)
		{
			PrintScratches(b);
			PrintCornersAndEdges(b);
		}
	}

	private void PrintScratches(Building b)
	{
		int num = 0;
		List<DamageOverlay> overlays = BuildingsDamageSectionLayerUtility.GetOverlays(b);
		for (int i = 0; i < overlays.Count; i++)
		{
			if (overlays[i] == DamageOverlay.Scratch)
			{
				num++;
			}
		}
		if (num == 0)
		{
			return;
		}
		Rect damageRect = BuildingsDamageSectionLayerUtility.GetDamageRect(b);
		float num2 = Mathf.Min(0.5f * Mathf.Min(damageRect.width, damageRect.height), 1f);
		damageRect = damageRect.ContractedBy(num2 / 2f);
		if (damageRect.width <= 0f || damageRect.height <= 0f)
		{
			return;
		}
		float minDist = Mathf.Max(damageRect.width, damageRect.height) * 0.7f;
		scratches.Clear();
		Rand.PushState();
		Rand.Seed = b.thingIDNumber * 3697;
		for (int j = 0; j < num; j++)
		{
			AddScratch(damageRect.width, damageRect.height, ref minDist);
		}
		Rand.PopState();
		float damageTexturesAltitude = GetDamageTexturesAltitude(b);
		IList<Material> scratchMats = BuildingsDamageSectionLayerUtility.GetScratchMats(b);
		Rand.PushState();
		Rand.Seed = b.thingIDNumber * 7;
		for (int k = 0; k < scratches.Count; k++)
		{
			float x = scratches[k].x;
			float y = scratches[k].y;
			float rot = Rand.Range(0f, 360f);
			float num3 = num2;
			if (damageRect.width > 0.95f && damageRect.height > 0.95f)
			{
				num3 *= Rand.Range(0.85f, 1f);
			}
			Vector3 center = new Vector3(damageRect.xMin + x, damageTexturesAltitude, damageRect.yMin + y);
			Material material = scratchMats.RandomElement();
			Graphic.TryGetTextureAtlasReplacementInfo(material, TextureAtlasGroup.Building, flipUv: false, vertexColors: false, out material, out var uvs, out var _);
			Printer_Plane.PrintPlane(this, center, new Vector2(num3, num3), material, rot, flipUv: false, uvs, null, 0f);
		}
		Rand.PopState();
	}

	private void AddScratch(float rectWidth, float rectHeight, ref float minDist)
	{
		bool flag = false;
		float num = 0f;
		float num2 = 0f;
		while (!flag)
		{
			for (int i = 0; i < 5; i++)
			{
				num = Rand.Value * rectWidth;
				num2 = Rand.Value * rectHeight;
				float num3 = float.MaxValue;
				for (int j = 0; j < scratches.Count; j++)
				{
					float num4 = (num - scratches[j].x) * (num - scratches[j].x) + (num2 - scratches[j].y) * (num2 - scratches[j].y);
					if (num4 < num3)
					{
						num3 = num4;
					}
				}
				if (num3 >= minDist * minDist)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				minDist *= 0.85f;
				if (minDist < 0.001f)
				{
					break;
				}
			}
		}
		if (flag)
		{
			scratches.Add(new Vector2(num, num2));
		}
	}

	private void PrintCornersAndEdges(Building b)
	{
		Rand.PushState();
		Rand.Seed = b.thingIDNumber * 3;
		if (BuildingsDamageSectionLayerUtility.UsesLinkableCornersAndEdges(b))
		{
			DrawLinkableCornersAndEdges(b);
		}
		else
		{
			DrawFullThingCorners(b);
		}
		Rand.PopState();
	}

	private void DrawLinkableCornersAndEdges(Building b)
	{
		if (b.def.graphicData == null)
		{
			return;
		}
		DamageGraphicData damageData = b.def.graphicData.damageData;
		if (damageData == null)
		{
			return;
		}
		float damageTexturesAltitude = GetDamageTexturesAltitude(b);
		List<DamageOverlay> overlays = BuildingsDamageSectionLayerUtility.GetOverlays(b);
		IntVec3 position = b.Position;
		Vector3 vector = new Vector3((float)position.x + 0.5f, damageTexturesAltitude, (float)position.z + 0.5f);
		float x = Rand.Range(0.4f, 0.6f);
		float z = Rand.Range(0.4f, 0.6f);
		float x2 = Rand.Range(0.4f, 0.6f);
		float z2 = Rand.Range(0.4f, 0.6f);
		Vector2[] uvs = null;
		for (int i = 0; i < overlays.Count; i++)
		{
			Color32 vertexColor;
			switch (overlays[i])
			{
			case DamageOverlay.TopEdge:
			{
				Material material = damageData.edgeTopMat;
				Graphic.TryGetTextureAtlasReplacementInfo(material, TextureAtlasGroup.Building, flipUv: false, vertexColors: false, out material, out uvs, out vertexColor);
				Printer_Plane.PrintPlane(this, vector + new Vector3(x, 0f, 0f), Vector2.one, material, 0f, flipUv: false, uvs, null, 0f);
				break;
			}
			case DamageOverlay.RightEdge:
			{
				Material material = damageData.edgeRightMat;
				Graphic.TryGetTextureAtlasReplacementInfo(material, TextureAtlasGroup.Building, flipUv: false, vertexColors: false, out material, out uvs, out vertexColor);
				Printer_Plane.PrintPlane(this, vector + new Vector3(0f, 0f, z), Vector2.one, material, 90f, flipUv: false, uvs, null, 0f);
				break;
			}
			case DamageOverlay.BotEdge:
			{
				Material material = damageData.edgeBotMat;
				Graphic.TryGetTextureAtlasReplacementInfo(material, TextureAtlasGroup.Building, flipUv: false, vertexColors: false, out material, out uvs, out vertexColor);
				Printer_Plane.PrintPlane(this, vector + new Vector3(x2, 0f, 0f), Vector2.one, material, 180f, flipUv: false, uvs, null, 0f);
				break;
			}
			case DamageOverlay.LeftEdge:
			{
				Material material = damageData.edgeLeftMat;
				Graphic.TryGetTextureAtlasReplacementInfo(material, TextureAtlasGroup.Building, flipUv: false, vertexColors: false, out material, out uvs, out vertexColor);
				Printer_Plane.PrintPlane(this, vector + new Vector3(0f, 0f, z2), Vector2.one, material, 270f, flipUv: false, uvs, null, 0f);
				break;
			}
			case DamageOverlay.TopLeftCorner:
			{
				Material material = damageData.cornerTLMat;
				Graphic.TryGetTextureAtlasReplacementInfo(material, TextureAtlasGroup.Building, flipUv: false, vertexColors: false, out material, out uvs, out vertexColor);
				Printer_Plane.PrintPlane(this, vector, Vector2.one, material, 0f, flipUv: false, uvs, null, 0f);
				break;
			}
			case DamageOverlay.TopRightCorner:
			{
				Material material = damageData.cornerTRMat;
				Graphic.TryGetTextureAtlasReplacementInfo(material, TextureAtlasGroup.Building, flipUv: false, vertexColors: false, out material, out uvs, out vertexColor);
				Printer_Plane.PrintPlane(this, vector, Vector2.one, material, 90f, flipUv: false, uvs, null, 0f);
				break;
			}
			case DamageOverlay.BotRightCorner:
			{
				Material material = damageData.cornerBRMat;
				Graphic.TryGetTextureAtlasReplacementInfo(material, TextureAtlasGroup.Building, flipUv: false, vertexColors: false, out material, out uvs, out vertexColor);
				Printer_Plane.PrintPlane(this, vector, Vector2.one, material, 180f, flipUv: false, uvs, null, 0f);
				break;
			}
			case DamageOverlay.BotLeftCorner:
			{
				Material material = damageData.cornerBLMat;
				Graphic.TryGetTextureAtlasReplacementInfo(material, TextureAtlasGroup.Building, flipUv: false, vertexColors: false, out material, out uvs, out vertexColor);
				Printer_Plane.PrintPlane(this, vector, Vector2.one, material, 270f, flipUv: false, uvs, null, 0f);
				break;
			}
			}
		}
	}

	private void DrawFullThingCorners(Building b)
	{
		if (b.def.graphicData?.damageData == null)
		{
			return;
		}
		Rect damageRect = BuildingsDamageSectionLayerUtility.GetDamageRect(b);
		float damageTexturesAltitude = GetDamageTexturesAltitude(b);
		float num = Mathf.Min(Mathf.Min(damageRect.width, damageRect.height), 1.5f);
		BuildingsDamageSectionLayerUtility.GetCornerMats(out var topLeft, out var topRight, out var botRight, out var botLeft, b);
		float num2 = num * Rand.Range(0.9f, 1f);
		float num3 = num * Rand.Range(0.9f, 1f);
		float num4 = num * Rand.Range(0.9f, 1f);
		float num5 = num * Rand.Range(0.9f, 1f);
		Vector2[] uvs = null;
		List<DamageOverlay> overlays = BuildingsDamageSectionLayerUtility.GetOverlays(b);
		for (int i = 0; i < overlays.Count; i++)
		{
			Color32 vertexColor;
			switch (overlays[i])
			{
			case DamageOverlay.TopLeftCorner:
			{
				Rect rect4 = new Rect(damageRect.xMin, damageRect.yMax - num2, num2, num2);
				Material material = topLeft;
				Graphic.TryGetTextureAtlasReplacementInfo(material, TextureAtlasGroup.Building, flipUv: false, vertexColors: false, out material, out uvs, out vertexColor);
				Printer_Plane.PrintPlane(this, new Vector3(rect4.center.x, damageTexturesAltitude, rect4.center.y), rect4.size, material, 0f, flipUv: false, uvs, null, 0f);
				break;
			}
			case DamageOverlay.TopRightCorner:
			{
				Rect rect3 = new Rect(damageRect.xMax - num3, damageRect.yMax - num3, num3, num3);
				Material material = topRight;
				Graphic.TryGetTextureAtlasReplacementInfo(material, TextureAtlasGroup.Building, flipUv: false, vertexColors: false, out material, out uvs, out vertexColor);
				Printer_Plane.PrintPlane(this, new Vector3(rect3.center.x, damageTexturesAltitude, rect3.center.y), rect3.size, material, 90f, flipUv: false, uvs, null, 0f);
				break;
			}
			case DamageOverlay.BotRightCorner:
			{
				Rect rect2 = new Rect(damageRect.xMax - num4, damageRect.yMin, num4, num4);
				Material material = botRight;
				Graphic.TryGetTextureAtlasReplacementInfo(material, TextureAtlasGroup.Building, flipUv: false, vertexColors: false, out material, out uvs, out vertexColor);
				Printer_Plane.PrintPlane(this, new Vector3(rect2.center.x, damageTexturesAltitude, rect2.center.y), rect2.size, material, 180f, flipUv: false, uvs, null, 0f);
				break;
			}
			case DamageOverlay.BotLeftCorner:
			{
				Rect rect = new Rect(damageRect.xMin, damageRect.yMin, num5, num5);
				Material material = botLeft;
				Graphic.TryGetTextureAtlasReplacementInfo(material, TextureAtlasGroup.Building, flipUv: false, vertexColors: false, out material, out uvs, out vertexColor);
				Printer_Plane.PrintPlane(this, new Vector3(rect.center.x, damageTexturesAltitude, rect.center.y), rect.size, material, 270f, flipUv: false, uvs, null, 0f);
				break;
			}
			}
		}
	}

	private float GetDamageTexturesAltitude(Building b)
	{
		return b.def.Altitude + 15f / 82f;
	}
}
