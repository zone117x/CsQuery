﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CsQuery.StringScanner;

namespace CsQuery.Output
{
    /// <summary>
    /// Abstract base class for custom HTML encoder implementations
    /// </summary>

    public class HtmlEncoderBase: IHtmlEncoder
    {
        /// <summary>
        /// Determines of a character must be encoded; if so, encodes it as the output parameter and
        /// returns true; if not, returns false.
        /// </summary>
        ///
        /// <param name="c">
        /// The text string to encode.
        /// </param>
        /// <param name="encoded">
        /// [out] The encoded string.
        /// </param>
        ///
        /// <returns>
        /// True if the character was encoded.
        /// </returns>

        protected virtual bool TryEncode(char c, out string encoded);

        public virtual void Encode(string html, TextWriter output)
        {
            StringBuilder sb = new StringBuilder();
            int pos = 0,
                len = html.Length;

            while (pos < len)
            {
                char c = html[pos++];
                string encoded;
                if (TryEncode(c, out encoded))
                {
                    output.Write(encoded);
                }
                else
                {
                    output.Write(c);
                }
            }
        }

        public virtual void Decode(string value, TextWriter output)
        {
            System.Web.HttpUtility.HtmlDecode(value, output);
        }
    }
}
