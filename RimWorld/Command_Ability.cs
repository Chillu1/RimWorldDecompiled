using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Command_Ability : Command
{
	protected Ability ability;

	private List<Command_Ability> groupedCasts;

	private string pawnLabel;

	private string originalLabel;

	private string pawnTooltip;

	public bool devGizmo;

	public new static readonly Texture2D BGTex = ContentFinder<Texture2D>.Get("UI/Widgets/AbilityButBG");

	public new static readonly Texture2D BGTexShrunk = ContentFinder<Texture2D>.Get("UI/Widgets/AbilityButBGShrunk");

	private static readonly Texture2D cooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color32(9, 203, 4, 64));

	public Ability Ability => ability;

	public override Texture2D BGTexture => BGTex;

	public override Texture2D BGTextureShrunk => BGTexShrunk;

	public Pawn Pawn { get; private set; }

	public virtual string Tooltip => ability.Tooltip;

	public override string TopRightLabel => ability.GizmoExtraLabel;

	public override bool Disabled
	{
		get
		{
			DisabledCheck();
			return disabled;
		}
		set
		{
			disabled = value;
		}
	}

	public Command_Ability(Ability ability, Pawn pawn)
	{
		this.ability = ability;
		Order = 5f + (((float?)ability.def.category?.displayOrder) ?? 0f) / 100f + (float)ability.def.displayOrder / 1000f + (float)ability.def.level / 10000f;
		defaultLabel = ability.def.LabelCap;
		hotKey = ability.def.hotKey;
		icon = ability.def.uiIcon;
		shrinkable = true;
		Pawn = pawn;
		originalLabel = defaultLabel;
		string text = LabelCap.Colorize(ColoredText.TipSectionTitleColor);
		if (pawn.Name != null)
		{
			string text2 = " (" + pawn.Name.ToStringShort + ")";
			pawnLabel = defaultLabel + text2;
			pawnTooltip = Tooltip.Insert(text.Length, text2);
		}
		else
		{
			pawnLabel = defaultLabel;
			pawnTooltip = Tooltip;
		}
	}

	public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
	{
		defaultLabel = (parms.multipleSelected ? pawnLabel : originalLabel);
		if (devGizmo)
		{
			defaultLabel = "DEV: " + defaultLabel;
		}
		Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
		GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);
		bool flag = (ability.Casting || KeyBindingDefOf.QueueOrder.IsDownEvent) && !ability.CanQueueCast;
		if (flag)
		{
			Widgets.FillableBar(rect, 0f, cooldownBarTex, null, doBorder: false);
		}
		else if (ability.CooldownTicksRemaining > 0)
		{
			float value = Mathf.InverseLerp(ability.CooldownTicksTotal, 0f, ability.CooldownTicksRemaining);
			Widgets.FillableBar(rect, Mathf.Clamp01(value), cooldownBarTex, null, doBorder: false);
			if (ability.CooldownTicksRemaining > 0)
			{
				Text.Font = GameFont.Tiny;
				string text = ability.CooldownTicksRemaining.ToStringTicksToPeriod();
				Vector2 vector = Text.CalcSize(text);
				vector.x += 2f;
				Rect rect2 = rect;
				rect2.x = rect.x + rect.width / 2f - vector.x / 2f;
				rect2.width = vector.x;
				rect2.height = vector.y;
				Rect position = rect2.ExpandedBy(8f, 0f);
				Text.Anchor = TextAnchor.UpperCenter;
				GUI.DrawTexture(position, TexUI.GrayTextBG);
				Widgets.Label(rect2, text);
				Text.Anchor = TextAnchor.UpperLeft;
			}
		}
		if (result.State == GizmoState.Interacted && (bool)ability.CanCast && !flag)
		{
			return result;
		}
		return new GizmoResult(result.State);
	}

	protected override GizmoResult GizmoOnGUIInt(Rect butRect, GizmoRenderParms parms)
	{
		if (Mouse.IsOver(butRect))
		{
			if (parms.multipleSelected)
			{
				if (Pawn.Map != null)
				{
					GenUI.DrawArrowPointingAtWorldspace(Pawn.DrawPos, Find.Camera);
				}
				defaultDesc = pawnTooltip;
			}
			else
			{
				defaultDesc = Tooltip;
			}
		}
		DisabledCheck();
		return base.GizmoOnGUIInt(butRect, parms);
	}

	public override bool GroupsWith(Gizmo other)
	{
		if (ability.def.groupAbility && other is Command_Ability command_Ability)
		{
			return command_Ability.ability.def == ability.def;
		}
		return false;
	}

	public override bool ShowPawnDetailsWith(Gizmo other)
	{
		if (other is Command_Ability command_Ability)
		{
			return command_Ability.ability.def == ability.def;
		}
		return false;
	}

	public virtual void GroupAbilityCommands(List<Gizmo> group)
	{
		if (groupedCasts == null)
		{
			groupedCasts = new List<Command_Ability>();
		}
		groupedCasts.Clear();
		for (int i = 0; i < group.Count; i++)
		{
			if (group[i].GroupsWith(this))
			{
				groupedCasts.Add((Command_Ability)group[i]);
			}
		}
	}

	protected virtual void DisabledCheck()
	{
		disabled = ability.GizmoDisabled(out var reason);
		if (disabled)
		{
			DisableWithReason(reason.CapitalizeFirst());
		}
	}

	private ITargetingSource GetBetterTargetingSource(LocalTargetInfo t)
	{
		groupedCasts.SortBy((Command_Ability c) => c.ability.pawn.Position.DistanceToSquared(t.Cell));
		for (int num = 0; num < groupedCasts.Count; num++)
		{
			if (groupedCasts[num].ability.CanQueueCast && groupedCasts[num].ability.verb.ValidateTarget(t, showMessages: false))
			{
				return groupedCasts[num].ability.verb;
			}
		}
		return null;
	}

	public override void ProcessInput(Event ev)
	{
		base.ProcessInput(ev);
		SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
		if (ability.def.targetRequired)
		{
			Find.DesignatorManager.Deselect();
			if (!ability.def.targetWorldCell)
			{
				if (groupedCasts.NullOrEmpty())
				{
					Find.Targeter.BeginTargeting(ability.verb);
				}
				else
				{
					Find.Targeter.BeginTargeting(ability.verb, null, allowNonSelectedTargetingSource: false, GetBetterTargetingSource);
				}
				return;
			}
			CameraJumper.TryJump(CameraJumper.GetWorldTarget(ability.pawn));
			Find.WorldTargeter.BeginTargeting(delegate(GlobalTargetInfo t)
			{
				if (ability.ValidateGlobalTarget(t))
				{
					ability.QueueCastingJob(t);
					return true;
				}
				return false;
			}, canTargetTiles: true, ability.def.uiIcon, !ability.pawn.IsCaravanMember(), null, ability.WorldMapExtraLabel, ability.ValidateGlobalTarget);
		}
		else
		{
			ability.QueueCastingJob(ability.pawn, LocalTargetInfo.Invalid);
		}
	}

	public override void GizmoUpdateOnMouseover()
	{
		if (ability.verb is Verb_CastAbility verb_CastAbility)
		{
			verb_CastAbility.verbProps.DrawRadiusRing(verb_CastAbility.caster.Position, verb_CastAbility);
		}
		ability.OnGizmoUpdate();
	}

	protected void DisableWithReason(string reason)
	{
		disabledReason = reason;
		disabled = true;
	}
}
