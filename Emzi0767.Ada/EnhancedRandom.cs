// This file is part of ADA project
//
// Copyright 2018 Emzi0767
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using Emzi0767.Ada.Services;

namespace Emzi0767.Ada
{
    /// <summary>
    /// Enhanced implementation of <see cref="Random"/>, utilising a CSPRNG to provide higher-quality randomness.
    /// </summary>
    public sealed class EnhancedRandom : Random
    {
        private CSPRNG InternalRng { get; }

        /// <summary>
        /// Instantiates a new enhanced random number generator.
        /// </summary>
        /// <param name="csprng">CSPRNG used to provide random data.</param>
        public EnhancedRandom(CSPRNG csprng)
        {
            this.InternalRng = csprng;
        }

        /// <summary>
        /// Generates a random <see cref="int"/> between <paramref name="minValue"/> and <paramref name="maxValue"/> (lower bound inclusive).
        /// </summary>
        /// <param name="minValue">Minimum value to generate (inclusive).</param>
        /// <param name="maxValue">Maximum value to generate (exclusive).</param>
        /// <returns>Generated random number.</returns>
        public override int Next(int minValue, int maxValue)
            => this.InternalRng.Next(minValue, maxValue);

        /// <summary>
        /// Generates a random <see cref="int"/> between 0 and <paramref name="maxValue"/> (lower bound inclusive).
        /// </summary>
        /// <param name="maxValue">Maximum value to generate (exclusive).</param>
        /// <returns>Generated random number.</returns>
        public override int Next(int maxValue)
            => this.Next(0, maxValue);

        /// <summary>
        /// Generates a random <see cref="int"/> between 0 and <see cref="int.MaxValue"/> (lower bound inclusive).
        /// </summary>
        /// <returns>Generated random number.</returns>
        public override int Next()
            => this.Next(0, int.MaxValue);

        /// <summary>
        /// Generates a random <see cref="double"/> between 0.0 and 1.0 (lower bound inclusive).
        /// </summary>
        /// <returns>Generated random number.</returns>
        public override double NextDouble()
            => this.InternalRng.NextDouble();

        /// <summary>
        /// Generates random bytes and fills the supplied array with the result.
        /// </summary>
        /// <param name="buffer">Buffer to fill with random bytes.</param>
        public override void NextBytes(byte[] buffer)
            => this.InternalRng.GetBytes(buffer);

        /// <summary>
        /// Generates random bytes and fills the supplied span with the result.
        /// </summary>
        /// <param name="buffer">Span to fill with random bytes.</param>
        public override void NextBytes(Span<byte> buffer)
            => this.InternalRng.GetBytes(buffer);
    }
}
