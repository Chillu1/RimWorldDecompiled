using UnityEngine;

namespace Verse;

public interface IWindowDrawing
{
	GUIStyle EmptyStyle { get; }

	void DoWindowBackground(Rect rect);

	bool DoCloseButton(Rect rect, string text);

	bool DoClostButtonSmall(Rect rect);

	void BeginGroup(Rect rect);

	void EndGroup();

	void DoGrayOut(Rect rect);
}
