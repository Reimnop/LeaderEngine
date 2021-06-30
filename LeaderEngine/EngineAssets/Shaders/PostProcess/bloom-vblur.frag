#version 450 core

layout (location = 0) out vec4 fragColor;

in vec2 TexCoord;
uniform sampler2D sourceTexture;

uniform int size = 25;
uniform float sigma = 10;

float normpdf(float x, float sigma)
{
	return 0.39894*exp(-0.5*x*x/(sigma*sigma))/sigma;
}

void main() {
	float unitVertical = 1.0 / textureSize(sourceTexture, 0).y;
	float currentLoc = TexCoord.y - size * unitVertical;

	for (int i = 0; i < size * 2 + 1; i++) {
		fragColor += texture(sourceTexture, vec2(TexCoord.x, currentLoc)) * normpdf(i - size, sigma);
		currentLoc += unitVertical;
	}
}