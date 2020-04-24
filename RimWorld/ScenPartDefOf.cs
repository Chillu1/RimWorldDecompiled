namespace RimWorld
{
	[DefOf]
	public static class ScenPartDefOf
	{
		public static ScenPartDef PlayerFaction;

		public static ScenPartDef ConfigPage_ConfigureStartingPawns;

		public static ScenPartDef PlayerPawnsArriveMethod;

		public static ScenPartDef ForcedTrait;

		public static ScenPartDef ForcedHediff;

		public static ScenPartDef StartingAnimal;

		public static ScenPartDef ScatterThingsNearPlayerStart;

		public static ScenPartDef StartingThing_Defined;

		public static ScenPartDef ScatterThingsAnywhere;

		public static ScenPartDef GameStartDialog;

		static ScenPartDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ScenPartDefOf));
		}
	}
}
