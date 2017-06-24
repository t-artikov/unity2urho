#include <string>
#include <d3d11.h>
#include <atlbase.h>
#include <deque>
#include <mutex>
#include <fstream>
#include "Unity/IUnityGraphics.h"
#include "Unity/IUnityGraphicsD3D11.h"
#include "DirectXTex/DirectXTex.h"

struct Texture 
{
	Texture(wchar_t* filePath, ID3D11Resource* texture) :
		filePath(filePath),
		texture(texture)
	{}
	std::wstring filePath;
	CComPtr<ID3D11Resource> texture;
};

std::deque<Texture> textures;
std::mutex mutex;

static IUnityInterfaces* unity = NULL;

typedef void (*DebugFunction)(const wchar_t*);
static DebugFunction debugFunction;

static void Print(const std::wstring& str)
{
	if(debugFunction) debugFunction(str.c_str());
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetDebugFunction(DebugFunction value)
{
	debugFunction = value;
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API AddTexture(wchar_t* filePath, void* texturePtr)
{
	std::unique_lock<std::mutex> lock(mutex);
	textures.push_back(Texture(filePath, static_cast<ID3D11Resource*>(texturePtr)));
}


extern "C" int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetUnfinishedTextureCount()
{
	std::unique_lock<std::mutex> lock(mutex);
	return static_cast<int>(textures.size());
}

static void SaveTexture(const Texture& texture)
{
	IUnityGraphicsD3D11* unityDx11 = unity->Get<IUnityGraphicsD3D11>();
	if(!unityDx11) return;
	ID3D11Device* device = unityDx11->GetDevice();
	ID3D11DeviceContext* deviceContext = 0;
	device->GetImmediateContext(&deviceContext);
	DirectX::ScratchImage image;
	HRESULT hr = DirectX::CaptureTexture(device, deviceContext, texture.texture, image);
	if(FAILED(hr)) 
	{
		Print(L"DirectX::CaptureTexture failed: " + texture.filePath + L", " + std::to_wstring(hr));
		return;
	}
	hr = DirectX::SaveToDDSFile(image.GetImages(), image.GetImageCount(), image.GetMetadata(), DirectX::DDS_FLAGS_NONE, texture.filePath.c_str());
	if(FAILED(hr)) 
	{
		Print(L"DirectX::SaveToDDSFile failed: " + texture.filePath + L", " + std::to_wstring(hr));
	}
}

static void UNITY_INTERFACE_API OnRenderEvent(int eventID)
{
	std::unique_lock<std::mutex> lock(mutex);
	while(!textures.empty())
	{
		Texture texture = textures.front();
		SaveTexture(texture);
		textures.pop_front();
	}
}

extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRenderEventFunction()
{
	return OnRenderEvent;
}

extern "C" void	UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
	unity = unityInterfaces;
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{}
