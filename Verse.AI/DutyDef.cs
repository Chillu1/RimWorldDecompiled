using RimWorld;

namespace Verse.AI
{
	public class DutyDef : Def
	{
		public ThinkNode thinkNode;

		public ThinkNode constantThinkNode;

		public bool alwaysShowWeapon;

		public ThinkTreeDutyHook hook = ThinkTreeDutyHook.HighPriority;

		public RandomSocialMode socialModeMax = RandomSocialMode.SuperActive;

		public bool threatDisabled;
	}
}
