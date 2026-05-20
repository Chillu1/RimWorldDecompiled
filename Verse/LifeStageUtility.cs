using System;
using System.Collections.Generic;
using RimWorld;
using Verse.Sound;

namespace Verse;

public static class LifeStageUtility
{
	public static void PlayNearestLifestageSound(Pawn pawn, Func<LifeStageAge, SoundDef> lifestageGetter, Func<GeneDef, SoundDef> geneGetter, Func<MutantDef, SoundDef> mutantGetter, float volumeFactor = 1f)
	{
		GetNearestLifestageSound(pawn, lifestageGetter, out var def, out var pitch, out var volume);
		if (ModsConfig.BiotechActive && pawn.genes != null && geneGetter != null)
		{
			def = pawn.genes.GetSoundOverrideFromGenes(geneGetter, def);
			pitch = pawn.ageTracker.CurLifeStage.voxPitch;
			volume = pawn.ageTracker.CurLifeStage.voxVolume;
		}
		if (pawn.IsMutant && mutantGetter != null)
		{
			def = mutantGetter(pawn.mutant.Def);
			pitch = pawn.ageTracker.CurLifeStage.voxPitch;
			volume = pawn.ageTracker.CurLifeStage.voxVolume;
		}
		if (def != null && pawn.SpawnedOrAnyParentSpawned)
		{
			SoundInfo info = SoundInfo.InMap(new TargetInfo(pawn.PositionHeld, pawn.MapHeld));
			info.pitchFactor = pitch;
			info.volumeFactor = volume * volumeFactor;
			def.PlayOneShot(info);
		}
	}

	private static void GetNearestLifestageSound(Pawn pawn, Func<LifeStageAge, SoundDef> getter, out SoundDef def, out float pitch, out float volume)
	{
		int num = pawn.ageTracker.CurLifeStageIndex;
		do
		{
			LifeStageAge lifeStageAge = pawn.RaceProps.lifeStageAges[num];
			def = getter(lifeStageAge);
			if (def != null)
			{
				pitch = pawn.ageTracker.CurLifeStage.voxPitch / lifeStageAge.def.voxPitch;
				volume = pawn.ageTracker.CurLifeStage.voxVolume / lifeStageAge.def.voxVolume;
				return;
			}
			num++;
		}
		while (num >= 0 && num < pawn.RaceProps.lifeStageAges.Count);
		def = null;
		pitch = (volume = 1f);
	}

	private static LifeStageAge GetLifeStageAgeForYears(Pawn pawn, float years)
	{
		List<LifeStageAge> lifeStageAges = pawn.RaceProps.lifeStageAges;
		int num = 0;
		for (int i = 1; i < lifeStageAges.Count && lifeStageAges[i].minAge < years; i++)
		{
			num++;
		}
		return lifeStageAges[num];
	}

	public static DevelopmentalStage CalculateDevelopmentalStage(Pawn pawn, float years)
	{
		return GetLifeStageAgeForYears(pawn, years).def.developmentalStage;
	}

	public static float GetMaxBabyAge(RaceProperties raceProps)
	{
		foreach (LifeStageAge lifeStageAge in raceProps.lifeStageAges)
		{
			if (!lifeStageAge.def.developmentalStage.Baby())
			{
				return lifeStageAge.minAge;
			}
		}
		return 0f;
	}

	public static bool AlwaysDowned(Pawn pawn)
	{
		return pawn.ageTracker?.CurLifeStage?.alwaysDowned == true;
	}
}
