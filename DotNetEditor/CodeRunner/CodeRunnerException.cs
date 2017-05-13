// Copyright 2017 Leung Wing-chung. All rights reserved.
// Use of this source code is governed by a GPLv3 license that can be found in
// the LICENSE file.

using System;

namespace DotNetEditor.CodeRunner
{
    [Serializable]
    class CodeRunnerException : Exception
    {
        public CodeRunnerException() { }
        public CodeRunnerException(string message) : base(message) { }
        public CodeRunnerException(string message, Exception e) : base(message, e) { }
    }
}
