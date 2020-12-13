#version 330 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aVertCol;

out vec3 VertCol;

uniform mat4 mvp;

void main() 
{
	VertCol = aVertCol;
	gl_Position = vec4(aPos, 1.0) * mvp;
}