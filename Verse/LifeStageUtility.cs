using System;
using Verse.Sound;

namespace Verse
{
	public static class LifeStageUtility
	{
		public static void PlayNearestLifestageSound(Pawn pawn, Func<LifeStageAge, SoundDef> getter, float volumeFactor = 1f)
		{
			GetNearestLifestageSound(pawn, getter, out var def, out var pitch, out var volume);
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
	}
}
