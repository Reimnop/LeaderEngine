#version 450 core
layout (location = 0) out vec4 FragColor;

in vec2 TexCoord;

uniform sampler2D lastStageTexture;

void main() {
	vec2 tStep = 1.0 / textureSize(lastStageTexture, 0);

	vec4 col = vec4(0.0);
	for (int i = -4; i <= 4; i++) {
		col += texture(lastStageTexture, TexCoord + tStep * vec2(i, 0.0));
	}
	col /= 9.0;

	FragColor = col;
}