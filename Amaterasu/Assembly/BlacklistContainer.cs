/*
==============================================================================
Copyright © Jason Drawdy (CloneMerge)

All rights reserved.

The MIT License (MIT)

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
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

Except as contained in this notice, the name of the above copyright holder
shall not be used in advertising or otherwise to promote the sale, use or
other dealings in this Software without prior written authorization.
==============================================================================
*/

#region Imports

using System;
using System.Collections.Generic;

#endregion
namespace Amaterasu
{
    /// <summary>
    /// Holds a collection of files that should be removed from the local computer system.
    /// </summary>
    internal class BlacklistContainer
    {
        #region Variables

        private List<string> _files { get; set; }
        /// <summary>
        /// Collection of files that should be removed from the local computer system.
        /// </summary>
        public string[] Files { get { return _files.ToArray(); } }

        #endregion
        #region Initialization

        /// <summary>
        /// A collection of files that should be removed from the system.
        /// </summary>
        public BlacklistContainer() { }
        /// <summary>
        /// A collection of files that should be removed from the system.
        /// </summary>
        /// <param name="files">The files that should be removed.</param>
        public BlacklistContainer(string[] files) { _files.AddRange(files); }
        /// <summary>
        /// A collection of files that should be removed from the system.
        /// </summary>
        /// <param name="files">The files that should be removed.</param>
        public BlacklistContainer(List<string> files) { _files = files; }

        #endregion
        #region Methods

        /// <summary>
        /// Adds a file to the blacklist for later removal.
        /// </summary>
        /// <param name="file">The path of the file to be added to the container.</param>
        public bool Add(string file)
        {
            if (!_files.Contains(file))
                _files.Add(file);
            else
                throw new Exception("File already exists in the container.");
            return true;
        }
        /// <summary>
        /// Removes a file from the blacklist container.
        /// </summary>
        /// <param name="file">The file that should be removed from the container.</param>
        public bool Remove(string file)
        {
            if (_files.Count > 0)
            {
                if (_files.Contains(file))
                    _files.Remove(file);
                else
                    throw new Exception("File does not exist within the container.");
                return true;
            }
            else
                throw new Exception("The container does not contain any files to remove.");
        }
        /// <summary>
        /// Removes a file from the blacklist container at a specific index.
        /// </summary>
        /// <param name="index">The index number of the file to be removed.</param>
        public bool RemoveAt(int index)
        {
            if (_files.Count > 0)
            {
                if (index <= _files.Count)
                {
                    _files.RemoveAt(index);
                    return true;
                }
                else
                    throw new Exception("The index is larger than the amount of files in the container.");
            }
            else
                throw new Exception("The container does not contain any files to remove.");
        }

        #endregion
    }
}
