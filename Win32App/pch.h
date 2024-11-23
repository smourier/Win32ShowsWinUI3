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
#include "wil/result.h"
#include "wil/stl.h"
#include "wil/win32_helpers.h"
#include "wil/com.h"

// C++/WinRT, requires "Microsoft.Windows.CppWinRT" nuget
#include "winrt/base.h"
#include <winrt/Windows.Foundation.h>
#include <winrt/Windows.Foundation.Collections.h>
#include <winrt/Windows.ApplicationModel.h>
#include <winrt/Windows.Management.Deployment.h>
