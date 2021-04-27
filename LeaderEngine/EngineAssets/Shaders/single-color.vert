#version 430 core

layout (location = 0) in vec3 aPosition;
layout (location = 2) in vec3 aColor;

out vec3 Color;

uniform mat4 mvp;

void main() {
	Color = aColor;
	gl_Position = vec4(aPosition, 1.0) * mvp; 
}