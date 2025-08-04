using System.Runtime.CompilerServices;

namespace Source.Common.Commands;

public interface IConVar {
	public void SetValue(string value);
	public void SetValue(int value);
	public void SetValue(float value);
	public void SetValue(double value);

	public string GetName();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsFlagSet(ConVarFlags flag);
}
