#version 330 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aVertCol;
layout (location = 2) in vec2 aTexCoord;

out vec3 VertCol;
out vec2 TexCoord;

uniform mat4 mvp;

void main() 
{
	VertCol = aVertCol;
	TexCoord = aTexCoord;
	gl_Position = vec4(aPos, 1.0) * mvp;
}