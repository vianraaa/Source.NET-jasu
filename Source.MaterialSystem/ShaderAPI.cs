using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.MaterialSystem;

public struct ShadowShaderState{
	public VertexShaderHandle VertexShader;
	public PixelShaderHandle PixelShader;

	public VertexFormat VertexUsage;
	public bool ModulateConstantColor;
}

public class ShaderShadowGl46 : IShaderShadow {

}

public class ShaderAPIGl46 : IShaderAPI
{
	public VertexFormat ComputeVertexFormat(Span<StateSnapshot_t> snapshots) {
		return ComputeVertexUsage(snapshots);
	}

	public VertexFormat ComputeVertexUsage(Span<StateSnapshot_t> snapshots) {
		if (snapshots.Length == 0)
			return 0;

		if (snapshots.Length == 1) {
			ref ShadowShaderState state = TransitionTable.GetSnapshotShader(snapshots[0]);
			return state.VertexUsage;
		}
	}
}