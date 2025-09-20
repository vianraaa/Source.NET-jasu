using Source.Common;
using Source.Common.Bitbuffers;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Source.Engine;


public struct DeltaBitsReader : IDisposable
{
	bf_read? Buffer;
	int LastProp;

	public readonly bf_read GetBitBuf() => Buffer!;

	public DeltaBitsReader(bf_read? buf) {
		Buffer = buf;
		LastProp = -1;
	}

	public void ForceFinished() {
		Buffer = null;
	}

	public uint ReadNextPropIndex() {
		Assert(Buffer != null);

		if (Buffer.BitsLeft >= 7) {
			uint bits = Buffer.ReadUBitLong(7);
			if ((bits & 1) != 0) {
				uint delta = bits >> 3;
				if ((bits & 6) != 0)
					delta = Buffer.ReadUBitVarInternal((int)((bits & 6) >> 1));

				LastProp = (int)(LastProp + 1 + delta);
				Assert(LastProp < Constants.MAX_DATATABLE_PROPS);
				return (uint)LastProp;
			}
			Buffer.BitsRead -= 6;
		}
		else {
			if (Buffer.ReadOneBit() != 0)
				Buffer.Seek(-1);
		}
		ForceFinished();
		return ~0u;
	}

	public void SkipPropData(SendProp prop) => BasePropTypeFns.Get(prop.Type).SkipProp(prop, Buffer!);
	public void CopyPropData(bf_write outWrite, SendProp prop) {
		int start = Buffer!.BitsRead;
		BasePropTypeFns.Get(prop.Type).SkipProp(prop, Buffer!);
		int len = Buffer!.BitsRead - start;
		Buffer!.Seek(start);
		outWrite.WriteBitsFromBuffer(Buffer!, len);
	}
	public void ComparePropData(ref DeltaBitsReader inReader, SendProp prop) => BasePropTypeFns.Get(prop.Type).CompareDeltas(prop, Buffer!, inReader.Buffer!);

	public void Dispose() {
		Assert(Buffer == null);
	}
}

public struct DeltaBitsWriter : IDisposable
{
	bf_write? buf;
	int LastProp;
	public DeltaBitsWriter(bf_write buf) {
		this.buf = buf;
		LastProp = -1;
	}

	public readonly bf_write GetBitBuf() => buf!;

	public void WritePropIndex(int prop) {
		Assert(prop >= 0 && prop < Constants.MAX_DATATABLE_PROPS);
		uint diff = (uint)(prop - LastProp);
		LastProp = prop;
		Assert(diff > 0 && diff <= Constants.MAX_DATATABLE_PROPS);
		int n = ((diff < 0x11u) ? -1 : 0) + ((diff < 0x101u) ? -1 : 0);
		buf!.WriteUBitLong((uint)(diff * 8 - 8 + 4 + n * 2 + 1), 8 + n * 4 + 4 + 2 + 1);
	}

	public readonly void Dispose() {
		buf!.WriteOneBit(0);
	}
}

public class PropVisitor<TableType, PropType>(TableType table) : IEnumerable<PropType> where TableType : IDataTableBase<PropType> where PropType : IDataTableProp
{
	public IEnumerator<PropType> GetEnumerator() {
		foreach (var r in Visit(table))
			yield return r;
	}
	public IEnumerable<PropType> Visit(TableType table) {
		for (int i = 0; i < table.GetNumProps(); i++) {
			PropType type = table.GetProp(i);
			yield return type;
			if (type.GetPropType() == SendPropType.DataTable)
				foreach (var t in Visit((TableType)type.GetDataTable<PropType>()!))
					yield return t;
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public abstract class DatatableStack {
	public SendTablePrecalc Precalc;
	public InlineArray256<object?> Proxies;
	public object Instance;
	protected int CurPropIndex;
	protected SendProp? CurProp;
	protected int ObjectID;
	protected bool Initted;
	public DatatableStack(SendTablePrecalc precalc, object instance, int objectID) {
		Precalc = precalc;
		Instance = instance;
		ObjectID = objectID;
	}

	public void Init(bool explicitRoutes = false) {
		if (explicitRoutes) {

		}
		else {
			RecurseAndCallProxies(Precalc.Root, Instance);
		}
		Initted = true;
	}
	
	public abstract void RecurseAndCallProxies(SendNode node, object instance);

	public bool IsCurProxyValid() => Proxies[Precalc.PropProxyIndices[CurPropIndex]] != null;
	public object? GetCurStructBase() => Proxies[Precalc.PropProxyIndices[CurPropIndex]];
	public void SeekToProp(uint iProp) {
		CurPropIndex = (int)iProp;
		CurProp = Precalc.GetProp((int)iProp);
	}

	public int GetObjectID() {
		return ObjectID;
	}
}

public class ClientDatatableStack : DatatableStack
{
	readonly RecvDecoder Decoder;

	public ClientDatatableStack(RecvDecoder decoder, object instance, int objectID) : base(decoder.Precalc, instance, objectID) {
		Decoder = decoder;
	}

	public override void RecurseAndCallProxies(SendNode node, object? instance) {
		Proxies[node.GetRecursiveProxyIndex()] = instance;

		for (int iChild = 0; iChild < node.GetNumChildren(); iChild++) {
			SendNode? curChild = node.GetChild(iChild);

			object? newStructBase = null;
			if (instance != null)
				newStructBase = CallPropProxy(curChild, curChild.DataTableProp, instance);

			RecurseAndCallProxies(curChild, newStructBase);
		}
	}

	private object? CallPropProxy(SendNode curChild, int prop, object instance) {
		RecvProp recvProp = Decoder.GetDatatableProp(prop);

		object? val = null;
		Assert(recvProp != null);
		if (recvProp == null)
			return null;

		recvProp.GetDataTableProxyFn()(
			recvProp,
			ref val,
			recvProp.FieldInfo,
			GetObjectID()
			);

		return val;
	}
}

[EngineComponent]
public class EngineRecvTable(DtCommonEng DtCommonEng)
{
	public bool Init(Span<RecvTable> tables) {
		foreach (var table in tables) {
			if (table.IsInMainList())
				continue;

			table.SetInMainList(true);
			DtCommonEng.RecvTables.AddLast(table);
		}

		return true;
	}

	public bool Decode(RecvTable table, object instance, bf_read inRead, int objectID, bool updateDTI = true) {
		RecvDecoder? decoder = table.Decoder;
		ErrorIfNot(decoder != null, $"RecvTable_Decode: table '{table.GetName()}' missing a decoder.");

		ClientDatatableStack theStack = new(decoder, instance, objectID);

		theStack.Init();
		int iStartBit = 0, nIndexBits = 0, iLastBit = inRead.BitsRead;
		uint iProp;
		using DeltaBitsReader deltaBitsReader = new(inRead);
		while ((iProp = deltaBitsReader.ReadNextPropIndex()) < Constants.MAX_DATATABLE_PROPS) {
			theStack.SeekToProp(iProp);

			RecvProp? recvProp = decoder.GetProp((int)iProp);

			DecodeInfo decodeInfo = new();
			decodeInfo.Object = theStack.GetCurStructBase();

			if (recvProp != null)
				decodeInfo.FieldInfo = recvProp.FieldInfo;
			else {
				decodeInfo.FieldInfo = null;
			}

			decodeInfo.RecvProxyData.RecvProp = theStack.IsCurProxyValid() ? recvProp : null; 
			decodeInfo.Prop = decoder.GetSendProp((int)iProp);
			decodeInfo.In = inRead;
			decodeInfo.RecvProxyData.ObjectID = objectID;

			BasePropTypeFns.Get(decodeInfo.Prop.GetPropType()).Decode(ref decodeInfo);
		}

		return !inRead.Overflowed;
	}
	public int MergeDeltas(RecvTable table, bf_read? oldState, bf_read newState, bf_write outState, int objectID = -1, Span<int> changedProps = default, bool updateDTI = false) {
		using DeltaBitsReader oldStateReader = new(oldState);
		using DeltaBitsReader newStateReader = new(newState);

		using DeltaBitsWriter deltaBitsWriter = new(outState);

		RecvDecoder? decoder = table.Decoder;
		ErrorIfNot(decoder != null, $"RecvTable_MergeDeltas: table '{table.GetName()}' is missing its decoder.");

		int changed = 0;

		uint oldProp = 0u;
		if (oldState != null)
			oldProp = oldStateReader.ReadNextPropIndex();

		int iStartBit = 0, nIndexBits = 0, iLastBit = newState.BitsRead;

		uint newProp = newStateReader.ReadNextPropIndex();

		while (true) {
			while (oldProp < newProp) {
				deltaBitsWriter.WritePropIndex((int)oldProp);
				oldStateReader.CopyPropData(deltaBitsWriter.GetBitBuf(), decoder.GetSendProp((int)oldProp));
				oldProp = oldStateReader.ReadNextPropIndex();
			}

			if (newProp >= Constants.MAX_DATATABLE_PROPS)
				break;

			if (oldProp == newProp) {
				oldStateReader.SkipPropData(decoder.GetSendProp((int)oldProp));
				oldProp = oldStateReader.ReadNextPropIndex();
			}

			deltaBitsWriter.WritePropIndex((int)newProp);
			newStateReader.CopyPropData(deltaBitsWriter.GetBitBuf(), decoder.GetSendProp((int)newProp));

			if (changedProps != null)
				changedProps[changed] = (int)newProp;

			changed++;

			newProp = newStateReader.ReadNextPropIndex();
		}

		Assert(changed <= Constants.MAX_DATATABLE_PROPS);

		ErrorIfNot(!(oldState != null && oldState.Overflowed) && !newState.Overflowed && !outState.Overflowed, $"RecvTable_MergeDeltas: overflowed in RecvTable '{table.GetName()}'.");

		return changed;
	}

	public bool SetupClientSendTableHierarchy() {
		foreach (ClientSendTable table in DtCommonEng.ClientSendTables) {
			for (int i = 0; i < table.GetNumProps(); i++) {
				ClientSendProp clientProp = table.GetClientProp(i);
				SendProp prop = table.SendTable.Props![i];

				if (prop.Type == SendPropType.DataTable) {
					ReadOnlySpan<char> tableName = clientProp.GetTableName();
					ErrorIfNot(tableName != null && tableName.Length > 0, $"SetupClientSendTableHierarchy: missing table name for prop '{prop.GetName()}'.");

					ClientSendTable? child = FindClientSendTable(tableName);
					if (child == null) {
						Warning($"SetupClientSendTableHierarchy: missing SendTable '{tableName}' (referenced by '{table.GetName()}').\n");
						return false;
					}

					prop.SetDataTable(child.SendTable);
				}
			}
		}

		return true;
	}

	private ClientSendTable? FindClientSendTable(ReadOnlySpan<char> tableName) {
		foreach (var table in DtCommonEng.ClientSendTables) {
			if (table.GetName().Equals(tableName, StringComparison.OrdinalIgnoreCase))
				return table;
		}

		return null;
	}

	internal bool CreateDecoders(bool allowMismatches, out bool anyMismatches) {
		anyMismatches = false;
		if (!SetupClientSendTableHierarchy())
			return false;

		foreach (var decoder in DtCommonEng.RecvDecoders) {
			if (decoder.ClientSendTable == null)
				return false;

			if (!decoder.Precalc.SetupFlatPropertyArray())
				return false;

			Dictionary<SendProp, RecvProp> propLookup = [];
			if (!MatchRecvPropsToSendProps_R(propLookup, decoder.GetSendTable()!.NetTableName, decoder.GetSendTable()!, DtCommonEng.FindRecvTable(decoder.GetSendTable()!.NetTableName)!, allowMismatches, out anyMismatches))
				return false;

			SendTablePrecalc precalc = decoder.Precalc;
			CopySendPropsToRecvProps(propLookup, precalc.Props, decoder.Props);
			CopySendPropsToRecvProps(propLookup, precalc.DataTableProps, decoder.DataTableProps);
		}

		return true;
	}

	private bool MatchRecvPropsToSendProps_R(Dictionary<SendProp, RecvProp> lookup, ReadOnlySpan<char> sendTableName, SendTable sendTable, RecvTable? recvTable, bool allowMismatches, out bool anyMismatches) {
		anyMismatches = false;

		for (int i = 0; i < (sendTable.Props?.Length ?? 0); i++) {
			SendProp sendProp = sendTable.Props![i];
			if (sendProp.IsExcludeProp() || sendProp.IsInsideArray())
				continue;

			RecvProp? recvProp = null;
			if (recvTable != null)
				recvProp = FindRecvProp(recvTable, sendProp.GetName());

			if (recvProp != null) {
				RecvPropMismatchReason mismatch = CompareRecvPropToSendProp(recvProp, sendProp);
				if (mismatch != RecvPropMismatchReason.Matched) {
					Warning($"{mismatch switch {
						RecvPropMismatchReason.MissingRecvProp => "Missing RecvProp",
						RecvPropMismatchReason.MissingSendProp => "Missing SendProp",
						RecvPropMismatchReason.MismatchedPropType => "RecvProp property type doesn't match server property type",
						RecvPropMismatchReason.MismatchedArrayType => "RecvProp array state doesn't match server array state",
						RecvPropMismatchReason.MismatchedArrayElements => "RecvProp array elements doesn't match server array elements",
						_ => "RecvProp type doesn't match server type",
					}} for {sendTable.GetName()}/{sendProp.GetName()}\n");
					return false;
				}

				lookup[sendProp] = recvProp;
			}
			else {
				anyMismatches = true;
				Warning($"Missing RecvProp for {sendTableName} - {sendTable.GetName()}/{sendProp.GetName()}\n");
				if (!allowMismatches)
					return false;
			}

			if (sendProp.GetPropType() == SendPropType.DataTable)
				if (!MatchRecvPropsToSendProps_R(lookup, sendTableName, sendProp.GetDataTable()!, DtCommonEng.FindRecvTable(sendProp.GetDataTable()!.NetTableName), allowMismatches, out anyMismatches))
					return false;
		}

		return true;
	}

	enum RecvPropMismatchReason
	{
		Matched,
		MissingRecvProp,
		MissingSendProp,
		MismatchedPropType,
		MismatchedArrayType,
		MismatchedArrayElements,
	}

	private RecvPropMismatchReason CompareRecvPropToSendProp(RecvProp? recvProp, SendProp? sendProp) {
		while (true) {
			if (recvProp == null)
				return RecvPropMismatchReason.MissingRecvProp;
			if (sendProp == null)
				return RecvPropMismatchReason.MissingSendProp;

			if (recvProp.GetPropType() != sendProp.GetPropType())
				return RecvPropMismatchReason.MismatchedPropType;
			if (recvProp.IsInsideArray() != sendProp.IsInsideArray())
				return RecvPropMismatchReason.MismatchedArrayType;


			if (recvProp.GetPropType() == SendPropType.Array) {
				if (recvProp.GetNumElements() != sendProp.GetNumElements())
					return RecvPropMismatchReason.MismatchedArrayElements;

				recvProp = recvProp.GetArrayProp<RecvProp>();
				sendProp = sendProp.GetArrayProp<SendProp>();
			}
			else
				return RecvPropMismatchReason.Matched;
		}
	}

	private RecvProp? FindRecvProp(RecvTable table, ReadOnlySpan<char> name) {
		for (int i = 0; i < table.GetNumProps(); i++) {
			RecvProp prop = table.GetProp(i);
			if (prop.GetName().Equals(name, StringComparison.OrdinalIgnoreCase))
				return prop;
		}
		return null;
	}

	private void CopySendPropsToRecvProps(Dictionary<SendProp, RecvProp> lookup, List<SendProp> sendProps, List<RecvProp> recvProps) {
		recvProps.SetSize(sendProps.Count);
		for (int i = 0; i < sendProps.Count; i++) {
			SendProp sendProp = sendProps[i];
			if (sendProp == null)
				break;
			if (!lookup.TryGetValue(sendProp, out RecvProp? recv))
				recvProps[i] = null!;
			else
				recvProps[i] = recv == null ? throw new NullReferenceException() : recv;
		}
	}
}
