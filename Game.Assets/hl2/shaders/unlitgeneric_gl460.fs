#version 460

in vec2 vs_TexCoord;
in vec4 vs_Color;

uniform sampler2D basetexture;

out vec4 fragColor;

void main()
{
    vec4 texelColor = texture(basetexture, vs_TexCoord);
    fragColor = texelColor; // f_Color multiply later. Requires a flags check
    //fragColor = vec4(vs_TexCoord.x, vs_TexCoord.y, 1.0, 1.0);
}