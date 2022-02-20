using HaritaOrnek;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace names
{
    
    public class ShortestPath
    {
        
        public static int N;           //--> _mesafeler.Length

        public double[] final_path;
        
        public bool[] visited;

        //son yolu gösterir
        public double final_res = int.MaxValue;

        public void Populate<T>(T[] x, T value)
        {
            for (int i = 0; i < x.Length; i++)
            {
                x[i] = value;
            }
        }



        public void copyToFinal(int[] curr_path)
        {
            for (int i = 0; i < N; i++)
                final_path[i] = curr_path[i];
            final_path[N] = curr_path[0];
        }

        public double firstMin(double[,] adj, int i)
        {
            double min = int.MaxValue;
            for (int k = 0; k < N; k++)
                if (adj[i, k] < min && i != k)
                    min = adj[i, k];
            return min;
        }


        public double secondMin(double[,] adj, int i)
        {
            double first = int.MaxValue, second = int.MaxValue;
            for (int j = 0; j < N; j++)
            {
                if (i == j)
                    continue;

                if (adj[i, j] <= first)
                {
                    second = first;
                    first = adj[i, j];
                }
                else if (adj[i, j] <= second &&
                        adj[i, j] != first)
                    second = adj[i, j];
            }
            return second;
        }

        
        public void TSPRec(double[,] adj, double curr_bound, double curr_weight,
                    int level, int[] curr_path)
        {

           
            if (level == N)
            {
                
                if (adj[curr_path[level - 1], curr_path[0]] != 0)
                {
                    
                    double curr_res = curr_weight +
                                adj[curr_path[level - 1], curr_path[0]];

                    
                    if (curr_res < final_res)
                    {
                        copyToFinal(curr_path);
                        final_res = curr_res;
                    }
                }
                return;
            }

           
            for (int i = 0; i < N; i++)
            {
                
                if (adj[curr_path[level - 1], i] != 0 &&
                        visited[i] == false)
                {
                    double temp = curr_bound;
                    curr_weight += adj[curr_path[level - 1], i];

                   
                    if (level == 1)
                        curr_bound -= (firstMin(adj, curr_path[level - 1]) +
                                        firstMin(adj, i)) / 2;
                    else
                        curr_bound -= (secondMin(adj, curr_path[level - 1]) +
                                        firstMin(adj, i)) / 2;

                    
                    if (curr_bound + curr_weight < final_res)
                    {
                        curr_path[level] = i;
                        visited[i] = true;

                        
                        TSPRec(adj, curr_bound, curr_weight, level + 1,
                            curr_path);
                    }

                    
                    curr_weight -= adj[curr_path[level - 1], i];
                    curr_bound = temp;


                    
                    Populate(visited, false);
                    

                    for (int j = 0; j <= level - 1; j++)
                        visited[curr_path[j]] = true;
                }
            }
        }

        
        public void TSP(double[,] adj)
        {
            int[] curr_path = new int[N + 1];

            
            double curr_bound = 0;

            Populate(curr_path, -1);
            
            Populate(visited, false);


            
            for (int i = 0; i < N; i++)
                curr_bound += firstMin(adj, i) + secondMin(adj, i);


            
            curr_bound = curr_bound == 1 ? curr_bound / 2 + 1 :
                                        curr_bound / 2;

            
            visited[0] = true;
            curr_path[0] = 0;

            
            TSPRec(adj, curr_bound, 0, 1, curr_path);
        }

    }
}