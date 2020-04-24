using Verse;

namespace RimWorld
{
	public interface IActiveDropPod : IThingHolder
	{
		ActiveDropPodInfo Contents
		{
			get;
		}
	}
}
