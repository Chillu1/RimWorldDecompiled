using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

public class PowerNet
{
	public PowerNetManager powerNetManager;

	public bool hasPowerSource;

	public List<CompPower> connectors = new List<CompPower>();

	public List<CompPower> transmitters = new List<CompPower>();

	public List<CompPowerTrader> powerComps = new List<CompPowerTrader>();

	public List<CompPowerBattery> batteryComps = new List<CompPowerBattery>();

	private float debugLastCreatedEnergy;

	private float debugLastRawStoredEnergy;

	private float debugLastApparentStoredEnergy;

	private const int MaxRestartTryInterval = 200;

	private const int MinRestartTryInterval = 30;

	private const float RestartMinFraction = 0.05f;

	private const int ShutdownInterval = 20;

	private const float ShutdownMinFraction = 0.05f;

	private const float MinStoredEnergyToTurnOn = 5f;

	private static List<CompPowerTrader> partsWantingPowerOn = new List<CompPowerTrader>();

	private static List<CompPowerTrader> potentialShutdownParts = new List<CompPowerTrader>();

	private List<CompPowerBattery> givingBats = new List<CompPowerBattery>();

	private static List<CompPowerBattery> batteriesShuffled = new List<CompPowerBattery>();

	public Map Map => powerNetManager.map;

	public bool HasActivePowerSource
	{
		get
		{
			if (!hasPowerSource)
			{
				return false;
			}
			for (int i = 0; i < transmitters.Count; i++)
			{
				if (IsActivePowerSource(transmitters[i]))
				{
					return true;
				}
			}
			return false;
		}
	}

	public PowerNet(IEnumerable<CompPower> newTransmitters)
	{
		foreach (CompPower newTransmitter in newTransmitters)
		{
			transmitters.Add(newTransmitter);
			newTransmitter.transNet = this;
			RegisterAllComponentsOf(newTransmitter.parent);
			if (newTransmitter.connectChildren != null)
			{
				List<CompPower> connectChildren = newTransmitter.connectChildren;
				for (int i = 0; i < connectChildren.Count; i++)
				{
					RegisterConnector(connectChildren[i]);
				}
			}
		}
		hasPowerSource = false;
		for (int j = 0; j < transmitters.Count; j++)
		{
			if (IsPowerSource(transmitters[j]))
			{
				hasPowerSource = true;
				break;
			}
		}
	}

	private bool IsPowerSource(CompPower cp)
	{
		if (cp is CompPowerBattery)
		{
			return true;
		}
		if (cp is CompPowerTrader && cp.Props.PowerConsumption < 0f)
		{
			return true;
		}
		return false;
	}

	private bool IsActivePowerSource(CompPower cp)
	{
		if (cp is CompPowerBattery { StoredEnergy: >0f, StunnedByEMP: false })
		{
			return true;
		}
		if (cp is CompPowerTrader { PowerOutput: >0f })
		{
			return true;
		}
		return false;
	}

	public void RegisterConnector(CompPower b)
	{
		if (connectors.Contains(b))
		{
			Log.Error("PowerNet registered connector it already had: " + b);
			return;
		}
		connectors.Add(b);
		RegisterAllComponentsOf(b.parent);
	}

	public void DeregisterConnector(CompPower b)
	{
		connectors.Remove(b);
		DeregisterAllComponentsOf(b.parent);
	}

	private void RegisterAllComponentsOf(ThingWithComps parentThing)
	{
		CompPowerTrader comp = parentThing.GetComp<CompPowerTrader>();
		if (comp != null)
		{
			if (powerComps.Contains(comp))
			{
				Log.Error("PowerNet adding powerComp " + comp?.ToString() + " which it already has.");
			}
			else
			{
				powerComps.Add(comp);
			}
		}
		CompPowerBattery comp2 = parentThing.GetComp<CompPowerBattery>();
		if (comp2 != null)
		{
			if (batteryComps.Contains(comp2))
			{
				Log.Error("PowerNet adding batteryComp " + comp2?.ToString() + " which it already has.");
			}
			else
			{
				batteryComps.Add(comp2);
			}
		}
	}

	private void DeregisterAllComponentsOf(ThingWithComps parentThing)
	{
		CompPowerTrader comp = parentThing.GetComp<CompPowerTrader>();
		if (comp != null)
		{
			powerComps.Remove(comp);
		}
		CompPowerBattery comp2 = parentThing.GetComp<CompPowerBattery>();
		if (comp2 != null)
		{
			batteryComps.Remove(comp2);
		}
	}

	public float CurrentEnergyGainRate()
	{
		if (DebugSettings.unlimitedPower)
		{
			return 100000f;
		}
		float num = 0f;
		for (int i = 0; i < powerComps.Count; i++)
		{
			if (powerComps[i].PowerOn)
			{
				num += powerComps[i].EnergyOutputPerTick;
			}
		}
		return num;
	}

	public float CurrentStoredEnergy()
	{
		float num = 0f;
		for (int i = 0; i < batteryComps.Count; i++)
		{
			num += (batteryComps[i].StunnedByEMP ? 0f : batteryComps[i].StoredEnergy);
		}
		return num;
	}

	public void PowerNetTick()
	{
		float num = CurrentEnergyGainRate();
		float num2 = CurrentStoredEnergy();
		if (num2 + num >= -1E-07f && !Map.gameConditionManager.ElectricityDisabled(Map))
		{
			float num3 = ((batteryComps.Count <= 0 || !(num2 >= 0.1f)) ? num2 : (num2 - 5f));
			if (num3 + num >= 0f)
			{
				partsWantingPowerOn.Clear();
				for (int i = 0; i < powerComps.Count; i++)
				{
					if (!powerComps[i].PowerOn && FlickUtility.WantsToBeOn(powerComps[i].parent) && !powerComps[i].parent.IsBrokenDown())
					{
						partsWantingPowerOn.Add(powerComps[i]);
					}
				}
				if (partsWantingPowerOn.Count > 0)
				{
					int num4 = 200 / partsWantingPowerOn.Count;
					if (num4 < 30)
					{
						num4 = 30;
					}
					if (Find.TickManager.TicksGame % num4 == 0)
					{
						int num5 = Mathf.Max(1, Mathf.RoundToInt((float)partsWantingPowerOn.Count * 0.05f));
						for (int j = 0; j < num5; j++)
						{
							CompPowerTrader compPowerTrader = partsWantingPowerOn.RandomElement();
							if (!compPowerTrader.PowerOn && num + num2 >= 0f - (compPowerTrader.EnergyOutputPerTick + 1E-07f))
							{
								compPowerTrader.PowerOn = true;
								num += compPowerTrader.EnergyOutputPerTick;
							}
						}
					}
				}
			}
			ChangeStoredEnergy(num);
		}
		else
		{
			if (Find.TickManager.TicksGame % 20 != 0)
			{
				return;
			}
			potentialShutdownParts.Clear();
			for (int k = 0; k < powerComps.Count; k++)
			{
				if (powerComps[k].PowerOn && powerComps[k].EnergyOutputPerTick < 0f)
				{
					potentialShutdownParts.Add(powerComps[k]);
				}
			}
			if (potentialShutdownParts.Count > 0)
			{
				int num6 = Mathf.Max(1, Mathf.RoundToInt((float)potentialShutdownParts.Count * 0.05f));
				for (int l = 0; l < num6; l++)
				{
					potentialShutdownParts.RandomElement().PowerOn = false;
				}
			}
		}
	}

	public bool CanPowerNow(CompPowerTrader powerTrader)
	{
		float num = CurrentEnergyGainRate();
		float num2 = CurrentStoredEnergy();
		return num + num2 >= 0f - (powerTrader.EnergyOutputPerTick + 1E-07f);
	}

	private void ChangeStoredEnergy(float extra)
	{
		if (extra > 0f)
		{
			DistributeEnergyAmongBatteries(extra);
			return;
		}
		float num = 0f - extra;
		givingBats.Clear();
		for (int i = 0; i < batteryComps.Count; i++)
		{
			if (batteryComps[i].StoredEnergy > 1E-07f)
			{
				givingBats.Add(batteryComps[i]);
			}
		}
		float a = num / (float)givingBats.Count;
		int num2 = 0;
		while (num > 1E-07f)
		{
			for (int j = 0; j < givingBats.Count; j++)
			{
				float num3 = Mathf.Min(a, givingBats[j].StoredEnergy);
				givingBats[j].DrawPower(num3);
				num -= num3;
				if (num < 1E-07f)
				{
					return;
				}
			}
			num2++;
			if (num2 > 10)
			{
				break;
			}
		}
		if (num > 1E-07f)
		{
			Log.Warning("Drew energy from a PowerNet that didn't have it.");
		}
	}

	private void DistributeEnergyAmongBatteries(float energy)
	{
		if (energy <= 0f || !batteryComps.Any())
		{
			return;
		}
		batteriesShuffled.Clear();
		batteriesShuffled.AddRange(batteryComps);
		batteriesShuffled.Shuffle();
		int num = 0;
		do
		{
			num++;
			if (num > 10000)
			{
				Log.Error("Too many iterations.");
				break;
			}
			float num2 = float.MaxValue;
			for (int i = 0; i < batteriesShuffled.Count; i++)
			{
				num2 = Mathf.Min(num2, batteriesShuffled[i].AmountCanAccept);
			}
			if (energy >= num2 * (float)batteriesShuffled.Count)
			{
				for (int num3 = batteriesShuffled.Count - 1; num3 >= 0; num3--)
				{
					float amountCanAccept = batteriesShuffled[num3].AmountCanAccept;
					bool num4 = amountCanAccept <= 0f || amountCanAccept == num2;
					if (num2 > 0f)
					{
						batteriesShuffled[num3].AddEnergy(num2);
						energy -= num2;
					}
					if (num4)
					{
						batteriesShuffled.RemoveAt(num3);
					}
				}
				continue;
			}
			float amount = energy / (float)batteriesShuffled.Count;
			for (int j = 0; j < batteriesShuffled.Count; j++)
			{
				batteriesShuffled[j].AddEnergy(amount);
			}
			energy = 0f;
			break;
		}
		while (!(energy < 0.0005f) && batteriesShuffled.Any());
		batteriesShuffled.Clear();
	}

	public string DebugString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("POWERNET:");
		stringBuilder.AppendLine("  Created energy: " + debugLastCreatedEnergy);
		stringBuilder.AppendLine("  Raw stored energy: " + debugLastRawStoredEnergy);
		stringBuilder.AppendLine("  Apparent stored energy: " + debugLastApparentStoredEnergy);
		stringBuilder.AppendLine("  hasPowerSource: " + hasPowerSource);
		stringBuilder.AppendLine("  Connectors: ");
		foreach (CompPower connector in connectors)
		{
			stringBuilder.AppendLine("      " + connector.parent);
		}
		stringBuilder.AppendLine("  Transmitters: ");
		foreach (CompPower transmitter in transmitters)
		{
			stringBuilder.AppendLine("      " + transmitter.parent);
		}
		stringBuilder.AppendLine("  powerComps: ");
		foreach (CompPowerTrader powerComp in powerComps)
		{
			stringBuilder.AppendLine("      " + powerComp.parent);
		}
		stringBuilder.AppendLine("  batteryComps: ");
		foreach (CompPowerBattery batteryComp in batteryComps)
		{
			stringBuilder.AppendLine("      " + batteryComp.parent);
		}
		return stringBuilder.ToString();
	}
}
