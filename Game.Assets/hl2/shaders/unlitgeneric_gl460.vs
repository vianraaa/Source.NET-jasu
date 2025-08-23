#version 460

in vec3 v_Position;
in vec2 v_TexCoord;
in vec3 v_Normal;
in vec4 v_Color;

uniform mat4 mvp;

out vec2 f_TexCoord;
out vec4 f_Color;

void main()
{
	// mat4 mvp = u_Projection * u_View * u_Model;

    gl_Position = mvp * vec4(v_Position, 1.0);
    f_TexCoord = v_TexCoord;
    f_Color    = v_Color;
}