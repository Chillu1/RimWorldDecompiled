using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompPowerBattery : CompPower
{
	private float storedEnergy;

	private CompStunnable stunnableComp;

	private const float SelfDischargingWatts = 5f;

	public float AmountCanAccept
	{
		get
		{
			if (parent.IsBrokenDown() || StunnedByEMP)
			{
				return 0f;
			}
			CompProperties_Battery compProperties_Battery = Props;
			return (compProperties_Battery.storedEnergyMax - storedEnergy) / compProperties_Battery.efficiency;
		}
	}

	public float StoredEnergy => storedEnergy;

	public float StoredEnergyPct => storedEnergy / Props.storedEnergyMax;

	public new CompProperties_Battery Props => (CompProperties_Battery)props;

	public bool StunnedByEMP
	{
		get
		{
			if (stunnableComp != null)
			{
				if (stunnableComp.StunHandler.Stunned)
				{
					return stunnableComp.StunHandler.StunFromEMP;
				}
				return false;
			}
			return false;
		}
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		stunnableComp = parent.GetComp<CompStunnable>();
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref storedEnergy, "storedPower", 0f);
		CompProperties_Battery compProperties_Battery = Props;
		if (storedEnergy > compProperties_Battery.storedEnergyMax)
		{
			storedEnergy = compProperties_Battery.storedEnergyMax;
		}
	}

	public override void CompTick()
	{
		base.CompTick();
		DrawPower(Mathf.Min(5f * CompPower.WattsToWattDaysPerTick, storedEnergy));
	}

	public void AddEnergy(float amount)
	{
		if (amount < 0f)
		{
			Log.Error("Cannot add negative energy " + amount);
		}
		else if (!StunnedByEMP)
		{
			if (amount > AmountCanAccept)
			{
				amount = AmountCanAccept;
			}
			amount *= Props.efficiency;
			storedEnergy += amount;
		}
	}

	public void DrawPower(float amount)
	{
		storedEnergy -= amount;
		if (storedEnergy < 0f)
		{
			Log.Error("Drawing power we don't have from " + parent);
			storedEnergy = 0f;
		}
	}

	public void SetStoredEnergyPct(float pct)
	{
		pct = Mathf.Clamp01(pct);
		storedEnergy = Props.storedEnergyMax * pct;
	}

	public override void ReceiveCompSignal(string signal)
	{
		if (signal == "Breakdown")
		{
			DrawPower(StoredEnergy);
		}
	}

	public override string CompInspectStringExtra()
	{
		CompProperties_Battery compProperties_Battery = Props;
		string text = "PowerBatteryStored".Translate() + ": " + storedEnergy.ToString("F0") + " / " + compProperties_Battery.storedEnergyMax.ToString("F0") + " Wd";
		text += "\n" + "PowerBatteryEfficiency".Translate() + ": " + (compProperties_Battery.efficiency * 100f).ToString("F0") + "%";
		if (storedEnergy > 0f)
		{
			text += "\n" + "SelfDischarging".Translate() + ": " + 5f.ToString("F0") + " W";
		}
		return text + "\n" + base.CompInspectStringExtra();
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
		if (DebugSettings.ShowDevGizmos)
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "DEV: Fill";
			command_Action.action = delegate
			{
				SetStoredEnergyPct(1f);
			};
			yield return command_Action;
			Command_Action command_Action2 = new Command_Action();
			command_Action2.defaultLabel = "DEV: Empty";
			command_Action2.action = delegate
			{
				SetStoredEnergyPct(0f);
			};
			yield return command_Action2;
		}
	}
}
