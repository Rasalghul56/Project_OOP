using System;
using System.IO;

namespace Confectionery.Helpers
{
    /// <summary>
    /// Centralized helper for product image paths.
    /// Only the filename (e.g. "a3b4c5.jpg") is stored in the database.
    /// The full path is resolved at runtime so the app works correctly
    /// on any PC regardless of the username or install location.
    /// </summary>
    public static class ImageHelper
    {
        /// <summary>Folder where product images are stored on this machine.</summary>
        public static string ImagesDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Confectionery", "Images");

        /// <summary>
        /// Converts a stored value (filename or legacy absolute path) to a full absolute path.
        /// Returns null if the input is null or empty.
        /// </summary>
        public static string GetFullPath(string storedValue)
        {
            if (string.IsNullOrWhiteSpace(storedValue))
                return null;

            // Legacy: value already contains a full path (older records)
            if (Path.IsPathRooted(storedValue))
                return storedValue;

            // New: value is just a filename → resolve against ImagesDirectory
            return Path.Combine(ImagesDirectory, storedValue);
        }
    }
}
