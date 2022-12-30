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

using System;
using System.Linq;
using JetBrains.Annotations;

namespace UnityVersionChangeset.Models
{
    /// <summary>
    /// Represents the version number of a Unity engine release. This class cannot be inherited.
    /// </summary>
    public sealed class UnityVersion : ICloneable, IComparable, IComparable<UnityVersion>, IEquatable<UnityVersion>
    {
        /// <summary>
        /// Type of build
        /// </summary>
        public enum VersionType
        {
            /// <summary>
            /// Release. Uses "f" letter in patch part
            /// </summary>
            Alpha = 0,
            
            /// <summary>
            /// Alpha. Uses "a" letter in patch part
            /// </summary>
            Beta,
            
            /// <summary>
            /// Beta. Uses "b" letter in patch part
            /// </summary>
            Release
        }
        
        /// <summary>
        /// Gets the value of the major component of the version number for the current UnityVersion object
        /// </summary>
        [PublicAPI]
        public int Major { get; private set; }
        
        /// <summary>
        /// Gets the value of the minor component of the version number for the current UnityVersion object
        /// </summary>
        [PublicAPI]
        public int Minor { get; private set; }
        
        /// <summary>
        /// Gets the value of the patch component of the version number for the current UnityVersion object
        /// </summary>
        [PublicAPI]
        public int Patch { get; private set; }
        
        /// <summary>
        /// Gets the revision value of the version number for the current UnityVersion object
        /// </summary>
        [PublicAPI]
        public int Revision { get; private set; }
        
        /// <summary>
        /// Gets the build type for the current UnityVersion object
        /// </summary>
        [PublicAPI]
        public VersionType Type { get; private set; }

        /// <summary>
        /// Initializes a new instance of the UnityVersion class
        /// </summary>
        /// <param name="major">Major component</param>
        /// <param name="minor">Minor component</param>
        /// <param name="patch">Patch component</param>
        /// <param name="revision">Revision value of the patch component</param>
        /// <param name="type">Build type</param>
        [PublicAPI]
        public UnityVersion(int major, int minor, int patch, int revision = 1, VersionType type = VersionType.Release)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Revision = revision;
            Type = type;
        }
        
        /// <summary>
        /// Initializes a new instance of the UnityVersion class and uses string representation of a version number
        /// </summary>
        /// <param name="version">Version to parse</param>
        [PublicAPI]
        public UnityVersion([NotNull] string version)
        {
            Parse(version);
        }

        public static implicit operator Version(UnityVersion version)
        {
            return new Version(version.Major, version.Minor, version.Patch);
        }

        public static explicit operator UnityVersion(Version version)
        {
            return new UnityVersion(version.Major, version.Minor, version.Build);
        }

        /// <summary>
        /// Determines whether two specified <see cref="UnityVersion">UnityVersion</see> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="UnityVersion">UnityVersion</see> object</param>
        /// <param name="right">The second <see cref="UnityVersion">UnityVersion</see> object</param>
        /// <returns>True if left equals right; otherwise, false.</returns>
        public static bool operator ==(UnityVersion left, UnityVersion right)
        {
            if (right is null)
                return left is null;
            
            return ReferenceEquals(left, right) || right.Equals(left);
        }
        
        /// <summary>
        /// Determines whether two specified <see cref="UnityVersion">UnityVersion</see> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="UnityVersion">UnityVersion</see> object</param>
        /// <param name="right">The second <see cref="UnityVersion">UnityVersion</see> object</param>
        /// <returns>True if left doesn't equals right; otherwise, false.</returns>
        public static bool operator !=(UnityVersion left, UnityVersion right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Determines whether the first specified <see cref="UnityVersion">UnityVersion</see> object is less than
        /// the second specified <see cref="UnityVersion">UnityVersion</see> object.
        /// </summary>
        /// <param name="left">The first <see cref="UnityVersion">UnityVersion</see> object</param>
        /// <param name="right">The second <see cref="UnityVersion">UnityVersion</see> object</param>
        /// <returns>True if left is less than right; otherwise, false.</returns>
        public static bool operator <(UnityVersion left, UnityVersion right)
        {
            if (left is null || right is null)
                return true;

            return left.CompareTo(right) < 0;
        }
        
        /// <summary>
        /// Determines whether the first specified <see cref="UnityVersion">UnityVersion</see> object is less than
        /// or equal to the second <see cref="UnityVersion">UnityVersion</see> object.
        /// </summary>
        /// <param name="left">The first <see cref="UnityVersion">UnityVersion</see> object</param>
        /// <param name="right">The second <see cref="UnityVersion">UnityVersion</see> object</param>
        /// <returns>True if left is less than or equal to right; otherwise, false.</returns>
        public static bool operator <=(UnityVersion left, UnityVersion right)
        {
            if (left is null || right is null)
                return true;

            return left.CompareTo(right) <= 0;
        }

        /// <summary>
        /// Determines whether the first specified <see cref="UnityVersion">UnityVersion</see> object is greater than
        /// the second specified <see cref="UnityVersion">UnityVersion</see> object.
        /// </summary>
        /// <param name="left">The first <see cref="UnityVersion">UnityVersion</see> object</param>
        /// <param name="right">The second <see cref="UnityVersion">UnityVersion</see> object</param>
        /// <returns>True if left is greater than right; otherwise, false.</returns>
        public static bool operator >(UnityVersion left, UnityVersion right)
        {
            return right < left;
        }
        
        /// <summary>
        /// Determines whether the first specified <see cref="UnityVersion">UnityVersion</see> object is greater than
        /// or equal to the second <see cref="UnityVersion">UnityVersion</see> object.
        /// </summary>
        /// <param name="left">The first <see cref="UnityVersion">UnityVersion</see> object</param>
        /// <param name="right">The second <see cref="UnityVersion">UnityVersion</see> object</param>
        /// <returns>True if left is greater than or equal to right; otherwise, false.</returns>
        public static bool operator >=(UnityVersion left, UnityVersion right)
        {
            return right <= left;
        }

        public override string ToString()
        {
            if (Type != VersionType.Release)
            {
                var revision = Type switch
                {
                    VersionType.Alpha => $"a{Revision}",
                    VersionType.Beta => $"b{Revision}",
                    _ => throw new ArgumentOutOfRangeException()
                };
            
                return $"{Major}.{Minor}.{Patch}{revision}";
            }
            
            return $"{Major}.{Minor}.{Patch}";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as UnityVersion);
        }
        
        public override int GetHashCode()
        {
            var accumulator = 0;

            accumulator |= (Major & 0xFF) << 28;
            accumulator |= (Minor & 0xFF) << 24;
            accumulator |= (Patch & 0xFF) << 20;
            accumulator |= (Revision & 0xFF) << 16;
            accumulator |= (int)Type & 0xF;

            return accumulator;
        }
        
        public bool Equals(UnityVersion other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (other == null)
                return false;

            return Major == other.Major && Minor == other.Minor && Patch == other.Patch && Revision == other.Revision &&
                   Type == other.Type;
        }

        public int CompareTo(object obj)
        {
            return obj switch
            {
                null => 1,
                UnityVersion version => CompareTo(version),
                _ => throw new ArgumentException($"Object must be {nameof(UnityVersion)}")
            };
        }
        
        public int CompareTo(UnityVersion version)
        {
            if (version.Equals(this))
                return 0;

            if (Major != version.Major)
                return Major > version.Major ? 1 : -1;
            
            if (Minor != version.Minor)
                return Minor > version.Minor ? 1 : -1;
            
            if (Patch != version.Patch)
                return Patch > version.Patch ? 1 : -1;

            if (Type != version.Type)
                return Type > version.Type ? 1 : -1;

            if (Revision != version.Revision)
                return Revision > version.Revision ? 1 : -1;
            
            return 0;
        }

        public object Clone()
        {
            return new UnityVersion(Major, Minor, Patch, Revision);
        }

        /// <summary>
        /// Converts the string representation of a version number to an equivalent UnityVersion object
        /// </summary>
        /// <param name="input">A string that contains a version number to convert</param>
        /// <exception cref="ArgumentNullException">input is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">At least one component in input is less than zero</exception>
        /// <exception cref="FormatException">At least one component in input is not an integer</exception>
        [PublicAPI]
        public void Parse([NotNull] string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentNullException(nameof(input), "Version can't be null or empty");
            
            var versionSplit = input.Split('.');

            if (versionSplit.Length < 3)
                throw new FormatException("Version must contain major, minor and patch parts");

            if (int.TryParse(versionSplit[0], out var major))
            {
                if (major < 0)
                    throw new ArgumentOutOfRangeException(nameof(major), "Major part is less than zero");
            }
            else
            {
                throw new FormatException("Major part is not an integer type");
            }

            if (int.TryParse(versionSplit[1], out var minor))
            {
                if (minor < 0)
                    throw new ArgumentOutOfRangeException(nameof(major), "Minor part is less than zero");
            }
            else
            {
                throw new FormatException("Minor part is not an integer type");
            }
            
            var patch = 0;
            var revision = 1;
            var type = VersionType.Release;
            var patchString = versionSplit[2];
            var revisionLetterChecks = 0;

            RevisionCheck('a', VersionType.Alpha);
            RevisionCheck('b', VersionType.Beta);
            RevisionCheck('f', VersionType.Release);

            if (revisionLetterChecks == 3)
                throw new FormatException($"Patch part is wrong: {patchString}.\nExpected letter values: a, b, f");

            Major = major;
            Minor = minor;
            Patch = patch;
            Revision = revision;
            Type = type;

            void RevisionCheck(char letter, VersionType expectedType)
            {
                if (!patchString.Contains(letter))
                {
                    if (patchString.Any(char.IsLetter))
                    {
                        revisionLetterChecks++;
                        return;
                    }
                    
                    if (int.TryParse(versionSplit[2], out patch))
                    {
                        if (patch < 0)
                            throw new ArgumentOutOfRangeException(nameof(patch), "Patch part is less than zero");
                    }
                    else
                    {
                        throw new FormatException("Patch part is not an integer type");
                    }
                    
                    return;
                }
                
                var patchSplit = patchString.Split(letter);
                
                if (versionSplit.Length < 2)
                    throw new FormatException("If patch part contains a letter, it must be at the middle between patch and revision numbers");
                
                if (int.TryParse(patchSplit[0], out patch))
                {
                    if (patch < 0)
                        throw new ArgumentOutOfRangeException(nameof(patch), "Patch number is less than zero");
                }
                else
                {
                    throw new FormatException("Patch number is not an integer type");
                }
                
                if (int.TryParse(patchSplit[1], out revision))
                {
                    if (revision < 0)
                        throw new ArgumentOutOfRangeException(nameof(revision), "Revision number is less than zero");
                }
                else
                {
                    throw new FormatException("Revision number is not an integer type");
                }
                
                type = expectedType;
            }
        }
    }
}