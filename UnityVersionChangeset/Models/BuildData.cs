﻿/* MIT License

Copyright (c) 2022 Alexander Pluzhnikov

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE. */

using System;
using System.Collections.Generic;

namespace UnityVersionChangeset.Models
{
    /// <summary>
    /// Data class that contains the info about one version of engine
    /// </summary>
    public class BuildData
    {
        /// <summary>
        /// The version of engine
        /// </summary>
        public UnityVersion Version { get; internal set; }
        
        /// <summary>
        /// Release date. Contains only date, the time always equals to midnight.
        /// </summary>
        public DateTime ReleaseDate { get; internal set; }
        
        /// <summary>
        /// Installable modules
        /// </summary>
        internal List<ModuleData> Modules { get; set; }
        
        /// <summary>
        /// Build changeset sum
        /// </summary>
        internal string ChangeSet { get; set; }
    }
}