// dllmain.h : Declaration of module class.

class CIPhreeqcCOMModule : public ATL::CAtlDllModuleT< CIPhreeqcCOMModule >
{
public :
	DECLARE_LIBID(LIBID_IPhreeqcCOMLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_IPHREEQCCOM, "{2539a7ee-93f4-4c9e-b875-e194168d4cd0}")
};

extern class CIPhreeqcCOMModule _AtlModule;
