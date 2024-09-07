#!/bin/sh

# Start the backend and check it's return code
/app/backend 
if [ $? -ne 0 ]; then
	echo "Backend failed. Not copying output."
	exit 1
fi;

# Copy the generated files 
cp -var /app/wwwroot/* /output/
