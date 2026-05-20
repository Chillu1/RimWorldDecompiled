using Verse;

namespace RimWorld;

public class StudyNote
{
	public float threshold;

	public FloatRange thresholdRange = FloatRange.Zero;

	public float? studyKnowledgeAmount;

	[MustTranslate]
	public string label;

	[MustTranslate]
	public string text;
}
