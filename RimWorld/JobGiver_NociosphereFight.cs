using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_NociosphereFight : ThinkNode_JobGiver
{
	private readonly List<CompProperties_Nociosphere.AttackDetails> tmpAttacks = new List<CompProperties_Nociosphere.AttackDetails>();

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (IsAnyAttackOnCooldown(pawn))
		{
			return null;
		}
		Thing thing = NociosphereUtility.FindTarget(pawn);
		if (thing == null)
		{
			return null;
		}
		return GetRandomPossibleAbility(pawn, thing)?.GetJob(thing, null);
	}

	private bool IsAnyAttackOnCooldown(Pawn pawn)
	{
		List<CompProperties_Nociosphere.AttackDetails> attacks = pawn.TryGetComp<CompNociosphere>().Props.attacks;
		for (int i = 0; i < attacks.Count; i++)
		{
			Ability ability = pawn.abilities.GetAbility(attacks[i].ability);
			if (!ability.CanCast && (float)ability.CooldownTicksRemaining <= attacks[i].maxCooldownTicks)
			{
				return true;
			}
		}
		return false;
	}

	private Ability GetRandomPossibleAbility(Pawn pawn, Thing target)
	{
		List<CompProperties_Nociosphere.AttackDetails> attacks = pawn.TryGetComp<CompNociosphere>().Props.attacks;
		bool flag = false;
		for (int i = 0; i < attacks.Count; i++)
		{
			Ability ability = pawn.abilities.GetAbility(attacks[i].ability);
			Verb verb = ability.verb;
			if ((bool)ability.CanCast && verb.CanHitTarget(target) && (attacks[i].requiresPawn || target is Pawn))
			{
				if (attacks[i].aiPreferArtificial)
				{
					flag = true;
				}
				tmpAttacks.Add(attacks[i]);
			}
		}
		if (tmpAttacks.Empty())
		{
			return null;
		}
		bool artificialTarget = target == null || target is Building || target is Plant;
		CompProperties_Nociosphere.AttackDetails attackDetails = ((!flag) ? tmpAttacks.RandomElement() : tmpAttacks.RandomElementByWeight((CompProperties_Nociosphere.AttackDetails a) => a.aiPreferArtificial ? ((float)((!artificialTarget) ? 1 : 2)) : ((float)(artificialTarget ? 1 : 2))));
		tmpAttacks.Clear();
		return pawn.abilities.GetAbility(attackDetails.ability);
	}
}
