using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DirectoryPropSwitch.internals
{
    internal static class FileBomDetector
    {
        private const int PreambleLength = 3;
        private static readonly byte[] Preambles = new byte[] { 239, 187, 191 };

        public static Encoding Detect(string pathToFile)
        {
            int bufl = 0;
            var buf = ArrayPool<byte>.Shared.Rent(PreambleLength);
            try
            {
                return DetectCore(pathToFile, buf, bufl);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buf);
            }
        }

        private static Encoding DetectCore(string pathToFile, byte[] buf, int bufl)
        {
            using (var reader = File.OpenRead(pathToFile))
            {
                bufl = reader.Read(buf, 0, buf.Length);
            }
            var isBom = IsBom(buf.AsSpan());
            return new UTF8Encoding(isBom);
        }

        public static bool IsBom(ReadOnlySpan<byte> fileBytes)
        {
            var file = fileBytes.Slice(0, PreambleLength);
            return file.SequenceEqual(Preambles);
        }
    }

    internal static class FileEolDetector
    {
        public static EolStyle Detect(string pathToFile)
        {
            int bufl = 0;
            var buf = ArrayPool<char>.Shared.Rent(8192);
            try
            {
                return DetectCore(pathToFile, buf, bufl);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buf);
            }
        }

        private static EolStyle DetectCore(string pathToFile, char[] buf, int bufl)
        {
            using (var reader = File.OpenText(pathToFile))
            {
                bufl = reader.ReadBlock(buf, 0, buf.Length);
            }

            var (crlf, cr, lf) = (0, 0, 0);
            for (int i = 0; i < bufl;)
            {
                if (buf[i] == '\r' && i < bufl - 1 && buf[i + 1] == '\n') { ++crlf; i += 2; }
                else if (buf[i] == '\r') { ++cr; i += 1; }
                else if (buf[i] == '\n') { ++lf; i += 1; }
                else { i++; }
            }

            EolStyle style;
            if (crlf > cr && crlf > lf) style = EolStyle.Windows;
            else if (lf > crlf && lf > cr) style = EolStyle.Unix;
            else if (cr > crlf && cr > lf) style = EolStyle.MacOs;
            else style = EolStyle.Unknown;

            return style;
        }

        public static string TrimEnd(ReadOnlySpan<char> span, string line)
        {
            if (((byte)line[line.Length - 1]) == 13)
            {
                // cr
                var result = span.Slice(0, span.Length).ToString();
                return result;
            }
            else if (((byte)line[line.Length - 1]) == 10)
            {
                if (((byte)line[line.Length - 2]) == 13)
                {
                    // crlf
                    var result = span.Slice(0, span.Length - 1).ToString();
                    return result;
                }
                else
                {
                    // lf
                    var result = span.Slice(0, span.Length).ToString();
                    return result;
                }
            }
            else
            {
                return span.ToString();
            }
        }

        public static string AddLast(string path, ReadOnlySpan<char> span, string line)
        {
            var eol = Detect(path).GetLabel();
            if (((byte)line[line.Length - 1]) == 13)
            {
                // cr
                var result = span.Slice(0, span.Length).ToString() + eol;
                return result;
            }
            else if (((byte)line[line.Length - 1]) == 10)
            {
                if (((byte)line[line.Length - 2]) == 13)
                {
                    // crlf
                    var result = span.Slice(0, span.Length - 1).ToString() + eol;
                    return result;
                }
                else
                {
                    // lf
                    var result = span.Slice(0, span.Length).ToString() + eol;
                    return result;
                }
            }
            else
            {
                return span.ToString() + eol;
            }
        }
    }
}
