// Copyright 2017 Leung Wing-chung. All rights reserved.
// Use of this source code is governed by a GPLv3 license that can be found in
// the LICENSE file.

using TextDocument = ICSharpCode.AvalonEdit.Document.TextDocument;

namespace DotNetEditor.TextBuffer
{
    enum FileMode
    {
        VB,
        CS
    }

    class TextBuffer
    {
        public string Filename { get; set; }

        private TextDocument _document;
        public TextDocument Document
        {
            get { return _document; }
            set
            {
                _document = value;
                IsDirty = value != null && value.UndoStack.IsOriginalFile;
            }
        }

        public FileMode Language { get; set; }
        public bool IsDirty { get; set; }

        public TextBuffer(string filename, TextDocument document, FileMode language)
        {
            Filename = filename;
            Document = document;
            Language = language;
        }

        public string GetLanguageAsString()
        {
            return Language == FileMode.VB ? "VB" : "C#";
        }

        // Select a language from the file extension. Returns true if successful.
        public bool AutoSelectLanguage()
        {
            string ext =
                System.IO.Path.GetExtension(Filename).ToUpperInvariant();

            if (ext == ".VB")
            {
                Language = FileMode.VB;
                return true;
            }

            if (ext == ".CS")
            {
                Language = FileMode.CS;
                return true;
            }

            return false;
        }
    }
}
