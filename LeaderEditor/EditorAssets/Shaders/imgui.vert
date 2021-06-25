#version 450 core

layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec4 aColor;

uniform mat4 projection;

out vec4 Color;
out vec2 TexCoord;

void main()
{
    Color = aColor;
    TexCoord = aTexCoord;
    gl_Position = vec4(aPosition, 0.0, 1.0) * projection;
}