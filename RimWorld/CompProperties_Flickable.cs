using Verse;

namespace RimWorld
{
	public class CompProperties_Flickable : CompProperties
	{
		[NoTranslate]
		public string commandTexture = "UI/Commands/DesirePower";

		[NoTranslate]
		public string commandLabelKey = "CommandDesignateTogglePowerLabel";

		[NoTranslate]
		public string commandDescKey = "CommandDesignateTogglePowerDesc";

		public CompProperties_Flickable()
		{
			compClass = typeof(CompFlickable);
		}
	}
}
