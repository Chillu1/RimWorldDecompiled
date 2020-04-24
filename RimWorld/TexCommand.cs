using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class TexCommand
	{
		public static readonly Texture2D DesirePower = ContentFinder<Texture2D>.Get("UI/Commands/DesirePower");

		public static readonly Texture2D Draft = ContentFinder<Texture2D>.Get("UI/Commands/Draft");

		public static readonly Texture2D ReleaseAnimals = ContentFinder<Texture2D>.Get("UI/Commands/ReleaseAnimals");

		public static readonly Texture2D HoldOpen = ContentFinder<Texture2D>.Get("UI/Commands/HoldOpen");

		public static readonly Texture2D GatherSpotActive = ContentFinder<Texture2D>.Get("UI/Commands/GatherSpotActive");

		public static readonly Texture2D Install = ContentFinder<Texture2D>.Get("UI/Commands/Install");

		public static readonly Texture2D SquadAttack = ContentFinder<Texture2D>.Get("UI/Commands/SquadAttack");

		public static readonly Texture2D AttackMelee = ContentFinder<Texture2D>.Get("UI/Commands/AttackMelee");

		public static readonly Texture2D Attack = ContentFinder<Texture2D>.Get("UI/Commands/Attack");

		public static readonly Texture2D FireAtWill = ContentFinder<Texture2D>.Get("UI/Commands/FireAtWill");

		public static readonly Texture2D ToggleVent = ContentFinder<Texture2D>.Get("UI/Commands/Vent");

		public static readonly Texture2D PauseCaravan = ContentFinder<Texture2D>.Get("UI/Commands/PauseCaravan");

		public static readonly Texture2D ForbidOff = ContentFinder<Texture2D>.Get("UI/Designators/ForbidOff");

		public static readonly Texture2D ForbidOn = ContentFinder<Texture2D>.Get("UI/Designators/ForbidOn");

		public static readonly Texture2D RearmTrap = ContentFinder<Texture2D>.Get("UI/Designators/RearmTrap");

		public static readonly Texture2D CannotShoot = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

		public static readonly Texture2D ClearPrioritizedWork = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

		public static readonly Texture2D RemoveRoutePlannerWaypoint = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

		public static readonly Texture2D OpenLinkedQuestTex = ContentFinder<Texture2D>.Get("UI/Commands/ViewQuest");
	}
}
