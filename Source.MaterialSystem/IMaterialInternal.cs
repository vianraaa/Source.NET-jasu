using Source.Common.MaterialSystem;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.MaterialSystem;

public record struct MaterialLookup(IMaterialInternal material, long symbol, bool manuallyCreated);
public class MaterialDict : Dictionary<IMaterialInternal, MaterialLookup> {

}

public interface IMaterialInternal : IMaterial
{
	string GetName();
	bool IsManuallyCreated();
	bool IsPrecached();
	void Precache();
}