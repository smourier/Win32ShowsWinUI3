#pragma once

#include "targetver.h"
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <pathcch.h>

// clr hosting
#define NETHOST_USE_AS_STATIC
#include "hosting\nethost.h"
#include "hosting\coreclr_delegates.h"
#include "hosting\hostfxr.h"

// WIL, requires "Microsoft.Windows.ImplementationLibrary" nuget
#include "wil/win32_helpers.h"

#pragma comment(lib, "pathcch.lib")
