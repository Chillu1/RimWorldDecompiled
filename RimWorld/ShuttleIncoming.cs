using Verse;

namespace RimWorld
{
	public class ShuttleIncoming : Skyfaller, IActiveDropPod, IThingHolder
	{
		public ActiveDropPodInfo Contents
		{
			get
			{
				return ((ActiveDropPod)innerContainer[0]).Contents;
			}
			set
			{
				((ActiveDropPod)innerContainer[0]).Contents = value;
			}
		}
	}
}
