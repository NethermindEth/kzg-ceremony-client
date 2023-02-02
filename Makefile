ifeq ($(OS),Windows_NT)
	CFLAGS += -O2
else
	CFLAGS += -O2 -fPIC
endif

CLANG_EXECUTABLE=clang
BLST_BUILD_SCRIPT=./build.sh


# RUN BLST ALWAYS
blst: FORCE
	cd ./blst; \
	${BLST_BUILD_SCRIPT} && \
	cp libblst.a ../lib && \
	cp bindings/*.h ../inc

FORCE: ;
