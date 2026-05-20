using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse;

[StaticConstructorOnStartup]
public static class BuildingsDamageSectionLayerUtility
{
	private static readonly Material[] DefaultScratchMats = new Material[3]
	{
		MaterialPool.MatFrom("Damage/Scratch1", ShaderDatabase.Transparent),
		MaterialPool.MatFrom("Damage/Scratch2", ShaderDatabase.Transparent),
		MaterialPool.MatFrom("Damage/Scratch3", ShaderDatabase.Transparent)
	};

	private static List<DamageOverlay> availableOverlays = new List<DamageOverlay>();

	private static List<DamageOverlay> overlaysWorkingList = new List<DamageOverlay>();

	private static List<DamageOverlay> overlays = new List<DamageOverlay>();

	public static void TryInsertIntoAtlas()
	{
		for (int i = 0; i < DefaultScratchMats.Length; i++)
		{
			GlobalTextureAtlasManager.TryInsertStatic(TextureAtlasGroup.Building, (Texture2D)DefaultScratchMats[i].mainTexture);
		}
	}

	public static void Notify_BuildingHitPointsChanged(Building b, int oldHitPoints)
	{
		if (b.Spawned && b.def.useHitPoints && b.HitPoints != oldHitPoints && b.def.drawDamagedOverlay && GetDamageOverlaysCount(b, b.HitPoints) != GetDamageOverlaysCount(b, oldHitPoints))
		{
			b.Map.mapDrawer.MapMeshDirty(b.Position, MapMeshFlagDefOf.BuildingsDamage);
		}
	}

	public static bool UsesLinkableCornersAndEdges(Building b)
	{
		if (b.def.size.x == 1 && b.def.size.z == 1)
		{
			return b.def.Fillage == FillCategory.Full;
		}
		return false;
	}

	public static IList<Material> GetScratchMats(Building b)
	{
		IList<Material> result = DefaultScratchMats;
		if (b.def.graphicData != null && b.def.graphicData.damageData != null && b.def.graphicData.damageData.scratchMats != null)
		{
			result = b.def.graphicData.damageData.scratchMats;
		}
		return result;
	}

	public static List<DamageOverlay> GetAvailableOverlays(Building b)
	{
		availableOverlays.Clear();
		if (GetScratchMats(b).Any())
		{
			int num = 3;
			Rect damageRect = GetDamageRect(b);
			float num2 = damageRect.width * damageRect.height;
			if (num2 > 4f)
			{
				num += Mathf.RoundToInt((num2 - 4f) * 0.54f);
			}
			for (int i = 0; i < num; i++)
			{
				availableOverlays.Add(DamageOverlay.Scratch);
			}
		}
		if (UsesLinkableCornersAndEdges(b))
		{
			if (b.def.graphicData != null && b.def.graphicData.damageData != null)
			{
				IntVec3 position = b.Position;
				DamageGraphicData damageData = b.def.graphicData.damageData;
				if (damageData.edgeTopMat != null && DifferentAt(b, position.x, position.z + 1) && SameAndDamagedAt(b, position.x + 1, position.z) && DifferentAt(b, position.x + 1, position.z + 1))
				{
					availableOverlays.Add(DamageOverlay.TopEdge);
				}
				if (damageData.edgeRightMat != null && DifferentAt(b, position.x + 1, position.z) && SameAndDamagedAt(b, position.x, position.z + 1) && DifferentAt(b, position.x + 1, position.z + 1))
				{
					availableOverlays.Add(DamageOverlay.RightEdge);
				}
				if (damageData.edgeBotMat != null && DifferentAt(b, position.x, position.z - 1) && SameAndDamagedAt(b, position.x + 1, position.z) && DifferentAt(b, position.x + 1, position.z - 1))
				{
					availableOverlays.Add(DamageOverlay.BotEdge);
				}
				if (damageData.edgeLeftMat != null && DifferentAt(b, position.x - 1, position.z) && SameAndDamagedAt(b, position.x, position.z + 1) && DifferentAt(b, position.x - 1, position.z + 1))
				{
					availableOverlays.Add(DamageOverlay.LeftEdge);
				}
				if (damageData.cornerTLMat != null && DifferentAt(b, position.x - 1, position.z) && DifferentAt(b, position.x, position.z + 1))
				{
					availableOverlays.Add(DamageOverlay.TopLeftCorner);
				}
				if (damageData.cornerTRMat != null && DifferentAt(b, position.x + 1, position.z) && DifferentAt(b, position.x, position.z + 1))
				{
					availableOverlays.Add(DamageOverlay.TopRightCorner);
				}
				if (damageData.cornerBRMat != null && DifferentAt(b, position.x + 1, position.z) && DifferentAt(b, position.x, position.z - 1))
				{
					availableOverlays.Add(DamageOverlay.BotRightCorner);
				}
				if (damageData.cornerBLMat != null && DifferentAt(b, position.x - 1, position.z) && DifferentAt(b, position.x, position.z - 1))
				{
					availableOverlays.Add(DamageOverlay.BotLeftCorner);
				}
			}
		}
		else
		{
			GetCornerMats(out var topLeft, out var topRight, out var botRight, out var botLeft, b);
			if (topLeft != null)
			{
				availableOverlays.Add(DamageOverlay.TopLeftCorner);
			}
			if (topRight != null)
			{
				availableOverlays.Add(DamageOverlay.TopRightCorner);
			}
			if (botLeft != null)
			{
				availableOverlays.Add(DamageOverlay.BotLeftCorner);
			}
			if (botRight != null)
			{
				availableOverlays.Add(DamageOverlay.BotRightCorner);
			}
		}
		return availableOverlays;
	}

	public static void GetCornerMats(out Material topLeft, out Material topRight, out Material botRight, out Material botLeft, Building b)
	{
		if (b.def.graphicData == null || b.def.graphicData.damageData == null)
		{
			topLeft = null;
			topRight = null;
			botRight = null;
			botLeft = null;
			return;
		}
		DamageGraphicData damageData = b.def.graphicData.damageData;
		if (b.Rotation == Rot4.North)
		{
			topLeft = damageData.cornerTLMat;
			topRight = damageData.cornerTRMat;
			botRight = damageData.cornerBRMat;
			botLeft = damageData.cornerBLMat;
		}
		else if (b.Rotation == Rot4.East)
		{
			topLeft = damageData.cornerBLMat;
			topRight = damageData.cornerTLMat;
			botRight = damageData.cornerTRMat;
			botLeft = damageData.cornerBRMat;
		}
		else if (b.Rotation == Rot4.South)
		{
			topLeft = damageData.cornerBRMat;
			topRight = damageData.cornerBLMat;
			botRight = damageData.cornerTLMat;
			botLeft = damageData.cornerTRMat;
		}
		else
		{
			topLeft = damageData.cornerTRMat;
			topRight = damageData.cornerBRMat;
			botRight = damageData.cornerBLMat;
			botLeft = damageData.cornerTLMat;
		}
	}

	public static List<DamageOverlay> GetOverlays(Building b)
	{
		overlays.Clear();
		overlaysWorkingList.Clear();
		overlaysWorkingList.AddRange(GetAvailableOverlays(b));
		if (!overlaysWorkingList.Any())
		{
			return overlays;
		}
		Rand.PushState();
		Rand.Seed = Gen.HashCombineInt(b.thingIDNumber, 1958376471);
		int damageOverlaysCount = GetDamageOverlaysCount(b, b.HitPoints);
		for (int i = 0; i < damageOverlaysCount; i++)
		{
			if (!overlaysWorkingList.Any())
			{
				break;
			}
			DamageOverlay item = overlaysWorkingList.RandomElement();
			overlaysWorkingList.Remove(item);
			overlays.Add(item);
		}
		Rand.PopState();
		return overlays;
	}

	public static Rect GetDamageRect(Building b)
	{
		DamageGraphicData damageGraphicData = null;
		if (b.def.graphicData != null)
		{
			damageGraphicData = b.def.graphicData.damageData;
		}
		CellRect cellRect = b.OccupiedRect();
		Rect result = new Rect(cellRect.minX, cellRect.minZ, cellRect.Width, cellRect.Height);
		if (damageGraphicData != null)
		{
			if (b.Rotation == Rot4.North && damageGraphicData.rectN != default(Rect))
			{
				result.position += damageGraphicData.rectN.position;
				result.size = damageGraphicData.rectN.size;
			}
			else if (b.Rotation == Rot4.East && damageGraphicData.rectE != default(Rect))
			{
				result.position += damageGraphicData.rectE.position;
				result.size = damageGraphicData.rectE.size;
			}
			else if (b.Rotation == Rot4.South && damageGraphicData.rectS != default(Rect))
			{
				result.position += damageGraphicData.rectS.position;
				result.size = damageGraphicData.rectS.size;
			}
			else if (b.Rotation == Rot4.West && damageGraphicData.rectW != default(Rect))
			{
				result.position += damageGraphicData.rectW.position;
				result.size = damageGraphicData.rectW.size;
			}
			else if (damageGraphicData.rect != default(Rect))
			{
				Rect rect = damageGraphicData.rect;
				if (b.Rotation == Rot4.North)
				{
					result.x += rect.x;
					result.y += rect.y;
					result.width = rect.width;
					result.height = rect.height;
				}
				else if (b.Rotation == Rot4.South)
				{
					result.x += (float)cellRect.Width - rect.x - rect.width;
					result.y += (float)cellRect.Height - rect.y - rect.height;
					result.width = rect.width;
					result.height = rect.height;
				}
				else if (b.Rotation == Rot4.West)
				{
					result.x += (float)cellRect.Width - rect.y - rect.height;
					result.y += rect.x;
					result.width = rect.height;
					result.height = rect.width;
				}
				else if (b.Rotation == Rot4.East)
				{
					result.x += rect.y;
					result.y += (float)cellRect.Height - rect.x - rect.width;
					result.width = rect.height;
					result.height = rect.width;
				}
			}
		}
		return result;
	}

	private static int GetDamageOverlaysCount(Building b, int hp)
	{
		float num = (float)hp / (float)b.MaxHitPoints;
		int count = GetAvailableOverlays(b).Count;
		return count - Mathf.FloorToInt((float)count * num);
	}

	private static bool DifferentAt(Building b, int x, int z)
	{
		IntVec3 c = new IntVec3(x, 0, z);
		if (!c.InBounds(b.Map))
		{
			return true;
		}
		List<Thing> thingList = c.GetThingList(b.Map);
		for (int i = 0; i < thingList.Count; i++)
		{
			if (thingList[i].def == b.def)
			{
				return false;
			}
		}
		return true;
	}

	private static bool SameAndDamagedAt(Building b, int x, int z)
	{
		IntVec3 c = new IntVec3(x, 0, z);
		if (!c.InBounds(b.Map))
		{
			return false;
		}
		List<Thing> thingList = c.GetThingList(b.Map);
		for (int i = 0; i < thingList.Count; i++)
		{
			if (thingList[i].def == b.def && thingList[i].HitPoints < thingList[i].MaxHitPoints)
			{
				return true;
			}
		}
		return false;
	}

	public static void DebugDraw()
	{
		if (Prefs.DevMode && DebugViewSettings.drawDamageRects && Find.CurrentMap != null && Find.Selector.FirstSelectedObject is Building b)
		{
			Material material = DebugSolidColorMats.MaterialOf(Color.red);
			Rect damageRect = GetDamageRect(b);
			float y = 14.99f;
			Vector3 pos = new Vector3(damageRect.x + damageRect.width / 2f, y, damageRect.y + damageRect.height / 2f);
			Vector3 s = new Vector3(damageRect.width, 1f, damageRect.height);
			Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(pos, Quaternion.identity, s), material, 0);
		}
	}
}
