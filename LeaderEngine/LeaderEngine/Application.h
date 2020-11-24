#pragma once

#include <GLFW/glfw3.h>
#include <list>
#include "Types/GameObject.h"

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
		float lastTime;

		void load();
		void update();
		void render();
	};
}