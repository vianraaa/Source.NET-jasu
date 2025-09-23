using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common;

public delegate void EnsureCapacityFn(object instance, object list, int length);
public delegate void ResizeVectorFn(object instance, object list, int length);
public class SendPropExtra_UtlVector
{
	public SendTableProxyFn DataTableProxyFn;
	public SendVarProxyFn ProxyFn;
	public EnsureCapacityFn EnsureCapacityFn;
	public int MaxElements;
}
