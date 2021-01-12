#version 330 core

layout (location = 0) out float fragColor;

uniform sampler2D ssaoTexture;

uniform int blurSamples = 4;

uniform vec2 vSize;

in vec2 TexCoord;

void main() 
{
	vec2 pixelSize = 1.0 / vSize;

	float outPixel = 0.0;

	if (blurSamples > 0) {
		for (int i = -blurSamples; i < blurSamples; i++) {
			for (int j = -blurSamples; j < blurSamples; j++) {
				outPixel += texture(ssaoTexture, TexCoord + pixelSize * vec2(i, j)).r;
			}
		}

		outPixel /= 4.0 * blurSamples * blurSamples;
	}
	else {
		outPixel = texture(ssaoTexture, TexCoord).r;
	}

	fragColor = outPixel;
}