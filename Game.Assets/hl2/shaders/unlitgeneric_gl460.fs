#version 460

in vec2 f_TexCoord;
in vec4 f_Color;

uniform sampler2D basetexture;

out vec4 fragColor;

void main()
{
    vec4 texelColor = texture(basetexture, f_TexCoord);
    //fragColor = texelColor; // f_Color multiply later. Requires a flags check
    fragColor = vec4(f_TexCoord.x, f_TexCoord.y, 1.0, 1.0);
}