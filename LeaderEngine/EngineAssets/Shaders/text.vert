#version 430 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;

out vec2 TexCoord;

uniform mat4 mvp;

void main() {
	TexCoord = aTexCoord;
	gl_Position = vec4(aPosition, 1.0) * mvp; 
}