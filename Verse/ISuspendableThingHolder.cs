namespace Verse
{
	public interface ISuspendableThingHolder : IThingHolder
	{
		bool IsContentsSuspended { get; }
	}
}
