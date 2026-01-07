using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

//
// Program to sort a large number of mixed photographs (.JPG, .ARW (sony RAW files)) into folders.
//
// Image date taken is read from EXIF and the file is moved in folder named with YYYY-DD-MM format.
// Allows RAWs and JPGs to be separated into different folders (requires uncommenting / recompiling as i was too lazy to provide a command line switch).
// Allows files to be processed in-place (i.e. source and dest folder are same).
//  Useful if you want to switch between RAW in separate folder vs RAWs in same folder as JPG.
//
// Usage: Sort-Images.exe <directory> <optional dst_directory>
//
// Since Images have sentimental value, the code/APIs used are carefully written/selected and guaranted
// to not corrupt, misplace, overwrite or delete files.
//

namespace Sort_Images {
    class Program {
        static void Main(string[] args)
        {
            if (args.Length == 1) {
                var path = args[0];

                if (File.Exists(path)) {
                    // This path is a file
                    // ProcessFile(path);
                    Console.WriteLine("{0} is not a valid directory.", path);
                }
                else if (System.IO.Directory.Exists(path)) {
                    // This path is a directory
                    ProcessDirectoryInplace(path);
                }
                else {
                    Console.WriteLine("{0} is not a valid directory.", path);
                }
            }
            else if (args.Length == 2) {
                var srcPath = args[0];
                var dstPath = args[1];

                if (File.Exists(srcPath) || File.Exists(dstPath)) {
                    Console.WriteLine("Not a valid directory.");
                }
                else if (System.IO.Directory.Exists(srcPath)) {
                    // This path is a directory
                    ProcessDirectory(srcPath, dstPath);
                }
                else {
                    Console.WriteLine("{0} is not a valid directory.", srcPath);
                }
            }
            else {
                Console.WriteLine("Usage: Sort-Images <directory> <optional dst_directory>");
            }
        }

        public static void ProcessDirectory(string srcDir, string targetDir)
        {
            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = System.IO.Directory.GetDirectories(srcDir);
            foreach (string subdirectory in subdirectoryEntries) {
                ProcessDirectory(subdirectory, targetDir);
            }

            // Process the list of files found in the directory.
            string[] fileEntries = System.IO.Directory.GetFiles(srcDir);
            foreach (string fileName in fileEntries) {
                ProcessFile(fileName, targetDir);
            }
        }

        static void ProcessFile(string filePath, string targetDir)
        {
            try {
                var directories = ImageMetadataReader.ReadMetadata(filePath);
                var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().LastOrDefault();

                DateTime dateTime = subIfdDirectory.GetDateTime(ExifDirectoryBase.TagDateTimeOriginal);

                FileInfo fi = new FileInfo(filePath);
                string fileName = fi.Name;    // with extension

                string year = dateTime.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);

                //
                // Uncomment to split RAW files in a separate \ARW directory.
                //
                //if (fi.Extension.Equals(".ARW", StringComparison.InvariantCultureIgnoreCase))
                //{
                //   string targetPath = Path.Combine(targetDir, year + @"\ARW");
                //   System.IO.Directory.CreateDirectory(targetPath);

                //   File.Move(filePath, System.IO.Path.Combine(targetPath, fileName));
                //}
                //else
                {
                    string targetPath = Path.Combine(targetDir, year);
                    System.IO.Directory.CreateDirectory(targetPath);

                    File.Move(filePath, System.IO.Path.Combine(targetPath, fileName));
                }
            } catch (Exception) {
                Console.WriteLine("Unable to process: {0}", filePath);
            };
        }

        //
        // Allows the source and destination to be the same directory. This allows moving
        // files in their right place. For ex. If RAW's were in separate folder but user
        // wants to merge them back with JPG or vice versa.
        //
        public static void ProcessDirectoryInplace(string directory)
        {
            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = System.IO.Directory.GetDirectories(directory);
            foreach (string subdirectory in subdirectoryEntries) {
                ProcessDirectoryInplace(subdirectory);
            }

            // Process the list of files found in the directory.
            string[] fileEntries = System.IO.Directory.GetFiles(directory);
            foreach (string fileName in fileEntries) {
                ProcessFileInplace(fileName);
            }

            if (System.IO.Directory.GetFiles(directory).Length == 0
                && System.IO.Directory.GetDirectories(directory).Length == 0) {
                Console.WriteLine("Deleting {0}", directory.ToString());
                System.IO.Directory.Delete(directory, false);
            }
        }

        static void ProcessFileInplace(string filePath)
        {
            try {
                var directories = ImageMetadataReader.ReadMetadata(filePath);
                var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().LastOrDefault();

                DateTime dateTime = subIfdDirectory.GetDateTime(ExifDirectoryBase.TagDateTimeOriginal);

                FileInfo fi = new FileInfo(filePath);
                string sourcePath = fi.DirectoryName;
                string fileName = fi.Name;

                string currentDirectoryName = fi.Directory.Name;
                string expectedDirectoryName = dateTime.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);

                //
                // Uncomment to move RAW files to a separate ARW folder. Could be a command line switch
                //
                //if (fi.Extension.Equals(".ARW", StringComparison.InvariantCultureIgnoreCase))
                //{
                //   // For RAW files, if the file is already in YYYY-MM-DD folder, move it to YYYY-MM-DD/ARW folder
                //   if (sourcePath.EndsWith(expectedDirectoryName))
                //   {
                //      string targetPath = Path.Combine(sourcePath, "ARW");
                //      System.IO.Directory.CreateDirectory(targetPath);

                //      File.Move(filePath, System.IO.Path.Combine(targetPath, fileName));
                //   }
                //   else if (!sourcePath.EndsWith(expectedDirectoryName + @"\ARW"))
                //   {
                //      string targetPath = Path.Combine(sourcePath, expectedDirectoryName + @"\ARW");
                //      System.IO.Directory.CreateDirectory(targetPath);

                //      File.Move(filePath, System.IO.Path.Combine(targetPath, fileName));
                //   }
                //}
                //else
                {
                    // check if the parent director is already in YYYY-MM-DD format
                    if (!sourcePath.EndsWith(expectedDirectoryName)) {
                        string targetPath = Path.Combine(sourcePath, expectedDirectoryName);
                        System.IO.Directory.CreateDirectory(targetPath);

                        Console.WriteLine("Moving {0} to {1}", filePath, targetPath);
                        File.Move(filePath, System.IO.Path.Combine(targetPath, fileName));
                    }
                }
            } catch (Exception) {
                Console.WriteLine("Unable to process: {0}", filePath);
            };
        }


        // Moves file under Vidoe(s)/ARW to parent directory, so all video/raw/jpg files are in a single folder.
        //static void ProcessFileInplace(string filePath)
        //{
        //    try {
        //        FileInfo fi = new FileInfo(filePath);
        //        string sourcePath = fi.DirectoryName;
        //        string fileName = fi.Name;
        //        string currentDirectoryName = fi.Directory.Name;

        //        if (fi.Extension.Equals(".XMP", StringComparison.InvariantCultureIgnoreCase)) {
        //            // For sidecar file, if the file is already in YYYY-MM-DD/ARW folder, move it to parent folder
        //            if (sourcePath.EndsWith(@"\ARW")) {
        //                string targetPath = System.IO.Directory.GetParent(sourcePath).FullName;
        //                Console.WriteLine("Moving {0} to {1}", filePath, targetPath);
        //                File.Move(filePath, System.IO.Path.Combine(targetPath, fileName));
        //            }
        //        }
        //        else if (fi.Extension.Equals(".MP4", StringComparison.InvariantCultureIgnoreCase)) {
        //            // For video file, if the file is already in /Video(s) folder, move it to parent folder
        //            if (sourcePath.EndsWith(@"\Video") || sourcePath.EndsWith(@"\Videos")) {
        //                string targetPath = System.IO.Directory.GetParent(sourcePath).FullName;
        //                Console.WriteLine("Moving {0} to {1}", filePath, targetPath);
        //                File.Move(filePath, System.IO.Path.Combine(targetPath, fileName));
        //            }
        //        }
        //        else if (fi.Extension.Equals(".ARW", StringComparison.InvariantCultureIgnoreCase)) {
        //            // For RAW files, if the file is in /ARW folder, move it to parent folder

        //            if (sourcePath.EndsWith(@"\ARW")) {
        //                //var directories = ImageMetadataReader.ReadMetadata(filePath);
        //                //var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().LastOrDefault();
        //                //DateTime dateTime = subIfdDirectory.GetDateTime(ExifDirectoryBase.TagDateTimeOriginal);
        //                //string expectedDirectoryName = dateTime.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);

        //                string targetPath = System.IO.Directory.GetParent(sourcePath).FullName;
        //                Console.WriteLine("Moving {0} to {1}", filePath, targetPath);
        //                File.Move(filePath, System.IO.Path.Combine(targetPath, fileName));
        //            }
        //        }
        //        //else {
        //        //    // check if the parent director is already in YYYY-MM-DD format
        //        //    if (!sourcePath.EndsWith(expectedDirectoryName)) {
        //        //        string targetPath = Path.Combine(sourcePath, expectedDirectoryName);
        //        //        System.IO.Directory.CreateDirectory(targetPath);

        //        //        File.Move(filePath, System.IO.Path.Combine(targetPath, fileName));
        //        //    }
        //        //}
        //    } catch (Exception) {
        //        Console.WriteLine("Unable to process: {0}", filePath);
        //    };
        //}
    }
}