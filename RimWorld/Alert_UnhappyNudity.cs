using Verse;

namespace RimWorld
{
	public class Alert_UnhappyNudity : Alert_Thought
	{
		protected override ThoughtDef Thought => ThoughtDefOf.Naked;

		public Alert_UnhappyNudity()
		{
			defaultLabel = "AlertUnhappyNudity".Translate();
			explanationKey = "AlertUnhappyNudityDesc";
		}
	}
}
