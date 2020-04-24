namespace Verse
{
	public class SubEffecter_DrifterEmoteTriggered : SubEffecter_DrifterEmote
	{
		public SubEffecter_DrifterEmoteTriggered(SubEffecterDef def, Effecter parent)
			: base(def, parent)
		{
		}

		public override void SubTrigger(TargetInfo A, TargetInfo B)
		{
			MakeMote(A);
		}
	}
}
