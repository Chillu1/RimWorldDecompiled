using System;
using RimWorld.Planet;
using Verse;

namespace LudeonTK;

[AttributeUsage(AttributeTargets.Method)]
public class DebugActionAttribute : Attribute
{
	public string name;

	public string category = "General";

	public AllowedGameStates allowedGameStates = AllowedGameStates.Playing;

	public DebugActionType actionType;

	public bool requiresRoyalty;

	public bool requiresIdeology;

	public bool requiresBiotech;

	public bool requiresAnomaly;

	public bool requiresOdyssey;

	public int displayPriority;

	public bool hideInSubMenu;

	public bool IsAllowedInCurrentGameState
	{
		get
		{
			bool num = (allowedGameStates & AllowedGameStates.Entry) == 0 || Current.ProgramState == ProgramState.Entry;
			bool flag = (allowedGameStates & AllowedGameStates.Playing) == 0 || Current.ProgramState == ProgramState.Playing;
			bool flag2 = (allowedGameStates & AllowedGameStates.WorldRenderedNow) == 0 || WorldRendererUtility.WorldSelected;
			bool flag3 = (allowedGameStates & AllowedGameStates.IsCurrentlyOnMap) == 0 || (!WorldRendererUtility.WorldSelected && Find.CurrentMap != null);
			bool flag4 = (allowedGameStates & AllowedGameStates.HasGameCondition) == 0 || (!WorldRendererUtility.WorldSelected && Find.CurrentMap != null && Find.CurrentMap.gameConditionManager.ActiveConditions.Count > 0);
			bool flag5 = (!requiresRoyalty || ModsConfig.RoyaltyActive) && (!requiresIdeology || ModsConfig.IdeologyActive) && (!requiresBiotech || ModsConfig.BiotechActive) && (!requiresAnomaly || ModsConfig.AnomalyActive) && (!requiresOdyssey || ModsConfig.OdysseyActive);
			return num && flag && flag2 && flag3 && flag4 && flag5;
		}
	}

	public DebugActionAttribute(string category = null, string name = null, bool requiresRoyalty = false, bool requiresIdeology = false, bool requiresBiotech = false, bool requiresAnomaly = false, bool requiresOdyssey = false, int displayPriority = 0, bool hideInSubMenu = false)
	{
		this.name = name;
		this.requiresRoyalty = requiresRoyalty;
		this.requiresIdeology = requiresIdeology;
		this.requiresBiotech = requiresBiotech;
		this.requiresAnomaly = requiresAnomaly;
		this.requiresOdyssey = requiresOdyssey;
		this.displayPriority = displayPriority;
		this.hideInSubMenu = hideInSubMenu;
		if (!string.IsNullOrEmpty(category))
		{
			this.category = category;
		}
	}
}
