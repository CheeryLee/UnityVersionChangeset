/* MIT License

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

namespace UnityVersionChangeset.Models
{
    /// <summary>
    /// Possible result status 
    /// </summary>
    public enum ResultStatus
    {
        /// <summary>
        /// Everything is OK
        /// </summary>
        Ok = 0,
        
        /// <summary>
        /// Probably Unity version hasn't been found
        /// </summary>
        NotFound,
        
        /// <summary>
        /// Something happened to HTTP connection
        /// </summary>
        HttpError,
        
        /// <summary>
        /// No valid regex output
        /// </summary>
        RegexNoValue,
        
        /// <summary>
        /// Unknown type of error
        /// </summary>
        UnknownError
    }
    
    /// <summary>
    /// A structure that contains the status of requested operation and it's result
    /// </summary>
    /// <typeparam name="T">Result type</typeparam>
    public struct RequestResult<T>
    {
        /// <summary>
        /// Status value
        /// </summary>
        public ResultStatus Status;
        
        /// <summary>
        /// Result value
        /// </summary>
        public T Result;
    }
}