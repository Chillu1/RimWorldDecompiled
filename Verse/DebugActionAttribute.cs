using RimWorld.Planet;
using System;

namespace Verse
{
	[AttributeUsage(AttributeTargets.Method)]
	public class DebugActionAttribute : Attribute
	{
		public string name;

		public string category = "General";

		public AllowedGameStates allowedGameStates = AllowedGameStates.Playing;

		public DebugActionType actionType;

		public bool IsAllowedInCurrentGameState
		{
			get
			{
				bool num = (allowedGameStates & AllowedGameStates.Entry) == 0 || Current.ProgramState == ProgramState.Entry;
				bool flag = (allowedGameStates & AllowedGameStates.Playing) == 0 || Current.ProgramState == ProgramState.Playing;
				bool flag2 = (allowedGameStates & AllowedGameStates.WorldRenderedNow) == 0 || WorldRendererUtility.WorldRenderedNow;
				bool flag3 = (allowedGameStates & AllowedGameStates.IsCurrentlyOnMap) == 0 || (!WorldRendererUtility.WorldRenderedNow && Find.CurrentMap != null);
				bool flag4 = (allowedGameStates & AllowedGameStates.HasGameCondition) == 0 || (!WorldRendererUtility.WorldRenderedNow && Find.CurrentMap != null && Find.CurrentMap.gameConditionManager.ActiveConditions.Count > 0);
				return num && flag && flag2 && flag3 && flag4;
			}
		}

		public DebugActionAttribute(string category = null, string name = null)
		{
			this.name = name;
			if (!string.IsNullOrEmpty(category))
			{
				this.category = category;
			}
		}
	}
}
