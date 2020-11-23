#include "GameObject.h"
#include "Application.h"

using namespace LeaderEngine;

GameObject::GameObject(char* name) {
	this->name = name;
	Application::instance->gameObjects.push_back(this);
	start();
}

GameObject::~GameObject() {
	Application::instance->gameObjects.remove(this);
}

void GameObject::start() {
	//TODO: add start for GameObject
}

void GameObject::update() {
	for (auto comp : components)
		comp->update();
}

void GameObject::render() {
	//TODO: add render for GameObject
}

void GameObject::addComponent(Component* component) {
	components.push_back(component);
	component->start();
}

template<typename T>
inline T& GameObject::getComponent() {
	for (auto comp : components)
		if (typeof(comp) == T)
			return comp;
}

void GameObject::removeComponent(Component* component) {
	components.remove(component);
}