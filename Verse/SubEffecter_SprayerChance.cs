namespace Verse
{
	public class SubEffecter_SprayerChance : SubEffecter_Sprayer
	{
		public SubEffecter_SprayerChance(SubEffecterDef def, Effecter parent)
			: base(def, parent)
		{
		}

		public override void SubEffectTick(TargetInfo A, TargetInfo B)
		{
			float num = def.chancePerTick;
			if (def.spawnLocType == MoteSpawnLocType.RandomCellOnTarget && B.HasThing)
			{
				num *= (float)(B.Thing.def.size.x * B.Thing.def.size.z);
			}
			if (Rand.Value < num)
			{
				MakeMote(A, B);
			}
		}
	}
}
