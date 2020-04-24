namespace Verse.AI
{
	internal class Toils_Interact
	{
		public static Toil DestroyThing(TargetIndex ind)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Thing thing = toil.actor.jobs.curJob.GetTarget(ind).Thing;
				if (!thing.Destroyed)
				{
					thing.Destroy();
				}
			};
			return toil;
		}
	}
}
