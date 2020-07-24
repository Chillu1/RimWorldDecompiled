using UnityEngine;

namespace Verse
{
	public static class DebugInputLogger
	{
		public static void InputLogOnGUI()
		{
			if (DebugViewSettings.logInput && (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseUp || Event.current.type == EventType.KeyDown || Event.current.type == EventType.KeyUp || Event.current.type == EventType.ScrollWheel))
			{
				Log.Message("Frame " + Time.frameCount + ": " + Event.current.ToStringFull());
			}
		}

		public static string ToStringFull(this Event ev)
		{
			return string.Concat("(EVENT\ntype=", ev.type, "\nbutton=", ev.button, "\nkeyCode=", ev.keyCode, "\ndelta=", ev.delta, "\nalt=", ev.alt.ToString(), "\ncapsLock=", ev.capsLock.ToString(), "\ncharacter=", ((ev.character != 0) ? ev.character : ' ').ToString(), "\nclickCount=", ev.clickCount, "\ncommand=", ev.command.ToString(), "\ncommandName=", ev.commandName, "\ncontrol=", ev.control.ToString(), "\nfunctionKey=", ev.functionKey.ToString(), "\nisKey=", ev.isKey.ToString(), "\nisMouse=", ev.isMouse.ToString(), "\nmodifiers=", ev.modifiers, "\nmousePosition=", ev.mousePosition, "\nnumeric=", ev.numeric.ToString(), "\npressure=", ev.pressure, "\nrawType=", ev.rawType, "\nshift=", ev.shift.ToString(), "\n)");
		}
	}
}
