namespace Verse;

public interface IRenameable
{
	string RenamableLabel { get; set; }

	string BaseLabel { get; }

	string InspectLabel { get; }
}
