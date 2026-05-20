using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace RimWorld;

public static class AmbientSoundManager
{
	private static readonly List<Sustainer> biomeAmbientSustainers = new List<Sustainer>();

	private static Sustainer altitudeWindSustainer;

	private static bool WorldAmbientSoundCreated => Find.SoundRoot.sustainerManager.SustainerExists(SoundDefOf.Ambient_Space);

	private static bool AltitudeWindSoundCreated => Find.SoundRoot.sustainerManager.SustainerExists(SoundDefOf.Ambient_AltitudeWind);

	public static void EnsureWorldAmbientSoundCreated()
	{
		if (!WorldAmbientSoundCreated)
		{
			SoundDefOf.Ambient_Space.TrySpawnSustainer(SoundInfo.OnCamera());
		}
	}

	public static void Notify_SwitchedMap()
	{
		LongEventHandler.ExecuteWhenFinished(RecreateMapSustainers);
	}

	private static void RecreateMapSustainers()
	{
		if (Find.CurrentMap == null)
		{
			return;
		}
		BiomeDef biome = Find.CurrentMap.Biome;
		if (!AltitudeWindSoundCreated && !biome.noAmbientWind)
		{
			altitudeWindSustainer = SoundDefOf.Ambient_AltitudeWind.TrySpawnSustainer(SoundInfo.OnCamera());
		}
		else if (altitudeWindSustainer != null && biome.noAmbientWind)
		{
			altitudeWindSustainer.End();
			altitudeWindSustainer = null;
		}
		SustainerManager sustainerManager = Find.SoundRoot.sustainerManager;
		for (int i = 0; i < biomeAmbientSustainers.Count; i++)
		{
			Sustainer sustainer = biomeAmbientSustainers[i];
			if (sustainerManager.AllSustainers.Contains(sustainer) && !sustainer.Ended)
			{
				sustainer.End();
			}
		}
		biomeAmbientSustainers.Clear();
		if (Find.CurrentMap != null)
		{
			List<SoundDef> soundsAmbient = Find.CurrentMap.Biome.soundsAmbient;
			for (int j = 0; j < soundsAmbient.Count; j++)
			{
				Sustainer item = soundsAmbient[j].TrySpawnSustainer(SoundInfo.OnCamera());
				biomeAmbientSustainers.Add(item);
			}
		}
	}
}
