#version 330 core

layout (location = 0) out vec4 fragColor;

in vec3 VertCol;

void main() 
{
	fragColor = vec4(VertCol, 1.0);
}