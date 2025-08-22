using Source.Common.MaterialSystem;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common.ShaderAPI;
public interface IShaderAPI : IShaderDynamicAPI
{
	public bool IsAlphaTested(StateSnapshot_t snapshot);
	public bool IsTranslucent(StateSnapshot_t snapshot);
	public bool IsDepthWriteEnabled(StateSnapshot_t id);
	public bool UsesVertexAndPixelShaders(StateSnapshot_t id);
	public StateSnapshot_t TakeSnapshot();
	void DrawMesh(IMesh mesh);
}