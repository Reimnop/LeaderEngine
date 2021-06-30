#version 450 core

layout (location = 0) out vec4 fragColor;

in vec2 TexCoord;

uniform sampler2D sourceTexture;
uniform sampler2D bloomTexture;

void main() {
	vec3 hdrColor = texture(sourceTexture, TexCoord).rgb;
	vec3 bloomColor = texture(bloomTexture, TexCoord).rgb;

	fragColor = vec4(hdrColor + bloomColor, 1.0);
}