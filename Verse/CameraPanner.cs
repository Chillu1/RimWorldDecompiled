using UnityEngine;

namespace Verse
{
	public struct CameraPanner
	{
		public readonly struct Interpolant
		{
			public Vector3 Position { get; }

			public float Size { get; }

			public Interpolant(Vector3 position, float size)
			{
				Position = position;
				Size = size;
			}
		}

		public bool Moving;

		private float TimeSinceStart;

		private Interpolant Source;

		private Interpolant Destination;

		private float Duration;

		private PanCompletionCallback Completion;

		public const float DefaultDuration = 0.25f;

		public void PanTo(Interpolant source, Interpolant destination, float duration = 0.25f, PanCompletionCallback completion = null)
		{
			Moving = true;
			TimeSinceStart = 0f;
			Source = source;
			Destination = destination;
			Duration = duration;
			Completion = completion;
		}

		public void JumpOnNextUpdate()
		{
			TimeSinceStart = Duration;
		}

		public Interpolant? Update()
		{
			if (!Moving)
			{
				return null;
			}
			TimeSinceStart += Time.deltaTime;
			if (TimeSinceStart >= Duration)
			{
				Moving = false;
				Completion?.Invoke();
				Completion = null;
			}
			return GetCurrentInterpolation();
		}

		private Interpolant GetCurrentInterpolation()
		{
			float t = SmootherStep(TimeSinceStart / Duration);
			return new Interpolant(Vector3.LerpUnclamped(Source.Position, Destination.Position, t), Mathf.LerpUnclamped(Source.Size, Destination.Size, t));
		}

		public static float SmootherStep(float t)
		{
			float num = Mathf.Clamp01(t);
			return num * num * num * (num * (num * 6f - 15f) + 10f);
		}
	}
}
