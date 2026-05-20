using System;
using System.Collections.Generic;
using LudeonTK;

namespace Verse;

public class CurveKeyAnimationPart : AnimationPart
{
	public List<CurveKey> keyframes;

	public bool normalized = true;

	public Vector3Curve offset = new Vector3Curve();

	public ComplexCurve angle = new ComplexCurve();

	public Vector3Curve scale = new Vector3Curve();

	protected override Type DefaultWorker => typeof(AnimationWorker_Curves);
}
