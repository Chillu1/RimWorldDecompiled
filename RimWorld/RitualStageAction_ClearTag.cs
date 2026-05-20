using Verse;

namespace RimWorld
{
	public class RitualStageAction_ClearTag : RitualStageAction
	{
		[NoTranslate]
		public string roleId;

		[NoTranslate]
		public string tag;

		public override void Apply(LordJob_Ritual ritual)
		{
			Pawn key = ritual.PawnWithRole(roleId);
			if (ritual.perPawnTags.TryGetValue(key, out var value))
			{
				value.tags?.Remove(tag);
			}
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref roleId, "roleId");
			Scribe_Values.Look(ref tag, "tag");
		}
	}
}
