#pragma once
#include "Sample.h"

namespace Urho3D
{
	class Drawable;
	class Node;
	class Scene;
}

class UrhoExample : public Sample
{
    URHO3D_OBJECT(UrhoExample, Sample);

public:
    UrhoExample(Context* context);
    virtual void Start();
private:
    void CreateScene();
    void SetupViewport();
    void SubscribeToEvents();
    void MoveCamera(float timeStep);
    void HandleUpdate(StringHash eventType, VariantMap& eventData);
};
