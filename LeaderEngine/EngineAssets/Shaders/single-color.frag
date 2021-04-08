#version 430 core

layout (location = 0) out vec4 fragColor;

in vec3 Color;

void main() {
	fragColor = vec4(Color, 1.0);
}