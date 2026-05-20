using UnityEngine;

namespace RimWorld;

public interface IPawnRoleSelectionWidget
{
	void DrawPawnList(Rect listRectPawns);

	void WindowUpdate();
}
