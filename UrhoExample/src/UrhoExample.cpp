#include <Urho3D/Core/CoreEvents.h>
#include <Urho3D/Engine/Engine.h>
#include <Urho3D/Graphics/Camera.h>
#include <Urho3D/Graphics/Graphics.h>
#include <Urho3D/Graphics/Zone.h>
#include <Urho3D/Graphics/Model.h>
#include <Urho3D/Graphics/Skybox.h>
#include <Urho3D/Input/Input.h>
#include <Urho3D/Resource/ResourceCache.h>
#include <Urho3D/Scene/Scene.h>
#include "UrhoExample.h"

int main()
{
	Urho3D::Context context;
	UrhoExample app(&context);
	return app.Run();
}

UrhoExample::UrhoExample(Context* context) :
    Sample(context)
{
}

void UrhoExample::Start()
{
    // Execute base class startup
    Sample::Start();

    // Create the scene content
    CreateScene();

    // Setup the viewport for displaying the scene
    SetupViewport();

	SetLogoVisible(true);

    // Subscribe to global events for camera movement
    SubscribeToEvents();
}

void UrhoExample::CreateScene()
{
    ResourceCache* cache = GetSubsystem<ResourceCache>();

    scene_ = new Scene(context_);

	cache->AddResourceDir("../scene", 0);

    SharedPtr<File> file = cache->GetFile("scene.xml");
    scene_->LoadXML(*file);

	Camera* camera = scene_->GetComponent<Camera>(true);
	if (camera) cameraNode_ = camera->GetNode();
	if (!cameraNode_)
	{
		cameraNode_ = scene_->CreateChild("Camera");
		cameraNode_->CreateComponent<Camera>();
		cameraNode_->SetPosition(Vector3(0.0f, 2.0f, -10.0f));
	}
	pitch_ = cameraNode_->GetRotation().EulerAngles().x_;
	yaw_ = cameraNode_->GetRotation().EulerAngles().y_;
}

void UrhoExample::SetupViewport()
{
    Renderer* renderer = GetSubsystem<Renderer>();

    // Set up a viewport to the Renderer subsystem so that the 3D scene can be seen
    SharedPtr<Viewport> viewport(new Viewport(context_, scene_, cameraNode_->GetComponent<Camera>()));
    renderer->SetViewport(0, viewport);
}

void UrhoExample::SubscribeToEvents()
{
    // Subscribe HandleUpdate() function for camera motion
    SubscribeToEvent(E_UPDATE, URHO3D_HANDLER(UrhoExample, HandleUpdate));
}

void UrhoExample::MoveCamera(float timeStep)
{
    Input* input = GetSubsystem<Input>();

    const float MOVE_SPEED = 20.0f;
    const float MOUSE_SENSITIVITY = 0.1f;

    IntVector2 mouseMove = input->GetMouseMove();
    yaw_ += MOUSE_SENSITIVITY * mouseMove.x_;
    pitch_ += MOUSE_SENSITIVITY * mouseMove.y_;
    pitch_ = Clamp(pitch_, -90.0f, 90.0f);

    cameraNode_->SetRotation(Quaternion(pitch_, yaw_, 0.0f));

    if (input->GetKeyDown('W'))
        cameraNode_->Translate(Vector3::FORWARD * MOVE_SPEED * timeStep);
    if (input->GetKeyDown('S'))
        cameraNode_->Translate(Vector3::BACK * MOVE_SPEED * timeStep);
    if (input->GetKeyDown('A'))
        cameraNode_->Translate(Vector3::LEFT * MOVE_SPEED * timeStep);
    if (input->GetKeyDown('D'))
        cameraNode_->Translate(Vector3::RIGHT * MOVE_SPEED * timeStep);
}

void UrhoExample::HandleUpdate(StringHash eventType, VariantMap& eventData)
{
    using namespace Update;

    // Take the frame time step, which is stored as a float
    float timeStep = eventData[P_TIMESTEP].GetFloat();

    // Move the camera, scale movement with time step
    MoveCamera(timeStep);
}
