using Verse;

namespace RimWorld;

public interface ILordJobRole
{
	int MaxCount { get; }

	int MinCount { get; }

	TaggedString Label { get; }

	TaggedString LabelCap { get; }

	TaggedString CategoryLabel { get; }

	TaggedString CategoryLabelCap { get; }
}
