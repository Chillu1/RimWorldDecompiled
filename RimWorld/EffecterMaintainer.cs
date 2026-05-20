using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class EffecterMaintainer
{
	public struct MaintainedEffecter
	{
		public Effecter Effecter;

		public TargetInfo A;

		public TargetInfo B;

		public MaintainedEffecter(Effecter effecter, TargetInfo A, TargetInfo B)
		{
			Effecter = effecter;
			this.A = A;
			this.B = B;
		}
	}

	private Map map;

	private List<MaintainedEffecter> maintainedEffecters = new List<MaintainedEffecter>();

	public EffecterMaintainer(Map map)
	{
		this.map = map;
	}

	public void AddEffecterToMaintain(Effecter eff, IntVec3 pos, int ticks)
	{
		eff.ticksLeft = ticks;
		TargetInfo a = new TargetInfo(pos, map);
		maintainedEffecters.Add(new MaintainedEffecter(eff, a, TargetInfo.Invalid));
	}

	public void AddEffecterToMaintain(Effecter eff, Thing target, int ticks)
	{
		eff.ticksLeft = ticks;
		maintainedEffecters.Add(new MaintainedEffecter(eff, target, TargetInfo.Invalid));
	}

	public void AddEffecterToMaintain(Effecter eff, TargetInfo A, TargetInfo B, int ticks)
	{
		eff.ticksLeft = ticks;
		maintainedEffecters.Add(new MaintainedEffecter(eff, A, B));
	}

	public void EffecterMaintainerTick()
	{
		for (int num = maintainedEffecters.Count - 1; num >= 0; num--)
		{
			MaintainedEffecter maintainedEffecter = maintainedEffecters[num];
			if (maintainedEffecter.Effecter.ticksLeft > 0)
			{
				maintainedEffecter.Effecter.EffectTick(maintainedEffecter.A, maintainedEffecter.B);
				maintainedEffecter.Effecter.ticksLeft--;
			}
			else
			{
				maintainedEffecter.Effecter.Cleanup();
				maintainedEffecters.RemoveAt(num);
			}
		}
	}
}
