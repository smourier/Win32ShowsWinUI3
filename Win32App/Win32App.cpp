#include "pch.h"
#include "Hosting.h"
#include "Win32App.h"

constexpr auto MAX_LOADSTRING = 100;

HINSTANCE hInst;                                // current instance
WCHAR szTitle[MAX_LOADSTRING];                  // The title bar text
WCHAR szWindowClass[MAX_LOADSTRING];            // the main window class name

ATOM MyRegisterClass(HINSTANCE hInstance);
BOOL InitInstance(HINSTANCE, int);
LRESULT CALLBACK WndProc(HWND, UINT, WPARAM, LPARAM);
INT_PTR CALLBACK About(HWND, UINT, WPARAM, LPARAM);
void CenterWindow(HWND hwnd, bool useCursorPos = true);

int APIENTRY wWinMain(_In_ HINSTANCE hInstance, _In_opt_ HINSTANCE hPrevInstance, _In_ LPWSTR lpCmdLine, _In_ int nCmdShow)
{
	UNREFERENCED_PARAMETER(hPrevInstance);
	UNREFERENCED_PARAMETER(lpCmdLine);

	LoadStringW(hInstance, IDS_APP_TITLE, szTitle, MAX_LOADSTRING);
	LoadStringW(hInstance, IDC_WIN32APP, szWindowClass, MAX_LOADSTRING);
	MyRegisterClass(hInstance);

	if (!InitInstance(hInstance, nCmdShow))
		return FALSE;

	auto hAccelTable = LoadAccelerators(hInstance, MAKEINTRESOURCE(IDC_WIN32APP));

	MSG msg;
	while (GetMessage(&msg, nullptr, 0, 0))
	{
		if (!TranslateAccelerator(msg.hwnd, hAccelTable, &msg))
		{
			TranslateMessage(&msg);
			DispatchMessage(&msg);
		}
	}

	return (int)msg.wParam;
}

ATOM MyRegisterClass(HINSTANCE hInstance)
{
	WNDCLASSEXW wcex{};
	wcex.cbSize = sizeof(WNDCLASSEX);
	wcex.style = CS_HREDRAW | CS_VREDRAW;
	wcex.lpfnWndProc = WndProc;
	wcex.cbClsExtra = 0;
	wcex.cbWndExtra = 0;
	wcex.hInstance = hInstance;
	wcex.hIcon = LoadIcon(hInstance, MAKEINTRESOURCE(IDI_WIN32APP));
	wcex.hCursor = LoadCursor(nullptr, IDC_ARROW);
	wcex.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);
	wcex.lpszMenuName = MAKEINTRESOURCEW(IDC_WIN32APP);
	wcex.lpszClassName = szWindowClass;
	wcex.hIconSm = LoadIcon(wcex.hInstance, MAKEINTRESOURCE(IDI_SMALL));
	return RegisterClassExW(&wcex);
}

BOOL InitInstance(HINSTANCE hInstance, int nCmdShow)
{
	hInst = hInstance;
	auto hWnd = CreateWindowW(szWindowClass, szTitle, WS_OVERLAPPEDWINDOW, CW_USEDEFAULT, CW_USEDEFAULT, 400, 300, nullptr, nullptr, hInstance, nullptr);
	if (!hWnd)
		return FALSE;

	CenterWindow(hWnd);
	ShowWindow(hWnd, nCmdShow);
	UpdateWindow(hWnd);
	return TRUE;
}

LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	switch (message)
	{
	case WM_COMMAND:
	{
		int wmId = LOWORD(wParam);
		switch (wmId)
		{
		case IDM_ABOUT:
			DialogBox(hInst, MAKEINTRESOURCE(IDD_ABOUTBOX), hWnd, About);
			break;

		case ID_FILE_SHOWWINUI3WINDOW:
			ShowWinUI3Window(hWnd);
			break;

		case IDM_EXIT:
			DestroyWindow(hWnd);
			break;

		default:
			return DefWindowProc(hWnd, message, wParam, lParam);
		}
	}
	break;

	case WM_PAINT:
	{
		PAINTSTRUCT ps;
		HDC hdc = BeginPaint(hWnd, &ps);
		// TODO: Add any drawing code that uses hdc here...
		EndPaint(hWnd, &ps);
	}
	break;

	case WM_DESTROY:
		PostQuitMessage(0);
		break;

	default:
		return DefWindowProc(hWnd, message, wParam, lParam);
	}
	return 0;
}

INT_PTR CALLBACK About(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam)
{
	UNREFERENCED_PARAMETER(lParam);
	switch (message)
	{
	case WM_INITDIALOG:
		return (INT_PTR)TRUE;

	case WM_COMMAND:
		if (LOWORD(wParam) == IDOK || LOWORD(wParam) == IDCANCEL)
		{
			EndDialog(hDlg, LOWORD(wParam));
			return (INT_PTR)TRUE;
		}
		break;
	}
	return (INT_PTR)FALSE;
}

void CenterWindow(HWND hwnd, bool useCursorPos)
{
	if (!IsWindow(hwnd))
		return;

	RECT rc{};
	GetWindowRect(hwnd, &rc);
	auto width = rc.right - rc.left;
	auto height = rc.bottom - rc.top;

	if (useCursorPos)
	{
		POINT pt{};
		if (GetCursorPos(&pt))
		{
			auto monitor = MonitorFromPoint(pt, MONITOR_DEFAULTTONEAREST);
			MONITORINFOEX  mi{};
			mi.cbSize = sizeof(MONITORINFOEX);
			if (GetMonitorInfo(monitor, &mi))
			{
				SetWindowPos(hwnd, NULL, mi.rcWork.left + (mi.rcWork.right - mi.rcWork.left - width) / 2, mi.rcWork.top + (mi.rcWork.bottom - mi.rcWork.top - height) / 2, 0, 0, SWP_NOREDRAW | SWP_NOSIZE | SWP_NOZORDER);
				return;
			}
		}
	}

	SetWindowPos(hwnd, NULL, (GetSystemMetrics(SM_CXSCREEN) - width) / 2, (GetSystemMetrics(SM_CYSCREEN) - height) / 2, 0, 0, SWP_NOREDRAW | SWP_NOSIZE | SWP_NOZORDER);
}
