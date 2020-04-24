using System.Collections.Generic;

namespace Verse
{
	public interface ISelectable
	{
		IEnumerable<Gizmo> GetGizmos();

		string GetInspectString();

		IEnumerable<InspectTabBase> GetInspectTabs();
	}
}
