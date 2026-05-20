using System.Collections.Generic;

namespace Verse;

public interface IRenderNodePropertiesParent
{
	bool HasDefinedGraphicProperties { get; }

	List<PawnRenderNodeProperties> RenderNodeProperties { get; }
}
