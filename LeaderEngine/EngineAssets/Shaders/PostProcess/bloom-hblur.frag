#version 450 core

layout (location = 0) out vec4 fragColor;

in vec2 TexCoord;
uniform sampler2D sourceTexture;

float[] kernel = { 
	0.0036264159768789666,
	0.007525556986797724,
	0.014331637237175307,
	0.025046560558617814,
	0.040169475088747915,
	0.059121057685709716,
	0.07985256808220205,
	0.09897763368952453,
	0.1125860511279542,
	0.1175260871327833,
	0.1125860511279542,
	0.09897763368952453,
	0.07985256808220205,
	0.059121057685709716,
	0.040169475088747915,
	0.025046560558617814,
	0.014331637237175307,
	0.007525556986797724,
	0.0036264159768789666 };

void main() {
	float unitHorizontal = 1.0 / textureSize(sourceTexture, 0).x;
	float currentLoc = TexCoord.x - floor(kernel.length() / 2) * unitHorizontal;

	for (int i = 0; i < kernel.length(); i++) {
		fragColor += texture(sourceTexture, vec2(currentLoc, TexCoord.y)) * kernel[i];
		currentLoc += unitHorizontal;
	}
}