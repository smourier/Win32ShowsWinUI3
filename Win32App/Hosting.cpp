#include "pch.h"
#include "Hosting.h"

#pragma comment(lib, "hosting\\nethost.lib")

hostfxr_initialize_for_dotnet_command_line_fn init_for_cmd_line_fptr = nullptr;
hostfxr_initialize_for_runtime_config_fn init_for_config_fptr = nullptr;
hostfxr_get_runtime_delegate_fn get_delegate_fptr = nullptr;
hostfxr_run_app_fn run_app_fptr = nullptr;
hostfxr_close_fn close_fptr = nullptr;
component_entry_point_fn _showWindowFn = nullptr;

bool load_hostfxr()
{
	char_t buffer[2048];
	auto buffer_size = sizeof(buffer) / sizeof(char_t);
	auto rc = get_hostfxr_path(buffer, &buffer_size, nullptr);
	if (rc != 0)
		return false;

	auto lib = LoadLibrary(buffer);
	if (!lib)
		return false;

	init_for_config_fptr = (hostfxr_initialize_for_runtime_config_fn)GetProcAddress(lib, "hostfxr_initialize_for_runtime_config");
	get_delegate_fptr = (hostfxr_get_runtime_delegate_fn)GetProcAddress(lib, "hostfxr_get_runtime_delegate");
	close_fptr = (hostfxr_close_fn)GetProcAddress(lib, "hostfxr_close");

	return (init_for_config_fptr && get_delegate_fptr && close_fptr);
}

load_assembly_and_get_function_pointer_fn get_dotnet_load_assembly(const char_t* config_path)
{
	void* load_assembly_and_get_function_pointer = nullptr;
	hostfxr_handle cxt = nullptr;
	auto rc = init_for_config_fptr(config_path, nullptr, &cxt);
	if (rc != 0 || !cxt)
	{
		close_fptr(cxt);
		return nullptr;
	}

	rc = get_delegate_fptr(cxt, hdt_load_assembly_and_get_function_pointer, &load_assembly_and_get_function_pointer);
	close_fptr(cxt);
	return (load_assembly_and_get_function_pointer_fn)load_assembly_and_get_function_pointer;
}

int ShowWinUI3Window(HWND hwnd)
{
	if (!_showWindowFn)
	{
		auto loaded = load_hostfxr();
		if (!loaded)
		{
			MessageBox(nullptr, L"Error: cannot load .NET Core.", L"Win32ShowsWinUI3", 0);
			return 0;
		}

		auto filePath = wil::GetModuleFileNameW();
		PathCchRemoveFileSpec(filePath.get(), lstrlen(filePath.get()));

		wil::unique_cotaskmem_string rtPath;
		PathAllocCombine(filePath.get(), L"WinUI3ClassLibrary.runtimeconfig.json", 0, &rtPath);

		auto load_assembly_and_get_function_pointer = get_dotnet_load_assembly(rtPath.get());
		if (!load_assembly_and_get_function_pointer)
		{
			MessageBox(nullptr, L"Error: cannot load 'WinUI3ClassLibrary' assembly", L"Win32ShowsWinUI3", 0);
			return 0;
		}

		wil::unique_cotaskmem_string dllPath;
		PathAllocCombine(filePath.get(), L"WinUI3ClassLibrary.dll", 0, &dllPath);

		auto hr = load_assembly_and_get_function_pointer(
			dllPath.get(),
			L"WinUI3ClassLibrary.SampleWindow, WinUI3ClassLibrary",
			L"ShowWindow",
			nullptr,
			nullptr,
			(void**)&_showWindowFn);

		if (!_showWindowFn)
		{
			MessageBox(nullptr, L"Error: cannot load 'ShowWindow' function.", L"Win32ShowsWinUI3", 0);
			return 0;
		}
	}

	return _showWindowFn((void*)hwnd, sizeof(void*));
}
