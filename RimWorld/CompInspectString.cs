using Verse;

namespace RimWorld
{
	public class CompInspectString : ThingComp
	{
		public CompProperties_InspectString Props => (CompProperties_InspectString)props;

		public override string CompInspectStringExtra()
		{
			string text = base.CompInspectStringExtra();
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			return text + Props.inspectString.Formatted(parent.Named("PARENT")).Resolve();
		}
	}
}
