using System.Collections.Generic;

namespace Verse;

public abstract class DrawStyle
{
	public virtual bool CanHaveDuplicates => true;

	public virtual bool SingleCell => false;

	public abstract void Update(IntVec3 origin, IntVec3 target, List<IntVec3> buffer);

	public virtual void Draw()
	{
	}
}
