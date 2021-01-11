#version 330 core

layout (location = 0) out vec4 fragColor;

uniform sampler2D ssaoTexture;
uniform sampler2D albedo;

uniform vec2 vSize;

in vec2 TexCoord;

void main() 
{
	vec3 col = texture(albedo, TexCoord).rgb;

	vec2 pixelSize = 1.0 / vSize;

	vec3 blurredPixel = vec3(0.0);
	for (int i = -2; i < 2; i++) {
		for (int j = -2; j < 2; j++) {
			blurredPixel += texture(ssaoTexture, TexCoord + pixelSize * vec2(i, j)).rgb;
		}
	}

	blurredPixel /= 16.0;

	fragColor = vec4(col * blurredPixel, 1.0);
}