INCLUDE_DIRS = ../../src ../../blst/bindings

ifeq ($(OS),Windows_NT)
	BLST_BUILDSCRIPT=./build.bat
	BLST_OBJ=blst.lib
	# TESTS_EXECUTABLE=test_ckzg.exe
	CSHARP_PLATFORM?=win-x64
	CLANG_PLATFROM?=x86_64-win
	CLANG_FLAGS=
	CLANG_EXECUTABLE=clang
	CKZG_LIBRARY_PATH=Blst.Bindings\runtimes\$(CSHARP_PLATFORM)\native\blstlib.dll
else
	BLST_BUILDSCRIPT=./build.sh
	BLST_OBJ=libblst.a
	# TESTS_EXECUTABLE=./test_ckzg
	CSHARP_PLATFORM?=linux-x64
	CLANG_PLATFORM?=x86_64-linux
	CLANG_FLAGS=-fPIC
	CLANG_EXECUTABLE=cc
	CKZG_LIBRARY_PATH=Blst.Bindings/runtimes/$(CSHARP_PLATFORM)/native/blstlib.so
endif

TARGETS=../../blst/$(BLST_OBJ)

.blst:
	cd ../../blst && $(BLST_BUILDSCRIPT)

.wrap:
	$(CLANG_EXECUTABLE) -O -Wall -shared $(CLANG_FLAGS) ${addprefix -I,${INCLUDE_DIRS}} -o $(CKZG_LIBRARY_PATH) $(TARGETS)

blstLib:
	@make .blst
	@make .wrap