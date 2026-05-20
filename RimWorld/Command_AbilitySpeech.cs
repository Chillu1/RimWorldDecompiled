using UnityEngine;
using Verse;

namespace RimWorld;

public class Command_AbilitySpeech : Command_Ability
{
	private Precept_Ritual ritualCached;

	protected Precept_Ritual Ritual
	{
		get
		{
			if (ritualCached == null)
			{
				CompAbilityEffect_StartRitual compAbilityEffect_StartRitual = ability.CompOfType<CompAbilityEffect_StartRitual>();
				if (compAbilityEffect_StartRitual != null)
				{
					ritualCached = compAbilityEffect_StartRitual.Ritual;
				}
			}
			return ritualCached;
		}
	}

	public override string Tooltip
	{
		get
		{
			if (Ritual == null)
			{
				return "";
			}
			string text = Ritual.Label.Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + Ritual.def.description.Formatted(ability.pawn.Named("ORGANIZER")).Resolve() + "\n";
			if (ability.CooldownTicksRemaining > 0)
			{
				text = text + "\n" + "AbilitySpeechCooldown".Translate().Resolve() + ": " + ability.CooldownTicksRemaining.ToStringTicksToPeriod();
			}
			if (Ritual.outcomeEffect != null)
			{
				text = text + "\n" + Ritual.outcomeEffect.ExtraAlertParagraph(Ritual);
			}
			text = text + "\n\n" + ("AbilitySpeechTargetsLabel".Translate().Resolve() + ":").Colorize(ColoredText.TipSectionTitleColor) + "\n" + Ritual.targetFilter.GetTargetInfos(ability.pawn).ToLineList(" -  ", capitalizeItems: true);
			return text.CapitalizeFirst();
		}
	}

	public Command_AbilitySpeech(Ability ability, Pawn pawn)
		: base(ability, pawn)
	{
		defaultLabel = Ritual?.GetBeginRitualText() ?? ((TaggedString)"");
		icon = ((ability.def.iconPath == null) ? Ritual.Icon : ContentFinder<Texture2D>.Get(ability.def.iconPath));
	}
}
