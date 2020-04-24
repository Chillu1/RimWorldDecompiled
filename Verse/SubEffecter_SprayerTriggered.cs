namespace Verse
{
	public class SubEffecter_SprayerTriggered : SubEffecter_Sprayer
	{
		public SubEffecter_SprayerTriggered(SubEffecterDef def, Effecter parent)
			: base(def, parent)
		{
		}

		public override void SubTrigger(TargetInfo A, TargetInfo B)
		{
			MakeMote(A, B);
		}
	}
}
