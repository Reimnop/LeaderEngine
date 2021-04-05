#version 430 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aColor;
layout (location = 2) in vec2 aTexCoord;

out vec2 TexCoord;
out vec3 Color;

uniform mat4 mvp;

void main() {
	TexCoord = aTexCoord;
	Color = aColor;
	gl_Position = vec4(aPos, 1.0) * mvp;
}