"""
Script only works on windows
"""

import win32gui
import win32com.client
import time
import argparse
import xml.etree.ElementTree as ET


XSIZE=10
YSIZE=8

description_text = """Add a level from an *.ipe file. \r \n

The procedure to use this script is as follows. First draw a single polygon using ipe with the vertices in a CLOCKWISE order.
Save this as a *.ipe or *.xml file.
Then run this script with as file argument the relative path to this file.
The script will then search for an open unity window and focus it.
It then gives you 3 seconds to select the Levelcontroller.Levelpoints.Size field in this unity window.
After which the script will simulate keypresses to enter the provided Ipe polygon.
"""

parser = argparse.ArgumentParser(description=description_text)
parser.add_argument('file', help="relative path to *.ipe or *.xml file containing the polygon")
args = parser.parse_args()


tree = ET.parse(args.file)
root = tree.getroot()
paths = root.findall("./page/path")

if len(paths) >1:
    raise Exception("File contains too many paths (lines/polygons)")

if len(paths) <1:
    raise Exception("No paths (lines/polygons) found in ipe file")

path = paths[0]
stripped_path = path.text.splitlines()[1:-1]

coords = []
maxx, minx, maxy, miny = -float("inf"),float("inf"),-float("inf"),float("inf")
count = len(stripped_path)
for i in stripped_path: #disregard first and last line since first line is empty and last line contains only the charachter h
    print(i)
    x, y, _ = i.split(" ")
    x= float(x)
    y= float(y)

    if x > maxx:
        maxx = x
    if x < minx:
        minx = x
    if y > maxy:
        maxy = y
    if y < miny:
        miny = y
    coords.append( (x,y) )

#normalize
coords = list(map(lambda t : ((t[0]-minx)*XSIZE/(maxx-minx) - XSIZE/2, (t[1]-miny)*YSIZE/(maxy-miny) -YSIZE/2) , coords))
coords = reversed(coords)

window = win32gui.FindWindow("UnityContainerWndClass",None)
try:
    win32gui.SetForegroundWindow(window)
except Exception as e:
    raise Exception("Couldn't find open Unity window") from e


time.sleep(8)

shell = win32com.client.Dispatch("WScript.Shell")
shell.SendKeys(str(count))
shell.SendKeys("{TAB}{TAB}")
for c in coords:
    shell.SendKeys(str(c[0]))
    shell.SendKeys("{TAB}")
    shell.SendKeys(str(c[1]))
    shell.SendKeys("{TAB}{TAB}")
