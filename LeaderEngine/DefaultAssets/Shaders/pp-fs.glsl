#version 330 core

layout (location = 0) out vec4 fragColor;

uniform sampler2D colorTexture;
uniform sampler2D depthTexture;

in vec2 TexCoord;

void main() 
{
    fragColor = vec4(texture(colorTexture, TexCoord).rgb, 1.0);

    gl_FragDepth = texture(depthTexture, TexCoord).r;
}