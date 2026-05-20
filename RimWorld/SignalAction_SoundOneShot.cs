using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class SignalAction_SoundOneShot : SignalAction
	{
		public SoundDef sound;

		protected override void DoAction(SignalArgs args)
		{
			sound.PlayOneShot(this);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref sound, "sound");
		}
	}
}
