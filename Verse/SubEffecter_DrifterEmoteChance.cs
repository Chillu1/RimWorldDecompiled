namespace Verse
{
	public class SubEffecter_DrifterEmoteChance : SubEffecter_DrifterEmote
	{
		public SubEffecter_DrifterEmoteChance(SubEffecterDef def, Effecter parent)
			: base(def, parent)
		{
		}

		public override void SubEffectTick(TargetInfo A, TargetInfo B)
		{
			float chancePerTick = def.chancePerTick;
			if (Rand.Value < chancePerTick)
			{
				MakeMote(A);
			}
		}
	}
}
