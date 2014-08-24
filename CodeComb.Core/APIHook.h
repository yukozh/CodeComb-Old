#include "stdafx.h"

#ifndef _APIHOOK_H
#define _APIHOOK_H

DWORD EnablePrivilege();
BOOL LoadRemoteDLL(DWORD dwProcessId, LPWSTR lpszLibName);

#endif