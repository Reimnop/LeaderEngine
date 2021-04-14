#version 450 core
layout (location = 0) out vec4 FragColor;

in vec2 TexCoord;

uniform sampler2D sourceTexture;

void main() {
	vec3 col = texture(sourceTexture, TexCoord).rgb;
	FragColor = col.r * 0.2126 + col.g * 0.7152 + col.b * 0.0722 >= 0.9 ? vec4(col, 1.0) : vec4(0.0);
}