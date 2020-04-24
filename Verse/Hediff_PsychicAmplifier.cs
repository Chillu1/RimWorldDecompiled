using RimWorld;
using System;
using System.Linq;

namespace Verse
{
	public class Hediff_PsychicAmplifier : Hediff_ImplantWithLevel
	{
		public override void PostAdd(DamageInfo? dinfo)
		{
			base.PostAdd(dinfo);
			TryGiveAbilityOfLevel(level);
		}

		public override void ChangeLevel(int levelOffset)
		{
			if (levelOffset > 0)
			{
				float num = Math.Min(levelOffset, def.maxSeverity - (float)level);
				for (int i = 0; (float)i < num; i++)
				{
					int abilityLevel = level + 1 + i;
					TryGiveAbilityOfLevel(abilityLevel);
				}
			}
			base.ChangeLevel(levelOffset);
		}

		public void TryGiveAbilityOfLevel(int abilityLevel)
		{
			if (!pawn.abilities.abilities.Any((Ability a) => a.def.level == abilityLevel))
			{
				AbilityDef abilityDef = DefDatabase<AbilityDef>.AllDefs.Where((AbilityDef a) => a.level == abilityLevel).RandomElement();
				pawn.abilities.GainAbility(abilityDef);
				if (PawnUtility.ShouldSendNotificationAbout(pawn))
				{
					Messages.Message("PsycastLearnedFromImplant".Translate(pawn.Named("USER"), abilityLevel, abilityDef.LabelCap), pawn, MessageTypeDefOf.PositiveEvent);
				}
			}
		}
	}
}
