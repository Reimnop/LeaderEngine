#version 450 core
layout (location = 0) out vec4 FragColor;

const float weight[] = { 0.079733, 0.078159, 0.073622, 0.066638, 0.05796, 0.048441, 0.038903, 0.030022, 0.022263, 0.015864, 0.010863, 0.007147, 0.004519, 0.002745, 0.001603, 0.000899, 0.000485 };

in vec2 TexCoord;

uniform sampler2D lastStageTexture;

void main() {
	vec2 tStep = 1.0 / textureSize(lastStageTexture, 0);

	vec3 col = vec3(0.0);
	for (int i = 0; i < weight.length(); i++)
    {
		col += texture(lastStageTexture, TexCoord + vec2(0.0, tStep.y * i)).rgb * weight[i];

		if (i != 0)
			col += texture(lastStageTexture, TexCoord - vec2(0.0, tStep.y * i)).rgb * weight[i];
    }

	FragColor = vec4(col, 1.0);
}