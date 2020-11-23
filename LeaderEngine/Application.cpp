#include <stdlib.h>
#include <stdio.h>
#include "linmath.h"
#include "Application.h"

using namespace LeaderEngine;

void Application::start(int width, int height, const char* title, void (*loadCallback)()) 
{
	if (instance)
		return;

	instance = this;

	if (!glfwInit())
		exit(EXIT_FAILURE);

	this->loadCallback = loadCallback;

	//window creation
	glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 4);
	glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 0);
	glfwWindowHint(GLFW_RESIZABLE, 0);

	window = glfwCreateWindow(width, height, title, NULL, NULL);

	if (!window) {
		glfwTerminate();
		exit(EXIT_FAILURE);
	}

	glfwMakeContextCurrent(window);
	glfwSwapInterval(0);

	load();

	while (!glfwWindowShouldClose(window)) {
		update();
		render();
	}

	//cleanup
	glfwDestroyWindow(window);
	glfwTerminate();

	exit(EXIT_SUCCESS);
}

void Application::load() 
{
	(*loadCallback)();
}

void Application::update() 
{
	for (auto go : gameObjects)
		go->update();
}

void Application::render() 
{
	int width, height;
	glfwGetFramebufferSize(window, &width, &height);

	glViewport(0, 0, width, height);
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

	

	glfwSwapBuffers(window);
	glfwPollEvents();
}
