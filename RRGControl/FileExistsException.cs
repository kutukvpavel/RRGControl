using System;

namespace RRGControl
{
    public class FileExistsException : Exception
    {
        public FileExistsException() : base() { }
        public FileExistsException(string path) : base($"File already exists: {path}") { }
    }
}