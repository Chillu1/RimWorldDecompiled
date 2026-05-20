using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Ability_RandomEffect : Ability
	{
		public Ability_RandomEffect(Pawn pawn)
			: base(pawn)
		{
		}

		public Ability_RandomEffect(Pawn pawn, AbilityDef def)
			: base(pawn, def)
		{
		}

		public override bool CanApplyOn(LocalTargetInfo target)
		{
			foreach (CompAbilityEffect effectComp in base.EffectComps)
			{
				if (effectComp.CanApplyOn(target, null))
				{
					return true;
				}
			}
			return false;
		}

		protected override void ApplyEffects(IEnumerable<CompAbilityEffect> effects, LocalTargetInfo target, LocalTargetInfo dest)
		{
			effects.Where((CompAbilityEffect x) => x.Props.weight > 0f && x.CanApplyOn(target, dest)).RandomElementByWeight((CompAbilityEffect x) => x.Props.weight).Apply(target, dest);
		}
	}
}
