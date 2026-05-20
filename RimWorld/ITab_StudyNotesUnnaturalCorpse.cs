using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ITab_StudyNotesUnnaturalCorpse : ITab_StudyNotes
{
	public override bool IsVisible
	{
		get
		{
			if (base.Studiable)
			{
				return base.StudiableThing is UnnaturalCorpse;
			}
			return false;
		}
	}

	private UnnaturalCorpse UnnaturalCorpse => (UnnaturalCorpse)base.StudiableThing;

	private UnnaturalCorpseTracker Tracker => UnnaturalCorpse.Tracker;

	protected override IReadOnlyList<ChoiceLetter> Letters => Tracker.Letters;

	protected override bool StudyCompleted => Tracker.CanDestroyViaResearch;

	protected override bool DrawThingIcon => false;
}
