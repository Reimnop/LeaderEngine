#version 430 core

layout (location = 0) out vec4 fragColor;

in vec2 TexCoord;

uniform sampler2D fontAtlas;

const float width = 0.5;
const float edge = 0.01;

void main() {
	float dist = texture(fontAtlas, TexCoord).a;
	float alpha = smoothstep(width, width + edge, dist);

	fragColor = vec4(vec3(1.0), alpha);
}