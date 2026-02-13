#!/bin/zsh

#Expects version string as $1

function compress()
{
	#Expects input dir as $1 and output name as $2
	zip -r "bin/Release/$2.zip" "$1"
}

function build()
{
	#Expects RID as $1
	dotnet publish -c Release -r "$1" --self-contained -p:PublishReadyToRun=true
}

if [[ -z "$1" ]]; then
    echo "Please specify release version tag!"
    exit 1
fi

build "linux-x64"
build "win-x64"
build "win-x86"

release_name="RRGControl"
dir="bin/Release/net6.0/linux-x64/publish"
if [[ -d $dir ]]; then
	compress $dir "$release_name-$1_lin-x64"
fi
dir="bin/Release/net6.0/win-x64/publish"
if [[ -d $dir ]]; then
	compress $dir "$release_name-$1_win-x64"
fi
dir="bin/Release/net6.0/win-x86/publish"
if [[ -d $dir ]]; then
	compress $dir "$release_name-$1_win-x86"
fi
