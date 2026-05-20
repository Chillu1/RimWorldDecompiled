using System;
using UnityEngine;

namespace Verse;

public abstract class AnimationPart
{
	public Vector2 pivot = new Vector2(0.5f, 0.5f);

	public RotationMode rotationMode;

	public Type workerType;

	protected abstract Type DefaultWorker { get; }

	public Type WorkerType => workerType ?? DefaultWorker;
}
