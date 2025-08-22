using Source.Common.ShaderAPI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.MaterialSystem;

public class ShaderDeviceGl46 : IShaderDevice
{
	internal ShaderAPIGl46 ShaderAPI;
	public bool IsUsingGraphics() {
		return false; // I don't care
	}
}