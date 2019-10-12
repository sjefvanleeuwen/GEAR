﻿using ST.Files.Abstraction.Models;

namespace ST.Files.Box.Abstraction.Models
{
    public sealed class FileBox : File
    {
        /// <summary>
        /// Path
        /// </summary>
        public string Path { get; set; }
    }
}