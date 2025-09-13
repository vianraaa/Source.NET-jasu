using Game.Client.HL2MP;
using Game.Shared;

using Source.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Client;

[DeclareClientClass(ManualIndexOverride = StaticClassIndices.CWorld)]
[LinkEntityToClass(LocalName = "player")]
public class C_World : C_BaseEntity
{
	public static IClientNetworkable CreateObject(int entNum, int serialNum) {
		C_World ret = new C_World();
		ret.Init(entNum, serialNum);
		return ret;
	}
	public static readonly RecvTable DT_World = [];
	public static readonly ClientClass ClientClass = new("CWorld", CreateObject, null, DT_World);
}