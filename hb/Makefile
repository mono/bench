#
# Makefile for hb.exe
#
# Authors:
#   Gonzalo Paniagua Javier (gonzalo@ximian.com)
#
topdir=..

INSTALL=/usr/bin/install
RUNTIME=
MCS=mcs
MCSFLAGS= /debug+ /debug:full /nologo

ASSEMBLIES= Mono.GetOptions

SOURCES= hb.cs \
	 AssemblyInfo.cs

# 

REFS= $(addsuffix .dll, $(addprefix /r:, $(ASSEMBLIES)))

all: hb.exe

hb.exe: $(SOURCES) $(RESOURCES) $(STRINGFILES)
	$(MCS) $(MCSFLAGS) $(REFS) $(RESS) /out:$@ $(SOURCES)

clean:
	rm -f *~ *.exe *.bak *.temp

