#version 330 core

layout (location = 0) out vec4 fragColor;

uniform sampler2D ssaoTexture;

uniform int blurSamples = 4;

uniform vec2 vSize;

in vec2 TexCoord;

void main() 
{
	vec2 pixelSize = 1.0 / vSize;

	vec3 outPixel = vec3(0.0);

	if (blurSamples > 0) {
		for (int i = -blurSamples; i < blurSamples; i++) {
			for (int j = -blurSamples; j < blurSamples; j++) {
				outPixel += texture(ssaoTexture, TexCoord + pixelSize * vec2(i, j)).rgb;
			}
		}

		outPixel /= 4.0 * blurSamples * blurSamples;
	}
	else {
		outPixel = texture(ssaoTexture, TexCoord).rgb;
	}

	fragColor = vec4(outPixel, 1.0);
}