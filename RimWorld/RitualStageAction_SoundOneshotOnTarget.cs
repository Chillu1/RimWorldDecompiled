using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class RitualStageAction_SoundOneshotOnTarget : RitualStageAction
	{
		public SoundDef sound;

		public override void Apply(LordJob_Ritual ritual)
		{
			sound.PlayOneShot(SoundInfo.InMap(ritual.selectedTarget));
		}

		public override void ExposeData()
		{
			Scribe_Defs.Look(ref sound, "sound");
		}
	}
}
