using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class Dialog_BeginPsychicRitual : Dialog_BeginLordJob
{
	private PsychicRitualDef psychicRitualDef;

	private PsychicRitualRoleAssignments assignments;

	private Map map;

	private static List<Pawn> tmpSleepingPawns = new List<Pawn>(16);

	private static List<Pawn> tmpDraftedPawns = new List<Pawn>(16);

	public override TaggedString HeaderLabel => psychicRitualDef.LabelCap;

	public override TaggedString DescriptionLabel => psychicRitualDef.description.CapitalizeFirst();

	public override TaggedString ExpectedQualityLabel => "PsychicRitualExpectedQualityLabel".Translate().CapitalizeFirst();

	public override TaggedString QualityFactorsLabel => "PsychicRitualQualityFactorLabel".Translate().CapitalizeFirst();

	public override Texture2D Icon => psychicRitualDef.uiIcon;

	public Dialog_BeginPsychicRitual(PsychicRitualDef def, PsychicRitualCandidatePool candidatePool, PsychicRitualRoleAssignments assignments, Map map)
		: base(new PawnPsychicRitualRoleSelectionWidget(def, candidatePool, assignments)
		{
			showIdeoIcon = false
		})
	{
		psychicRitualDef = def;
		this.assignments = assignments;
		this.map = map;
	}

	protected override IEnumerable<string> BlockingIssues()
	{
		return assignments.BlockingIssues().Concat(psychicRitualDef.BlockingIssues(assignments, map));
	}

	protected override void Start()
	{
		psychicRitualDef.MakeNewLord(assignments);
		base.Start();
		Find.PsychicRitualManager.RegisterCooldown(psychicRitualDef);
	}

	protected override List<QualityFactor> PopulateQualityFactors(out FloatRange qualityRange)
	{
		FloatRange qualityRange2;
		List<QualityFactor> list = base.PopulateQualityFactors(out qualityRange2);
		psychicRitualDef.CalculateMaxPower(assignments, list, out var power);
		if (power > 0f)
		{
			qualityRange = new FloatRange(power, power);
		}
		else
		{
			qualityRange = new FloatRange(0f, 0f);
		}
		foreach (QualityFactor item in list)
		{
			item.qualityChange = ((item.quality >= 0f) ? "+ " : "- ") + Mathf.Abs(item.quality).ToStringPercent();
		}
		return list;
	}

	private static List<Pawn> SleepingPawns(PsychicRitualRoleAssignments assignments)
	{
		tmpSleepingPawns.Clear();
		foreach (var (psychicRitualRoleDef2, list2) in assignments.RoleAssignments)
		{
			if (psychicRitualRoleDef2.ConditionAllowed(PsychicRitualRoleDef.Condition.Sleeping))
			{
				continue;
			}
			foreach (Pawn item in list2)
			{
				if (!item.Awake() && item.health.capacities.CanBeAwake)
				{
					tmpSleepingPawns.Add(item);
				}
			}
		}
		return tmpSleepingPawns;
	}

	private static List<Pawn> DraftedPawns(PsychicRitualRoleAssignments assignments)
	{
		tmpDraftedPawns.Clear();
		foreach (var (psychicRitualRoleDef2, list2) in assignments.RoleAssignments)
		{
			if (psychicRitualRoleDef2.ConditionAllowed(PsychicRitualRoleDef.Condition.Drafted))
			{
				continue;
			}
			foreach (Pawn item in list2)
			{
				if (item.Drafted)
				{
					tmpDraftedPawns.Add(item);
				}
			}
		}
		return tmpDraftedPawns;
	}

	public override void DrawExtraOutcomeDescriptions(Rect viewRect, FloatRange qualityRange, string qualityNumber, ref float curY, ref float totalInfoHeight)
	{
		curY += Text.LineHeight;
		totalInfoHeight += Text.LineHeight;
		TaggedString label = psychicRitualDef.OutcomeDescription(qualityRange, qualityNumber, assignments);
		float num = Text.CalcHeight(label.Resolve(), viewRect.width);
		Widgets.Label(new Rect(viewRect.x, curY, viewRect.width, num), label);
		curY += num;
		totalInfoHeight += num;
		foreach (TaggedString item in psychicRitualDef.OutcomeWarnings(assignments))
		{
			float num2 = Math.Max(Text.CalcHeight(item, viewRect.width) + 3f, 28f);
			Widgets.Label(new Rect(viewRect.x, curY + 4f, viewRect.width, num2), item);
			curY += num2;
			totalInfoHeight += num2;
		}
		List<Pawn> list = SleepingPawns(assignments);
		if (list.Count > 0)
		{
			string text = list.Select((Pawn pawn) => pawn.LabelShortCap).ToCommaList(useAnd: true);
			TaggedString taggedString = ((list.Count > 1) ? "PsychicRitualWakingPawnsWarning" : "PsychicRitualWakingPawnWarning").Translate(text);
			float num3 = Math.Max(Text.CalcHeight(taggedString, viewRect.width) + 3f, 28f);
			Widgets.Label(new Rect(viewRect.x, curY + 4f, viewRect.width, num3), taggedString);
			curY += num3;
			totalInfoHeight += num3;
		}
		List<Pawn> list2 = DraftedPawns(assignments);
		if (list2.Count > 0)
		{
			string text2 = list2.Select((Pawn pawn) => pawn.LabelShortCap).ToCommaList(useAnd: true);
			TaggedString taggedString2 = ((list2.Count > 1) ? "PsychicRitualUndraftPawnsWarning" : "PsychicRitualUndraftPawnWarning").Translate(text2);
			float num4 = Math.Max(Text.CalcHeight(taggedString2, viewRect.width) + 3f, 28f);
			Widgets.Label(new Rect(viewRect.x, curY + 4f, viewRect.width, num4), taggedString2);
			curY += num4;
			totalInfoHeight += num4;
		}
	}

	public override TaggedString ExpectedDurationLabel(FloatRange qualityRange)
	{
		return psychicRitualDef.TimeAndOfferingLabel();
	}
}
