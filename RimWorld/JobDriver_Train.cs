using System.Collections.Generic;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Train : JobDriver_InteractAnimal
	{
		protected override bool CanInteractNow => !TrainableUtility.TrainedTooRecently(base.Animal);

		protected override IEnumerable<Toil> MakeNewToils()
		{
			foreach (Toil item in base.MakeNewToils())
			{
				yield return item;
			}
			this.FailOn(() => base.Animal.training.NextTrainableToTrain() == null && !base.OnLastToil);
		}

		protected override Toil FinalInteractToil()
		{
			return Toils_Interpersonal.TryTrain(TargetIndex.A);
		}
	}
}
