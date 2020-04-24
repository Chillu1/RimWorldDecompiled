using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class ItemStashContentsComp : WorldObjectComp, IThingHolder
	{
		public ThingOwner contents;

		public ItemStashContentsComp()
		{
			contents = new ThingOwner<Thing>(this);
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Deep.Look(ref contents, "contents", this);
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return contents;
		}

		public override void PostDestroy()
		{
			base.PostDestroy();
			contents.ClearAndDestroyContents();
		}
	}
}
