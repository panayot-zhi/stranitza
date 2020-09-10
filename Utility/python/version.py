import sys
import subprocess
from xml.etree import ElementTree as et

try:
	csproj = sys.argv[1]
	tree = et.parse(csproj)

	version = tree.find('.//Version').text
	gitcset = subprocess.check_output("git rev-parse --short HEAD").decode('utf-8').rstrip()
	info = version.split(".")

	major = info[0]
	minor = info[1]
	build = info[2].split('-')[0]

	bump = int(build) + 1
	newversion = major + "." + minor + "." + str(bump) + "-" + gitcset
		
	tree.find('.//Version').text = newversion
	print("stranitza -> version updated to " + newversion)
	tree.write(csproj)
except Exception as e:
  print("stranitza -> error updating version, " + str(e))