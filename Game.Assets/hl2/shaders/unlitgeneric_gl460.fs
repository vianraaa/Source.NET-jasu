#version 460

in vec2 f_TexCoord;
in vec4 f_Color;

uniform sampler2D basetexture;

out vec4 gl_Frag;

void main()
{
    vec4 texelColor = texture(texture0, fragTexCoord);
    gl_Frag = texelColor;
}