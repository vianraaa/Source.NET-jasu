#version 460
#extension GL_ARB_separate_shader_objects : enable

out gl_PerVertex {
    vec4 gl_Position;
};

layout(location = 0) in vec3 v_Position;
layout(location = 6) in vec2 v_TexCoord;
layout(location = 2) in vec4 v_Color;

uniform mat4 mvp;

out vec2 vs_TexCoord;
out vec4 vs_Color;

void main()
{
	// mat4 mvp = u_Projection * u_View * u_Model;

    gl_Position = mvp * vec4(v_Position, 1.0);
    vs_TexCoord = v_TexCoord;
    vs_Color    = v_Color;
}