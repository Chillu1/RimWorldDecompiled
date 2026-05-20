using System.Collections.Generic;

namespace Verse;

public class SubEffecter_Random : SubEffecter
{
	public SubEffecter child;

	public SubEffecter_Random(SubEffecterDef subDef, Effecter parent)
		: base(subDef, parent)
	{
		if (def.children == null)
		{
			return;
		}
		List<float> list = new List<float>();
		foreach (SubEffecterDef child in subDef.children)
		{
			list.Add(child.randomWeight);
		}
		this.child = subDef.children.RandomElementByWeight((SubEffecterDef p) => p.randomWeight).Spawn(parent);
	}

	public override void SubEffectTick(TargetInfo A, TargetInfo B)
	{
		child?.SubEffectTick(A, B);
	}

	public override void SubTrigger(TargetInfo A, TargetInfo B, int overrideSpawnTick = -1, bool force = false)
	{
		child?.SubTrigger(A, B, overrideSpawnTick, force);
	}
}
