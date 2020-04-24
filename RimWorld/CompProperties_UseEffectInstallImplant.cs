using Verse;

namespace RimWorld
{
	public class CompProperties_UseEffectInstallImplant : CompProperties_Usable
	{
		public HediffDef hediffDef;

		public BodyPartDef bodyPart;

		public bool canUpgrade;

		public bool allowNonColonists;

		public CompProperties_UseEffectInstallImplant()
		{
			compClass = typeof(CompUseEffect_InstallImplant);
		}
	}
}
