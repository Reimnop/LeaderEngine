#version 400 core

layout (location = 0) out vec4 gAlbedoSpec;
layout (location = 1) out vec3 gPosition;
layout (location = 2) out vec3 gNormal;
layout (location = 3) out vec3 gPositionViewSpace;
layout (location = 4) out vec3 gNormalViewSpace;
layout (location = 5) out vec4 alpha;

uniform bool useTexture;

uniform vec4 color;

uniform sampler2D texture0;

in vec3 VertCol;
in vec2 TexCoord;

in vec3 NormalWorldSpace;
in vec3 FragPosWorldSpace;

in vec3 NormalViewSpace;
in vec3 FragPosViewSpace;

in vec3 FragPos;

in float Depth;

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
	alpha = vec4(outColor.a, 0.0, 1.0, outColor.a);
}
