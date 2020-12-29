#version 330 core

layout (location = 0) out vec4 fragColor;

uniform sampler2D texture0;

in vec2 TexCoord;

float gamma = 1.0;
float bloomThreshold = 0.75;

float exposure = 1.0;

float bloomIntensity = 5;

uniform float weight[5] = float[] (0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216);

vec3 gammaCorrect(vec3 col) {
	return pow(col, vec3(1.0 / gamma));
}

vec3 getBloomColor(sampler2D tex, vec2 coords) {
	vec3 col = texture(texture0, coords).rgb;
	if ((col.x + col.y + col.z) / 3.0 >= bloomThreshold)
		return col;
	else return vec3(0.0);
}

void main() 
{
	/*vec3 bloomCol = getBloomColor(texture0, TexCoord);
	vec2 tex_offset = 1.0 / textureSize(texture0, 0);
	vec3 result = bloomCol * weight[0];

	for(int i = 1; i < 40; ++i)
    {
		result += getBloomColor(texture0, TexCoord + vec2(tex_offset.x * i, 0.0)) * weight[int(i / 8)];
		result += getBloomColor(texture0, TexCoord - vec2(tex_offset.x * i, 0.0)) * weight[int(i / 8)];
	}

	for(int i = 1; i < 40; ++i)
    {
		result += getBloomColor(texture0, TexCoord + vec2(0.0, tex_offset.y * i)) * weight[int(i / 8)];
		result += getBloomColor(texture0, TexCoord - vec2(0.0, tex_offset.y * i)) * weight[int(i / 8)];
    }

	result /= 80.0;

	result *= bloomIntensity;

	result += texture(texture0, TexCoord).rgb;*/

	fragColor = vec4(gammaCorrect(texture(texture0, TexCoord).rgb), 1.0);
}