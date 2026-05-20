using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_StudyUnlocks : CompProperties
{
	public List<StudyNote> studyNotes = new List<StudyNote>();

	public float? defaultStudyAmount;

	public KnowledgeCategoryDef defaultCategoryOverride;

	public CompProperties_StudyUnlocks()
	{
		compClass = typeof(CompStudyUnlocks);
	}

	public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
	{
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
		float num = 0f;
		for (int i = 0; i < studyNotes.Count; i++)
		{
			StudyNote note = studyNotes[i];
			if (num != 0f)
			{
				if (note.threshold != 0f && note.threshold <= num)
				{
					yield return $"Threshold for note at index {i} had a threshold value ({note.threshold}) lower than the previous maximum ({num})";
				}
				else if (note.threshold == 0f && note.thresholdRange.min <= num)
				{
					yield return $"Threshold for note at index {i} had a min threshold value ({note.thresholdRange.min}) lower than the previous maximum ({num})";
				}
			}
			num = ((note.threshold != 0f) ? note.threshold : note.thresholdRange.max);
		}
	}
}
