using System;
using System.Collections.Generic;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Train : JobDriver_InteractAnimal
	{
		protected override bool CanInteractNow => !TrainableUtility.TrainedTooRecently(base.Animal);

		protected override IEnumerable<Toil> MakeNewToils()
		{
			Func<bool> noLongerTrainable = () => base.Animal.training.NextTrainableToTrain() == null;
			foreach (Toil item in base.MakeNewToils())
			{
				item.FailOn(noLongerTrainable);
				yield return item;
			}
			yield return Toils_Interpersonal.TryTrain(TargetIndex.A);
		}
	}
}
