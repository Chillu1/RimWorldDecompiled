using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RitualVisualEffect : IExposable
{
	public LordJob_Ritual ritual;

	public RitualVisualEffectDef def;

	public List<RitualVisualEffectComp> comps = new List<RitualVisualEffectComp>();

	private List<Mote> maintainedMotes = new List<Mote>();

	private List<Pair<Effecter, TargetInfo>> maintainedEffecters = new List<Pair<Effecter, TargetInfo>>();

	public void Setup(LordJob_Ritual r, bool loading)
	{
		ritual = r;
		if (!loading)
		{
			foreach (CompProperties_RitualVisualEffect comp in def.comps)
			{
				RitualVisualEffectComp instance = comp.GetInstance();
				instance.props = comp;
				instance.OnSetup(this, ritual, loading: false);
				comps.Add(instance);
			}
			return;
		}
		for (int i = 0; i < comps.Count; i++)
		{
			RitualVisualEffectComp ritualVisualEffectComp = comps[i];
			ritualVisualEffectComp.props = def.comps[i];
			ritualVisualEffectComp.OnSetup(this, ritual, loading: true);
		}
	}

	public void Tick()
	{
		foreach (RitualVisualEffectComp comp in comps)
		{
			comp.TickInterval(1);
		}
		foreach (Mote maintainedMote in maintainedMotes)
		{
			maintainedMote.Maintain();
		}
		foreach (Pair<Effecter, TargetInfo> maintainedEffecter in maintainedEffecters)
		{
			maintainedEffecter.First.EffectTick(maintainedEffecter.Second, maintainedEffecter.Second);
		}
	}

	public void AddMoteToMaintain(Mote mote)
	{
		maintainedMotes.Add(mote);
	}

	public void AddEffecterToMaintain(TargetInfo target, Effecter eff)
	{
		maintainedEffecters.Add(new Pair<Effecter, TargetInfo>(eff, target));
	}

	public void Cleanup()
	{
		foreach (Pair<Effecter, TargetInfo> maintainedEffecter in maintainedEffecters)
		{
			maintainedEffecter.First.Cleanup();
		}
	}

	public void ExposeData()
	{
		Scribe_Defs.Look(ref def, "def");
		Scribe_Collections.Look(ref comps, "comps", LookMode.Deep);
	}
}
