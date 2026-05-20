using Verse;

namespace RimWorld;

public interface ILordJobOutcomePossibility
{
	TaggedString Label { get; }

	TaggedString ToolTip { get; }

	float Weight(FloatRange qualityRange);
}
