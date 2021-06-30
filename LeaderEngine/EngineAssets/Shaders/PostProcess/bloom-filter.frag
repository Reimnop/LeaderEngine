#version 450 core

layout (location = 0) out vec4 fragColor;

in vec2 TexCoord;
uniform sampler2D sourceTexture;

uniform float threshold = 0.8;

void main() {
	vec3 color = texture(sourceTexture, TexCoord).rgb;
	float brightness = dot(color, vec3(0.2126, 0.7152, 0.0722));

	fragColor = brightness >= threshold ? vec4(color, 1.0) : vec4(0.0, 0.0, 0.0, 1.0);
}