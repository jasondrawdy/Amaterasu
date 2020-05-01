/*
==============================================================================
Copyright © Jason Drawdy 

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
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Collections.Generic;

#endregion
namespace Amaterasu
{
    internal class LocalSystem
    {
        #region Variables

        private static bool IsWorking = false;
        /// <summary>
        /// The web address that should be used to obtain a list of files to remove from the system.
        /// </summary>
        public string Website { get; set; }
        /// <summary>
        /// A collection of files that should be removed from the system.
        /// </summary>
        public BlacklistContainer Blacklist { get; set; }

        #endregion
        #region Initialization

        /// <summary>
        /// Allows access to methods associated with the local Windows environment.
        /// </summary>
        public LocalSystem() { }
        /// <summary>
        /// Allows the enumeration of a blacklist through a web address represented as a <see cref="string"/>.
        /// </summary>
        /// <param name="website">The web address that will be used for enumeration.</param>
        public LocalSystem(string website)
        {
            Website = website;
        }
        /// <summary>
        /// Allows the enumeration of a blacklist through a web address represented as a <see cref="Uri"/>.
        /// </summary>
        /// <param name="website"></param>
        public LocalSystem(Uri website)
        {
            Website = website.ToString();
        }
        /// <summary>
        /// Initializes a <see cref="LocalSystem"/> object with a collection of blacklisted files to removed from the system.
        /// </summary>
        /// <param name="container"></param>
        public LocalSystem(BlacklistContainer container)
        {
            Blacklist = container;
        }

        #endregion
        #region Methods

        /// <summary>
        /// Searches the current system for files contained within a blacklist and attempts to remove them.
        /// </summary>
        /// <param name="container">A collection of files to search the system for.</param>
        internal void CheckSystem(BlacklistContainer container = null)
        {
            // Check the current system for blacklisted files.
            if (!IsDebugging())
            {
                if (container != null)
                {
                    if (IsAdmin())
                    {
                        IsWorking = true;
                        Thread t = new Thread(() => Search(Blacklist.Files));
                        t.Start();

                        // Let the previous thread finish before continuing.
                        while (IsWorking) { Thread.Sleep(100); }
                    }
                }
            }
            else
                throw new Exception("The process is currently being debugged.");

            // Reset our flags.
            IsWorking = false;
        }

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);
        /// <summary>
        /// Checks if the current process is being debugged by an external program.
        /// </summary>
        internal bool IsDebugging()
        {
            // Check if someone is trying to debug us.
            bool isDebuggerPresent = false;
            CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref isDebuggerPresent);
            if (isDebuggerPresent)
                return true;
            if (Debugger.IsAttached)
                return true;
            return false;
        }

        /// <summary>
        /// Checks if the current user has administrative privileges.
        /// </summary>
        internal bool IsAdmin()
        {
            // Try to create a directory in an elevated area to check process rights.
            try
            {
                string path = @"C:\Program Files\Temp";
                Directory.CreateDirectory(path);
                Directory.Delete(path);
                return true;
            }
            catch { return false; }
        }
        
        /// <summary>
        /// Search for a list of files in a blacklist and remove them from the system.
        /// </summary>
        /// <param name="blacklist">An array of files to remove from the system.</param>
        private void Search(string[] blacklist)
        {
            // Indentify and rectify files contained within the blacklist.
            if (blacklist != null)
            {
                Cryptography.Hashing h = new Cryptography.Hashing();
                const long max = 104857600;
                string program32 = @"C:\Program Files(x86)";
                string program64 = @"C:\Program Files";
                string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                // Search 32bit files first.
                foreach (string file in Directory.EnumerateFileSystemEntries(program32, "*.*", SearchOption.AllDirectories))
                {
                    // Check if the file is a certain size.
                    FileInfo fi = new FileInfo(file);
                    if (fi.Length < max)
                    {
                        // Check the the file against the blacklist.
                        string hash = h.ComputeFileHash(file, Cryptography.Hashing.HashType.MD5);
                        foreach (string checksum in blacklist)
                        {
                            // Delete the file if it's blacklisted.
                            if (hash == checksum)
                            {
                                try
                                {
                                    File.Delete(file);
                                    break;
                                }
                                catch { break; } // The file was found but couldn't be deleted. 
                            }
                        }
                    }
                }

                // Search 64bit files next.
                foreach (string file in Directory.EnumerateFiles(program64, "*.*", SearchOption.AllDirectories))
                {
                    // Check if the file is a certain size.
                    FileInfo fi = new FileInfo(file);
                    if (fi.Length < max)
                    {
                        // Check the the file against the blacklist.
                        string hash = h.ComputeFileHash(file, Cryptography.Hashing.HashType.MD5);
                        foreach (string checksum in blacklist)
                        {
                            // Delete the file if it's blacklisted.
                            if (hash == checksum)
                            {
                                try
                                {
                                    File.Delete(file);
                                    break;
                                }
                                catch { break; } // The file was found but couldn't be deleted. 
                            }
                        }
                    }
                }

                // Lastly search the user directory.
                foreach (string file in Directory.EnumerateFiles(userPath, "*.*", SearchOption.AllDirectories))
                {
                    // Check if the file is a certain size.
                    FileInfo fi = new FileInfo(file);
                    if (fi.Length < max)
                    {
                        // Check the the file against the blacklist.
                        string hash = h.ComputeFileHash(file, Cryptography.Hashing.HashType.MD5);
                        foreach (string checksum in blacklist)
                        {
                            // Delete the file if it's blacklisted.
                            if (hash == checksum)
                            {
                                try
                                {
                                    File.Delete(file);
                                    break;
                                }
                                catch { break; } // The file was found but couldn't be deleted. 
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Obtains a blacklist from a webserver using a PHP script and the provided <see cref="RSACryptoServiceProvider"/> public key.
        /// </summary>
        /// <param name="key">The public key to used to use for cryptographic transformations.</param>
        private string[] GetBlacklist(string key)
        {
            // Import our public key.
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(key);

            // Encrypt a chunk of data so our server can verify it's really us.
            string data = (rsa.Encrypt(("Hello").GetBytes(), false)).GetString();
            NetRequest request = new NetRequest(Website, NetRequest.RequestType.POST, "chunk=" + data);
            string response = request.GetResponse();

            // Try to decrypt the server's response so we can verify it's really the server.
            try
            {
                string decrypted = (rsa.Decrypt(response.GetBytes(), false)).GetString();
                string[] blacklist = decrypted.Split('|');
                return blacklist;
            }
            catch { return null; }
        }

        #endregion
    }
}
