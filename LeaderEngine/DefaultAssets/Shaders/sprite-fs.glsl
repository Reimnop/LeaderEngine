#version 330 core

layout (location = 0) out vec4 fragColor;

uniform vec4 color;

uniform sampler2D texture0;

in vec2 TexCoord;

void main() 
{
	fragColor = texture(texture0, TexCoord) * color;
}