namespace Verse;

public interface IPathFindCostProvider
{
	ushort PathFindCostFor(Pawn pawn);

	CellRect GetOccupiedRect();
}
