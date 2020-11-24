#include <iostream>
#include "LeaderEngine/LeaderEngine.h"

using namespace LeaderEngine;

float vertices[] = {
    -0.5f, -0.5f, 0.0f,
     0.5f, -0.5f, 0.0f,
     0.0f,  0.5f, 0.0f
};

unsigned int indices[] = {
    0, 1, 2
};

const char* vertexShaderSource = "#version 330 core\n"
"layout (location = 0) in vec3 aPos;\n"
"void main()\n"
"{\n"
"   gl_Position = vec4(aPos.x, aPos.y, aPos.z, 1.0);\n"
"}";

const char* fragmentShaderSource = "#version 330 core\n"
"out vec4 FragColor;\n"
"void main()\n"
"{\n"
"    FragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);\n"
"}";

void onLoad() {
    VertexArray* vertArray = new VertexArray(std::vector<float>(vertices, vertices + sizeof(vertices) / sizeof(vertices[0])), std::vector<unsigned int>(indices, indices + sizeof(indices) / sizeof(indices[0])));
    Shader* shader = new Shader(vertexShaderSource, fragmentShaderSource);

    GameObject* go = new GameObject("test obj");
    go->vertArray = vertArray;
    go->shader = shader;
}

int main(void) 
{
	Application* app = new Application();
	app->start(1280, 720, "Hello, World!", &onLoad);
}