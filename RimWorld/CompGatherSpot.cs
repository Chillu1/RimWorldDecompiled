using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompGatherSpot : ThingComp
{
	private bool active = true;

	public bool Active
	{
		get
		{
			return active;
		}
		set
		{
			if (value == active)
			{
				return;
			}
			active = value;
			if (parent.Spawned)
			{
				if (active)
				{
					parent.Map.gatherSpotLister.RegisterActivated(this);
				}
				else
				{
					parent.Map.gatherSpotLister.RegisterDeactivated(this);
				}
			}
		}
	}

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref active, "active", defaultValue: false);
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		if (parent.Faction != Faction.OfPlayer && !respawningAfterLoad)
		{
			active = false;
		}
		if (Active)
		{
			parent.Map.gatherSpotLister.RegisterActivated(this);
		}
	}

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		base.PostDeSpawn(map, mode);
		if (Active)
		{
			map.gatherSpotLister.RegisterDeactivated(this);
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		Command_Toggle command_Toggle = new Command_Toggle();
		command_Toggle.hotKey = KeyBindingDefOf.Command_TogglePower;
		command_Toggle.defaultLabel = "CommandGatherSpotToggleLabel".Translate();
		command_Toggle.icon = TexCommand.GatherSpotActive;
		command_Toggle.isActive = () => Active;
		command_Toggle.toggleAction = delegate
		{
			Active = !Active;
		};
		if (Active)
		{
			command_Toggle.defaultDesc = "CommandGatherSpotToggleDescActive".Translate();
		}
		else
		{
			command_Toggle.defaultDesc = "CommandGatherSpotToggleDescInactive".Translate();
		}
		yield return command_Toggle;
	}
}
