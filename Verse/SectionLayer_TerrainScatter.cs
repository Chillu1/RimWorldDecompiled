using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public class SectionLayer_TerrainScatter : SectionLayer
{
	private class Scatterable
	{
		private Map map;

		private ScatterableDef def;

		private Vector3 loc;

		private float size;

		private float rotation;

		public bool IsOnValidTerrain
		{
			get
			{
				IntVec3 intVec = loc.ToIntVec3();
				if (def.scatterType != map.terrainGrid.TerrainAt(intVec).scatterType || intVec.Filled(map))
				{
					return false;
				}
				if (!def.placeUnderNaturalRoofs)
				{
					RoofDef roof = intVec.GetRoof(map);
					if (roof != null && roof.isNatural)
					{
						return false;
					}
				}
				foreach (IntVec3 item in CellRect.CenteredOn(intVec, Mathf.FloorToInt(size / 2f)).ClipInsideMap(map))
				{
					TerrainDef terrainDef = map.terrainGrid.TerrainAt(item);
					if (terrainDef.IsFloor || terrainDef.IsSubstructure || terrainDef == TerrainDefOf.Space)
					{
						return false;
					}
				}
				return true;
			}
		}

		public Scatterable(ScatterableDef def, Vector3 loc, Map map)
		{
			this.def = def;
			this.loc = loc;
			this.map = map;
			size = Rand.Range(def.minSize, def.maxSize);
			rotation = Rand.Range(0f, 360f);
		}

		public void PrintOnto(SectionLayer layer)
		{
			Material material = def.mat;
			Graphic.TryGetTextureAtlasReplacementInfo(material, TextureAtlasGroup.Terrain, flipUv: false, vertexColors: false, out material, out var uvs, out var _);
			Printer_Plane.PrintPlane(layer, loc, Vector2.one * size, material, rotation, flipUv: false, uvs);
		}
	}

	private const float MinimumSpacing = 5f;

	private List<Scatterable> scats;

	private Dictionary<string, List<ScatterableDef>> scatterDefsByType = new Dictionary<string, List<ScatterableDef>>();

	private static readonly Dictionary<int, List<IntVec3>> cachedScatterablePoints = new Dictionary<int, List<IntVec3>>();

	public override bool Visible => DebugViewSettings.drawTerrain;

	public SectionLayer_TerrainScatter(Section section)
		: base(section)
	{
		relevantChangeTypes = MapMeshFlagDefOf.Terrain;
	}

	public override void Regenerate()
	{
		ClearSubMeshes(MeshParts.All);
		if (scats == null)
		{
			scats = new List<Scatterable>();
			GenerateScats();
		}
		scats.RemoveAll((Scatterable scat) => !scat.IsOnValidTerrain);
		for (int num = 0; num < scats.Count; num++)
		{
			scats[num].PrintOnto(this);
		}
		FinalizeMesh(MeshParts.All);
	}

	private void GenerateScats()
	{
		foreach (IntVec3 scatPoint in GetScatPoints(base.Map))
		{
			if (!section.CellRect.Contains(scatPoint))
			{
				continue;
			}
			TerrainDef terrainDef = base.Map.terrainGrid.TerrainAt(scatPoint);
			if (terrainDef.scatterType == null)
			{
				continue;
			}
			if (!scatterDefsByType.ContainsKey(terrainDef.scatterType))
			{
				scatterDefsByType[terrainDef.scatterType] = new List<ScatterableDef>();
				foreach (ScatterableDef allDef in DefDatabase<ScatterableDef>.AllDefs)
				{
					if (allDef.scatterType == terrainDef.scatterType)
					{
						scatterDefsByType[terrainDef.scatterType].Add(allDef);
					}
				}
			}
			if (scatterDefsByType[terrainDef.scatterType].Empty())
			{
				continue;
			}
			ScatterableDef scatterableDef = scatterDefsByType[terrainDef.scatterType].RandomElement();
			if (Rand.Chance(scatterableDef.scatterChance))
			{
				Vector3 loc = new Vector3((float)scatPoint.x + (Rand.Value - 0.5f), 0f, (float)scatPoint.z + (Rand.Value - 0.5f));
				Scatterable scatterable = new Scatterable(scatterableDef, loc, base.Map);
				if (scatterable.IsOnValidTerrain)
				{
					scats.Add(scatterable);
				}
			}
		}
	}

	private static List<IntVec3> GetScatPoints(Map map)
	{
		int key = map.ConstantRandSeed * Find.World.info.Seed;
		if (cachedScatterablePoints.TryGetValue(key, out var value))
		{
			return value;
		}
		List<IntVec3> list = new List<IntVec3>();
		foreach (IntVec3 allCell in map.AllCells)
		{
			list.Add(allCell);
		}
		using (new ProfilerBlock("GenerateBlueNoisePoints"))
		{
			cachedScatterablePoints[key] = GenerateBlueNoisePoints(list, 5f);
		}
		return cachedScatterablePoints[key];
	}

	private static List<IntVec3> GenerateBlueNoisePoints(List<IntVec3> candidatePoints, float minimumSpacing)
	{
		IntVec3[] array = new IntVec3[candidatePoints.Count];
		List<IntVec3> list = new List<IntVec3>();
		int num = 0;
		foreach (IntVec3 item in candidatePoints.InRandomOrder())
		{
			array[num] = item;
			num++;
		}
		for (int num2 = array.Length - 1; num2 >= 0; num2--)
		{
			bool flag = true;
			for (int i = 0; i < list.Count; i++)
			{
				IntVec3 otherLoc = list[i];
				if (array[num2].InHorDistOf(otherLoc, minimumSpacing))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				list.Add(array[num2]);
			}
		}
		return list;
	}
}
