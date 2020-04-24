namespace RimWorld
{
	[DefOf]
	public static class InstructionDefOf
	{
		public static InstructionDef RandomizeCharacter;

		public static InstructionDef ChooseLandingSite;

		static InstructionDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(InstructionDefOf));
		}
	}
}
