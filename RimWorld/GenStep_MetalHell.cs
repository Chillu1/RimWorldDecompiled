using System.Collections.Generic;
using Verse;
using Verse.Noise;

namespace RimWorld;

public class GenStep_MetalHell : GenStep
{
	private const float WallNoiseCutoff = 1.75f;

	private const float MassNoiseCutoff = 1.9f;

	private static readonly FloatRange InnerMassCutoffRange = new FloatRange(2f, 3f);

	private const float InnerMassChancePerCell = 0.05f;

	public float perlinFrequency = 0.05f;

	public float perlinLacunarity = 2f;

	public float perlinPersistence = 0.5f;

	public int perlinOctaves = 1;

	[Unsaved(false)]
	private ModuleBase noise;

	public override int SeedPart => 41234756;

	public override void Generate(Map map, GenStepParams parms)
	{
		if (!ModLister.CheckAnomaly("Metalhell"))
		{
			return;
		}
		List<ThingDef> source = new List<ThingDef>
		{
			ThingDefOf.VoidmetalMassSmall,
			ThingDefOf.VoidmetalMassMedium,
			ThingDefOf.VoidmetalMassLarge
		};
		noise = new Perlin(perlinFrequency, perlinLacunarity, perlinPersistence, perlinOctaves, Rand.Range(0, int.MaxValue), QualityMode.Medium);
		NoiseDebugUI.StoreNoiseRender(noise, "metal hell");
		foreach (IntVec3 allCell in map.AllCells)
		{
			float num = (float)(allCell.x - map.Center.x) / ((float)map.Size.x / 2f);
			float num2 = (float)(allCell.z - map.Center.z) / ((float)map.Size.z / 2f);
			float num3 = num * num + num2 * num2;
			float num4 = 1f / num3 + noise.GetValue(allCell) / 2f;
			map.terrainGrid.SetTerrain(allCell, TerrainDefOf.Gravel);
			map.terrainGrid.SetTerrain(allCell, TerrainDefOf.Voidmetal);
			if (num4 < 1.75f)
			{
				GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.VoidmetalWall), allCell, map);
			}
			else if (num4 < 1.9f)
			{
				ThingDef thingDef = source.RandomElement();
				bool flag = true;
				foreach (IntVec3 item in GenAdj.CellsOccupiedBy(allCell, Rot4.North, thingDef.size))
				{
					if (!item.InBounds(map) || !map.thingGrid.ThingsListAt(item).NullOrEmpty())
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					GenSpawn.Spawn(ThingMaker.MakeThing(thingDef), allCell, map);
				}
			}
			if (InnerMassCutoffRange.Includes(num4) && Rand.Chance(0.05f))
			{
				GenSpawn.Spawn(ThingMaker.MakeThing(source.RandomElement()), allCell, map);
			}
		}
		GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.VoidNode), map.Center, map);
		GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.MetalHellAmbience), map.Center, map);
		IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
		foreach (IntVec3 intVec in cardinalDirections)
		{
			Rot4 opposite = Rot4.FromIntVec3(intVec).Opposite;
			GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.TwistedArchotechSupport_Large), map.Center + intVec * 10, map, opposite);
			GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.TwistedArchotechSupport_Small), map.Center + intVec * 16, map, opposite);
			GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.TwistedArchotechSupport_Vertical), map.Center + intVec * 7 + intVec.RotatedBy(RotationDirection.Clockwise) * 7, map);
		}
		GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.MetalHellFloorCracks), map.Center, map);
		GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.MetalHellFloorMarkings), map.Center, map);
		MapGenerator.PlayerStartSpot = map.Center;
	}
}
