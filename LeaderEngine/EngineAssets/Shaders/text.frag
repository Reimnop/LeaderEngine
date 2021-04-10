#version 430 core

layout (location = 0) out vec4 fragColor;

in vec2 TexCoord;

uniform sampler2D fontAtlas;

void main() {
	fragColor = vec4(vec3(1.0), texture(fontAtlas, TexCoord).r);
}