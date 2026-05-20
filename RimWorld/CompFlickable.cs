using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompFlickable : ThingComp
{
	private bool switchOnInt = true;

	private bool wantSwitchOn = true;

	private Graphic offGraphic;

	private Texture2D cachedCommandTex;

	private const string OffGraphicSuffix = "_Off";

	public const string FlickedOnSignal = "FlickedOn";

	public const string FlickedOffSignal = "FlickedOff";

	private CompProperties_Flickable Props => (CompProperties_Flickable)props;

	private Texture2D CommandTex
	{
		get
		{
			if (cachedCommandTex == null)
			{
				cachedCommandTex = ContentFinder<Texture2D>.Get(Props.commandTexture);
			}
			return cachedCommandTex;
		}
	}

	public bool SwitchIsOn
	{
		get
		{
			return switchOnInt;
		}
		set
		{
			if (switchOnInt != value)
			{
				switchOnInt = value;
				if (switchOnInt)
				{
					parent.BroadcastCompSignal("FlickedOn");
				}
				else
				{
					parent.BroadcastCompSignal("FlickedOff");
				}
				if (parent.Spawned)
				{
					parent.Map.mapDrawer.MapMeshDirty(parent.Position, (ulong)MapMeshFlagDefOf.Buildings | (ulong)MapMeshFlagDefOf.Things);
				}
			}
		}
	}

	public Graphic CurrentGraphic
	{
		get
		{
			if (SwitchIsOn)
			{
				return parent.DefaultGraphic;
			}
			if (offGraphic == null)
			{
				offGraphic = GraphicDatabase.Get(parent.def.graphicData.graphicClass, parent.def.graphicData.texPath + "_Off", parent.def.graphicData.shaderType.Shader, parent.def.graphicData.drawSize, parent.DrawColor, parent.DrawColorTwo);
			}
			return offGraphic;
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref switchOnInt, "switchOn", defaultValue: true);
		Scribe_Values.Look(ref wantSwitchOn, "wantSwitchOn", defaultValue: true);
	}

	public bool WantsFlick()
	{
		return wantSwitchOn != switchOnInt;
	}

	public void DoFlick()
	{
		SwitchIsOn = !SwitchIsOn;
		SoundDefOf.FlickSwitch.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
	}

	public void ResetToOn()
	{
		switchOnInt = true;
		wantSwitchOn = true;
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
		if (parent.Faction == Faction.OfPlayer)
		{
			Command_Toggle command_Toggle = new Command_Toggle();
			command_Toggle.hotKey = KeyBindingDefOf.Command_TogglePower;
			command_Toggle.icon = CommandTex;
			command_Toggle.defaultLabel = Props.commandLabelKey.Translate();
			command_Toggle.defaultDesc = Props.commandDescKey.Translate();
			command_Toggle.isActive = () => wantSwitchOn;
			command_Toggle.toggleAction = delegate
			{
				wantSwitchOn = !wantSwitchOn;
				FlickUtility.UpdateFlickDesignation(parent);
			};
			yield return command_Toggle;
		}
	}
}
