using UnityEngine;
using Verse;

namespace RimWorld;

public class CompStunnable : ThingComp
{
	private StunHandler stunHandler;

	private CompProperties_Stunnable Props => (CompProperties_Stunnable)props;

	public StunHandler StunHandler => stunHandler;

	public bool UseLargeEMPEffecter => Props.useLargeEMPEffecter;

	public Vector3? EMPEffecterDimensions => Props.empEffecterDimensions;

	public Vector3? EMPEffecterOffset => Props.empEffecterOffset;

	public float? EMPChancePerTick => Props.empChancePerTick;

	public override void Initialize(CompProperties props)
	{
		base.Initialize(props);
		stunHandler = new StunHandler(parent);
	}

	public override void CompTick()
	{
		stunHandler.StunHandlerTick();
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Deep.Look(ref stunHandler, "stunHandler", parent);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && stunHandler == null)
		{
			stunHandler = new StunHandler(parent);
		}
	}

	public override string CompInspectStringExtra()
	{
		if (!stunHandler.Stunned)
		{
			return null;
		}
		if (stunHandler.Hypnotized)
		{
			return "InTrance".Translate();
		}
		if (stunHandler.StunFromEMP)
		{
			return "StunnedByEMP".Translate().CapitalizeFirst() + ": " + stunHandler.StunTicksLeft.ToStringSecondsFromTicks();
		}
		return "StunLower".Translate().CapitalizeFirst() + ": " + stunHandler.StunTicksLeft.ToStringSecondsFromTicks();
	}

	public void ApplyDamage(DamageInfo damageInfo)
	{
		stunHandler.Notify_DamageApplied(damageInfo);
	}

	public bool CanBeStunnedByDamage(DamageDef def)
	{
		return Props.affectedDamageDefs.Contains(def);
	}

	public bool CanAdaptToDamage(DamageDef def)
	{
		return Props.adaptableDamageDefs.Contains(def);
	}
}
