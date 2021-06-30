#version 450 core

layout (location = 0) out vec4 fragColor;

in vec2 TexCoord;

uniform sampler2D sourceTexture;
uniform sampler2D ssaoTexture;

void main() {
	vec2 texUnit = 1.0 / textureSize(sourceTexture, 0);

	float ssaoAvg = 0.0;
	for (float i = -2; i < 2; i++) {
		for (float j = -2; j < 2; j++) {
			ssaoAvg += texture(ssaoTexture, TexCoord + texUnit * vec2(i, j)).r;
		}
	}
	ssaoAvg /= 16.0;

	fragColor = vec4(texture(sourceTexture, TexCoord).rgb * ssaoAvg, 1.0);
}