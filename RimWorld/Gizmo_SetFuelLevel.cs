using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;

namespace RimWorld;

public class Gizmo_SetFuelLevel : Gizmo_Slider
{
	private CompRefuelable refuelable;

	private static bool draggingBar;

	protected override float Target
	{
		get
		{
			return refuelable.TargetFuelLevel / refuelable.Props.fuelCapacity;
		}
		set
		{
			refuelable.TargetFuelLevel = value * refuelable.Props.fuelCapacity;
		}
	}

	protected override float ValuePercent => refuelable.FuelPercentOfMax;

	protected override string Title => refuelable.Props.FuelGizmoLabel;

	protected override bool IsDraggable => refuelable.Props.targetFuelLevelConfigurable;

	protected override string BarLabel => refuelable.Fuel.ToStringDecimalIfSmall() + " / " + refuelable.Props.fuelCapacity.ToStringDecimalIfSmall();

	protected override bool DraggingBar
	{
		get
		{
			return draggingBar;
		}
		set
		{
			draggingBar = value;
		}
	}

	public Gizmo_SetFuelLevel(CompRefuelable refuelable)
	{
		this.refuelable = refuelable;
	}

	public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
	{
		if (!refuelable.Props.showAllowAutoRefuelToggle)
		{
			return base.GizmoOnGUI(topLeft, maxWidth, parms);
		}
		if (SteamDeck.IsSteamDeckInNonKeyboardMode)
		{
			return base.GizmoOnGUI(topLeft, maxWidth, parms);
		}
		KeyCode keyCode = ((KeyBindingDefOf.Command_ItemForbid != null) ? KeyBindingDefOf.Command_ItemForbid.MainKey : KeyCode.None);
		if (keyCode != KeyCode.None && !GizmoGridDrawer.drawnHotKeys.Contains(keyCode) && KeyBindingDefOf.Command_ItemForbid.KeyDownEvent)
		{
			ToggleAutoRefuel();
			Event.current.Use();
		}
		return base.GizmoOnGUI(topLeft, maxWidth, parms);
	}

	protected override void DrawHeader(Rect headerRect, ref bool mouseOverElement)
	{
		if (refuelable.Props.showAllowAutoRefuelToggle)
		{
			headerRect.xMax -= 24f;
			Rect rect = new Rect(headerRect.xMax, headerRect.y, 24f, 24f);
			GUI.DrawTexture(rect, refuelable.Props.FuelIcon);
			GUI.DrawTexture(new Rect(rect.center.x, rect.y, rect.width / 2f, rect.height / 2f), refuelable.allowAutoRefuel ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex);
			if (Widgets.ButtonInvisible(rect))
			{
				ToggleAutoRefuel();
			}
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
				TooltipHandler.TipRegion(rect, RefuelTip, 828267373);
				mouseOverElement = true;
			}
		}
		base.DrawHeader(headerRect, ref mouseOverElement);
	}

	private void ToggleAutoRefuel()
	{
		refuelable.allowAutoRefuel = !refuelable.allowAutoRefuel;
		if (refuelable.allowAutoRefuel)
		{
			SoundDefOf.Tick_High.PlayOneShotOnCamera();
		}
		else
		{
			SoundDefOf.Tick_Low.PlayOneShotOnCamera();
		}
	}

	private string RefuelTip()
	{
		string text = string.Format("{0}", "CommandToggleAllowAutoRefuel".Translate()) + "\n\n";
		string str = (refuelable.allowAutoRefuel ? "On".Translate() : "Off".Translate());
		string text2 = refuelable.TargetFuelLevel.ToString("F0").Colorize(ColoredText.TipSectionTitleColor);
		string text3 = string.Concat(text + "CommandToggleAllowAutoRefuelDesc".Translate(text2, str.UncapitalizeFirst().Named("ONOFF")).Resolve(), "\n\n");
		string text4 = KeyPrefs.KeyPrefsData.GetBoundKeyCode(KeyBindingDefOf.Command_ItemForbid, KeyPrefs.BindingSlot.A).ToStringReadable();
		return text3 + ("HotKeyTip".Translate() + ": " + text4);
	}

	protected override string GetTooltip()
	{
		return "";
	}
}
