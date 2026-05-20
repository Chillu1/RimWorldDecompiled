namespace Verse;

public abstract class SectionLayer_Dynamic : SectionLayer
{
	public virtual bool ShouldDrawDynamic(CellRect view)
	{
		return true;
	}

	public SectionLayer_Dynamic(Section section)
		: base(section)
	{
		base.section = section;
	}
}
