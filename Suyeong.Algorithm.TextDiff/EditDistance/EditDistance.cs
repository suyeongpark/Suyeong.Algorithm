using System;

namespace Suyeong.Algorithm.TextDiff
{
    public static class EditDistance
    {
        public static int GetEditDistance(string main, string sub)
        {
            int distance = -1;

            try
            {
                int mainLength = main.Length + 1, subLength = sub.Length + 1;

                int[,] dist = new int[mainLength, subLength];

                for (int i = 0; i < mainLength; i++)
                {
                    dist[i, 0] = i;
                }

                for (int i = 0; i < subLength; i++)
                {
                    dist[0, i] = i;
                }

                for (int i = 1; i < mainLength; i++)
                {
                    for (int j = 1; j < subLength; j++)
                    {
                        if (char.Equals(main[i - 1], sub[j - 1]))
                        {
                            dist[i, j] = dist[i - 1, j - 1];
                        }
                        else
                        {
                            dist[i, j] = 1 + Min(dist[i - 1, j], dist[i, j - 1], dist[i - 1, j - 1]);
                        }
                    }
                }

                distance = dist[main.Length, sub.Length];
            }
            catch (Exception)
            {
                throw;
            }

            return distance;
        }

        public static double FigureEditDistanceRatio(string main, string sub)
        {
            double ratio = 0d;

            try
            {
                int mainLength = main.Length + 1, subLength = sub.Length + 1;

                int[,] dist = new int[mainLength, subLength];

                for (int i = 0; i < mainLength; i++)
                {
                    dist[i, 0] = i;
                }

                for (int i = 0; i < subLength; i++)
                {
                    dist[0, i] = i;
                }

                for (int i = 1; i < mainLength; i++)
                {
                    for (int j = 1; j < subLength; j++)
                    {
                        if (char.Equals(main[i - 1], sub[j - 1]))
                        {
                            dist[i, j] = dist[i - 1, j - 1];
                        }
                        else
                        {
                            dist[i, j] = 1 + Min(dist[i - 1, j], dist[i, j - 1], dist[i - 1, j - 1]);
                        }
                    }
                }

                double distance = (double)dist[main.Length, sub.Length];
                double length = mainLength > subLength ? (double)mainLength : (double)subLength;

                ratio = 1d - (distance / length);
            }
            catch (Exception)
            {
                throw;
            }

            return ratio;
        }

        static int Min(int a, int b, int c)
        {
            return Min(a, Min(b, c));
        }

        static int Min(int a, int b)
        {
            return a < b ? a : b;
        }
    }
}
