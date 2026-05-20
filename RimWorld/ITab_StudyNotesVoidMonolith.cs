using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ITab_StudyNotesVoidMonolith : ITab_StudyNotes
{
	public override bool IsVisible
	{
		get
		{
			if (base.Studiable && base.StudiableThing is Building_VoidMonolith)
			{
				return !Find.Anomaly.QuestlineEnded;
			}
			return false;
		}
	}

	protected override IReadOnlyList<ChoiceLetter> Letters => Find.Anomaly.MonolithLetters;

	protected override bool StudyCompleted => Find.Anomaly.MonolithStudyCompleted;
}
