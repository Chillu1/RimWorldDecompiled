namespace Verse;

public interface IBuildableDriver
{
	bool TryGetBuildableRect(out CellRect rect);
}
