using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Command_Ritual : Command
{
	private Precept_Ritual ritual;

	private RitualObligation obligation;

	private TargetInfo targetInfo;

	private Dictionary<string, Pawn> forcedForRole;

	private readonly IntVec2 PenaltyIconSize = new IntVec2(16, 16);

	private static readonly Texture2D CooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color32(170, 150, 0, 60));

	private static Texture2D penaltyArrowTex;

	private static Texture2D PenaltyArrowTex
	{
		get
		{
			if (penaltyArrowTex == null)
			{
				penaltyArrowTex = ContentFinder<Texture2D>.Get("UI/Icons/Rituals/QualityPenalty");
			}
			return penaltyArrowTex;
		}
	}

	public override string Desc => ritual.TipLabel.Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + ritual.TipMainPart();

	public override string DescPostfix => ritual.TipExtraPart();

	public Command_Ritual(Precept_Ritual ritual, TargetInfo targetInfo, RitualObligation forObligation = null, Dictionary<string, Pawn> forcedForRole = null)
	{
		this.ritual = ritual;
		this.targetInfo = targetInfo;
		this.forcedForRole = forcedForRole;
		obligation = forObligation;
		defaultLabel = ritual.GetBeginRitualText(obligation);
		defaultDesc = ritual.def.description;
		foreach (RitualObligationTrigger obligationTrigger in ritual.obligationTriggers)
		{
			if (obligationTrigger.TriggerExtraDesc != null)
			{
				defaultDesc = defaultDesc + "\n\n" + obligationTrigger.TriggerExtraDesc;
			}
		}
		groupKey = (ritual.canMergeGizmosFromDifferentIdeos ? (-1) : ritual.ideo.id);
		icon = ritual.Icon;
		if (!ritual.def.mergeRitualGizmosFromAllIdeos && !ritual.def.iconIgnoresIdeoColor)
		{
			defaultIconColor = ritual.ideo.Color;
		}
		if (!disabled)
		{
			ValidateDisabledState();
		}
	}

	public override void DrawIcon(Rect rect, Material buttonMat, GizmoRenderParms parms)
	{
		base.DrawIcon(rect, buttonMat, parms);
		if (ritual.RepeatPenaltyActive)
		{
			float value = Mathf.InverseLerp(1200000f, 0f, ritual.TicksSinceLastPerformed);
			Widgets.FillableBar(rect.ContractedBy(1f), Mathf.Clamp01(value), CooldownBarTex, null, doBorder: false);
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.UpperCenter;
			float num = (float)(1200000 - ritual.TicksSinceLastPerformed) / 60000f;
			Widgets.Label(label: "PeriodDays".Translate((!(num >= 1f)) ? ((float)(int)(num * 10f) / 10f) : ((float)Mathf.RoundToInt(num))), rect: rect);
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.DrawTexture(new Rect(rect.xMax - (float)PenaltyIconSize.x, rect.yMin + 4f, PenaltyIconSize.x, PenaltyIconSize.z), PenaltyArrowTex);
		}
	}

	protected override GizmoResult GizmoOnGUIInt(Rect butRect, GizmoRenderParms parms)
	{
		if (!disabled)
		{
			ValidateDisabledState();
		}
		return base.GizmoOnGUIInt(butRect, parms);
	}

	private void ValidateDisabledState()
	{
		string str = ritual.behavior.CanStartRitualNow(targetInfo, ritual);
		if (!str.NullOrEmpty())
		{
			disabled = true;
			disabledReason = str;
		}
		else if (ritual.abilityOnCooldownUntilTick > Find.TickManager.TicksGame)
		{
			disabled = true;
			disabledReason = "AbilityOnCooldown".Translate((ritual.abilityOnCooldownUntilTick - Find.TickManager.TicksGame).ToStringTicksToPeriod()).Resolve();
		}
		else
		{
			if (ritual.def.sourcePawnRoleDef == null || ritual.def.sourceAbilityDef == null)
			{
				return;
			}
			Precept_Role precept_Role = ritual.ideo.RolesListForReading.FirstOrDefault((Precept_Role r) => r.def == ritual.def.sourcePawnRoleDef);
			if (precept_Role != null && precept_Role is Precept_RoleSingle precept_RoleSingle && precept_RoleSingle.ChosenPawnSingle() != null)
			{
				Ability ability = precept_RoleSingle.AbilitiesForReading.FirstOrDefault((Ability a) => a.def == ritual.def.sourceAbilityDef);
				if (ability != null)
				{
					disabled = ability.GizmoDisabled(out disabledReason);
				}
			}
		}
	}

	public override void GizmoUpdateOnMouseover()
	{
		base.GizmoUpdateOnMouseover();
		ritual.behavior?.DrawPreviewOnTarget(targetInfo);
	}

	public override void ProcessInput(Event ev)
	{
		base.ProcessInput(ev);
		ritual.ShowRitualBeginWindow(targetInfo, null, null, forcedForRole);
		SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
	}
}
