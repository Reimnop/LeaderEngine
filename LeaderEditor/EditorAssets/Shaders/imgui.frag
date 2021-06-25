#version 450 core

layout (location = 0) out vec4 fragColor;

uniform sampler2D inTexture;

in vec4 Color;
in vec2 TexCoord;

void main()
{
    fragColor = texture(inTexture, TexCoord) * Color;
}