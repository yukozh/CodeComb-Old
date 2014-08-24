// CodeComb.Core.cpp : 定义控制台应用程序的入口点。
//

#include "stdafx.h"
#include "CodeComb.Core.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// 唯一的应用程序对象

CWinApp theApp;

using namespace std;

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

int _tmain(int argc, TCHAR* argv[], TCHAR* envp[])
{
	int nRetCode = 0;
	const int nSuccess = 0;
	const int nSystemError = 1;
	const int nArgcError = 2;

	HMODULE hModule = ::GetModuleHandle(NULL);
	HANDLE TimeLimitValidator;
	DWORD TimeLimitValidatorThreadID;

	DWORD WINAPI TimeLimitValidatorThreadProc(LPVOID lpParam);

	if (hModule != NULL)
	{
		if (!AfxWinInit(hModule, NULL, ::GetCommandLine(), 0))
		{
			_tprintf(_T("ERROR:  MFC init failed\n"));
			nRetCode = nSystemError;
		}
		else
		{
			char buf[255];
			gets_s(buf);
			CommondLine = CString(buf);
			gets_s(buf);
			InputFilePath = CString(buf);
			gets_s(buf);
			OutputFilePath = CString(buf);
			gets_s(buf);
			ErrorFilePath = CString(buf);
			gets_s(buf);
			APIHookPath = CString(buf);
			cin >> TimeLimit >> MemoryLimit >> HighPriorityTime;
			SECURITY_ATTRIBUTES saAttr;
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
				HANDLE InputFile = CreateFile(InputFilePath.c_str(), GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, &saAttr, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
				StartupInfo.hStdInput = InputFile;
			}
			if (OutputFilePath != L"")
			{
				HANDLE OutputFile = CreateFile(OutputFilePath.c_str(), GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, &saAttr, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
				StartupInfo.hStdOutput = OutputFile;
			}
			if (ErrorFilePath != L"")
			{
				HANDLE ErrputFile = CreateFile(ErrorFilePath.c_str(), GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, &saAttr, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
				StartupInfo.hStdError = ErrputFile;
			}
			StartupInfo.dwFlags |= STARTF_USESTDHANDLES;

			HANDLE hToken;
			HANDLE hNewToken;
			PWSTR szIntegritySid = L"S-1-16-4096"; // 低完整性 SID
			PSID pIntegritySid = NULL;
			TOKEN_MANDATORY_LABEL TIL = { 0 };
			ULONG ExitCode = 0;
			OpenProcessToken(GetCurrentProcess(), MAXIMUM_ALLOWED, &hToken);
			DuplicateTokenEx(hToken, MAXIMUM_ALLOWED, NULL, SecurityImpersonation, TokenPrimary, &hNewToken);
			ConvertStringSidToSid(szIntegritySid, &pIntegritySid);
			TIL.Label.Attributes = SE_GROUP_INTEGRITY;
			TIL.Label.Sid = pIntegritySid;
			SetTokenInformation(hNewToken, TokenIntegrityLevel, &TIL,
				sizeof(TOKEN_MANDATORY_LABEL)+sizeof(pIntegritySid));
			CreateProcessAsUser(hNewToken, NULL, (LPWSTR)CommondLine.c_str(), NULL, NULL, FALSE, 0, NULL, NULL, (LPSTARTUPINFOW)(&StartupInfo), &ProcessInfo);

			//CreateProcess(NULL, (LPWSTR)CommondLine.c_str(), NULL, NULL, TRUE, CREATE_SUSPENDED, NULL, NULL, (LPSTARTUPINFOW)(&StartupInfo), &ProcessInfo);
			if (CString(APIHookPath.c_str()) != CString(""))
			{
				BOOL hooked = LoadRemoteDLL(ProcessInfo.dwProcessId, (LPWSTR)APIHookPath.c_str());
				if (!hooked)
					wcerr << L"API Hook Failed !" << endl;
				else
					wcerr << L"API Hook Succeeded !" << endl;
			}
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
			nRetCode = nSuccess;
		}
	}
	else
	{
		_tprintf(_T("ERROR:  GetModuleHandle failed\n"));
		nRetCode = nSystemError;
	}
	return nRetCode;
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