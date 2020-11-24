#include "GameObject.h"
#include "../Application.h"

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
	this->transform = new Transform();
	addComponent(this->transform);
	//TODO: add start for GameObject
}

void GameObject::update() {
	for (auto& comp : components)
		comp.second->update();
	for (auto& comp : components)
		comp.second->lateUpdate();
}

void GameObject::render() {
	//TODO: add render for GameObject
}

void GameObject::setActive(bool active) {
	this->active = active;
}

//methods for the entity component system
void GameObject::addComponent(Component* component) {
	if (typeid(*component) == typeid(Transform))
		return;

	components[typeid(*component)] = component;
	component->start();
}

void GameObject::addComponents(std::vector<Component*> components) {
	for (auto comp : components)
		if (typeid(*comp) == typeid(Transform))
			return;

	for (auto comp : components)
		this->components[typeid(*comp)] = comp;

	for (auto comp : components)
		comp->start();
}

template<typename T>
void GameObject::removeComponent() {
	if (typeid(T) == typeid(Transform))
		return;

	components.erase(typeid(T));
}