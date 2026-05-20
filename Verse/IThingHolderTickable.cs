namespace Verse;

public interface IThingHolderTickable : IThingHolder
{
	bool ShouldTickContents { get; }
}
