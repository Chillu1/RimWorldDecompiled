using Verse;

namespace RimWorld
{
	public class Command_AbilitySpeech : Command_Ability
	{
		public override string Tooltip
		{
			get
			{
				TaggedString taggedString = ability.def.LabelCap + "\n\n" + "AbilitySpeechTooltip".Translate(ability.pawn.Named("ORGANIZER")) + "\n";
				if (ability.CooldownTicksRemaining > 0)
				{
					taggedString += "\n" + "AbilitySpeechCooldown".Translate() + ": " + ability.CooldownTicksRemaining.ToStringTicksToPeriod();
				}
				taggedString += "\n" + GatheringWorker_Speech.OutcomeBreakdownForPawn(ability.pawn);
				return taggedString;
			}
		}

		public Command_AbilitySpeech(Ability ability)
			: base(ability)
		{
		}
	}
}
