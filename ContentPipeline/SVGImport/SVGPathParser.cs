/*
 * Original source nanosvg:
 * Copyright (c) 2009 Mikko Mononen memon@inside.org https://code.google.com/p/nanosvg/
 * 
 * This software is provided 'as-is', without any express or implied 
 * warranty.  In no event will the authors be held liable for any damages 
 * arising from the use of this software. 
 * Permission is granted to anyone to use this software for any purpose, 
 * including commercial applications, and to alter it and redistribute it 
 * freely, subject to the following restrictions: 
 * 1. The origin of this software must not be misrepresented; you must not 
 * claim that you wrote the original software. If you use this software 
 * in a product, an acknowledgment in the product documentation would be 
 * appreciated but is not required. 
 * 2. Altered source versions must be plainly marked as such, and must not be 
 * misrepresented as being the original software. 
 * 3. This notice may not be removed or altered from any source distribution. 
 * 
 * The SVGPathParser is based on nanosvg's path parser
 * Copyright (c) 2013 Helge Backhaus
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using VelcroPhysics.ContentPipelines.SVGImport.Objects;
using VelcroPhysics.Shared;

namespace VelcroPhysics.ContentPipelines.SVGImport
{
    public class SVGPathParser
    {
        private const string _isNumber = @"\A[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?";
        private const string _isCommaWhitespace = @"\A[\s,]*";

        private readonly int _iterations;

        public SVGPathParser(int bezierIterations)
        {
            _iterations = bezierIterations;
        }

        public List<VerticesExt> ParseSVGPath(string path, Matrix transformation)
        {
            List<VerticesExt> result = new List<VerticesExt>();
            Vertices currentPath = null;
            Vector2 currentPosition = Vector2.Zero;

            char command = '0';

            int argumentsPerCommand = 0;
            int argumentCount = 0;

            float[] arguments = new float[6];

            while (!string.IsNullOrEmpty(path))
            {
                string item;
                path = GetNextPathItem(path, out item);

                if (string.IsNullOrEmpty(item))
                    break;

                if (Regex.IsMatch(item, _isNumber))
                {
                    if (argumentCount < 6)
                        arguments[argumentCount++] = float.Parse(item, CultureInfo.InvariantCulture);

                    if (argumentCount < argumentsPerCommand)
                        continue;

                    switch (command)
                    {
                        case 'm':
                        case 'l':
                            {
                                currentPosition.X += arguments[0];
                                currentPosition.Y += arguments[1];
                                currentPath.Add(currentPosition);
                            }
                            break;
                        case 'M':
                        case 'L':
                            {
                                currentPosition.X = arguments[0];
                                currentPosition.Y = arguments[1];
                                currentPath.Add(currentPosition);
                            }
                            break;
                        case 'h':
                            {
                                currentPosition.X += arguments[0];
                                currentPath.Add(currentPosition);
                            }
                            break;
                        case 'H':
                            {
                                currentPosition.X = arguments[0];
                                currentPath.Add(currentPosition);
                            }
                            break;
                        case 'v':
                            {
                                currentPosition.Y += arguments[0];
                                currentPath.Add(currentPosition);
                            }
                            break;
                        case 'V':
                            {
                                currentPosition.Y = arguments[0];
                                currentPath.Add(currentPosition);
                            }
                            break;
                        case 'c':
                            {
                                Vector2 end = currentPosition + new Vector2(arguments[4], arguments[5]);
                                CubicBezierRecursive(currentPath, currentPosition, end, currentPosition + new Vector2(arguments[0], arguments[1]), currentPosition + new Vector2(arguments[2], arguments[3]));

                                //cubicBezier(currentPath, currentPosition, end, currentPosition + new Vector2(arguments[0], arguments[1]), currentPosition + new Vector2(arguments[2], arguments[3]), (int)Math.Pow(2.0, BezierIterations));
                                currentPosition = end;
                                currentPath.Add(currentPosition);
                            }
                            break;
                        case 'C':
                            {
                                Vector2 end = new Vector2(arguments[4], arguments[5]);
                                CubicBezierRecursive(currentPath, currentPosition, end, new Vector2(arguments[0], arguments[1]), new Vector2(arguments[2], arguments[3]));

                                //cubicBezier(currentPath, currentPosition, end, new Vector2(arguments[0], arguments[1]), new Vector2(arguments[2], arguments[3]), (int)Math.Pow(2.0, BezierIterations));
                                currentPosition = end;
                                currentPath.Add(currentPosition);
                            }
                            break;
                        case 's':
                        case 'S':
                        case 't':
                        case 'T':
                        case 'q':
                        case 'Q':
                        case 'a':
                        case 'A':
                            {
                                throw new Exception("Path command '" + command + "' is not supported.");
                            }
                    }
                    argumentCount = 0;
                }
                else
                {
                    command = item[0];
                    switch (char.ToLower(command))
                    {
                        case 'v':
                        case 'h':
                            argumentsPerCommand = 1;
                            break;
                        case 'm':
                        case 'l':
                            argumentsPerCommand = 2;
                            break;
                        case 'c':
                            argumentsPerCommand = 6;
                            break;
                        default:
                            argumentsPerCommand = 0;
                            break;
                    }
                    switch (command)
                    {
                        case 'M':
                        case 'm':
                            {
                                if (currentPath != null && currentPath.Count > 1)
                                    result.Add(new VerticesExt(currentPath, false));

                                currentPath = new Vertices();
                                argumentCount = 0;

                                if (command == 'M')
                                    currentPosition = Vector2.Zero;

                                break;
                            }
                        case 'Z':
                        case 'z':
                            {
                                if (currentPath != null && currentPath.Count > 1)
                                    result.Add(new VerticesExt(currentPath, true));

                                currentPath = null;
                                argumentCount = 0;
                                break;
                            }
                    }
                }
            }

            if (currentPath != null && currentPath.Count > 1)
                result.Add(new VerticesExt(currentPath, false));

            foreach (VerticesExt poly in result)
            {
                for (int i = 0; i < poly.Count; i++)
                {
                    poly[i] = Vector2.Transform(poly[i], transformation);
                }
            }
            return result;
        }

        private string GetNextPathItem(string input, out string item)
        {
            item = string.Empty;
            string output = Regex.Replace(input, _isCommaWhitespace, "");

            if (string.IsNullOrEmpty(output))
                return output;

            if (Regex.IsMatch(output, _isNumber))
            {
                int matchLength = Regex.Match(output, _isNumber).Length;
                item = output.Substring(0, matchLength);
                output = output.Remove(0, matchLength);
            }
            else
            {
                item = output[0].ToString();
                output = output.Remove(0, 1);
            }

            return output;
        }

        private void CubicBezierRecursive(Vertices path, Vector2 start, Vector2 end, Vector2 controlStart, Vector2 controlEnd, int level = 0)
        {
            if (level >= _iterations)
            {
                return;
            }

            Vector2 s_CS = (start + controlStart) / 2f;
            Vector2 e_CE = (end + controlEnd) / 2f;
            Vector2 cS_CE = (controlStart + controlEnd) / 2f;

            Vector2 s_CS_CS_CE = (s_CS + cS_CE) / 2f;
            Vector2 e_CE_CS_CE = (e_CE + cS_CE) / 2f;

            Vector2 curvePoint = (s_CS_CS_CE + e_CE_CS_CE) / 2f;

            CubicBezierRecursive(path, start, curvePoint, s_CS, s_CS_CS_CE, level + 1);
            path.Add(curvePoint);
            CubicBezierRecursive(path, curvePoint, end, e_CE_CS_CE, e_CE, level + 1);
        }

        private void CubicBezier(Vertices path, Vector2 start, Vector2 end, Vector2 controlStart, Vector2 controlEnd, int steps)
        {
            Vector2 s_CS;
            Vector2 e_CE;
            Vector2 cS_CE;
            Vector2 s_CS_CS_CE;
            Vector2 e_CE_CS_CE;
            Vector2 curvePoint;

            for (int i = 0; i < steps; i++)
            {
                float t = i / (float)steps;
                s_CS = Vector2.Lerp(start, controlStart, t);
                cS_CE = Vector2.Lerp(controlStart, controlEnd, t);
                e_CE = Vector2.Lerp(controlEnd, end, t);
                s_CS_CS_CE = Vector2.Lerp(s_CS, cS_CE, t);
                e_CE_CS_CE = Vector2.Lerp(cS_CE, e_CE, t);
                curvePoint = Vector2.Lerp(s_CS_CS_CE, e_CE_CS_CE, t);
                path.Add(curvePoint);
            }
        }
    }
}