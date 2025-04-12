/*
 * Copyright (C) .NET Foundation and Contributors
 *
 * All rights reserved.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

#pragma warning disable IDE1006, S1066, S1905

using System;

namespace HtmlPdf;

/// <summary>
///   Provides a cached reusable instance of <see cref="StringBuilder"/> per thread.
/// </summary>
internal static class StringBuilderCache
{
    internal const int MaxBuilderSize = 32 * 1024;
    private const int DefaultCapacity = 8192;

    [ThreadStatic]
    private static StringBuilder? t_cachedInstance;

    /// <summary>
    ///   Get a <see cref="StringBuilder"/> for the specified capacity.
    /// </summary>
    /// <remarks>
    ///   If a <see cref="StringBuilder"/> of an appropriate size is cached, it will be returned and the cache emptied.
    /// </remarks>
    public static StringBuilder Acquire(int capacity = DefaultCapacity)
    {
        if (capacity <= MaxBuilderSize)
        {
            var sb = t_cachedInstance;
            if (sb is not null)
            {
                // Avoid StringBuilder block fragmentation by getting a new StringBuilder
                // when the requested size is larger than the current capacity
                if (capacity <= sb.Capacity)
                {
                    t_cachedInstance = null;
                    _ = sb.Clear();
                    return sb;
                }
            }
        }

        return new StringBuilder(capacity);
    }

    /// <summary>
    ///   Place the specified builder in the cache if it is not too big.
    /// </summary>
    public static void Release(StringBuilder sb)
    {
        if (sb.Capacity <= MaxBuilderSize)
        {
            t_cachedInstance = sb;
        }
    }

    /// <summary>
    ///   ToString() the <see cref="StringBuilder"/>, Release it to the cache, and return the resulting string.
    /// </summary>
    public static string GetStringAndRelease(StringBuilder sb)
    {
        var result = sb.ToString();
        Release(sb);
        return result;
    }
}
