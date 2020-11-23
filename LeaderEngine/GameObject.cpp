#include "GameObject.h"
#include "Application.h"

using namespace LeaderEngine;

GameObject::GameObject(const char* name) {
	this->name = (char*)name;
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

void GameObject::removeComponent(Component* component) {
	components.remove(component);
}