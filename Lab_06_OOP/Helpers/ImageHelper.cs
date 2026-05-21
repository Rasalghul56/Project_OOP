using System;
using System.IO;

namespace Confectionery.Helpers
{


    public static class ImageHelper
    {

        public static string ImagesDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Confectionery", "Images");


        public static string GetFullPath(string storedValue)
        {
            if (string.IsNullOrWhiteSpace(storedValue))
                return null;


            if (Path.IsPathRooted(storedValue))
                return storedValue;


            return Path.Combine(ImagesDirectory, storedValue);
        }
    }
}
