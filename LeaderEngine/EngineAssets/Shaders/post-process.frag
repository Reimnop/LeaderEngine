#version 450 core
layout (location = 0) out vec4 FragColor;

in vec2 TexCoord;

uniform sampler2D _texture;

void main() {
	FragColor = vec4(texture(_texture, TexCoord).rgb, 1.0);
}