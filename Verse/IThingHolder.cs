using System.Collections.Generic;

namespace Verse
{
	public interface IThingHolder
	{
		IThingHolder ParentHolder
		{
			get;
		}

		void GetChildHolders(List<IThingHolder> outChildren);

		ThingOwner GetDirectlyHeldThings();
	}
}
