using Verse;

namespace RimWorld
{
	public class CompNeurotrainer : CompUsable
	{
		public SkillDef skill;

		public AbilityDef ability;

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Defs.Look(ref skill, "skill");
			Scribe_Defs.Look(ref ability, "ability");
		}

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			CompProperties_Neurotrainer compProperties_Neurotrainer = (CompProperties_Neurotrainer)props;
			ability = compProperties_Neurotrainer.ability;
			skill = compProperties_Neurotrainer.skill;
		}

		protected override string FloatMenuOptionLabel(Pawn pawn)
		{
			return string.Format(base.Props.useLabel, (skill != null) ? skill.skillLabel : ability.label);
		}

		public override bool AllowStackWith(Thing other)
		{
			if (!base.AllowStackWith(other))
			{
				return false;
			}
			CompNeurotrainer compNeurotrainer = other.TryGetComp<CompNeurotrainer>();
			if (compNeurotrainer == null || compNeurotrainer.skill != skill || compNeurotrainer.ability != ability)
			{
				return false;
			}
			return true;
		}

		public override void PostSplitOff(Thing piece)
		{
			base.PostSplitOff(piece);
			CompNeurotrainer compNeurotrainer = piece.TryGetComp<CompNeurotrainer>();
			if (compNeurotrainer != null)
			{
				compNeurotrainer.skill = skill;
				compNeurotrainer.ability = ability;
			}
		}
	}
}
