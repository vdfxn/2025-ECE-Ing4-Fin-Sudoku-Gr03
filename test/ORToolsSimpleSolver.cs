using Sudoku.Shared;
using Google.OrTools.Sat;
using System;

namespace Sudoku.ORToolsSimpleSolver;

public class ORToolsSimpleSolver : ISudokuSolver
{
    public SudokuGrid Solve(SudokuGrid s)
    {
        CpModel model = new CpModel();  // Crée un modèle de programmation par contraintes
        IntVar[,] grid = new IntVar[9, 9];  // Crée une grille 9x9 de variables entières

        // 1. Créer les variables de décision pour la grille de Sudoku
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                grid[i, j] = model.NewIntVar(1, 9, $"grid{i}{j}");
            }
        }

        // 2. Ajouter les contraintes de Sudoku
        //    - Contraintes de lignes: chaque ligne doit avoir des chiffres uniques de 1 à 9
        for (int i = 0; i < 9; i++)
        {
            model.AddAllDifferent(Enumerable.Range(0, 9).Select(j => grid[i, j]));
        }

        //    - Contraintes de colonnes: chaque colonne doit avoir des chiffres uniques de 1 à 9
        for (int j = 0; j < 9; j++)
        {
            model.AddAllDifferent(Enumerable.Range(0, 9).Select(i => grid[i, j]));
        }

        //    - Contraintes de blocs 3x3: chaque bloc 3x3 doit avoir des chiffres uniques de 1 à 9
        for (int blockRow = 0; blockRow < 3; blockRow++)
        {
            for (int blockCol = 0; blockCol < 3; blockCol++)
            {
                var block = new List<IntVar>();
                for (int i = blockRow * 3; i < (blockRow + 1) * 3; i++)
                {
                    for (int j = blockCol * 3; j < (blockCol + 1) * 3; j++)
                    {
                        block.Add(grid[i, j]);
                    }
                }
                model.AddAllDifferent(block);
            }
        }
        
        // 3. Ajouter les indices de départ (les indices initiaux qui ne sont pas 0)
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (s.Cells[i, j] != 0)
                {
                    model.Add(grid[i, j] == s.Cells[i, j]); // force la variable a la valeur de depart
                }
            }
        }


        // 4. Résoudre le modèle
        CpSolver solver = new CpSolver();
        CpSolverStatus status = solver.Solve(model);

        // 5. Traiter le résultat
        if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
        {
           
            //On doit maintenant remplir une nouvelle SudokuGrid avec les valeurs trouvées par le solveur
            SudokuGrid solvedGrid = new SudokuGrid();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                     solvedGrid.Cells[i, j] = (int)solver.Value(grid[i, j]);
                }
            }

            return solvedGrid;
        }
        else
        {
            // Le solveur n'a pas trouvé de solution
            Console.WriteLine("No solution found.");
            return s;
        }
    }
}