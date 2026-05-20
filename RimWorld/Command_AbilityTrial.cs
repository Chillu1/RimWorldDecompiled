using UnityEngine;
using Verse;

namespace RimWorld;

public class Command_AbilityTrial : Command_AbilitySpeech
{
	public Command_AbilityTrial(Ability ability, Pawn pawn)
		: base(ability, pawn)
	{
		defaultLabel = "Accuse".Translate();
		defaultIconColor = Color.white;
	}
}
