using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompToxifier : ThingComp
{
	private IntVec3 nextPollutionCell = IntVec3.Invalid;

	private int pollutingProgressTicks;

	private CompProperties_Toxifier Props => (CompProperties_Toxifier)props;

	public bool CanPolluteNow
	{
		get
		{
			if (!parent.Spawned)
			{
				return false;
			}
			if (!nextPollutionCell.CanPollute(parent.Map))
			{
				UpdateNextPolluteCell();
			}
			return nextPollutionCell.CanPollute(parent.Map);
		}
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		if (!ModLister.CheckBiotech("Toxifier"))
		{
			parent.Destroy();
			return;
		}
		base.PostSpawnSetup(respawningAfterLoad);
		if (!respawningAfterLoad && !parent.BeingTransportedOnGravship)
		{
			pollutingProgressTicks = Props.pollutionIntervalTicks;
		}
	}

	public override string CompInspectStringExtra()
	{
		TaggedString taggedString = base.CompInspectStringExtra();
		if (CanPolluteNow)
		{
			if (!taggedString.NullOrEmpty())
			{
				taggedString += "\n";
			}
			taggedString += string.Concat("PollutingTerrainProgress".Translate() + ": " + (Props.pollutionIntervalTicks - pollutingProgressTicks).ToStringTicksToPeriod() + " (", Props.cellsToPollute.ToString(), " ") + "TilesLower".Translate() + ")";
		}
		return taggedString.Resolve();
	}

	private void UpdateNextPolluteCell()
	{
		if (nextPollutionCell.CanPollute(parent.Map))
		{
			return;
		}
		nextPollutionCell = IntVec3.Invalid;
		int num = GenRadial.NumCellsInRadius(Props.radius);
		for (int i = 0; i < num; i++)
		{
			IntVec3 c = parent.Position + GenRadial.RadialPattern[i];
			if (NextPolluteCellValidator(c))
			{
				nextPollutionCell = c;
				break;
			}
		}
		bool NextPolluteCellValidator(IntVec3 c2)
		{
			if (!c2.InBounds(parent.Map))
			{
				return false;
			}
			if (!c2.CanPollute(parent.Map))
			{
				return false;
			}
			return true;
		}
	}

	private void PolluteNextCell(bool silent = false)
	{
		if (!CanPolluteNow)
		{
			return;
		}
		int num = GenRadial.NumCellsInRadius(Props.radius);
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			IntVec3 c = parent.Position + GenRadial.RadialPattern[i];
			if (c.InBounds(parent.Map) && c.CanPollute(parent.Map))
			{
				c.Pollute(parent.Map, silent);
				num2++;
				if (num2 >= Props.cellsToPollute)
				{
					break;
				}
			}
		}
		if (!silent)
		{
			DoEffects();
		}
	}

	private void DoEffects()
	{
		FleckMaker.Static(parent.TrueCenter(), parent.Map, FleckDefOf.Fleck_ToxifierPollutionSource);
		SoundDefOf.Toxifier_Pollute.PlayOneShot(parent);
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (!DebugSettings.ShowDevGizmos)
		{
			yield break;
		}
		Command_Action command_Action = new Command_Action();
		command_Action.defaultLabel = "DEV: Pollute";
		command_Action.action = delegate
		{
			PolluteNextCell();
		};
		command_Action.Disabled = !CanPolluteNow;
		yield return command_Action;
		Command_Action command_Action2 = new Command_Action();
		command_Action2.defaultLabel = "DEV: Pollute all";
		command_Action2.action = delegate
		{
			while (CanPolluteNow)
			{
				PolluteNextCell(silent: true);
			}
		};
		command_Action2.Disabled = !CanPolluteNow;
		yield return command_Action2;
		yield return new Command_Action
		{
			defaultLabel = "DEV: Set next pollution time",
			action = delegate
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				int[] array = new int[11]
				{
					60, 120, 180, 240, 300, 600, 900, 1200, 1500, 1800,
					3600
				};
				foreach (int ticks in array)
				{
					list.Add(new FloatMenuOption(ticks.ToStringSecondsFromTicks("F0"), delegate
					{
						pollutingProgressTicks = Props.pollutionIntervalTicks - ticks;
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			},
			Disabled = !CanPolluteNow
		};
	}

	public override void CompTickInterval(int delta)
	{
		if (CanPolluteNow)
		{
			pollutingProgressTicks += delta;
			if (pollutingProgressTicks >= Props.pollutionIntervalTicks)
			{
				pollutingProgressTicks = 0;
				PolluteNextCell();
			}
		}
	}

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref nextPollutionCell, "nextPollutionCell", IntVec3.Invalid);
		Scribe_Values.Look(ref pollutingProgressTicks, "pollutingProgressTicks", 0);
	}
}
