using Verse;

namespace RimWorld
{
	public class ThingSetMakerDef : Def
	{
		public ThingSetMaker root;

		public ThingSetMakerParams debugParams;

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			root.ResolveReferences();
		}
	}
}
