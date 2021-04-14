#version 450 core
layout (location = 0) out vec4 FragColor;

in vec2 TexCoord;

uniform sampler2D sourceTexture;
uniform sampler2D lastStageTexture;

void main() {
    vec3 hdrColor = texture(sourceTexture, TexCoord).rgb;      
    vec3 bloomColor = texture(lastStageTexture, TexCoord).rgb;

    FragColor = vec4(hdrColor + bloomColor * 0.4, 1.0);
}