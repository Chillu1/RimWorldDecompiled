using System.Linq;
using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_TrainRandomSkill : CompAbilityEffect
	{
		private const float XPGainAmount = 50000f;

		public new CompProperties_AbilityTrainRandomSkill Props => (CompProperties_AbilityTrainRandomSkill)props;

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			base.Apply(target, dest);
			if (target.Pawn != null)
			{
				SkillDef skillDef = DefDatabase<SkillDef>.AllDefs.Where((SkillDef x) => !target.Pawn.skills.GetSkill(x).TotallyDisabled).RandomElement();
				int level = target.Pawn.skills.GetSkill(skillDef).GetLevel(includeAptitudes: false);
				target.Pawn.skills.Learn(skillDef, 50000f, direct: true);
				if (base.SendLetter)
				{
					int num = target.Pawn.skills.GetSkill(skillDef).GetLevel(includeAptitudes: false) - level;
					Find.LetterStack.ReceiveLetter(Props.customLetterLabel.Formatted(skillDef.LabelCap), Props.customLetterText.Formatted(parent.pawn, target.Pawn, skillDef, num), LetterDefOf.PositiveEvent, new LookTargets(target.Pawn));
				}
			}
		}
	}
}
