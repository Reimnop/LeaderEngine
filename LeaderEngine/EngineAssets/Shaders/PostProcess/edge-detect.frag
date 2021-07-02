#version 450 core

layout (location = 0) out vec4 fragColor;

in vec2 TexCoord;

uniform sampler2D sourceTexture;
uniform sampler2D depthTexture;
uniform sampler2D gNormal;

vec2 kernel[9] = { 
	vec2( 0,  0),
	vec2(-1, -1),
	vec2(-1,  0),
	vec2(-1,  1),
	vec2( 0,  1),
	vec2( 1,  1),
	vec2( 1,  0),
	vec2( 1, -1),
	vec2( 0, -1)
};

void main() {
	vec2 unit = 1.0 / textureSize(sourceTexture, 0);

	float depth[9];
	vec3 normal[9];

	for (int i = 0; i < 9; i++) {
		depth[i]  = texture(depthTexture, TexCoord + unit * kernel[i] * 2.0).r * 20.0;
		normal[i] = texture(gNormal     , TexCoord + unit * kernel[i] * 2.0).xyz;
	}

	vec4 delta0 = vec4(depth[1], depth[2], depth[3], depth[4]);
	vec4 delta1 = vec4(depth[5], depth[6], depth[7], depth[8]);

	delta0 = abs(delta0 - vec4(depth[0]));
	delta1 = abs(delta1 - vec4(depth[0]));

	vec4 maxDelta = max(delta0, delta1);
	vec4 minDelta = max(min(delta0, delta1), vec4(0.0001));

	vec4 dresult = step(minDelta * 25.0, maxDelta);

	delta0 = vec4(
		dot(normal[1], normal[0]),
		dot(normal[3], normal[0]),
		dot(normal[5], normal[0]),
		dot(normal[7], normal[0])
	);

	vec4 nresult = step(0.4, delta1);
	nresult = max(nresult, dresult);
	float edge = (nresult.x + nresult.y + nresult.z + nresult.w) * 0.25;

	vec3 color = texture(sourceTexture, TexCoord).rgb * (1.0 - edge);

	fragColor = vec4(color, 1.0);
}