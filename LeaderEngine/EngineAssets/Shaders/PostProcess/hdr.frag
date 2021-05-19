#version 450 core
layout (location = 0) out vec4 FragColor;

in vec2 TexCoord;

uniform sampler2D sourceTexture;

uniform float exposure = 1.0;

void main() {
	const float gamma = 2.2;

	vec3 hdrColor = texture(sourceTexture, TexCoord).rgb;

    // exposure tone mapping
    vec3 color = vec3(1.0) - exp(-hdrColor * exposure);

	//gamma correct
	color = pow(color, vec3(1.0 / gamma));

	FragColor = vec4(color, 1.0);
}