using UnityEngine;

namespace RimWorld.Planet
{
	public class Caravan_Tweener
	{
		private Caravan caravan;

		private Vector3 tweenedPos = Vector3.zero;

		private Vector3 lastTickSpringPos;

		private const float SpringTightness = 0.09f;

		public Vector3 TweenedPos => tweenedPos;

		public Vector3 LastTickTweenedVelocity => TweenedPos - lastTickSpringPos;

		public Vector3 TweenedPosRoot => CaravanTweenerUtility.PatherTweenedPosRoot(caravan) + CaravanTweenerUtility.CaravanCollisionPosOffsetFor(caravan);

		public Caravan_Tweener(Caravan caravan)
		{
			this.caravan = caravan;
		}

		public void TweenerTick()
		{
			lastTickSpringPos = tweenedPos;
			Vector3 a = TweenedPosRoot - tweenedPos;
			tweenedPos += a * 0.09f;
		}

		public void ResetTweenedPosToRoot()
		{
			tweenedPos = TweenedPosRoot;
			lastTickSpringPos = tweenedPos;
		}
	}
}
