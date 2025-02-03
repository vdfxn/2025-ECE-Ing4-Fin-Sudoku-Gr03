using Sudoku.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.GraphColoringSolver
{
    public partial class OptimizedGraphColoringSolverV3
    {
         private const int TabuListMaxSize = 10; // Taille de la liste tabou

 private SudokuGrid SolveWithTabuSearch(SudokuGrid initialGrid)
        {
            var currentSolution = initialGrid.CloneSudoku();
            var tabuList = new Queue<((int, int), (int, int))>(); // Queue pour la liste tabou

            var bestSolution = initialGrid.CloneSudoku();
            int bestFitness = CalculateConflicts(initialGrid);

            int iterations = 0;
            while (iterations < 10000) // TODO: Choisir un bon critère d'arrêt
            {
               iterations++;

              // Génération des voisins
              var neighbors = GenerateNeighbors(currentSolution);

              SudokuGrid bestNeighbor = null;
               int bestNeighborFitness = int.MaxValue;
              ((int, int), (int, int)) bestMove = ((-1,-1),(-1,-1));

               foreach(var neighbor in neighbors){
                   var currentFitness = CalculateConflicts(neighbor.Item1); // Corrected
                  // Vérifier si le mouvement est tabou
                   if(!IsMoveTabu(neighbor.Item2, tabuList) || currentFitness < bestFitness )
                   {
                        if(currentFitness < bestNeighborFitness){
                            bestNeighbor = neighbor.Item1;
                            bestNeighborFitness = currentFitness;
                            bestMove = neighbor.Item2;
                        }
                   }
               }

               // On continue si aucun voisin n'a été trouvé
               if(bestNeighbor == null){
                  if(CalculateConflicts(currentSolution) == 0){
                   return currentSolution;
                  } else {
                     // pas de voisin trouvé
                    continue;
                 }

               }


              //  Console.WriteLine($"[TabuSearch Iteration : {iterations}] bestFitness : {bestNeighborFitness}");


                currentSolution = bestNeighbor;


                  // Mettre a jour la liste tabou
                 tabuList.Enqueue(bestMove);
                if (tabuList.Count > TabuListMaxSize)
                    tabuList.Dequeue();



               // Met a jour la meilleur solution si on l'améliore
                  int currentFitnessSolution = CalculateConflicts(currentSolution);
                  if(currentFitnessSolution < bestFitness){
                     bestFitness = currentFitnessSolution;
                      bestSolution = currentSolution.CloneSudoku();
                     //  Console.WriteLine($"[TabuSearch Iteration : {iterations}] bestFitness Updated : {bestFitness}");
                  }

                  // Si plus de conflit, on return
                    if(bestFitness == 0){
                        return bestSolution;
                   }


            }
               // si pas de solution trouvé, on retourne la meilleur trouvé
            return bestSolution;
         }

        private List<(SudokuGrid, ((int, int), (int, int)))> GenerateNeighbors(SudokuGrid grid)
        {
             var neighbors = new List<(SudokuGrid, ((int, int), (int, int)))>();

            var cells = new List<(int, int)>();
             for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                   if(grid.Cells[row, col] != 0) {
                        cells.Add((row,col));
                   }
                }
            }

             if (cells.Count < 2) {
                    return neighbors;
                }

             for(int i = 0; i < 5; i++) {
               // Choix aléatoire de 2 cases
              var random = new Random();
               int index1 = random.Next(cells.Count);
                int index2 = random.Next(cells.Count);
                while(index1 == index2){
                   index2 = random.Next(cells.Count);
                }
               var cell1 = cells[index1];
               var cell2 = cells[index2];
                var neighborGrid = grid.CloneSudoku();
              // échange les 2 cases
                int temp = neighborGrid.Cells[cell1.Item1, cell1.Item2];
                neighborGrid.Cells[cell1.Item1, cell1.Item2] = neighborGrid.Cells[cell2.Item1, cell2.Item2];
                 neighborGrid.Cells[cell2.Item1, cell2.Item2] = temp;
               neighbors.Add((neighborGrid, (cell1, cell2)));

            }
            return neighbors;
        }


         private bool IsMoveTabu(((int, int), (int, int)) move, Queue<((int, int), (int, int))> tabuList)
         {
            foreach (var tabuMove in tabuList)
            {
                  if ((tabuMove.Item1 == move.Item1 && tabuMove.Item2 == move.Item2) || (tabuMove.Item1 == move.Item2 && tabuMove.Item2 == move.Item1)) {
                        return true;
                    }
            }
            return false;
        }


           private int CalculateConflicts(SudokuGrid grid)
        {
            int conflicts = 0;

            // Lignes
             for (int row = 0; row < 9; row++)
            {
               var values = new HashSet<int>();
                 for (int col = 0; col < 9; col++)
                {
                      if (grid.Cells[row, col] != 0) {
                          if (values.Contains(grid.Cells[row,col])){
                                conflicts++;
                            } else {
                                values.Add(grid.Cells[row,col]);
                            }
                       }


                 }
            }

            // Colonnes
             for (int col = 0; col < 9; col++)
            {
               var values = new HashSet<int>();
                 for (int row = 0; row < 9; row++)
                {
                     if (grid.Cells[row, col] != 0) {
                          if (values.Contains(grid.Cells[row,col])){
                                conflicts++;
                            } else {
                                values.Add(grid.Cells[row,col]);
                            }
                       }
                }
            }

            // Blocs
             for (int blockRow = 0; blockRow < 3; blockRow++)
            {
                for (int blockCol = 0; blockCol < 3; blockCol++)
                {
                    var values = new HashSet<int>();
                    for (int row = blockRow * 3; row < blockRow * 3 + 3; row++)
                    {
                        for (int col = blockCol * 3; col < blockCol * 3 + 3; col++)
                        {
                              if (grid.Cells[row, col] != 0) {
                                if (values.Contains(grid.Cells[row,col])){
                                        conflicts++;
                                    } else {
                                        values.Add(grid.Cells[row,col]);
                                    }
                            }
                        }
                    }
                }
            }


            return conflicts;
        }
    }
}