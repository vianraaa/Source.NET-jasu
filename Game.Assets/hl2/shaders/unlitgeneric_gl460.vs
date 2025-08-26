#version 460
#extension GL_ARB_separate_shader_objects : enable

out gl_PerVertex {
    vec4 gl_Position;
};

layout(location = 0) in vec3 v_Position;
layout(location = 2) in uvec4 v_Color;
layout(location = 6) in vec2 v_TexCoord;

layout(std140, binding = 0) uniform Matrices {
    mat4 viewMatrix;
    mat4 projectionMatrix;
    mat4 modelMatrix;
};

out vec2 vs_TexCoord;
out vec4 vs_Color;

void main()
{
	mat4 mvp = projectionMatrix * viewMatrix * modelMatrix;

    gl_Position = mvp * vec4(v_Position, 1.0);
    vs_TexCoord = v_TexCoord;
    vs_Color    = v_Color;
}