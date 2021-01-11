#version 330 core

layout (location = 0) out vec4 fragColor;

uniform sampler2D ssaoTexture;

uniform int blurSamples = 2;

uniform vec2 vSize;

in vec2 TexCoord;

void main() 
{
	vec2 pixelSize = 1.0 / vSize;

	vec3 blurredPixel = vec3(0.0);
	for (int i = -blurSamples; i < blurSamples; i++) {
		for (int j = -blurSamples; j < blurSamples; j++) {
			blurredPixel += texture(ssaoTexture, TexCoord + pixelSize * vec2(i, j)).rgb;
		}
	}

	blurredPixel /= 4.0 * blurSamples * blurSamples;

	fragColor = vec4(blurredPixel, 1.0);
}