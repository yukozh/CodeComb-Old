#include <iostream>
#include <Windows.h>
#include <Tlhelp32.h>
#include <Psapi.h>
#include <Sddl.h>
#include <winnt.h>
#include <string>
#pragma comment(lib,"psapi.lib")
using namespace std;
DWORD EnablePrivilege()
{
	HANDLE hToken;
	BOOL rv;
	TOKEN_PRIVILEGES priv = { 1, { 0, 0, SE_PRIVILEGE_ENABLED } };
	OpenProcessToken(
		GetCurrentProcess(),
		TOKEN_ADJUST_PRIVILEGES,
		&hToken
		);
	AdjustTokenPrivileges(
		hToken,
		FALSE,
		&priv,
		sizeof priv,
		0,
		0
		);
	rv = GetLastError();
	CloseHandle(hToken);
	return rv;
}

BOOL GetProcessIdByName(LPCSTR szProcessName, LPDWORD lpPID)
{
	STARTUPINFO st;
	PROCESS_INFORMATION pi;
	PROCESSENTRY32 ps;
	HANDLE hSnapshot;
	ZeroMemory(&st, sizeof(STARTUPINFO));
	ZeroMemory(&pi, sizeof(PROCESS_INFORMATION));
	st.cb = sizeof(STARTUPINFO);
	ZeroMemory(&ps, sizeof(PROCESSENTRY32));
	ps.dwSize = sizeof(PROCESSENTRY32);
	hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
	if (hSnapshot == INVALID_HANDLE_VALUE)
	{
		return FALSE;
	}

	if (!Process32First(hSnapshot, &ps))
	{
		return FALSE;
	}
	do
	{
		if (lstrcmpi(ps.szExeFile, szProcessName) == 0)
		{
			*lpPID = ps.th32ProcessID;
			CloseHandle(hSnapshot);
			return TRUE;
		}
	} while (Process32Next(hSnapshot, &ps));
	CloseHandle(hSnapshot);
	return FALSE;
}

BOOL LoadRemoteDLL(DWORD dwProcessId, LPWSTR lpszLibName)
{
	if (!EnablePrivilege())
		return FALSE;
	BOOL bResult = FALSE;
	HANDLE hProcess = NULL;
	HANDLE hThread = NULL;
	PSTR pszLibFileRemote = NULL;
	DWORD cch;
	LPTHREAD_START_ROUTINE pfnThreadRtn;
	hProcess = OpenProcess(
		PROCESS_ALL_ACCESS,
		FALSE,
		dwProcessId
		);
	if (hProcess == NULL)
		return FALSE;
	cch = 1 + lstrlen(lpszLibName.c_str());
	pszLibFileRemote = (PSTR)VirtualAllocEx
		(
		hProcess,
		NULL,
		cch,
		MEM_COMMIT,
		PAGE_READWRITE
		);
	if (pszLibFileRemote == NULL)
		return FALSE;
	if (!WriteProcessMemory
		(
		hProcess,
		(PVOID)pszLibFileRemote,
		(PVOID)lpszLibName,
		cch,
		NULL
		))
		return FALSE;
	pfnThreadRtn = (LPTHREAD_START_ROUTINE)GetProcAddress(
		GetModuleHandle(_T("Kernel32")),
		"LoadLibraryW"
		);
	if (pfnThreadRtn == NULL)
		return FALSE;
	hThread = CreateRemoteThread(
		hProcess,
		NULL,
		0,
		pfnThreadRtn,
		(PVOID)pszLibFileRemote,
		0,
		NULL
		);
	if (hThread == NULL)
		return FALSE;
	WaitForSingleObject(hThread, INFINITE);
	bResult = TRUE;
	if (pszLibFileRemote != NULL)
		bResult = VirtualFreeEx(hProcess, (PVOID)pszLibFileRemote, 0, MEM_RELEASE);
	if (bResult == FALSE)
		return bResult;
	if (hThread != NULL)
		bResult = CloseHandle(hThread);
	if (bResult == FALSE)
		return bResult;
	if (hProcess != NULL)
		bResult = CloseHandle(hProcess);
	if (bResult == FALSE)
		return bResult;
	return bResult;
}
typedef struct _THREAD_PARAM
{
	HANDLE ProcessHandle;
	int TimeLimit;
} THREAD_PARAM, *LPTHREAD_PARAM;

int TimeLimit;
int MemoryLimit;
int HighPriorityTime;
DWORD ExitCode = 0;
int TimeUsed = -1;
int PagedUsed = -1;
int WorkingSetUsed = -1;
wstring CommondLine, InputFilePath, OutputFilePath, ErrorFilePath, APIHookPath;
int main()
{
	getline(wcin, CommondLine);//filename & args
	getline(wcin, InputFilePath);
	getline(wcin, OutputFilePath);
	getline(wcin, ErrorFilePath);
	getline(wcin, APIHookPath);
	wcin >> TimeLimit >> MemoryLimit >> HighPriorityTime;

	int nRetCode = 0;
	const int nSuccess = 0;
	const int nSystemError = 1;
	const int nArgcError = 2;

	HMODULE hModule = ::GetModuleHandle(NULL);
	HANDLE TimeLimitValidator;
	DWORD TimeLimitValidatorThreadID;

	DWORD WINAPI TimeLimitValidatorThreadProc(LPVOID lpParam);
	SECURITY_ATTRIBUTES saAttr;
	HANDLE ChildIn_Read, ChildIn_Write, ChildOut_Read, ChildOut_Write;
	HANDLE TimeLimitValidator;
	STARTUPINFOA StartupInfo;
	PROCESS_INFORMATION ProcessInfo;
	PROCESS_MEMORY_COUNTERS ProcessMemoryCounters;

	ZeroMemory(&saAttr, sizeof(saAttr));
	saAttr.nLength = sizeof(SECURITY_ATTRIBUTES);
	saAttr.bInheritHandle = TRUE;
	saAttr.lpSecurityDescriptor = NULL;
	ZeroMemory(&StartupInfo, sizeof(StartupInfo));
	StartupInfo.cb = sizeof(STARTUPINFO);
	ZeroMemory(&ProcessInfo, sizeof(ProcessInfo));
	if (InputFilePath != L"")
	{
		HANDLE InputFile = CreateFile(argv[2], GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, &saAttr, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
		StartupInfo.hStdInput = InputFile;
	}
	if (OutputFilePath != L"")
	{
		HANDLE OutputFile = CreateFile(argv[3], GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, &saAttr, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
		StartupInfo.hStdOutput = OutputFile;
	}
	if (ErrorFilePath != L"")
	{
		HANDLE ErrputFile = CreateFile(argv[4], GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, &saAttr, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
		StartupInfo.hStdError = ErrputFile;
	}
	StartupInfo.dwFlags |= STARTF_USESTDHANDLES;
	CreateProcess(NULL, argv[1], NULL, NULL, TRUE, CREATE_SUSPENDED, NULL, NULL, (LPSTARTUPINFOW)(&StartupInfo), &ProcessInfo);
		if (APIHookPath != L"")
			LoadRemoteDLL(ProcessInfo.dwProcessId, (wchar_t*)APIHookPath.c_str());
	LPTHREAD_PARAM pData;
	pData = (LPTHREAD_PARAM)HeapAlloc(GetProcessHeap(), HEAP_ZERO_MEMORY, sizeof(THREAD_PARAM));
	pData->ProcessHandle = ProcessInfo.hProcess;
	pData->TimeLimit = TimeLimit;
	TimeLimitValidator = CreateThread(
		NULL,
		0,
		TimeLimitValidatorThreadProc,
		pData,
		0,
		&TimeLimitValidatorThreadID
		);
	ResumeThread(ProcessInfo.hThread);
	WaitForSingleObject(ProcessInfo.hProcess, INFINITE);
	GetProcessMemoryInfo(ProcessInfo.hProcess, &ProcessMemoryCounters, sizeof(ProcessMemoryCounters));
	PagedUsed = ProcessMemoryCounters.PeakPagefileUsage / 1024;
	WorkingSetUsed = ProcessMemoryCounters.PeakWorkingSetSize / 1024;
	DWORD ThreadExitCode;
	GetExitCodeThread(TimeLimitValidator, &ThreadExitCode);
	CloseHandle(TimeLimitValidator);
	GetExitCodeProcess(pData->ProcessHandle, &ExitCode);
	FILETIME CreateTime, ExitTime, KernelTime, UserTime, CurrentTime;
	GetProcessTimes(ProcessInfo.hProcess, &CreateTime, &ExitTime, &KernelTime, &UserTime);
	TimeUsed = UserTime.dwLowDateTime / 10000;
	if (ThreadExitCode == 2)
	{
		TimeUsed = TimeLimit + 1;
		ExitCode = -1;
	}
	wcout << L"{\"ExitCode\":" << ExitCode
		  << L",\"TimeUsage\":" << TimeUsed
		  << L",\"PagedSize\":" << PagedUsed
		  << L",\"WorkingSet\":" << WorkingSetUsed
		  << L"}";
	return 0;
}

DWORD WINAPI TimeLimitValidatorThreadProc(LPVOID lpParam)
{
	LPTHREAD_PARAM pData;
	pData = (LPTHREAD_PARAM)lpParam;
	DWORD SleepTime = pData->TimeLimit - 100 > 0 ? pData->TimeLimit - 100 : pData->TimeLimit;
	while (true)
	{
		Sleep(SleepTime);
		DWORD ExitCode;
		GetExitCodeProcess(pData->ProcessHandle, &ExitCode);
		if (ExitCode != STILL_ACTIVE)
		{
			return 0;
		}
		FILETIME CreateTime, ExitTime, KernelTime, UserTime, CurrentTime;
		GetSystemTimeAsFileTime(&CurrentTime);

		GetProcessTimes(pData->ProcessHandle, &CreateTime, &ExitTime, &KernelTime, &UserTime);
		DWORD PhysicalTime = (CurrentTime.dwLowDateTime - CreateTime.dwLowDateTime) / 10000;
		TimeUsed = UserTime.dwLowDateTime / 10000;
		if (TimeUsed > HighPriorityTime)
		{
			SetPriorityClass(pData->ProcessHandle, IDLE_PRIORITY_CLASS);
			SetPriorityClass(GetCurrentProcess(), IDLE_PRIORITY_CLASS);
		}
		if (TimeUsed > pData->TimeLimit)
		{
			TerminateProcess(pData->ProcessHandle, NULL);
			return 1;
		}
		if (PhysicalTime > pData->TimeLimit * 2)
		{
			TerminateProcess(pData->ProcessHandle, NULL);
			return 2;
		}

	}
	return 0;
}
