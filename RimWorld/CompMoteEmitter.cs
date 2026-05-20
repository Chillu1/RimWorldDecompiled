using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompMoteEmitter : ThingComp
{
	public int ticksSinceLastEmitted;

	protected Mote mote;

	private CompProperties_MoteEmitter Props => (CompProperties_MoteEmitter)props;

	public bool MoteLive
	{
		get
		{
			if (mote != null)
			{
				return !mote.Destroyed;
			}
			return false;
		}
	}

	public override void Initialize(CompProperties props)
	{
		base.Initialize(props);
		if (Props.ticksSinceLastEmittedMaxOffset > 0)
		{
			ticksSinceLastEmitted = Rand.Range(0, Props.ticksSinceLastEmittedMaxOffset);
		}
	}

	public override void CompTick()
	{
		if (!parent.Spawned)
		{
			return;
		}
		CompPowerTrader comp = parent.GetComp<CompPowerTrader>();
		if (comp != null && !comp.PowerOn)
		{
			return;
		}
		CompSendSignalOnCountdown comp2 = parent.GetComp<CompSendSignalOnCountdown>();
		if (comp2 != null && comp2.ticksLeft <= 0)
		{
			return;
		}
		CompInitiatable comp3 = parent.GetComp<CompInitiatable>();
		if (comp3 != null && !comp3.Initiated)
		{
			return;
		}
		Skyfaller skyfaller = parent as Skyfaller;
		if (skyfaller != null && skyfaller.FadingOut)
		{
			return;
		}
		if (Props.emissionInterval != -1 && !Props.maintain)
		{
			if (ticksSinceLastEmitted >= Props.emissionInterval)
			{
				Emit();
				ticksSinceLastEmitted = 0;
			}
			else
			{
				ticksSinceLastEmitted++;
			}
		}
		else if (mote == null || mote.Destroyed)
		{
			Emit();
		}
		if (mote != null && !mote.Destroyed)
		{
			if (typeof(MoteAttached).IsAssignableFrom(Props.mote.thingClass) && skyfaller != null)
			{
				mote.exactRotation = skyfaller.DrawAngle();
			}
			if (Props.maintain)
			{
				Maintain();
			}
		}
	}

	public void Maintain()
	{
		mote.Maintain();
	}

	public virtual void Emit()
	{
		if (!parent.Spawned)
		{
			Log.Error("Thing tried spawning mote without being spawned!");
			return;
		}
		Vector3 vector = Props.offset + Props.RotationOffset(parent.Rotation);
		if (Props.offsetMin != Vector3.zero || Props.offsetMax != Vector3.zero)
		{
			vector = Props.EmissionOffset;
		}
		ThingDef thingDef = Props.RotationMote(parent.Rotation) ?? Props.mote;
		if (typeof(MoteAttached).IsAssignableFrom(thingDef.thingClass))
		{
			mote = MoteMaker.MakeAttachedOverlay(parent, thingDef, vector);
		}
		else
		{
			Vector3 vector2 = parent.DrawPos + vector;
			if (vector2.InBounds(parent.Map))
			{
				mote = MoteMaker.MakeStaticMote(vector2, parent.Map, thingDef);
			}
		}
		if (mote != null && Props.useParentRotation)
		{
			mote.exactRotation = parent.Rotation.AsAngle;
		}
		if (!Props.soundOnEmission.NullOrUndefined())
		{
			Props.soundOnEmission.PlayOneShot(SoundInfo.InMap(parent));
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref ticksSinceLastEmitted, ((Props.saveKeysPrefix != null) ? (Props.saveKeysPrefix + "_") : "") + "ticksSinceLastEmitted", 0);
	}
}
