using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Dialog_BeginGravshipLaunch : Dialog_BeginRitual
{
	private bool forceVisitorsToLeave = true;

	private bool boardColonyAnimals = true;

	private bool boardColonyMechs = true;

	private const float CheckboxHeight = 28f;

	public Dialog_BeginGravshipLaunch(string ritualLabel, Precept_Ritual ritual, TargetInfo target, Map map, ActionCallback action, Pawn organizer, RitualObligation obligation, PawnFilter filter = null, string okButtonText = null, List<Pawn> requiredPawns = null, Dictionary<string, Pawn> forcedForRole = null, RitualOutcomeEffectDef outcome = null, List<string> extraInfoText = null, Pawn selectedPawn = null)
		: base(ritualLabel, ritual, target, map, action, organizer, obligation, filter, okButtonText, requiredPawns, forcedForRole, outcome, extraInfoText, selectedPawn)
	{
	}

	protected override void Start()
	{
		RitualBehaviorWorker_GravshipLaunch obj = ritual.behavior as RitualBehaviorWorker_GravshipLaunch;
		obj.forceVisitorsToLeave = forceVisitorsToLeave;
		obj.boardColonyAnimals = boardColonyAnimals;
		obj.boardColonyMechs = boardColonyMechs;
		ActionCallback actionCallback = action;
		if (actionCallback != null && actionCallback(assignments))
		{
			Close();
		}
	}

	public override void DoRightColumn(ref RectDivider layout)
	{
		float height = (ModsConfig.BiotechActive ? 84f : 56f);
		Rect rect = layout.NewRow(height).Rect;
		Rect rect2 = new Rect(rect.x, rect.y, 402f, 28f);
		Widgets.CheckboxLabeled(rect2, "GravshipForceVisitorsToLeaveLabel".Translate(), ref forceVisitorsToLeave);
		TooltipHandler.TipRegion(rect2, "GravshipForceVisitorsToLeaveTooltip".Translate());
		rect2.y += 28f;
		Widgets.CheckboxLabeled(rect2, "GravshipBoardColonyAnimalsLabel".Translate(), ref boardColonyAnimals);
		TooltipHandler.TipRegion(rect2, "GravshipBoardColonyAnimalsTooltip".Translate());
		if (ModsConfig.BiotechActive)
		{
			rect2.y += 28f;
			Widgets.CheckboxLabeled(rect2, "GravshipBoardColonyMechsLabel".Translate(), ref boardColonyMechs);
			TooltipHandler.TipRegion(rect2, "GravshipBoardColonyMechsTooltip".Translate());
		}
		layout.NewRow(17f);
		base.DoRightColumn(ref layout);
	}
}
