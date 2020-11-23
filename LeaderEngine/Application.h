#pragma once

#include <GLFW/glfw3.h>
#include <list>
#include "GameObject.h"

namespace LeaderEngine 
{
	class Application
	{
	public:
		static inline Application* instance;

		GLFWwindow* window;

		void (*loadCallback)();

		std::list<GameObject*> gameObjects;

		void start(int width, int height, const char* title, void (*loadCallback)());
	private:
		void load();
		void update();
		void render();
	};
}