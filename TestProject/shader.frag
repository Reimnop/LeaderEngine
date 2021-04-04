#version 430 core
layout (location = 0) out vec4 FragColor;

in vec3 Color;

uniform sampler2D _texture;

void main() {
	FragColor = vec4(Color, 1.0);
}