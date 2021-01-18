#version 450 core

layout (location = 0) out vec4 fragColor;

uniform float color;

void main() 
{
	fragColor = vec4(color, 0.0, 0.0, 1.0);
}