#include "pch.h"
#include "Hosting.h"

#pragma comment(lib, "hosting\\nethost.lib")

hostfxr_initialize_for_dotnet_command_line_fn init_for_cmd_line_fptr;
hostfxr_initialize_for_runtime_config_fn init_for_config_fptr;
hostfxr_get_runtime_delegate_fn get_delegate_fptr;
hostfxr_run_app_fn run_app_fptr;
hostfxr_close_fn close_fptr;

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
