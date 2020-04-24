using Verse.Sound;

namespace Verse.AI
{
	public static class Toils_Effects
	{
		public static Toil MakeSound(SoundDef soundDef)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				soundDef.PlayOneShot(new TargetInfo(actor.Position, actor.Map));
			};
			return toil;
		}
	}
}
