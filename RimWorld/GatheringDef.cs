using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class GatheringDef : Def
{
	public Type workerClass = typeof(GatheringWorker);

	public DutyDef duty;

	public float randomSelectionWeight;

	public bool respectTimetable = true;

	public List<ThingDef> gatherSpotDefs;

	[MustTranslate]
	public string letterTitle;

	[MustTranslate]
	public string letterText;

	[MustTranslate]
	public string calledOffMessage;

	[MustTranslate]
	public string finishedMessage;

	public List<RoyalTitleDef> requiredTitleAny = new List<RoyalTitleDef>();

	private GatheringWorker worker;

	public bool IsRandomSelectable => randomSelectionWeight > 0f;

	public GatheringWorker Worker
	{
		get
		{
			if (worker == null)
			{
				worker = (GatheringWorker)Activator.CreateInstance(workerClass);
				worker.def = this;
			}
			return worker;
		}
	}

	public bool CanExecute(Map map, Pawn organizer = null, bool ignoreGameConditions = false)
	{
		if (ignoreGameConditions || GatheringsUtility.AcceptableGameConditionsToStartGathering(map, this))
		{
			return Worker.CanExecute(map, organizer);
		}
		return false;
	}
}
