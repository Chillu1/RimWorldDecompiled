using Verse;

namespace LudeonTK;

public abstract class Window_Dev : Window
{
	public Window_Dev()
		: base(new DevWindowDrawing())
	{
	}
}
