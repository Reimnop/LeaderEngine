#version 450 core
layout (location = 0) out vec4 FragColor;

in vec2 TexCoord;

uniform sampler2D lastStageTexture;

void main() {
	FragColor = vec4(texture(lastStageTexture, TexCoord).rgb, 1.0);
}