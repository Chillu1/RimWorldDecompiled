using System.Collections;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldLayer_Hills : WorldLayer
	{
		private static readonly FloatRange BaseSizeRange = new FloatRange(0.9f, 1.1f);

		private static readonly IntVec2 TexturesInAtlas = new IntVec2(2, 2);

		private static readonly FloatRange BasePosOffsetRange_SmallHills = new FloatRange(0f, 0.37f);

		private static readonly FloatRange BasePosOffsetRange_LargeHills = new FloatRange(0f, 0.2f);

		private static readonly FloatRange BasePosOffsetRange_Mountains = new FloatRange(0f, 0.08f);

		private static readonly FloatRange BasePosOffsetRange_ImpassableMountains = new FloatRange(0f, 0.08f);

		public override IEnumerable Regenerate()
		{
			foreach (object item in base.Regenerate())
			{
				yield return item;
			}
			Rand.PushState();
			Rand.Seed = Find.World.info.Seed;
			WorldGrid worldGrid = Find.WorldGrid;
			int tilesCount = worldGrid.TilesCount;
			for (int i = 0; i < tilesCount; i++)
			{
				Material material;
				FloatRange floatRange;
				switch (worldGrid[i].hilliness)
				{
				case Hilliness.SmallHills:
					material = WorldMaterials.SmallHills;
					floatRange = BasePosOffsetRange_SmallHills;
					break;
				case Hilliness.LargeHills:
					material = WorldMaterials.LargeHills;
					floatRange = BasePosOffsetRange_LargeHills;
					break;
				case Hilliness.Mountainous:
					material = WorldMaterials.Mountains;
					floatRange = BasePosOffsetRange_Mountains;
					break;
				case Hilliness.Impassable:
					material = WorldMaterials.ImpassableMountains;
					floatRange = BasePosOffsetRange_ImpassableMountains;
					break;
				default:
					continue;
				}
				LayerSubMesh subMesh = GetSubMesh(material);
				Vector3 tileCenter = worldGrid.GetTileCenter(i);
				Vector3 posForTangents = tileCenter;
				float magnitude = tileCenter.magnitude;
				tileCenter = (tileCenter + Rand.UnitVector3 * floatRange.RandomInRange * worldGrid.averageTileSize).normalized * magnitude;
				WorldRendererUtility.PrintQuadTangentialToPlanet(tileCenter, posForTangents, BaseSizeRange.RandomInRange * worldGrid.averageTileSize, 0.005f, subMesh, counterClockwise: false, randomizeRotation: true, printUVs: false);
				WorldRendererUtility.PrintTextureAtlasUVs(Rand.Range(0, TexturesInAtlas.x), Rand.Range(0, TexturesInAtlas.z), TexturesInAtlas.x, TexturesInAtlas.z, subMesh);
			}
			Rand.PopState();
			FinalizeMesh(MeshParts.All);
		}
	}
}
