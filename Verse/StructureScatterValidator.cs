namespace Verse;

public class StructureScatterValidator : ScattererValidator
{
	public CellRect structureRect;

	public float maxDistFromStructure;

	public override bool Allows(IntVec3 c, Map map)
	{
		if (!c.InHorDistOf(structureRect.CenterCell, maxDistFromStructure))
		{
			return false;
		}
		return true;
	}
}
