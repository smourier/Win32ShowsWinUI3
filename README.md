# Win32ShowsWinUI3
A simple Win32 C++ desktop application that can show WinUI3 elements

This is a regular Win32 desktop application which knows nothing about WinUI3, it's main message loop is completely standard. Click on "Show WinUI3 Window menu item" to add WinUI3 content to it:

![Screenshot 2024-11-23 152526](https://github.com/user-attachments/assets/5d07b087-1e6d-4998-bd5e-9c99ec1b5208)

This is what we'll see, the Click button is a WinUI3 button, using [DesktopWindowXamlSource](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.hosting.desktopwindowxamlsource) to connect to a C# .NET 8 WinUI3 component library:

![Screenshot 2024-11-23 152510](https://github.com/user-attachments/assets/db5fd88b-9895-44dc-a9af-c6ef61fd08a0)

Click on it and a WinUI3 `ContentDialog` is shown:

![Screenshot 2024-11-23 152547](https://github.com/user-attachments/assets/08ec8aae-368a-4e8a-b5be-918a9edafa53)

Note there's a way to keep Win32 style (XAML style not initialized) like this:

![Screenshot 2024-11-23 152608](https://github.com/user-attachments/assets/d63fffa6-6b56-40a0-beb1-220a19c1f4b5)
