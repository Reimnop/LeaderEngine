#version 430 core

layout (location = 0) out vec4 fragColor;

in vec2 TexCoord;

uniform float t_buffer = 0.7;
uniform float t_gamma = 0.0;

uniform sampler2D fontAtlas;

void main() {
	vec2 _step = 1.0 / textureSize(fontAtlas, 0);

	float alpha = 0.0;
	for (int i = -1; i <= 1; i++)
		for (int j = -1; j <= 1; j++)
			alpha += texture(fontAtlas, TexCoord + _step * vec2(i, j)).r;

	alpha /= 9.0;

	alpha = alpha > 0.5 ? 1.0 : 0.0;

	fragColor = vec4(vec3(1.0), alpha);
}