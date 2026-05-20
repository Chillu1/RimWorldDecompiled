using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class WorldDrawLayer_Landmarks : WorldDrawLayer
{
	private static readonly List<PlanetTile> neighbours = new List<PlanetTile>(8);

	public override bool VisibleWhenLayerNotSelected => false;

	public override bool VisibleInBackground => false;

	public override bool Visible => ModsConfig.OdysseyActive;

	public override IEnumerable Regenerate()
	{
		foreach (object item in base.Regenerate())
		{
			yield return item;
		}
		if (Find.World.landmarks != null)
		{
			foreach (var (tile, landmark2) in Find.World.landmarks.landmarks)
			{
				if (planetLayer == tile.Layer && landmark2.def.drawType == LandmarkDef.LandmarkDrawType.Standard)
				{
					DrawStandard(landmark2, tile);
				}
			}
		}
		FinalizeMesh(MeshParts.All);
	}

	private void DrawStandard(Landmark landmark, PlanetTile tile)
	{
		LayerSubMesh subMesh = GetSubMesh(landmark.def.Material);
		Vector3 tileCenter = planetLayer.GetTileCenter(tile);
		float rotation = Rand.Range(0f, 360f);
		if (landmark.def.coastRotateMode != LandmarkDef.CoastRotateMode.None)
		{
			planetLayer.GetTileNeighbors(tile, neighbours);
			PlanetTile toTile = PlanetTile.Invalid;
			for (int i = 0; i < neighbours.Count; i++)
			{
				if (LandmarkDef.IsValidRotatableNeighbour(planetLayer[neighbours[i]].PrimaryBiome, landmark.def.coastRotateMode))
				{
					toTile = neighbours[i];
					break;
				}
			}
			if (toTile.Valid)
			{
				rotation = planetLayer.GetHeadingFromTo(tile, toTile) - 90f;
			}
		}
		Rand.PushState(tile.tileId);
		WorldRendererUtility.PrintQuadTangentialToPlanet(tileCenter, planetLayer.GetTileSize(tile) * landmark.def.drawScale, landmark.def.drawAboveRoads ? 0.025f : 0.0075f, subMesh, counterClockwise: false, rotation, printUVs: false);
		WorldRendererUtility.PrintTextureAtlasUVs(Rand.Range(0, landmark.def.atlasSize.x), Rand.Range(0, landmark.def.atlasSize.z), landmark.def.atlasSize.x, landmark.def.atlasSize.z, subMesh);
		Rand.PopState();
	}
}
