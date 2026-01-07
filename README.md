# Sort-Images

A simple tool to sort a large number of mixed photographs (.JPG, .ARW (sony RAW files)) into a clean folder structure based on date the images were taken.

Usage: Sort-Images.exe <source_directory> <optional_destination_directory>

* Image date taken is read from EXIF, a folder with YYYY-DD-MM format is created (if not already existing) and the file is moved into the folder.
* Allows RAWs to be separated from JPGs in a separate folder (requires uncommenting / recompiling as i was too lazy to provide a command line switch).
* Allows files to be processed in-place (i.e. source and dest folder are same). Useful if you want to switch between RAW in separate folder vs RAWs in same folder as JPG.

Since Images have sentimental value and I used this program to sort my own pictures, the code/APIs used are carefully written/selected and guaranteed to not corrupt, misplace, overwrite or delete files.

I wrote this program when I need to sort 40 thousand+ of my own files lying around in multiple harddrives into one clean structure.

This is how the destination folder will look like after the program is done sorting files:

<img width="96" height="169" alt="image" src="https://github.com/user-attachments/assets/cb8e2431-4bda-4d06-ab02-a335f0df7153" />
