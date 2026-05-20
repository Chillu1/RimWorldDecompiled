using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse;

public class CompGlower : ThingComp
{
	private enum GlowerColorChangeType
	{
		None,
		ColorPickerEnabled,
		DarklightToggle,
		AllGlowers
	}

	public static Widgets.ColorComponents visibleColorTextfields = Widgets.ColorComponents.Hue | Widgets.ColorComponents.Sat;

	public static Widgets.ColorComponents editableColorTextfields = Widgets.ColorComponents.Hue | Widgets.ColorComponents.Sat;

	public static Widgets.ColorComponents visibleDebugColorTextfields = Widgets.ColorComponents.All;

	public static Widgets.ColorComponents editableDebugColorTextfields = Widgets.ColorComponents.Red | Widgets.ColorComponents.Green | Widgets.ColorComponents.Blue | Widgets.ColorComponents.Hue | Widgets.ColorComponents.Sat;

	public static ColorInt? ColorClipboard;

	private bool glowOnInt;

	private ColorInt? glowColorOverride;

	private float? glowRadiusOverride;

	private static List<CompGlower> tmpExtraGlowers = new List<CompGlower>(64);

	public CompProperties_Glower Props => (CompProperties_Glower)props;

	public virtual ColorInt GlowColor
	{
		get
		{
			return glowColorOverride ?? Props.glowColor;
		}
		set
		{
			SetGlowColorInternal(value);
		}
	}

	public virtual float GlowRadius
	{
		get
		{
			return glowRadiusOverride ?? Props.glowRadius;
		}
		set
		{
			glowRadiusOverride = value;
		}
	}

	protected virtual bool ShouldBeLitNow
	{
		get
		{
			if (!parent.Spawned)
			{
				return false;
			}
			if (!FlickUtility.WantsToBeOn(parent))
			{
				return false;
			}
			if (parent is IThingGlower thingGlower && !thingGlower.ShouldBeLitNow())
			{
				return false;
			}
			ThingWithComps thingWithComps = parent;
			if (thingWithComps != null)
			{
				foreach (ThingComp allComp in thingWithComps.AllComps)
				{
					if (allComp is IThingGlower thingGlower2 && !thingGlower2.ShouldBeLitNow())
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public bool Glows => glowOnInt;

	public bool HasGlowColorOverride => glowColorOverride.HasValue;

	public void UpdateLit(Map map)
	{
		bool shouldBeLitNow = ShouldBeLitNow;
		if (glowOnInt != shouldBeLitNow)
		{
			glowOnInt = shouldBeLitNow;
			if (!glowOnInt)
			{
				map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlagDefOf.Things);
				map.glowGrid.DeRegisterGlower(this);
			}
			else
			{
				map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlagDefOf.Things);
				map.glowGrid.RegisterGlower(this);
			}
		}
	}

	public void ForceRegister(Map map)
	{
		map.glowGrid.DeRegisterGlower(this);
		map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlagDefOf.Things);
		map.glowGrid.RegisterGlower(this);
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		if (!parent.BeingTransportedOnGravship)
		{
			RefreshGlower();
		}
	}

	public override void PostSwapMap()
	{
		RefreshGlower();
	}

	public override void PostMapInit()
	{
		RefreshGlower();
	}

	private void RefreshGlower()
	{
		UpdateLit(parent.Map);
		if (ShouldBeLitNow)
		{
			parent.Map.glowGrid.RegisterGlower(this);
		}
	}

	public override void ReceiveCompSignal(string signal)
	{
		switch (signal)
		{
		case "PowerTurnedOn":
		case "PowerTurnedOff":
		case "FlickedOn":
		case "FlickedOff":
		case "Refueled":
		case "RanOutOfFuel":
		case "ScheduledOn":
		case "ScheduledOff":
		case "MechClusterDefeated":
		case "Hacked":
		case "RitualTargetChanged":
		case "CrateContentsChanged":
		case "Vacuum":
			UpdateLit(parent.Map);
			break;
		}
	}

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref glowOnInt, "glowOn", defaultValue: false);
		Scribe_Values.Look(ref glowColorOverride, "glowColorOverride");
	}

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		base.PostDeSpawn(map, mode);
		UpdateLit(map);
	}

	private List<CompGlower> ExtraSelectedGlowers(GlowerColorChangeType type)
	{
		tmpExtraGlowers.Clear();
		foreach (object item in Find.Selector.SelectedObjectsListForReading)
		{
			if (item == this || !(item is ThingWithComps thingWithComps))
			{
				continue;
			}
			foreach (CompGlower comp in thingWithComps.GetComps<CompGlower>())
			{
				if (type == GlowerColorChangeType.AllGlowers || (type == GlowerColorChangeType.ColorPickerEnabled && comp.Props.colorPickerEnabled) || (type == GlowerColorChangeType.DarklightToggle && comp.Props.darklightToggle))
				{
					tmpExtraGlowers.Add(comp);
				}
			}
		}
		return tmpExtraGlowers;
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
		if (parent.Faction != Faction.OfPlayer)
		{
			yield break;
		}
		bool doCopyPasteGizmos = false;
		GlowerColorChangeType type = GlowerColorChangeType.None;
		if (Props.colorPickerEnabled)
		{
			type = GlowerColorChangeType.ColorPickerEnabled;
		}
		else if (DebugSettings.editableGlowerColors)
		{
			type = GlowerColorChangeType.AllGlowers;
		}
		List<CompGlower> extraGlowers = ExtraSelectedGlowers(type);
		Color32 value = GlowColor.ProjectToColor32();
		value.a = byte.MaxValue;
		Color32? color = value;
		foreach (CompGlower item2 in extraGlowers)
		{
			if (item2.GlowColor != GlowColor)
			{
				color = null;
			}
		}
		Command_ColorIcon command_ColorIcon = new Command_ColorIcon
		{
			defaultLabel = "GlowerChangeColor".Translate(),
			defaultDesc = "GlowerChangeColorDescription".Translate(),
			icon = ContentFinder<Texture2D>.Get("UI/Commands/ChangeColor"),
			color = color,
			action = delegate
			{
				Widgets.ColorComponents visibleTextfields = (DebugSettings.editableGlowerColors ? visibleDebugColorTextfields : visibleColorTextfields);
				Widgets.ColorComponents editableTextfields = (DebugSettings.editableGlowerColors ? editableDebugColorTextfields : editableColorTextfields);
				Dialog_GlowerColorPicker window = new Dialog_GlowerColorPicker(this, extraGlowers, visibleTextfields, editableTextfields);
				Find.WindowStack.Add(window);
			}
		};
		if (Props.colorPickerEnabled)
		{
			bool flag = DebugSettings.editableGlowerColors || ResearchProjectDefOf.ColoredLights.IsFinished;
			doCopyPasteGizmos = flag;
			command_ColorIcon.Disabled = !flag;
			command_ColorIcon.disabledReason = "GlowerChangeColorNeedsResearch".Translate(ResearchProjectDefOf.ColoredLights.label);
			yield return command_ColorIcon;
		}
		else if (DebugSettings.editableGlowerColors)
		{
			doCopyPasteGizmos = true;
			yield return command_ColorIcon;
		}
		if (doCopyPasteGizmos)
		{
			Command_ColorIcon command_ColorIcon2 = new Command_ColorIcon();
			command_ColorIcon2.icon = ContentFinder<Texture2D>.Get("UI/Commands/CopyColor");
			command_ColorIcon2.defaultLabel = "CommandCopyColorLabel".Translate();
			command_ColorIcon2.defaultDesc = "CommandCopyColorDesc".Translate();
			Color32 value2 = GlowColor.ProjectToColor32();
			value2.a = byte.MaxValue;
			command_ColorIcon2.color = value2;
			command_ColorIcon2.action = delegate
			{
				ColorClipboard = GlowColor;
				Messages.Message("ColorCopiedSuccessfully".Translate(), MessageTypeDefOf.PositiveEvent, historical: false);
			};
			command_ColorIcon2.hotKey = KeyBindingDefOf.Misc4;
			yield return command_ColorIcon2;
			bool flag2 = true;
			float hue = 0f;
			float sat = 0f;
			if (ColorClipboard.HasValue)
			{
				Color.RGBToHSV(ColorClipboard.Value.ProjectToColor32(), out hue, out sat, out var _);
				flag2 = false;
			}
			Command_ColorIcon command_ColorIcon3 = new Command_ColorIcon();
			command_ColorIcon3.icon = ContentFinder<Texture2D>.Get("UI/Commands/PasteColor");
			command_ColorIcon3.defaultLabel = "CommandPasteColorLabel".Translate();
			command_ColorIcon3.defaultDesc = "CommandPasteColorDesc".Translate();
			if (!flag2)
			{
				command_ColorIcon3.color = Color.HSVToRGB(hue, sat, 1f);
			}
			command_ColorIcon3.Disabled = flag2;
			command_ColorIcon3.disabledReason = "ClipboardInvalidColor".Translate();
			command_ColorIcon3.action = delegate
			{
				try
				{
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
					ColorInt glowColor = GlowColor;
					glowColor.SetHueSaturation(hue, sat);
					GlowColor = glowColor;
					foreach (CompGlower item3 in extraGlowers)
					{
						glowColor = item3.GlowColor;
						glowColor.SetHueSaturation(hue, sat);
						item3.GlowColor = glowColor;
					}
					Messages.Message("ColorPastedSuccessfully".Translate(), MessageTypeDefOf.PositiveEvent, historical: false);
				}
				catch (Exception)
				{
					Messages.Message("ClipboardInvalidColor".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				}
			};
			command_ColorIcon3.hotKey = KeyBindingDefOf.Misc5;
			yield return command_ColorIcon3;
		}
		if (!ModsConfig.IdeologyActive || !Props.darklightToggle)
		{
			yield break;
		}
		bool darklight = DarklightUtility.IsDarklight(GlowColor.ToColor);
		yield return new Command_Action
		{
			icon = ContentFinder<Texture2D>.Get(darklight ? "UI/Commands/SetNormalLight" : "UI/Commands/SetDarklight"),
			defaultLabel = (darklight ? "ToggleDarklightOff" : "ToggleDarklightOn").Translate(),
			defaultDesc = (darklight ? "ToggleDarklightOffDesc" : "ToggleDarklightOnDesc").Translate(),
			action = delegate
			{
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
				if (darklight)
				{
					SetGlowColorInternal(null);
				}
				else
				{
					GlowColor = new ColorInt(DarklightUtility.DefaultDarklight);
				}
				foreach (CompGlower item4 in ExtraSelectedGlowers(GlowerColorChangeType.DarklightToggle))
				{
					if (darklight)
					{
						item4.SetGlowColorInternal(null);
					}
					else
					{
						item4.GlowColor = new ColorInt(DarklightUtility.DefaultDarklight);
					}
				}
			}
		};
	}

	protected virtual void SetGlowColorInternal(ColorInt? color)
	{
		if (ShouldBeLitNow)
		{
			parent.MapHeld.glowGrid.DeRegisterGlower(this);
		}
		glowColorOverride = color;
		if (ShouldBeLitNow)
		{
			parent.MapHeld.glowGrid.RegisterGlower(this);
		}
	}
}
