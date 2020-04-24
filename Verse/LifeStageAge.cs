using RimWorld;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public class LifeStageAge
	{
		public LifeStageDef def;

		public float minAge;

		public SoundDef soundCall;

		public SoundDef soundAngry;

		public SoundDef soundWounded;

		public SoundDef soundDeath;

		private static readonly Texture2D VeryYoungIcon = ContentFinder<Texture2D>.Get("UI/Icons/LifeStage/VeryYoung");

		private static readonly Texture2D YoungIcon = ContentFinder<Texture2D>.Get("UI/Icons/LifeStage/Young");

		private static readonly Texture2D AdultIcon = ContentFinder<Texture2D>.Get("UI/Icons/LifeStage/Adult");

		public Texture2D GetIcon(Pawn forPawn)
		{
			if (def.iconTex != null)
			{
				return def.iconTex;
			}
			int count = forPawn.RaceProps.lifeStageAges.Count;
			int num = forPawn.RaceProps.lifeStageAges.IndexOf(this);
			if (num == count - 1)
			{
				return AdultIcon;
			}
			if (num == 0)
			{
				return VeryYoungIcon;
			}
			return YoungIcon;
		}
	}
}
