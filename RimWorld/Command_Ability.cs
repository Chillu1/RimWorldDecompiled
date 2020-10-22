using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class Command_Ability : Command
	{
		protected Ability ability;

		public new static readonly Texture2D BGTex = ContentFinder<Texture2D>.Get("UI/Widgets/AbilityButBG");

		public new static readonly Texture2D BGTexShrunk = ContentFinder<Texture2D>.Get("UI/Widgets/AbilityButBGShrunk");

		private static readonly Texture2D cooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color32(9, 203, 4, 64));

		public Ability Ability => ability;

		public override Texture2D BGTexture => BGTex;

		public override Texture2D BGTextureShrunk => BGTexShrunk;

		public virtual string Tooltip => ability.def.GetTooltip(ability.pawn);

		public Command_Ability(Ability ability)
		{
			this.ability = ability;
			order = 5f;
			defaultLabel = ability.def.LabelCap;
			hotKey = ability.def.hotKey;
			icon = ability.def.uiIcon;
		}

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
		{
			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth);
			if (ability.CooldownTicksRemaining > 0)
			{
				float num = Mathf.InverseLerp(ability.CooldownTicksTotal, 0f, ability.CooldownTicksRemaining);
				Widgets.FillableBar(rect, Mathf.Clamp01(num), cooldownBarTex, null, doBorder: false);
				if (ability.CooldownTicksRemaining > 0)
				{
					Text.Font = GameFont.Tiny;
					Text.Anchor = TextAnchor.UpperCenter;
					Widgets.Label(rect, num.ToStringPercent("F0"));
					Text.Anchor = TextAnchor.UpperLeft;
				}
			}
			if (result.State == GizmoState.Interacted && ability.CanCast)
			{
				return result;
			}
			return new GizmoResult(result.State);
		}

		protected override GizmoResult GizmoOnGUIInt(Rect butRect, bool shrunk = false)
		{
			if (Mouse.IsOver(butRect))
			{
				defaultDesc = Tooltip;
			}
			DisabledCheck();
			return base.GizmoOnGUIInt(butRect, shrunk);
		}

		protected virtual void DisabledCheck()
		{
			disabled = ability.GizmoDisabled(out var reason);
			if (disabled)
			{
				DisableWithReason(reason.CapitalizeFirst());
			}
		}

		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
			SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
			if (ability.def.targetRequired)
			{
				if (!ability.def.targetWorldCell)
				{
					Find.Targeter.BeginTargeting(ability.verb);
					return;
				}
				CameraJumper.TryJump(CameraJumper.GetWorldTarget(ability.pawn));
				Find.WorldTargeter.BeginTargeting_NewTemp(delegate(GlobalTargetInfo t)
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
			Verb_CastAbility verb_CastAbility;
			if ((verb_CastAbility = ability.verb as Verb_CastAbility) != null)
			{
				verb_CastAbility.DrawRadius();
			}
		}

		protected void DisableWithReason(string reason)
		{
			disabledReason = reason;
			disabled = true;
		}
	}
}
