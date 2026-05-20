using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public class WorldLandmarks : IExposable
{
	public Dictionary<PlanetTile, Landmark> landmarks = new Dictionary<PlanetTile, Landmark>();

	public Landmark this[PlanetTile tile]
	{
		get
		{
			if (landmarks.ContainsKey(tile))
			{
				return landmarks[tile];
			}
			return null;
		}
		set
		{
			landmarks[tile] = value;
		}
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref landmarks, "landmarks", LookMode.Value, LookMode.Deep);
	}

	public void AddLandmark(LandmarkDef landmarkDef, PlanetTile tile, PlanetLayer layer = null, bool forced = false)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return;
		}
		if (layer == null)
		{
			layer = tile.Layer;
		}
		Landmark landmark = (Landmark)Activator.CreateInstance(landmarkDef.workerClass, landmarkDef);
		if (landmarkDef.nameMaker != null)
		{
			landmark.name = NameGenerator.GenerateName(landmarkDef.nameMaker, null, appendNumberIfNameUsed: false, "r_name", null, null);
		}
		else
		{
			landmark.name = landmarkDef.LabelCap;
		}
		this[tile] = landmark;
		foreach (MutatorChance mutatorChance in landmarkDef.mutatorChances)
		{
			if (Rand.Chance(mutatorChance.chance) && ((mutatorChance.required && forced) || mutatorChance.mutator.IsValidTile(tile, layer)))
			{
				layer[tile].AddMutator(mutatorChance.mutator);
			}
		}
		foreach (MutatorChance comboLandmarkMutator in landmarkDef.comboLandmarkMutators)
		{
			if (Rand.Chance(comboLandmarkMutator.chance) && ((comboLandmarkMutator.required && forced) || comboLandmarkMutator.mutator.IsValidTile(tile, layer)))
			{
				layer[tile].AddMutator(comboLandmarkMutator.mutator);
				landmark.isComboLandmark = true;
				break;
			}
		}
	}

	public void RemoveLandmark(PlanetTile tile)
	{
		if (landmarks.ContainsKey(tile))
		{
			landmarks.Remove(tile);
		}
	}
}
