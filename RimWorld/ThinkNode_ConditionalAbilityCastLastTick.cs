using Verse;
using Verse.AI;

namespace RimWorld;

public class ThinkNode_ConditionalAbilityCastLastTick : ThinkNode_Conditional
{
	public int maxTicksAgo = int.MaxValue;

	public int minTicksAgo;

	public AbilityDef ability;

	protected override bool Satisfied(Pawn pawn)
	{
		Ability ability = pawn.abilities?.GetAbility(this.ability);
		if (ability == null)
		{
			return false;
		}
		if (ability.lastCastTick < 0)
		{
			return false;
		}
		int num = Find.TickManager.TicksGame - ability.lastCastTick;
		if (num > maxTicksAgo)
		{
			return false;
		}
		if (num < minTicksAgo)
		{
			return false;
		}
		return true;
	}

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		ThinkNode_ConditionalAbilityCastLastTick obj = (ThinkNode_ConditionalAbilityCastLastTick)base.DeepCopy(resolve);
		obj.ability = ability;
		obj.maxTicksAgo = maxTicksAgo;
		obj.minTicksAgo = minTicksAgo;
		return obj;
	}
}
