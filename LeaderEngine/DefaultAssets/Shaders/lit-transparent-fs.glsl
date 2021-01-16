#version 400 core

layout (location = 0) out vec4 gAlbedoSpec;
layout (location = 1) out vec3 gPosition;
layout (location = 2) out vec3 gNormal;
layout (location = 3) out vec3 gPositionViewSpace;
layout (location = 4) out vec3 gNormalViewSpace;
layout (location = 5) out vec4 accumulation;
layout (location = 6) out float revealage;

uniform bool useTexture;

uniform vec4 color;

uniform mat4 projection;

uniform sampler2D texture0;

in vec3 VertCol;
in vec2 TexCoord;

in vec3 NormalWorldSpace;
in vec3 FragPosWorldSpace;

in vec3 NormalViewSpace;
in vec3 FragPosViewSpace;

in vec3 FragPos;

in float Depth;

void writePixel(vec3 color, float alpha, float wsZ) {
    float ndcZ = 2.0 * wsZ - 1.0;
    // linearize depth for proper depth weighting
    //See: https://stackoverflow.com/questions/7777913/how-to-render-depth-linearly-in-modern-opengl-with-gl-fragcoord-z-in-fragment-sh
    //or: https://stackoverflow.com/questions/11277501/how-to-recover-view-space-position-given-view-space-depth-value-and-ndc-xy
	mat4 proj = transpose(projection);
    float linearZ = (proj[2][2] + 1.0) * wsZ / (proj[2][2] + ndcZ);
    //float tmp = (1.0 - linearZ) * alpha;
    float tmp = (linearZ * 0.99) * alpha * 10.0;
    float w = clamp(pow(tmp, 10.0), 0.2, 512.0);
    accumulation = vec4(color * alpha * w, alpha);
    revealage = alpha * w;
}

void main() 
{
	vec4 outColor = vec4(VertCol, 1.0) * color;

	if (useTexture)
		outColor *= texture(texture0, TexCoord);

	gAlbedoSpec = vec4(vec3(outColor), 1.0);
	gPosition = FragPosWorldSpace;
	gNormal = normalize(NormalWorldSpace);

	gPositionViewSpace = FragPosViewSpace;
	gNormalViewSpace = NormalViewSpace;

	//alpha
	writePixel(vec3(outColor), outColor.a, Depth); 
}
