/*
  FarseerPhysics Copyright (c) 2009 Matthew Bettcher
  Original Parallel class version Copyright (c) 2008 Martin Konicek http://coding-time.blogspot.com/2008/03/implement-your-own-parallelfor-in-c.html

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.
*/

using System;

namespace FarseerPhysics
{
    public delegate void ForDelegate(int i);

    public delegate void ThreadDelegate();


    public class Parallel
    {
        /// <summary>
        /// Parallel for loop. Invokes given action, passing arguments
        /// fromInclusive - toExclusive on multiple threads.
        /// Returns when loop finished.
        /// </summary>
        public static void For(int fromInclusive, int toExclusive, ForDelegate action)
        {
            // ChunkSize = 1 makes items to be processed in order.

            // Bigger chunk size should reduce lock waiting time and thus

            // increase paralelism.

            int chunkSize = 4;


            // number of process() threads

            int threadCount = Environment.ProcessorCount;

            int cnt = fromInclusive - chunkSize;


            // processing function

            // takes next chunk and processes it using action

            ThreadDelegate process = delegate
                                         {
                                             while (true)
                                             {
                                                 int cntMem = 0;

                                                 lock (typeof (Parallel))
                                                 {
                                                     // take next chunk

                                                     cnt += chunkSize;

                                                     cntMem = cnt;
                                                 }

                                                 // process chunk

                                                 // here items can come out of order if chunkSize > 1

                                                 for (int i = cntMem; i < cntMem + chunkSize; ++i)
                                                 {
                                                     if (i >= toExclusive) return;

                                                     action(i);
                                                 }
                                             }
                                         };


            // launch process() threads

            IAsyncResult[] asyncResults = new IAsyncResult[threadCount];

            for (int i = 0; i < threadCount; ++i)
            {
                asyncResults[i] = process.BeginInvoke(null, null);
            }

            // wait for all threads to complete

            for (int i = 0; i < threadCount; ++i)
            {
                process.EndInvoke(asyncResults[i]);
            }
        }
    }
}