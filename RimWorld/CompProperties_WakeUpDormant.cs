using Verse;

namespace RimWorld
{
	public class CompProperties_WakeUpDormant : CompProperties
	{
		public string wakeUpSignalTag = "CompCanBeDormant.WakeUp";

		public float anyColonistCloseCheckRadius;

		public float wakeUpOnThingConstructedRadius = 3f;

		public bool wakeUpOnDamage = true;

		public bool onlyWakeUpSameFaction = true;

		public SoundDef wakeUpSound;

		public bool wakeUpIfAnyColonistClose;

		public CompProperties_WakeUpDormant()
		{
			compClass = typeof(CompWakeUpDormant);
		}
	}
}
