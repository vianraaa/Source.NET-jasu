namespace Source.Common;

public enum FieldType {
	Void = 0,         // No type or value
	Float,            // Any floating point value
	String,           // A string ID (return from ALLOC_STRING)
	Vector,           // Any vector, QAngle, or AngularImpulse
	Quaternion,       // A quaternion
	Integer,          // Any integer or enum
	Boolean,          // boolean, implemented as an int, I may use this as a hint for compression
	Short,            // 2 byte integer
	Character,        // a byte
	Color32,          // 8-bit per channel r,g,b,a (32bit color)
	Embedded,         // an embedded object with a datadesc, recursively traverse and embedded class/structure based on an additional typedescription
	Custom,           // special type that contains function pointers to it's read/write/parse functions

	ClassPtr,         // CBaseEntity *
	EHandle,          // Entity handle
	EDict,            // edict_t *

	PositionVector,  // A world coordinate (these are fixed up across level transitions automagically)
	Time,             // a floating point time (these are fixed up automatically too!)
	Tick,             // an integer tick count( fixed up similarly to time)
	ModelName,        // Engine string that is a model name (needs precache)
	SoundName,        // Engine string that is a sound name (needs precache)

	Input,            // a list of inputed data fields (all derived from CMultiInputVar)
	Function,         // A class function pointer (Think, Use, etc)

	Matrix,          // a vmatrix (output coords are NOT worldspace)

	// NOTE: Use float arrays for local transformations that don't need to be fixed up.
	WorldspaceMatrix,// A VMatrix that maps some local space to world space (translation is fixed up on level transitions)
	WorldspaceMatrix3x4, // matrix3x4_t that maps some local space to world space (translation is fixed up on level transitions)

	Interval,         // a start and range floating point interval ( e.g., 3.2->3.6 == 3.2 and 0.4 )
	ModelIndex,       // a model index
	MaterialIndex,    // a material index (using the material precache string table)

	Vector2D,         // 2 floats

	Count,        // MUST BE LAST
}

public enum TypeDescriptionOffset {
	Normal,
	Packed,
	Count
}

public class TypeDescription {
	public FieldType FieldType;
	public string FieldName = "";
	public readonly int[] FieldOffset = new int[(int)TypeDescriptionOffset.Count];
	public ushort FieldSize;
	public short Flags;
	public string ExternalName = "";
	// ISaveRestoreOps?
	// InputFunc?
	public DataMap TD;
	public int FieldSizeInBytes;
	public TypeDescription OverrideField;
	public int OverrideCount;
	public float FieldTolerance;
}

public class DataMap {
	public TypeDescription[] DataDesc;
	public string DataClassName = "";
	public DataMap? BaseMap;
	public bool ChainsValidated;
	public bool PackedOffsetsComputed;
	public int PackedSize;
#if DEBUG
	public bool ValidityChecked = false;
#endif
}