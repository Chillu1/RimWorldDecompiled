using System;
using UnityEngine;

namespace Verse;

[StaticConstructorOnStartup]
public abstract class BaseAnimationWorker
{
	static BaseAnimationWorker()
	{
		foreach (Type item in typeof(BaseAnimationWorker).AllSubclassesNonAbstract())
		{
			GenWorker<BaseAnimationWorker>.Get(item);
		}
	}

	public abstract bool Enabled(AnimationDef def, PawnRenderNode node, AnimationPart part, PawnDrawParms parms);

	public abstract void PostDraw(AnimationDef def, PawnRenderNode node, AnimationPart part, PawnDrawParms parms, Matrix4x4 matrix);

	public abstract Vector3 OffsetAtTick(int tick, AnimationDef def, PawnRenderNode node, AnimationPart part, PawnDrawParms parms);

	public abstract float AngleAtTick(int tick, AnimationDef def, PawnRenderNode node, AnimationPart part, PawnDrawParms parms);

	public abstract Vector3 ScaleAtTick(int tick, AnimationDef def, PawnRenderNode node, AnimationPart part, PawnDrawParms parms);

	public abstract GraphicStateDef GraphicStateAtTick(int tick, AnimationDef def, PawnRenderNode node, AnimationPart part, PawnDrawParms parms);
}
