namespace Source.Common.MaterialSystem;

public enum VertexElement : int
{
	None = -1,
	Position = 0,
	Normal = 1,
	Color = 2,
	Specular = 3,
	TangentS = 4,
	TangentT = 5,
	Wrinkle = 6,
}

[Flags]
public enum VertexFormat : ulong
{
	Position = 1 << VertexElement.Position,
	Normal = 1 << VertexElement.Normal,
	Color = 1 << VertexElement.Color,
	Specular = 1 << VertexElement.Specular,
	TangentS = 1 << VertexElement.TangentS,
	TangentT = 1 << VertexElement.TangentT,
	TangentSpace = TangentS | TangentT,
}

public interface IMaterial
{

}
