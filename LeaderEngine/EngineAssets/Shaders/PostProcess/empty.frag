#version 450 core
layout (location = 0) out vec4 FragColor;

in vec2 TexCoord;

uniform sampler2D sourceTexture;

void main() {
	FragColor = vec4(texture(sourceTexture, TexCoord).rgb, 1.0);
}