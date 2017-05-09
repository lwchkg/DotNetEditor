// Copyright 2017 Leung Wing-chung. All rights reserved.
// Use of this source code is governed by a GPLv3 license that can be found in
// the LICENSE file.

using System;
using System.IO;

namespace DotNetEditor.FileUtils
{
    // Handles file loading and saving actions. A load and a save action is required to instantiate
    // the class. If file operation fails, the action should throw an System.IO.IOException for this
    // class to catch.
    class FileHandler
    {
        public delegate void FileOperation(Stream stream);

        System.Windows.Window _window;
        FileOperation _load;
        FileOperation _save;

        public struct FileOperationResult
        {
            public bool Success;
            public string Filename;

            public FileOperationResult(bool success, string filename)
            {
                this.Success = success;
                this.Filename = filename;
            }
        }

        // Return for cancelled or failed file operations.
        private static readonly FileOperationResult _fileOperationNotSucceed =
            new FileOperationResult(false, "");

        public FileHandler(System.Windows.Window window,
            FileOperation loadAction, FileOperation saveAction)
        {
            _window = window;
            _load = loadAction;
            _save = saveAction;
        }

        public FileOperationResult Load(string filename)
        {
            try
            {
                _load(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read));
            }
            catch (IOException)
            {
                // TODO: display dialog for failure.
                return _fileOperationNotSucceed;
            }
            catch (UnauthorizedAccessException)
            {
                return _fileOperationNotSucceed;
            }
            return new FileOperationResult(true, filename);
        }

        public FileOperationResult LoadWithDialog(string filter)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = filter;

            if (dialog.ShowDialog(_window) == true)
            {
                return Load(dialog.FileName);
            }
            else
            {
                return _fileOperationNotSucceed;
            }
        }

        public FileOperationResult Save(string filename)
        {
            try
            {
                _save(new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Write));
            }
            catch (System.IO.IOException)
            {
                // TODO: display dialog for failure.
                return _fileOperationNotSucceed;
            }
            return new FileOperationResult(true, filename);
        }


        public FileOperationResult SaveAs(string filter)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = filter;

            if (dialog.ShowDialog(_window) == true)
            {
                return Save(dialog.FileName);
            }
            else
            {
                return _fileOperationNotSucceed;
            }
        }
    }
}
