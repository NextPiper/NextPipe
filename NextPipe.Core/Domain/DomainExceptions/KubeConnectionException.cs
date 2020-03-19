using System;
using Microsoft.VisualBasic;

namespace NextPipe.Core.Documents
{
    public class KubeConnectionException : Exception
    {
        public KubeConnectionException(string msg) : base(msg)
        {
            Console.WriteLine(msg);
        }
    }
}