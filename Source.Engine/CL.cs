using Source.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Engine;

/// <summary>
/// Various clientside methods. In Source, these would mostly be represented by
/// CL_MethodName's in the static global namespace
/// </summary>
public class CL(IServiceProvider services)
{
	internal void ApplyAddAngle() {
		throw new NotImplementedException();
	}

	internal void CheckClientState() {

	}

	internal void ExtraMouseUpdate(double frameTime) {
		throw new NotImplementedException();
	}

	internal void Init() {

	}

	internal void RunPrediction(PredictionReason reason) {
		
	}
}



/// <summary>
/// Loads and shuts down the client DLL
/// </summary>
/// <param name="services"></param>
public class ClientDLL(IServiceProvider services, Sys Sys)
{
	public void Init() {

	}

	public void Update() {

	}
}