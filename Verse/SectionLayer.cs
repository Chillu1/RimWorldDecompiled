namespace Verse;

public abstract class SectionLayer : MapDrawLayer
{
	protected Section section;

	public SectionLayer(Section section)
		: base(section.map)
	{
		this.section = section;
	}

	public override CellRect GetBoundaryRect()
	{
		return section.CellRect;
	}
}
