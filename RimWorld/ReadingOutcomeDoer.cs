using Verse;

namespace RimWorld;

public abstract class ReadingOutcomeDoer
{
	protected ReadingOutcomeProperties props;

	public ThingWithComps Parent { get; private set; }

	public CompReadable Readable { get; private set; }

	public ReadingOutcomeProperties Props => props;

	public virtual void Initialize(ThingWithComps parent, ReadingOutcomeProperties props)
	{
		Parent = parent;
		Readable = parent.GetComp<CompReadable>();
		this.props = props;
	}

	public virtual void PostMake()
	{
	}

	public virtual void OnReadingTick(Pawn reader, float factor)
	{
	}

	public virtual void PostExposeData()
	{
	}
}
